using Prism.Mvvm;
using System.Text;

namespace Core
{
    public class ModbusResponseCommand : BindableBase
    {
        // 核心属性
        private int _slaveId;
        private int _funcCode;
        private int _startAddress;
        private string _nums;
        private byte[] _fullCommand;

        public int SlaveId { get => _slaveId; set => SetProperty(ref _slaveId, value); }
        public int FuncCode { get => _funcCode; set => SetProperty(ref _funcCode, value); }
        public int StartAddress { get => _startAddress; set => SetProperty(ref _startAddress, value); } // 新增
        public string Nums { get => _nums; set => SetProperty(ref _nums, value); }
        public byte[] FullCommand { get => _fullCommand; set => SetProperty(ref _fullCommand, value); }

        // 唯一键：用于在表格中查找是否存在旧数据
        public string UniqueKey => $"{SlaveId}-{FuncCode}-{StartAddress}";

        // 构造函数：接收“请求上下文” + “响应数据”
        public ModbusResponseCommand(ModbusQueryCommand request, byte[] response)
        {
            // 1. 从请求中获取上下文
            SlaveId = request.SlaveId;
            FuncCode = request.FuncCode;
            StartAddress = request.StartAddress; // 关键！

            // 2. 解析响应
            UpdateData(response);
        }

        // 更新数据的方法
        public void UpdateData(byte[] response)
        {
            FullCommand = response;

            // 简单解析逻辑 (根据你的实际解析类进行调整)
            if (response == null || response.Length < 3)
            {
                Nums = "Error";
                return;
            }

            // 这里简化演示，直接转Hex字符串，你应保留你之前的 ParseBitData/ParseRegisterData 逻辑
            Nums = System.BitConverter.ToString(response).Replace("-", " ");
        }
    }
}