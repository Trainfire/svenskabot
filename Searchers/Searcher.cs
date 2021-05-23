using DSharpPlus;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace svenskabot
{
    public interface ISearchResult
    {
        public List<DiscordEmbedBuilder> AsEmbeds(DiscordClient discordClient);
    }

    public enum SearchResponse
    {
        Successful,
        WebException,
    }

    public interface ISearcherAsync
    {
        string SearchTerm { get; }
        Task<SearchResponse> SearchAsync(string searchTerm);
        ISearchResult LastResult { get; }
    }

    public abstract class Searcher : ISearcherAsync
    {
        public string SearchTerm { get; private set; }
        public abstract string SearchUrl { get; }
        public ISearchResult LastResult { get; private set; } = default;

        public async Task<SearchResponse> SearchAsync(string searchTerm)
        {
            SearchTerm = searchTerm;

            try
            {
                var doc = await LoadFromUrl(SearchUrl);

                LastResult = await ProcessDoc(doc);

                return SearchResponse.Successful;
            }
            catch (WebException ex)
            {
                LastResult = new WebExceptionSearchResult(this, ex);

                return SearchResponse.WebException;
            }
        }

        protected abstract Task<ISearchResult> ProcessDoc(HtmlDocument htmlDocument);

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

    public class WebExceptionSearchResult : ISearchResult
    {
        private Searcher _searcher;
        private WebException _webException;

        public WebExceptionSearchResult(Searcher searcher, WebException webException)
        {
            _searcher = searcher;
            _webException = webException;
        }

        public List<DiscordEmbedBuilder> AsEmbeds(DiscordClient discordClient)
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
