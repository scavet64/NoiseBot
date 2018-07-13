using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Threading;
using System.Collections.Concurrent;
using NoiseBot.Controllers;

namespace NoiseBot.Commands.VoiceCommands
{
    class PlayAudioCommand : VoiceCommand
    {
        [Command("play"), Description("Plays an audio file.")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                // file does not exist
                await ctx.RespondAsync($"Specify the file you want to play");
                return;
            }

            // check if file exists
            if (!File.Exists(filename))
            {
                // file does not exist
                await ctx.RespondAsync($"File `{filename}` does not exist.");
                return;
            }

            // get member's voice state
            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null)
            {
                // they did not specify a channel and are not in one
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            int placeInQueue = AudioController.Instance.AddAudioToQueue(filename, vstat?.Channel, ctx.Guild);
            //maybe say added to queue?
            await ctx.RespondAsync(string.Format("Added to the play queue: {0} in line", placeInQueue));
            return;
        }
    }
}
