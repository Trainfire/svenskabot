﻿using DSharpPlus.Entities;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace svenskabot
{
    class ExempelMeningarResult
    {
        public IReadOnlyList<string> Examples { get { return _examples; } }
        public string SourceUrl { get; private set; }

        private List<string> _examples { get; set; } = new List<string>();

        public ExempelMeningarResult(List<string> examples, string sourceUrl)
        {
            _examples = examples;
            SourceUrl = sourceUrl;
        }
    }

    class ExempelMeningarSearcher
    {
        public string SearchTerm { get; private set; }

        public string SourceUrl
        {
            get { return RootUrl + SearchTerm; }
        }

        private const string RootUrl = "https://exempelmeningar.se/sv/";

        public ExempelMeningarSearcher(string searchTerm)
        {
            SearchTerm = searchTerm;
        }

        public async Task<ExempelMeningarResult> GetExamples()
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(SourceUrl);

            return GetExamples(doc);
        }

        private ExempelMeningarResult GetExamples(HtmlDocument htmlDocument)
        {
            var examples = new List<string>();

            var tabContentNodes = htmlDocument.DocumentNode
                .SelectNodes("//*[@class='tab-content']");

            if (tabContentNodes != null && tabContentNodes.Count > 0)
            {
                var firstTab = tabContentNodes.First();

                var sourceText = string.Join("", firstTab.InnerText.Split("<hr>"));

                // Remove line break spam.
                sourceText = sourceText.Replace("\n", "");

                var textArr = sourceText.Split(".");

                foreach (var text in textArr)
                {
                    if (text != string.Empty)
                        examples.Add(text);
                }
            }

            return new ExempelMeningarResult(examples, SourceUrl);
        }
    }

    class DiscordEmbedBuilderFromExampelMeningarSearcher
    {
        public DiscordEmbedBuilder EmbedBuilder { get; private set; } = new DiscordEmbedBuilder();

        public DiscordEmbedBuilderFromExampelMeningarSearcher(ExempelMeningarResult result, int maxExamples = -1)
        {
            var filteredExamples = maxExamples > 0 ? result.Examples.Take(maxExamples) : result.Examples;

            if (filteredExamples.Count() != 0)
            {
                EmbedBuilder.AddField("Exempel", string.Join("\n", filteredExamples));
                EmbedBuilder.AddField("Källa", result.SourceUrl);
            }
            else
            {
                EmbedBuilder.AddField("Exempel", "Inga exempel hittades.");
            }
        }
    }
}
