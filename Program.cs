using System;
using System.Threading.Tasks;
using System.IO;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace svenskabot
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextModule commands;

        static void Main(string[] args)
        {
            MainAsync(GetToken()).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string token)
        {
            discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "!",
            });

            commands.RegisterCommands<Commands>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        static string GetToken()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "token";

            if (File.Exists(path))
            {
                using (var file = File.OpenText(path))
                {
                    var token = file.ReadToEnd();
                    Console.WriteLine("Token was loaded from file.");
                    return token;
                }
            }
            else
            {
                Console.WriteLine("Please enter a token:");

                var token = Console.ReadLine();

                using (var sw = File.CreateText(path))
                {
                    sw.Write(token);
                }

                Console.WriteLine("Token was saved to file.");

                return token;
            }
        }
    }

    public static class DiscordTextFormatter
    {
        public static string ToBold(this string aString) { return $"**{ aString }**"; }
        public static string ToBoldAndItalics(this string aString) { return $"***{ aString }***"; }
        public static string ToItalics(this string aString) { return $"*{ aString }*"; }
    }

    public static class DiscordEmbedBuilderEx
    {
        // See here: https://support.discord.com/hc/en-us/community/posts/360056286551--BUG-Infinite-Scroll-Embed-Android-
        public static void HackFixWidth(this DiscordEmbedBuilder discordEmbedBuilder)
        {
            discordEmbedBuilder.WithDescription("------------------------------------------------------");
        }
    }
}
