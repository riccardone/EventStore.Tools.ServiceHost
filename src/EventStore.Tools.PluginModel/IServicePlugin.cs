namespace EventStore.Tools.PluginModel
{
    public interface IServicePlugin
    {
        string Name { get; }

        string Version { get; }

        IServiceStrategyFactory GetStrategyFactory();
    }
}
