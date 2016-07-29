using System;
using System.Linq;
using EventStore.Tools.PluginModel;
using log4net;
using Topshelf;

namespace EventStore.Tools.ServiceHost
{
    public static class ConfigureHostService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigureHostService));
        public static void Configure()
        {
            //XmlConfigurator.Configure();
            Logger.Setup();
            var serviceContainersFactories = GetStrategyFactoriesFromPlugins();
            HostFactory.Run(x =>
            {
                x.UseLog4Net();
                x.Service<ServiceContainer>(s =>
                {
                    s.ConstructUsing(name => new ServiceContainer(serviceContainersFactories));
                    s.WhenStarted(
                        (tc, hostControl) =>
                            tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.StartAutomatically();
                x.SetDescription("This process host Application Service modules from the plugins directory");
                x.SetDisplayName("EventStore Tools ServiceHost");
                x.SetServiceName("EventStore.Tools.ServiceHost");
            });
        }

        private static IServiceStrategyFactory[] GetStrategyFactoriesFromPlugins()
        {
            PluginLoader.LoadPlugins("plugins", Log);
            var plugins =
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName.Contains("Plugin") && !a.FullName.Contains("PluginModel"))
                    .ToList();
            return (from domainAssembly in plugins
                from assemblyType in domainAssembly.GetTypes()
                where typeof(IServiceStrategyFactory).IsAssignableFrom(assemblyType)
                select assemblyType).ToArray().Select(Activator.CreateInstance)
                .Select(instance => instance)
                .Cast<IServiceStrategyFactory>()
                .ToArray();
        }
    }
}
