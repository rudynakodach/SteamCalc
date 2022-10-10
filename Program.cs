using System;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
#pragma warning disable SYSLIB0014
#pragma warning disable CS8602

namespace SteamCalc
{
    public static class SteamCalculator
    {
        private static bool startupMessage = true;
        internal static string KeyFileDirectory = Directory.GetCurrentDirectory() + @"/SteamCalc/";
        internal static string saveFilename = "APIKey.dat";

        internal static int totalPlaytime = 0;
        public static string? APIKey;

        static WebClient wc = new();

        internal static string? steamID;

        internal static string? Username;
        internal static string? RealName;
        internal static string? CountryCode;
        internal static string? GamesOwned;
        internal static string? LastPlayedGame;

        public static bool ChangeAPIKey = false;

        public static async Task Main()
        {
            if (File.Exists(KeyFileDirectory + saveFilename))
            {
                using (StreamReader sr = new(KeyFileDirectory + saveFilename))
                {
                    APIKey = sr.ReadLine();
                    sr.Close();
                }
            }
            else
            {
                Directory.CreateDirectory(KeyFileDirectory);
                using (FileStream fs = new(KeyFileDirectory + saveFilename, FileMode.Create))
                using (StreamWriter sw = new(fs))
                {
                    if(!ChangeAPIKey)
                        Console.Write("API key not found! You can get one from https://steamcommunity.com/dev/apikey\nEnter your API key: ");
                    else
                    {
                        Console.Write("You can get your API key from https://steamcommunity.com/dev/apikey \nEnter your API key: ");
                        ChangeAPIKey = false;
                    }
                    string SteamAPIKey = Console.ReadLine();

                    sw.Write(SteamAPIKey);
                    sw.Close();
                    APIKey = SteamAPIKey;
                }
            }

            if (startupMessage)
            {
                startupMessage = false;
                Console.WriteLine($"Your API Key: {APIKey}\nType \"-edit apikey\" to edit your API key. ");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("SteamID or vanity URL: ");
            steamID = Console.ReadLine();
            Console.Clear();

            if (steamID.ToLower() == "-edit apikey")
            {
                File.Delete(KeyFileDirectory + saveFilename);
                ChangeAPIKey = true;
                await Main();
            }
            if (steamID.Contains("steamcommunity.com/id"))
            {
                Console.WriteLine("\nVanity URL detected! Getting steamID from API...\n");
                string[] strings = steamID.Split("/");

                steamID = strings[strings.Length - 2];

                Console.WriteLine("Current Link: " + steamID);

                string ResolveVanityURLLink = $"https://api.steampowered.com/ISteamUser/ResolveVanityURL/v1?key={APIKey}&steamid&vanityurl={steamID}";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("GET ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(ResolveVanityURLLink + "\n");
                Console.ForegroundColor = ConsoleColor.White;
                byte[] ResolveVanityURLApiResponse = wc.DownloadData(ResolveVanityURLLink);

                string ResolveVanityURLString = Encoding.Default.GetString(ResolveVanityURLApiResponse);

                var ResolveVanityURLRoot = JToken.Parse(ResolveVanityURLString);

                var GetSteamID = ResolveVanityURLRoot
                   .SelectToken("response")
                   .SelectToken("steamid");

                steamID = GetSteamID.ToString();
                Console.WriteLine("Returned steamID: {0}", steamID);

            }
            

            if (String.IsNullOrWhiteSpace(steamID))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("SteamID cannot be empty!");
                Console.ForegroundColor = ConsoleColor.White;
                await Main();
            }

            try
            {
                string SteamGamesAPILink = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1?key={APIKey}&steamid={steamID}";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("GET ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(SteamGamesAPILink + "");
                byte[] SteamGamesAPIResponse = wc.DownloadData(SteamGamesAPILink);

                string SteamLevelAPILink = $"https://api.steampowered.com/IPlayerService/GetSteamLevel/v1?key={APIKey}&steamid={steamID}";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\nGET ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(SteamLevelAPILink + "\n");
                byte[] SteamLevelAPIResponse = wc.DownloadData(SteamLevelAPILink);

                string PlayerInfoAPILink = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2?key={APIKey}&steamids={steamID}";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("GET ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(PlayerInfoAPILink + "\n");
                byte[] PlayerInfoAPIResponse = wc.DownloadData(PlayerInfoAPILink);

                string LastPlayedGameAPILink = $"https://api.steampowered.com/IPlayerService/GetRecentlyPlayedGames/v1?key={APIKey}&steamid={steamID}&count=1";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("GET ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(LastPlayedGameAPILink + "\n");
                byte[] LastPlayedGameAPIResponse = wc.DownloadData(LastPlayedGameAPILink);

                string LastPlayedGameAPIResponseString = Encoding.Default.GetString(LastPlayedGameAPIResponse);
                string PlayerInfoAPIResponseString = Encoding.Default.GetString(PlayerInfoAPIResponse);
                string SteamLevelAPIResponseString = Encoding.Default.GetString(SteamLevelAPIResponse);
                string SteamGamesAPIResponseString = Encoding.Default.GetString(SteamGamesAPIResponse);


                var lastPlayedGameRoot = JToken.Parse(LastPlayedGameAPIResponseString);
                var levelRoot = JToken.Parse(SteamLevelAPIResponseString);
                var gamesRoot = JToken.Parse(SteamGamesAPIResponseString);
                var infoRoot = JToken.Parse(PlayerInfoAPIResponseString);

                var playtimeRoot = gamesRoot
                    .SelectToken("response")
                    .SelectToken("games");

                var level = levelRoot
                    .SelectToken("response")
                    .SelectToken("player_level");

                var userRoot = infoRoot
                    .SelectToken("response")
                    .SelectToken("players");

                var lastPlayedGame = lastPlayedGameRoot
                    .SelectToken("response")
                    .SelectToken("games");


                var gameCountVar = gamesRoot
                    .SelectToken("response")
                   .SelectToken("game_count").ToString();

                GamesOwned = gameCountVar;

                try
                {
                    foreach (JToken token in lastPlayedGame)
                    {
                        LastPlayedGame = token.SelectToken("name").ToString();
                    }
                }
                catch
                {
                    LastPlayedGame = "Not Provided.";
                }

                foreach (JToken token in userRoot)
                {
                    Username = token.SelectToken("personaname").ToString();
                }

                try
                {
                    foreach (JToken token in userRoot)
                    {
                        RealName = token.SelectToken("realname").ToString();
                    }
                }
                catch
                {
                    RealName = "Not provided.";
                }

                try
                {
                    foreach (JToken token in userRoot)
                    {
                        CountryCode = token.SelectToken("loccountrycode").ToString();
                    }
                }
                catch
                {
                    CountryCode = "Not Provided.";
                }

                try
                {
                    foreach (JToken token in playtimeRoot)
                    {
                        var playtimeKeyForCurrentGame = token.SelectToken("playtime_forever").ToString();

                        totalPlaytime += int.Parse(playtimeKeyForCurrentGame);
                    }
                }
                catch
                {
                    Console.WriteLine("Error occured when iteratimg in JSON!");
                    await Main();
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n" +
                    "----------------------------------------" +
                    "\n");
                Console.ForegroundColor = ConsoleColor.White;


                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{Username} | {CountryCode}");
                Console.WriteLine(RealName);
                Console.WriteLine($"Player level: {level}");
                Console.WriteLine($"{GamesOwned} games owned.");
                Console.WriteLine($"Total Playtime: {totalPlaytime / 60} Hours / {totalPlaytime} Mins / {totalPlaytime * 60} Secs");
                Console.WriteLine($"Recently played game: {LastPlayedGame}");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n" +
                    "----------------------------------------" +
                    "\n");
                Console.ForegroundColor = ConsoleColor.White;

                totalPlaytime = 0;
            }
            catch (Exception e)
            {
                if (e.Message == "The remote server returned an error: (500) Internal Server Error.")
                {
                    Console.WriteLine("Please provide a working API key or check the SteamID");
                }
                else
                {
                    Console.WriteLine(e.Message);
                }
            }
            await Main();
        }
    }
}