using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoiseBot.Commands.VoiceCommands.CustomVoiceCommands
{
    public class CustomAudioCommandModel
    {
        [JsonProperty("commandName")]
        public string CommandName { get; private set; }

        [JsonProperty("filepath")]
        public string Filepath { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAudioCommandModel"/> class.
        /// Empty for json serializer
        /// </summary>
        public CustomAudioCommandModel() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAudioCommandModel"/> class.
        /// </summary>
        /// <param name="commandName">Name of the command.</param>
        /// <param name="filepath">The filepath.</param>
        public CustomAudioCommandModel(string commandName, string filepath)
        {
            this.CommandName = commandName;
            this.Filepath = filepath;
        }
    }
}
