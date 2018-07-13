using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NoiseBot.Commands.VoiceCommands
{
    class EmoteCommands : PlayAudioCommand
    {
        [Command("StartEmote"), Description("Start doing emote stuff")]
        public async Task StartEmote(CommandContext ctx)
        {
            ctx.Client.MessageCreated += Client_MessageCreatedAsync;
        }

        private async Task Client_MessageCreatedAsync(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (e.Message.Content.Contains(":foodReview:"))
            {
                VoiceNextExtension voiceNextClient = Program.Client.GetVoiceNext();
                VoiceNextConnection voiceNextCon = voiceNextClient.GetConnection(e.Guild);
                if (voiceNextCon == null)
                {
                    foreach(DiscordVoiceState vs in e.Guild.VoiceStates)
                    {
                        if (vs.User.Username.Equals(e.Author.Username))
                        {
                            voiceNextCon = await voiceNextClient.ConnectAsync(vs.Channel);
                        }
                    }
                }
                if(voiceNextCon == null)
                {
                    //user wasnt in a voice channel
                    return;
                }
                else
                {
                    //await PlayAudio(voiceNextCon, @"AudioFiles\foodReview.mp3");
                    voiceNextCon.Disconnect();
                }
            }
        }
    }
}
