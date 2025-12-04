using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using Core.Event;

namespace DataMonitorModule.ViewModels
{
    public class DataMonitorViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        // UI 绑定的数据源放在这里
        public ObservableCollection<ModbusResponseCommand> ResponseList { get; set; } = new ObservableCollection<ModbusResponseCommand>();

        public DataMonitorViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            // 订阅“轮询结果”事件
            // ThreadOption.UIThread 确保在主线程更新 UI，防止跨线程异常
            _eventAggregator.GetEvent<ModbusDataReceivedEvent>()
                            .Subscribe(OnDataReceived, ThreadOption.UIThread);
        }

        // 处理接收到的数据
        private void OnDataReceived(ModbusTransactionResult result)
        {
            // 构造唯一键: 站号-功能码-起始地址
            // 这些信息都包含在发布的 result.Request 中
            string key = $"{result.Request.SlaveId}-{result.Request.FuncCode}-{result.Request.StartAddress}";

            // 查找是否已存在该行
            var existingItem = ResponseList.FirstOrDefault(x => x.UniqueKey == key);

            if (existingItem != null)
            {
                // [更新] 已存在，直接更新属性，UI 会自动刷新数值
                existingItem.UpdateData(result.ResponseBytes);
            }
            else
            {
                // [插入] 第一次收到该地址的数据，新增一行
                var newItem = new ModbusResponseCommand(result.Request, result.ResponseBytes);
                ResponseList.Add(newItem);
            }
        }
    }
}
