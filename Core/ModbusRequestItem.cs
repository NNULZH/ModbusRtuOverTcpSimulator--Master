namespace Core
{
    // A. 请求对象：代表队列中的一个轮询任务
    public class ModbusRequestItem
    {
        public int SlaveId { get; set; }
        public int FuncCode { get; set; }
        public int StartAddress { get; set; } // 关键：这是响应里没有的，必须由请求带过去
        public ushort Length { get; set; }
        
        //标识符，用于表格去重
        public string UniqueKey => $"{SlaveId}-{FuncCode}-{StartAddress}";
    }

}