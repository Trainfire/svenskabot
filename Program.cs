﻿using System;
using System.Threading.Tasks;
using System.IO;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Net;

namespace svenskabot
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextModule commands;

        static void Main(string[] args)
        {
            Resources.Initialize();
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

            discord.AddModule(new DagensOrdModule());

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
                Console.WriteLine("No token found.");
                Console.WriteLine("If you don't have one, you can find information about creating one at: https://discord.com/developers/docs/topics/oauth2#bots");
                Console.WriteLine("Otherwise enter it now:");

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
}
