using System;
using System.Collections.Generic;

namespace RecNet
{
	public class GameSession : IRecNetObject
	{
		public string Id { get; set; }

		public string AppVersion { get; set; }

		public string Activity { get; set; }

		public bool Private { get; set; }

		public int AvailableSpace { get; set; }

		public bool GameInProgress { get; set; }

		public List<ulong> PlayerIds { get; set; }

		public void Deserialize(Dictionary<string, object> dict)
		{
			Id = Util.GetKey<string>("Id", dict);
			AppVersion = Util.GetKey<string>("AppVersion", dict);
			Activity = Util.GetKey<string>("Activity", dict);
			Private = Util.GetKey<bool>("Private", dict);
			AvailableSpace = Util.GetKey<int>("AvailableSpace", dict);
			GameInProgress = Util.GetKey<bool>("GameInProgress", dict);
			List<object> key = Util.GetKey<List<object>>("PlayerIds", dict);
			PlayerIds = new List<ulong>(key.Count);
			foreach (object item in key)
			{
				PlayerIds.Add((ulong)Convert.ChangeType(item, typeof(ulong)));
			}
		}

		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Id", Id);
			dictionary.Add("AppVersion", AppVersion);
			dictionary.Add("Activity", Activity);
			dictionary.Add("Private", Private);
			dictionary.Add("AvailableSpace", AvailableSpace);
			dictionary.Add("GameInProgress", GameInProgress);
			dictionary.Add("PlayerIds", PlayerIds);
			return dictionary;
		}
	}
}
