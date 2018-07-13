using Newtonsoft.Json;
using NoiseBot.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoiseBot.Commands.VoiceCommands.CustomIntroCommands
{
    /// <summary>
    /// Custom intro file. This class is a singleton and represents the CustomIntros.Json file. Any operation on the list
    /// is managed by this class.
    /// TODO: Make this an active object so there wont be multi threading issues
    /// </summary>
    public class CustomIntroFile
    {
        private static readonly string CommandFilePath = "CustomIntros.json";
        private static CustomIntroFile instance;

        /// <summary>
        /// Gets or sets the singleton instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static CustomIntroFile Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = LoadConfigFromFile(CommandFilePath);
                }
                return instance;
            }
            set { instance = value; }
        }

        [JsonProperty("CustomIntros")]
        private List<CustomIntroModel> customIntros;

        /// <summary>
        /// Prevents a default instance of the <see cref="CustomIntroFile"/> class from being created.
        /// </summary>
        private CustomIntroFile()
        {
            customIntros = new List<CustomIntroModel>();
        }

        /// <summary>
        /// Loads the configuration from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Custom intro object loaded from the json file</returns>
        /// <exception cref="InvalidConfigException">
        /// Could not read config file: " + ioex.Message
        /// or
        /// Config json was incorrectly formatted: " + jex.Message
        /// </exception>
        private static CustomIntroFile LoadConfigFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // create the file
                instance = new CustomIntroFile();
                instance.SaveFile();
                return instance;
            }

            CustomIntroFile rVal;

            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    {
                        string json = sr.ReadToEnd();
                        rVal = JsonConvert.DeserializeObject<CustomIntroFile>(json);
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

        /// <summary>
        /// Gets the intro for username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>Intro model for the passed in username</returns>
        public CustomIntroModel GetIntroForUsername(string username)
        {
            foreach (CustomIntroModel introModel in customIntros)
            {
                if (username.Equals(introModel.Username, StringComparison.InvariantCultureIgnoreCase))
                {
                    return introModel;
                }
            }

            // return null or maybe an exception?
            return null;
        }

        /// <summary>
        /// Adds the custom intro object to the list.
        /// </summary>
        /// <param name="username">The username that the file should play for.</param>
        /// <param name="customAudioFile">The custom audio file to play.</param>
        public void AddCustomIntro(string username, string customAudioFile)
        {
            AddCustomIntro(new CustomIntroModel(username, customAudioFile));
        }

        /// <summary>
        /// Adds the custom intro object to the list.
        /// </summary>
        /// <param name="introToAdd">The CustomIntroModel object to add.</param>
        public void AddCustomIntro(CustomIntroModel introToAdd)
        {
            customIntros.Add(introToAdd);
            SaveFile();
        }

        /// <summary>
        /// Changes the intro for the specified user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="filepath">The filepath.</param>
        /// <returns>Success boolean</returns>
        public bool ChangeIntro(string username, string filepath)
        {
            CustomIntroModel model = GetIntroForUsername(username);
            if (model == null)
            {
                return false;
            }
            else
            {
                model.Filepath = filepath;
                SaveFile();
                return true;
            }
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        private void SaveFile()
        {
            using (StreamWriter file = File.CreateText(CommandFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Instance);
            }
        }

        /// <summary>
        /// Gets all custom intro usernames.
        /// </summary>
        /// <returns>List of all the users who have a custom intro</returns>
        public List<string> GetAllCustomIntroUsernames()
        {
            List<string> customIntroNames = new List<string>();
            foreach (CustomIntroModel command in customIntros)
            {
                customIntroNames.Add(command.Username);
            }
            return customIntroNames;
        }
    }
}
