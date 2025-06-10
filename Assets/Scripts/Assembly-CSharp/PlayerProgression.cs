using Photon;
using RecNet;
using UnityEngine;

public class PlayerProgression : Photon.MonoBehaviour
{
	[Header("Feedback")]
	[SerializeField]
	private PooledParticle levelUpParticlesPrefab;

	private Player thisPlayer;

	private const string PlayerLevelProperty = "PlayerLevel";

	private const string PlayerMemberProperty = "PlayerMember";

	public int Level
	{
		get
		{
			return thisPlayer.PlayerData.GetData("PlayerLevel", 1);
		}
		private set
		{
			if (thisPlayer.isLocal)
			{
				if (thisPlayer.PlayerData.HasData("PlayerLevel") && value > Level)
				{
					ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Vital, "You Leveled Up!", 5f, PlayLevelUpFeedback);
					thisPlayer.PlayerEvents.LevelUp(value);
				}
				thisPlayer.PlayerData.SetData("PlayerLevel", value);
			}
		}
	}

	public bool IsMember { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		thisPlayer = GetComponent<Player>();
		thisPlayer.PlayerData.RegisterCallback("PlayerMember", OnPlayerMemberPropertyChanged);
		if (base.isLocal)
		{
			Level = Profiles.LocalProfile.Level;
			Profiles.LocalProfileDownloaded += Profile_LocalProfileDownloaded;
			Profile_LocalProfileDownloaded(Profiles.LocalProfile);
			Profiles.LocalProfileLevelUpdated += Profile_LocalProfileLevelUpdated;
			Profiles.LocalProfileXpUpdated += Profile_LocalProfileXpUpdated;
		}
		else
		{
			OnPlayerMemberPropertyChanged(null, null);
		}
	}

	protected override void OnDestroy()
	{
		if (base.isLocal)
		{
			Profiles.LocalProfileDownloaded -= Profile_LocalProfileDownloaded;
			Profiles.LocalProfileLevelUpdated -= Profile_LocalProfileLevelUpdated;
			Profiles.LocalProfileXpUpdated -= Profile_LocalProfileXpUpdated;
		}
		thisPlayer.PlayerData.UnregisterCallback("PlayerMember", OnPlayerMemberPropertyChanged);
		base.OnDestroy();
	}

	private void Profile_LocalProfileXpUpdated(int currentXp, int deltaXp)
	{
		thisPlayer.PlayerEvents.XPUpdated(currentXp);
	}

	private void Profile_LocalProfileLevelUpdated(int level)
	{
		Level = level;
	}

	private void Profile_LocalProfileDownloaded(Profile profile)
	{
		if (base.isLocal)
		{
			thisPlayer.PlayerData.SetData("PlayerMember", profile != null && profile.Verified);
		}
	}

	private void OnPlayerMemberPropertyChanged(PlayerData sender, string key)
	{
		IsMember = thisPlayer.PlayerData.GetData("PlayerMember", false);
	}

	public void PlayLevelUpFeedback()
	{
		base.photonView.RPC("RpcPlayLevelUpFeedback", PhotonTargets.All);
	}

	[PunRPC]
	public void RpcPlayLevelUpFeedback()
	{
		thisPlayer.PlayerAudio.OnLevelUp();
		PooledParticle pooledParticle = ObjectPool.Instance.Acquire(levelUpParticlesPrefab);
		if (pooledParticle != null)
		{
			pooledParticle.Play();
			PlayerFloorPositionFollowing component = pooledParticle.GetComponent<PlayerFloorPositionFollowing>();
			if (component != null)
			{
				component.TrackedPlayer = thisPlayer;
				return;
			}
			pooledParticle.transform.position = thisPlayer.CurrentFloorPosition;
			pooledParticle.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.forward);
		}
	}
}
