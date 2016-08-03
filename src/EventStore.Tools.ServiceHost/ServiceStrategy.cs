using System.Linq;
using EventStore.Tools.PluginModel;
using log4net;

namespace EventStore.Tools.ServiceHost
{
    internal class ServiceStrategy : IServiceStrategy
    {
        private readonly IServiceStrategyFactory[] _factories;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceStrategy));

        public ServiceStrategy(IServiceStrategyFactory[] factories)
        {
            _factories = factories;
        }

        public void Stop()
        {
            Log.Info("ServiceHost stopped");
        }

        public bool Start()
        {
            foreach (var strategy in _factories.Select(factory => factory.Create()))
            {
                strategy.Start();
                Log.Info($"{strategy.GetType().Name} started");
            }
            Log.Info("ServiceHost started");
            return true;
        }
    }
}
