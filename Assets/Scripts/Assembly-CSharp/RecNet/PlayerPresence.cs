using System;
using System.Collections.Generic;

namespace RecNet
{
	public class PlayerPresence : IRecNetObject
	{
		public ulong PlayerId { get; set; }

		public string GameSessionId { get; set; }

		public string AppVersion { get; set; }

		public DateTime LastUpdateTime { get; set; }

		public string Activity { get; set; }

		public bool Private { get; set; }

		public int AvailableSpace { get; set; }

		public bool GameInProgress { get; set; }

		public void Deserialize(Dictionary<string, object> dict)
		{
			PlayerId = Util.GetKey<ulong>("PlayerId", dict);
			GameSessionId = Util.GetKey<string>("GameSessionId", dict);
			AppVersion = Util.GetKey<string>("AppVersion", dict);
			LastUpdateTime = Util.GetDateTimeKey("LastUpdateTime", dict);
			Activity = Util.GetKey<string>("Activity", dict);
			Private = Util.GetKey<bool>("Private", dict);
			AvailableSpace = Util.GetKey<int>("AvailableSpace", dict);
			GameInProgress = Util.GetKey<bool>("GameInProgress", dict);
		}

		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("PlayerId", PlayerId);
			dictionary.Add("GameSessionId", GameSessionId);
			dictionary.Add("AppVersion", AppVersion);
			dictionary.Add("Activity", Activity);
			dictionary.Add("Private", Private);
			dictionary.Add("AvailableSpace", AvailableSpace);
			dictionary.Add("GameInProgress", GameInProgress);
			return dictionary;
		}
	}
}
