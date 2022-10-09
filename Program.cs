using System;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using SteamCalc.APIKey;
using System.IO;
using System.Threading.Tasks;
#pragma warning disable SYSLIB0014
#pragma warning disable CS8602
#pragma warning disable SYSLIB0011

namespace SteamCalc
{
    public static class SteamCalculator
    {
        private static bool startupMessage = true;
        internal static string KeyFileDirectory = Directory.GetCurrentDirectory() + @"/SteamCalc/";
        internal static string saveFilename = "APIKey.dat";


        internal static int totalPlaytime = 0;
        public static string? APIKey;
        public const string APIKey2 = ProtectedAPIKey.APIKey;

        static WebClient wc = new();

        internal static string? steamID;

        internal static string? Username;
        internal static string? RealName;
        internal static string? CountryCode;
        internal static string? GamesOwned;

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
                //File.Create(KeyFileDirectory + saveFilename);
                using (FileStream fs = new(KeyFileDirectory + saveFilename, FileMode.Create))
                using (StreamWriter sw = new(fs))
                {
                    Console.Write("API Key not found! You can get one from https://steamcommunity.com/dev/apikey\nEnter your API Key: ");
                    string SteamAPIKey = Console.ReadLine();

                    sw.Write(SteamAPIKey);
                    sw.Close();
                    APIKey = SteamAPIKey;
                }
            }

            if(startupMessage)
            {
                startupMessage = false;
                Console.WriteLine($"Your API Key: {APIKey}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("SteamID: ");
            steamID = Console.ReadLine();


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
                Console.Write(SteamGamesAPILink);
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

                string PlayerInfoAPIResponseString = Encoding.Default.GetString(PlayerInfoAPIResponse);
                string SteamLevelAPIResponseString = Encoding.Default.GetString(SteamLevelAPIResponse);
                string SteamGamesAPIResponseString = Encoding.Default.GetString(SteamGamesAPIResponse);

                var levelRoot = JToken.Parse(SteamLevelAPIResponseString);
                var gamesRoot = JToken.Parse(SteamGamesAPIResponseString);
                var infoRoot = JToken.Parse(PlayerInfoAPIResponseString);

#pragma warning disable CS8604
                var playtimeRoot = gamesRoot
                    .SelectToken("response")
                    .SelectToken("games");
#pragma warning restore CS8604

                var level = levelRoot
                    .SelectToken("response")
                    .SelectToken("player_level");

                var userRoot = infoRoot
                    .SelectToken("response")
                    .SelectToken("players");

                

                foreach(JToken token in userRoot)
                {
                    RealName = token.SelectToken("realname").ToString();
                    Username = token.SelectToken("personaname").ToString();
                    CountryCode = token.SelectToken("loccountrycode").ToString();
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

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{Username} | {CountryCode}");
                Console.WriteLine(RealName);
                Console.WriteLine($"Player level: {level}");
                Console.WriteLine($"Playtime: {totalPlaytime / 60} Hours / {totalPlaytime} Mins / {totalPlaytime * 60} Secs");

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