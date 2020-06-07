using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace svenskabot
{
    public class Config
    {
        public ExempelMeningarConfig ExempelMeningar { get; set; } = new ExempelMeningarConfig();
        public SvenskaSearcherConfig SvenskaSearcher { get; set; } = new SvenskaSearcherConfig();
        public OrdConfig Ord { get; set; } = new OrdConfig();

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

    public static class ConfigInstance
    {
        public static Config Config { get; private set; }

        private static string Path { get { return AppDomain.CurrentDomain.BaseDirectory + "config"; } }

        static ConfigInstance()
        {
            if (File.Exists(Path))
            {
                using (var file = File.OpenText(Path))
                {
                    Config = JsonConvert.DeserializeObject<Config>(file.ReadToEnd());
                }
            }
            else
            {
                Config = new Config();

                using (var sw = File.CreateText(Path))
                {
                    sw.Write(JsonConvert.SerializeObject(Config, Formatting.Indented));
                }
            }
        }
    }
}
