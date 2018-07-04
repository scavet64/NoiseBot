using Newtonsoft.Json;
using NoiseBot.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoiseBot.Commands.VoiceCommands.CustomVoiceCommands
{
    /// <summary>
    /// TODO: Make this an active object so there wont be multi threading issues
    /// </summary>
    public class CustomCommandFile
    {
        private static readonly string commandFilePath = "config.json";
        private static CustomCommandFile instance;

        public static CustomCommandFile Instance
        {
            get {
                if (instance == null)
                {
                    instance = LoadConfigFromFile(commandFilePath);
                }
                return instance;
            }
            set { instance = value; }
        }

        [JsonProperty("CustomAudioCommands")]
        private List<CustomAudioCommandModel> customAudioCommands;

        /// <summary>
        /// Prevents a default instance of the <see cref="CustomCommandFile"/> class from being created.
        /// </summary>
        private CustomCommandFile() { }

        private static CustomCommandFile LoadConfigFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                //create the file
            }

            CustomCommandFile rVal;

            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    {
                        string json = sr.ReadToEnd();
                        rVal = JsonConvert.DeserializeObject<CustomCommandFile>(json);
                    }
                }
            }
            catch (IOException ioex)
            {
                throw new InvalidConfigException("Could not read config file: " + ioex.Message);
            }
            catch (JsonException jex)
            {
                throw new InvalidConfigException("Config json was incorrectly formatted: " + jex.Message);
            }

            return rVal;
        }

        public CustomAudioCommandModel GetAudioFileForCommand(string commandName)
        {
            foreach(CustomAudioCommandModel command in customAudioCommands)
            {
                if(string.Equals(command.CommandName.Equals(command), StringComparison.CurrentCultureIgnoreCase))
                {
                    return command;
                }
            }

            // return null or maybe an exception?
            return null;
        }

        public void AddCustomCommand(string commandName, string customAudioFile)
        {
            AddCustomCommand(new CustomAudioCommandModel(commandName, customAudioFile));
        }

        public void AddCustomCommand(CustomAudioCommandModel commandToAdd)
        {
            customAudioCommands.Add(commandToAdd);
        }

    }
}
