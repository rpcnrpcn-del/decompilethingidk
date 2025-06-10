using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMessagePanel : Selectable
{
	[Header("Message specific")]
	[SerializeField]
	private Image partyColorImage;

	[SerializeField]
	private Text playerNameText;

	[SerializeField]
	private Outline outline;

	private Player _player;

	public MessagesMenuController MessagesMenuController { get; set; }

	public Player Player
	{
		get
		{
			return _player;
		}
		set
		{
			_player = value;
			bool flag = _player != null && _player.PhotonPlayer != null && !_player.PhotonPlayer.isInactive;
			base.gameObject.SetActive(flag);
			if (flag)
			{
				playerNameText.text = _player.PlayerName;
			}
			Refresh();
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
	}

	public void Refresh()
	{
		if (Player != null)
		{
			partyColorImage.gameObject.SetActive(Player.PlayerParty.IsInParty);
			if (Player.PlayerParty.PartyColor.HasValue)
			{
				partyColorImage.color = Player.PlayerParty.PartyColor.Value;
			}
			if (outline != null)
			{
				outline.effectColor = ((!Player.PlayerParty.PartyColor.HasValue) ? Color.clear : Player.PlayerParty.PartyColor.Value);
			}
		}
	}

	public void Button_Accept()
	{
		MessagesMenuController.Refresh();
	}

	public void Button_Reject()
	{
		MessagesMenuController.Refresh();
	}
}
