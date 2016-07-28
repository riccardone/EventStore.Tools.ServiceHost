using System;
using EventStore.Tools.PluginModel;

namespace EventStore.Tools.Example.SimplePlugin
{
    public class SimpleServiceStrategy : IServiceStrategy
    {
        public bool Start()
        {
            Console.WriteLine("SimpleServiceStrategy started");
            return true;
        }
    }
}
