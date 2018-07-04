using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace NoiseBot.Commands.VoiceCommands.CustomVoiceCommands
{
    class CustomVoiceCommand : VoiceCommand
    {
        [Command("Custom"), Description("Plays an audio file.")]
        public async Task PlayCustom(CommandContext ctx, [RemainingText, Description("Name of the command")] string customCommandName)
        {
            CustomAudioCommandModel custCom = CustomCommandFile.Instance.GetAudioFileForCommand(customCommandName);

            if (custCom == null)
            {
                // file does not exist
                await ctx.RespondAsync("Could not find a custom command with that name :^(");
                return;
            }
        }
    }
}
