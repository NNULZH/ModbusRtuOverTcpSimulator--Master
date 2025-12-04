using Core;
using Interfaces;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ConnectModule.ViewModels
{
    public class ConnectViewModel : BindableBase
    {
        public ICommand ConnectCommand { get; set; }

        private string host ="127.0.0.1";
        private string port ="8889";
        
        IModbusMaster master;

        public IRegionManager RegionManager { get; set; }
        public IContainerProvider containerProvider { get; set; }
        public string Port { get => port; set => SetProperty(ref port ,value); }
        public string Host { get => host; set => SetProperty(ref host, value); }

        public ConnectViewModel(IModbusMaster modbusMaster,IRegionManager regionManager,IContainerProvider containerProvider)
        {   
            this.containerProvider = containerProvider;
            RegionManager = regionManager;
            master = modbusMaster;
            ConnectCommand = new DelegateCommand(ConnectToSlaves);
        }

        public void ConnectToSlaves() //需要一个未登陆时不能切换窗口的功能
        {
            try 
            {   
                master.Connect(Host, Convert.ToInt32(Port));
                RegionManager.RequestNavigate(RegionNames.ContentRegion,"MainControlView");
                var LoginState = containerProvider.Resolve<AppState>();
                LoginState.IsLoggedIn = true;
            }
            catch(Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            
        }
    }
}
