namespace _09.Mass.Extinction.Discord.Helpers;

using global::Discord.WebSocket;

public static class SlashCommandHelper
{
    public static TValue GetValue<TValue>(this IReadOnlyCollection<SocketSlashCommandDataOption> options, string name) => (TValue)options.Single(o => o.Name == name).Value;

    public static TValue? GetNullableValue<TValue>(this IReadOnlyCollection<SocketSlashCommandDataOption> options, string name) => (TValue?)options.SingleOrDefault(o => o.Name == name)?.Value;
}
