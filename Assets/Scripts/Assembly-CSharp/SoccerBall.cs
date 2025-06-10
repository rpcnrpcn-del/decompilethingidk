using System.Collections.Generic;
using UnityEngine;

public class SoccerBall : Ball
{
	public delegate void Score(SoccerBall thisBall, SoccerGoal goal, Vector3 scorePoint);

	public ParticleSystem FireTrail;

	private Dictionary<GameTeam, PhotonPlayer> lastHitPlayerByTeam = new Dictionary<GameTeam, PhotonPlayer>();

	public event Score ScoreEvent;

	protected override void Start()
	{
		base.Start();
		base.photonView.OwnerChangedEvent += PhotonView_OwnerChangedEvent;
		UpdateLastHitPlayerByTeam(PhotonNetwork.masterClient);
	}

	protected override void OnTriggerEnter(Collider collider)
	{
		base.OnTriggerEnter(collider);
		SoccerGoal component = collider.GetComponent<SoccerGoal>();
		if (component != null && base.hasAuthority && this.ScoreEvent != null)
		{
			this.ScoreEvent(this, component, base.transform.position);
		}
	}

	public override void OnOwnerRoleUpdated()
	{
		base.OnOwnerRoleUpdated();
		UpdateLastHitPlayerByTeam(base.Owner.PhotonPlayer);
	}

	protected override void OnApplyForceVFX(PhotonPlayer photonPlayer, Vector3 position, Vector3 force)
	{
		base.OnApplyForceVFX(photonPlayer, position, force);
		FireTrail.Play();
	}

	private void PhotonView_OwnerChangedEvent(int newOwnerId)
	{
		UpdateLastHitPlayerByTeam(PhotonPlayer.Find(newOwnerId));
	}

	private void UpdateLastHitPlayerByTeam(PhotonPlayer pPlayer)
	{
		if (pPlayer == null)
		{
			return;
		}
		GameTeam playerTeam = RecRoomSceneManager.Instance.GameManager.TeamManager.GetPlayerTeam(pPlayer);
		if (playerTeam != GameTeam.INVALID)
		{
			lastHitPlayerByTeam[playerTeam] = pPlayer;
		}
		GameTeam? gameTeam = null;
		foreach (KeyValuePair<GameTeam, PhotonPlayer> item in lastHitPlayerByTeam)
		{
			if (item.Value == pPlayer && item.Key != playerTeam)
			{
				gameTeam = item.Key;
				break;
			}
		}
		if (gameTeam.HasValue)
		{
			lastHitPlayerByTeam[gameTeam.Value] = null;
		}
	}

	public PhotonPlayer GetLastHitPlayer(GameTeam team)
	{
		PhotonPlayer value = null;
		lastHitPlayerByTeam.TryGetValue(team, out value);
		return value;
	}
}
