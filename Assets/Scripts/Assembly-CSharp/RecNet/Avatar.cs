using System.Collections.Generic;

namespace RecNet
{
	public class Avatar : IRecNetObject
	{
		public string OutfitSelections { get; set; }

		public string SkinColor { get; set; }

		public string HairColor { get; set; }

		public void Deserialize(Dictionary<string, object> dict)
		{
			OutfitSelections = Util.GetKey<string>("OutfitSelections", dict);
			HairColor = Util.GetKey<string>("HairColor", dict);
			SkinColor = Util.GetKey<string>("SkinColor", dict);
		}

		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("OutfitSelections", OutfitSelections);
			dictionary.Add("SkinColor", SkinColor);
			dictionary.Add("HairColor", HairColor);
			return dictionary;
		}
	}
}
