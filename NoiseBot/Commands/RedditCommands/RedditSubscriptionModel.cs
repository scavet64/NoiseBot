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
        public ulong DiscordGuildId { get; private set; }

        [JsonProperty("ChannelId")]
        public ulong ChannelId { get; private set; }

        [JsonProperty("UserSubscribed")]
        public string UserSubscribed { get; private set; }

        [JsonProperty("IntervalMin")]
        public int IntervalMin { get; set; }

        [JsonProperty("PostedLinks")]
        public List<string> PostedLinks { get; private set; }

        public RedditSubscriptionModel() {
            this.PostedLinks = new List<string>();
            this.IntervalMin = 15;
        }

        public RedditSubscriptionModel(string subreddit, ulong DiscordGuildId, ulong channelId, string UserSubscribed)
        {
            this.Subreddit = subreddit;
            this.DiscordGuildId = DiscordGuildId;
            this.ChannelId = channelId;
            this.UserSubscribed = UserSubscribed;
            this.PostedLinks = new List<string>();
            this.IntervalMin = 15;
        }

        public RedditSubscriptionModel(string subreddit, ulong DiscordGuildId, ulong channelId, string UserSubscribed, int IntervalMin)
        {
            this.Subreddit = subreddit;
            this.DiscordGuildId = DiscordGuildId;
            this.ChannelId = channelId;
            this.UserSubscribed = UserSubscribed;
            this.PostedLinks = new List<string>();
            this.IntervalMin = IntervalMin;
        }
    }
}
