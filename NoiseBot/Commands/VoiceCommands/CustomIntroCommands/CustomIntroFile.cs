using Newtonsoft.Json;
using NoiseBot.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoiseBot.Commands.VoiceCommands.CustomIntroCommands
{
    public class CustomIntroFile
    {
        private static readonly string commandFilePath = "CustomIntros.json";
        private static CustomIntroFile instance;

        public static CustomIntroFile Instance
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

        [JsonProperty("CustomIntros")]
        private List<CustomIntroModel> customIntros;

        /// <summary>
        /// Prevents a default instance of the <see cref="CustomIntroFile"/> class from being created.
        /// </summary>
        private CustomIntroFile()
        {
            customIntros = new List<CustomIntroModel>();
        }

        private static CustomIntroFile LoadConfigFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                //create the file
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

        public void AddCustomIntro(string username, string customAudioFile)
        {
            AddCustomIntro(new CustomIntroModel(username, customAudioFile));
        }

        public void AddCustomIntro(CustomIntroModel introToAdd)
        {
            customIntros.Add(introToAdd);
            SaveFile();
        }

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

        private void SaveFile()
        {
            using (StreamWriter file = File.CreateText(commandFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Instance);
            }

        }

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
