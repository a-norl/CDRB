using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;
using ImageMagick;
using DSharpPlus.SlashCommands.EventArgs;
using CDRB.EventListeners;
using CDRB.Commands;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using CDRB.Database;
using PokeApiNet;

namespace CDRB;

public sealed class BotService : IHostedService
{
    private readonly ILogger<BotService> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IHostEnvironment _hostEnviroment;
    private readonly BotSettings _botSettings;
    private readonly IDbContextFactory<BotDatabaseContext> _botDatabaseContextFactory;
    private readonly DiscordClient _discordClient;
    private readonly CommandsNextExtension _commandsNext;
    private readonly SlashCommandsExtension _slashCommands;
    private readonly HttpClient _httpClient;
    private readonly PokeApiClient _pokeApiClient;

    public BotService(ILogger<BotService> logger, IHostApplicationLifetime applicationLifetime, IHostEnvironment hostEnvironment, IOptions<BotSettings> botSettings, IDbContextFactory<BotDatabaseContext> botDatabaseContextFactory)
    {
        this._logger = logger;
        this._applicationLifetime = applicationLifetime;
        this._hostEnviroment = hostEnvironment;
        this._botSettings = botSettings.Value;
        this._botDatabaseContextFactory = botDatabaseContextFactory;

        this._discordClient = new(new()
        {
            Token = _botSettings.BotKey,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            LoggerFactory = new LoggerFactory().AddSerilog(),
            MinimumLogLevel = LogLevel.Information,
        });

        MagickNET.Initialize();
        MagickNET.SetLogEvents(LogEvents.Exception);
        MagickNET.Log += (sender, args) =>
        {
            _logger.LogDebug("ImageMagick: {message}", args.Message);
        };

        var pinboard = new Pinboard(_botSettings);

        _httpClient = new HttpClient();
        _pokeApiClient = new PokeApiClient(_httpClient);

        var services = new ServiceCollection()
            .AddLogging(logging => logging.AddSerilog())
            .AddDbContextFactory<BotDatabaseContext>()
            .AddSingleton<Random>()
            .AddSingleton(this._botSettings)
            .AddSingleton(this._hostEnviroment)
            .AddSingleton(this._httpClient)
            .AddSingleton(this._pokeApiClient)
            .AddSingleton<UtilityCommands>()
            .AddSingleton<ImageCommands>()
            .AddSingleton<PokemonCommands>()
            .BuildServiceProvider()
            ;

        _commandsNext = _discordClient.UseCommandsNext(new()
        {
            StringPrefixes = new [] {_botSettings.CommandPrefix},
            Services = services,
            EnableDms = false
        });
        _slashCommands = _discordClient.UseSlashCommands(new()
        {
            Services = services
        });

        _discordClient.MessageUpdated += pinboard.Archiver;

        _commandsNext.RegisterCommands<PrefixCommandTriggers>();
        _slashCommands.RegisterCommands<SlashCommandTriggers>(_botSettings.CurrentServerID);

        _commandsNext.CommandExecuted += PrefixCommandExecuted;
        _commandsNext.CommandErrored += PrefixCommandErrored;

        _slashCommands.SlashCommandExecuted += SlashCommandExecuted;
        _slashCommands.SlashCommandErrored += SlashCommandErrored;
    }

    #pragma warning disable CS1998
    private async Task PrefixCommandExecuted(CommandsNextExtension cnext, CommandExecutionEventArgs eventArgs)
    {
        _logger.LogInformation("Command {commandName} executed for user {discordUser}", eventArgs.Command.Name, eventArgs.Context.User.Username);
    }

    private async Task PrefixCommandErrored(CommandsNextExtension cnext, CommandErrorEventArgs eventArgs)
    {
        if(eventArgs.Command is not null)
        {
            _logger.LogError(eventArgs.Exception, "Command {commandName} requested by user {discordUser} threw an exception", eventArgs.Command.Name, eventArgs.Context.User.Username);
        }
    }
    
    private async Task SlashCommandExecuted(SlashCommandsExtension snext, SlashCommandExecutedEventArgs eventArgs)
    {
        _logger.LogInformation("Command {commandName} executed for user {discordUser}", eventArgs.Context.CommandName, eventArgs.Context.User.Username);
    }

    private async Task SlashCommandErrored(SlashCommandsExtension snext, SlashCommandErrorEventArgs eventArgs)
    {
        _logger.LogError(eventArgs.Exception, "Command {commandName} requested by user {discordUser} threw an exception", eventArgs.Context.CommandName, eventArgs.Context.User.Username);
    }
    #pragma warning restore CS1998 

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _discordClient.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discordClient.DisconnectAsync();
    }
}