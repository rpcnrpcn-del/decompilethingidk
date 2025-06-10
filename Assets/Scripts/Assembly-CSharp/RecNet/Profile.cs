using System.Collections.Generic;

namespace RecNet
{
	public class Profile : IRecNetObject
	{
		public ulong Id { get; private set; }

		public string Username { get; set; }

		public string DisplayName { get; set; }

		public int XP { get; set; }

		public int XpRequiredToLevelUp { get; set; }

		public int Level { get; set; }

		public int Reputation { get; set; }

		public bool Verified { get; set; }

		public bool Developer { get; set; }

		public void Deserialize(Dictionary<string, object> dict)
		{
			Id = Util.GetKey<ulong>("Id", dict);
			Username = Util.GetKey<string>("Username", dict);
			DisplayName = Util.GetKey<string>("DisplayName", dict);
			XP = Util.GetKey<int>("XP", dict);
			Level = Util.GetKey<int>("Level", dict);
			Reputation = Util.GetKey<int>("Reputation", dict);
			Verified = Util.GetKey<bool>("Verified", dict);
			Developer = Util.GetKey<bool>("Developer", dict);
		}

		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Id", Id);
			dictionary.Add("Username", Username);
			dictionary.Add("DisplayName", DisplayName);
			dictionary.Add("XP", XP);
			dictionary.Add("Level", Level);
			dictionary.Add("Reputation", Reputation);
			dictionary.Add("Verified", Verified);
			return dictionary;
		}
	}
}
