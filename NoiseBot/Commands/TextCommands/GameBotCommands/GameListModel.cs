using Newtonsoft.Json;
using System;

namespace NoiseBot.Commands.VoiceCommands.GameBotCommands
{
    public class GameListModel : IComparable
    {
        [JsonProperty("GameName")]
        public string Name { get; private set; }

        public GameListModel(string name)
        {
            Name = name;
        }

        public int CompareTo(object obj)
        {
            if (obj is GameListModel other)
            {
                return Name.CompareTo(other.Name);
            }
            return 0;
        }
    }
}