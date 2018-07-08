using DSharpPlus;
using System;

namespace NoiseBot.Extensions
{
    public static class LoggingExtensions
    {
        public static void LogMessage(this DebugLogger logger, LogLevel logLevel, string message)
        {
            logger.LogMessage(logLevel, "NoiseBot", message, DateTime.Now);
        }

        public static void Warn(this DebugLogger logger, string message)
        {
            logger.LogMessage(LogLevel.Warning, "NoiseBot", message, DateTime.Now);
        }

        public static void Info(this DebugLogger logger, string message)
        {
            logger.LogMessage(LogLevel.Info, "NoiseBot", message, DateTime.Now);
        }

        public static void Debug(this DebugLogger logger, string message)
        {
            logger.LogMessage(LogLevel.Debug, "NoiseBot", message, DateTime.Now);
        }

        public static void Critical(this DebugLogger logger, string message)
        {
            logger.LogMessage(LogLevel.Critical, "NoiseBot", message, DateTime.Now);
        }

        public static void Error(this DebugLogger logger, string message)
        {
            logger.LogMessage(LogLevel.Error, "NoiseBot", message, DateTime.Now);
        }
    }
}
