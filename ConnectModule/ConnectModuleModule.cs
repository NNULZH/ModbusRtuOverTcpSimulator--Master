using ConnectModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Core;
using Prism.Regions;

namespace ConnectModule
{
    public class ConnectModuleModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            IRegionManager regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.ContentRegion,"Connect");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Connect>();
        }
    }
}