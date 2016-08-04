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
        public static void Configure()
        {
            if (ConfigurationManager.GetSection("log4net") != null)
                XmlConfigurator.Configure();
            else
                Logger.Setup();
            HostFactory.Run(x =>
            {
                x.UseLog4Net();
                x.Service<ServiceStartup>(s =>
                {
                    s.ConstructUsing(name => new ServiceStartup());
                    s.WhenStarted(
                        (tc, hostControl) =>
                            tc.Start(hostControl));
                    s.WhenStopped(tc => tc.Stop());
                });
                x.StartAutomatically();
                x.SetDescription("This process load and run Application Service modules from the plugins directory");
                x.SetDisplayName("EventStore Tools ServiceHost");
                x.SetServiceName("EventStore.Tools.ServiceHost");
            });
        }
    }
}
