namespace ExitGames.Client.Photon.Voice
{
	internal interface IVoiceFrontend : IVoiceActions
	{
		int DebugLostPercent { get; set; }

		void DebugReturn(DebugLevel level, string message);

		bool IsChannelJoined(int channelId);

		void SendVoicesInfo(object content, int channelId, int targetPlayerId);

		void SendVoiceRemove(object content, int channelId);

		void SendFrame(object content, int channelId, byte audioGroup);

		string ChannelIdStr(int channelId);

		string PlayerIdStr(int playerId);
	}
}
