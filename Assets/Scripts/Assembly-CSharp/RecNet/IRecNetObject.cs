using System.Collections.Generic;

namespace RecNet
{
	public interface IRecNetObject
	{
		void Deserialize(Dictionary<string, object> dict);

		Dictionary<string, object> Serialize();
	}
}
