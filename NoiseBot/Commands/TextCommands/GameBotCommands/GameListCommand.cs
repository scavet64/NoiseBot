using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using NoiseBot.Commands.CommandUtil;
using NoiseBot.Commands.VoiceCommands.CustomVoiceCommands;

namespace NoiseBot.Commands.VoiceCommands.GameBotCommands
{
    public class GameListCommand : BaseCommandModule
    {
        [Command("AddGame"), Description("Adds a game to the game list.")]
        public async Task AddGame(CommandContext ctx, [RemainingText, Description("Name of the command")] string game)
        {
            if (GameListFile.Instance.GetAllGames().Contains(game))
            {
                await ctx.RespondAsync("This is the best gun!");
                return;
            }

            GameListFile.Instance.AddGame(game);
            await ctx.RespondAsync($"{game} added.");
        }

        [Command("RemoveGame"), Description("Removes a game to the game list.")]
        public async Task RemoveGame(CommandContext ctx, [RemainingText, Description("Name of the command")] string game)
        {
            if (!GameListFile.Instance.GetAllGames().Contains(game))
            {
                await ctx.RespondAsync("This is not the best gun :^(");
                return;
            }

            GameListFile.Instance.RemoveGame(game);
            await ctx.RespondAsync($"{game} removed.");
        }

        [Command("GameList"), Description("Adds a game to the game list.")]
        public async Task GameList(CommandContext ctx)
        {
            var games = GameListFile.Instance.GetAllGames();
            games.Sort();

            string content = string.Empty;
            foreach (string game in games)
            {
                content += game + "\n";
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x5349fe),
                Title = "List of games:",
            };

            embed.AddField($"{games.Count} Games:", content);
            await ctx.RespondAsync(embed: embed);
        }

        [Command("Vote"), Description("Starts a game vote.")]
        public async Task Vote(CommandContext ctx, [RemainingText, Description("number of voters")] int number)
        {
            var games = GameListFile.Instance.GetAllGames();
            var gameToVotes = new Dictionary<string, int>(games.Count);

            foreach (var game in games)
            {
                await ctx.RespondAsync($"Vote for {game}. (0-3)");
                gameToVotes[game] = await AwaitVotes(ctx, number);
                
                if(gameToVotes[game] == -1)
                {
                    await ctx.RespondAsync("Vote cancelled :^(");
                    return;
                }
            }

            List<string> possibleGames = new List<string>();
            foreach (var game in gameToVotes.Keys)
            {
                for (int i = 0; i < gameToVotes[game]; i++)
                {
                    possibleGames.Add(game);
                }
            }

            Random rng = new Random();

            List<string> gamesChosen = new List<string>();
            for (int i = 0; i < 5 && possibleGames.Count > 0; i++)
            {
                string choice = possibleGames[rng.Next(possibleGames.Count)];
                gamesChosen.Add(choice);
                possibleGames.RemoveAll(s => s == choice);
            }

            string message = "Play these games :^)\n";
            foreach (var game in gamesChosen)
            {
                message += $"\t{game}\n";
            }

            await ctx.RespondAsync(message);
        }

        private async Task<int> AwaitVotes(CommandContext ctx, int number)
        {
            var votes = new List<int>(number);
            var users = new List<string>(number);

            while (votes.Count < number)
            {
                var test = ctx.Client.GetInteractivity();
                var message = await test.WaitForMessageAsync(m =>
                {
                    try
                    {
                        if (m.Content.ToLower() == "stop") return true;

                        int vote = int.Parse(m.Content);
                        return !users.Contains(m.Author.Username) && vote >= 0 && vote <= 3;
                    }
                    catch (FormatException)
                    {
                        return false;
                    }
                });

                if (message.Message.Content.ToLower() == "stop") return -1;

                users.Add(message.User.Username);
                votes.Add(int.Parse(message.Message.Content));
            }

            return votes.Contains(0) ? 0 : votes.Sum();
        }
    }
}
