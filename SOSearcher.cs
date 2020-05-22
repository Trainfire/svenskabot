using DSharpPlus.Entities;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace svenskabot
{
    class SOSearchResult : ISearchResult
    {
        private OrdEntry _ordEntry;
        private string _searchTerm;

        public SOSearchResult(string searchTerm, OrdEntry ordEntry = null)
        {
            _searchTerm = searchTerm;
            _ordEntry = ordEntry;
        }

        public DiscordEmbedBuilder AsEmbed()
        {
            DiscordEmbedBuilder outBuilder;

            if (_ordEntry == null)
            {
                outBuilder = new DiscordEmbedBuilder();
                outBuilder.AddField("No result found for", _searchTerm);
            }
            else
            {
                outBuilder = _ordEntry.AsEmbed();

                // NB: The url for viewing the source is different from the one used for parsing the entry.
                var sourceUrl = $"https://svenska.se/tre/?sok={ _ordEntry.Grundform }&pz=1";
                sourceUrl = sourceUrl.Replace(" ", "+");

                outBuilder.AddField("Källa", $"SO - { sourceUrl }");
            }

            return outBuilder;
        }
    }

    class SOSearcher : Searcher
    {
        public override string SearchUrl
        {
            get { return ("https://svenska.se/tri/f_so.php?sok=" + SearchTerm).Replace(' ', '+'); }
        }

        protected override ISearchResult ProcessDoc(HtmlDocument htmlDocument)
        {
            if (!htmlDocument.DocumentNode.InnerHtml.Contains("lexem"))
                return new SOSearchResult(SearchTerm);

            var lexemNodes = htmlDocument.DocumentNode
                .SelectNodes("//*[@class='lexem']")
                .ToList();

            var definitions = new List<OrdDefinition>();

            foreach (var lexemNode in lexemNodes)
            {
                var definitionNode = lexemNode.SelectSingleNode($"./*[@class='def']");
                var definitionTNode = lexemNode.SelectSingleNode($"./*[@class='deft']");

                var syntexNodes = lexemNode.SelectNodes(".//*[@class='syntex']");
                var exempel = new List<string>();

                if (syntexNodes != null)
                {
                    syntexNodes
                        .ToList()
                        .ForEach(x => exempel.Add(x.InnerText));
                }

                definitions.Add(new OrdDefinition(definitionNode.InnerText, definitionTNode != null ? definitionTNode.InnerText : "", exempel));
            }

            string localParseClass(string className)
            {
                var node = htmlDocument.DocumentNode.SelectSingleNode($"//*[@class='{ className }']");
                return node != null ? node.InnerText : string.Empty;
            };

            var grundForm = localParseClass("grundform");
            var ordklass = localParseClass("ordklass");
            var böjning = localParseClass("bojning");

            var ordEntry = new OrdEntry(grundForm, ordklass, definitions, böjning);

            return new SOSearchResult(SearchTerm, ordEntry);
        }
    }
}
