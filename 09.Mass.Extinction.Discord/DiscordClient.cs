namespace _09.Mass.Extinction.Discord;

using System.Text.Json;
using Commands;
using Events;
using global::Discord;
using global::Discord.Net;
using global::Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class DiscordClient
{
    private readonly ILogger<DiscordClient> _logger;
    private readonly IOptionsMonitor<DiscordConfiguration> _configurationMonitor;
    private readonly DiscordSocketClient _client;
    private readonly IEnumerable<IMessageHandler> _messageHandlers;
    private readonly Dictionary<string, ISlashCommand> _slashCommands;

    public static SocketGuild Guild { get; private set; }

    public DiscordClient(ILogger<DiscordClient> logger, IOptionsMonitor<DiscordConfiguration> configurationMonitor, DiscordSocketClient client, IEnumerable<ISlashCommand> slashCommands, IEnumerable<IMessageHandler> messageHandlers)
    {
        _logger = logger;
        _configurationMonitor = configurationMonitor;
        _client = client;
        _slashCommands = new Dictionary<string, ISlashCommand>();
        foreach (var slashCommand in slashCommands)
        {
            _slashCommands.Add(slashCommand.Name, slashCommand);
        }
        _messageHandlers = messageHandlers;
    }

    public async Task StartClient()
    {
        _client.Log += DiscordLogger;
        _client.Ready += ClientReady;

        _logger.LogInformation("Logging in.");
        await _client.LoginAsync(TokenType.Bot, _configurationMonitor.CurrentValue.Token);
        _logger.LogInformation("Logged in.");
        _logger.LogInformation("Starting client.");
        await _client.StartAsync();
    }

    private async Task ClientReady()
    {
        _logger.LogInformation("Client started.");
        Guild = _client.GetGuild(_configurationMonitor.CurrentValue.GuildId);

        // setup slash commands
        foreach (var (name, slashCommand) in _slashCommands)
        {
            _logger.LogInformation("Creating/updating slash command {command}.", name);
            var command = CreateSlashCommand(slashCommand);
            try
            {
                await Guild.CreateApplicationCommandAsync(command.Build());
                _logger.LogInformation("Slash command created/updated.");
            }
            catch (HttpException exception)
            {
                var json = JsonSerializer.Serialize(exception.Errors);
                _logger.LogError(exception, "An error has occurred adding the command, {command}:\n{error}.", name, json);
            }
        }

        _client.SlashCommandExecuted += command =>
        {
            var loggerScope = new Dictionary<string, object> 
            {
                { "channel", command.Channel.Id },
                { "caller", command.User.Id },
                { "command", command.CommandName },
                { "options", command.Data.Options }
            };
            using (_logger.BeginScope(loggerScope))
            {
                _logger.LogDebug("Slash command executed.");
                if (!_slashCommands.ContainsKey(command.CommandName))
                {
                    _logger.LogWarning("Invalid command used.");
                    return command.RespondAsync("Invalid command used.", ephemeral: true);
                }

                _logger.LogDebug("Getting slash command handler.");
                var slashCommand = _slashCommands[command.CommandName];
                var commandConfiguration = _configurationMonitor.CurrentValue.CommandConfiguration.FirstOrDefault(c => c.Command == slashCommand.Name);

                if (ISlashCommand.IsInvalidUsage(commandConfiguration, command, out var message))
                {
                    _logger.LogWarning(message);
                    return command.RespondAsync("You are not authorized to use this command.", ephemeral: true);
                }

                _logger.LogDebug("Executing slash command {slashCommand}.", slashCommand.Name);
                return slashCommand.Handle(command);
            }
        };

        // setup message handlers
        foreach (var messageHandler in _messageHandlers)
        {
            _logger.LogInformation("Attaching message handler: {handler}.", messageHandler.Name);
            _client.MessageReceived += message => messageHandler.Handle((SocketUserMessage)message);
            _logger.LogInformation("Message handler attached.");
        }
    }

    private static SlashCommandBuilder CreateSlashCommand(ISlashCommand slashCommand)
    {
        var command = new SlashCommandBuilder();
        command.WithName(slashCommand.Name);
        command.WithDescription(slashCommand.Description);
        command.AddOptions(slashCommand.Options);

        return command;
    }

    private Task DiscordLogger(LogMessage arg)
    {
        switch (arg.Severity)
        {
            case LogSeverity.Critical:
                _logger.LogCritical(arg.Exception, "Discord: {source}.", arg.Source);
                break;
            case LogSeverity.Error:
                _logger.LogError(arg.Exception, "Discord: {source}.", arg.Source);
                break;
            case LogSeverity.Warning:
                _logger.LogWarning(arg.Exception, "Discord: {message}.", arg.Message);
                break;
            case LogSeverity.Info:
                _logger.LogInformation("Discord: {message}.", arg.Message);
                break;
            case LogSeverity.Verbose:
                _logger.LogTrace("Discord: {message}.", arg.Message);
                break;
            case LogSeverity.Debug:
                _logger.LogDebug(arg.Exception, "Discord: {message}.", arg.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return Task.CompletedTask;
    }
}
