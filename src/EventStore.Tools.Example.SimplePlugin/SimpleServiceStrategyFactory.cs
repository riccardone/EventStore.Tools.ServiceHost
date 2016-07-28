using EventStore.Tools.PluginModel;

namespace EventStore.Tools.Example.SimplePlugin
{
    public class SimpleServiceStrategyFactory : IServiceStrategyFactory
    {
        public string StrategyName => typeof(SimpleServiceStrategyFactory).Name;

        public IServiceStrategy Create()
        {
            return new SimpleServiceStrategy();
        }
    }
}
