using Newtonsoft.Json;
using NoiseBot.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoiseBot
{
    class ConfigFile
    {
        private static readonly string configFilePath = "config.json";
        private static ConfigFile configFile;

        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }

        private ConfigFile() { }

        public static ConfigFile GetConfig()
        {
            if (configFile == null)
            {
                configFile = LoadConfigFromFile(configFilePath);
            }
            return configFile;
        }

        private static ConfigFile LoadConfigFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new InvalidConfigException("Config file is missing");
            }

            ConfigFile rVal;

            try
            {
                using (var fs = File.OpenRead("config.json"))
                {
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    {
                        string json = sr.ReadToEnd();
                        rVal = JsonConvert.DeserializeObject<ConfigFile>(json);
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
    }
}
