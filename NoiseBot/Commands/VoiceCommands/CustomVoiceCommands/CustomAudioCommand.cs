using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using NoiseBot.Commands.CommandUtil;
using NoiseBot.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoiseBot.Commands.VoiceCommands.CustomVoiceCommands
{
    class CustomAudioCommand : PlayAudioCommand
    {
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
            string content = string.Empty;
            foreach (string name in custCom)
            {
                content += ConfigFile.Instance.CommandPrefix + ". " + name + "\n";
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x5349fe),
                Title = "List of custom commands:",
                Description = string.Format("To use a custom command use `{0}. ` followed by the desired command", ConfigFile.Instance.CommandPrefix)
            };

            embed.AddField($"{custCom.Count} Custom Commands:", content);
            await ctx.RespondAsync(embed: embed);
        }
    }
}
