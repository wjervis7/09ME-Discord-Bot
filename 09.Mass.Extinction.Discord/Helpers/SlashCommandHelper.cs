namespace Ninth.Mass.Extinction.Discord.Helpers;

using global::Discord.WebSocket;

public static class SlashCommandHelper
{
    public static TValue GetValue<TValue>(this IEnumerable<SocketSlashCommandDataOption> options, string name) => (TValue)options.Single(o => o.Name == name).Value;

    public static TValue? GetNullableValue<TValue>(this IEnumerable<SocketSlashCommandDataOption> options, string name) => (TValue?)options.SingleOrDefault(o => string.Equals(o.Name, name))?.Value;
}
