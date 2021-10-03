using DSharpPlus;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace svenskabot
{
    class SvenskaSearchResult : ISearcherResult
    {
        public IReadOnlyList<OrdEntry> OrdEntries { get { return _ordEntries; } }

        private SvenskaKälla _källa;
        private string _searchTerm;
        private List<OrdEntry> _ordEntries = new List<OrdEntry>();

        public SvenskaSearchResult(SvenskaKälla källa, string searchTerm, List<OrdEntry> ordEntries = null)
        {
            _källa = källa;
            _searchTerm = searchTerm;
            _ordEntries = ordEntries;
        }

        public List<DiscordEmbedBuilder> GetEmbedsFromSearchResult(DiscordClient discordClient)
        {
            var outBuilders = new List<DiscordEmbedBuilder>();

            if (_ordEntries == null)
            {
                var outBuilder = new DiscordEmbedBuilder();
                outBuilder.AddSearchTitle(discordClient, _källa.ToString());
                outBuilder.WithDescription(Strings.NoResultFound);
                outBuilders.Add(outBuilder);
            }
            else
            {
                int maxEmbeds = Resources.ConstantData.SvenskaSearcher.MaxEmbeds;
                int count = Math.Min(maxEmbeds, _ordEntries.Count());

                for (int i = 0; i < count; i++)
                {
                    var ordEntry = _ordEntries[i];

                    var outBuilder = new DiscordEmbedBuilder();

                    outBuilder = ordEntry.AsEmbedBuilder();

                    outBuilder.SetupWithDefaultValues(discordClient);

                    string source;
                    if (count > 1)
                    {
                        source = $"{ _källa.ToString() } - { i + 1 }/{ count }";
                    }
                    else
                    {
                        source = _källa.ToString();
                    }

                    outBuilder.AddSearchTitle(discordClient, source);

                    outBuilders.Add(outBuilder);
                }

                if (_ordEntries.Count > maxEmbeds)
                {
                    var outBuilder = new DiscordEmbedBuilder();

                    outBuilder.SetupWithDefaultValues(discordClient);

                    outBuilder.AddField(Strings.Warning, $"Det finns { _ordEntries.Count - maxEmbeds } till inlägg som kan ses online.");

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

    public class SvenskaSearcher : WebscrappingSearcher
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

                var url = $"{ _rootUrl }/{ källaOrd }/?sok={ SearchTerm }&pz=1";

                return url;
            }
        }

        private const string _rootUrl = "http://www.svenska.se";
        private SvenskaKälla _källa;

        public SvenskaSearcher(SvenskaKälla källa)
        {
            _källa = källa;
        }

        protected override async Task<ISearcherResult> ProcessHtmlDocument(HtmlDocument htmlDocument)
        {
            // Is invalid search?
            if (htmlDocument.DocumentNode.InnerHtml.Contains("gav inga svar"))
                return new SvenskaSearchResult(_källa, SearchTerm);

            // Check if the page contains multiple different options, then move onto the first one.
            // (It doesn't matter which link we choose since they all lead to the same page.)
            var cshowNode = htmlDocument.DocumentNode
            .SelectSingleNode("//*[@class='cshow']");

            if (cshowNode != null)
            {
                var slankNodes = cshowNode
                .SelectNodes("./*[@class='slank']")
                .ToList();

                if (slankNodes != null)
                {
                    var firstLink = slankNodes
                        .First()
                        .GetAttributeValue("href", string.Empty);

                    if (firstLink != string.Empty)
                    {
                        try
                        {
                            // Replace htmlDocument with redirect page.
                            htmlDocument = await LoadFromUrl(_rootUrl + firstLink);
                        }
                        catch (WebException ex)
                        {
                            return new WebExceptionSearchResult(this, ex);
                        }
                    }
                }
            }

            var superlemmaNodes = htmlDocument.DocumentNode
                .SelectNodes("//*[@class='superlemma']")
                .ToList();

            var ordEntries = new List<OrdEntry>();

            foreach (var superLemmaNode in superlemmaNodes)
            {
                var ordEntry = ProcessSuperLemmaNode(superLemmaNode);
                ordEntries.Add(ordEntry);
            }

            var result = new SvenskaSearchResult(_källa, SearchTerm, ordEntries);

            return result;
        }

        private OrdEntry ProcessSuperLemmaNode(HtmlNode superLemmaNode)
        {
            string localParseClass(string className)
            {
                var nodes = superLemmaNode.SelectNodes($".//*[@class='{ className }']");
                return nodes != null ? string.Join(", ", nodes.ToList().Select(x => x.InnerText)) : string.Empty;
            };

            var grundForm = localParseClass("orto");
            if (grundForm == string.Empty)
                grundForm = localParseClass("grundform_ptv");

            var ordklass = localParseClass("ordklass");

            var böjningar = localParseClass("bojning");

            var lexemDivs = superLemmaNode
                .SelectNodes("./*[@class='lexemdiv']")
                .ToList();

            var definitions = new List<OrdDefinition>();

            foreach (var lexemDiv in lexemDivs)
            {
                var lexems = lexemDiv
                .SelectNodes("./*[@class='lexem']")
                .ToList();

                foreach (var lexem in lexems)
                {
                    var ordDefinitionBuilder = new OrdDefinitionBuilder();

                    ordDefinitionBuilder.Definition = lexem.SelectSingleNode($".//*[@class='def']")?.InnerText;
                    ordDefinitionBuilder.DefinitionT = lexem.SelectSingleNode($".//*[@class='deft']")?.InnerText;

                    var syntexNodes = lexem.SelectNodes(".//*[@class='syntex']");
                    if (syntexNodes != null)
                    {
                        syntexNodes
                            .ToList()
                            .ForEach(x => ordDefinitionBuilder.Exempel.Add(x.InnerText));
                    }

                    var valens = lexem.SelectSingleNode($".//*[@class='valens']");
                    if (valens != null)
                    {
                        var vtNodes = valens.SelectNodes($".//*[@class='vt']");

                        if (vtNodes != null)
                        {
                            vtNodes
                            .ToList()
                            .ForEach(x => ordDefinitionBuilder.Konstruktion.Add(x.InnerText));
                        }
                    }

                    var definition = ordDefinitionBuilder.AsNew();

                    if (definition.IsValid())
                        definitions.Add(definition);
                }
            }

            return new OrdEntry(grundForm, ordklass, definitions, böjningar, SearchUrl);
        }
    }
}
