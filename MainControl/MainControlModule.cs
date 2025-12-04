using MainControl.ViewModels;
using MainControl.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace MainControl
{
    public class MainControlModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainControlView>();
        }
    }
}