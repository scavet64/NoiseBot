using DSharpPlus;
using System;

namespace NoiseBot.Extensions
{
    /// <summary>
    /// Extension for the default DSharp logger
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The message.</param>
        public static void LogMessage(this DebugLogger logger, LogLevel logLevel, string message)
        {
            logger.LogMessage(logLevel, "NoiseBot", message, DateTime.Now);
        }

        /// <summary>
        /// Logs the message with a Warn level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        public static void Warn(this DebugLogger logger, string message)
        {
            logger.LogMessage(LogLevel.Warning, "NoiseBot", message, DateTime.Now);
        }

        /// <summary>
        /// Logs the message with a Info level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        public static void Info(this DebugLogger logger, string message)
        {
            logger.LogMessage(LogLevel.Info, "NoiseBot", message, DateTime.Now);
        }

        /// <summary>
        /// Logs the message with a Debug level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        public static void Debug(this DebugLogger logger, string message)
        {
            logger.LogMessage(LogLevel.Debug, "NoiseBot", message, DateTime.Now);
        }

        /// <summary>
        /// Logs the message with a Critical level.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        public static void Critical(this DebugLogger logger, string message)
        {
            logger.LogMessage(LogLevel.Critical, "NoiseBot", message, DateTime.Now);
        }

        /// <summary>
        /// Logs the message with an Error level..
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        public static void Error(this DebugLogger logger, string message)
        {
            logger.LogMessage(LogLevel.Error, "NoiseBot", message, DateTime.Now);
        }
    }
}
