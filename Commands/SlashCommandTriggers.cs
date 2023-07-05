using DSharpPlus.SlashCommands;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using CDRB.Database;

namespace CDRB.Commands;

public class SlashCommandTriggers : ApplicationCommandModule
{
    private readonly HttpClient _httpClient;
    private readonly IDbContextFactory<BotDatabaseContext> _botDatabaseContextFactory;
    private readonly UtilityCommands _utilityCommands;
    // private readonly PokemonCommands _pokemonCommands;
    private readonly ImageCommands _imageCommands;

    public SlashCommandTriggers(HttpClient httpClient, UtilityCommands utilityCommands, /*PokemonCommands pokemonCommands,*/ ImageCommands imageCommands, IDbContextFactory<BotDatabaseContext> botDatabaseContextFactory)
    {
        _httpClient = httpClient;
        _utilityCommands = utilityCommands;
        // _pokemonCommands = pokemonCommands;
        _imageCommands = imageCommands;
        _botDatabaseContextFactory = botDatabaseContextFactory;
    }

    [SlashCommand("random", "generate a random number")]
    public async Task RandomCommand(InteractionContext context, [Option("max", "max number in generated range")] long max)
    {
        await context.CreateResponseAsync(new(_utilityCommands.RandomNumber(max)));
    }

    [SlashCommand("TimeFromNow", "Get when the specified time interval is from now.")]
    public async Task TimeFromNowCommand(InteractionContext context,
        [Option("seconds", "x seconds from now")] double seconds = 0,
        [Option("minutes", "x minutes from now")] double minutes = 0,
        [Option("hours", "x hours from now")] double hours = 0,
        [Option("days", "x days from now")] double days = 0,
        [Option("months", "x months from now")] long months = 0)
    {
        await context.CreateResponseAsync(new(_utilityCommands.TimeFromNow(seconds, minutes, hours, days, months)));
    }

    [SlashCommand("CheckLogs", "Check the bot's logs")]
    public async Task TimeFromNowCommand(InteractionContext context, [Option("Amount", "How many logged events to get")] long amount)
    {
        await context.CreateResponseAsync(new(_utilityCommands.GetLogs((int)amount, context.Member)));
    }

    [SlashCommand("Roll", "Roll some dice")]
    public async Task RollDiceCommand(InteractionContext context, [Option("Amount", "How many dice to roll")] long diceAmount, [Option("Sides", "How many side does do the dice have")] long diceSides)
    {
        if(diceSides >= int.MaxValue)
        {
            await context.CreateResponseAsync("Too many dice sides");
            return;
        } 
        else if(diceAmount > int.MaxValue)
        {
            await context.CreateResponseAsync("Too many dice");
            return;
        }
        await context.DeferAsync();
        await context.EditResponseAsync(new(_utilityCommands.RollDice((int)diceAmount, (int)diceSides)));
    }

    [SlashCommand("wojak", "turns the selected user into a crying wojak")]
    public async Task WojakCommand(InteractionContext context, [Option("victim", "who has angered you")] DiscordUser victim)
    {
        await context.DeferAsync();
        var victimMember = (DiscordMember) victim;
        var image = await victimMember.ConvertToImageAsync(_httpClient);
        var imageEmbed = _imageCommands.WojakPhoto(image, context.Member, victimMember.DisplayName);
        await context.EditResponseAsync(new(imageEmbed));
    }

    [SlashCommand("Impact", "Put impact text on the attached image")]
    public async Task ImpactCommand(InteractionContext context, [Option("image", "what image to write on")] DiscordAttachment attachment, [Option("TopText", "top text")] string topText = " ", [Option("BottomText", "bottom text")] string bottomText = " ")
    {
        await context.DeferAsync();
        var image = await attachment.ConvertToImageAsync(_httpClient);
        var imageEmbed = _imageCommands.ImpactPhoto(image, topText, bottomText, context.Member);
        await context.EditResponseAsync(new(imageEmbed));
    }

    [SlashCommand("InflateUser", "inflates the selected user big and round.")]
    public async Task InflateUser(InteractionContext context, [Option("user", "who to inflate")] DiscordUser victim)
    {
        await context.DeferAsync();
        var victimMember = (DiscordMember) victim;
        var image = await victimMember.ConvertToImageAsync(_httpClient);
        await context.EditResponseAsync(new(_imageCommands.InflatePhoto(image, context.Member, victimMember.DisplayName)));
    }
}