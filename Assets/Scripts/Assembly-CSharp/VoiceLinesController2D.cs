using System;
using System.Collections;
using Photon;
using UnityEngine;

public class VoiceLinesController2D : Photon.MonoBehaviour
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
	private VoiceLineConfig shoutConfig;

	[SerializeField]
	private VoiceLineControl.VoiceLineControlConfig voiceLineConfig;

	[SerializeField]
	[Range(0f, 20f)]
	private float minVisualizationDistance = 1.5f;

	[SerializeField]
	[Range(0f, 180f)]
	private float maxAngleDiff = 45f;

	[SerializeField]
	[Range(0f, 180f)]
	private float offsetAngle = 45f;

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
		if (thisPlayer == null || Player.LocalPlayer == null || Player.LocalPlayer.IsSpawning)
		{
			return;
		}
		Transform transform = Player.LocalPlayer.Head.transform;
		float num = Vector3.Distance(thisPlayer.Head.transform.position, Player.LocalPlayer.Head.transform.position);
		if (num < minVisualizationDistance)
		{
			return;
		}
		Vector3 vector = thisPlayer.Head.transform.position - transform.position;
		if (thisPlayer.AudioMaxForFrame > PhotonVoiceSettings.Instance.VoiceDetectionThreshold)
		{
			float num2 = Mathf.InverseLerp(PhotonVoiceSettings.Instance.VoiceDetectionThreshold, 1f, thisPlayer.AudioMaxForFrame);
			if (num2 >= shoutConfig.threshold)
			{
				shoutConfig.lineControl.gameObject.SetActive(true);
				shoutConfig.lineControl.Config = voiceLineConfig;
				shoutConfig.lineControl.SetLifespan(Time.time + voiceLineConfig.TTL);
			}
			for (int num3 = voiceLines.Length - 1; num3 >= 0; num3--)
			{
				bool flag = num2 >= voiceLines[num3].threshold;
				VoiceLineControl lineControl = voiceLines[num3].lineControl;
				if (shoutConfig.lineControl.gameObject.activeInHierarchy)
				{
					lineControl.SetLifespan(0f);
				}
				else if (flag)
				{
					lineControl.gameObject.SetActive(flag);
					lineControl.Config = voiceLineConfig;
					lineControl.SetLifespan(Time.time + voiceLineConfig.TTL);
				}
			}
		}
		Vector3 forward = transform.forward;
		float num4 = Vector3.Angle(thisPlayer.Head.transform.right, vector.normalized);
		voiceLinesAnchor.localPosition = new Vector3(0f, 0f, 0.3f);
		float num5 = Mathf.Abs(num4 - 90f);
		if (num5 < maxAngleDiff)
		{
			float y = ((!(num4 < 90f)) ? (0f - offsetAngle) : offsetAngle);
			base.transform.localRotation = Quaternion.Euler(0f, y, 0f);
			voiceLinesAnchor.localPosition = new Vector3(0f, 0f, 0.3f);
		}
		else
		{
			base.transform.localRotation = Quaternion.identity;
		}
	}
}
