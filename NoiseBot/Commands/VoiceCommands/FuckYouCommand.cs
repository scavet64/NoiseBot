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
    class FuckYouCommand : PlayAudioCommand
    {
        [Command("fuckyou"), Description("")]
        public async Task Fuckyou(CommandContext ctx)
        {
            string fuckyoupath = @"AudioFiles\fuckyou.mp3";
            // check if file exists
            if (!File.Exists(fuckyoupath))
            {
                // file does not exist
                await ctx.RespondAsync("Cant find the file");
                return;
            }

            await Play(ctx, fuckyoupath);
        }
    }
}
