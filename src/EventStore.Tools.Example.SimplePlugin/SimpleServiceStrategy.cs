using System;
using EventStore.Tools.PluginModel;

namespace EventStore.Tools.Example.SimplePlugin
{
    public class SimpleServiceStrategy : IServiceStrategy
    {
        public bool Start()
        {
            Console.WriteLine("I'm a SimpleServiceStrategy. You can use me to instantiate and start your application service logic.");
            return true;
        }
    }
}
