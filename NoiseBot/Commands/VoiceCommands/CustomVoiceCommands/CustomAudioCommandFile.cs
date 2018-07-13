using Newtonsoft.Json;
using NoiseBot.Exceptions;
using NoiseBot.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace NoiseBot.Commands.VoiceCommands.CustomVoiceCommands
{
    /// <summary>
    /// TODO: Make this an active object so there wont be multi threading issues
    /// </summary>
    public class CustomAudioCommandFile
    {
        private static readonly object lockObject = new object();
        private static readonly string commandFilePath = "CustomAudioCommands.json";
        private static CustomAudioCommandFile instance;

        public static CustomAudioCommandFile Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = LoadConfigFromFile(commandFilePath);
                        }
                    }
                }
                return instance;
            }
            set { instance = value; }
        }

        [JsonProperty("CustomAudioCommands")]
        private ObservableCollection<CustomAudioCommandModel> customAudioCommands;

        /// <summary>
        /// Prevents a default instance of the <see cref="CustomAudioCommandFile"/> class from being created.
        /// </summary>
        private CustomAudioCommandFile()
        {
            customAudioCommands = new ObservableCollection<CustomAudioCommandModel>();
            customAudioCommands.CollectionChanged += CustomAudioCommands_CollectionChanged;
        }

        private void CustomAudioCommands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveFile();
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

        public bool DeleteCustomCommand(CustomAudioCommandModel custCom)
        {
            if (custCom == null)
            {
                throw new ArgumentException("Argument cannot be null");
            }
            else if (custCom.CommandName == null || custCom.Filepath == null)
            {
                throw new ArgumentException("Invalid custom command object. Properties were null");
            }

            bool success;

            try
            {
                FileInfo fileInfo = new FileInfo(custCom.Filepath);
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }

                success = customAudioCommands.Remove(custCom);
            }
            catch (SystemException sysEx)
            {
                Program.Client.DebugLogger.Error("Could not delete file: " + sysEx.Message);
                success = false;
            }

            return success;
        }

        public void AddCustomCommand(string commandName, string customAudioFile)
        {
            AddCustomCommand(new CustomAudioCommandModel(commandName, customAudioFile));
        }

        public void AddCustomCommand(CustomAudioCommandModel commandToAdd)
        {
            customAudioCommands.Add(commandToAdd);
        }

        public bool RenameCustomCommand(string oldCommandName, string newCommandName)
        {
            CustomAudioCommandModel custCom = GetAudioFileForCommand(oldCommandName);
            if (custCom == null)
            {
                throw new ArgumentException("Could not find old command in the list");
            }

            customAudioCommands.Remove(custCom);
            customAudioCommands.Add(new CustomAudioCommandModel(newCommandName, custCom.Filepath));

            return true;
        }

        private void SaveFile()
        {
            if (instance != null)
            {
                lock (instance)
                {
                    using (StreamWriter file = File.CreateText(commandFilePath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, Instance);
                    }
                }
            }
        }

        public List<string> GetAllCustomCommandNames()
        {
            List<string> commandNames = new List<string>();
            foreach (CustomAudioCommandModel command in customAudioCommands)
            {
                commandNames.Add(command.CommandName);
            }
            return commandNames;
        }

    }
}
