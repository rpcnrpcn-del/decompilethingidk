using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MenuController
{
	[SerializeField]
	private Slider sfxSlider;

	[SerializeField]
	private Slider musicSlider;

	[SerializeField]
	private Slider voiceSlider;

	[SerializeField]
	private Slider teleportBufferSlider;

	[SerializeField]
	private Toggle motionTeleportToggle;

	[SerializeField]
	private Slider rotationIncrementSlider;

	[SerializeField]
	private Slider trackingModeSlider;

	[SerializeField]
	private Slider voiceFilterSlider;

	[SerializeField]
	private Slider voiceChatSlider;

	[SerializeField]
	private Slider qualitySettingSlider;

	[SerializeField]
	private Toggle showNames;

	[SerializeField]
	private Toggle personalBubble;

	[SerializeField]
	private Toggle h264PluginToggle;

	[SerializeField]
	private Slider flyingSpeedSlider;

	[SerializeField]
	private Slider continuousRotationSlider;

	[Header("Three Sixty Degree Tracking Settings")]
	[SerializeField]
	private GameObject showRoomCenterSetting;

	[SerializeField]
	private Toggle showRoomCenterToggle;

	[SerializeField]
	private GameObject rotateInPlaceEnabledSetting;

	[SerializeField]
	private Toggle rotateInPlaceToggle;

	public override void Initialize(PlayerMenu playerMenu)
	{
		base.Initialize(playerMenu);
		sfxSlider.value = SingletonMonoBehaviour<AudioManager>.Instance.SfxVolumePercentage;
		voiceSlider.value = SingletonMonoBehaviour<AudioManager>.Instance.VoiceVolumePercentage;
		musicSlider.value = SingletonMonoBehaviour<AudioManager>.Instance.MusicVolumePercentage;
		teleportBufferSlider.value = (float)SingletonMonoBehaviour<SettingsManager>.Instance.TeleportBuffer;
		motionTeleportToggle.isOn = SingletonMonoBehaviour<SettingsManager>.Instance.MotionTeleportEnabled;
		rotateInPlaceToggle.isOn = SingletonMonoBehaviour<SettingsManager>.Instance.RotateInPlaceEnabled;
		continuousRotationSlider.value = (float)SingletonMonoBehaviour<SettingsManager>.Instance.ContinuousRotationMode;
		rotationIncrementSlider.value = (float)SingletonMonoBehaviour<SettingsManager>.Instance.RotationIncrement;
		voiceFilterSlider.value = (float)SingletonMonoBehaviour<SettingsManager>.Instance.VoiceFilter;
		voiceChatSlider.value = (float)SingletonMonoBehaviour<SettingsManager>.Instance.VoiceChat;
		qualitySettingSlider.value = (float)SingletonMonoBehaviour<SettingsManager>.Instance.QualitySetting;
		showNames.isOn = SingletonMonoBehaviour<SettingsManager>.Instance.ShowNames;
		personalBubble.isOn = SingletonMonoBehaviour<SettingsManager>.Instance.PersonalBubble;
		h264PluginToggle.isOn = SingletonMonoBehaviour<SettingsManager>.Instance.H264Plugin;
		showRoomCenterToggle.isOn = SingletonMonoBehaviour<SettingsManager>.Instance.ShowRoomCenter;
		flyingSpeedSlider.value = LaserTeleporter.FlightSpeed;
		trackingModeSlider.value = (float)PlatformManager.Instance.CurrentTrackingMode;
	}

	protected override void OnDisable()
	{
		RecroomPrefs.Save();
		base.OnDisable();
	}

	public void MusicVolumeSliderValueChange(float value)
	{
		SingletonMonoBehaviour<AudioManager>.Instance.MusicVolumePercentage = value;
	}

	public void SFXVolumeSliderValueChange(float value)
	{
		SingletonMonoBehaviour<AudioManager>.Instance.SfxVolumePercentage = value;
	}

	public void VoiceVolumeSliderValueChange(float value)
	{
		SingletonMonoBehaviour<AudioManager>.Instance.VoiceVolumePercentage = value;
	}

	public void TeleportBufferSliderValueChange(float value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.TeleportBuffer = (TeleportBuffer)value;
	}

	public void RotateInPlaceToggleValueChanged(bool value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.RotateInPlaceEnabled = value;
	}

	public void RotationIncrementValueChanged(float value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.RotationIncrement = (PlayerLocomotion.RotationIncrement)value;
	}

	public void ContinuousRotationModeValueChanged(float value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.ContinuousRotationMode = (ContinuousRotationMode)value;
	}

	public void MotionTeleportToggleValueChanged(bool value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.MotionTeleportEnabled = value;
	}

	public void TrackingModeSliderValueChanged(float value)
	{
		PlatformManager.Instance.CurrentTrackingMode = (PlatformManager.TrackingMode)value;
		Refresh();
	}

	public void VoiceFilterValueChange(float value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.VoiceFilter = (AudioManager.VOIPFilter)value;
	}

	public void VoiceChatValueChange(float value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.VoiceChat = (VoiceChat)value;
	}

	public void QualitySettingValueChange(float value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.QualitySetting = (RecRoomQualitySetting)value;
	}

	public void ShowNamesValueChange(bool value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.ShowNames = value;
	}

	public void PersonalBubbleValueChange(bool value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.PersonalBubble = value;
	}

	public void H264PluginValueChange(bool value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.H264Plugin = value;
	}

	public void ShowRoomCenterValueChange(bool value)
	{
		SingletonMonoBehaviour<SettingsManager>.Instance.ShowRoomCenter = value;
	}

	public void FlyingSpeedSliderValueChange(float value)
	{
		LaserTeleporter.FlightSpeed = value;
	}

	public override void Refresh()
	{
		base.Refresh();
		bool active = PlatformManager.Instance.CurrentTrackingMode == PlatformManager.TrackingMode.THREE_SIXTY_DEGREE;
		showRoomCenterSetting.SetActive(active);
		rotateInPlaceEnabledSetting.SetActive(active);
	}
}
