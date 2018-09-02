using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using NoiseBot.Commands.VoiceCommands;
using NoiseBot.Extensions;
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
    /// <summary>
    /// Audio controller. This class is responsible for managing and playing any audio into the server.
    /// </summary>
    public class AudioController
    {
        private static object lockingObject = new object();

        private static AudioController instance;

        /// <summary>
        /// Gets or sets the singleton instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static AudioController Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockingObject)
                    {
                        if (instance == null)
                        {
                            instance = new AudioController();
                        }
                    }
                }
                return instance;
            }
            set { instance = value; }
        }

        private BlockingCollection<PlayQueueElement> playQueue = new BlockingCollection<PlayQueueElement>();

        private class PlayQueueElement
        {
            public string Filepath { get; set; }
            public DiscordChannel ChannelToJoin { get; set; }
            public DiscordGuild GuildToJoin { get; set; }
        }

        private AudioController()
        {
            var t = new Thread(() => AudioPlayingThread());
            t.Start();
        }

        /// <summary>
        /// Adds the audio to queue.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        /// <param name="channelToJoin">The channel to join.</param>
        /// <param name="guildToJoin">The guild to join.</param>
        /// <returns>position in the queue</returns>
        public int AddAudioToQueue(string filepath, DiscordChannel channelToJoin, DiscordGuild guildToJoin)
        {
            PlayQueueElement playQueueElement = new PlayQueueElement
            {
                Filepath = filepath,
                ChannelToJoin = channelToJoin,
                GuildToJoin = guildToJoin
            };
            playQueue.Add(playQueueElement);
            Program.Client.DebugLogger.Info($"Added playing file [{filepath}] to position [{playQueue.Count}] of the queue");

            return playQueue.Count;
        }

        private async void AudioPlayingThread()
        {
            while (true)
            {
                try
                {
                    PlayQueueElement elementToPlay = playQueue.Take();
                    Program.Client.DebugLogger.Info($"Took [{elementToPlay.Filepath}] off the queue");

                    // Connect if not already
                    VoiceNextExtension voiceNextClient = Program.Client.GetVoiceNext();
                    VoiceNextConnection voiceNextCon = voiceNextClient.GetConnection(elementToPlay.GuildToJoin);
                    if (voiceNextCon == null)
                    {
                        Program.Client.DebugLogger.Info($"Not currently connected");
                        Task<VoiceNextConnection> voiceNextConTask = voiceNextClient.ConnectAsync(elementToPlay.ChannelToJoin);
                        voiceNextConTask.Wait(new TimeSpan(0, 0, 3));
                        if(voiceNextConTask.IsCompletedSuccessfully)
                        {
                            voiceNextCon = voiceNextConTask.Result;
                            Program.Client.DebugLogger.Info($"Joined: {voiceNextCon.Channel}");
                        }
                        else
                        {
                            Program.Client.DebugLogger.Error($"Could not join: {voiceNextCon.Channel}");
                            continue;
                        }
                        
                    }

                    await PlayAudio(voiceNextCon, elementToPlay.Filepath);
                    if (playQueue.Count == 0)
                    {
                        voiceNextCon.Disconnect();
                        Program.Client.DebugLogger.Info($"Leaving: {voiceNextCon.Channel}");
                    }
                }
                catch (Exception ex)
                {
                    Program.Client.DebugLogger.Critical($"Exception was caught in the Audio Thread: {ex}");
                }
            }
        }

        /// <summary>
        /// Plays the audio.
        /// </summary>
        /// <param name="voiceNextCon">The voice next con.</param>
        /// <param name="filename">The filename.</param>
        /// <returns>Completed Task once the audio finishes playing</returns>
        public async Task PlayAudio(VoiceNextConnection voiceNextCon, string filename)
        {
            // wait for current playback to finish
            while (voiceNextCon.IsPlaying)
            {
                Program.Client.DebugLogger.Info($"Waiting for current audio to finish");
                await voiceNextCon.WaitForPlaybackFinishAsync();
            }

            // play
            await voiceNextCon.SendSpeakingAsync(true);
            try
            {
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
                Program.Client.DebugLogger.Info($"Exception playing audio {ex}");
                throw ex;
            }
            finally
            {
                await voiceNextCon.SendSpeakingAsync(false);
                Program.Client.DebugLogger.Info($"Finished playing audio");
            }
        }
    }
}
