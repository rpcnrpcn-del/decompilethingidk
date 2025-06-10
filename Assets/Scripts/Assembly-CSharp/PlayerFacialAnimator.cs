using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerFacialAnimator : MonoBehaviour
{
	[Serializable]
	private struct FacialFeature
	{
		public Renderer renderer;

		public MaterialPropertyBlock materialProps;

		public Vector2 maxJitterOffset;

		public Vector2 maxJitterScale;

		[NonSerialized]
		public Vector2 jitterOffset;

		[NonSerialized]
		public Vector2 jitterScale;

		public void UpdateJitterParams()
		{
			jitterOffset = new Vector2(UnityEngine.Random.Range(0f - maxJitterOffset.x, maxJitterOffset.x), UnityEngine.Random.Range(0f - maxJitterOffset.y, maxJitterOffset.y));
			jitterScale = new Vector2(UnityEngine.Random.Range(0f - maxJitterScale.x, maxJitterScale.x), UnityEngine.Random.Range(0f - maxJitterScale.y, maxJitterScale.y));
		}
	}

	[Header("Facial Features")]
	[SerializeField]
	private FacialFeature eye = new FacialFeature
	{
		maxJitterOffset = new Vector2(0.01f, 0.01f),
		maxJitterScale = new Vector2(0.03f, 0.03f)
	};

	[SerializeField]
	private FacialFeature mouth = new FacialFeature
	{
		maxJitterOffset = new Vector2(0.02f, 0.02f),
		maxJitterScale = new Vector2(0.1f, 0.1f)
	};

	[SerializeField]
	private Sprite eyeSprite;

	[SerializeField]
	private Sprite mouthSprite;

	[Header("Blinking Configuration")]
	[SerializeField]
	private Sprite blinkEyeSprite;

	[SerializeField]
	private float minBlinkInterval = 1f;

	[SerializeField]
	private float maxBlinkInterval = 8f;

	[SerializeField]
	private float blinkDuration = 0.1f;

	[Header("Jitter Configuration")]
	[SerializeField]
	private int jitterFPS = 6;

	private bool blinking;

	private Animator animator;

	private static int EmoteHappyID = Animator.StringToHash("EmoteHappy");

	private static int EmoteSadID = Animator.StringToHash("EmoteSad");

	private static int EmoteDisgustID = Animator.StringToHash("EmoteDisgust");

	private static int EmoteLaunghID = Animator.StringToHash("EmoteLaugh");

	private static int SmileID = Animator.StringToHash("Smile");

	private static int HugeSmileID = Animator.StringToHash("HugeSmile");

	private static int ScoreID = Animator.StringToHash("Score");

	private static int SadID = Animator.StringToHash("Sad");

	private static int DisgustID = Animator.StringToHash("Disgust");

	private static int HitID = Animator.StringToHash("Hit");

	private static int ConcentrateID = Animator.StringToHash("Concentrate");

	private static int KissID = Animator.StringToHash("Kiss");

	private static int TalkingID = Animator.StringToHash("Talking");

	private static int MuteID = Animator.StringToHash("Mute");

	[HideInInspector]
	public Player ThisPlayer;

	public bool Talking
	{
		get
		{
			return animator.GetBool(TalkingID);
		}
		set
		{
			animator.SetBool(TalkingID, value);
		}
	}

	public void PlayEmoteUp()
	{
		animator.SetTrigger(EmoteHappyID);
	}

	public void PlayEmoteDown()
	{
		animator.SetTrigger(EmoteSadID);
	}

	public void PlayEmoteLeft()
	{
		animator.SetTrigger(EmoteDisgustID);
	}

	public void PlayEmoteRight()
	{
		animator.SetTrigger(EmoteLaunghID);
	}

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Start()
	{
		eye.materialProps = new MaterialPropertyBlock();
		mouth.materialProps = new MaterialPropertyBlock();
		StartCoroutine(BlinkCoroutine());
		StartCoroutine(JitterCoroutine());
		if (ThisPlayer != null)
		{
			ThisPlayer.PlayerEvents.GameOverEvent += OnGameOver;
			ThisPlayer.PlayerEvents.PlayerHitEvent += OnPlayerHit;
			ThisPlayer.PlayerEvents.ToolHitEvent += OnToolHit;
			ThisPlayer.PlayerEvents.ToolPickupEvent += OnToolPickup;
			ThisPlayer.PlayerEvents.ToolReleaseEvent += OnToolRelease;
			ThisPlayer.PlayerEvents.TeleportEvent += OnTeleport;
			ThisPlayer.PlayerEvents.OutfitEquipEvent += OnOutfitEquipped;
			ThisPlayer.PlayerEvents.MicrophoneMuteEvent -= OnMicrophoneMuted;
		}
	}

	private void OnDestroy()
	{
		if (ThisPlayer != null)
		{
			ThisPlayer.PlayerEvents.GameOverEvent -= OnGameOver;
			ThisPlayer.PlayerEvents.PlayerHitEvent -= OnPlayerHit;
			ThisPlayer.PlayerEvents.ToolHitEvent -= OnToolHit;
			ThisPlayer.PlayerEvents.ToolPickupEvent -= OnToolPickup;
			ThisPlayer.PlayerEvents.ToolReleaseEvent -= OnToolRelease;
			ThisPlayer.PlayerEvents.TeleportEvent -= OnTeleport;
			ThisPlayer.PlayerEvents.OutfitEquipEvent -= OnOutfitEquipped;
			ThisPlayer.PlayerEvents.MicrophoneMuteEvent -= OnMicrophoneMuted;
		}
	}

	private void OnMicrophoneMuted(bool muted)
	{
		animator.SetBool(MuteID, muted);
	}

	private void OnOutfitEquipped(Player.BodyPart bodyPart)
	{
		animator.SetTrigger(SmileID);
	}

	private void OnTeleport()
	{
		animator.SetTrigger(SmileID);
	}

	private void OnToolRelease(Tool tool)
	{
		animator.SetTrigger(SmileID);
	}

	private void OnToolPickup(Tool tool)
	{
		animator.SetTrigger(ConcentrateID);
	}

	private void OnToolHit(Player.BodyPart bodyPart, Tool tool)
	{
		if (bodyPart == Player.BodyPart.Head)
		{
			animator.SetTrigger(HitID);
		}
	}

	private void OnPlayerHit(Player.BodyPart bodyPart, Player otherPlayer, Player.BodyPart otherPlayerBodyPart)
	{
		if (!(otherPlayer == ThisPlayer))
		{
			if (bodyPart == Player.BodyPart.Head && (otherPlayerBodyPart == Player.BodyPart.LeftHand || otherPlayerBodyPart == Player.BodyPart.RightHand))
			{
				animator.SetTrigger(HitID);
			}
			else if (bodyPart == Player.BodyPart.Head && otherPlayerBodyPart == Player.BodyPart.Head)
			{
				animator.SetTrigger(KissID);
			}
		}
	}

	private void OnActivityScored(bool thisPlayerScored)
	{
		int trigger = ((!thisPlayerScored) ? DisgustID : ScoreID);
		animator.SetTrigger(trigger);
	}

	private void OnGameOver(bool thisPlayerWon)
	{
		int trigger = ((!thisPlayerWon) ? SadID : HugeSmileID);
		animator.SetTrigger(trigger);
	}

	private IEnumerator JitterCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f / (float)jitterFPS);
			eye.UpdateJitterParams();
			mouth.UpdateJitterParams();
		}
	}

	private void Update()
	{
		UpdateFeature(eye, (!blinking) ? eyeSprite : blinkEyeSprite);
		UpdateFeature(mouth, mouthSprite);
	}

	private void UpdateFeature(FacialFeature feature, Sprite sprite)
	{
		if ((bool)sprite)
		{
			Vector2 vector = new Vector2(sprite.textureRect.position.x / (float)sprite.texture.width, sprite.textureRect.position.y / (float)sprite.texture.height);
			Vector2 vector2 = new Vector2(sprite.textureRect.size.x / (float)sprite.texture.width, sprite.textureRect.size.y / (float)sprite.texture.height);
			Vector2 vector3 = vector + vector2 / 2f;
			vector3 += new Vector2(vector2.x * feature.jitterOffset.x, vector2.y * feature.jitterOffset.y);
			vector2 += new Vector2(vector2.x * feature.jitterScale.x, vector2.y * feature.jitterScale.y);
			vector = vector3 - vector2 / 2f;
			feature.materialProps.SetTexture("_MainTex", sprite.texture);
			feature.materialProps.SetVector("_MainTex_ST", new Vector4(vector2.x, vector2.y, vector.x, vector.y));
			feature.renderer.SetPropertyBlock(feature.materialProps);
		}
	}

	private IEnumerator BlinkCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(minBlinkInterval, maxBlinkInterval));
			blinking = true;
			yield return new WaitForSeconds(blinkDuration);
			blinking = false;
		}
	}
}
