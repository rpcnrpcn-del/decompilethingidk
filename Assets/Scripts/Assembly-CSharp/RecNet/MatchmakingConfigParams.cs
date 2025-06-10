using System.Collections.Generic;

namespace RecNet
{
	public class MatchmakingConfigParams : IRecNetObject
	{
		public float PreferFullRoomsFrequency { get; set; }

		public float PreferEmptyRoomsFrequency { get; set; }

		public void Deserialize(Dictionary<string, object> dict)
		{
			PreferFullRoomsFrequency = Util.GetKey<float>("PreferFullRoomsFrequency", dict);
			PreferEmptyRoomsFrequency = Util.GetKey<float>("PreferEmptyRoomsFrequency", dict);
		}

		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("PreferFullRoomsFrequency", PreferFullRoomsFrequency);
			dictionary.Add("PreferEmptyRoomsFrequency", PreferEmptyRoomsFrequency);
			return dictionary;
		}
	}
}
