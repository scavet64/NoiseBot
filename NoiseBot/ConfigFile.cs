using Newtonsoft.Json;
using NoiseBot.Services;
using NoiseBot.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoiseBot
{
    class ConfigFile
    {
        private static readonly string ConfigFilePath = "config.json";

        private static ConfigFile instance;

        public static ConfigFile Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = LoadConfigFromFile(ConfigFilePath);
                }
                return instance;
            }
            set { instance = value; }
        }


        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }

        [JsonProperty("enableIncoming")]
        public bool EnableIncoming { get; private set; } = false;

        private ConfigFile() { }

        private static ConfigFile LoadConfigFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new InvalidConfigException("Config file is missing");
            }

            ConfigFile rVal = SerializationService.DeserializeFile<ConfigFile>(ConfigFilePath);

            

            return rVal;
        }
    }
}