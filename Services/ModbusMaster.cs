using Core.Event;
using Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Timers; // 使用 System.Timers.Timer
using System.Windows;

namespace Services
{
    public class ModbusMaster : IModbusMaster, IDisposable
    {
        public readonly TcpClient SerialClient = new TcpClient();
        public TcpSerial tcpSerial;
        private readonly object _lockObject = new object();

        // 核心缓冲区
        private List<byte> _dataBuffer = new List<byte>();
        public List<byte> DataBuffer { get => _dataBuffer; set => _dataBuffer = value; }

        // 帧检测定时器 (3.5 字符时间)
        private System.Timers.Timer _frameTimer;
        private double _frameInterval;

        // 串口参数（用于计算传输时间）
        public int BaudRate { get; } = 9600;
        public int DataBits { get; } = 8;
        public bool HasParity { get; } = false;
        public double StopBits { get; } = 1.0;

        public IEventAggregator EventAggregator { get; set; }

        public ModbusMaster(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            InitializeTimer();
            CalWaitTime(); // 初始化静默时间计算
        }

        private void InitializeTimer()
        {
            _frameTimer = new System.Timers.Timer();
            _frameTimer.AutoReset = false; // 只触发一次
            _frameTimer.Elapsed += OnFrameTimeOut;
        }

        public void Connect(string host, int port)
        {
            try
            {
                if (SerialClient.Connected) SerialClient.Close();

                SerialClient.Connect(host, port);
                tcpSerial = new TcpSerial(SerialClient.GetStream(), BaudRate, DataBits, HasParity, StopBits);
                tcpSerial.OnReceivedData += ReceiveByte;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Connect Exception: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 计算 3.5 字符时间
        /// </summary>
        private void CalWaitTime()
        {
            double bitsPerChar = 1.0 + DataBits + (HasParity ? 1.0 : 0.0) + StopBits;
            double timePerBit = 1.0 / BaudRate;
            double timePerChar = timePerBit * bitsPerChar;
            double waitTimeMs = timePerChar * 3.5 * 1000;

            if (BaudRate > 19200)
                _frameInterval = 2.0;
            else
                _frameInterval = waitTimeMs;

            if (_frameInterval < 10) _frameInterval = 10; // 给 Windows 定时器一点余量

            if (_frameTimer != null)
                _frameTimer.Interval = _frameInterval;
        }

        public void ReceiveByte(byte b)
        {
            lock (_lockObject)
            {
                _dataBuffer.Add(b);
            }

            // 喂狗机制：重置定时器，检测静默
            _frameTimer.Stop();
            _frameTimer.Start();
        }

        /// <summary>
        /// 3.5字符静默超时处理：解析响应帧 (Response Parsing)
        /// </summary>
        private void OnFrameTimeOut(object sender, ElapsedEventArgs e)
        {
            List<byte[]> validResponses = new List<byte[]>();
            List<byte> snapshot;

            // 1) 短锁：拷贝快照
            lock (_lockObject)
            {
                if (_dataBuffer.Count == 0) return;

                // 防止缓冲区爆炸
                if (_dataBuffer.Count > 4096)
                {
                    _dataBuffer.Clear();
                    return;
                }

                snapshot = new List<byte>(_dataBuffer);
                _dataBuffer.Clear();
            }

            // 2) 无锁处理：滑动窗口解析
            List<byte> tail = new List<byte>();

            try
            {
                while (snapshot.Count > 0)
                {
                    if (snapshot.Count < 4) // 最小 Modbus 帧长度 (异常帧是5，但我们先保留)
                    {
                        tail.AddRange(snapshot);
                        break;
                    }

                    // 获取预期的响应帧长度 (这是 Master 和 Slave 最大的区别)
                    int expectedLength = GetExpectedResponseLength(snapshot);

                    if (expectedLength == -1)
                    {
                        // 头部无效，滑动丢弃
                        snapshot.RemoveAt(0);
                        continue;
                    }

                    if (expectedLength == 0 || snapshot.Count < expectedLength)
                    {
                        // 分包，数据未收齐，保留为尾部
                        tail.AddRange(snapshot);
                        break;
                    }

                    // CRC 校验
                    if (CheckCrc(snapshot, 0, expectedLength))
                    {
                        // 提取完整帧
                        byte[] frameBytes = new byte[expectedLength];
                        snapshot.CopyTo(0, frameBytes, 0, expectedLength);

                        validResponses.Add(frameBytes);

                        // 移除已处理数据
                        snapshot.RemoveRange(0, expectedLength);
                    }
                    else
                    {
                        // CRC 失败，滑动窗口
                        snapshot.RemoveAt(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Parse Response Error: {ex.Message}");
                tail.AddRange(snapshot);
            }

            // 3) 回填尾部
            if (tail.Count > 0)
            {
                lock (_lockObject)
                {
                    _dataBuffer.InsertRange(0, tail);
                }
            }

            // 4) 发布响应 (这里直接发布 byte[]，让上层处理 Hex 转换或逻辑)
            foreach (var response in validResponses)
            {
                // 注意：这里建议发布 byte[]，因为处理响应时经常需要根据字节索引取值
                EventAggregator.GetEvent<OnReceivedResponse>().Publish(response);
            }
        }

        /// <summary>
        /// 核心：根据 Modbus Master 协议判断预期的 Response 帧长度
        /// </summary>
        private int GetExpectedResponseLength(List<byte> buffer)
        {
            if (buffer.Count < 2) return 0;

            byte funcCode = buffer[1];

            // 1. 检查是否为异常响应 (Function Code + 0x80)
            if ((funcCode & 0x80) != 0)
            {
                // 异常帧结构: ID(1) + (Func|0x80)(1) + ExCode(1) + CRC(2) = 5字节
                return 5;
            }

            switch (funcCode)
            {
                // 读指令响应 (变长)
                // 结构: ID(1) + Func(1) + ByteCount(1) + Data(N) + CRC(2)
                case 0x01: // Read Coils
                case 0x02: // Read Discrete Inputs
                case 0x03: // Read Holding Registers
                case 0x04: // Read Input Registers
                    if (buffer.Count < 3) return 0; // 还没读到 ByteCount
                    byte byteCount = buffer[2];
                    return 3 + byteCount + 2; // Header(2) + ByteCount(1) + Data + CRC(2)

                // 写指令响应 (定长 - 回显)
                // 结构: ID(1) + Func(1) + Addr(2) + Data/Qty(2) + CRC(2) = 8字节
                case 0x05: // Write Single Coil
                case 0x06: // Write Single Register
                case 0x0F: // Write Multiple Coils (响应不是变长的，只回显数量)
                case 0x10: // Write Multiple Registers (响应不是变长的，只回显数量)
                    return 8;

                default:
                    return -1; // 未知功能码
            }
        }

        private bool CheckCrc(List<byte> buffer, int start, int length)
        {
            if (length < 4) return false;
            int end = start + length;

            byte receivedCrcLo = buffer[end - 2];
            byte receivedCrcHi = buffer[end - 1];

            UInt16 calculatedCrc = 0xFFFF;
            for (int i = start; i < end - 2; i++)
            {
                calculatedCrc ^= buffer[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((calculatedCrc & 0x0001) != 0)
                    {
                        calculatedCrc >>= 1;
                        calculatedCrc ^= 0xA001;
                    }
                    else
                    {
                        calculatedCrc >>= 1;
                    }
                }
            }

            byte calcLo = (byte)(calculatedCrc & 0xFF);
            byte calcHi = (byte)((calculatedCrc >> 8) & 0xFF);

            return (receivedCrcLo == calcLo) && (receivedCrcHi == calcHi);
        }

        public async void WriteByteAsync(byte b)
        {
            if (tcpSerial != null)
                await tcpSerial.WriteBytesAsync(new byte[] { b });
        }

        public async Task WriteBytesAsync(byte[] bytes)
        {
            if (tcpSerial != null)
                await tcpSerial.WriteBytesAsync(bytes);
        }

        public void Dispose()
        {
            _frameTimer?.Stop();
            _frameTimer?.Dispose();
            tcpSerial?.Dispose();
            SerialClient?.Close();
        }
    }
}