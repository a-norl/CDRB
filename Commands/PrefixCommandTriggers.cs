using CDRB.Database;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;


namespace CDRB.Commands;

public class PrefixCommandTriggers : BaseCommandModule
{

    private readonly HttpClient _httpClient;
    private readonly IDbContextFactory<BotDatabaseContext> _botDatabaseContextFactory;
    private readonly UtilityCommands _utilityCommands;
    // private readonly PokemonCommands _pokemonCommands;
    private readonly ImageCommands _imageCommands;
    private readonly Regex _timeRegex;

    public PrefixCommandTriggers(HttpClient httpClient, UtilityCommands utilityCommands, /*PokemonCommands pokemonCommands,*/ ImageCommands imageCommands, IDbContextFactory<BotDatabaseContext> botDatabaseContextFactory)
    {
        _timeRegex = new Regex(@"(?:(\d+)(?:months|month))?(?:(\d+\.?\d*)(?:days|day|d))?(?:(\d+\.?\d*)(?:hours|hour|h))?(?:(\d+\.?\d*)(?:minutes|minute|min|m))?(?:(\d+\.?\d*)(?:seconds|second|sec|s))?", RegexOptions.IgnoreCase);
        _httpClient = httpClient;
        _utilityCommands = utilityCommands;
        // _pokemonCommands = pokemonCommands;
        _imageCommands = imageCommands;
        _botDatabaseContextFactory = botDatabaseContextFactory;
    }

    [Command("random")]
    public async Task RandomCommand(CommandContext context, long max)
    {
        await context.RespondAsync(_utilityCommands.RandomNumber(max));
    }

    [Command("timefromnow")]
    public async Task TimeFromNowCommand(CommandContext context, string input)
    {
        var timeMatches = _timeRegex.Matches(input);
        if (!timeMatches[0].Success)
        {
            await context.RespondAsync("Unable to parse input.");
            return;
        }

        double seconds = 0, minutes = 0, hours = 0, days = 0;
        int months = 0;
        foreach (var key in timeMatches[0].Groups.Keys)
        {
            var value = timeMatches[0].Groups[key].Value;
            switch (key)
            {
                case "1": //months
                    if (value != String.Empty)
                        months = int.Parse(value);
                    break;
                case "2": //days
                    if (value != String.Empty)
                        days = double.Parse(value);
                    break;
                case "3": //hours
                    if (value != String.Empty)
                        hours = double.Parse(value);
                    break;
                case "4": //minutes
                    if (value != String.Empty)
                        minutes = double.Parse(value);
                    break;
                case "5": //seconds
                    if (value != String.Empty)
                        seconds = double.Parse(value);
                    break;
            }
        }
        await context.RespondAsync(_utilityCommands.TimeFromNow(seconds, minutes, hours, days, months));
    }

    [Command("checklogs")]
    public async Task CheckLogsCommand(CommandContext context, int amount)
    {
        if(context.Member is not null)
            await context.RespondAsync(_utilityCommands.GetLogs(amount, context.Member));
    }

    [Command("roll")]
    public async Task RollDiceCommand(CommandContext context, ulong diceAmount, ulong diceSides)
    {
        if(diceSides >= int.MaxValue)
        {
            await context.RespondAsync("Too many dice sides");
            return;
        } 
        else if(diceAmount > int.MaxValue)
        {
            await context.RespondAsync("Too many dice");
            return;
        }
        await context.RespondAsync(_utilityCommands.RollDice((int)diceAmount, (int)diceSides));
    }
}