using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TearStripLink : MonoBehaviour
{
	[SerializeField]
	private Renderer linkRenderer;

	public bool LastLink { get; set; }

	public Rigidbody LinkRigidbody { get; protected set; }

	public HingeJoint Joint { get; protected set; }

	public Renderer LinkRenderer
	{
		get
		{
			return linkRenderer;
		}
	}

	private void Awake()
	{
		Joint = GetComponent<HingeJoint>();
		linkRenderer = GetComponentInChildren<Renderer>();
		LinkRigidbody = GetComponent<Rigidbody>();
	}

	private void OnDestroy()
	{
		if (Joint != null && Joint.connectedBody != null)
		{
			Object.Destroy(Joint.connectedBody.gameObject);
		}
	}

	public void ChangeParent(Transform newParent)
	{
		base.transform.SetParent(newParent);
		if (Joint != null && Joint.connectedBody != null)
		{
			TearStripLink component = Joint.connectedBody.GetComponent<TearStripLink>();
			if (component != null)
			{
				component.ChangeParent(newParent);
			}
		}
	}
}
