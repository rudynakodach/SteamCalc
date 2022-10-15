using System;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

#pragma warning disable SYSLIB0014
#pragma warning disable CS8602
#pragma warning disable CS8618
#pragma warning disable CS8604

namespace SteamCalc
{
	public static class SteamCalculator
	{
		private static bool startupMessage = true;
		internal static string KeyFileDirectory = Directory.GetCurrentDirectory() + @"/SteamCalc/";
		internal static string saveFilename = "APIKey.dat";

		internal static string totalPlaytime;
		internal static int totalPlaytimeInt;
		public static string? APIKey;

		static WebClient wc = new();

		internal static string? steamID;

		internal static string? Username;
		internal static string? RealName;
		internal static string? CountryCode;
		internal static string? GamesOwned;
		internal static string? LastPlayedGame;

		public static bool ChangeAPIKey = false;
        public static Stopwatch st = new();

        public static async Task Main()
		{

			Console.Title = "Steam Calculator - RudyNaKodach";
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
					if (!ChangeAPIKey)
						Console.Write("API key not found! You can get one from https://steamcommunity.com/dev/apikey\nEnter your API key: ");
					else
					{
						Console.Write("You can get your API key from https://steamcommunity.com/dev/apikey \nEnter your API key: ");
						ChangeAPIKey = false;
					}
#pragma warning disable CS8600
					string SteamAPIKey = Console.ReadLine();
#pragma warning restore CS8600
					sw.Write(SteamAPIKey);
					sw.Close();
					APIKey = SteamAPIKey;
				}
			}

			if (startupMessage)
			{
				startupMessage = false;
				Console.WriteLine($"Your API Key: {APIKey}\nType \"-edit apikey\" to edit your API key. \nYou can get someone's profile just by using their Vanity ID by using \"-v [VanityID]\".");
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

				var SteamIDRoot = ResolveVanityURLRoot
				   .SelectToken("response");

				if(SteamIDRoot.Contains("steamid"))
                {
					var GetSteamID = SteamIDRoot
					   .SelectToken("steamid");
                    steamID = GetSteamID.ToString();
                    Console.WriteLine("Returned steamID: {0}", steamID);
                }
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Incorrect Vanity ID porvided!");
					Console.ForegroundColor = ConsoleColor.White;
					await Main();
				}


			}
			else if (steamID.Contains("-v"))
			{
				try
				{
					string[] SlicedSteamID = steamID.Split(' ');
					string VanityID = SlicedSteamID[1];

					string ResolveVanityURLLink = $"https://api.steampowered.com/ISteamUser/ResolveVanityURL/v1?key={APIKey}&steamid&vanityurl={VanityID}";
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
				catch (Exception e2)
				{
					Console.WriteLine(e2.Message);
				}
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
				st.Start();
				string SteamGamesAPILink = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1?key={APIKey}&steamid={steamID}";
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("GET ");
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(SteamGamesAPILink + "");
				byte[] SteamGamesAPIResponse = wc.DownloadData(SteamGamesAPILink);
				st.Stop();
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("\nSUCCESS: ");
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"Operation completed in {0}ms", st.Elapsed);


				st.Reset();
				st.Start();
				string SteamLevelAPILink = $"https://api.steampowered.com/IPlayerService/GetSteamLevel/v1?key={APIKey}&steamid={steamID}";
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("\nGET ");
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(SteamLevelAPILink + "\n");
				byte[] SteamLevelAPIResponse = wc.DownloadData(SteamLevelAPILink);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("SUCCESS: ");
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"Operation completed in {0}ms\n", st.Elapsed);

				st.Reset();
				st.Start();
				string PlayerInfoAPILink = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2?key={APIKey}&steamids={steamID}";
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("GET ");
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(PlayerInfoAPILink + "\n");
				byte[] PlayerInfoAPIResponse = wc.DownloadData(PlayerInfoAPILink);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("SUCCESS: ");
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"Operation completed in {0}ms\n", st.Elapsed);

				st.Reset();
				st.Start();
				string LastPlayedGameAPILink = $"https://api.steampowered.com/IPlayerService/GetRecentlyPlayedGames/v1?key={APIKey}&steamid={steamID}&count=1";
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("GET ");
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write(LastPlayedGameAPILink + "\n");
				byte[] LastPlayedGameAPIResponse = wc.DownloadData(LastPlayedGameAPILink);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("SUCCESS: ");
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"Operation completed in {0}ms\n", st.Elapsed);

				Console.WriteLine("\nIterating through JSON keys...\n");
				st.Reset();
				st.Start();

				string LastPlayedGameAPIResponseString = Encoding.Default.GetString(LastPlayedGameAPIResponse);
				string PlayerInfoAPIResponseString = Encoding.Default.GetString(PlayerInfoAPIResponse);
				string SteamLevelAPIResponseString = Encoding.Default.GetString(SteamLevelAPIResponse);
				string SteamGamesAPIResponseString = Encoding.Default.GetString(SteamGamesAPIResponse);


				var lastPlayedGameRoot = JToken.Parse(LastPlayedGameAPIResponseString);
				var levelRoot = JToken.Parse(SteamLevelAPIResponseString);
				var gamesRoot = JToken.Parse(SteamGamesAPIResponseString);
				var infoRoot = JToken.Parse(PlayerInfoAPIResponseString);

				var playtimeRoot = gamesRoot
					.SelectToken("response");

				var playtimeString = playtimeRoot.ToString();

				bool playtimeAvaible;
				if (!playtimeString.ToString().Contains("games"))
				{
					playtimeAvaible = false;
				}
				else
				{
					playtimeAvaible = true;
					playtimeRoot = gamesRoot
						.SelectToken("response")
						.SelectToken("games");
				}


				var level = levelRoot
					.SelectToken("response")
					.SelectToken("player_level");

				var userRoot = infoRoot
					.SelectToken("response")
					.SelectToken("players");


				bool LastPlayedGameAvaible;
				var lastPlayedGame = lastPlayedGameRoot
					.SelectToken("response");

				if(lastPlayedGame.ToString().Contains("games"))
				{
					lastPlayedGame = lastPlayedGame
					   .SelectToken("games");
					LastPlayedGameAvaible = true;
                }
				else
				{
					LastPlayedGameAvaible = false;
					LastPlayedGame = "N/A";
				}


				if(LastPlayedGameAvaible)
				{
					foreach(JToken token in lastPlayedGame)
					{
                        LastPlayedGame = token
					      .SelectToken("name").ToString();
                    }
				}

				var gameCountVar = gamesRoot
					.SelectToken("response");

				if (gameCountVar.ToString() == @"{}")
				{
					GamesOwned = "N/A";
				}
				else
				{
					GamesOwned = gameCountVar
						.SelectToken("game_count").ToString();
				}

				try
				{
					foreach (JToken token in userRoot)
					{
						Username = token.SelectToken("personaname").ToString();
					}

					if (userRoot.ToString().Contains("realname"))
					{
						foreach (JToken token in userRoot)
						{
							RealName = token.SelectToken("realname").ToString();
						}
					}
					else
					{
						RealName = "N/A";
					}

					if (userRoot.ToString().Contains("loccountrycode"))
					{
						foreach(JToken token in userRoot)
						{
							CountryCode = token.SelectToken("loccountrycode").ToString();
						}
                    }
                    else
					{
                        CountryCode = "N/A";
					}

					if (playtimeAvaible)
					{
						if (string.IsNullOrEmpty(playtimeRoot.ToString()))
						{
							totalPlaytime = "N/A";
						}
						else
						{
							foreach (JToken token in playtimeRoot)
							{
								var playtimeKeyForCurrentGame = token.SelectToken("playtime_forever").ToString();

								totalPlaytimeInt += int.Parse(playtimeKeyForCurrentGame);
							}
							totalPlaytime = totalPlaytimeInt.ToString();
						}
					}
				}
				catch
				{
					Console.WriteLine("Error occured when iteratimg in JSON!\n\nPress any key to continue...");
					Console.ReadKey(true);
					await Main();
				}
				st.Stop();
				Console.WriteLine("Iteration completed in {0}ms", st.ElapsedMilliseconds);


				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\n" +
					"--------------------------------------------------------------------------------" +
					"\n");
				Console.ForegroundColor = ConsoleColor.White;


				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("   {0,-35} {1,35}", "Username: ", $"{Username} | {CountryCode}");
				Console.WriteLine("   {0,-35} {1,35}", $"Name: ", RealName);
				Console.WriteLine("   {0,-35} {1,35}", $"Player level: ", level);
				Console.WriteLine("   {0,-35} {1,35}", $"Games owned: ", GamesOwned);
				Console.WriteLine("   {0,-35} {1,35}", $"Total Playtime: ", $"{totalPlaytimeInt / 60} Hours / {totalPlaytime} Mins / {totalPlaytimeInt * 60} Secs");
				Console.WriteLine("   {0,-35} {1,35}", $"Recently played game: ", LastPlayedGame);

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\n" +
					"--------------------------------------------------------------------------------" +
					"\n");
				Console.ForegroundColor = ConsoleColor.White;

				totalPlaytimeInt = 0;

			}
			catch (Exception e)
			{
				if (e.Message == "The remote server returned an error: (500) Internal Server Error.")
				{
					Console.Beep();
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Please provide a working API key or check the SteamID");
					Console.ForegroundColor = ConsoleColor.White;
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(e.Message);
					Console.WriteLine("Does the user have a private profile?");
					Console.ForegroundColor = ConsoleColor.White;
				}
			}
			await Main();
		}
	}
}