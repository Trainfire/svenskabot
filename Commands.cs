using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
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

        [Command("so"), Description("Searches SO for the specified word.")]
        public async Task SearchSO(CommandContext ctx) => await SearchAsync(ctx, new SvenskaSearcher(SvenskaKälla.SO));

        // TODO: Update SAOL to support 2021 changes.
        //[Command("saol"), Description("Searches SAOL for the specified word.")]
        //public async Task SearchSAOL(CommandContext ctx) => await SearchAsync(ctx, new SvenskaSearcher(SvenskaKälla.SAOL));

        [Command("examples"), Description("Searches exempelmeningar.se for the specified word."), Aliases("e", "exempel")]
        public async Task SearchExempelMeningar(CommandContext ctx) => await SearchAsync(ctx, new ExempelMeningarSearcher());

        // TODO: Need a better way of handling invalid search results.
        //[Command("uttal"), Description("Searches forvo.com for the specified word."), Aliases("u")]
        //public async Task SearchForvo(CommandContext ctx) => await SearchAsync(ctx, new ForvoSearcher());

        [Command("folkets")]
        public async Task SearchFolketsOrdbok(CommandContext ctx) => await SearchAsync(ctx, new FolketsOrdbokSearcher());

        [Command("dagensord")]
        public async Task GetDagensOrd(CommandContext ctx)
        {
            var module = ctx.Client.GetModule<DagensOrdModule>();

            try
            {
                await ctx.RespondAsync(embed: module.Entry.AsEmbedBuilder());
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex);
            }
        }

        private async Task SearchAsync(CommandContext ctx, ISearcherAsync searcher)
        {
            string searchTerm = ctx.RawArgumentString;

            if (searchTerm == null)
            {
                await ctx.RespondAsync("Du måste ange ett sökord.");
                return;
            }

            searchTerm = searchTerm.TrimStart();

            // Show typing response whilst searching.
            await ctx.TriggerTypingAsync();

            try
            {
                await searcher.SearchAsync(searchTerm);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex);
            }

            if (searcher.LastResult != null)
            {
                searcher.LastResult.AsEmbeds(ctx.Client).ForEach(async e =>
                {
                    try
                    {
                        await ctx.RespondAsync(embed: e);
                    }
                    catch (Exception ex)
                    {
                        ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "Command", ex.Message, DateTime.Now);
                        await ctx.RespondAsync(ex);
                    }
                });
            }
        }
    }
}
