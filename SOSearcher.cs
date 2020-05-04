using HtmlAgilityPack;
using System.Collections.Generic;
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
                var definition = lexemNode.SelectSingleNode("./*[@class='def']").InnerText;

                var exemplar = new List<string>();

                lexemNode
                    .SelectNodes(".//*[@class='syntex']")
                    .ToList()
                    .ForEach(x => exemplar.Add(x.InnerText));

                definitions.Add(new OrdDefinition(definition, exemplar));
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
