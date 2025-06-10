using System.Collections.Generic;
using UnityEngine;

public class BasketballRim : MonoBehaviour
{
	[SerializeField]
	private CollisionForwarder rimTop;

	[SerializeField]
	private CollisionForwarder rimBottom;

	private List<Basketball> trackedBalls = new List<Basketball>();

	private void Start()
	{
		rimTop.TriggerEnter += RimTop_TriggerEnter;
		rimBottom.TriggerEnter += RimBottom_TriggerEnter;
	}

	private void RimTop_TriggerEnter(Collider trigger)
	{
		Basketball basketballTool = GetBasketballTool(trigger);
		if (basketballTool != null && !trackedBalls.Contains(basketballTool))
		{
			trackedBalls.Add(basketballTool);
		}
	}

	private void RimBottom_TriggerEnter(Collider trigger)
	{
		Basketball basketballTool = GetBasketballTool(trigger);
		if (basketballTool != null && trackedBalls.Contains(basketballTool))
		{
			trackedBalls.Remove(basketballTool);
			basketballTool.PassedRim();
		}
	}

	private Basketball GetBasketballTool(Collider collider)
	{
		ToolCollider component = collider.GetComponent<ToolCollider>();
		if (component != null)
		{
			return component.Tool as Basketball;
		}
		return null;
	}

	private void Update()
	{
		for (int num = trackedBalls.Count - 1; num >= 0; num--)
		{
			if (trackedBalls[num].transform.position.y < rimBottom.transform.position.y)
			{
				trackedBalls.RemoveAt(num);
			}
		}
	}
}
