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

        private static string ConfigPath { get { return AppDomain.CurrentDomain.BaseDirectory + "config"; } }
        private static string FolketsOrdbokFileName { get; } = "folkets";

        public static void Initialize()
        {
            LoadConfig();
            LoadFolketsOrdbok();
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

        static void LoadFolketsOrdbok()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + FolketsOrdbokFileName;

            Console.WriteLine("Downloading lexicon from Folkets Ordbok...");

            if (File.Exists(path))
            {
                Console.WriteLine("Lexicon from Folkets Ordbok already exists. Skipping download...");
            }
            else
            {
                using (var client = new WebClient())
                {
                    // TODO: Check if URL is valid...
                    client.DownloadFile(Resources.Config.Sources.FolketsOrdbokLexicon, FolketsOrdbokFileName);
                }

                Console.WriteLine("Finished downloading lexicon from Folkets Ordbok.");
            }

            if (File.Exists(path))
            {
                using (var file = File.OpenText(path))
                {
                    var serializer = new XmlSerializer(typeof(FolketsOrdbokSource));

                    var folketsOrdbokSource = (FolketsOrdbokSource)(serializer.Deserialize(file));

                    FolketsOrdbok = new FolketsOrdbok(folketsOrdbokSource);
                }
            }
        }
    }
}
