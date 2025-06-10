using UnityEngine;

public class CollisionForwarder : MonoBehaviour
{
	public delegate void TriggerEventHandler(Collider trigger);

	public delegate void CollisionEventHandler(Collision collision);

	public Rigidbody Rigidbody { get; private set; }

	public event TriggerEventHandler TriggerEnter;

	public event TriggerEventHandler TriggerStay;

	public event TriggerEventHandler TriggerExit;

	public event CollisionEventHandler CollisionEnter;

	public event CollisionEventHandler CollisionStay;

	public event CollisionEventHandler CollisionExit;

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (this.TriggerEnter != null)
		{
			this.TriggerEnter(collider);
		}
	}

	private void OnTriggerStay(Collider collider)
	{
		if (this.TriggerStay != null)
		{
			this.TriggerStay(collider);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (this.TriggerExit != null)
		{
			this.TriggerExit(collider);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (this.CollisionEnter != null)
		{
			this.CollisionEnter(collision);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (this.CollisionStay != null)
		{
			this.CollisionStay(collision);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (this.CollisionExit != null)
		{
			this.CollisionExit(collision);
		}
	}
}
