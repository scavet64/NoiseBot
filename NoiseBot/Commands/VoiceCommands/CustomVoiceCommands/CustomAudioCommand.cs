using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Net;
using System.Collections.Generic;
using NoiseBot.Commands.CommandUtil;

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

        [Command("CustomList"), Description("Get a list of all custom commands")]
        public async Task ListCustom(CommandContext ctx, [RemainingText, Description("Name of the command")] string customCommandName)
        {
            List<string> custCom = CustomAudioCommandFile.Instance.GetAllCustomCommandNames();
            string content = "";
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

            embed.AddField("Custom Commands:", content);
            await ctx.RespondAsync(embed: embed);
        }
    }
}
