using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace svenskabot
{
    public static class DiscordTextFormatter
    {
        public static string ToBold(this string aString) { return $"**{ aString }**"; }
        public static string ToBoldAndItalics(this string aString) { return $"***{ aString }***"; }
        public static string ToItalics(this string aString) { return $"*{ aString }*"; }
    }

    public class SvenskaBotDiscordEmbedBuilder : DiscordEmbedBuilder
    {
        public SvenskaBotDiscordEmbedBuilder(DiscordClient discordClient)
        {
            WithColor(DiscordColor.Green);
            WithFooter($"Med kärlek från + { discordClient.CurrentUser.Username }", null);
        }
    }

    public class StringBuilderEx
    {
        private StringBuilder _stringBuilder = new StringBuilder();

        public StringBuilderEx Append(string value)
        {
            _stringBuilder.Append(value);
            return this;
        }

        public StringBuilderEx AppendWithCondition(string value, bool condition)
        {
            if (condition)
                Append(value);
            return this;
        }

        /// <summary>
        /// Appends '\nHeader: '
        /// </summary>
        public StringBuilderEx AddHeader(string header)
        {
            AppendLineBreak();
            _stringBuilder.Append($"{ header }: ".ToBold());
            return this;
        }

        /// <summary>
        /// Adds '\nHeader: Value'
        /// </summary>
        public StringBuilderEx AddField(string header, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                AddHeader(header);
                _stringBuilder.Append(value);
            }
            return this;
        }

        /// <summary>
        /// Appends '\nHeader: Value 1, Value 2, Value 3, ...' if the values array is valid.
        /// </summary>
        public StringBuilderEx AddField(string header, string[] values)
        {
            if (values.Length != 0)
            {
                AddHeader(header);
                _stringBuilder.AppendJoin("; ", values);
            }
            return this;
        }

        /// <summary>
        /// Appends '\nHeader: Value 1, Value 2, Value 3, ...' if the values list is valid.
        /// </summary>
        public StringBuilderEx AddField(string header, List<string> values)
        {
            AddField(header, values.ToArray());
            return this;
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }

        private StringBuilderEx AppendLineBreak()
        {
            _stringBuilder.Append("\n");
            return this;
        }
    }

    public static class DiscordEmbedBuilderEx
    {
        // See here: https://support.discord.com/hc/en-us/community/posts/360056286551--BUG-Infinite-Scroll-Embed-Android-
        public static void HackFixWidth(this DiscordEmbedBuilder discordEmbedBuilder)
        {
            discordEmbedBuilder.WithDescription("------------------------------------------------------");
        }

        public static void SetupWithDefaultValues(this DiscordEmbedBuilder discordEmbedBuilder, DiscordClient discordClient)
        {
            foreach (var guild in discordClient.Guilds)
            {
                foreach (var member in guild.Value.Members)
                {
                    if (member.Id == discordClient.CurrentUser.Id)
                    {
                        if (member.Roles.Count() != 0)
                        {
                            var role = member.Roles.First();
                            discordEmbedBuilder.WithColor(role.Color);
                        }

                        return;
                    }
                }
            }

            discordEmbedBuilder.WithFooter($"Med kärlek från { discordClient.CurrentUser.Username }", null);
        }

        public static void WithTitle(this DiscordEmbedBuilder discordEmbedBuilder, DiscordClient discordClient, string title, string emojiString)
        {
            var emoji = DiscordEmoji.FromName(discordClient, emojiString);

            discordEmbedBuilder.WithTitle($"{ emoji } { title }");
        }

        /// <summary>
        /// Checks if name and value are both valid before calling AddField.
        /// </summary>
        public static void AddFieldSafe(this DiscordEmbedBuilder discordEmbedBuilder, string name, string value, bool inline = false)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
                discordEmbedBuilder.AddField(name, value, inline);
        }

        public static void AddSearchTitle(this DiscordEmbedBuilder discordEmbedBuilder, DiscordClient discordClient, string source = "")
        {
            var emoji = DiscordEmoji.FromName(discordClient, ":mag:");

            var title = $"{ emoji } Sökresultat";

            if (source != string.Empty)
                title = $"{ title } ({ source })";

            discordEmbedBuilder.WithTitle(title);
        }

        public static DiscordEmbedBuilder FromException(DiscordClient discordClient, Exception exception)
        {
            var builder = new DiscordEmbedBuilder();

            var fireEmoji = DiscordEmoji.FromName(discordClient, ":fire:");
            builder.WithTitle($"{ fireEmoji } Exception Thrown { fireEmoji }");

            if (exception.Message != string.Empty)
                builder.AddField("Exception Message", exception.Message);

            builder.AddField("Stack Track", exception.StackTrace);

            var ownerID = Resources.ConstantData.General.OwnerID;
            if (ownerID != string.Empty)
            {
                builder.WithDescription($"<@{ ownerID }>");
            }

            return builder;
        }
    }

    public static class CommandContextEx
    {
        public static Task<DiscordMessage> RespondAsync(this CommandContext ctx, Exception exception)
        {
            var builder = DiscordEmbedBuilderEx.FromException(ctx.Client, exception);

            builder.AddField("Context Message", ctx.Message.Content);

            return ctx.RespondAsync(embed: builder);
        }
    }
}
