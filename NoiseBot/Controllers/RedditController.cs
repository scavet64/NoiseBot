using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NoiseBot.Commands.RedditCommands;

namespace NoiseBot.Controllers
{
    public static class RedditController
    {
        private static Dictionary<RedditSubscriptionModel, bool> subscriptionToIsRunning = new Dictionary<RedditSubscriptionModel, bool>();

        public static RedditSubscriptionModel AddNewSubscription(string subreddit, ulong guildId, ulong channelId, string username)
        {
            RedditSubscriptionModel model = new RedditSubscriptionModel(subreddit, guildId, channelId, username);
            RedditSubscriptionsFile.Instance.AddSubscription(model);

            // start the thread
            SpawnThreadForSubscription(model);

            return model;
        }

        public static bool RemoveSubscription(string subreddit, ulong guildId)
        {
            RedditSubscriptionModel model = RedditSubscriptionsFile.Instance.GetSubscriptionByIdAndUrl(subreddit, guildId);
            if (model != null)
            {
                RedditSubscriptionsFile.Instance.RemoveSubscription(model);

                // stop the thread from running.
                subscriptionToIsRunning[model] = false;

                return true;
            }
            else
        public static bool UpdateInterval(string subreddit, ulong guildId, int newPollingInterval)
        {
            bool wasUpdated = false;
            RedditSubscriptionModel model = RedditSubscriptionsFile.Instance.GetSubscriptionByIdAndUrl(subreddit, guildId);
            if (model != null)
            {
                model.IntervalMin = newPollingInterval;
                RedditSubscriptionsFile.Instance.SaveFile();
                wasUpdated = true;
            }
            return wasUpdated;
        }

        private static void StartSubscriptionThreads()
        {
            foreach(RedditSubscriptionModel model in RedditSubscriptionsFile.Instance.RedditSubscriptions)
            {
                SpawnThreadForSubscription(model);
            }
        }

        public static Thread SpawnThreadForSubscription(RedditSubscriptionModel subscription)
        {
            var t = new Thread(() => SubscriptionThreadMethod(subscription))
            {
                Name = "SubscriptionThread - " + subscription.Subreddit
            };
            t.Start();
            subscriptionToIsRunning.Add(subscription, true);
            return t;
        }

        private static void SubscriptionThreadMethod(RedditSubscriptionModel subscription)
        {
            // Get the boolean that maps to this subscription. If not found false is returned by default and the loop will end
            while (subscriptionToIsRunning.GetValueOrDefault(subscription, false))
            {
                PostFromRedditAsync(subscription).Wait();
                Thread.Sleep(new TimeSpan(0, 0, 30));
            }
        }

        private static async Task PostFromRedditAsync(RedditSubscriptionModel subscription)
        {
            var reddit = new Reddit();
            Subreddit subreddit = reddit.GetSubreddit(subscription.Subreddit);
            Listing<Post> listings = subreddit.GetTop(FromTime.Hour);
            foreach (Post post in listings.GetListing(10))
            {
                // See if one out of the ten posts are unique, if not, nothing will post
                string urlToPost = post.Url.ToString();
                if (!subscription.PostedLinks.Contains(urlToPost)) {
                    DiscordGuild guildToPostIn = await Program.Client.GetGuildAsync(subscription.DiscordGuildId);
                    await guildToPostIn.GetChannel(subscription.ChannelId).SendMessageAsync(urlToPost);
                    subscription.PostedLinks.Add(urlToPost);
                    break;
                }
            }
        }
    }
}
