using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace svenskabot
{
    class SOSearcher
    {
        public string SearchTerm { get; private set; }

        public string SourceUrl
        {
            get { return (RootUrl + SearchTerm).Replace(' ', '+'); }
        }

        private const string RootUrl = "https://svenska.se/tri/f_so.php?sok=";

        public SOSearcher(string searchTerm)
        {
            SearchTerm = searchTerm;
        }

        public async Task<OrdEntry> GetOrdEntry()
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(SourceUrl);

            return GetEntry(doc);
        }

        private OrdEntry GetEntry(HtmlDocument htmlDocument)
        {
            if (!htmlDocument.DocumentNode.InnerHtml.Contains("lexem"))
                return null;

            var lexemNodes = htmlDocument.DocumentNode
                .SelectNodes("//*[@class='lexem']")
                .ToList();

            var definitions = new List<OrdDefinition>();

            foreach (var lexemNode in lexemNodes)
            {
                var definitionNode = lexemNode.SelectSingleNode($"./*[@class='def']");
                var definitionTNode = lexemNode.SelectSingleNode($"./*[@class='deft']");

                var exempel = new List<string>();

                lexemNode
                    .SelectNodes(".//*[@class='syntex']")
                    .ToList()
                    .ForEach(x => exempel.Add(x.InnerText));

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

            return new OrdEntry(grundForm, ordklass, definitions, böjning);
        }
    }
}
