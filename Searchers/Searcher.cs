using DSharpPlus.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace svenskabot
{
    public interface ISearchResult
    {
        public List<DiscordEmbedBuilder> AsEmbeds();
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

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SearchUrl);
            request.Timeout = 15000;
            request.Method = "HEAD";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(SearchUrl);
                    LastResult = ProcessDoc(doc);

                    return SearchResponse.Successful;
                }
            }
            catch (WebException ex)
            {
                LastResult = new WebExceptionSearchResult(this, ex);

                return SearchResponse.WebException;
            }
        }

        protected abstract ISearchResult ProcessDoc(HtmlDocument htmlDocument);
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

        public List<DiscordEmbedBuilder> AsEmbeds()
        {
            var embedBuilder = new DiscordEmbedBuilder();

            var uri = new Uri(_searcher.SearchUrl);

            embedBuilder.AddField("Åh nej.", $"Något gick fel med { uri.Host } ({ _webException.Status })");

            return new List<DiscordEmbedBuilder>() { embedBuilder };
        }
    }
}
