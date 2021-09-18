using DSharpPlus;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace svenskabot
{
    class ExempelMeningarResult : ISearcherResult
    {
        public IReadOnlyList<string> Examples { get { return _examples; } }
        public string SourceUrl { get; private set; }
        public string SearchTerm { get; private set; }

        private List<string> _examples { get; set; } = new List<string>();

        public ExempelMeningarResult() { }

        public ExempelMeningarResult(List<string> examples, string sourceUrl, string searchTerm)
        {
            _examples = examples;
            SourceUrl = sourceUrl;
            SearchTerm = searchTerm;
        }

        public List<DiscordEmbedBuilder> GetEmbedsFromSearchResult(DiscordClient discordClient)
        {
            var outEmbed = new DiscordEmbedBuilder();

            outEmbed.SetupWithDefaultValues(discordClient);
            outEmbed.AddSearchTitle(discordClient, "Exempelmeningar");

            int maxExamples = Resources.ConstantData.ExempelMeningar.MaxSentences;

            var filteredExamples = new List<string>();

            if (Examples.Count() != 0)
            {
                float sample = Math.Max(1, Examples.Count() / (float)maxExamples);

                for (int i = 0; i < Math.Min(maxExamples, Examples.Count() - 1); i++)
                {
                    int sampleIndex = (int)Math.Ceiling(i * sample);
                    sampleIndex = (int)MathF.Min(sampleIndex, Examples.Count() - 1);

                    filteredExamples.Add(Examples[sampleIndex]);
                }
            }

            if (filteredExamples.Count() != 0)
            {
                var outExamples = new List<string>();

                // Embed values can have a max of 1024 characters.
                const int lengthLimit = 1024;

                var totalLength = 0;

                for (int i = 0; i < filteredExamples.Count(); i++)
                {
                    var example = filteredExamples[i];

                    // Make search term bold.
                    var regex = new Regex(SearchTerm, RegexOptions.IgnoreCase);
                    var match = regex.Match(example);
                    example = example.Replace(match.Value, match.Value.ToBoldAndItalics());

                    //  Make ordinal.
                    example = $"{ i + 1 }. { example }.";

                    totalLength += example.Length;

                    if (totalLength < lengthLimit)
                        outExamples.Add(example);
                }

                outEmbed.AddField("Exempel", string.Join("\n", outExamples));

                outEmbed.AddField(Strings.Source, SourceUrl);
            }
            else
            {
                outEmbed.WithDescription(Strings.NoResultFound);
            }

            var outEmbeds = new List<DiscordEmbedBuilder>();
            outEmbeds.Add(outEmbed);

            return new List<DiscordEmbedBuilder>(outEmbeds);
        }
    }

    class ExempelMeningarSearcher : WebscrappingSearcher
    {
        public override string SearchUrl
        {
            get { return "https://exempelmeningar.se/sv/" + SearchTerm; }
        }

        protected override Task<ISearcherResult> ProcessHtmlDocument(HtmlDocument htmlDocument)
        {
            var examples = new List<string>();

            var tabContentNodes = htmlDocument.DocumentNode
                .SelectNodes("//*[@class='tab-content']");

            if (tabContentNodes != null && tabContentNodes.Count > 0)
            {
                var firstTab = tabContentNodes.First();

                var sourceTextArr = string.Join("", firstTab.InnerText.Split("<hr>"))
                    .Replace("\n", "")
                    .Trim()
                    .Split(".");

                for (int i = 0; i < sourceTextArr.Length; i++)
                {
                    var text = sourceTextArr[i];
                    if (text != string.Empty)
                        examples.Add(text);
                }
            }

            return Task.FromResult<ISearcherResult>(new ExempelMeningarResult(examples, SearchUrl, SearchTerm));
        }
    }
}
