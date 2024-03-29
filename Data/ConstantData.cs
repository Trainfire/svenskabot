﻿namespace svenskabot
{
    public class ConstantData
    {
        public GeneralConfig General { get; set; } = new GeneralConfig();
        public ExternalSources Sources { get; set; } = new ExternalSources();
        public ExempelMeningarConfig ExempelMeningar { get; set; } = new ExempelMeningarConfig();
        public SvenskaSearcherConfig SvenskaSearcher { get; set; } = new SvenskaSearcherConfig();
        public OrdConfig Ord { get; set; } = new OrdConfig();
        public DagensOrdConfig DagensOrd { get; set; } = new DagensOrdConfig();

        public class GeneralConfig
        {
            public string OwnerID;
        }

        public class ExternalSources
        {
            public string FolketsOrdbokLexicon { get; set; } = "https://folkets-lexikon.csc.kth.se/folkets/folkets_sv_en_public.xml";
        }

        public class ExempelMeningarConfig
        {
            public int MaxSentences { get; set; } = 5;
        }

        public class SvenskaSearcherConfig
        {
            public string Url { get; set; } = "https://www.svenska.se";
            public int MaxEmbeds { get; set; } = 2;
        }

        public class OrdConfig
        {
            public int MaxDefinitions { get; set; } = 2;
            public int MaxExamplesPerDefinition { get; set; } = 5;
        }

        public class DagensOrdConfig
        {
            public int AnnounceHour { get; set; }
            public int AnnounceMinute { get; set; }
            public int MSDelayBetweenSearches { get; set; } = 1000;
            public bool IsDebugEnabled { get; set; }
            public string ChannelID { get; set; }
            public string GrodanEmoji { get; set; } = "grodan_boll";
        }
    }
}
