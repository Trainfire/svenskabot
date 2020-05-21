using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Web;

namespace svenskabot
{
    public class Commands
    {
        [Command("source"), Description("Returns the GitHub project page.")]
        public async Task GetProjectSource(CommandContext ctx)
        {
            var builder = new DiscordEmbedBuilder();
            builder.AddField("Source", "https://github.com/Trainfire/svenskabot");

            await ctx.RespondAsync(embed: builder);
        }

        [Command("define"), Description("Searches SO for the specified word."), Aliases("definera")]
        public async Task SearchSO(CommandContext ctx)
        {
            string searchTerm = ctx.RawArgumentString;
            searchTerm = searchTerm.TrimStart();

            // Show typing response whilst searching.
            await ctx.TriggerTypingAsync();

            var searcher = new SOSearcher(searchTerm);
            var entry = await searcher.GetOrdEntry();

            DiscordEmbedBuilder outBuilder;
            if (entry != null)
            {
                outBuilder = new DiscordEmbedBuilderFromOrdEntry(entry, 1).EmbedBuilder;

                var url = $"https://svenska.se/tre/?sok={ entry.Grundform }&pz=1";
                url = url.Replace(" ", "+");

                outBuilder.AddField("Källa", $"SO - { url }");
            }
            else
            {
                outBuilder = new DiscordEmbedBuilder();
                outBuilder.AddField("No result found for", searchTerm);
            }

            await ctx.RespondAsync(embed: outBuilder);
        }

        [Command("examples"), Description("Searches exempelmeningar.se for the specified word."), Aliases("exempel")]
        public async Task SearchExempelMeningar(CommandContext ctx)
        {
            string searchTerm = ctx.RawArgumentString;
            searchTerm = searchTerm.TrimStart();

            // Show typing response whilst searching.
            await ctx.TriggerTypingAsync();

            var searcher = new ExempelMeningarSearcher(searchTerm);
            var result = await searcher.GetExamples();
            var embedBuilder = new DiscordEmbedBuilderFromExampelMeningarSearcher(result, 5);

            await ctx.RespondAsync(embed: embedBuilder.EmbedBuilder);
        }
    }
}
