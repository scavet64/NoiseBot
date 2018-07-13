using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using NoiseBot.Commands.VoiceCommands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseBot.Controllers
{
    public class AudioController
    {
        private static object lockingObject = new object();

        private BlockingCollection<PlayQueueElement> playQueue = new BlockingCollection<PlayQueueElement>();

        private class PlayQueueElement
        {
            public string Filepath { get; set; }
            public CommandContext Context { get; set; }
            public PlayQueueElement(string filepath, CommandContext context)
            {
                Filepath = filepath;
                Context = context;
            }
        }

        private static AudioController instance;
        public static AudioController Instance
        {
            get {
                if (instance == null)
                {
                    lock (lockingObject)
                    {
                        if(instance == null)
                        {
                            instance = new AudioController();
                        }
                    }
                }
                return instance;
            }
            set { instance = value; }
        }


        private AudioController()
        {
            var t = new Thread(() => AudioPlayingThread());
            t.Start();
        }

        public void AddAudioToQueue(string filepath, CommandContext context)
        {
            playQueue.Add(new PlayQueueElement(filepath, context));
        }

        private async void AudioPlayingThread()
        {
            while (true)
            {
                PlayQueueElement elementToPlay = playQueue.Take();


                await PlayCommandAudio(elementToPlay.Context, elementToPlay.Filepath);
            }
        }

        /// <summary>
        /// Plays audio that originally came from a command.
        /// </summary>
        /// <param name="ctx">The command context</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        private async Task PlayCommandAudio(CommandContext ctx, string filename)
        {
            // check whether VNext is enabled
            await VoiceCommand.JoinIfNotConnected(ctx);
            VoiceNextConnection voiceNextCon = ctx.Client.GetVoiceNext().GetConnection(ctx.Guild);
            if (voiceNextCon == null)
            {
                // already connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

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

            try
            {
                await PlayAudio(voiceNextCon, filename);

                if (playQueue.Count == 0)
                {
                    voiceNextCon.Disconnect();
                }
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync($"An exception occured during playback: `{ex.GetType()}: {ex.Message}`");
            }
        }

        public async static Task PlayAudio(VoiceNextConnection voiceNextCon, string filename)
        {
            // wait for current playback to finish
            while (voiceNextCon.IsPlaying)
            {
                await voiceNextCon.WaitForPlaybackFinishAsync();
            }

            // play
            await voiceNextCon.SendSpeakingAsync(true);
            try
            {
                // borrowed from
                // https://github.com/RogueException/Discord.Net/blob/5ade1e387bb8ea808a9d858328e2d3db23fe0663/docs/guides/voice/samples/audio_create_ffmpeg.cs

                var ffmpeg_inf = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{filename}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var ffmpeg = Process.Start(ffmpeg_inf);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                // let's buffer ffmpeg output
                using (var ms = new MemoryStream())
                {
                    await ffout.CopyToAsync(ms);
                    ms.Position = 0;

                    var buff = new byte[3840]; // buffer to hold the PCM data
                    var br = 0;
                    while ((br = ms.Read(buff, 0, buff.Length)) > 0)
                    {
                        if (br < buff.Length) // it's possible we got less than expected, let's null the remaining part of the buffer
                        {
                            for (var i = br; i < buff.Length; i++)
                            {
                                buff[i] = 0;
                            }
                        }
                        await voiceNextCon.SendAsync(buff, 20); // we're sending 20ms of data
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                await voiceNextCon.SendSpeakingAsync(false);
            }
        }

    }
}
