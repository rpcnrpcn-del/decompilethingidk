using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAudio : MonoBehaviour
{
	private class CollisionRecord
	{
		public GameObject Collider;

		public float CollisionTime;
	}

	[Serializable]
	public class AudioClipsToLayer
	{
		public string Name;

		public LayerMask Layers;

		public string Tag = string.Empty;

		public AnimationCurve velocityToVolumeCurve;

		public RecRoomAudioClip[] Clips;

		[Tooltip("Per clip cooldown to limit the number of consecutive audios. Set it to 0 for most clips")]
		public float InstanceLimitDuration;

		public float LastInstanceTime { get; set; }

		public bool CanPlay(GameObject go, LayerMask layerMask)
		{
			if ((Time.time <= InstanceLimitDuration || Time.time - LastInstanceTime > InstanceLimitDuration) && (Layers.value & (int)layerMask) == (int)layerMask && (string.IsNullOrEmpty(Tag) || go.CompareTag(Tag)))
			{
				LastInstanceTime = Time.time;
				return true;
			}
			return false;
		}
	}

	private const float MAX_COLLISION_MAGNITUDE = 25f;

	private float cooldown = 0.1f;

	[SerializeField]
	private AudioClipsToLayer[] audioMappings;

	private List<CollisionRecord> collisionHistory = new List<CollisionRecord>();

	private bool enableCollisionEnter;

	private IEnumerator Start()
	{
		yield return null;
		enableCollisionEnter = true;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!enableCollisionEnter)
		{
			return;
		}
		CollisionRecord collisionRecord = null;
		foreach (CollisionRecord item in collisionHistory)
		{
			if (item.Collider == collision.gameObject)
			{
				collisionRecord = item;
				break;
			}
		}
		bool flag = collisionRecord == null || Time.time - collisionRecord.CollisionTime > cooldown;
		if (collisionRecord == null)
		{
			CollisionRecord collisionRecord2 = new CollisionRecord();
			collisionRecord2.Collider = collision.gameObject;
			collisionRecord2.CollisionTime = Time.time;
			collisionRecord = collisionRecord2;
			collisionHistory.Add(collisionRecord);
		}
		else
		{
			collisionRecord.CollisionTime = Time.time;
		}
		if (!flag || collision.contacts.Length <= 0)
		{
			return;
		}
		int num = 1 << collision.gameObject.layer;
		AudioClipsToLayer[] array = audioMappings;
		foreach (AudioClipsToLayer audioClipsToLayer in array)
		{
			if (audioClipsToLayer.CanPlay(collision.gameObject, num))
			{
				RecRoomAudioClip recRoomAudioClip = audioClipsToLayer.Clips[UnityEngine.Random.Range(0, audioClipsToLayer.Clips.Length)];
				float magnitude = collision.relativeVelocity.magnitude;
				if (audioClipsToLayer.velocityToVolumeCurve != null && audioClipsToLayer.velocityToVolumeCurve.length > 0)
				{
					recRoomAudioClip.volume = audioClipsToLayer.velocityToVolumeCurve.Evaluate(Mathf.Clamp01(magnitude / 25f));
				}
				else
				{
					recRoomAudioClip.volume = Mathf.Clamp01(magnitude / 25f);
				}
				AudioManager.Play3DSFX(recRoomAudioClip, collision.contacts[0].point);
				break;
			}
		}
	}
}
