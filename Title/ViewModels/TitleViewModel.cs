using Core;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Title.ViewModels
{
    public class TitleViewModel : BindableBase
    {
        IRegionManager regionManager;
        public ICommand NavigateCommand { get; private set; }

        public AppState appState;
        public TitleViewModel(IRegionManager regionManager,IContainerProvider containerProvider)
        {   
            this.appState = containerProvider.Resolve<AppState>();
            this.regionManager = regionManager;
            NavigateCommand = new DelegateCommand<string>(Navagate);
        }

        private void Navagate(string DesView)
        {   
            if(appState.IsLoggedIn) //登陆了才能切换!
                regionManager.RequestNavigate(RegionNames.ContentRegion,DesView);
        }
    }
}
