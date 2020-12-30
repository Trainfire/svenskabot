namespace svenskabot
{
    public class Config
    {
        public ExternalSources Sources { get; set; } = new ExternalSources();
        public ExempelMeningarConfig ExempelMeningar { get; set; } = new ExempelMeningarConfig();
        public SvenskaSearcherConfig SvenskaSearcher { get; set; } = new SvenskaSearcherConfig();
        public OrdConfig Ord { get; set; } = new OrdConfig();

        public class ExternalSources
        {
            public string FolketsOrdbokLexicon { get; set; } = "https://folkets-lexikon.csc.kth.se/folkets/folkets_sv_en_public.xml";
            // TODO: Add other URL resources here
        }

        public class ExempelMeningarConfig
        {
            public int MaxSentences { get; set; } = 5;
        }

        public class SvenskaSearcherConfig
        {
            public int MaxEmbeds { get; set; } = 2;
        }

        public class OrdConfig
        {
            public int MaxDefinitions { get; set; } = 2;
            public int MaxExamplesPerDefinition { get; set; } = 5;
        }
    }
}
