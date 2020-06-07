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

        [Command("d")]
        public async Task SearchDeprecated(CommandContext ctx)
        {
            await ctx.RespondAsync($"Det här kommandot har tagits bort. Använd { "!so".ToItalics() } och { "!saol".ToItalics() } istället");
        }

        [Command("so"), Description("Searches SO for the specified word.")]
        public async Task SearchSO(CommandContext ctx) => await Search(ctx, new SvenskaSearcher(SvenskaKälla.SO));

        [Command("saol"), Description("Searches SAOL for the specified word.")]
        public async Task SearchSAOL(CommandContext ctx) => await Search(ctx, new SvenskaSearcher(SvenskaKälla.SAOL));

        [Command("examples"), Description("Searches exempelmeningar.se for the specified word."), Aliases("e", "exempel")]
        public async Task SearchExempelMeningar(CommandContext ctx) => await Search(ctx, new ExempelMeningarSearcher());

        private async Task Search(CommandContext ctx, ISearcher searcher)
        {
            string searchTerm = ctx.RawArgumentString;
            searchTerm = searchTerm.TrimStart();

            // Show typing response whilst searching.
            await ctx.TriggerTypingAsync();

            await searcher.Search(searchTerm);

            if (searcher.LastResult != null)
            {
                searcher.LastResult.AsEmbeds().ForEach(async e =>
                {
                    await ctx.RespondAsync(embed: e);
                });
            }
        }
    }
}
