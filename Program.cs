using System;
using System.Net;
using System.Collections;
using System.Text;

#pragma warning disable SYSLIB0014
#pragma warning disable CS8602

namespace SteamCalc
{ 
	public static class SteamCalculator
	{
		static WebClient wc = new();

		internal static string? username;
		public static void Main()
		{
			username = Console.ReadLine();

			string link = "https://steamcommunity.com/id/" + username + "/games/?tab=all";

			byte[] htmlContent = wc.DownloadData(link);

			char[] htmlChars = Encoding.Default.GetString(htmlContent).ToCharArray();

			string htmlContentSTR = String.Join("", htmlChars);

			List<string> html_quotes = WebBrowser.HtmlGetEveryObjectInQuotes(htmlChars);

			Console.WriteLine(htmlContentSTR);
			Console.ReadKey();
		}
	}
	public static class WebBrowser
	{
		public static bool Get(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "GET";
			request.Timeout = 5000;
			request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";

			bool success = true;

			try
			{
				using (var response = request.GetResponse() as HttpWebResponse)
				{
					if (response.StatusCode == HttpStatusCode.OK)
					{
						success = true;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				success = false;
			}

			request.Abort();
			return success;
		}
		public static List<string> HtmlGetEveryObjectInQuotes(char[] htmlChars)
		{
			List<string> links = new List<string>();
			string link = "";
			bool afterQuote = false;
			foreach (char ch in htmlChars)
			{
				if (ch == '"')
				{
					afterQuote = !afterQuote;

					if (!afterQuote)
					{
						links.Add(link);
						link = "";
					}
				}
				else if (afterQuote)
				{
					link += ch;
				}
			}
			return links;
		}
	}
}

