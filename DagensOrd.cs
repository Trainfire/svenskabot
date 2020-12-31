using FolketsOrdbokResource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;

namespace svenskabot
{
    public class DagensOrd
    {
        public OrdEntry Entry { get; private set; }

        private const string FileName = "dagensord";
        private string Path { get; } = AppDomain.CurrentDomain.BaseDirectory + FileName;

        private FolketsOrdbok _folketsOrdbok;
        private Timer _timer;
        private DateTime _triggerTime;

        public DagensOrd(FolketsOrdbok folketsOrdbok)
        {
            _folketsOrdbok = folketsOrdbok;

            if (File.Exists(Path))
            {
                using (var file = File.OpenText(Path))
                {
                    Entry = JsonConvert.DeserializeObject<OrdEntry>(file.ReadToEnd());
                }
            }
            else
            {
                GetDagensOrd();
            }

            // Start timer to fetch next word of the day.
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

            UpdateTimer();
        }

        private void UpdateTimer()
        {
            _triggerTime = DateTime.Now.AddSeconds(10);
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now > _triggerTime)
            {
                Console.WriteLine("Fetch new word...");

                GetDagensOrd();
                UpdateTimer();

                Console.WriteLine($"Dagensord is now: { Entry.Grundform }");
            }
        }

        private void GetDagensOrd()
        {
            var r = new Random().Next(0, _folketsOrdbok.Words.Count - 1);

            Entry = _folketsOrdbok.Words[r];

            using (var sw = File.CreateText(FileName))
            {
                sw.Write(JsonConvert.SerializeObject(Entry, Formatting.Indented));
            }
        }
    }
}
