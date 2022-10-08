using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamCalc.APIKey;


#pragma warning disable SYSLIB0014
#pragma warning disable CS8602

namespace SteamCalc
{ 
	public static class SteamCalculator
	{
		public const string APIKey = ProtectedAPIKey.APIKey;

		public const string SteamGamesAPILink = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1?key={APIKey}&steamid=76561198871868188";
		public const string SteamLevelAPILink = $"https://api.steampowered.com/IPlayerService/GetSteamLevel/v1?key={APIKey}&steamid=76561198871868188";

        static WebClient wc = new();

		//internal static string? steamID;
		public static void Main()
		{
			//steamID = Console.ReadLine();

			byte[] SteamGamesAPIResponse = wc.DownloadData(SteamGamesAPILink);
			byte[] SteamLevelAPIResponse = wc.DownloadData(SteamLevelAPILink);


            string SteamLevelAPIResponseString = Encoding.Default.GetString(SteamLevelAPIResponse);
			string SteamGamesAPIResponseString = Encoding.Default.GetString(SteamGamesAPIResponse);

            var levelRoot = JToken.Parse(SteamLevelAPIResponseString);

			var level = levelRoot
				.SelectToken("response")
				.SelectToken("player_level");


			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine($"Player level: {level}");
        }
    }
}


