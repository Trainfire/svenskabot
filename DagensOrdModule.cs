using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace svenskabot
{
    class DagensOrdModule : BaseModule
    {
        public OrdEntry Entry { get; private set; }

        private string Path { get; } = AppDomain.CurrentDomain.BaseDirectory + FileName;
        private const string FileName = "dagensord";

        private DiscordClient _discordClient;
        private DiscordChannel _discordChannel;
        private DateTime _targetTime;

        protected override void Setup(DiscordClient client)
        {
            _discordClient = client;

            if (Resources.ConstantData.DagensOrd.IsDebugEnabled)
                LogWarning("Debug mode is enabled.");

            LoadDagensOrdFromFile();

            Task.Run(Update);

            _discordClient.Ready += OnReady;
        }

        private Task OnReady(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            TryGetChannel(_discordClient);

            return Task.CompletedTask;
        }

        private void LoadDagensOrdFromFile()
        {
            if (File.Exists(Path))
            {
                using (var file = File.OpenText(Path))
                {
                    Entry = JsonConvert.DeserializeObject<OrdEntry>(file.ReadToEnd());
                }

                Log("Loaded word from file...");
            }
        }

        private void TryGetChannel(DiscordClient client)
        {
            var channelIDstr = Resources.ConstantData.DagensOrd.ChannelID;

            if (channelIDstr != null && channelIDstr != string.Empty)
            {
                var channelID = ulong.Parse(channelIDstr);

                client.GetChannelAsync(channelID).ContinueWith(task =>
                {
                    if (task.Status == TaskStatus.Faulted)
                    {
                        LogWarning($"Failed to find channel with id '{ channelID }'. Dagensord will not be posted!");

                        foreach (var ex in task.Exception.InnerExceptions)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        _discordChannel = task.Result;

                        Log($"Dagensord will be sent to channel: { _discordChannel.Name }");
                    }
                });
            }
        }

        private async Task Update()
        {
            UpdateTargetTime();

            while (true)
            {
                if (DateTime.Now > _targetTime)
                {
                    Log("Updating...");

                    await GetDagensOrd();

                    if (_discordChannel != null)
                    {
                        Log($"Posting to channel: { _discordChannel.Name }");

                        var strings = new List<string>();

                        if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
                        {
                            strings.Add($"God fredag mina bekanta!");

                            DiscordEmoji grodanEmoji = null;

                            try
                            {
                                grodanEmoji = DiscordEmoji.FromName(_discordClient, $":{ Resources.ConstantData.DagensOrd.GrodanEmoji }:");
                            }
                            catch
                            {
                                LogWarning("Could not find emoji...");
                            }

                            if (grodanEmoji != null)
                                strings.Add($"{ grodanEmoji }");
                        }

                        strings.Add("Dagens ord är...");

                        await _discordChannel.SendMessageAsync(content: string.Join(" ", strings), embed: Entry.AsEmbed());
                    }

                    UpdateTargetTime();
                }

                await Task.Delay(1000);
            }
        }

        private void UpdateTargetTime()
        {
            if (Resources.ConstantData.DagensOrd.IsDebugEnabled)
            {
                _targetTime = DateTime.Now.AddSeconds(15);
            }
            else
            {
                var hour = Math.Min(Resources.ConstantData.DagensOrd.AnnounceHour, 24);
                var minute = Math.Min(Resources.ConstantData.DagensOrd.AnnounceMinute, 60);
                var targetTimespan = new TimeSpan(hour, minute, 0);

                // Start from today since bot may have been started prior to showing dagensord.
                _targetTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, targetTimespan.Hours, targetTimespan.Minutes, targetTimespan.Seconds);

                var currentTimespan = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                if (currentTimespan.TotalMilliseconds >= targetTimespan.TotalMilliseconds)
                    _targetTime = _targetTime.AddDays(1);
            }

            Log($"Dagensord will be updated at: { _targetTime.ToString() }");
        }

        private async Task GetDagensOrd()
        {
            var ordbok = Resources.RuntimeData.FolketsOrdbok;

            bool resultFound = false;

            while (!resultFound)
            {
                var r = new Random().Next(0, ordbok.Words.Count - 1);

                Entry = ordbok.Words[r];

                Log($"Target word is: { Entry.Grundform }. Searching on SO...");

                var searcher = new SvenskaSearcher(SvenskaKälla.SO);

                await searcher.SearchAsync(Entry.Grundform);

                if (searcher.LastResult != null)
                {
                    var convertedResult = (SvenskaSearchResult)searcher.LastResult;

                    if (convertedResult.OrdEntries != null && convertedResult.OrdEntries.Count != 0)
                    {
                        Log($"Success! Dagensord is now: { Entry.Grundform }");

                        Entry = convertedResult.OrdEntries.First();

                        resultFound = true;
                    }
                    else
                    {
                        var delay = Resources.ConstantData.DagensOrd.MSDelayBetweenSearches;

                        Log($"Failed to fetch entry from SO. Trying again in { delay }ms...");

                        await Task.Delay(delay);
                    }
                }
            }

            using (var sw = File.CreateText(Path))
            {
                sw.Write(JsonConvert.SerializeObject(Entry, Formatting.Indented));
            }
        }

        private void Log(string message, LogLevel logLevel = LogLevel.Info)
        {
            _discordClient.DebugLogger.LogMessage(logLevel, "DagensOrd", message, DateTime.Now);
        }

        private void LogWarning(string message)
        {
            Log(message, LogLevel.Warning);
        }
    }
}
