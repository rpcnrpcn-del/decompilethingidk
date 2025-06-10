using UnityEngine;

public class PlayerBody : MonoBehaviour
{
	[SerializeField]
	private Rigidbody torsoCollider;

	private bool _visible = true;

	public Player Player { get; private set; }

	public Rigidbody Rigidbody { get; private set; }

	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (value != _visible)
			{
				_visible = value;
				for (int i = 0; i < base.transform.childCount; i++)
				{
					base.transform.GetChild(i).gameObject.SetActive(_visible);
				}
				torsoCollider.gameObject.SetActive(_visible);
			}
		}
	}

	public Vector3 ToHead
	{
		get
		{
			return Player.Head.transform.position - base.transform.position;
		}
	}

	public Vector3 HeightOffset
	{
		get
		{
			return Vector3.Project(base.transform.position - Player.CurrentFloorPosition, Vector3.up);
		}
	}

	private void Awake()
	{
		Player = GetComponentInParent<Player>();
		Rigidbody = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		torsoCollider.MovePosition(Rigidbody.position);
		torsoCollider.MoveRotation(Rigidbody.rotation);
	}
}
