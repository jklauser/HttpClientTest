using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Win32;

namespace WebVersion.Controllers
{
	public class HomeController : Controller
	{
		private static string gabesSystemUrl = "https://wfm-tm-jotunngabe.nonprod.apihc.com/jott103/tm/live/portal/Login.aspx";
		private static string phoenixServiceUrl = "https://wfm.apihc.com/newc999/tm/live/web-services/message-server/ping";

		public ActionResult Index()
		{
			ViewBag.Title = "Home Page";

			ViewBag.FrameworkVersion = HomeController.Get45PlusFromRegistry();

			// Best practice to use HttpClient for as long as possible.
			HttpClient httpClient = new HttpClient();
			StringBuilder queryResults = new StringBuilder();

			queryResults.AppendLine(HomeController.TestHttpClient(httpClient, HomeController.gabesSystemUrl));
			queryResults.AppendLine(HomeController.TestHttpClient(httpClient, HomeController.phoenixServiceUrl));
			////queryResults.AppendLine(HomeController.TestWebClient(HomeController.gabesSystemUrl));

			ViewBag.QueryResults = queryResults.ToString();

			return View();
		}

		/// <summary>
		/// Don't do this at home.
		/// </summary>
		/// <param name="url">The url to try to hit.</param>
		private static string TestWebClient(string url)
		{
			WebClient webClient = new WebClient();
			StringBuilder resultText = new StringBuilder("WebClient | ");
			resultText.AppendFormat("{0}: ", url);
			try
			{
				string result = webClient.DownloadString(url);
				int lengthToDisplay = Math.Min(50, result.Length);
				if (lengthToDisplay == 0)
				{
					resultText.AppendLine("<No content>");
				}
				else
				{
					resultText.AppendLine(result.Substring(0, lengthToDisplay));
				}
			}
			catch (Exception e)
			{
				resultText.AppendLine("Exception thrown");
				resultText.AppendLine(e.ToString());
			}

			return resultText.ToString();
		}

		private static string TestHttpClient(HttpClient httpClient, string url)
		{
			StringBuilder resultText = new StringBuilder("HttpClient| ");
			resultText.AppendFormat("{0}: ", url);
			try
			{
				var requestResultTask = httpClient.GetAsync(url);
				resultText.Append(requestResultTask.Result.StatusCode);

				if (requestResultTask.Result.IsSuccessStatusCode)
				{
					resultText.Append(": Success :");
					string content = requestResultTask.Result.Content.ReadAsStringAsync().Result;
					int lengthToRead = Math.Min(content.Length, 50);
					if (lengthToRead == 0)
					{
						resultText.AppendLine("<No content>");
					}
					else
					{
						resultText.AppendLine(content.Substring(0, 50));
					}
				}
				else
				{
					resultText.AppendLine(": Fail");
				}
			}
			catch (Exception e)
			{
				resultText.AppendFormat(": XX : Fail : exception: {0}", e.ToString());
			}
			

			return resultText.ToString();
		}

		private static string Get45PlusFromRegistry()
		{
			const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

			using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
			{
				if (ndpKey != null && ndpKey.GetValue("Release") != null)
				{
					return ".NET Framework Version: " + CheckFor45PlusVersion((int)ndpKey.GetValue("Release"));
				}
				else
				{
					return ".NET Framework Version 4.5 or later is not detected.";
				}
			}
		}

		// Checking the version using >= will enable forward compatibility.
		private static string CheckFor45PlusVersion(int releaseKey)
		{
			if (releaseKey >= 461808)
				return "4.7.2 or later";
			if (releaseKey >= 461308)
				return "4.7.1";
			if (releaseKey >= 460798)
				return "4.7";
			if (releaseKey >= 394802)
				return "4.6.2";
			if (releaseKey >= 394254)
				return "4.6.1";
			if (releaseKey >= 393295)
				return "4.6";
			if (releaseKey >= 379893)
				return "4.5.2";
			if (releaseKey >= 378675)
				return "4.5.1";
			if (releaseKey >= 378389)
				return "4.5";
			// This code should never execute. A non-null release key should mean
			// that 4.5 or later is installed.
			return "No 4.5 or later version detected";
		}
	}
}
