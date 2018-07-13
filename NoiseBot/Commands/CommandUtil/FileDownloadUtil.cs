using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using NoiseBot.Commands.VoiceCommands.CustomVoiceCommands;
using NoiseBot.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NoiseBot.Commands.CommandUtil
{
    /// <summary>
    /// File Download Utility. Responsible for downloading files from discord
    /// </summary>
    public static class FileDownloadUtil
    {
        /// <summary>
        /// Downloads the file from discord message asynchronously.
        /// </summary>
        /// <param name="context">The context where the file was attached.</param>
        /// <returns>Completed Task of type boolean. True if the process was a success</returns>
        public static async Task<bool> DownloadFileFromDiscordMessageAsync(CommandContext context)
        {
            if (context.Message.Attachments.Count <= 0)
            {
                context.Client.DebugLogger.Warn(string.Format("{0} forgot to upload a file with the custom command", context.User.Username));
                await context.RespondAsync("No file was attached to the message :^(");
                return false;
            }

            DiscordAttachment attachment = context.Message.Attachments[0];
            string customAudioPath = string.Format(@"AudioFiles\{0}", attachment.FileName);

            if (File.Exists(customAudioPath))
            {
                context.Client.DebugLogger.Warn(string.Format("{0} uploaded a file thats name already existed", context.User.Username));
                await context.RespondAsync("filename already exists :^(");
                return false;
            }

            using (var client = new WebClient())
            {
                client.DownloadFile(attachment.Url, customAudioPath);
            }

            context.Client.DebugLogger.Info("A file was successfully uploaded");
            return true;
        } 
    }
}
