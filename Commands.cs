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
        public async Task SearchSO(CommandContext ctx) => await Search(ctx, new SOSearcher());

        [Command("examples"), Description("Searches exempelmeningar.se for the specified word."), Aliases("exempel")]
        public async Task SearchExempelMeningar(CommandContext ctx) => await Search(ctx, new ExempelMeningarSearcher());

        private async Task Search(CommandContext ctx, ISearcher searcher)
        {
            string searchTerm = ctx.RawArgumentString;
            searchTerm = searchTerm.TrimStart();

            // Show typing response whilst searching.
            await ctx.TriggerTypingAsync();

            await searcher.Search(searchTerm);

            if (searcher.LastResult != null)
                await ctx.RespondAsync(embed: searcher.LastResult.AsEmbed());
        }
    }
}
