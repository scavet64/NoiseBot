using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace NoiseBot.Commands.VoiceCommands
{
    class MadWorldCommand : PlayAudioCommand
    {
        [Command("Madworld"), Description("For when its a mad world")]
        public async Task Madworld(CommandContext ctx)
        {
            string madWorldPath = @"AudioFiles\MadWorld.ogg";
            // check if file exists
            if (!File.Exists(madWorldPath))
            {
                // file does not exist
                await ctx.RespondAsync("Cant find the file");
                return;
            }

            await Play(ctx, madWorldPath);
        }
    }
}
