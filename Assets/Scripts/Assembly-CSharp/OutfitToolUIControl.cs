using System;
using UnityEngine;
using UnityEngine.UI;

public class OutfitToolUIControl : MonoBehaviour
{
	[Flags]
	public enum ItemProgressionState
	{
		None = 0,
		Locked = 1,
		New = 2
	}

	[Header("Locked Item Controls")]
	[SerializeField]
	private Text lockLevelText;

	[SerializeField]
	private Image lockIcon;

	[Header("New Item Controls")]
	[SerializeField]
	private GameObject newItemIcon;

	[SerializeField]
	private ItemProgressionState _state;

	public Vector3 Offset { get; set; }

	public ItemProgressionState State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
			lockLevelText.gameObject.SetActive(false);
			lockIcon.gameObject.SetActive(false);
			newItemIcon.gameObject.SetActive(false);
			if ((_state & ItemProgressionState.Locked) == ItemProgressionState.Locked)
			{
				lockLevelText.gameObject.SetActive(true);
				lockIcon.gameObject.SetActive(true);
			}
			if ((_state & ItemProgressionState.New) == ItemProgressionState.New)
			{
				newItemIcon.gameObject.SetActive(true);
			}
		}
	}

	public void SetLockLevel(int level)
	{
		lockLevelText.text = level.ToString();
	}

	private void LateUpdate()
	{
		base.transform.position = base.transform.parent.position + Offset;
	}
}
