using UnityEngine;

public class BuddyEntity : SingletonMonoBehaviour<BuddyEntity>
{
	[SerializeField]
	private Transform visualRoot;

	private FollowCamera followCamera;

	public bool Visible
	{
		set
		{
			visualRoot.gameObject.SetActive(value);
			followCamera.ForceUpdate();
		}
	}

	private void Awake()
	{
		SingletonMonoBehaviour<BuddyEntity>.Instance = this;
		followCamera = GetComponent<FollowCamera>();
	}

	private void Start()
	{
		followCamera.ForceUpdate();
		Visible = false;
	}
}
