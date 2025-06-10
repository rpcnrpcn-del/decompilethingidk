using System;
using UnityEngine;

public class MenuController : MonoBehaviour
{
	[SerializeField]
	private Transform visualRoot;

	[SerializeField]
	private RecRoomAudioClip menuVisibleAudio;

	[SerializeField]
	protected MenuController subMenu;

	protected GameManager gameManager;

	private bool _visible;

	public Player Owner { get; set; }

	public PlayerMenu PlayerMenu { get; set; }

	public virtual bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (_visible != value)
			{
				_visible = value;
				visualRoot.gameObject.SetActive(_visible);
				if (_visible)
				{
					AudioManager.Play3DSFX(menuVisibleAudio, base.transform.position);
				}
				else
				{
					ShowSubMenu(false);
				}
				if (this.VisibleChanged != null)
				{
					this.VisibleChanged(this, _visible);
				}
			}
		}
	}

	public event Action<MenuController, bool> VisibleChanged;

	protected virtual void Awake()
	{
		if (visualRoot == null)
		{
			visualRoot = base.transform;
		}
		visualRoot.gameObject.SetActive(_visible);
	}

	public virtual void Initialize(PlayerMenu playerMenu)
	{
		PlayerMenu = playerMenu;
		Owner = playerMenu.ThisPlayer;
		gameManager = RecRoomSceneManager.Instance.GameManager;
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	public virtual void Refresh()
	{
		if (subMenu != null && subMenu.Visible)
		{
			subMenu.Refresh();
		}
	}

	public void Button_HideSelf()
	{
		Visible = false;
	}

	public void Button_HideSubMenu()
	{
		ShowSubMenu(false);
	}

	public void Button_ShowSubMenu()
	{
		ShowSubMenu(true);
	}

	public bool ShowSubMenu(bool show)
	{
		if (subMenu != null && subMenu.Visible != show)
		{
			subMenu.Visible = show;
			return true;
		}
		return false;
	}
}
