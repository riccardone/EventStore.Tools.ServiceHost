using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using EventStore.Tools.PluginModel;
using log4net;
using Topshelf;

namespace EventStore.Tools.ServiceHost
{
    public class ServiceStartup 
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceStartup));
        private HostControl _hostControl;

        public ServiceStartup() { }

        public void Stop()
        {
            _hostControl.Stop();
            Log.Info("ServiceHost stopped");
        }

        public bool Start(HostControl hostControl)
        {
            _hostControl = hostControl;
            var myThread = new Thread(Init) {IsBackground = true};
            myThread.Start();
            return true;
        }

        private static void Init()
        {
            var factories = GetStrategyFactories();
            foreach (var strategy in factories.Select(factory => factory.Create()))
            {
                strategy.Start();
                Log.Info($"{strategy.GetType().Name} started");
            }
            Log.Info($"ServiceHost intialization completed. Found {factories.Length} plugins");
        }

        private static IServiceStrategyFactory[] GetStrategyFactories()
        {
            var pluginsFolder = GetPluginsFolder();
            PluginLoader.LoadPlugins(pluginsFolder, Log);
            return (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                    from assemblyType in domainAssembly.GetTypes()
                    where typeof(IServiceStrategyFactory).IsAssignableFrom(assemblyType) && assemblyType.IsClass
                    select assemblyType).ToArray().Select(Activator.CreateInstance)
                .Select(instance => instance)
                .Cast<IServiceStrategyFactory>()
                .ToArray();
        }

        private static string GetPluginsFolder()
        {
            var setting = ConfigurationManager.AppSettings["PluginsFolder"] ?? "plugins";
            var uri = new Uri(setting, UriKind.RelativeOrAbsolute);
            return uri.IsAbsoluteUri ? uri.AbsolutePath : setting;
        }
    }
}
