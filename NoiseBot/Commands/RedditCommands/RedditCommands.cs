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
        public async Task AddRedditSubscription(CommandContext ctx, [RemainingText, Description("Subreddit to subscribe to (ex. /r/pics)")] string subredditUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subredditUrl))
                {
                    await ctx.RespondAsync("You need to enter a subreddit to subscribe too :^(");
                    return;
                }

                if (RedditSubscriptionsFile.Instance.DoesSubExistForGuild(subredditUrl, ctx.Guild.Id))
                {
                    await ctx.RespondAsync("This Subreddit subscription already exists :^(");
                    return;
                }

                RedditController.AddNewSubscription(subredditUrl, ctx.Guild.Id, ctx.Channel.Id, ctx.User.Username);
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
