using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using NoiseBot.Commands.CommandUtil;
using NoiseBot.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoiseBot.Commands.VoiceCommands.CustomVoiceCommands
{
    class CustomAudioCommand : PlayAudioCommand
    {
        private const int SpacesBetweenCommands = 10;
        private const int MaxLength = 1900;

        [Command("."), Description("Plays a custom audio command.")]
        public async Task PlayCustom(CommandContext ctx, [RemainingText, Description("Name of the command")] string customCommandName)
        {
            CustomAudioCommandModel custCom = CustomAudioCommandFile.Instance.GetAudioFileForCommand(customCommandName);

            if (custCom == null)
            {
                // file does not exist
                await ctx.RespondAsync("Could not find a custom command with that name :^(");
                return;
            }

            ctx.Client.DebugLogger.Info(string.Format("{0} executed custom command: {1}", ctx.User.Username, customCommandName));
            await Play(ctx, custCom.Filepath);
        }

        [Command("CustomAdd"), Description("Adds a custom audio command.")]
        public async Task AddCustom(CommandContext ctx, [RemainingText, Description("Name of the command")] string customCommandName)
        {
            CustomAudioCommandModel custCom = CustomAudioCommandFile.Instance.GetAudioFileForCommand(customCommandName);
            if (custCom != null)
            {
                await ctx.RespondAsync("A command with that name already exists :^(");
                return;
            }

            if (await FileDownloadUtil.DownloadFileFromDiscordMessageAsync(ctx))
            {
                DiscordAttachment attachment = ctx.Message.Attachments[0];
                string customAudioPath = string.Format(@"AudioFiles\{0}", attachment.FileName);

                CustomAudioCommandFile.Instance.AddCustomCommand(customCommandName, customAudioPath);
                await ctx.RespondAsync(string.Format("Custom command ```{0}``` was added :^)", customCommandName));
            }
        }

        [Command("CustomDelete"), Description("Deletes a custom audio command.")]
        public async Task DeleteCustom(CommandContext ctx, [RemainingText, Description("Name of the command")] string customCommandName)
        {
            CustomAudioCommandModel custCom = CustomAudioCommandFile.Instance.GetAudioFileForCommand(customCommandName);
            if (custCom == null)
            {
                await ctx.RespondAsync(string.Format("Could not find a command with the name `{0}` :^(", customCommandName));
                return;
            }

            if (CustomAudioCommandFile.Instance.DeleteCustomCommand(custCom))
            {
                await ctx.RespondAsync(string.Format("Custom command `{0}` was deleted :^)", customCommandName));
            }
            else
            {
                await ctx.RespondAsync(string.Format("Custom command `{0}` could not be deleted :^(", customCommandName));
            }
        }

        [Command("CustomRename"), Description("Renames a custom audio command.")]
        public async Task DeleteCustom(
            CommandContext ctx, 
            [Description("Old name of the command")] string customCommandName,
            [Description("New name of the command")] string newCustomCommandName)
        {
            bool success = false;
            try
            {
                success = CustomAudioCommandFile.Instance.RenameCustomCommand(customCommandName, newCustomCommandName);
            }
            catch (ArgumentException)
            {
                await ctx.RespondAsync(string.Format("Could not find a command with the name `{0}` :^(", customCommandName));
                return;
            }

            if (success)
            {
                await ctx.RespondAsync(string.Format("Custom command `{0}` was renamed to `{1}` :^)", customCommandName, newCustomCommandName));
            }
        }

        [Command("CustomList"), Description("Get a list of all custom commands")]
        public async Task ListCustom(CommandContext ctx, [RemainingText, Description("Name of the command")] string customCommandName)
        {
            List<string> custCom = CustomAudioCommandFile.Instance.GetAllCustomCommandNames();

            if (custCom.Count == 0)
            {
                await ctx.RespondAsync("No custom commands :^(");
                return;
            }

            List<string> contentLists = GetCommandlistParts(custCom);

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x5349fe),
                Title = "List of custom commands:",
                Description = string.Format("To use a custom command use `{0}. ` followed by the desired command", ConfigFile.Instance.CommandPrefix)
            };

            //embed.AddField($"{custCom.Count} Custom Commands:", contentLists[0]);

            await ctx.RespondAsync($"```{contentLists[0]}```");
            for (int i = 1; i < contentLists.Count; i++)
            {
                await ctx.RespondAsync($"```{contentLists[i]}```");
                //embed.AddField($"Continued:", contentLists[i]);
            }

            //await ctx.RespondAsync(embed: embed);
        }

        [Command("Random"), Description("Plays a random command")]
        public async Task RandomCommand(CommandContext ctx)
        {
            List<string> custCom = CustomAudioCommandFile.Instance.GetAllCustomCommandNames();

            string command = custCom[new Random().Next(custCom.Count)];

            await PlayCustom(ctx, command);
        }

        private List<string> GetCommandlistParts(List<string> custCom)
        {
            List<string> returnList = new List<string>();
            int longestCommand = FindLongestCommandLength(custCom);

            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < custCom.Count; i++)
            {
                string prepend = ConfigFile.Instance.CommandPrefix + ". " + custCom[i] + (i % 2 == 1 ? "\n" : GetSpacesToAdd(custCom[i], longestCommand));
                if (builder.Length + prepend.Length < MaxLength)
                {
                    builder.Append(prepend);
                }
                else
                {
                    returnList.Add(builder.ToString());
                    builder.Clear();
                }
            }
            returnList.Add(builder.ToString());

            return returnList;
        }

        private string GetSpacesToAdd(string command, int longest)
        {
            // Find the difference between the command name and the longest
            int temp = longest - command.Length;
            int spacesToAdd = temp + SpacesBetweenCommands;

            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < spacesToAdd; i++)
            {
                builder.Append(" ");
            }
            return builder.ToString();
        }

        private int FindLongestCommandLength(List<string> custCom)
        {
            int longest = 0;
            for (int i = 0; i < custCom.Count; i++)
            {
                if (custCom[i].Length > longest)
                {
                    longest = custCom[i].Length;
                }
            }

            return longest;
        }
    }
}
