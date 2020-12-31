using FolketsOrdbokResource;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;

namespace svenskabot
{
    public class DagensOrd
    {
        public OrdEntry Entry { get; private set; }

        private string Path { get; } = AppDomain.CurrentDomain.BaseDirectory + FileName;

        private const string FileName = "dagensord";
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
                // TODO: Fix hack.
                Task.Run((async () => await GetDagensOrd()));
            }

            // Start timer to fetch next word of the day.
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();

            UpdateTimer();
        }

        private void UpdateTimer()
        {
            _triggerTime = DateTime.Now.AddDays(1);
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now >= _triggerTime)
            {
                Console.WriteLine("Fetch new word...");

                await GetDagensOrd();

                UpdateTimer();

                Console.WriteLine($"Dagensord is now: { Entry.Grundform }");
            }
        }

        private async Task GetDagensOrd()
        {
            var r = new Random().Next(0, _folketsOrdbok.Words.Count - 1);

            Entry = _folketsOrdbok.Words[r];

            Console.WriteLine($"New word is: { Entry.Grundform }");

            // Try to fetch the entry from SO.

            Console.WriteLine($"Searching word on SO...");

            var searcher = new SvenskaSearcher(SvenskaKälla.SO);

            await searcher.SearchAsync(Entry.Grundform);

            if (searcher.LastResult != null)
            {
                var convertedResult = (SvenskaSearchResult)searcher.LastResult;

                if (convertedResult.OrdEntries.Count != 0)
                {
                    Console.WriteLine("Success!");

                    Entry = convertedResult.OrdEntries.First();
                }
                else
                {
                    Console.WriteLine("Failed to fetch entry from SO. Defaulting to Folketsordbok.");
                }
            }

            using (var sw = File.CreateText(FileName))
            {
                sw.Write(JsonConvert.SerializeObject(Entry, Formatting.Indented));
            }
        }
    }
}
