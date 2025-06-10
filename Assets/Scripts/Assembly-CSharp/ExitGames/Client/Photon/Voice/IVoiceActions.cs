using System;

namespace ExitGames.Client.Photon.Voice
{
	internal interface IVoiceActions
	{
		Action<int, int, byte, VoiceInfo> OnRemoteVoiceInfoAction { get; set; }

		Action<int, int, byte> OnRemoteVoiceRemoveAction { get; set; }

		Action<int, int, byte, float[]> OnAudioFrameAction { get; set; }
	}
}
