using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Net;

namespace NoiseBot.Commands.VoiceCommands.CustomVoiceCommands
{
    class CustomAudioCommand : PlayAudioCommand
    {
        [Command("Custom"), Description("Plays an audio file.")]
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

        [Command("CustomAdd"), Description("Adds a custom audio file.")]
        public async Task AddCustom(CommandContext ctx, [RemainingText, Description("Name of the command")] string customCommandName)
        {
            if (ctx.Message.Attachments.Count <= 0)
            {
                await ctx.RespondAsync("No file was attached to the message :^(");
                return;
            }

            CustomAudioCommandModel custCom = CustomAudioCommandFile.Instance.GetAudioFileForCommand(customCommandName);
            if (custCom != null)
            {
                await ctx.RespondAsync("A command with that name already exists :^(");
                return;
            }

            DiscordAttachment attachment = ctx.Message.Attachments[0];
            string customAudioPath = string.Format(@"AudioFiles\{0}", attachment.FileName);

            if (File.Exists(customAudioPath))
            {
                await ctx.RespondAsync("A command with that filename already exists :^(");
                return;
            }

            using (var client = new WebClient())
            {
                client.DownloadFile(attachment.Url, customAudioPath);
            }

            CustomAudioCommandFile.Instance.AddCustomCommand(customCommandName, customAudioPath);
            await ctx.RespondAsync(string.Format("Custom command ```{0}``` was added :^)", customCommandName));
        }
    }
}
