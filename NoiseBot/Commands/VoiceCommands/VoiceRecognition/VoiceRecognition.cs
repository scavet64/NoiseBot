using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.EventArgs;
using NAudio.Wave;
using NoiseBot.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Threading.Tasks;
using System.Timers;

namespace NoiseBot.Commands.VoiceCommands.VoiceRecognition
{
    class VoiceRecognition : VoiceCommand
    {
        ConcurrentDictionary<ulong, bool> IsSpeechPreservedForUser { get; } = new ConcurrentDictionary<ulong, bool>();
        ConcurrentDictionary<ulong, ConcurrentQueue<byte>> SpeechFromUser { get; } = new ConcurrentDictionary<ulong, ConcurrentQueue<byte>>();
        private ConcurrentDictionary<uint, Process> ffmpegs = new ConcurrentDictionary<uint, Process>();
        Dictionary<uint, BufferedWaveProvider> waveProviders = new Dictionary<uint, BufferedWaveProvider>();
        MixingWaveProvider32 mixer = new MixingWaveProvider32();
        WaveFormat format = new WaveFormat(48000, 2);

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

        public async Task OnVoiceReceived(VoiceReceiveEventArgs args)
        {
            if (args.User != null)
            {
                var user = true;
                if (!SpeechFromUser.ContainsKey(args.User.Id))
                {
                    user = SpeechFromUser.TryAdd(args.User.Id, new ConcurrentQueue<byte>());
                }

                if (user)
                {
                    var buff = args.PcmData.ToArray();
                    foreach (var b in buff)
                    {
                        SpeechFromUser[args.User.Id].Enqueue(b);
                    }
                }
            }
        }

        private async Task OnUserSpeaking(UserSpeakingEventArgs args)
        {
            if (args.User != null && args.Speaking == false)
            {
                //User stopped talking
                var queue = SpeechFromUser[args.User.Id];
                var buff = queue.ToArray();
                File.Delete($"{args.User.Id}Before.pcm");
                File.WriteAllBytes($"{args.User.Id}Before.pcm", buff);

                byte[] resampled = AudioConverterService.Resample(buff, 48000, 16000, 1, 1);
                File.Delete($"{args.User.Id}After.pcm");
                File.WriteAllBytes($"{args.User.Id}After.pcm", resampled);

                using (var stream = new MemoryStream(buff))
                {
                    RecognizeCompletedEventArgs Args;
                    var RecognizeWaiter = new TaskCompletionSource<RecognizeCompletedEventArgs>();
                    using (var Engine = await SpeechEngine.Get((s, e) => RecognizeWaiter.SetResult(e)))
                    {
                        Engine.Recognize(stream);
                        Args = await RecognizeWaiter.Task;
                    }

                    if (Args.Result != null)
                    {
                        Console.WriteLine($"you said: '{Args.Result?.Text}' with {Args.Result?.Confidence} confidence");
                    }
                    
                    //using (var rs = new RawSourceWaveStream(stream, new WaveFormat(16000, 16, 1)))
                    //using (var sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US")))
                    //{
                    //    var temp = new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null);
                    //    sre.SetInputToAudioStream(rs, temp);
                    //    sre.LoadGrammar(new DictationGrammar());

                    //    var result = sre.Recognize();
                    //    Console.WriteLine(result?.Text);
                    //}
                }

                if (!IsSpeechPreservedForUser.ContainsKey(args.User.Id) || IsSpeechPreservedForUser[args.User.Id])
                {
                    SpeechFromUser[args.User.Id] = new ConcurrentQueue<byte>();
                }
            }
        }

        [Command("StartListen"), Description("Start voice recognition")]
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

        [Command("StopListen"), Description("Stop voice recognition")]
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
                    voiceNextCon.VoiceReceived -= this.OnVoiceReceived;
                    voiceNextCon.UserSpeaking -= this.OnUserSpeaking;
                }
            }
        }

        // Things that were tried...

        //private Task Connection_VoiceReceived(VoiceReceiveEventArgs e)
        //{
        //    if (!waveProviders.ContainsKey(e.SSRC))
        //    {
        //        BufferedWaveProvider provider = new BufferedWaveProvider(format) { DiscardOnBufferOverflow = true, BufferDuration = TimeSpan.FromMilliseconds(1000) };
        //        mixer.AddInputStream(new Wave16ToFloatProvider(provider));
        //        waveProviders[e.SSRC] = provider;
        //    }

        //    waveProviders[e.SSRC].AddSamples(e.PcmData.ToArray(), 0, e.PcmData.Length);
        //    return Task.CompletedTask;
        //}


        //public async Task OnVoiceReceived(VoiceReceiveEventArgs ea)
        //{
        //    if (!this.ffmpegs.ContainsKey(ea.SSRC))
        //    {
        //        var psi = new ProcessStartInfo
        //        {
        //            FileName = "ffmpeg",
        //            Arguments = $@"-ac 2 -f s16le -ar 48000 -i pipe:0 -ac 2 -ar 44100 {ea.SSRC}.wav",
        //            RedirectStandardInput = true
        //        };

        //        this.ffmpegs.TryAdd(ea.SSRC, Process.Start(psi));
        //    }

        //    var buff = ea.Voice.ToArray();

        //    var ffmpeg = this.ffmpegs[ea.SSRC];
        //    await ffmpeg.StandardInput.BaseStream.WriteAsync(buff, 0, buff.Length);
        //}

        //private async Task OnVoiceReceived(VoiceReceiveEventArgs ea)
        //{
        //    //lock (this)
        //    //{
        //    //    FileStream fs;
        //    //    if (!this.ssrcFilemap.ContainsKey(e.SSRC))
        //    //    {
        //    //        fs = File.Create($"{e.SSRC}.pcm");
        //    //        this.ssrcFilemap[e.SSRC] = fs;
        //    //    }

        //    //    fs = this.ssrcFilemap[e.SSRC];

        //    //    if (speakingSentence != null)
        //    //    {
        //    //        speakingSentence.Dispose();
        //    //    }
        //    //    speakingSentence = new Timer
        //    //    {
        //    //        Interval = 2000
        //    //    };

        //    //    speakingSentence.Elapsed += (sender, args) => OnTimedEvent(sender, args, fs);
        //    //    speakingSentence.Start();

        //    //    // e.Client.DebugLogger.LogMessage(LogLevel.Debug, "VNEXT RX", $"{e.User?.Username ?? "Unknown user"} sent voice data.", DateTime.Now);

        //    //    var buff = e.PcmData.ToArray();
        //    //    fs.Write(buff, 0, buff.Length);
        //    //    fs.Flush();
        //    //}
        //    //return Task.CompletedTask;

        //    if (!this.ffmpegs.ContainsKey(ea.SSRC))
        //    {
        //        // Starts writing audio to file using ffmpeg
        //        var psi = new ProcessStartInfo
        //        {
        //            FileName = "ffmpeg",
        //            Arguments = $@"-ac 2 -f s16le -ar 22050 -i pipe:0 -ac 2 -ar 44100 {ea.SSRC}.wav",
        //            RedirectStandardInput = true,
        //            UseShellExecute = false
        //        };

        //        // Adds voice stream to dictionary
        //        this.ffmpegs.TryAdd(ea.SSRC, Process.Start(psi));
        //    }

        //    var buff = ea.PcmData.ToArray();

        //    var ffmpeg = this.ffmpegs[ea.SSRC];
        //    //await ffmpeg.StandardInput.BaseStream.WriteAsync(buff, 0, buff.Length);
        //    //await ffmpeg.StandardInput.BaseStream.FlushAsync();

        //    using (var sre = new SpeechRecognitionEngine())
        //    {

        //        //var temp = new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Eight, AudioChannel.Stereo);
        //        sre.SetInputToWaveStream(ffmpeg.StandardInput.BaseStream);
        //        sre.LoadGrammar(new DictationGrammar());

        //        sre.BabbleTimeout = new TimeSpan(Int32.MaxValue);
        //        sre.InitialSilenceTimeout = new TimeSpan(Int32.MaxValue);
        //        sre.EndSilenceTimeout = new TimeSpan(100000000);
        //        sre.EndSilenceTimeoutAmbiguous = new TimeSpan(100000000);

        //        var result = sre.Recognize();
        //        Console.WriteLine(result.Text);
        //    }

        //    return;
        //}
    }
}
