using System.Windows;
using Modbus_Master_Rtu_over_Tcp.Views;
using Prism.Ioc;
using Prism.Modularity;
using ConnectModule;
using Interfaces;
using Services;
using MainControl;
using Title;
using Core;
using DataMonitorModule.Views;
using DataMonitorModule;
namespace Modbus_Master_Rtu_over_Tcp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterSingleton<ITcpConnnection,TcpConnection>();  //废弃
            containerRegistry.RegisterSingleton<IModbusMaster, ModbusMaster>();
            containerRegistry.Register<ModbusMaster>();
            containerRegistry.RegisterSingleton<AppState>();//专门用于控制登陆状态
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ConnectModuleModule>();
            moduleCatalog.AddModule<MainControlModule>();
            moduleCatalog.AddModule<TitleModule>();
            moduleCatalog.AddModule<DataMonitorModuleModule>();
        }
    }
}
