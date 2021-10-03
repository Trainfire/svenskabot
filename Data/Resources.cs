using Newtonsoft.Json;
using System;
using System.IO;

namespace svenskabot
{
    public static class Resources
    {
        public static ConstantData ConstantData { get; private set; } = new ConstantData();
        public static RuntimeData RuntimeData { get; private set; } = new RuntimeData();

        private static string ConfigPath { get { return AppDomain.CurrentDomain.BaseDirectory + "config"; } }

        public static void Initialize()
        {
            LoadConfig();
        }

        static void LoadConfig()
        {
            if (File.Exists(ConfigPath))
            {
                using (var file = File.OpenText(ConfigPath))
                {
                    ConstantData = JsonConvert.DeserializeObject<ConstantData>(file.ReadToEnd());
                }
            }
            else
            {
                ConstantData = new ConstantData();
            }

            // Save new file or overwrite existing one to keep structure updated.
            using (var sw = File.CreateText(ConfigPath))
            {
                sw.Write(JsonConvert.SerializeObject(ConstantData, Formatting.Indented));
            }
        }
    }
}
