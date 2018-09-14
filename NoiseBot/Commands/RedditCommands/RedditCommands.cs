using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseBot.Commands.RedditCommands
{
    public class RedditCommands : BaseCommandModule
    {
        [Command("RedditGo"), Description("Plays a custom audio command.")]
        public async Task RedditGo(CommandContext ctx, [RemainingText, Description("Name of the command")] string customCommandName)
        {
            var t = new Thread(() => IntroMethodThread(ctx));
            t.Start();
        }

        private void IntroMethodThread(CommandContext ctx)
        {
            while (true)
            {
                var reddit = new Reddit();
                Subreddit subreddit = reddit.GetSubreddit("/r/RealGirls/");
                Listing<Post> listings = subreddit.GetTop(FromTime.Hour;
                foreach(Post post in listings.GetListing(1))
                {
                    ctx.RespondAsync(post.Url.ToString());
                }
                
                Thread.Sleep(new TimeSpan(0, 0, 30));
            }
        }
    }
}
