using System.Collections.Generic;

namespace RecNet
{
	public class Setting : IRecNetObject
	{
		public string Key { get; set; }

		public string Value { get; set; }

		public void Deserialize(Dictionary<string, object> dict)
		{
			Key = Util.GetKey<string>("Key", dict);
			Value = Util.GetKey<string>("Value", dict);
		}

		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Key", Key);
			dictionary.Add("Value", Value);
			return dictionary;
		}
	}
}
