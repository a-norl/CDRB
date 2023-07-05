using DSharpPlus;
using DSharpPlus.Entities;
using Serilog.Sinks.InMemory;

namespace CDRB.Commands;

public class UtilityCommands
{

    private readonly Random _random;

    public UtilityCommands(Random random)
    {
        _random = random;
    }

    public DiscordMessageBuilder RandomNumber(long max)
    {
        return new DiscordMessageBuilder().WithContent($"{_random.NextInt64(max)}");
    }

    public DiscordMessageBuilder TimeFromNow(double seconds, double minutes, double hours, double days, long months)
    {
        var datetimefromnow = DateTime.Now.AddSeconds(seconds).AddMinutes(minutes).AddHours(hours).AddDays(days).AddMonths((int)months);
        return new DiscordMessageBuilder().WithContent($"{Formatter.Timestamp(datetimefromnow, TimestampFormat.LongDateTime)}");
    }

    public DiscordMessageBuilder GetLogs(int amount, DiscordUser caller)
    {
        var callerMember = (DiscordMember) caller;
        var logs = InMemorySink.Instance.LogEvents.TakeLast(amount);
        var logEmbed = new DiscordEmbedBuilder()
            .WithAuthor(callerMember.DisplayName, null, callerMember.GetGuildAvatarUrl(ImageFormat.Png));
        string logString = "```\n";
        foreach(var log in logs)
        {
            var stringwriter = new StringWriter();
            log.RenderMessage(stringwriter);
            logString += $"{stringwriter.ToString()}\n";
        }
        logString += "```";
        logEmbed.AddField($"Last {amount} Logged Events", logString);

        return new DiscordMessageBuilder().AddEmbed(logEmbed);
    }

    public DiscordMessageBuilder RollDice(int diceAmount, int diceSide)
    {
        ulong totalResult = 0;
        for(int i = 0; i < diceAmount; i++)
        {
            totalResult += (ulong) _random.Next(1, diceSide+1);
        }

        return new DiscordMessageBuilder().WithContent($"Rolled a {diceAmount}d{diceSide}.\n Result: {Formatter.Bold(totalResult.ToString())}");
    }
}