using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;

namespace NoiseBot.Commands.RedditCommands
{
    public class PersonalRedditNotification
    {
        [JsonProperty("UserMentionString")]
        public string UserMentionString { get; private set; }

        [JsonProperty("SubbedKeyword")]
        public string SubscribedKeyword { get; private set; }

        public PersonalRedditNotification(string userMentionString, string subscribedKeyword)
        {
            UserMentionString = userMentionString ?? throw new ArgumentNullException(nameof(userMentionString));
            SubscribedKeyword = subscribedKeyword ?? throw new ArgumentNullException(nameof(subscribedKeyword));
        }

        public override bool Equals(object obj)
        {
            var subscription = obj as PersonalRedditNotification;
            return subscription != null &&
                   UserMentionString == subscription.UserMentionString &&
                   SubscribedKeyword == subscription.SubscribedKeyword;
        }

        public override int GetHashCode()
        {
            var hashCode = -474107959;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserMentionString);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SubscribedKeyword);
            return hashCode;
        }
    }
}
