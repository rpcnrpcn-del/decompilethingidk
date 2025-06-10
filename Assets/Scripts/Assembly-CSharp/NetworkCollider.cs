using Photon;
using UnityEngine;

public class NetworkCollider : Photon.MonoBehaviour
{
	public delegate void ToolCollision(GameObject thisGameObject, Tool toolB, Vector3 point);

	public delegate void PlayerCollision(GameObject thisGameObject, Player player, Player.BodyPart bodyPart, Vector3 point);

	private const float COLLISION_COOLDOWN = 0.5f;

	public bool IsOnline = true;

	private Tool lastToolCollision;

	private float lastToolCollisionTime;

	private Player lastPlayerCollision;

	private float lastPlayerCollisionTime;

	public event ToolCollision ServerToolCollisionEnterEvent;

	public event PlayerCollision ServerPlayerCollisionEnterEvent;

	public event ToolCollision ServerToolTriggerEnterEvent;

	public event PlayerCollision ServerPlayerTriggerEnterEvent;

	private void OnTriggerEnter(Collider collider)
	{
		Vector3 position = base.transform.position;
		Tool colliderTool = collider.GetColliderTool();
		if (colliderTool != null)
		{
			FireToolCollisionRpc(true, colliderTool, position);
			return;
		}
		Player.BodyPart bodyPart;
		Player colliderPlayer = collider.GetColliderPlayer(out bodyPart);
		if (colliderPlayer != null)
		{
			FirePlayerCollisionRpc(true, colliderPlayer, bodyPart, position);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		Vector3 point = ((collision.contacts.Length <= 0) ? base.transform.position : collision.contacts[0].point);
		Tool colliderTool = collision.GetColliderTool();
		if (colliderTool != null)
		{
			FireToolCollisionRpc(false, colliderTool, point);
			return;
		}
		Player.BodyPart bodyPart;
		Player colliderPlayer = collision.GetColliderPlayer(out bodyPart);
		if (colliderPlayer != null)
		{
			FirePlayerCollisionRpc(false, colliderPlayer, bodyPart, point);
		}
	}

	[PunRPC]
	public void RpcMasterToolCollisionEnterEvent(bool isTrigger, int toolId, Vector3 collisionPoint)
	{
		Tool tool = Tool.Find(toolId);
		if (tool != null)
		{
			if (isTrigger && this.ServerToolTriggerEnterEvent != null)
			{
				this.ServerToolTriggerEnterEvent(base.gameObject, tool, collisionPoint);
			}
			else if (!isTrigger && this.ServerToolCollisionEnterEvent != null)
			{
				this.ServerToolCollisionEnterEvent(base.gameObject, tool, collisionPoint);
			}
		}
	}

	[PunRPC]
	public void RpcMasterPlayerCollisionEnterEvent(bool isTrigger, PhotonPlayer hitPhotonPlayer, int playerBodyPart, Vector3 collisionPoint)
	{
		Player player = hitPhotonPlayer.ToPlayer();
		if (player != null)
		{
			if (isTrigger && this.ServerPlayerTriggerEnterEvent != null)
			{
				this.ServerPlayerTriggerEnterEvent(base.gameObject, player, (Player.BodyPart)playerBodyPart, collisionPoint);
			}
			else if (!isTrigger && this.ServerPlayerCollisionEnterEvent != null)
			{
				this.ServerPlayerCollisionEnterEvent(base.gameObject, player, (Player.BodyPart)playerBodyPart, collisionPoint);
			}
		}
	}

	private void FireToolCollisionRpc(bool isTrigger, Tool hitTool, Vector3 point)
	{
		if (!(hitTool == lastToolCollision) || !(Time.time - lastToolCollisionTime <= 0.5f))
		{
			lastToolCollision = hitTool;
			lastToolCollisionTime = Time.time;
			if (IsOnline && base.hasAuthority)
			{
				base.photonView.RPC("RpcMasterToolCollisionEnterEvent", PhotonTargets.MasterClient, isTrigger, hitTool.GetComponent<PhotonView>().viewID, point);
			}
		}
	}

	private void FirePlayerCollisionRpc(bool isTrigger, Player hitPlayer, Player.BodyPart bodyPart, Vector3 point)
	{
		if (!(hitPlayer == lastPlayerCollision) || !(Time.time - lastPlayerCollisionTime <= 0.5f))
		{
			lastPlayerCollision = hitPlayer;
			lastPlayerCollisionTime = Time.time;
			if (IsOnline && base.hasAuthority)
			{
				base.photonView.RPC("RpcMasterPlayerCollisionEnterEvent", PhotonTargets.MasterClient, isTrigger, hitPlayer.PhotonPlayer, (int)bodyPart, point);
			}
		}
	}
}
