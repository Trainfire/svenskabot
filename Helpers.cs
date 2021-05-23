using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace svenskabot
{
    public static class DiscordTextFormatter
    {
        public static string ToBold(this string aString) { return $"**{ aString }**"; }
        public static string ToBoldAndItalics(this string aString) { return $"***{ aString }***"; }
        public static string ToItalics(this string aString) { return $"*{ aString }*"; }
    }

    public static class DiscordEmbedBuilderEx
    {
        // See here: https://support.discord.com/hc/en-us/community/posts/360056286551--BUG-Infinite-Scroll-Embed-Android-
        public static void HackFixWidth(this DiscordEmbedBuilder discordEmbedBuilder)
        {
            discordEmbedBuilder.WithDescription("------------------------------------------------------");
        }

        public static void WithTitle(this DiscordEmbedBuilder discordEmbedBuilder, DiscordClient discordClient, string title, string emojiString)
        {
            var emoji = DiscordEmoji.FromName(discordClient, emojiString);

            discordEmbedBuilder.WithTitle($"{ emoji } { title }");
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
