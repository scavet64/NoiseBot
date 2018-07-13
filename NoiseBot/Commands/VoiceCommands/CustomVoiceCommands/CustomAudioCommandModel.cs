using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoiseBot.Commands.VoiceCommands.CustomVoiceCommands
{
    /// <summary>
    /// Model class for the json seralization and deserailization of custom audio commands
    /// </summary>
    public class CustomAudioCommandModel
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        [JsonProperty("commandName")]
        public string CommandName { get; private set; }

        /// <summary>
        /// Gets the filepath.
        /// </summary>
        /// <value>
        /// The filepath.
        /// </value>
        [JsonProperty("filepath")]
        public string Filepath { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAudioCommandModel"/> class.
        /// Empty for json serializer
        /// </summary>
        public CustomAudioCommandModel() { }

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

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            bool equal = false;

            if (obj != null && obj is CustomAudioCommandModel other)
            {
                equal = this.CommandName.Equals(other.CommandName) && this.Filepath.Equals(other.Filepath);
            }

            return equal;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int hash = 13;
            if (CommandName != null)
            {
                hash += CommandName.GetHashCode();
            }
            if (Filepath != null)
            {
                hash += Filepath.GetHashCode();
            }
            return hash;
        }
    }
}
