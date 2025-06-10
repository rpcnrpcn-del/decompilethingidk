using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class DebugGameSessionsController : MenuController
{
	[SerializeField]
	private UIGameSessionPanel roomPanelPrefab;

	[SerializeField]
	private RectTransform panelRoot;

	[SerializeField]
	private Text extraInfo;

	[SerializeField]
	private RectTransform noGamesPanel;

	[Header("Settings")]
	[SerializeField]
	private Toggle showDormToggle;

	[SerializeField]
	private Toggle showOnlySpaceAvailableToggle;

	private List<UIGameSessionPanel> sessionPanels;

	private List<GameSession> gameSessions;

	public override bool Visible
	{
		set
		{
			base.Visible = value;
			if (value)
			{
				RefreshGameSessionsFromServer();
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		roomPanelPrefab.gameObject.SetActive(false);
		sessionPanels = new List<UIGameSessionPanel>();
	}

	public override void Refresh()
	{
		base.Refresh();
		UpdateGameSessions();
	}

	private void RefreshGameSessionsFromServer()
	{
		extraInfo.text = "Downloading game sessions from server...";
		GameSessions.GetAllGameSessionsFromServer(delegate(string e, List<GameSession> sessions)
		{
			if (string.IsNullOrEmpty(e) && sessions != null)
			{
				gameSessions = sessions;
				extraInfo.text = "Sessions dowloaded!";
			}
			else
			{
				gameSessions = null;
				Debug.LogError("Failed to refresh game sessions.");
				extraInfo.text = e;
			}
			Refresh();
		});
	}

	private void UpdateGameSessions()
	{
		List<GameSession> list = gameSessions;
		if (list != null && list.Count > 0)
		{
			foreach (GameSession item in list)
			{
				if (string.IsNullOrEmpty(item.Activity) || item.PlayerIds.Count == 0)
				{
					continue;
				}
				bool flag = true;
				foreach (UIGameSessionPanel sessionPanel in sessionPanels)
				{
					if (sessionPanel.Session.Id == item.Id)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					AddGamePanel(item);
				}
			}
		}
		UIGameSessionPanel[] array = sessionPanels.ToArray();
		foreach (UIGameSessionPanel uIGameSessionPanel in array)
		{
			bool flag2 = false;
			foreach (GameSession item2 in list)
			{
				if (uIGameSessionPanel.Session.Id == item2.Id)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				sessionPanels.Remove(uIGameSessionPanel);
				UnityEngine.Object.Destroy(uIGameSessionPanel.gameObject);
			}
		}
		if (sessionPanels.Count > 0)
		{
			sessionPanels.Sort(delegate(UIGameSessionPanel c1, UIGameSessionPanel c2)
			{
				int num2 = c1.Session.Activity.CompareTo(c2.Session.Activity);
				int num3 = c1.Session.AvailableSpace.CompareTo(c2.Session.AvailableSpace);
				int num4 = c1.Session.Id.CompareTo(c2.Session.Id);
				return (num2 != 0) ? num2 : ((num3 == 0) ? num4 : num3);
			});
			for (int num = 0; num < sessionPanels.Count; num++)
			{
				sessionPanels[num].transform.SetSiblingIndex(num);
				if (sessionPanels[num].Session.Activity == "dormroom")
				{
					sessionPanels[num].gameObject.SetActive(showDormToggle.isOn && !showOnlySpaceAvailableToggle.isOn);
				}
				else
				{
					sessionPanels[num].gameObject.SetActive(!showOnlySpaceAvailableToggle.isOn || sessionPanels[num].Session.AvailableSpace > 0);
				}
			}
		}
		noGamesPanel.gameObject.SetActive(sessionPanels.Count == 0);
	}

	private void AddGamePanel(GameSession session)
	{
		if (session != null)
		{
			UIGameSessionPanel uIGameSessionPanel = UnityEngine.Object.Instantiate(roomPanelPrefab);
			uIGameSessionPanel.transform.SetParent(panelRoot, false);
			uIGameSessionPanel.gameObject.SetActive(true);
			uIGameSessionPanel.Session = session;
			uIGameSessionPanel.ParentMenu = this;
			sessionPanels.Add(uIGameSessionPanel);
		}
	}

	public void PrintExtraInfo(GameSession session)
	{
		StopAllCoroutines();
		StartCoroutine(RunPrintExtraInfo(session));
	}

	public IEnumerator RunPrintExtraInfo(GameSession session)
	{
		extraInfo.text = "Loading...";
		yield return Profiles.Get(session.PlayerIds, delegate(string error, List<Profile> profiles)
		{
			if (string.IsNullOrEmpty(error))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(session.Id);
				stringBuilder.AppendLine(session.Activity);
				stringBuilder.AppendLine();
				foreach (Profile profile in profiles)
				{
					stringBuilder.AppendFormat("({0}) {1} ({2})", profile.Level, profile.DisplayName, profile.Username);
					stringBuilder.AppendLine();
				}
				extraInfo.text = stringBuilder.ToString();
			}
			else
			{
				extraInfo.text = "Failed to load RecNet profiles" + Environment.NewLine + error;
			}
		});
	}
}
