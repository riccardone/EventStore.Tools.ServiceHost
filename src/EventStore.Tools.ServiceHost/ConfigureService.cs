using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
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
            //var connection = Configuration.CreateConnection();
            var plugInContainer = FindPlugins();
            var serviceContainersFactories = GetPlugInServiceContainersStrategyFactories(plugInContainer);
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

        private static IServiceStrategyFactory[] GetPlugInServiceContainersStrategyFactories(CompositionContainer plugInContainer)
        {
            var allPlugins = plugInContainer.GetExports<IServicePlugin>();

            var strategyFactories = new List<IServiceStrategyFactory>();

            foreach (var potentialPlugin in allPlugins)
            {
                try
                {
                    var plugin = potentialPlugin.Value;
                    Log.Info($"Loaded strategy plugin: {plugin.Name} version {plugin.Version}.");
                    strategyFactories.Add(plugin.GetStrategyFactory());
                }
                catch (CompositionException ex)
                {
                    Log.Error(ex);
                }
            }

            return strategyFactories.ToArray();
        }

        private static CompositionContainer FindPlugins()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
            var pluginsDirectory = GetPluginDirectory();

            if (Directory.Exists(pluginsDirectory))
            {
                Log.Info($"Plugins path: {pluginsDirectory}");
                catalog.Catalogs.Add(new DirectoryCatalog(pluginsDirectory));
            }
            else
            {
                Log.Info($"Cannot find plugins path: {pluginsDirectory}");
            }

            return new CompositionContainer(catalog);
        }

        private static string GetPluginDirectory()
        {
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                                   Path.GetFullPath(".");

            var pluginsDirectory = Path.Combine(applicationDirectory, "plugins");
            return pluginsDirectory;
        }
    }
}
