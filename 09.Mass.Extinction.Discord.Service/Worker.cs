namespace Ninth.Mass.Extinction.Discord.Service;

public class Worker(ILogger<Worker> logger, DiscordClient discordClient) : BackgroundService
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting worker.");
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => discordClient.StartClient();

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping worker.");
        return base.StopAsync(cancellationToken);
    }
}
