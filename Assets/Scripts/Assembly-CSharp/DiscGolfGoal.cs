using Photon;
using UnityEngine;

public class DiscGolfGoal : Photon.MonoBehaviour
{
	[Header("Hole")]
	[SerializeField]
	private DiscGolfHole hole = DiscGolfHole.INVALID;

	[SerializeField]
	private int par = 3;

	[Header("Feedback")]
	[SerializeField]
	private Renderer beamRenderer;

	public DiscGolfHole Hole
	{
		get
		{
			return hole;
		}
	}

	public int Par
	{
		get
		{
			return par;
		}
	}

	public bool BeamEnabled
	{
		get
		{
			return beamRenderer.gameObject.activeSelf;
		}
		set
		{
			beamRenderer.gameObject.SetActive(value);
		}
	}
}
