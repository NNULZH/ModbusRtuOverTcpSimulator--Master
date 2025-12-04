using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ModbusQueryCommand:BindableBase
    {
        int slaveId;       // 站地址
        int funcCode;      // 功能码
        int startAddress;  // 起始地址
        int nums;          // 要读的数量

        // 完整 Modbus RTU 查询命令
        public byte[] FullCommand { get => fullCommand; set => SetProperty(ref fullCommand,value); }

        byte[] fullCommand;

        public int SlaveId
        {
            get => slaveId;
            set => SetProperty(ref slaveId, value,BuildCommand); //这第三个参数竟然是委托方法
        }

        public int FuncCode
        {
            get => funcCode;
            set => SetProperty(ref funcCode, value, BuildCommand);
        }

        public int StartAddress
        {
            get => startAddress;
            set => SetProperty(ref startAddress, value, BuildCommand);
        }

        public int Nums
        {
            get => nums;
            set => SetProperty(ref nums, value, BuildCommand);
        }

        public ModbusQueryCommand(int slaveId, int funcCode, int startAddress, int nums)
        {
            this.SlaveId = slaveId;
            this.FuncCode = funcCode;
            this.StartAddress = startAddress;
            this.Nums = nums;

            BuildCommand();
        }

        private void BuildCommand()
        {
            List<byte> cmd = new List<byte>();

            cmd.Add((byte)slaveId);
            cmd.Add((byte)funcCode);

            cmd.Add((byte)(startAddress >> 8));   // 起始地址高位
            cmd.Add((byte)(startAddress & 0xFF)); // 起始地址低位

            cmd.Add((byte)(nums >> 8));   // 数量高位
            cmd.Add((byte)(nums & 0xFF)); // 数量低位

            // 计算 CRC16
            ushort crc = CRC16(cmd.ToArray());

            cmd.Add((byte)(crc & 0xFF));       // CRC低位
            cmd.Add((byte)((crc >> 8) & 0xFF)); // CRC高位

            FullCommand = cmd.ToArray();
        }

        // CRC16-Modbus (同你项目里用的一致)
        private ushort CRC16(byte[] data)
        {
            ushort crc = 0xFFFF;

            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    bool lsb = (crc & 0x0001) != 0;
                    crc >>= 1;
                    if (lsb) crc ^= 0xA001;
                }
            }
            return crc;
        }
    }




}
