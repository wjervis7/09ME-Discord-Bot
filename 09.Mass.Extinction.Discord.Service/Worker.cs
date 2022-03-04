namespace _09.Mass.Extinction.Discord.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly DiscordClient _discordClient;

    public Worker(ILogger<Worker> logger, DiscordClient discordClient)
    {
        _logger = logger;
        _discordClient = discordClient;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting worker.");
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => _discordClient.StartClient();

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping worker.");
        return base.StopAsync(cancellationToken);
    }
}
