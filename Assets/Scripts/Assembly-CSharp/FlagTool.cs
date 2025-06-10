using System.Collections.Generic;
using UnityEngine;

public class FlagTool : Tool
{
	public delegate void FlagAtGoal(FlagTool thisFlag, Player capturePlayer);

	public delegate void FlagRelease(FlagTool thisFlag, Player dropPlayer);

	public delegate void FlagPickup(FlagTool thisFlag, Player pickupPlayer);

	public delegate void FlagReset(FlagTool thisFlag);

	[Header("Team")]
	[SerializeField]
	private GameTeam team = GameTeam.INVALID;

	[Header("Beam")]
	[SerializeField]
	private Renderer beamRenderer;

	private SynchronizedField<bool> _atHomeGoal;

	private SynchronizedField<bool> _atEnemyGoal;

	private FlagToolRenderer flagToolRenderer;

	private List<FlagGoal> homeGoals = new List<FlagGoal>();

	private List<FlagGoal> enemyGoals = new List<FlagGoal>();

	public FlagAtGoal FlagAtGoalEvent;

	public FlagRelease FlagReleaseEvent;

	public FlagPickup FlagPickupEvent;

	public FlagReset FlagResetEvent;

	public bool AtHomeGoal
	{
		get
		{
			return _atHomeGoal.Get();
		}
		private set
		{
			_atHomeGoal.ForceSet(value);
		}
	}

	public bool AtEnemyGoal
	{
		get
		{
			return _atEnemyGoal.Get();
		}
		private set
		{
			_atEnemyGoal.ForceSet(value);
		}
	}

	public GameTeam Team
	{
		get
		{
			return team;
		}
	}

	public Color Color
	{
		set
		{
			flagToolRenderer.AccentColor = value;
			float a = beamRenderer.material.color.a;
			value.a = a;
			beamRenderer.material.color = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		flagToolRenderer = GetComponent<FlagToolRenderer>();
		_atHomeGoal = new SynchronizedField<bool>(this, "AT_HOME_GOAL", false, SetterPermissionMode.AUTHORITY);
		_atEnemyGoal = new SynchronizedField<bool>(this, "AT_ENEMY_GOAL", false, SetterPermissionMode.AUTHORITY);
		FlagGoal[] array = Object.FindObjectsOfType<FlagGoal>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Team == team)
			{
				homeGoals.Add(array[i]);
			}
			else
			{
				enemyGoals.Add(array[i]);
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		Color = GameTeamSettings.GetTeamColor(team);
		base.Rigidbody.isKinematic = true;
	}

	protected override void OnDisable()
	{
		if (base.hasAuthority)
		{
			AtEnemyGoal = false;
			AtHomeGoal = false;
		}
		base.OnDisable();
	}

	private void Update()
	{
		beamRenderer.enabled = true;
		Vector3 position;
		if (base.IsHeld && base.Owner != null)
		{
			if (base.Owner == Player.LocalPlayer)
			{
				beamRenderer.enabled = false;
			}
			position = base.Owner.CurrentFloorPosition;
		}
		else if (!TryGetBeamPosition(out position))
		{
			position = base.transform.position;
		}
		beamRenderer.transform.position = position;
		beamRenderer.transform.rotation = Quaternion.LookRotation(Vector3.up);
		if (!base.hasAuthority)
		{
			return;
		}
		bool atEnemyGoal = AtEnemyGoal;
		bool flag = IsAtGoal(enemyGoals);
		if (flag != atEnemyGoal)
		{
			AtEnemyGoal = flag;
			if (flag && FlagAtGoalEvent != null)
			{
				FlagAtGoalEvent(this, base.Owner);
			}
		}
		bool atHomeGoal = AtHomeGoal;
		bool flag2 = IsAtGoal(homeGoals);
		if (atHomeGoal != flag2)
		{
			AtHomeGoal = flag2;
		}
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
		if (base.hasAuthority && FlagPickupEvent != null)
		{
			FlagPickupEvent(this, player);
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		if (base.hasAuthority && FlagReleaseEvent != null)
		{
			FlagReleaseEvent(this, player);
		}
	}

	protected override void OnReset(Vector3 position, Quaternion rotation, bool wasEnabled, Vector3 oldPosition, bool wasCleanedUp)
	{
		base.OnReset(position, rotation, wasEnabled, oldPosition, wasCleanedUp);
		base.Rigidbody.isKinematic = true;
		if (wasCleanedUp && FlagResetEvent != null)
		{
			FlagResetEvent(this);
		}
	}

	private bool IsAtGoal(List<FlagGoal> goals)
	{
		bool result = false;
		for (int i = 0; i < goals.Count; i++)
		{
			if (Vector3.ProjectOnPlane(goals[i].transform.position - base.transform.position, Vector3.up).sqrMagnitude <= goals[i].Radius * goals[i].Radius)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private bool TryGetBeamPosition(out Vector3 position)
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(base.transform.position + Vector3.up * 0.1f, -Vector3.up, out hitInfo, float.MaxValue, 2048, QueryTriggerInteraction.Ignore))
		{
			position = hitInfo.point;
			return true;
		}
		position = Vector3.zero;
		return false;
	}
}
