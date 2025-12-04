using Core;
using Core.Event;
using Interfaces;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static MaterialDesignThemes.Wpf.Theme;

namespace MainControl.ViewModels
{
    public class MainControlViewModel : BindableBase
    {
        public IModbusMaster Master { get; set; }
        private readonly IEventAggregator _eventAggregator;

        // 互斥信号量：用于解决“轮询”和“手动发送”争抢串口的问题
        private readonly SemaphoreSlim _communicationLock = new SemaphoreSlim(1, 1);

        private IDialogService _dialogService;

        public MainControlViewModel(IModbusMaster master, IEventAggregator eventAggregator,IDialogService dialogService)
        {   
            _dialogService = dialogService;
            Master = master;
            _eventAggregator = eventAggregator;

            AddTaskCommand = new DelegateCommand(OnAddTask);
            StartPollingCommand = new DelegateCommand(OnStartPolling);
            StopPollingCommand = new DelegateCommand(OnStopPolling);
            DeleteCommand = new DelegateCommand(OnDeleteTask);
            SendCommand = new DelegateCommand(OnSend);

            // 启动后台轮询任务（建议保存 Task 引用以便管理，这里简化为直接运行）
            Task.Run(() => PollingLoop());
        }

        #region 属性与命令

        ObservableCollection<ModbusQueryCommand> commands = new ObservableCollection<ModbusQueryCommand>();
        public ObservableCollection<ModbusQueryCommand> Commands { get => commands; set => SetProperty(ref commands, value); }

        private ModbusQueryCommand selectedCommand;
        public ModbusQueryCommand SelectedCommand { get => selectedCommand; set => SetProperty(ref selectedCommand, value); }

        // 输入参数属性
        int inputSlaveId = 1;
        int inputFunctionCode = 3;
        int inputStartAddress = 0;
        int inputCount = 1;
        public int InputSlaveId { get => inputSlaveId; set => SetProperty(ref inputSlaveId, value); }
        public int InputFunctionCode { get => inputFunctionCode; set => SetProperty(ref inputFunctionCode, value); }
        public int InputStartAddress { get => inputStartAddress; set => SetProperty(ref inputStartAddress, value); }
        public int InputCount { get => inputCount; set => SetProperty(ref inputCount, value); }

        public DelegateCommand AddTaskCommand { get; private set; }
        public DelegateCommand StartPollingCommand { get; private set; }
        public DelegateCommand StopPollingCommand { get; private set; }
        public DelegateCommand DeleteCommand { get; private set; }
        public DelegateCommand SendCommand { get; private set; }

        bool Ispolling = false;

        #endregion

        #region 核心逻辑

        private void OnAddTask()
        {
            try
            {
                if (Ispolling)
                {
                    MessageBox.Show("请先停止轮询再添加!");
                    return;
                }
            }
            catch (Exception ex) 
            {
                Debug.WriteLine(ex.Message.ToString());
            }
            Commands.Add(new ModbusQueryCommand(InputSlaveId, InputFunctionCode, InputStartAddress, InputCount));
        }

        private void OnStartPolling()
        {
            Ispolling = true;
        }

        private void OnStopPolling()
        {
            Ispolling = false;
        }

        private void OnDeleteTask()
        {
            if (SelectedCommand != null)
                Commands.Remove(SelectedCommand);
        }

        /// <summary>
        /// 轮询主循环
        /// </summary>
        public async Task PollingLoop()
        {
            while (true)
            {
                // 如果开启了轮询且有指令
                if (Ispolling && Commands.Count > 0)
                {
                    // 快照
                    var currentCommands = Commands.ToList();

                    foreach (var command in currentCommands)
                    {
                        if (!Ispolling) break; // 及时响应停止

                        // 发送并等待响应
                        byte[] response = await SendAndAwaitResponse(command.FullCommand);

                        if (response != null)
                        {
                            Debug.WriteLine($"[Polling] Success: {BitConverter.ToString(response)}");
                            // 在这里解析 response 并更新 UI 或 Command 状态
                            var transaction = new ModbusTransactionResult(command, response);
                            _eventAggregator.GetEvent<ModbusDataReceivedEvent>().Publish(transaction);
                        }
                        else
                        {
                            Debug.WriteLine($"[Polling] Timeout or Error: {command}");
                        }

                        // 指令间隔，防止总线拥堵
                        await Task.Delay(50);
                    }
                }
                else
                {
                    // 空闲时休眠，避免死循环占用 CPU
                    await Task.Delay(200);
                }
            }
        }

        /// <summary>
        /// 手动发送单条指令（写者优先：会通过锁暂时阻塞轮询）
        /// </summary>
        private async void OnSend()
        {
            if (SelectedCommand == null) return;

            // 临时暂停轮询（可选），或者直接依靠 Semaphore 锁来排队
            // 这里依靠 Semaphore，手动发送会等待当前正在进行的轮询指令结束，然后插队执行
            Debug.WriteLine("[Manual] Sending command...");

            byte[] response = await SendAndAwaitResponse(SelectedCommand.FullCommand);

            if (response != null)
            {
                Debug.WriteLine($"[Manual] Response: {BitConverter.ToString(response)}");
                MessageBox.Show($"收到响应: {BitConverter.ToString(response)}");
            }
            else
            {
                MessageBox.Show("发送超时，未收到响应。");
            }
        }

        /// <summary>
        /// 核心方法：发送指令并异步等待 EventAggregator 的响应  学习什么东西可以做到?
        /// </summary>
        /// <param name="commandBytes">完整的 Modbus 指令</param>
        /// <param name="timeoutMs">超时时间（毫秒）</param>
        /// <returns>响应字节数组，如果超时则返回 null</returns>
        private async Task<byte[]> SendAndAwaitResponse(byte[] commandBytes, int timeoutMs = 1000)
        {
            // 1. 获取通信锁（确保同一时间只有一个命令在发送和等待）
            await _communicationLock.WaitAsync();

            try
            {
                // 2. 准备 TaskCompletionSource，作为一个"未来会完成的任务"
                var tcs = new TaskCompletionSource<byte[]>();

                // 3. 定义接收响应的 Action
                // 这里的逻辑比较简单：只要收到 OnReceivedResponse 事件，就认为是我们等的响应
                // 在更复杂的场景下，你可能需要检查 response 中的 TransactionID 是否匹配
                Action<byte[]> responseHandler = (data) =>
                {
                    tcs.TrySetResult(data);
                };

                // 4. 订阅事件
                // 使用 ThreadOption.PublisherThread 确保在发布者线程立即触发，减少上下文切换延迟
                SubscriptionToken token = _eventAggregator.GetEvent<OnReceivedResponse>()
                                                          .Subscribe(responseHandler, ThreadOption.PublisherThread);

                try
                {
                    // 5. 发送指令
                    await Master.WriteBytesAsync(commandBytes);

                    // 6. 等待：要么 tcs 被设置结果（收到响应），要么 Task.Delay 完成（超时）
                    var timeoutTask = Task.Delay(timeoutMs);
                    var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                    if (completedTask == tcs.Task)
                    {
                        // 成功收到响应
                        return tcs.Task.Result;
                    }
                    else
                    {
                        // 超时
                        Debug.WriteLine("Waiting for response timed out.");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Communication Error: {ex.Message}");
                    return null;
                }
                finally
                {
                    // 7. 无论成功还是超时，必须取消订阅，防止内存泄漏和逻辑错误
                    _eventAggregator.GetEvent<OnReceivedResponse>().Unsubscribe(token);
                }
            }
            finally
            {
                // 8. 释放锁，让下一个指令（轮询或手动）可以执行
                _communicationLock.Release();
            }
        }

        #endregion
    }
}