using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using NoiseBot.Services;
using NoiseBot.Extensions;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseBot.Commands.RedditCommands
{
    [Group("Reddit")]
    public class RedditCommands : BaseCommandModule
    {
        [Command("add"), Description("Adds a reddit subscription")]
        public async Task AddRedditSubscription(CommandContext ctx, [Description("Subreddit to subscribe to (ex. /r/pics)")] string subredditUrl, [Description("Interval in minutes to check")] int intervalMin = 30)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subredditUrl))
                {
                    await ctx.RespondAsync("You need to enter a subreddit to subscribe too :^(");
                    return;
                }

                if (intervalMin <= 0 || intervalMin > 1000000)
                {
                    await ctx.RespondAsync("The interval timer must fall between 1 and 1000000 minutes :^(");
                    return;
                }

                if (RedditSubscriptionsFile.Instance.DoesSubExistForGuild(subredditUrl, ctx.Guild.Id))
                {
                    await ctx.RespondAsync("This Subreddit subscription already exists :^(");
                    return;
                }

                RedditService.AddNewSubscription(subredditUrl, ctx.Guild.Id, ctx.Channel.Id, ctx.User.Username, intervalMin);
                await ctx.RespondAsync("Successfully added subscription :^)");
            }
            catch (Exception ex)
            {
                Program.Client.DebugLogger.Error(ex.StackTrace);
            }
        }

        [Command("remove"), Description("removes a reddit subscription")]
        public async Task RemoveRedditSubscription(CommandContext ctx, [RemainingText, Description("Subreddit to remove")] string subredditUrl)
        {
            if (!RedditSubscriptionsFile.Instance.DoesSubExistForGuild(subredditUrl, ctx.Guild.Id))
            {
                await ctx.RespondAsync("This Subreddit subscription does not exists :^(");
                return;
            }

            if (RedditService.RemoveSubscription(subredditUrl, ctx.Guild.Id))
            {
                await ctx.RespondAsync("Removed subscription :^)");
            }
        }

        [Command("list"), Description("List of current reddit subscriptions")]
        public async Task ListCurrentSubscriptions(CommandContext ctx)
        {
            StringBuilder builder = new StringBuilder();

            if (RedditSubscriptionsFile.Instance.RedditSubscriptions.Count > 0)
            {
                builder.Append("```");

                foreach (RedditSubscriptionModel sub in RedditSubscriptionsFile.Instance.RedditSubscriptions)
                {
                    builder.Append("Subreddit: ").Append(sub.Subreddit).Append("\t\t").Append(" | requested by: ").AppendLine(sub.UserSubscribed);
                }

                builder.Append("```");
            }
            else
            {
                builder.Append("There are no subscriptions :^(");
            }

            await ctx.RespondAsync(builder.ToString());
        }

        [Command("interval"), Description("Update the interval for a subscription.")]
        public async Task UpdateIntervalTimerForSubscription(CommandContext ctx, [Description("Subscription to update")] string subredditUrl, [Description("New polling interval in minutes")] int intervalMin)
        {
            if (string.IsNullOrWhiteSpace(subredditUrl))
            {
                await ctx.RespondAsync("You need to enter a subreddit to update :^(");
                return;
            }

            if (intervalMin <= 0 || intervalMin > 1000000)
            {
                await ctx.RespondAsync("The polling interval must fall between 1 and 1000000 minutes :^(");
                return;
            }

            if (RedditService.UpdateInterval(subredditUrl, ctx.Guild.Id, intervalMin))
            {
                await ctx.RespondAsync("Subscription Interval was updated :^)");
                return;
            }
            else
            {
                await ctx.RespondAsync("Subscription interval was not updated. Are you sure it exists? :^(");
                return;
            }
        }

        //Inner class for nested groups
        [Group("notify")]
        public class NotificationCommands : BaseCommandModule
        {

            [Command("add"), Description("Adds a notification to the user when the keyword is found in the reddit post")]
            public async Task AddRedditNotification(CommandContext ctx, [RemainingText, Description("The keyword")] string keyword)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(keyword))
                    {
                        await ctx.RespondAsync("The keyword cannot be empty :^(");
                        return;
                    }

                    if (RedditSubscriptionsFile.Instance.AddPersonalNotification(
                        new PersonalRedditNotification(ctx.User.Mention, keyword)))
                    {
                        await ctx.RespondAsync("Successfully added notification :^)");
                    }
                    else
                    {
                        await ctx.RespondAsync("This Subreddit subscription already exists :^(");
                    }
                }
                catch (Exception ex)
                {
                    Program.Client.DebugLogger.Error(ex.StackTrace);
                }
            }

            [Command("remove"), Description("Adds a notification to the user when the keyword is found in the reddit post")]
            public async Task RemoveRedditNotification(CommandContext ctx, [RemainingText, Description("The keyword")] string keyword)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(keyword))
                    {
                        await ctx.RespondAsync("The keyword cannot be empty :^(");
                        return;
                    }

                    if (RedditSubscriptionsFile.Instance.RemovePersonalNotification(new PersonalRedditNotification(ctx.User.Mention, keyword)))
                    {
                        await ctx.RespondAsync("Successfully removed notification :^)");
                    }
                    else
                    {
                        await ctx.RespondAsync("This Subreddit subscription already exists :^(");
                    }
                }
                catch (Exception ex)
                {
                    Program.Client.DebugLogger.Error(ex.StackTrace);
                }
            }

            [Command("list"), Description("returns a list of all the notifications for this user")]
            public async Task ListRedditNotification(CommandContext ctx)
            {
                try
                {
                    List<PersonalRedditNotification> notificationsForUser = RedditSubscriptionsFile.Instance.GetRedditNotificationsForUser(ctx.User);
                    StringBuilder builder = new StringBuilder();

                    if (notificationsForUser.Count > 0)
                    {
                        builder.Append("```");

                        foreach (PersonalRedditNotification sub in notificationsForUser)
                        {
                            builder.Append("Keyword: ").AppendLine(sub.SubscribedKeyword);
                        }

                        builder.Append("```");
                    }
                    else
                    {
                        builder.Append("You dont have any notifications :^(");
                    }

                    await ctx.RespondAsync(builder.ToString());
                }
                catch (Exception ex)
                {
                    Program.Client.DebugLogger.Error(ex.StackTrace);
                }
            }
        }
    }
}
