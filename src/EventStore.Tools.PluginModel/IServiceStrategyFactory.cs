namespace EventStore.Tools.PluginModel
{
    public interface IServiceStrategyFactory
    {
        string StrategyName { get; }
        IServiceStrategy Create();
    }
}
