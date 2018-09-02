using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;
using NoiseBot.Commands.VoiceCommands;
using NoiseBot.Commands.VoiceCommands.CustomIntroCommands;
using NoiseBot.Commands.VoiceCommands.CustomVoiceCommands;
using NoiseBot.Commands.VoiceCommands.VoiceRecognition;
using NoiseBot.Exceptions;
using NoiseBot.Extensions;

namespace NoiseBot
{
    class Program
    {
        private static readonly SimpleLogger Logger = SimpleLogger.GetSimpleLogger();

        public static DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }
        public VoiceNextExtension Voice { get; set; }

        private ConfigFile configFile;

        public static void Main(string[] args)
        {
            // since we cannot make the entry method asynchronous,
            // let's pass the execution to asynchronous code
            var prog = new Program();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }

        private DiscordConfiguration GetConfigFromJsonSettings()
        {
            // first, let's load our configuration file
            try
            {
                configFile = ConfigFile.Instance;
            }
            catch (InvalidConfigException ice)
            {
                throw ice;
            }

            var cfg = new DiscordConfiguration
            {
                Token = configFile.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true
            };

            return cfg;
        }

        public async Task RunBotAsync()
        {
            try
            {
                // load settings then we want to instantiate our client
                DiscordConfiguration discordConfiguration = GetConfigFromJsonSettings();
                Client = new DiscordClient(discordConfiguration);
            }
            catch (InvalidConfigException ice)
            {
                Console.WriteLine(string.Format("{0}:\tCRITICAL ERROR: Problem with the Json Settings: {1}\n\nPress enter to close", DateTime.Now, ice.Message));
                Console.ReadLine();
                return;
            }

            // If you are on Windows 7 and using .NETFX, install 
            // DSharpPlus.WebSocket.WebSocket4Net from NuGet,
            // add appropriate usings, and uncomment the following
            // line
            // this.Client.SetWebSocketClient<WebSocket4NetClient>();

            // If you are on Windows 7 and using .NET Core, install 
            // DSharpPlus.WebSocket.WebSocket4NetCore from NuGet,
            // add appropriate usings, and uncomment the following
            // line
            // this.Client.SetWebSocketClient<WebSocket4NetCoreClient>();

            // If you are using Mono, install 
            // DSharpPlus.WebSocket.WebSocketSharp from NuGet,
            // add appropriate usings, and uncomment the following
            // line
            // this.Client.SetWebSocketClient<WebSocketSharpClient>();

            // if using any alternate socket client implementations, 
            // remember to add the following to the top of this file:
            // using DSharpPlus.Net.WebSocket;

            // next, let's hook some events, so we know
            // what's going on
            Client.Ready += this.Client_Ready;
            Client.GuildAvailable += this.Client_GuildAvailable;
            Client.ClientErrored += this.Client_ClientError;
            Client.PresenceUpdated += Client_PresenceUpdated;
            Client.DebugLogger.LogMessageReceived += DebugLogger_LogMessageReceived;
            

            // up next, let's set up our commands
            var ccfg = new CommandsNextConfiguration
            {
                // let's use the string prefix defined in config.json
                StringPrefixes = new[] { configFile.CommandPrefix },

                // enable responding in direct messages
                EnableDms = true,

                // enable mentioning the bot as a command prefix
                EnableMentionPrefix = true
            };

            // and hook them up
            this.Commands = Client.UseCommandsNext(ccfg);

            // let's hook some command events, so we know what's 
            // going on
            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;

            // up next, let's register our commands
            this.Commands.RegisterCommands<VoiceCommand>();
            this.Commands.RegisterCommands<PlayAudioCommand>();
            this.Commands.RegisterCommands<WormsCommand>();
            this.Commands.RegisterCommands<VoiceRecognition>();
            this.Commands.RegisterCommands<FuckYouCommand>();
            this.Commands.RegisterCommands<CustomAudioCommand>();
            this.Commands.RegisterCommands<MadWorldCommand>();
            this.Commands.RegisterCommands<CustomIntroCommand>();
            this.Commands.RegisterCommands<EmoteCommands>();
            this.Commands.SetHelpFormatter<HelpFormatter>();
            //this.Client.TypingStarted += Client_TypingStarted;

            // let's set up voice
            var vcfg = new VoiceNextConfiguration
            {
                VoiceApplication = VoiceApplication.Music,
                EnableIncoming = false
            };

            // and let's enable it
            this.Voice = Client.UseVoiceNext(vcfg);

            // finally, let's connect and log in
            await Client.ConnectAsync();

            // for this example you will need to read the 
            // VoiceNext setup guide, and include ffmpeg.

            // and this is to prevent premature quitting
            await Task.Delay(-1);
        }

        private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            Logger.WriteLine(e.ToString());
        }

        private Task Client_PresenceUpdated(PresenceUpdateEventArgs e)
        {
            if (e.PresenceAfter.Activity.ActivityType == ActivityType.Playing && !string.IsNullOrWhiteSpace(e.PresenceAfter.Activity.Name))
            {
                //playing a game
            }
            else if (e.PresenceAfter.Activity.ActivityType == ActivityType.ListeningTo)
            {
                Client.DebugLogger.Info(string.Format("{0} is now listening to {1} by {2}", e.User.Username, e.PresenceAfter.Activity.RichPresence.Details, e.PresenceAfter.Activity.RichPresence.State));
                //listening to a song
            }
            else if (e.PresenceAfter.Status == UserStatus.Online && e.PresenceBefore.Status == UserStatus.Offline)
            {
                Client.DebugLogger.Info(string.Format("{0} is now online", e.User.Username));
            }
            else if (e.PresenceAfter.Status == UserStatus.Offline && e.PresenceBefore.Status == UserStatus.Online)
            {
                Client.DebugLogger.Info(string.Format("{0} is now offline", e.User.Username));
            }

            return Task.CompletedTask;
        }

        private Task Client_TypingStarted(TypingStartEventArgs e)
        {
            e.Channel.SendMessageAsync(":^)");

            return Task.CompletedTask;
        }

        private Task Client_Ready(ReadyEventArgs e)
        {
            // let's log the fact that this event occured
            e.Client.DebugLogger.Info("Client is ready to process events.");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Client.DebugLogger.Info($"Guild available: {e.Guild.Name}");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            e.Client.DebugLogger.Error($"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.DebugLogger.Info($"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.DebugLogger.Error($"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}");

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException ex)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync(string.Empty, embed: embed);
            }
        }
    }
}
