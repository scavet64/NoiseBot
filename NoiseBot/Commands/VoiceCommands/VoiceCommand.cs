﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using NoiseBot.Extensions;
using System;
using System.Threading.Tasks;

namespace NoiseBot.Commands.VoiceCommands
{
    /// <summary>
    /// Base class for any voice commands. Holds basic functionality for anything relating to a voice command such as joining
    /// </summary>
    public class VoiceCommand : BaseCommandModule
    {
        /// <summary>
        /// Joins the voice channel
        /// </summary>
        /// <param name="context">The command context</param>
        /// <param name="chn">Channel to join if provided</param>
        /// <returns>Task complete</returns>
        public static async Task JoinVoice(CommandContext context, DiscordChannel chn = null)
        {
            // check whether we aren't already connected
            if (await IsClientConnected(context))
            {
                await context.RespondAsync("Already connected in this guild.");
                return;
            }

            // get member's voice state
            var vstat = context.Member?.VoiceState;
            if (vstat?.Channel == null && chn == null)
            {
                // they did not specify a channel and are not in one
                await context.RespondAsync("You are not in a voice channel.");
                return;
            }

            // channel not specified, use user's
            if (chn == null)
            {
                chn = vstat.Channel;
            }

            // connect
            try
            {
                var vnc = await context.Client.GetVoiceNext().ConnectAsync(chn);
                context.Client.DebugLogger.Info($"Connected to `{chn.Name}`");
            }
            catch (DllNotFoundException)
            {
                context.Client.DebugLogger.LogMessage(LogLevel.Critical, "NoiseBot", "Missing Required DLL for Voice Chat", DateTime.Now);
                await context.RespondAsync("Missing dependant files. Contact your bot manager");
            }
        }

        /// <summary>
        /// Determines whether the client is connected to a voice channel.
        /// </summary>
        /// <remarks>
        /// Gets the current connection from the guild that the command came from. If there is a connection, we are already connected
        /// </remarks>
        /// <param name="context">The context.</param>
        /// <returns>true if connected</returns>
        public static async Task<bool> IsClientConnected(CommandContext context)
        {
            var vnext = context.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await context.RespondAsync("VNext is not enabled or configured.");
                return false;
            }

            var vnc = vnext.GetConnection(context.Guild);
            if (vnc != null)
            {
                // already connected since connection is not null
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Joins if not already connectd
        /// </summary>
        /// <param name="context">Command context</param>
        /// <returns>Complete Task</returns>
        public static async Task JoinIfNotConnected(CommandContext context)
        {
            // Check connection
            if (!await IsClientConnected(context))
            {
                // not connected, so join
                await JoinVoice(context);
            }
        }

        /// <summary>
        /// Joins a voice channel
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="chn">Channel to join</param>
        /// <returns>Completed Task</returns>
        [Command("join"), Description("Joins a voice channel.")]
        public async Task Join(CommandContext context, DiscordChannel chn = null)
        {
            await JoinVoice(context, chn);
        }

        /// <summary>
        /// Leave the voice channel
        /// </summary>
        /// <param name="context">Command context</param>
        /// <returns>Task Complete</returns>
        [Command("leave"), Description("Leaves a voice channel.")]
        public async Task Leave(CommandContext context)
        {
            // check whether VNext is enabled
            var vnext = context.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await context.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we are connected
            var vnc = vnext.GetConnection(context.Guild);
            if (vnc == null)
            {
                // not connected
                await context.RespondAsync("Not connected in this guild.");
                return;
            }

            // disconnect
            vnc.Disconnect();
            await context.RespondAsync("Disconnected");
        }
    }
}
