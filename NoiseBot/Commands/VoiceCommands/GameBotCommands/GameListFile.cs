using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NoiseBot.Commands.VoiceCommands.CustomIntroCommands;
using NoiseBot.Exceptions;

namespace NoiseBot.Commands.VoiceCommands.GameBotCommands
{
    /// <summary>
    /// Custom intro file. This class is a singleton and represents the CustomIntros.Json file. Any operation on the list
    /// is managed by this class.
    /// TODO: Make this an active object so there wont be multi threading issues
    /// </summary>
    public class GameListFile
    {
        private static readonly string CommandFilePath = "GameList.json";
        private static GameListFile instance;

        /// <summary>
        /// Gets or sets the singleton instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static GameListFile Instance
        {
            get => instance ?? (instance = LoadConfigFromFile(CommandFilePath));
            set => instance = value;
        }

        [JsonProperty("CustomIntros")]
        private List<GameListModel> gameList;

        /// <summary>
        /// Prevents a default instance of the <see cref="GameListFile"/> class from being created.
        /// </summary>
        private GameListFile()
        {
            gameList = new List<GameListModel>();
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
        private static GameListFile LoadConfigFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // create the file
                instance = new GameListFile();
                instance.SaveFile();
                return instance;
            }

            GameListFile rVal;

            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    {
                        string json = sr.ReadToEnd();
                        rVal = JsonConvert.DeserializeObject<GameListFile>(json);
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

        public void AddGame(string game)
        {
            AddGame(new GameListModel(game));
        }

        public void AddGame(GameListModel game)
        {
            gameList.Add(game);
            SaveFile();
        }

        public void RemoveGame(string game)
        {
            gameList.RemoveAll(g => g.Name == game);
            SaveFile();
        }

        private void SaveFile()
        {
            using (StreamWriter file = File.CreateText(CommandFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Instance);
            }
        }

        public List<string> GetAllGames()
        {
            List<string> gameList = new List<string>();
            foreach (var game in this.gameList)
            {
                gameList.Add(game.Name);
            }
            return gameList;
        }
    }
}
