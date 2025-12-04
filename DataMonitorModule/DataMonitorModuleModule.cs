using DataMonitorModule.ViewModels;
using DataMonitorModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DataMonitorModule
{
    public class DataMonitorModuleModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DataMonitorView>();
        }
    }
}