using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace svenskabot
{
    public interface ISearcherResult
    {
        public List<DiscordEmbedBuilder> GetEmbedsFromSearchResult(DiscordClient discordClient);
    }

    public interface ISearcherAsync
    {
        Task<ISearcherResult> SearchAsync(string searchTerm);
    }
}
