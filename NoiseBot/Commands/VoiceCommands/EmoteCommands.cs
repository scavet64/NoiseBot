using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using NoiseBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoiseBot.Commands.VoiceCommands
{
    class EmoteCommands : PlayAudioCommand
    {
        [Command("StartEmote"), Description("Start doing emote stuff")]
        public Task StartEmote(CommandContext ctx)
        {
            ctx.Client.MessageCreated += Client_MessageCreatedAsync;
            return Task.CompletedTask;
        }

        private async Task Client_MessageCreatedAsync(MessageCreateEventArgs e)
        {
            await Task.Run(() =>
            {
                if (e.Message.Content.Contains(":foodReview:"))
                {
                    ProcessEmoteSound(e, @"AudioFiles\foodReview.mp3");
                }
                if (e.Message.Content.Contains(":steventrue:"))
                {
                    ProcessEmoteSound(e, @"AudioFiles\true.mp3");
                }
            });
        }

        private void ProcessEmoteSound(MessageCreateEventArgs messageCreateEvent, string path)
        {
            VoiceNextExtension voiceNextClient = Program.Client.GetVoiceNext();
            VoiceNextConnection voiceNextCon = voiceNextClient.GetConnection(messageCreateEvent.Guild);
            if (voiceNextCon == null)
            {
                DiscordChannel channel = messageCreateEvent.Guild.VoiceStates[messageCreateEvent.Author.Id].Channel;
                AudioService.Instance.AddAudioToQueue(path, channel, messageCreateEvent.Guild);
            }
        }
    }
}
