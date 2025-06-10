using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPanicPlayerPanel : Selectable
{
	[SerializeField]
	private PanicMenuController panicMenuController;

	[Header("Player specific")]
	[SerializeField]
	private RawImage avatar;

	[SerializeField]
	private Image partyColorImage;

	[SerializeField]
	private Text playerNameText;

	[SerializeField]
	private Text playerStatusText;

	[SerializeField]
	private Text levelText;

	[SerializeField]
	private Text guestTag;

	[SerializeField]
	private Image levelTextBackground;

	[SerializeField]
	private Transform levelVisualRoot;

	[Header("Audio")]
	[SerializeField]
	private Image speakingIndicator;

	[SerializeField]
	private Image muteIndicator;

	[Range(0f, 1f)]
	[SerializeField]
	private float speakingColorAlpha = 0.9f;

	[Range(0f, 1f)]
	[SerializeField]
	private float notSpeakingColorAlpha = 0.1f;

	private float speakingColorFadeSpeed = 15f;

	private Player player;

	private int pointerCount;

	public CSteamID SteamID { get; private set; }

	public ulong PlayerId { get; private set; }

	public string PlayerName
	{
		get
		{
			return playerNameText.text;
		}
		set
		{
			playerNameText.text = value;
		}
	}

	public float PlayerDistanceSqrFromLocalPlayer { get; set; }

	public string PlayerStatus
	{
		get
		{
			return playerStatusText.text;
		}
		set
		{
			playerStatusText.text = value;
		}
	}

	public int PlayerLevel
	{
		set
		{
			levelText.text = value.ToString();
		}
	}

	public bool PlayerIsMember
	{
		set
		{
			guestTag.gameObject.SetActive(!value);
		}
	}

	public Texture PlayerAvatar
	{
		get
		{
			return avatar.texture;
		}
		set
		{
			avatar.texture = value;
			avatar.gameObject.SetActive(value != null);
		}
	}

	public Color? PartyColor
	{
		get
		{
			return (!partyColorImage.gameObject.activeSelf) ? ((Color?)null) : new Color?(partyColorImage.color);
		}
		set
		{
			partyColorImage.gameObject.SetActive(value.HasValue);
			if (value.HasValue)
			{
				partyColorImage.color = value.Value;
				levelTextBackground.color = value.Value;
			}
			else
			{
				levelTextBackground.color = Color.white;
			}
		}
	}

	public void SetPlayer(Player player)
	{
		this.player = player;
		PlayerId = player.PlayerId;
		//SteamID = player.SteamID;
		ShowLevelIndicator(true);
	}

	private void ShowLevelIndicator(bool visible)
	{
		levelVisualRoot.gameObject.SetActive(visible);
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			if (pointerCount > 0 && player != null)
			{
				player.PlayerUI.GlowVisible = true;
			}
			UpdateVoiceIndicators();
		}
	}

	private void UpdateVoiceIndicators()
	{
		if (player != null)
		{
			bool flag = player.Mute || player.RemoteMute;
			speakingIndicator.gameObject.SetActive(!flag);
			muteIndicator.gameObject.SetActive(flag);
			if (!flag)
			{
				Color color = speakingIndicator.color;
				float to = ((!player.IsTalking) ? notSpeakingColorAlpha : speakingColorAlpha);
				color.a = Mathf.SmoothStep(color.a, to, Time.deltaTime * speakingColorFadeSpeed);
				speakingIndicator.color = color;
			}
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		pointerCount++;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		pointerCount--;
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		Button_ShowPlayerDetails();
	}

	public void Button_ShowPlayerDetails()
	{
		if (player != null)
		{
			panicMenuController.ShowPlayerMenu(player);
		}
	}
}
