using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace svenskabot
{
    class FolketsOrdbokResult : ISearchResult
    {
        private string _searchTerm;
        private OrdEntry _ordEntry;

        public FolketsOrdbokResult(string searchTerm, OrdEntry ordEntry)
        {
            _searchTerm = searchTerm;
            _ordEntry = ordEntry;
        }

        public List<DiscordEmbedBuilder> AsEmbeds(DiscordClient discordClient)
        {
            DiscordEmbedBuilder outBuilder;

            var title = "Folkets Lexicon";

            if (_ordEntry == null)
            {
                outBuilder = new DiscordEmbedBuilder();
                outBuilder.AddSearchTitle(discordClient, title);
                outBuilder.WithDescription(Strings.NoResultFound);
            }
            else
            {
                outBuilder = _ordEntry.AsEmbed();
                outBuilder.AddSearchTitle(discordClient, title);
            }

            return new List<DiscordEmbedBuilder>() { outBuilder };
        }
    }

    class FolketsOrdbokSearcher : ISearcherAsync
    {
        public string SearchTerm { get; private set; }

        public ISearchResult LastResult { get; private set; }

        public async Task<SearchResponse> SearchAsync(string searchTerm)
        {
            await Task.Run(() =>
            {
                OrdEntry foundWord = null;

                foreach (var word in Resources.RuntimeData.FolketsOrdbok.Words)
                {
                    if (word.Grundform == searchTerm)
                    {
                        foundWord = word;
                    }
                }

                LastResult = new FolketsOrdbokResult(searchTerm, foundWord);
            });

            return SearchResponse.Successful;
        }
    }
}