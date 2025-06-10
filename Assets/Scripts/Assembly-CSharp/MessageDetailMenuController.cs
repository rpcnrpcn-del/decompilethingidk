using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class MessageDetailMenuController : MenuController
{
	[SerializeField]
	private RawImage avatar;

	[SerializeField]
	private Text playerNameText;

	[SerializeField]
	private Text playerStatusText;

	[SerializeField]
	private Text levelText;

	private string PlayerName
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

	private string PlayerStatus
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

	private string LevelText
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

	private Texture PlayerAvatar
	{
		get
		{
			return avatar.texture;
		}
		set
		{
			avatar.texture = value;
		}
	}

	public Message Message { get; private set; }

	public virtual void SetMessage(Message message)
	{
		Message = message;
		Profile profileFromCache = Profiles.GetProfileFromCache(Message.FromPlayerId);
		PlayerPresence playerPresenceFromCache = PlayerPresenceManager.GetPlayerPresenceFromCache(Message.FromPlayerId);
		if (profileFromCache != null)
		{
			PlayerName = profileFromCache.DisplayName;
			LevelText = profileFromCache.Level.ToString();
			PlayerStatus = PUNNetworkManager.Instance.GetPlayerStatusString(playerPresenceFromCache);
		}
		else
		{
			PlayerName = string.Empty;
			LevelText = string.Empty;
			PlayerStatus = string.Empty;
		}
		RefreshSenderImage();
	}

	public void RefreshSenderImage()
	{
		Profile profileFromCache = Profiles.GetProfileFromCache(Message.FromPlayerId);
		Texture texture = Images.GetProfileImageFromCache(Message.FromPlayerId);
		if (texture == null || profileFromCache == null || !profileFromCache.Verified)
		{
			texture = Player.LocalPlayer.DefaultProfileImage;
		}
		PlayerAvatar = texture;
	}

	public void CloseMessage()
	{
		Visible = false;
	}

	public void CloseAndDeleteMessage()
	{
		if (Message != null)
		{
			Messages.DeleteMessage(Message.Id);
		}
		Visible = false;
	}
}
