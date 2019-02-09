using NoiseBot.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NoiseBot
{
    /// <summary>
    /// Simple logger class
    /// </summary>
    public class SimpleLogController
    {
        //private static readonly Settings Settings = Settings.Default;
        private static readonly object Lock = new object();
        private static SimpleLogController instance;

        private readonly string datetimeFormat;

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>
        /// The filename to set
        /// </value>
        public string Filename { get; set; }

        /// <summary>
        /// Gets the simple logger instance
        /// </summary>
        /// <returns>singleton instance of the simple logger</returns>
        public static SimpleLogController GetSimpleLogger()
        {
            if (instance == null)
            {
                lock (Lock)
                {
                    if (instance == null)
                    {
                        instance = new SimpleLogController(true);
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        /// <param name="text">Message to write</param>
        public void Debug(string text)
        {
            WriteFormattedLog(LogLevel.DEBUG, text);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="text">Message to write</param>
        public void Error(string text)
        {
            WriteFormattedLog(LogLevel.ERROR, text);
        }

        /// <summary>
        /// Log a fatal error message
        /// </summary>
        /// <param name="text">Message to write</param>
        public void Fatal(string text)
        {
            WriteFormattedLog(LogLevel.FATAL, text);
        }

        /// <summary>
        /// Log an info message
        /// </summary>
        /// <param name="text">Message to write</param>
        public void Info(string text)
        {
            WriteFormattedLog(LogLevel.INFO, text);
        }

        /// <summary>
        /// Log a trace message
        /// </summary>
        /// <param name="text">Message to write</param>
        public void Trace(string text)
        {
            WriteFormattedLog(LogLevel.TRACE, text);
        }

        /// <summary>
        /// Log a waning message
        /// </summary>
        /// <param name="text">Message to write</param>
        public void Warning(string text)
        {
            WriteFormattedLog(LogLevel.WARNING, text);
        }

        /// <summary>
        /// Initialize a new instance of SimpleLogger class.
        /// Log file will be created automatically if not yet exists, else it can be either a fresh new file or append to the existing file.
        /// Default is create a fresh new log file.
        /// </summary>
        /// <param name="append">True to append to existing log file, False to overwrite and create new log file</param>
        private SimpleLogController(bool append = false)
        {
            datetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
            Filename = GetExecutingDirectory() + Assembly.GetExecutingAssembly().GetName().Name + "_" + GetCurrentDateString() + ".log";

            // Log file header line
            string logHeader = Filename + " is created.";
            if (!File.Exists(Filename))
            {
                WriteLine(DateTime.Now.ToString(datetimeFormat) + " " + logHeader, false);
            }
            else
            {
                if (append == false)
                {
                    WriteLine(DateTime.Now.ToString(datetimeFormat) + " " + logHeader, false);
                }
            }
        }

        private static string GetExecutingDirectory()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return new FileInfo(location).Directory.FullName + @"\";
        }

        /// <summary>
        /// Gets the current date string.
        /// </summary>
        /// <returns>Current date string</returns>
        private string GetCurrentDateString()
        {
            return DateTime.Now.ToShortDateString().Replace("/", "_");
        }

        /// <summary>
        /// Format a log message based on log level
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="text">Log message</param>
        private void WriteFormattedLog(LogLevel level, string text)
        {
            //if ((int)level < Settings.MinimumLogLevel)
            //{
            //    return;
            //}

            string pretext;
            switch (level)
            {
                case LogLevel.TRACE:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [TRACE]   ";
                    break;
                case LogLevel.INFO:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [INFO]    ";
                    break;
                case LogLevel.DEBUG:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [DEBUG]   ";
                    break;
                case LogLevel.WARNING:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [WARNING] ";
                    break;
                case LogLevel.ERROR:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [ERROR]   ";
                    break;
                case LogLevel.FATAL:
                    pretext = DateTime.Now.ToString(datetimeFormat) + " [FATAL]   ";
                    break;
                default:
                    pretext = string.Empty;
                    break;
            }

            WriteLine(pretext + text);
        }

        /// <summary>
        /// Write a line of formatted log message into a log file. If a failure happens when logging, this method silently fails
        /// </summary>
        /// <param name="text">Formatted log message</param>
        /// <param name="append">True to append, False to overwrite the file</param>
        public void WriteLine(string text, bool append = true)
        {
            lock (Lock)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(Filename, append, Encoding.UTF8))
                    {
                        if (text != string.Empty)
                        {
                            writer.WriteLine(text);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program.Client.DebugLogger.Critical($"An exception occured logging: {ex}");
                    //MessageBox.Show("There was an error saving the log file: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Supported log level
        /// </summary>
        [Serializable]
        private enum LogLevel
        {
            TRACE = 0,
            DEBUG = 1,
            INFO = 2,
            WARNING = 3,
            ERROR = 4,
            FATAL = 5
        }
    }
}
