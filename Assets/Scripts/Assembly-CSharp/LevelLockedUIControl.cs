using UnityEngine;
using UnityEngine.UI;

public class LevelLockedUIControl : MonoBehaviour
{
	[SerializeField]
	private Text lockLevelText;

	public Vector3 Offset { get; set; }

	public void SetLockLevel(int level)
	{
		lockLevelText.text = level.ToString();
	}

	private void LateUpdate()
	{
		base.transform.position = base.transform.parent.position + Offset;
	}
}
