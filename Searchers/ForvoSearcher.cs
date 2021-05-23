
using DSharpPlus;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace svenskabot
{
    class ForvoResult : ISearchResult
    {
        private bool _isResultValid;
        private string _searchTerm;
        private string _searchUrl = null;

        public ForvoResult(string searchTerm, string searchUrl, bool isResultValid)
        {
            _searchTerm = searchTerm;
            _searchUrl = searchUrl;
            _isResultValid = isResultValid;
        }

        public List<DiscordEmbedBuilder> AsEmbeds(DiscordClient discordClient)
        {
            var outBuilder = new DiscordEmbedBuilder();

            outBuilder.SetupWithDefaultValues(discordClient);

            outBuilder.WithTitle(discordClient, "Forvo", ":speaking_head:");

            if (_isResultValid)
            {
                outBuilder.WithDescription(_searchUrl);
            }
            else
            {
                outBuilder.WithDescription(Strings.NoResultFound);
            }

            return new List<DiscordEmbedBuilder>() { outBuilder };
        }
    }

    class ForvoSearcher : Searcher
    {
        public override string SearchUrl
        {
            get { return $"https://forvo.com/word/{ SearchTerm }/#sv"; }
        }

        protected override Task<ISearchResult> ProcessDoc(HtmlDocument htmlDocument)
        {
            var isResultValid = !htmlDocument.ParsedText.Contains("The page you are looking for doesn't exist.");

            return Task.FromResult<ISearchResult>(new ForvoResult(SearchTerm, SearchUrl, isResultValid));
        }
    }
}
