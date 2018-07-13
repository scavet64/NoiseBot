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
    /// CustomAudioCommandFile. This class is a singleton and represents the CustomAudioCommands.Json file. Any operation on the list
    /// is managed by this class. 
    /// TODO: Make this an active object so there wont be multi threading issues
    /// </summary>
    public class CustomAudioCommandFile
    {
        private static readonly object LockObject = new object();
        private static readonly string CustomAudioJsonFile = "CustomAudioCommands.json";
        private static CustomAudioCommandFile instance;

        /// <summary>
        /// Gets or sets the singleton instance. Implements the doubly locking singleton pattern
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static CustomAudioCommandFile Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (LockObject)
                    {
                        if (instance == null)
                        {
                            instance = LoadConfigFromFile(CustomAudioJsonFile);
                        }
                    }
                }
                return instance;
            }
            set { instance = value; }
        }

        /// <summary>
        /// Loads the configuration from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Returns the <code>CustomAudioCommandFile</code> object loaded from the json file</returns>
        /// <exception cref="InvalidConfigException">
        /// Could not read config file: " + ioex.Message
        /// or
        /// Config json was incorrectly formatted: " + jex.Message
        /// </exception>
        private static CustomAudioCommandFile LoadConfigFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // create the file
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

        /// <summary>
        /// Gets the audio file for command.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <returns><code>CustomAudioCommandModel</code> object</returns>
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

        /// <summary>
        /// Deletes the custom command.
        /// </summary>
        /// <param name="custCom">The customer COM.</param>
        /// <returns>true if the operation was sucessful </returns>
        /// <exception cref="ArgumentException">
        /// Argument cannot be null
        /// or
        /// Invalid custom command object. Properties were null
        /// </exception>
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

        /// <summary>
        /// Adds the custom command.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <param name="customAudioFile">The custom audio file.</param>
        public void AddCustomCommand(string commandName, string customAudioFile)
        {
            AddCustomCommand(new CustomAudioCommandModel(commandName, customAudioFile));
        }

        /// <summary>
        /// Adds the custom command.
        /// </summary>
        /// <param name="commandToAdd">The command to add.</param>
        public void AddCustomCommand(CustomAudioCommandModel commandToAdd)
        {
            customAudioCommands.Add(commandToAdd);
        }

        /// <summary>
        /// Renames the custom command.
        /// </summary>
        /// <param name="oldCommandName">Old name of the command.</param>
        /// <param name="newCommandName">New name of the command.</param>
        /// <returns>true if successful</returns>
        /// <exception cref="ArgumentException">Could not find old command in the list</exception>
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
                    using (StreamWriter file = File.CreateText(CustomAudioJsonFile))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, Instance);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all custom command names.
        /// </summary>
        /// <returns>Sorted List of all the custom command names.</returns>
        public List<string> GetAllCustomCommandNames()
        {
            List<string> commandNames = new List<string>();
            foreach (CustomAudioCommandModel command in customAudioCommands)
            {
                commandNames.Add(command.CommandName);
            }
            commandNames.Sort();
            return commandNames;
        }
    }
}
