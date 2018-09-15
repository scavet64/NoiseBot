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

        private ulong discordGuildId;

        [JsonIgnore]
        public ulong DiscordGuildId
        {
            get
            {
                return discordGuildId;
            }
            set
            {
                discordGuildId = value;
            }
        }

        [JsonProperty("DiscordGuildId")]
        public string DiscordGuildIdString
        {
            get
            {
                return discordGuildId.ToString();
            }
            set
            {
                discordGuildId = ulong.Parse(value);
            }
        }

        private ulong channelId;

        [JsonIgnore]
        public ulong ChannelId
        {
            get
            {
                return channelId;
            }
            private set
            {
                channelId = value;
            }
        }

        [JsonProperty("ChannelId")]
        public string ChannelIdString
        {
            get
            {
                return channelId.ToString();
            }
            set
            {
                ChannelId = ulong.Parse(value);
            }
        }

        [JsonProperty("UserSubscribed")]
        public string UserSubscribed { get; private set; }

        [JsonProperty("PostedLinks")]
        public List<string> PostedLinks { get; private set; }

        public RedditSubscriptionModel() { }

        public RedditSubscriptionModel(string subreddit, ulong DiscordGuildId, ulong channelId, string UserSubscribed)
        {
            this.Subreddit = subreddit;
            this.DiscordGuildId = DiscordGuildId;
            this.ChannelId = channelId;
            this.UserSubscribed = UserSubscribed;
            this.PostedLinks = new List<string>();
            this.PostedLinks.Add("heh");
        }
    }
}
