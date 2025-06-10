using UnityEngine;

public class GamePracticeObject : MonoBehaviour
{
	[SerializeField]
	private bool playAttractEffect = true;

	private Tool thisTool;

	public bool PlayAttractEffect
	{
		get
		{
			return playAttractEffect;
		}
	}

	private void Awake()
	{
		thisTool = GetComponent<Tool>();
	}

	public void Disable()
	{
		if (thisTool == null)
		{
			base.gameObject.SetActive(false);
		}
		else if (PhotonNetwork.isMasterClient)
		{
			thisTool.IsEnabled = false;
		}
	}

	public void Enable()
	{
		if (thisTool == null)
		{
			base.gameObject.SetActive(true);
		}
		else if (PhotonNetwork.isMasterClient)
		{
			thisTool.IsEnabled = true;
		}
	}
}
