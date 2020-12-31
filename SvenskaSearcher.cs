using DSharpPlus.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace svenskabot
{
    class SvenskaSearchResult : ISearchResult
    {
        public IReadOnlyList<OrdEntry> OrdEntries { get { return _ordEntries; } }

        private List<OrdEntry> _ordEntries = new List<OrdEntry>();
        private string _searchTerm;

        public SvenskaSearchResult(string searchTerm, List<OrdEntry> ordEntries = null)
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
                int maxEmbeds = Resources.Config.SvenskaSearcher.MaxEmbeds;
                int count = Math.Min(maxEmbeds, _ordEntries.Count());

                for (int i = 0; i < count; i++)
                {
                    var ordEntry = _ordEntries[i];

                    var outBuilder = new DiscordEmbedBuilder();
                    outBuilder = ordEntry.AsEmbed();
                    outBuilder.HackFixWidth();

                    // NB: The url for viewing the source is different from the one used for parsing the entry.
                    var sourceUrl = $"https://svenska.se/tre/?sok={ ordEntry.Grundform }&pz=1";
                    sourceUrl = sourceUrl.Replace(" ", "+");

                    outBuilder.AddField("Källa", sourceUrl);

                    outBuilders.Add(outBuilder);
                }

                if (_ordEntries.Count > maxEmbeds)
                {
                    var outBuilder = new DiscordEmbedBuilder();

                    outBuilder.AddField("Obs", $"Det finns { _ordEntries.Count - maxEmbeds } till inlägg som kan ses online.");

                    outBuilders.Add(outBuilder);
                }
            }

            return outBuilders;
        }
    }

    public enum SvenskaKälla
    {
        SO,
        SAOL,
    }

    public class SvenskaSearcher : Searcher
    {
        public override string SearchUrl
        {
            get 
            {
                string källaOrd = "";

                switch (_källa)
                {
                    case SvenskaKälla.SO: källaOrd = "so"; break;
                    case SvenskaKälla.SAOL: källaOrd = "saol"; break;
                }

                var url = $"https://svenska.se/tri/f_{ källaOrd }.php?sok={ SearchTerm }";
                url = url.Replace(' ', '+');

                return url;
            }
        }

        private SvenskaKälla _källa;

        public SvenskaSearcher(SvenskaKälla källa)
        {
            _källa = källa;
        }

        protected override ISearchResult ProcessDoc(HtmlDocument htmlDocument)
        {
            if (!htmlDocument.DocumentNode.InnerHtml.Contains("lemma"))
                return new SvenskaSearchResult(SearchTerm);

            var lemmaNodes = htmlDocument.DocumentNode
                .SelectNodes("//*[@class='lemma']")
                .ToList();

            var ordEntries = new List<OrdEntry>();

            foreach (var lemmaNode in lemmaNodes)
            {
                var ordEntry = ProcessLemmaNode(lemmaNode);
                ordEntries.Add(ordEntry);
            }

            return new SvenskaSearchResult(SearchTerm, ordEntries);
        }

        private OrdEntry ProcessLemmaNode(HtmlNode htmlNode)
        {
            string localParseClass(string className)
            {
                var node = htmlNode.SelectSingleNode($"./*[@class='{ className }']");
                return node != null ? node.InnerText : string.Empty;
            };

            var grundForm = localParseClass("grundform");
            if (grundForm == string.Empty)
                grundForm = localParseClass("grundform_ptv");

            var ordklass = localParseClass("ordklass");
            var böjning = localParseClass("bojning");

            var lexemNodes = htmlNode
                .SelectNodes("./*[@class='lexem']")
                .ToList();

            var definitions = new List<OrdDefinition>();

            foreach (var lexemNode in lexemNodes)
            {
                var definitionNode = lexemNode.SelectSingleNode($".//*[@class='def']");
                var definitionTNode = lexemNode.SelectSingleNode($".//*[@class='deft']");

                var syntexNodes = lexemNode.SelectNodes(".//*[@class='syntex']");
                var exempel = new List<string>();

                if (syntexNodes != null)
                {
                    syntexNodes
                        .ToList()
                        .ForEach(x => exempel.Add(x.InnerText));
                }

                var definitionStr = definitionNode?.InnerText;
                var definitionTStr = definitionTNode?.InnerText;
                var definition = new OrdDefinition(definitionStr, definitionTStr, exempel);

                if (definition.IsValid())
                    definitions.Add(definition);
            }

            return new OrdEntry(grundForm, ordklass, definitions, böjning);
        }
    }
}
