using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

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
                outBuilder.AddField("Källa", "SO");
            }
            else
            {
                outBuilder = new DiscordEmbedBuilder();
                outBuilder.AddField("No result found for", searchTerm);
            }

            await ctx.RespondAsync(embed: outBuilder);
        }
    }
}
