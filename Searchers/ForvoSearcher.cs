using DSharpPlus;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace svenskabot
{
    class ForvoResult : ISearcherResult
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

        public List<DiscordEmbedBuilder> GetEmbedsFromSearchResult(DiscordClient discordClient)
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

    class ForvoSearcher : WebscrappingSearcher
    {
        public override string SearchUrl
        {
            get { return $"https://forvo.com/word/{ SearchTerm }/#sv"; }
        }

        protected override Task<ISearcherResult> ProcessHtmlDocument(HtmlDocument htmlDocument)
        {
            var isResultValid = !htmlDocument.ParsedText.Contains("The page you are looking for doesn't exist.");

            return Task.FromResult<ISearcherResult>(new ForvoResult(SearchTerm, SearchUrl, isResultValid));
        }
    }
}
