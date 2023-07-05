namespace CDRB;

public sealed class BotSettings
{
    public required string BotKey { get; set; }
    public required string CommandPrefix { get; set; }
    public required ulong CurrentServerID { get; set; }
    public required ulong PinChannelID { get; set; }
}