using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
using System.Threading;
using NoiseBot.Commands.VoiceCommands.CustomVoiceCommands;
using NoiseBot.Extensions;

namespace NoiseBot.Commands.VoiceCommands.CustomIntroCommands
{
    class CustomIntroCommand : PlayAudioCommand
    {
        private List<DiscordUser> users = new List<DiscordUser>();
        private bool IsDoingIntros  = false;
        //private AutoResetEvent NewUserJoinedWaitHandle = new AutoResetEvent(false);

        BlockingCollection<IntroQueueElement> introQueue = new BlockingCollection<IntroQueueElement>();

        private class IntroQueueElement
        {
            public DiscordUser NewUser { get; set; }
            public DiscordChannel ChannelToJoin { get; set; }
            public DiscordGuild GuildToJoin { get; set; }
        }

        public CustomIntroCommand()
        {
            Program.Client.VoiceStateUpdated += Client_VoiceStateUpdated;
            IsDoingIntros = true;

            var t = new Thread(() => IntroMethodThread());
            t.Start();
        }

        private async Task Client_VoiceStateUpdated(VoiceStateUpdateEventArgs e)
        {
            //Ignore bots joining
            if (e.User.IsBot)
            {
                return;
            }
            try
            {
                lock (users)
                {
                    if (e.After.Channel != null && (e.Before == null || e.Before.Channel == null))
                    {
                        IntroQueueElement introQueueElement = new IntroQueueElement
                        {
                            NewUser = e.User,
                            ChannelToJoin = e.After.Channel,
                            GuildToJoin = e.After.Guild
                        };

                        introQueue.Add(introQueueElement);

                        e.Client.DebugLogger.LogMessage(LogLevel.Debug, "NoiseBot", string.Format("{0} joined channel {1}", e.User, e.After.Channel.Name), DateTime.Now);
                        //NewUserJoinedWaitHandle.Set();
                    }
                    else if (e.After.Channel == null && e.Before.Channel != null)
                    {
                        e.Client.DebugLogger.LogMessage(LogLevel.Debug, "NoiseBot", string.Format("{0} left channel {1}", e.User.Username, e.Before.Channel.Name), DateTime.Now);
                    }
                }
            }
            catch (Exception ex)
            {
                e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", string.Format("{0}", ex.Message), DateTime.Now);
            }
            return;
        }

        [Command("StartIntro"), Description("Start doing intros")]
        public async Task StartIntro(CommandContext ctx)
        {
            if (!IsDoingIntros)
            {
                ctx.Client.VoiceStateUpdated += Client_VoiceStateUpdated;
                IsDoingIntros = true;

                await ctx.RespondAsync("I will now introduce people as they join voice chat");
                //await JoinIfNotConnected(ctx);
                var t = new Thread(() => IntroMethodThread());
                t.Start();
            } 
            else
            {
                await ctx.RespondAsync("Already doing intros. To reset, use the `StopIntro` command");
                return;
            }

        }

        [Command("StopIntro"), Description("Stop doing intros")]
        public async Task StopIntro(CommandContext ctx)
        {
            ctx.Client.VoiceStateUpdated += NullVoiceStateUpdateHandler;
            IsDoingIntros = false;
        }

        private Task NullVoiceStateUpdateHandler(VoiceStateUpdateEventArgs e)
        {
            return Task.CompletedTask;
        }

        private async void IntroMethodThread()
        {
            while (IsDoingIntros)
            {
                IntroQueueElement introQueueElement = introQueue.Take();
                //NewUserJoinedWaitHandle.WaitOne();
                if (IsDoingIntros)
                {
                    VoiceNextExtension voiceNextClient = Program.Client.GetVoiceNext();
                    VoiceNextConnection voiceNextCon = voiceNextClient.GetConnection(introQueueElement.GuildToJoin);
                    if (voiceNextCon == null)
                    {
                        voiceNextCon = await voiceNextClient.ConnectAsync(introQueueElement.ChannelToJoin);
                    }

                    CustomIntroModel introModel = CustomIntroFile.Instance.GetIntroForUsername(introQueueElement.NewUser.Username);
                    string filepath;
                    if (introModel != null)
                    {
                        Program.Client.DebugLogger.Warn(string.Format("Playing {0} for {1}", introModel.Filepath, introQueueElement.NewUser.Username));
                        filepath = introModel.Filepath;
                    }
                    else
                    {
                        Program.Client.DebugLogger.Warn(string.Format("No intro was found for {0}. Using default", introQueueElement.NewUser.Username));
                        filepath = @"AudioFiles\fuckyou.mp3";
                    }
                    var vnc = Program.Client.GetVoiceNext().GetConnection(introQueueElement.GuildToJoin);
                    await PlayAudio(vnc, filepath);
                    vnc.Disconnect();
                }
            }
        }

        [Command("AddIntro"), Description("Add an intro for when you join voice chat")]
        public async Task AddIntro(CommandContext ctx, [RemainingText, Description("Command you want used as your intro")] string customCommandName)
        {
            CustomAudioCommandModel custCom = CustomAudioCommandFile.Instance.GetAudioFileForCommand(customCommandName);
            if (custCom == null)
            {
                await ctx.RespondAsync("Cannot find command with that name :^(");
                return;
            }

            //do you have an intro already?
            CustomIntroModel introModel = CustomIntroFile.Instance.GetIntroForUsername(ctx.User.Username);
            if(introModel != null)
            {
                //user already has an intro
                if(!CustomIntroFile.Instance.ChangeIntro(ctx.User.Username, custCom.Filepath))
                {
                    await ctx.RespondAsync(string.Format("I could not change your intro for some reason :^)"));
                    return;
                }
            }

            CustomIntroFile.Instance.AddCustomIntro(ctx.User.Username, custCom.Filepath);
            await ctx.RespondAsync(string.Format("I will now play `{0}` when you join voice channels :^)", customCommandName));
        }
    }
}
