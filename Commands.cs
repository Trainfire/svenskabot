using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace svenskabot
{
    public class Commands
    {
        [Command("define")]
        public async Task Define(CommandContext ctx) => await SearchSO(ctx);

        [Command("definera")]
        public async Task Definera(CommandContext ctx) => await SearchSO(ctx);

        private async Task SearchSO(CommandContext ctx)
        {
            string searchTerm = ctx.RawArgumentString;
            searchTerm = searchTerm.TrimStart();

            var searcher = new SOSearcher(searchTerm);
            var entry = await searcher.GetOrdEntry();

            DiscordEmbedBuilder outBuilder;
            if (entry != null)
            {
                outBuilder = new DiscordEmbedBuilderFromOrdEntry(entry).EmbedBuilder;
                outBuilder.AddField("Source", "SO");
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
