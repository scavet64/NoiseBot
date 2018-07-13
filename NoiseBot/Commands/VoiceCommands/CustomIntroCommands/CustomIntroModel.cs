using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoiseBot.Commands.VoiceCommands.CustomIntroCommands
{
    /// <summary>
    /// Model class for the json seralization and deserailization of custom intros
    /// </summary>
    public class CustomIntroModel
    {
        /// <summary>
        /// Gets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [JsonProperty("username")]
        public string Username { get; private set; }

        /// <summary>
        /// Gets or sets the filepath.
        /// </summary>
        /// <value>
        /// The filepath.
        /// </value>
        [JsonProperty("filepath")]
        public string Filepath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAudioCommandModel"/> class.
        /// Empty for json serializer
        /// </summary>
        public CustomIntroModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAudioCommandModel"/> class.
        /// </summary>
        /// <param name="username">username of the person who this intro belongs to.</param>
        /// <param name="filepath">The filepath.</param>
        public CustomIntroModel(string username, string filepath)
        {
            this.Username = username;
            this.Filepath = filepath;
        }
    }
}
