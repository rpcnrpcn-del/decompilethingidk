using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAudio : MonoBehaviour
{
	private const float clapCooldown = 0.1f;

	[SerializeField]
	private RecRoomAudioClip partyInvite;

	[Header("Hands")]
	[SerializeField]
	private RecRoomAudioClip handClap;

	[SerializeField]
	private RecRoomAudioClip fistBump;

	[Header("Progression")]
	[SerializeField]
	private RecRoomAudioClip levelUp;

	[SerializeField]
	private RecRoomAudioClip objectiveComplete;

	[Header("Push to talk")]
	[SerializeField]
	private RecRoomAudioClip pushToTalkOn;

	[SerializeField]
	private RecRoomAudioClip pushToTalkOff;

	[Header("Teleportation")]
	[SerializeField]
	private RecRoomAudioClip baseTeleport;

	[SerializeField]
	private RecRoomAudioClip[] teleportSounds;

	[SerializeField]
	private RecRoomAudioClip teleportReady;

	[SerializeField]
	private RecRoomAudioClip teleportError;

	[Header("In Place Rotate")]
	[SerializeField]
	private RecRoomAudioClip rotateLeft;

	[SerializeField]
	private Vector3 rotateLeftHeadRelativePosition = new Vector3(-0.1f, 0f, 0f);

	[SerializeField]
	private RecRoomAudioClip rotateRight;

	[SerializeField]
	private Vector3 rotateRightHeadRelativePosition = new Vector3(0.1f, 0f, 0f);

	[SerializeField]
	private RecRoomAudioClip rotate180;

	[SerializeField]
	private Vector3 rotate180HeadRelativePosition = new Vector3(0f, 0f, -0.1f);

	[Header("Emotes")]
	[SerializeField]
	private RecRoomAudioClip emoteUp;

	[SerializeField]
	private RecRoomAudioClip emoteDown;

	[SerializeField]
	private RecRoomAudioClip emoteLeft;

	[SerializeField]
	private RecRoomAudioClip emoteRight;

	private Player thisPlayer;

	public static AudioRolloff PlayerVoipAudioRolloff { get; set; }

	public static AudioRolloff Custom3DSFXAudioRolloff { get; set; }

	public static RecRoomAudioClip[] CustomTeleportSounds { get; set; }

	private void Awake()
	{
		thisPlayer = GetComponent<Player>();
	}

	private void Start()
	{
		base.gameObject.layer = 0;
		emoteUp = null;
		emoteDown = null;
		emoteLeft = null;
		emoteRight = null;
		if (PlayerVoipAudioRolloff != null)
		{
			PlayerVoipAudioRolloff.TrySet(thisPlayer.VoiceAudioSource);
		}
	}

	public void OnTeleport()
	{
		AudioManager.Play3DSFX(baseTeleport, thisPlayer.Head.transform);
		AudioManager.PlayRandom3DSFX((CustomTeleportSounds == null || CustomTeleportSounds.Length <= 0) ? teleportSounds : CustomTeleportSounds, thisPlayer.FloorPositionTransform);
	}

	public void OnLevelUp()
	{
		AudioManager.Play2DSFX(levelUp);
	}

	public void OnObjectiveComplete()
	{
		AudioManager.Play2DSFX(objectiveComplete);
	}

	public void OnTeleportReady()
	{
		AudioManager.Play2DSFX(teleportReady);
	}

	public void OnTeleportError()
	{
		AudioManager.Play2DSFX(teleportError);
	}

	public void OnInPlaceRotateLeft()
	{
		AudioManager.Play3DSFX(rotateLeft, thisPlayer.Head.transform.TransformPoint(rotateLeftHeadRelativePosition));
	}

	public void OnInPlaceRotateRight()
	{
		AudioManager.Play3DSFX(rotateRight, thisPlayer.Head.transform.TransformPoint(rotateRightHeadRelativePosition));
	}

	public void OnInPlaceRotate180()
	{
		AudioManager.Play3DSFX(rotate180, thisPlayer.Head.transform.TransformPoint(rotate180HeadRelativePosition));
	}

	public void OnPartyInvite()
	{
		AudioManager.Play3DSFX(partyInvite, thisPlayer.Head.transform.position);
	}

	public void OnHighFive(Vector3 position)
	{
		AudioManager.Play3DSFX(handClap, position);
	}

	public void OnFistBump(Vector3 position)
	{
		AudioManager.Play3DSFX(fistBump, position);
	}

	public void OnPushToTalk(bool isTalking)
	{
		AudioManager.Play3DSFX((!isTalking) ? pushToTalkOff : pushToTalkOn, thisPlayer.Head.transform.position);
	}

	public void SetFilterType(AudioManager.VOIPFilter filter)
	{
		thisPlayer.VoiceAudioSource.outputAudioMixerGroup = SingletonMonoBehaviour<AudioManager>.Instance.GetVOIPMixerOutput(filter);
	}
}
