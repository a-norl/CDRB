using PokeApiNet;
using DSharpPlus.Entities;

namespace CDRB.Commands;

public class PokemonCommands
{
    private readonly PokeApiClient _pokeApiClient;
    private readonly Random _random;
    static readonly Dictionary<string, string> TypeToColour = new()
        {
            { "normal", "A8A77A" },
            { "fire", "EE8130" },
            { "water", "6390F0" },
            { "electric", "F7D02C" },
            { "grass", "7AC74C" },
            { "ice", "96D9D6" },
            { "fighting", "C22E28" },
            { "poison", "A33EA1" },
            { "ground", "E2BF65" },
            { "flying", "A98FF3" },
            { "psychic", "F95587" },
            { "bug", "A6B91A" },
            { "rock", "B6A136" },
            { "ghost", "735797" },
            { "dragon", "6F35FC" },
            { "dark", "705746" },
            { "steel", "B7B7CE" },
            { "fairy", "D685AD" },
        };

    public PokemonCommands(PokeApiClient pokeApiClient, Random random)
    {
        _pokeApiClient = pokeApiClient;
        _random = random;
    }

    public async Task<DiscordMessageBuilder> GetPokemonByDex(int dexNum)
    {
        Pokemon requestedPokemon = await _pokeApiClient.GetResourceAsync<Pokemon>(dexNum);
        var pokemonEmbed = constructPokemonEmbed(requestedPokemon);
        return new DiscordMessageBuilder().WithEmbed(await pokemonEmbed);
    }

    public async Task<DiscordMessageBuilder> GetPokemonByName(string pokeName)
    {
        Pokemon requestedPokemon = await _pokeApiClient.GetResourceAsync<Pokemon>(pokeName);
        var pokemonEmbed = constructPokemonEmbed(requestedPokemon);
        return new DiscordMessageBuilder().WithEmbed(await pokemonEmbed);
    }

    public async Task<DiscordMessageBuilder> GetPokemonRandom()
    {
        int randomDex = _random.Next(1001);
        Pokemon requestedPokemon = await _pokeApiClient.GetResourceAsync<Pokemon>(randomDex);
        var pokemonEmbed = constructPokemonEmbed(requestedPokemon);
        return new DiscordMessageBuilder().WithEmbed(await pokemonEmbed);
    }

    public async Task<DiscordEmbedBuilder> constructPokemonEmbed(Pokemon pokemon)
    {
        var pokemonEmbed = new DiscordEmbedBuilder();

        return pokemonEmbed;
    }
}