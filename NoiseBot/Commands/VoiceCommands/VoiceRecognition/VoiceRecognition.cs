using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace NoiseBot.Commands.VoiceCommands.VoiceRecognition
{
    class VoiceRecognition : VoiceCommand
    {
        private ConcurrentDictionary<uint, ulong> ssrcMap;
        private ConcurrentDictionary<uint, FileStream> ssrcFilemap;
        private Timer speakingSentence;

        private static void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e, FileStream fs)
        {
            fs.Close();
            if (source is Timer timer)
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        private Task OnVoiceReceived(VoiceReceiveEventArgs e)
        {
            lock (this)
            {
                FileStream fs;
                if (!this.ssrcFilemap.ContainsKey(e.SSRC))
                {
                    fs = File.Create($"{e.SSRC}.pcm");
                    this.ssrcFilemap[e.SSRC] = fs;
                }

                fs = this.ssrcFilemap[e.SSRC];

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

                // e.Client.DebugLogger.LogMessage(LogLevel.Debug, "VNEXT RX", $"{e.User?.Username ?? "Unknown user"} sent voice data.", DateTime.Now);
                var buff = e.Voice.ToArray();
                fs.Write(buff, 0, buff.Length);
                fs.Flush();
            }
            return Task.CompletedTask;
        }

        private Task OnUserSpeaking(UserSpeakingEventArgs e)
        {
            if (this.ssrcMap.ContainsKey(e.SSRC))
            {
                return Task.CompletedTask;
            }

            if (e.User == null)
            {
                return Task.CompletedTask;
            }

            this.ssrcMap[e.SSRC] = e.User.Id;
            return Task.CompletedTask;
        }

        [Command("StartListen"), Description("Start voice recognition"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task StartListen(CommandContext ctx)
        {
            await JoinIfNotConnected(ctx);
            VoiceNextExtension voiceNextClient = ctx.Client.GetVoiceNext();
            VoiceNextConnection voiceNextCon = voiceNextClient.GetConnection(ctx.Guild);
            if (voiceNextClient.IsIncomingEnabled)
            {
                this.ssrcMap = new ConcurrentDictionary<uint, ulong>();
                this.ssrcFilemap = new ConcurrentDictionary<uint, FileStream>();
                voiceNextCon.VoiceReceived += this.OnVoiceReceived;
                voiceNextCon.UserSpeaking += this.OnUserSpeaking;
            }
        }

        [Command("StopListen"), Description("Stop voice recognition"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task StopListen(CommandContext ctx)
        {
            if (await IsClientConnected(ctx))
            {
                VoiceNextExtension voiceNextClient = ctx.Client.GetVoiceNext();
                VoiceNextConnection voiceNextCon = ctx.Client.GetVoiceNext().GetConnection(ctx.Guild);
                if (voiceNextClient.IsIncomingEnabled)
                {
                    this.ssrcMap = new ConcurrentDictionary<uint, ulong>();
                    this.ssrcFilemap = new ConcurrentDictionary<uint, FileStream>();
                    voiceNextCon.VoiceReceived += null;
                    voiceNextCon.UserSpeaking += null;
                }
            }
        }
    }
}
