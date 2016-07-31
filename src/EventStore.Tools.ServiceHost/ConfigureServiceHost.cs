using System;
using System.Configuration;
using System.Linq;
using EventStore.Tools.PluginModel;
using log4net;
using log4net.Config;
using Topshelf;

namespace EventStore.Tools.ServiceHost
{
    public static class ConfigureServiceHost
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigureServiceHost));
        public static void Configure()
        {
            if (ConfigurationManager.GetSection("log4net") != null)
                XmlConfigurator.Configure();
            else
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
                x.SetDescription("This process load and run Application Service modules from the plugins directory");
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
