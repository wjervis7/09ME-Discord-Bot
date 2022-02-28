namespace _09.Mass.Extinction.Discord.Events;

using global::Discord.WebSocket;

public interface IMessageHandler
{
    string Name { get; }
    Task Handle(SocketUserMessage message);
}
