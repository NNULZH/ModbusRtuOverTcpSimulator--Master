using Prism.Events;

namespace Core.Event
{
    // 1. 定义数据载体：把“请求”和“响应”打包在一起
    // 这样 UI 收到数据时，就知道这个响应是属于哪个站号、哪个地址的
    public class ModbusTransactionResult
    {
        public ModbusQueryCommand Request { get; set; } // 你的请求对象（包含Start Address）
        public byte[] ResponseBytes { get; set; }       // 原始响应字节

        public ModbusTransactionResult(ModbusQueryCommand request, byte[] response)
        {
            Request = request;
            ResponseBytes = response;
        }
    }

    // 2. 定义事件：用于通知 UI 更新
    public class ModbusDataReceivedEvent : PubSubEvent<ModbusTransactionResult> { }

}