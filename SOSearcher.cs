using DSharpPlus.Entities;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace svenskabot
{
    class SOSearchResult : ISearchResult
    {
        private List<OrdEntry> _ordEntries;
        private string _searchTerm;

        public SOSearchResult(string searchTerm, List<OrdEntry> ordEntries = null)
        {
            _searchTerm = searchTerm;
            _ordEntries = ordEntries;
        }

        public List<DiscordEmbedBuilder> AsEmbeds()
        {
            var outBuilders = new List<DiscordEmbedBuilder>();

            if (_ordEntries == null)
            {
                var outBuilder = new DiscordEmbedBuilder();
                outBuilder.AddField("No result found for", _searchTerm);
                outBuilders.Add(outBuilder);
            }
            else
            {
                foreach (var ordEntry in _ordEntries)
                {
                    var outBuilder = new DiscordEmbedBuilder();

                    // NB: The url for viewing the source is different from the one used for parsing the entry.
                    var sourceUrl = $"https://svenska.se/tre/?sok={ ordEntry.Grundform }&pz=1";
                    sourceUrl = sourceUrl.Replace(" ", "+");

                    outBuilder.AddField("Källa", $"SO - { sourceUrl }");

                    outBuilder = ordEntry.AsEmbed();

                    outBuilders.Add(outBuilder);
                }
            }

            return outBuilders;
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

            var lemmaNodes = htmlDocument.DocumentNode
                .SelectNodes("//*[@class='lemma']")
                .ToList();

            var ordEntries = new List<OrdEntry>();

            foreach (var lemmaNode in lemmaNodes)
            {
                var ordEntry = ProcessLemmaNode(lemmaNode);
                ordEntries.Add(ordEntry);
            }

            return new SOSearchResult(SearchTerm, ordEntries);
        }

        private OrdEntry ProcessLemmaNode(HtmlNode htmlNode)
        {
            var lexemNodes = htmlNode
                .SelectNodes("./*[@class='lexem']")
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

                var definition = definitionNode?.InnerText ?? string.Empty;
                var definitionT = definitionTNode?.InnerText ?? string.Empty;
                definitions.Add(new OrdDefinition(definition, definitionT, exempel));
            }

            string localParseClass(string className)
            {
                var node = htmlNode.SelectSingleNode($"./*[@class='{ className }']");
                return node != null ? node.InnerText : string.Empty;
            };

            var grundForm = localParseClass("grundform");
            var ordklass = localParseClass("ordklass");
            var böjning = localParseClass("bojning");

            return new OrdEntry(grundForm, ordklass, definitions, böjning);
        }
    }
}
