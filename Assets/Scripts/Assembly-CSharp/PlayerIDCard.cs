using UnityEngine;

public class PlayerIDCard : MonoBehaviour
{
	[Tooltip("Z - Distance from the menu we are following")]
	[SerializeField]
	private float idCardDepth = 0.02f;

	[SerializeField]
	private TextMesh nameMesh;

	public Renderer AvatarImageRenderer;

	private void Awake()
	{
		AvatarImageRenderer.material = new Material(AvatarImageRenderer.material);
	}

	public void SetName(string name)
	{
		nameMesh.text = name;
	}

	public void SetFollow(MenuController menu)
	{
		base.transform.SetParent(menu.transform, false);
		base.transform.localPosition = Vector3.back * idCardDepth;
		base.gameObject.SetActive(true);
	}
}
