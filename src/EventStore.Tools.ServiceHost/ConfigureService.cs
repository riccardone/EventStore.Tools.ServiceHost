using System;
using System.Linq;
using EventStore.Tools.PluginModel;
using log4net;
using log4net.Config;
using Topshelf;

namespace EventStore.Tools.ServiceHost
{
    internal static class ConfigureService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigureService));
        internal static void Configure()
        {
            XmlConfigurator.Configure();
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
                x.SetDescription("This process host any Application Service module implementing the IServicePlugin interface from the plugins directory");
                x.SetDisplayName("EventStore Host");
                x.SetServiceName("EventStore.Host");
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
