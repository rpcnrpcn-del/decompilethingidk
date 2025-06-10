using UnityEngine;

public static class DefaultAudioSettings
{
	private static DefaultAudioConfig ConfigAsset = CustomSettingsLoader.LoadConfig<DefaultAudioConfig>();

	private const string ConfigsAssetFilePath = "Assets/Core/Content/Configs/";

	public static bool TryLoadDefaultAudioClips(FxType audioType, out AudioClips audioClips)
	{
		audioClips = null;
		if (ConfigAsset != null)
		{
			for (int i = 0; i < ConfigAsset.DefaultAudioClips.Length; i++)
			{
				if (ConfigAsset.DefaultAudioClips[i].AudioType == audioType)
				{
					audioClips = new AudioClips();
					audioClips.SoundEffect = LoadRecRoomAudioClip(ConfigAsset.DefaultAudioClips[i].SoundEffectClip);
					audioClips.VoiceOver = LoadRecRoomAudioClip(ConfigAsset.DefaultAudioClips[i].VoiceOverClip);
					return true;
				}
			}
		}
		return false;
	}

	private static RecRoomAudioClip LoadRecRoomAudioClip(RecRoomAudioClip resourcesAsset)
	{
		RecRoomAudioClip recRoomAudioClip = null;
		if (resourcesAsset != null && resourcesAsset.audioClip != null)
		{
			recRoomAudioClip = new RecRoomAudioClip();
			recRoomAudioClip.audioClip = (AudioClip)Resources.Load(resourcesAsset.audioClip.name, typeof(AudioClip));
			recRoomAudioClip.pitchVariation = resourcesAsset.pitchVariation;
			recRoomAudioClip.volume = resourcesAsset.volume;
		}
		return recRoomAudioClip;
	}
}
