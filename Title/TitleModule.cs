using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Title.Views;
using Core;

namespace Title
{
    public class TitleModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            RegionManager regionManager = containerProvider.Resolve<RegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.TitleRegion,"TitleView");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<TitleView>();
        }
    }
}