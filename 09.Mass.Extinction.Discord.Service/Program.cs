using _09.Mass.Extinction.Data;
using _09.Mass.Extinction.Discord;
using _09.Mass.Extinction.Discord.Commands;
using _09.Mass.Extinction.Discord.DiscordActivity;
using _09.Mass.Extinction.Discord.Events;
using _09.Mass.Extinction.Discord.Service;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Serilog;
using DateTime = _09.Mass.Extinction.Discord.Commands.DateTime;
using TimeZone = _09.Mass.Extinction.Discord.Commands.TimeZone;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.docker.json", true, true))
    .UseSerilog((hostingContext, _, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration))
    .ConfigureServices((hostingContext, services)=>
    {
        services.Configure<DiscordConfiguration>(hostingContext.Configuration.GetSection("Discord"));
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(hostingContext.Configuration.GetConnectionString("DefaultConnection")));

        // Discord.Net stuff
        var client = new DiscordSocketClient(new DiscordSocketConfig {
            AlwaysDownloadUsers = true,
            GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMembers |
                GatewayIntents.DirectMessages |
                GatewayIntents.DirectMessageReactions
        });
        services.AddSingleton(client);
        services.AddSingleton<MessageCache>();
        services.AddSingleton<CommandService>();
        // services.AddSingleton<CommandHandlingService>();
        services.AddSingleton(new InteractiveService(client));

        // Discord handlers/commands
        services.AddSingleton<IMessageHandler, PrivateMessageHandler>();
        services.AddSingleton<ISlashCommand, Nickname>();
        services.AddSingleton<ISlashCommand, Activity>();
        services.AddSingleton<ISlashCommand, TimeZone>();
        services.AddSingleton<ISlashCommand, Time>();
        services.AddSingleton<ISlashCommand, DateTime>();
        services.AddScoped<ActivityHelper>();
        services.AddSingleton<DiscordClient>();

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
