# EventStore.Tools.ServiceHost
This library can be used to create a process that run interectively or as a service. It hosts Application Service modules that interact with GetEventStore https://github.com/EventStore  
  
Usage:  
1) Create a simple Console app  
2) Reference EventStore.Tools.ServiceHost nuget package: PM> Install-Package EventStore.Tools.ServiceHost  
3) From the Main function call 'ConfigureHostService.Configure();' 

To create a plugin  
1) Create a class library project with the AssemblyName terminating with the word 'Plugin' (Ex. MyServicePlugin)  
2) Reference EventStore.Tools.PluginModel nuget package: PM> Install-Package EventStore.Tools.PluginModel  
3) Implement the two interfaces provided by the PluginModel  
4) Build the project and copy all the dll's into the ServiceHost\Plugins directory  
  
Here you can see an example of a simple Host project   https://github.com/riccardone/EventStore.Tools.ServiceHost/tree/master/src/EventStore.Tools.Example.Host  
and a minimal Plugin  
https://github.com/riccardone/EventStore.Tools.ServiceHost/tree/master/src/EventStore.Tools.Example.SimplePlugin 
