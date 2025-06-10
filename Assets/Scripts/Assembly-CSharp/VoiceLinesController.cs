using System;
using System.Collections;
using Photon;
using UnityEngine;

[ExecuteInEditMode]
public class VoiceLinesController : Photon.MonoBehaviour
{
	[Serializable]
	public struct VoiceLineConfig
	{
		public VoiceLineControl lineControl;

		[Range(0f, 1f)]
		public float threshold;
	}

	[SerializeField]
	private Player thisPlayer;

	[SerializeField]
	private Transform voiceLinesAnchor;

	[Tooltip("We run a coroutine to make sure the icon doesn't flicker badly. This is its TTL")]
	[SerializeField]
	private float timeToDisableAnchor = 0.2f;

	[SerializeField]
	private VoiceLineConfig[] voiceLines;

	[SerializeField]
	private VoiceLineControl.VoiceLineControlConfig voiceLineConfig;

	[SerializeField]
	[Range(0f, 20f)]
	private float minVisualizationDistance = 1.5f;

	private bool _isTalking;

	public bool IsTalking
	{
		get
		{
			return _isTalking;
		}
		set
		{
			_isTalking = value;
			if (value && !thisPlayer.isLocal && base.gameObject.activeInHierarchy)
			{
				voiceLinesAnchor.gameObject.SetActive(_isTalking);
				StopAllCoroutines();
				StartCoroutine(StartNewFadeOutRoutine(voiceLinesAnchor.gameObject));
			}
		}
	}

	private IEnumerator StartNewFadeOutRoutine(GameObject obj)
	{
		yield return new WaitForSeconds(timeToDisableAnchor);
		obj.SetActive(false);
	}

	protected override void Awake()
	{
		base.Awake();
		VoiceLineConfig[] array = voiceLines;
		for (int i = 0; i < array.Length; i++)
		{
			VoiceLineConfig voiceLineConfig = array[i];
			voiceLineConfig.lineControl.gameObject.SetActive(false);
		}
	}

	protected void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;
		Gizmos.color = Color.grey;
		Gizmos.DrawWireSphere(thisPlayer.Head.transform.position, minVisualizationDistance);
		Gizmos.color = color;
	}

	private void Update()
	{
		Transform transform = Player.LocalPlayer.Head.transform;
		float num = Vector3.Distance(thisPlayer.Head.transform.position, Player.LocalPlayer.Head.transform.position);
		if (num < minVisualizationDistance || !(thisPlayer.AudioMaxForFrame > PhotonVoiceSettings.Instance.VoiceDetectionThreshold))
		{
			return;
		}
		float num2 = Mathf.InverseLerp(PhotonVoiceSettings.Instance.VoiceDetectionThreshold, 1f, thisPlayer.AudioMaxForFrame);
		for (int i = 0; i < voiceLines.Length; i++)
		{
			bool flag = num2 >= voiceLines[i].threshold;
			VoiceLineControl lineControl = voiceLines[i].lineControl;
			if (flag)
			{
				lineControl.gameObject.SetActive(flag);
				lineControl.Config = voiceLineConfig;
				lineControl.SetLifespan(Time.time + voiceLineConfig.TTL);
			}
		}
	}
}
