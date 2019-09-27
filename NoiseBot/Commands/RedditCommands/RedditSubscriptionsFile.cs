using Newtonsoft.Json;
using NoiseBot.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;

namespace NoiseBot.Commands.RedditCommands
{
    public class RedditSubscriptionsFile
    {
        private static readonly string filepath = "RedditSubscriptions.json";
        private static readonly object LockObject = new object();

        private static RedditSubscriptionsFile instance;


        [JsonProperty("RedditSubscriptions")]
        public ObservableCollection<RedditSubscriptionModel> RedditSubscriptions { get; private set; }

        [JsonProperty("PersonalSubscriptions")]
        public ObservableCollection<PersonalRedditNotification> PersonalRedditSubscriptions { get; private set; }

        public RedditSubscriptionsFile()
        {
            RedditSubscriptions = new ObservableCollection<RedditSubscriptionModel>();
            RedditSubscriptions.CollectionChanged += RedditSubscriptions_CollectionChanged;
            PersonalRedditSubscriptions = new ObservableCollection<PersonalRedditNotification>();
            PersonalRedditSubscriptions.CollectionChanged += RedditSubscriptions_CollectionChanged;
        }

        private void RedditSubscriptions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveFile();
        }

        public void SaveFile()
        {
            if (instance != null)
            {
                lock (LockObject)
                {
                    SerializationService.SerializeFile<RedditSubscriptionsFile>(instance, filepath);
                }
            }
        }

        /// <summary>
        /// Gets or sets the singleton instance. Implements the doubly locking singleton pattern
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static RedditSubscriptionsFile Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (LockObject)
                    {
                        if (instance == null)
                        {
                            instance = LoadConfigFromFile();
                        }
                    }
                }
                return instance;
            }
            set { instance = value; }
        }


        /// <summary>
        /// Loads the configuration from file.
        /// </summary>
        /// <returns>Returns the <code>CustomAudioCommandFile</code> object loaded from the json file</returns>
        /// <exception cref="InvalidConfigException">
        /// Could not read config file: " + ioex.Message
        /// or
        /// Config json was incorrectly formatted: " + jex.Message
        /// </exception>
        private static RedditSubscriptionsFile LoadConfigFromFile()
        {
            if (!File.Exists(filepath))
            {
                // create the file
                instance = new RedditSubscriptionsFile();
                return instance;
            }

            return SerializationService.DeserializeFile<RedditSubscriptionsFile>(filepath);
        }

        public bool AddSubscription(RedditSubscriptionModel model)
        {
            if (!DoesSubExistForGuild(model.Subreddit, model.DiscordGuildId))
            {
                RedditSubscriptions.Add(model);
                return true;
            }
            return false;
        }

        public bool DoesSubExistForGuild(string subreddit, ulong guildId)
        {
            return GetSubscriptionByIdAndUrl(subreddit, guildId) != null;
        }

        public void RemoveSubscription(string subredditUrl, ulong id)
        {
            RedditSubscriptionModel model = GetSubscriptionByIdAndUrl(subredditUrl, id);
            if (model != null)
            {
                RedditSubscriptions.Remove(model);
            }
        }

        public void RemoveSubscription(RedditSubscriptionModel model)
        {
            if (model != null)
            {
                RedditSubscriptions.Remove(model);
            }
        }

        public RedditSubscriptionModel GetSubscriptionByIdAndUrl(string subredditUrl, ulong id)
        {
            foreach (RedditSubscriptionModel sub in RedditSubscriptions)
            {
                if (sub.Subreddit.Equals(subredditUrl) && id == sub.DiscordGuildId)
                {
                    return sub;
                }
            }
            return null;
        }

        public bool AddPersonalNotification(PersonalRedditNotification notificationObject)
        {
            bool success = false;
            if (!PersonalRedditSubscriptions.Contains(notificationObject))
            {
                PersonalRedditSubscriptions.Add(notificationObject);
                success = true;
            }

            return success;
        }

        public bool RemovePersonalNotification(PersonalRedditNotification notificationObject)
        {
            bool success = false;
            if (PersonalRedditSubscriptions.Contains(notificationObject))
            {
                PersonalRedditSubscriptions.Remove(notificationObject);
                success = true;
            }

            return success;
        }

        public List<PersonalRedditNotification> GetRedditNotificationsForUser(DiscordUser user)
        {
            return PersonalRedditSubscriptions
                .Where(sub => sub.UserMentionString.Equals(user.Mention))
                .ToList();
        }
    }
}
