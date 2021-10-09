using System;
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
        private string Path { get; } = AppDomain.CurrentDomain.BaseDirectory + FileName;
        private const string FileName = "dagensord";
        private const int MinDelayBetweenSearches = 1000;

        private OrdEntry _ordEntry;
        private DiscordClient _discordClient;
        private DiscordChannel _discordChannel;
        private DateTime _targetTime;

        public DiscordEmbedBuilder GetDagensOrdEmbedBuilder()
        {
            var embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.SetupWithDefaultValues(_discordClient);
            embedBuilder.WithTitle($"{ DiscordEmoji.FromName(_discordClient, ":date:") } Dagens ord");

            if (_ordEntry == null)
            {
                if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
                {
                    try
                    {
                        var grodanEmoji = DiscordEmoji.FromName(_discordClient, $":{ Resources.ConstantData.DagensOrd.GrodanEmoji }:");
                        embedBuilder.AddField(Strings.FredagMessage, grodanEmoji);
                    }
                    catch
                    {
                        LogWarning("Could not find emoji...");
                    }

                }

                // Append the data in Ord to the end of this embed.
                embedBuilder = _ordEntry.AddDataToEmbedBuilder(embedBuilder);
            }
            else
            {
                embedBuilder.AddField("Ojdå.", "Dagens ord finns inte!");
            }

            return embedBuilder;
        }

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
                    _ordEntry = JsonConvert.DeserializeObject<OrdEntry>(file.ReadToEnd());
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
            else
            {
                LogWarning($"The specified value for 'channelID' is invalid. Dagensord will not be posted!");
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

                        await _discordChannel.SendMessageAsync(embed: GetDagensOrdEmbedBuilder());
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
                Log($"Searching on SO for: { _ordEntry.Grundform }...");

                var searcher = new SvenskaSearcher(SvenskaKälla.SO);

                await searcher.SearchAsync(_ordEntry.Grundform);

                if (searcher.LastResponse == WebscrappingSearcherResponse.WebException)
                {
                    var timespan = Resources.ConstantData.DagensOrd.IsDebugEnabled ? TimeSpan.FromSeconds(5) : TimeSpan.FromMinutes(5);

                    LogWarning($"Received a WebException. Will try again in { timespan.TotalSeconds } seconds.");

                    await Task.Delay(timespan);
                }
                else
                {
                    if (searcher.LastResponse == WebscrappingSearcherResponse.Successful)
                    {
                        var convertedResult = (SvenskaSearchResult)searcher.LastResult;

                        if (convertedResult.OrdEntries != null && convertedResult.OrdEntries.Count != 0)
                        {
                            Log($"Success! Dagensord is now: { _ordEntry.Grundform }");

                            _ordEntry = convertedResult.OrdEntries.First();

                            resultFound = true;
                        }
                        else
                        {
                            var delay = Math.Max(MinDelayBetweenSearches, Resources.ConstantData.DagensOrd.MSDelayBetweenSearches);

                            Log($"Failed to fetch entry from SO. Trying again in { delay }ms...");

                            GetRandomWord();

                            await Task.Delay(delay);
                        }
                    }
                }                
            }

            using (var sw = File.CreateText(Path))
            {
                sw.Write(JsonConvert.SerializeObject(_ordEntry, Formatting.Indented));
            }
        }

        private void GetRandomWord()
        {
            var ordbok = Resources.RuntimeData.FolketsOrdbok;

            var r = new Random().Next(0, ordbok.Words.Count - 1);

            _ordEntry = ordbok.Words[r];

            Log($"Seeding new word from Folketsordbok... New word is: { _ordEntry.Grundform }.");
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
