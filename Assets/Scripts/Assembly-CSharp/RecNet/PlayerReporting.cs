using System.Collections.Generic;
using UnityEngine;

namespace RecNet
{
	public class PlayerReporting
	{
		public enum ReportCategory
		{
			Unknown = 0,
			MicrophoneAbuse = 1,
			Harassment = 2,
			Cheating = 3,
			ImmatureBehavior = 4
		}

		private const string REPORTING_API = "api/PlayerReporting/";

		private static List<ulong> reportedPlayerIds = new List<ulong>();

		public static bool CreateReport(ulong playerId, ReportCategory reportCategory)
		{
			if (!reportedPlayerIds.Contains(playerId))
			{
				reportedPlayerIds.Add(playerId);
				string requestUri = string.Format("{0}v1/create", "api/PlayerReporting/");
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("PlayerIdReported", playerId.ToString());
				int num = (int)reportCategory;
				dictionary.Add("ReportCategory", num.ToString());
				dictionary.Add("Activity", RecRoomSceneManager.CurrentSceneName);
				Core.Post(requestUri, dictionary, delegate(string wwwError)
				{
					if (!string.IsNullOrEmpty(wwwError))
					{
						Debug.LogError("Failed to report player" + wwwError);
						reportedPlayerIds.Remove(playerId);
					}
					else
					{
						AnalyticsHelper.ReportPlayer(playerId, reportCategory);
					}
				});
				return true;
			}
			return false;
		}
	}
}
