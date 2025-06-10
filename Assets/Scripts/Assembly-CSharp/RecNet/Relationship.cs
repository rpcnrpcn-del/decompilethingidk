using System.Collections.Generic;

namespace RecNet
{
	public class Relationship : IRecNetObject
	{
		public enum RelationshipType
		{
			None = 0,
			FriendRequestSent = 1,
			FriendRequestReceived = 2,
			Friend = 3,
			BlockedLocal = 4,
			BlockedRemote = 5,
			BlockedMutual = 6
		}

		public ulong PlayerID { get; private set; }

		public RelationshipType Type { get; private set; }

		public void Deserialize(Dictionary<string, object> dict)
		{
			PlayerID = Util.GetKey<ulong>("PlayerID", dict);
			Type = (RelationshipType)Util.GetKey<int>("RelationshipType", dict);
		}

		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("PlayerID", PlayerID);
			dictionary.Add("RelationshipType", (int)Type);
			return dictionary;
		}
	}
}
