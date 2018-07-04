using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Collections.Concurrent;
using System.Linq;
using System.Timers;

namespace NoiseBot.Commands.VoiceCommands.VoiceRecognition
{
    class VoiceRecognition : VoiceCommand
    {
        private ConcurrentDictionary<uint, ulong> _ssrc_map;
        private ConcurrentDictionary<uint, FileStream> _ssrc_filemap;
        private Timer speakingSentence;

        private async Task OnVoiceReceived(VoiceReceiveEventArgs e)
        {
            lock (this)
            {
                FileStream fs;
                if (!this._ssrc_filemap.ContainsKey(e.SSRC))
                {
                    fs = File.Create($"{e.SSRC}.pcm");
                    this._ssrc_filemap[e.SSRC] = fs;
                }

                fs = this._ssrc_filemap[e.SSRC];

                if (speakingSentence != null)
                {
                    speakingSentence.Dispose();
                }
                speakingSentence = new Timer
                {
                    Interval = 2000
                };

                speakingSentence.Elapsed += (sender, args) => OnTimedEvent(sender, args, fs);
                speakingSentence.Start();

                //e.Client.DebugLogger.LogMessage(LogLevel.Debug, "VNEXT RX", $"{e.User?.Username ?? "Unknown user"} sent voice data.", DateTime.Now);
                var buff = e.Voice.ToArray();
                fs.Write(buff, 0, buff.Length);
                fs.Flush();
            }
        }

        private Task OnUserSpeaking(UserSpeakingEventArgs e)
        {
            if (this._ssrc_map.ContainsKey(e.SSRC))
                return Task.CompletedTask;

            if (e.User == null)
                return Task.CompletedTask;

            this._ssrc_map[e.SSRC] = e.User.Id;
            return Task.CompletedTask;
        }
        

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e, FileStream fs)
        {
            fs.Close();
            if(source is Timer timer)
            {
                timer.Stop();
                timer.Dispose();
            }
        }


        [Command("StartListen"), Description("Start voice recognition")]
        public async Task StartListen(CommandContext ctx)
        {
            VoiceNextClient voiceNextClient = await JoinIfNotConnected(ctx);
            VoiceNextConnection voiceNextCon = voiceNextClient.GetConnection(ctx.Guild);
            if (voiceNextClient.IsIncomingEnabled)
            {
                this._ssrc_map = new ConcurrentDictionary<uint, ulong>();
                this._ssrc_filemap = new ConcurrentDictionary<uint, FileStream>();
                voiceNextCon.VoiceReceived += this.OnVoiceReceived;
                voiceNextCon.UserSpeaking += this.OnUserSpeaking;
            }
        }

        [Command("StopListen"), Description("Stop voice recognition")]
        public async Task StopListen(CommandContext ctx)
        {
            if (await IsClientConnected(ctx))
            {
                VoiceNextClient voiceNextClient = ctx.Client.GetVoiceNextClient();
                VoiceNextConnection voiceNextCon = ctx.Client.GetVoiceNextClient().GetConnection(ctx.Guild);
                if (voiceNextClient.IsIncomingEnabled)
                {
                    this._ssrc_map = new ConcurrentDictionary<uint, ulong>();
                    this._ssrc_filemap = new ConcurrentDictionary<uint, FileStream>();
                    voiceNextCon.VoiceReceived += null;
                    voiceNextCon.UserSpeaking += null;
                }
            }
        }


    }
}
