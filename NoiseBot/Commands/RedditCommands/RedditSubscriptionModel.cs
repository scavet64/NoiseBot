using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoiseBot.Commands.RedditCommands
{
    public class RedditSubscriptionModel
    {
        [JsonProperty("subreddit")]
        public string Subreddit { get; private set; }

        [JsonProperty("DiscordGuildId")]
        public ulong DiscordGuildId { get; set; }

        [JsonProperty("ChannelId")]
        public ulong ChannelId { get; set; }

        [JsonProperty("UserSubscribed")]
        public string UserSubscribed { get; private set; }

        [JsonProperty("PostedLinks")]
        public List<string> PostedLinks { get; private set; }

        public RedditSubscriptionModel() {
            this.PostedLinks = new List<string>();
        }

        public RedditSubscriptionModel(string subreddit, ulong DiscordGuildId, ulong channelId, string UserSubscribed)
        {
            this.Subreddit = subreddit;
            this.DiscordGuildId = DiscordGuildId;
            this.ChannelId = channelId;
            this.UserSubscribed = UserSubscribed;
            this.PostedLinks = new List<string>();
        }
    }
}
