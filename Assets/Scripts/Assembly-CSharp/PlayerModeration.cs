using System;
using System.Collections.Generic;
using Photon;
using RecNet;
using UnityEngine;

public class PlayerModeration : Photon.MonoBehaviour
{
	[Serializable]
	public class VTKPunishmentBucket : IComparable<VTKPunishmentBucket>
	{
		public string name;

		[Tooltip("If you got kicked is equal or below this many times, you deserve this punishment")]
		public int MinOffenseCount;

		[SerializeField]
		private int seconds;

		[Tooltip("If you are a guest you may get different penalties for repeat offense")]
		[SerializeField]
		private float guestMultiplier = 1f;

		[Tooltip("We may change the ease of kicking you out, based on the punishment bucket.")]
		[Range(0.01f, 1f)]
		public float MinVoteToKickPlayerPercentModifier = 1f;

		public int GetDuration(Player player)
		{
			return Mathf.CeilToInt((float)seconds * ((!player.PlayerProgression.IsMember) ? guestMultiplier : 1f));
		}

		public int CompareTo(VTKPunishmentBucket other)
		{
			return MinOffenseCount.CompareTo(other.MinOffenseCount);
		}
	}

	[Range(0f, 1f)]
	[SerializeField]
	private float minVoteToKickPlayerPercent = 0.85f;

	[SerializeField]
	private float minVoteToKickPlayerCount = 4f;

	[SerializeField]
	[EnumFlags]
	private PlayerMenu.NotificationEffectFlag vtkMenuNotificationEffectFlag = (PlayerMenu.NotificationEffectFlag)(-1);

	[SerializeField]
	private List<VTKPunishmentBucket> vtkPunishmentBuckets;

	[Tooltip("How long should we keep the current votes towards a kick? (in seconds)")]
	[SerializeField]
	private float vtkActiveDuration = 30f;

	private float vtkExpireTime;

	private VTKPunishmentBucket currentVTKPunishmentBucket;

	private Dictionary<PhotonPlayer, float> voteToKickMap = new Dictionary<PhotonPlayer, float>();

	private Player thisPlayer;

	public const string VOTE_TO_KICK_COUNT_PREF = "VoteToKickCount";

	private ConfirmationMenuController modVtkConfirmMenu;

	private static float positiveKarmaSeconds;

	private const float PositiveKarmaUpdateInteval = 300f;

	private List<PhotonPlayer> vtkRequestSentPlayers = new List<PhotonPlayer>();

	public float VoteToKickCount
	{
		get
		{
			return thisPlayer.PlayerData.GetData("VoteToKickCount", 0f);
		}
		private set
		{
			thisPlayer.PlayerData.SetData("VoteToKickCount", value);
			if (!(value > 0f) || !((float)PhotonNetwork.playerList.Length > minVoteToKickPlayerCount))
			{
				return;
			}
			float num = value / (float)(PhotonNetwork.playerList.Length - 1);
			if (num >= LocalMinVoteToKickPlayerPercent)
			{
				if (SessionManager.IsDeveloper)
				{
					modVtkConfirmMenu = Player.LocalPlayer.PlayerUI.Menu.ShowConfirmation("Vote to kick?", "You are being vote kicked, do you accept this punishment?", "Kick me :(", "Oh, Hell no!", vtkMenuNotificationEffectFlag, "Vote to kick request!");
					modVtkConfirmMenu.ConfirmAction += OnModVTKConfirmed;
				}
				else
				{
					LocalKickUser(LocalGetUserBlockDuration(), PhotonNetwork.playerList.Length, num, true);
				}
			}
		}
	}

	private float LocalMinVoteToKickPlayerPercent
	{
		get
		{
			return minVoteToKickPlayerPercent * currentVTKPunishmentBucket.MinVoteToKickPlayerPercentModifier;
		}
	}

	public bool IsVotedForKick { get; private set; }

	public bool VoteKickHappened { get; set; }

	private void OnModVTKConfirmed(ConfirmationMenuController menuController)
	{
		if (modVtkConfirmMenu != null)
		{
			modVtkConfirmMenu.ConfirmAction -= OnModVTKConfirmed;
			if (Player.LocalPlayer != null && modVtkConfirmMenu.Confirm == true)
			{
				LocalKickUser(LocalGetUserBlockDuration(), PhotonNetwork.playerList.Length, VoteToKickCount / (float)(PhotonNetwork.playerList.Length - 1), true);
			}
			modVtkConfirmMenu = null;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		thisPlayer = GetComponent<Player>();
	}

	protected void Start()
	{
		VoteToKickCount = 0f;
		LocalCreateVTKPunishmentBucket();
	}

	private void Update()
	{
		if (!base.isLocal)
		{
			return;
		}
		if (!PUNNetworkManager.Instance.IsActivityInviteOnly && PhotonNetwork.playerList.Length > 1)
		{
			positiveKarmaSeconds += Time.unscaledDeltaTime;
			if (positiveKarmaSeconds >= 300f)
			{
				positiveKarmaSeconds -= 300f;
				Profiles.UpdateLocalProfileKarma(300f);
			}
		}
		if (VoteToKickCount > 0f && Time.time > vtkExpireTime)
		{
			LocalResetVTK();
		}
	}

	private void LocalResetVTK()
	{
		if (base.isLocal)
		{
			VoteToKickCount = 0f;
			voteToKickMap.Clear();
			vtkRequestSentPlayers.Clear();
			base.photonView.RPC("RpcModerationBroadcastVTKReset", PhotonTargets.Others);
		}
	}

	private void LocalCreateVTKPunishmentBucket()
	{
		if (!base.isLocal)
		{
			return;
		}
		vtkPunishmentBuckets.Sort();
		int num = -Profiles.LocalProfile.Reputation;
		currentVTKPunishmentBucket = null;
		for (int i = 0; i < vtkPunishmentBuckets.Count; i++)
		{
			if (num <= vtkPunishmentBuckets[i].MinOffenseCount)
			{
				currentVTKPunishmentBucket = vtkPunishmentBuckets[i];
				break;
			}
		}
		if (currentVTKPunishmentBucket == null)
		{
			Debug.LogWarning("Missing a VTK punishment bucket for reputation : " + num + ". Getting the closest punishment instead.");
			currentVTKPunishmentBucket = vtkPunishmentBuckets.LastItem();
		}
	}

	public void KickPlayer(int duration = 0)
	{
		base.photonView.RPC("RpcModeratorKickPlayer", thisPlayer.PhotonPlayer, duration);
	}

	public void ShowWarningMessage(string title, string subtitle, float duration, float scale)
	{
		base.photonView.RPC("RpcModeratorShowWarning", thisPlayer.PhotonPlayer, title, subtitle, duration, scale);
	}

	[PunRPC]
	public void RpcModeratorShowWarning(string title, string subtitle, float duration, float scale)
	{
		MenuNotification.PlayNext(title, duration, subtitle, true, null, scale);
	}

	[PunRPC]
	public void RpcModeratorKickPlayer(int duration)
	{
		LocalKickUser(duration, PhotonNetwork.playerList.Length, 1f);
	}

	[PunRPC]
	public void RpcModerationVoteToKick(PhotonPlayer otherPlayer, float vote)
	{
		if (!base.isLocal)
		{
			return;
		}
		if (otherPlayer != null && !voteToKickMap.ContainsKey(otherPlayer))
		{
			if (VoteToKickCount == 0f)
			{
				RecnetAnalyticsHelper.VoteKickInitiated(otherPlayer.ToPlayer(), thisPlayer);
			}
			voteToKickMap.Add(otherPlayer, vote);
			VoteToKickCount += vote;
			vtkExpireTime = Time.time + vtkActiveDuration;
			if (SessionManager.IsDeveloper)
			{
				ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "You are voted to be kicked by " + otherPlayer.name, 2f);
			}
		}
		PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
		foreach (PhotonPlayer pPlayer in otherPlayers)
		{
			SendVTKRequestMessage(pPlayer);
		}
	}

	private void SendVTKRequestMessage(PhotonPlayer pPlayer)
	{
		if (!vtkRequestSentPlayers.Contains(pPlayer) && !voteToKickMap.ContainsKey(pPlayer))
		{
			vtkRequestSentPlayers.Add(pPlayer);
			Messages.SendVoteToKickRequest(thisPlayer.PlayerId, pPlayer.ToPlayer().PlayerId);
		}
	}

	[PunRPC]
	public void RpcModerationBroadcastKickMessage()
	{
		if (!base.isLocal)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Major, string.Format("User {0} was vote kicked.", thisPlayer.PlayerName), 2.5f);
		}
	}

	[PunRPC]
	public void RpcModerationBroadcastVTKReset()
	{
		if (!base.isLocal)
		{
			IsVotedForKick = false;
		}
	}

	private void LocalKickUser(int duration, int userCount, float userPercent, bool broadcastKick = false)
	{
		if (broadcastKick)
		{
			base.photonView.RPC("RpcModerationBroadcastKickMessage", PhotonTargets.Others);
		}
		AnalyticsHelper.ModerationUserKicked(duration);
		RecnetAnalyticsHelper.VoteKicked(userPercent, userCount, duration);
		if (PhotonNetwork.inRoom)
		{
			SingletonMonoBehaviour<SessionManager>.Instance.BlockedRoomName = PhotonNetwork.room.name;
		}
		thisPlayer.ReturnToDormRoom("You got vote kicked.\nPlease refer to the Code of Conduct.");
		if (duration > 0)
		{
			SingletonMonoBehaviour<SettingsManager>.Instance.SetUserBlocked(duration);
		}
		SingletonMonoBehaviour<SessionManager>.Instance.NumberOfTimesUserGotKicked++;
		Profiles.UpdateLocalProfileReputation(-1);
	}

	public void VoteToKickPlayer()
	{
		if (!IsVotedForKick)
		{
			bool flag = Player.LocalPlayer.PlayerProgression.Level >= 20 && Profiles.LocalProfile.Reputation >= -1;
			base.photonView.RPC("RpcModerationVoteToKick", thisPlayer.PhotonPlayer, Player.LocalPlayer.PhotonPlayer, (!flag) ? 1f : 2f);
			IsVotedForKick = true;
			AnalyticsHelper.ModerationVoteKicked(thisPlayer, Player.LocalPlayer.PlayerModeration.VoteKickHappened);
			Player.LocalPlayer.PlayerModeration.VoteKickHappened = true;
		}
	}

	private int LocalGetUserBlockDuration()
	{
		return currentVTKPunishmentBucket.GetDuration(thisPlayer);
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		if (voteToKickMap.ContainsKey(player))
		{
			VoteToKickCount -= voteToKickMap[player];
			voteToKickMap.Remove(player);
		}
	}
}
