using Ninth.Mass.Extinction.Discord.Service;

var hostBuilder = Host.CreateDefaultBuilder(args);
Startup.ConfigureHost(hostBuilder);
hostBuilder.ConfigureServices(Startup.ConfigureServices);

var host = hostBuilder.Build();

await host.RunAsync();
