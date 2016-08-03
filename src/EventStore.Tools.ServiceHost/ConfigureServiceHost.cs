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
            var factories = GetStrategyFactories();
            HostFactory.Run(x =>
            {
                x.UseLog4Net();
                x.Service<ServiceStrategy>(s =>
                {
                    s.ConstructUsing(name => new ServiceStrategy(factories));
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
