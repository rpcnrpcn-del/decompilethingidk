using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PlayerSubMenu : MonoBehaviour
{
	public static List<PlayerSubMenu> All = new List<PlayerSubMenu>();

	public static Stack<PlayerSubMenu> SelectStack = new Stack<PlayerSubMenu>();

	public MenuController Controller;

	public Button TabHeaderButton;

	[SerializeField]
	private Color selectedHeaderButtonColor = new Color32(230, 103, 55, 240);

	[Header("Debug")]
	[Tooltip("List of items we do not want to show up in release builds.")]
	public List<GameObject> NotForRelease;

	private Color defaultSelectedHeaderButtonColor = Color.clear;

	private bool _selected;

	private bool defaultInterractable;

	private TabbedPanel[] tabbedPanels;

	public PlayerMenu PlayerMenu { get; set; }

	public bool Selected
	{
		get
		{
			return _selected;
		}
		set
		{
			if (_selected == value)
			{
				return;
			}
			_selected = value;
			if (_selected)
			{
				foreach (PlayerSubMenu item in All)
				{
					if (item != this)
					{
						item.Selected = false;
					}
				}
			}
			Controller.Visible = _selected;
			if (TabHeaderButton != null && TabHeaderButton.image != null)
			{
				TabHeaderButton.image.color = ((!_selected) ? defaultSelectedHeaderButtonColor : selectedHeaderButtonColor);
			}
			UpdateSelectStack();
			PlayerMenu.MarkDirty();
		}
	}

	public bool Interactable
	{
		set
		{
			if (TabHeaderButton != null && defaultInterractable)
			{
				TabHeaderButton.interactable = value;
			}
		}
	}

	protected virtual void Awake()
	{
		if (!SessionManager.IsDeveloper)
		{
			foreach (GameObject item in NotForRelease)
			{
				if (item != null)
				{
					item.SetActive(false);
				}
			}
		}
		if (TabHeaderButton != null)
		{
			if (TabHeaderButton.image != null)
			{
				defaultSelectedHeaderButtonColor = TabHeaderButton.image.color;
			}
			defaultInterractable = TabHeaderButton.interactable;
		}
		if (Controller != null)
		{
			Controller.VisibleChanged += Controller_VisibleChanged;
		}
		tabbedPanels = GetComponentsInChildren<TabbedPanel>(true);
		if (tabbedPanels != null && tabbedPanels.Length > 0)
		{
			for (int i = 0; i < tabbedPanels.Length; i++)
			{
				tabbedPanels[i].gameObject.SetActive(true);
				tabbedPanels[i].OnSelected += TabbedPabel_OnSelected;
			}
		}
		All.Add(this);
	}

	private void TabbedPabel_OnSelected(TabbedPanel panel, bool selected)
	{
		if (!selected)
		{
			return;
		}
		for (int i = 0; i < tabbedPanels.Length; i++)
		{
			if (panel != tabbedPanels[i])
			{
				tabbedPanels[i].IsSelected = false;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		All.Remove(this);
		if (SelectStack != null)
		{
			SelectStack.Clear();
		}
	}

	public virtual void Refresh()
	{
		if (Selected && Controller != null)
		{
			Controller.Refresh();
		}
	}

	private void Controller_VisibleChanged(MenuController controller, bool cVisible)
	{
		Selected = cVisible;
	}

	private void UpdateSelectStack()
	{
		if (!Selected)
		{
			if (SelectStack.Count > 0 && SelectStack.Peek() == this)
			{
				SelectStack.Pop();
			}
			bool flag = false;
			foreach (PlayerSubMenu item in All)
			{
				if (item.Selected)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (SelectStack.Count == 0)
				{
					PlayerMenu.ShowDefaultTab();
				}
				else
				{
					SelectStack.Peek().Selected = true;
				}
			}
		}
		else if (_selected && (SelectStack.Count == 0 || SelectStack.Peek() == this))
		{
			SelectStack.Push(this);
		}
	}

	public void ButtonPress_SubMenuSelected()
	{
		Selected = true;
	}

	public void Button_HideMenu()
	{
		PlayerMenu.Visible = false;
	}
}
