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
        public Task Play(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string filename)
        {
            AudioController.Instance.AddAudioToQueue(filename, ctx);
            //maybe say added to queue?
            return Task.CompletedTask;
        }
    }
}
