using DSharpPlus;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace svenskabot
{
    public enum WebscrappingSearcherResponse
    {
        Invalid,
        WebException,
        Successful,
    }

    /// <summary>
    /// Loads a webpage then performs webscrapping via HtmlAgilityPack.
    /// </summary>
    public abstract class WebscrappingSearcher : ISearcherAsync
    {
        public string SearchTerm { get; private set; }
        public abstract string SearchUrl { get; }
        public ISearcherResult LastResult { get; private set; } = default;
        public WebscrappingSearcherResponse LastResponse { get; private set; }

        public async Task<ISearcherResult> SearchAsync(string searchTerm)
        {
            SearchTerm = searchTerm;

            try
            {
                var doc = await LoadFromUrl(SearchUrl);

                LastResult = await ProcessHtmlDocument(doc);
                LastResponse = WebscrappingSearcherResponse.Successful;
            }
            catch (WebException ex)
            {
                LastResult = new WebExceptionSearchResult(this, ex);
                LastResponse = WebscrappingSearcherResponse.WebException;
            }

            return LastResult;
        }

        protected abstract Task<ISearcherResult> ProcessHtmlDocument(HtmlDocument htmlDocument);

        protected async Task<HtmlDocument> LoadFromUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 15000;
            request.Method = "HEAD";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                var web = new HtmlWeb();
                return await web.LoadFromWebAsync(request.RequestUri.ToString());
            }
        }
    }

    public class WebExceptionSearchResult : ISearcherResult
    {
        private WebscrappingSearcher _searcher;
        private WebException _webException;

        public WebExceptionSearchResult(WebscrappingSearcher searcher, WebException webException)
        {
            _searcher = searcher;
            _webException = webException;
        }

        public List<DiscordEmbedBuilder> GetEmbedsFromSearchResult(DiscordClient discordClient)
        {
            var embedBuilder = new DiscordEmbedBuilder();

            var uri = new Uri(_searcher.SearchUrl);

            embedBuilder.WithTitle(discordClient, "Web Exception", ":warning:");
            embedBuilder.AddField("URL", uri.OriginalString);
            embedBuilder.AddField("Exception", _webException.Message);
            embedBuilder.AddField("Status", _webException.Status.ToString());

            return new List<DiscordEmbedBuilder>() { embedBuilder };
        }
    }
}
