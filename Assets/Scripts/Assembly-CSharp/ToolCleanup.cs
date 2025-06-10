using Photon;
using UnityEngine;

[RequireComponent(typeof(Tool))]
public class ToolCleanup : Photon.MonoBehaviour
{
	[Tooltip("Whether or not this tool should be excluded from cleanup")]
	public bool ExcludeFromCleanup;

	[Tooltip("Min distance from original spawn position before a tool is considered eligible for cleanup")]
	public float MinDisplacementBeforeCleanup = 0.1f;

	[Tooltip("Min distance from the closest player before a tool is considered eligible for cleanup")]
	public float MinPlayerDistanceBeforeCleanup = 3f;

	[Tooltip("Delay before eligible tools are cleaned up")]
	public float DelayUntilCleanup = 15f;

	[Tooltip("Whether to try and check if this object moved or rotated only after its rigidbody has come to rest")]
	public bool TestRigidbody;

	[Header("Rotation")]
	[Tooltip("Whether to cleanup this tool after a rotation has occured")]
	public bool TestRotation;

	[Tooltip("How many degrees this object must have rotated in order to be considered rotated")]
	public float MinRotationAngleBeforeCleanup = 30f;

	private Tool tool;

	private Vector3 originalPosition;

	private Quaternion originalRotation;

	private bool isResetting;

	private bool hasMoved;

	private float resetTimerStart;

	private bool HasTranslated
	{
		get
		{
			return (!TestRigidbody || tool.Rigidbody.IsSleeping()) && Vector3.Distance(base.transform.position, originalPosition) > MinDisplacementBeforeCleanup;
		}
	}

	private bool HasRotated
	{
		get
		{
			return (!TestRigidbody || tool.Rigidbody.IsSleeping()) && Quaternion.Angle(originalRotation, base.transform.rotation) >= MinRotationAngleBeforeCleanup;
		}
	}

	private bool IsCloseToAnyPlayer
	{
		get
		{
			return MinPlayerDistanceBeforeCleanup > 0f && Player.IsCloseToAnyPlayer(base.transform.position, MinPlayerDistanceBeforeCleanup);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		tool = GetComponent<Tool>();
		tool.ResetEvent += OnReset;
		OnReset(tool, base.transform.position, base.transform.rotation);
	}

	private void OnReset(Tool tool, Vector3 position, Quaternion rotation)
	{
		ResetCleanupOriginalPosition(position, rotation);
	}

	public void ResetCleanupOriginalPosition(Vector3 position, Quaternion rotation)
	{
		isResetting = false;
		hasMoved = false;
		originalPosition = position;
		originalRotation = rotation;
	}

	public void CheckCleanup()
	{
		if (!isResetting && !(tool == null))
		{
			if (!hasMoved || !base.hasAuthority || !base.gameObject.activeSelf)
			{
				hasMoved = HasTranslated || HasRotated;
				resetTimerStart = Time.time;
			}
			else if (tool.IsHeld || !base.enabled || IsCloseToAnyPlayer)
			{
				resetTimerStart = Time.time;
			}
			else if (resetTimerStart + DelayUntilCleanup < Time.time)
			{
				isResetting = true;
				tool.AuthorityResetToDefault(originalPosition, originalRotation, true);
			}
		}
	}
}
