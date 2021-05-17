
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

        public List<DiscordEmbedBuilder> AsEmbeds()
        {
            var outBuilder = new DiscordEmbedBuilder();
            outBuilder.HackFixWidth();

            if (_isResultValid)
            {
                outBuilder.AddField($"Uttal för ordet '{ _searchTerm }' på Forvo", _searchUrl);
            }
            else
            {
                outBuilder.AddField("Inga result hittades på Forvo för", _searchTerm);
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
