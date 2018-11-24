using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NoiseBot.Commands.RedditCommands;
using NoiseBot.Extensions;

namespace NoiseBot.Controllers
{
    public static class RedditController
    {
        private static readonly string redditPostFormat = "{0}\nPost from `{1}` \nReddit Link: https://reddit.com{2} \n{3}";
        private static Dictionary<RedditSubscriptionModel, bool> subscriptionToIsRunning = new Dictionary<RedditSubscriptionModel, bool>();
        private static readonly short maxNumberOfSubscriptions = 5; // Not sure if I want this yet

        public static RedditSubscriptionModel AddNewSubscription(string subreddit, ulong guildId, ulong channelId, string username, int intervalMin)
        {
            RedditSubscriptionModel model = new RedditSubscriptionModel(subreddit, guildId, channelId, username, intervalMin);
            RedditSubscriptionsFile.Instance.AddSubscription(model);

            // start the thread
            SpawnThreadForSubscription(model);

            return model;
        }

        public static bool RemoveSubscription(string subreddit, ulong guildId)
        {
            bool wasRemoved = false;
            RedditSubscriptionModel model = RedditSubscriptionsFile.Instance.GetSubscriptionByIdAndUrl(subreddit, guildId);
            if (model != null)
            {
                RedditSubscriptionsFile.Instance.RemoveSubscription(model);

                // stop the thread from running.
                subscriptionToIsRunning.Remove(model);

                wasRemoved = true;
            }
            return wasRemoved;
        }

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

        /// <summary>
        /// Starts the subscription threads thread so that it will not block the main thread when its sleeps
        /// </summary>
        public static void StartSubscriptionThreadsThread()
        {
            var t = new Thread(() => StartSubscriptionThreadsWork())
            {
                Name = "Start Subscription Threads Thread"
            };
            t.Start();
        }

        public static void StartSubscriptionThreadsWork()
        {
            // Basically make a copy of the list just in case its modified during the staggered rollout
            List<RedditSubscriptionModel> startingSubs = new List<RedditSubscriptionModel>();
            foreach (RedditSubscriptionModel model in RedditSubscriptionsFile.Instance.RedditSubscriptions)
            {
                startingSubs.Add(model);
            }

            foreach (RedditSubscriptionModel model in startingSubs)
            {
                SpawnThreadForSubscription(model);

                //This will spread the subscription spam out
                int secondsToSleep = new Random().Next(60);
                Program.Client.DebugLogger.Info(string.Format("Sleeping for {0} seconds", secondsToSleep));
                Thread.Sleep(new TimeSpan(0, 0, secondsToSleep));
            }
            Program.Client.DebugLogger.Info(string.Format("Finished spawning threads"));
        }

        public static Thread SpawnThreadForSubscription(RedditSubscriptionModel subscription)
        {
            var t = new Thread(() => SubscriptionThreadMethod(subscription))
            {
                Name = subscription.Subreddit + " - SubscriptionThread"
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
                Thread.Sleep(new TimeSpan(0, subscription.IntervalMin, 0));
            }
        }

        private static async Task PostFromRedditAsync(RedditSubscriptionModel subscription)
        {
            try
            {
                var reddit = new Reddit();
                Subreddit subreddit = reddit.GetSubreddit(subscription.Subreddit);
                Listing<Post> listings = subreddit.Hot;
                Program.Client.DebugLogger.Info("Checking Posts from subreddit: " + subscription.Subreddit);
                foreach (Post post in listings.GetListing(15))
                {

                    // See if one out of the ten posts are unique, if not, nothing will post
                    string urlToPost = post.Url.ToString();
                    if (!subscription.PostedLinks.Contains(urlToPost))
                    {
                        Program.Client.DebugLogger.Info("Posting URL: " + urlToPost);

                        // Get the guild and channel to then post the message
                        DiscordGuild guildToPostIn = await Program.Client.GetGuildAsync(subscription.DiscordGuildId);
                        await guildToPostIn.GetChannel(subscription.ChannelId).SendMessageAsync(string.Format(redditPostFormat, post.Title, subscription.Subreddit, post.Permalink.ToString(), urlToPost));

                        //Notify users if keyword is triggered
                        foreach (PersonalRedditNotification notification in RedditSubscriptionsFile.Instance.PersonalRedditSubscriptions)
                        {
                            if (post.Title.ToLower().Contains(notification.SubscribedKeyword.ToLower()))
                            {
                                await guildToPostIn.GetChannel(subscription.ChannelId).SendMessageAsync($"Keyword `{notification.SubscribedKeyword}` was triggered for {notification.UserMentionString}");
                            }
                        }

                        // add the post to the list so no repeats and save
                        subscription.PostedLinks.Add(urlToPost);
                        RedditSubscriptionsFile.Instance.SaveFile();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Client.DebugLogger.Critical(ex.StackTrace.ToString());
            }
        }
    }
}
