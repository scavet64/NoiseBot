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
    class WormsCommand : PlayAudioCommand
    {
        [Command("worms"), Description("Open up a can of those")]
        public async Task Worms(CommandContext ctx)
        {
            string wormsFilePath = @"AudioFiles\worms.ogg";
            // check if file exists
            if (!File.Exists(wormsFilePath))
            {
                // file does not exist
                await ctx.RespondAsync("Cant find the file");
                return;
            }

            await Play(ctx, wormsFilePath);
        }
    }
}
