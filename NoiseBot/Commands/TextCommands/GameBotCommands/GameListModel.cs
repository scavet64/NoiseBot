using Newtonsoft.Json;

namespace NoiseBot.Commands.VoiceCommands.GameBotCommands
{
    public class GameListModel
    {
        public GameListModel(string name)
        {
            Name = name;
        }

        [JsonProperty("GameName")]
        public string Name { get; private set; }
    }
}