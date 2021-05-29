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

                        var embed = new DiscordEmbedBuilder();
                        embed.SetupWithDefaultValues(_discordClient);
                        embed.WithTitle($"{ DiscordEmoji.FromName(_discordClient, ":date:") } Dagens ord");

                        Entry.AddToBuilder(embed);

                        if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
                        {
                            try
                            {
                                var grodanEmoji = DiscordEmoji.FromName(_discordClient, $":{ Resources.ConstantData.DagensOrd.GrodanEmoji }:");
                                embed.AddField(Strings.FredagMessage, grodanEmoji);
                            }
                            catch
                            {
                                LogWarning("Could not find emoji...");
                            }

                        }

                        await _discordChannel.SendMessageAsync(embed: embed);
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
            GetRandomWord();

            bool resultFound = false;

            while (!resultFound)
            {
                Log($"Searching on SO for: { Entry.Grundform }...");

                var searcher = new SvenskaSearcher(SvenskaKälla.SO);

                var searcherTask = searcher.SearchAsync(Entry.Grundform);

                await searcherTask;

                if (searcherTask.Result == SearchResponse.WebException)
                {
                    var timespan = TimeSpan.FromMinutes(5);

                    LogWarning($"Received a WebException. Will try again in { timespan.TotalSeconds } seconds.");

                    await Task.Delay(timespan);
                }
                else
                {
                    if (searcherTask.Result == SearchResponse.Successful)
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

                            GetRandomWord();

                            await Task.Delay(delay);
                        }
                    }
                }                
            }

            using (var sw = File.CreateText(Path))
            {
                sw.Write(JsonConvert.SerializeObject(Entry, Formatting.Indented));
            }
        }

        private void GetRandomWord()
        {
            var ordbok = Resources.RuntimeData.FolketsOrdbok;

            var r = new Random().Next(0, ordbok.Words.Count - 1);

            Entry = ordbok.Words[r];

            Log($"Seeding new word from Folketsordbok... New word is: { Entry.Grundform }.");
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
