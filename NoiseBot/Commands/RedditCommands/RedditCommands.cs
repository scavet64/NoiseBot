using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using NoiseBot.Controllers;
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

                RedditController.AddNewSubscription(subredditUrl, ctx.Guild.Id, ctx.Channel.Id, ctx.User.Username, intervalMin);
                await ctx.RespondAsync("Successfully added subscription :^)");
            } catch (Exception ex)
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

            RedditSubscriptionsFile.Instance.RemoveSubscription(subredditUrl, ctx.Guild.Id);
        }
    }
}
