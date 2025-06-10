using System;
using System.Collections.Generic;

namespace RecNet
{
	public class Message : IRecNetObject
	{
		public enum MessageType
		{
			GameInvite = 0,
			GameInviteDeclined = 1,
			GameJoinFailed = 2,
			PartyActivitySwitch = 3,
			FriendInvite = 4,
			VoteToKick = 5
		}

		public long Id { get; private set; }

		public ulong FromPlayerId { get; private set; }

		public DateTime SentTime { get; private set; }

		public MessageType Type { get; private set; }

		public string Data { get; private set; }

		public object Details { get; private set; }

		public void Deserialize(Dictionary<string, object> dict)
		{
			Id = Util.GetKey<long>("Id", dict);
			FromPlayerId = Util.GetKey<ulong>("FromPlayerId", dict);
			SentTime = Util.GetDateTimeKey("SentTime", dict);
			Type = (MessageType)Util.GetKey<int>("Type", dict);
			Data = Util.GetKey<string>("Data", dict);
			if (Type == MessageType.GameJoinFailed)
			{
				Details = int.Parse(Data);
			}
		}

		public Dictionary<string, object> Serialize()
		{
			throw new NotImplementedException();
		}
	}
}
