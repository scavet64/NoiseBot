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
    public class CustomAudioCommandFile
    {
        private static readonly string commandFilePath = "CustomAudioCommands.json";
        private static CustomAudioCommandFile instance;

        public static CustomAudioCommandFile Instance
        {
            get
            {
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
        /// Prevents a default instance of the <see cref="CustomAudioCommandFile"/> class from being created.
        /// </summary>
        private CustomAudioCommandFile()
        {
            customAudioCommands = new List<CustomAudioCommandModel>();
        }

        private static CustomAudioCommandFile LoadConfigFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                //create the file
                instance = new CustomAudioCommandFile();
                return instance;
            }

            CustomAudioCommandFile rVal;

            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    {
                        string json = sr.ReadToEnd();
                        rVal = JsonConvert.DeserializeObject<CustomAudioCommandFile>(json);
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
            foreach (CustomAudioCommandModel command in customAudioCommands)
            {
                if (commandName.Equals(command.CommandName, StringComparison.InvariantCultureIgnoreCase))
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
            SaveFile();
        }

        private void SaveFile()
        {
            using (StreamWriter file = File.CreateText(commandFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Instance);
            }

        }

        public List<string> GetAllCustomCommandNames()
        {
            List<string> commandNames = new List<string>();
            foreach(CustomAudioCommandModel command in customAudioCommands)
            {
                commandNames.Add(command.CommandName);
            }
            return commandNames;
        }

    }
}
