﻿using DSharpPlus.Entities;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace svenskabot
{
    public interface ISearchResult
    {
        public List<DiscordEmbedBuilder> AsEmbeds();
    }

    public interface ISearcher
    {
        string SearchTerm { get; }
        string SearchUrl { get; }
        Task Search(string searchTerm);
        ISearchResult LastResult { get; }
    }

    public abstract class Searcher : ISearcher
    {
        public string SearchTerm { get; private set; }
        public abstract string SearchUrl { get; }
        public ISearchResult LastResult { get; private set; } = default;

        public async Task Search(string searchTerm) 
        {
            SearchTerm = searchTerm;

            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(SearchUrl);

            LastResult = ProcessDoc(doc);
        }

        protected abstract ISearchResult ProcessDoc(HtmlDocument htmlDocument);
    }
}
