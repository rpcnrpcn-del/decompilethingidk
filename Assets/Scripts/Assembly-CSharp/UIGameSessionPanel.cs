using RecNet;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGameSessionPanel : Selectable
{
	[Header("Room specific")]
	[SerializeField]
	private Button joinButton;

	[SerializeField]
	private Text labelText;

	private bool canJoin;

	private GameSession _session;

	public GameSession Session
	{
		get
		{
			return _session;
		}
		set
		{
			_session = value;
			string text = string.Empty;
			canJoin = false;
			if (_session != null)
			{
				text = string.Format("{3} - ({0}/{1}) ({2}) {4}", _session.PlayerIds.Count, _session.AvailableSpace + _session.PlayerIds.Count, (!_session.GameInProgress) ? "pregame" : "playing", _session.Activity, (!_session.Private) ? string.Empty : "(private)");
				canJoin = _session.AvailableSpace > 0;
			}
			joinButton.interactable = canJoin;
			labelText.text = text;
		}
	}

	public DebugGameSessionsController ParentMenu { get; set; }

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		ParentMenu.PrintExtraInfo(Session);
	}

	public void Button_JoinRoom()
	{
		if (canJoin)
		{
			PUNNetworkManager.Instance.JoinRoom(Session.Id);
		}
	}
}
