using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerEvents : MonoBehaviour
{
	public event Action<int> LevelUpEvent;

	public event Action<int> XPUpdatedEvent;

	public event Action<bool> MicrophoneMuteEvent;

	public event Action TeleportEvent;

	public event Action<bool> MenuVisibleEvent;

	public event Action<bool> GameOverEvent;

	public event Action<Tool> ToolReleaseEvent;

	public event Action<Tool> ToolPickupEvent;

	public event Action<Player.BodyPart, Tool> ToolHitEvent;

	public event Action<Player.BodyPart> OutfitEquipEvent;

	public event Action<Player.BodyPart, Player, Player.BodyPart> PlayerHitEvent;

	public event Action DodgeballOutEvent;

	public event Action PaintballHitEvent;

	public event Action PaintballFlagCaptureEvent;

	public event Action SoccerGoalEvent;

	public event Action<int> DiscGolfCompletedHoleEvent;

	public event Action PaddleballScoreEvent;

	public void LevelUp(int newLevel)
	{
		if (this.LevelUpEvent != null)
		{
			this.LevelUpEvent(newLevel);
		}
	}

	public void XPUpdated(int newXP)
	{
		if (this.XPUpdatedEvent != null)
		{
			this.XPUpdatedEvent(newXP);
		}
	}

	public void MicrophoneMuted(bool mute)
	{
		if (this.MicrophoneMuteEvent != null)
		{
			this.MicrophoneMuteEvent(mute);
		}
	}

	public void Teleport()
	{
		if (this.TeleportEvent != null)
		{
			this.TeleportEvent();
		}
	}

	public void MenuVisible(bool visible)
	{
		if (this.MenuVisibleEvent != null)
		{
			this.MenuVisibleEvent(visible);
		}
	}

	public void GameOver(bool won)
	{
		if (this.GameOverEvent != null)
		{
			this.GameOverEvent(won);
		}
	}

	public void ToolRelease(Tool tool)
	{
		if (this.ToolReleaseEvent != null)
		{
			this.ToolReleaseEvent(tool);
		}
	}

	public void ToolPickup(Tool tool)
	{
		if (this.ToolPickupEvent != null)
		{
			this.ToolPickupEvent(tool);
		}
	}

	public void ToolHit(Player.BodyPart bodyPart, Tool tool)
	{
		if (this.ToolHitEvent != null)
		{
			this.ToolHitEvent(bodyPart, tool);
		}
	}

	public void OutfitEquipped(Player.BodyPart bodyPart)
	{
		if (this.OutfitEquipEvent != null)
		{
			this.OutfitEquipEvent(bodyPart);
		}
	}

	public void PlayerHit(Player.BodyPart bodyPart, Player otherPlayer, Player.BodyPart otherPlayerBodyPart)
	{
		if (this.PlayerHitEvent != null)
		{
			this.PlayerHitEvent(bodyPart, otherPlayer, otherPlayerBodyPart);
		}
	}

	public void DodgeballOut()
	{
		if (this.DodgeballOutEvent != null)
		{
			this.DodgeballOutEvent();
		}
	}

	public void PaintballHit()
	{
		if (this.PaintballHitEvent != null)
		{
			this.PaintballHitEvent();
		}
	}

	public void PaintballFlagCapture()
	{
		if (this.PaintballFlagCaptureEvent != null)
		{
			this.PaintballFlagCaptureEvent();
		}
	}

	public void SoccerGoal()
	{
		if (this.SoccerGoalEvent != null)
		{
			this.SoccerGoalEvent();
		}
	}

	public void DiscGolfCompletedHole(int score)
	{
		if (this.DiscGolfCompletedHoleEvent != null)
		{
			this.DiscGolfCompletedHoleEvent(score);
		}
	}

	public void PaddleballScore()
	{
		if (this.PaddleballScoreEvent != null)
		{
			this.PaddleballScoreEvent();
		}
	}
}
