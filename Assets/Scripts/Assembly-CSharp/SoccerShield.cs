using UnityEngine;

public class SoccerShield : Tool
{
	[Header("Powerup Visuals")]
	[SerializeField]
	private TeleportChargeVisual powerupBar;

	[SerializeField]
	private Transform powerupVisualRoot;

	[SerializeField]
	private float powerupDepletionRateSeconds = 15f;

	[Header("Audio")]
	[SerializeField]
	private RecRoomAudioClip powerupStart;

	private float fillPowerup;

	private float powerupFillSpeed = 1f;

	private float _powerupCharge;

	private bool _isLocalUsingPowerup;

	private bool _usingPowerup;

	public float PowerupCharge
	{
		get
		{
			return _powerupCharge;
		}
		private set
		{
			_powerupCharge = Mathf.Clamp01(value);
			powerupBar.Charge = PowerupCharge;
			if (PowerupCharge == 0f && UsingPowerup)
			{
				UsingPowerup = false;
			}
		}
	}

	public bool UsingPowerup
	{
		get
		{
			return _usingPowerup;
		}
		private set
		{
			bool flag = value && PowerupCharge > 0f;
			if (_usingPowerup == flag)
			{
				return;
			}
			_usingPowerup = flag;
			UpdatePowerupFX();
			if (!(base.Owner != null))
			{
				return;
			}
			if (flag)
			{
				_isLocalUsingPowerup = base.isLocal;
				if (_isLocalUsingPowerup)
				{
					Player.LocalPlayer.PlayerLocomotion.PushTeleportMaxDistanceScalar(2f);
					Player.LocalPlayer.PlayerLocomotion.PushMaxToolHitHoldTime(0.1f);
				}
			}
			else if (_isLocalUsingPowerup)
			{
				Player.LocalPlayer.PlayerLocomotion.PopTeleportMaxDistanceScalar();
				Player.LocalPlayer.PlayerLocomotion.PopMaxToolHitHoldTime();
			}
		}
	}

	public Vector3 PowerupVisualPosition
	{
		get
		{
			return powerupVisualRoot.position;
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	private void Update()
	{
		if (!base.IsHeld || !(base.Owner != null) || !base.Owner.isLocal)
		{
			return;
		}
		if (fillPowerup > 0f)
		{
			float num = Time.deltaTime * powerupFillSpeed;
			fillPowerup -= num;
			PowerupCharge += num;
			if (PowerupCharge >= 1f)
			{
				fillPowerup = 0f;
			}
		}
		if (UsingPowerup)
		{
			float num2 = Time.deltaTime * 1f / powerupDepletionRateSeconds;
			PowerupCharge -= num2;
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		if (player != null && player.isLocal && RecRoomSceneManager.Instance != null)
		{
			PowerupCharge = 0f;
			UsingPowerup = false;
		}
	}

	public override void OnInputDown()
	{
		UsingPowerup = true;
	}

	public override void OnPowerup(Powerup powerup)
	{
		base.OnPowerup(powerup);
		if (base.Owner != null && base.Owner.isLocal)
		{
			fillPowerup = powerup.PowerupValue;
		}
	}

	private void UpdatePowerupFX()
	{
		powerupVisualRoot.gameObject.SetActive(UsingPowerup);
		if (UsingPowerup)
		{
			AudioManager.Play3DSFX(powerupStart, base.transform);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isReading)
		{
			UsingPowerup = (bool)stream.ReceiveNext();
			PowerupCharge = (float)stream.ReceiveNext();
		}
		else
		{
			stream.SendNext(UsingPowerup);
			stream.SendNext(PowerupCharge);
		}
	}
}
