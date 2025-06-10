using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPlayerPanel : Selectable
{
	[SerializeField]
	private PartyMenuController partyMenuController;

	[SerializeField]
	private Color offlineBackgroundColor;

	[SerializeField]
	private Color onlineBackgroundColor;

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

	private bool _playerIsMember;

	private bool _isOnline;

	private int pointerCount;

	public ulong PlayerId { get; private set; }

	public Player Player { get; private set; }

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

	public string PlayerLevel
	{
		get
		{
			return levelText.text;
		}
		set
		{
			levelText.text = value;
		}
	}

	public bool PlayerIsMember
	{
		get
		{
			return _playerIsMember;
		}
		set
		{
			_playerIsMember = value;
			if (guestTag != null)
			{
				guestTag.gameObject.SetActive(!_playerIsMember);
			}
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

	public bool IsOnline
	{
		get
		{
			return _isOnline;
		}
		set
		{
			if (_isOnline != value)
			{
				_isOnline = value;
				base.targetGraphic.color = ((!value) ? offlineBackgroundColor : onlineBackgroundColor);
			}
		}
	}

	public void setPlayerId(ulong playerId)
	{
		Player = null;
		PlayerId = playerId;
	}

	public void SetPlayer(Player player)
	{
		Player = player;
		if (player != null)
		{
			PlayerId = player.PlayerId;
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			if (pointerCount > 0 && Player != null)
			{
				Player.PlayerUI.GlowVisible = true;
			}
			UpdateVoiceIndicators();
		}
	}

	private void UpdateVoiceIndicators()
	{
		if (Player != null)
		{
			bool flag = Player.Mute || Player.RemoteMute;
			speakingIndicator.gameObject.SetActive(!flag);
			muteIndicator.gameObject.SetActive(flag);
			if (!flag)
			{
				Color color = speakingIndicator.color;
				float to = ((!Player.IsTalking) ? notSpeakingColorAlpha : speakingColorAlpha);
				color.a = Mathf.SmoothStep(color.a, to, Time.deltaTime * speakingColorFadeSpeed);
				speakingIndicator.color = color;
			}
		}
		else
		{
			speakingIndicator.gameObject.SetActive(false);
			muteIndicator.gameObject.SetActive(false);
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
		if (Player != null)
		{
			partyMenuController.ShowPlayerMenu(Player);
		}
		else
		{
			partyMenuController.ShowFriendMenu(PlayerId);
		}
	}
}
