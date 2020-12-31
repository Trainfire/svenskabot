using FolketsOrdbokResource;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace svenskabot
{
    public static class Resources
    {
        public static Config Config { get; private set; }
        public static FolketsOrdbok FolketsOrdbok { get; private set; }
        public static DagensOrd DagensOrd { get; private set; }

        private static string ConfigPath { get { return AppDomain.CurrentDomain.BaseDirectory + "config"; } }

        public static void Initialize()
        {
            LoadConfig();
            FolketsOrdbok = new FolketsOrdbok();
            DagensOrd = new DagensOrd(FolketsOrdbok);
        }

        static void LoadConfig()
        {
            if (File.Exists(ConfigPath))
            {
                using (var file = File.OpenText(ConfigPath))
                {
                    Config = JsonConvert.DeserializeObject<Config>(file.ReadToEnd());
                }
            }
            else
            {
                Config = new Config();

                using (var sw = File.CreateText(ConfigPath))
                {
                    sw.Write(JsonConvert.SerializeObject(Config, Formatting.Indented));
                }
            }
        }
    }
}
