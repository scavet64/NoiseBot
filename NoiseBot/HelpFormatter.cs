using DSharpPlus.CommandsNext.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Entities;
using System.Linq;
using DSharpPlus.Entities;

namespace NoiseBot
{
    public class HelpFormatter : BaseHelpFormatter
    {
        private readonly ConfigFile config = ConfigFile.Instance;

        private StringBuilder Content { get; }

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {
            this.Content = new StringBuilder();
        }

        public override CommandHelpMessage Build()
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x5349fe),
                Title = "List of Commands:"
            };

            embed.AddField($"Commands:", this.Content.ToString().Trim());
            DiscordEmbed discordEmbed = embed.Build();

            return new CommandHelpMessage(null, discordEmbed);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            this.Content.Append(command.Description ?? "No description provided.")
                .Append("\n\n");

            if (command.Aliases?.Any() == true)
            {
                this.Content.Append("Aliases: ")
                    .Append(string.Join(", ", command.Aliases))
                    .Append("\n\n");
            }

            if (command.Overloads?.Any() == true)
            {
                var sb = new StringBuilder();

                foreach (var ovl in command.Overloads.OrderByDescending(x => x.Priority))
                {
                    sb.Append(command.QualifiedName);

                    foreach (var arg in ovl.Arguments)
                    {
                        sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <")
                            .Append(arg.Name)
                            .Append(arg.IsCatchAll ? "..." : "")
                            .Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');
                    }

                    sb.Append('\n');

                    foreach (var arg in ovl.Arguments)
                    {
                        sb.Append(arg.Name)
                            .Append(" (")
                            .Append(this.CommandsNext.GetUserFriendlyTypeName(arg.Type))
                            .Append("): ")
                            .Append(arg.Description ?? "No description provided.")
                            .Append('\n');
                    }

                    sb.Append('\n');
                }

                this.Content.Append("Arguments:\n")
                    .Append(sb.ToString());
            }

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (this.Content.Length == 0)
            {
                //this.Content.Append("Displaying all available commands.\n\n");
            }
            else
            {
                this.Content.Append("Subcommands:\n");
            }
            if (subcommands?.Any() == true)
            {
                var maxCommandLength = subcommands.Max(xc => xc.Name.Length);
                var builder = new StringBuilder();
                foreach (var command in subcommands)
                {
                    builder
                        .Append("**")
                        .Append(config.CommandPrefix)
                        .Append(command.Name)
                        .Append("**")
                        .Append(" - ")
                       // .Append("\"")
                        .Append(string.IsNullOrWhiteSpace(command.Description) ? "No Description :^(" : command.Description)
                        //.Append("\"")
                        .Append("\n");
                }
                this.Content.Append(builder.ToString());
            }

            return this;
        }
    }
}
