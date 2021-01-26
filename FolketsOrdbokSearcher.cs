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

        public List<DiscordEmbedBuilder> AsEmbeds()
        {
            if (_ordEntry == null)
            {
                var outBuilder = new DiscordEmbedBuilder();
                outBuilder.HackFixWidth();
                outBuilder.AddField("Inga result hittades i Folketsordbok för", _searchTerm);

                return new List<DiscordEmbedBuilder>() { outBuilder };
            }
            else
            {
                return new List<DiscordEmbedBuilder>() { _ordEntry.AsEmbed() };
            }
        }
    }

    class FolketsOrdbokSearcher : ISearcherAsync
    {
        public string SearchTerm { get; private set; }

        public ISearchResult LastResult { get; private set; }

        public Task SearchAsync(string searchTerm)
        {
            var task = new Task(() =>
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

            task.RunSynchronously();

            return task;
        }
    }
}