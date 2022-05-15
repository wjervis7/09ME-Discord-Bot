namespace _09.Mass.Extinction.Discord.Service;

using _09.Mass.Extinction.Discord.Helpers;
using Commands;
using Data;
using DiscordActivity;
using Events;
using global::Discord;
using global::Discord.Addons.Interactive;
using global::Discord.Commands;
using global::Discord.WebSocket;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;

public static class Startup
{
    public static void ConfigureHost(IHostBuilder host)
    {
        host.ConfigureAppConfiguration((context, configuration) =>
        {
            if (!context.HostingEnvironment.IsEnvironment("Docker"))
            {
                return;
            }

            configuration.AddJsonFile("appsettings.docker.json", false, true);
            configuration.AddKeyPerFile("/run/secrets", true, true);
        });

        host.UseSerilog((hostingContext, _, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
    }

    public static void ConfigureServices(HostBuilderContext hostingContext, IServiceCollection services)
    {
        var configuration = hostingContext.Configuration;

        var connectionStringBuilder = new SqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"));
        var sqlPassword = configuration.GetValue<string>("sqlpass");
        if (!string.IsNullOrWhiteSpace(sqlPassword))
        {
            connectionStringBuilder.Password = sqlPassword;
        }

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionStringBuilder.ToString()));

        services.Configure<DiscordConfiguration>(configuration.GetSection("Discord"));

        // Discord.Net stuff
        var client = new DiscordSocketClient(new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMembers |
                GatewayIntents.DirectMessages |
                GatewayIntents.DirectMessageReactions
            //UseInteractionSnowflakeDate = false
        });
        services.AddSingleton(client);
        services.AddScoped<MessageCache>();
        services.AddSingleton<CommandService>();
        // services.AddSingleton<CommandHandlingService>();
        services.AddSingleton(new InteractiveService(client));

        // Discord handlers/commands
        services.AddSingleton<IMessageHandler, PrivateMessageHandler>();
        services.AddSingleton<ISlashCommand, Onboard>();
        services.AddSingleton<ISlashCommand, Nickname>();
        services.AddSingleton<ISlashCommand, Activity>();
        services.AddSingleton<ISlashCommand, TimeZone>();
        services.AddSingleton<ISlashCommand, Time>();
        services.AddSingleton<ISlashCommand, DateTime>();
        services.AddScoped<ActivityHelper>();
        services.AddSingleton<DiscordClient>();

        services.AddHostedService<Worker>();
    }
}
