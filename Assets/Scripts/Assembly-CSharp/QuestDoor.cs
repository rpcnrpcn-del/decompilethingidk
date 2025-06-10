using UnityEngine;

public class QuestDoor : TeleportationPortal
{
	[SerializeField]
	private GameObject lockVisual;

	private bool _isLocked;

	public bool IsLocked
	{
		get
		{
			return _isLocked;
		}
		set
		{
			_isLocked = value;
			if (lockVisual != null)
			{
				lockVisual.gameObject.SetActive(_isLocked);
			}
		}
	}

	public override bool SupportsPlayerRespawn
	{
		get
		{
			return true;
		}
	}

	public override bool IsValid
	{
		get
		{
			return !IsLocked && base.IsValid;
		}
	}
}
