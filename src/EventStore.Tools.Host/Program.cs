using EventStore.Tools.ServiceHost;

namespace EventStore.Tools.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureServiceHost.Configure();
        }
    }
}
