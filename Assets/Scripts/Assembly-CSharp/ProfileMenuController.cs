using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class ProfileMenuController : MenuController
{
	[SerializeField]
	private PlayerProfileView profileView;

	[SerializeField]
	private Button leavePartyButton;

	[SerializeField]
	private DailyObjectiveView[] dailyObjectiveViews;

	[SerializeField]
	private GameObject allObjectivesCompleteImage;

	[SerializeField]
	private Text dailyObjectivesHeader;

	[SerializeField]
	private string dailyObjectivesHeaderDaily = "Daily Challenges:";

	[SerializeField]
	private string dailyObjectivesHeaderOOBE = "Intro Checklist:";

	private bool initialized;

	private void LazyInit()
	{
		if (!initialized && base.Owner != null)
		{
			initialized = true;
			base.Owner.ObjectiveTracker.ObjectiveProgressUpdateEvent += Refresh;
			Images.OnProfileImageUpdated += OnProfileImageUpdated;
		}
	}

	private void OnDestroy()
	{
		if (initialized && base.Owner != null)
		{
			base.Owner.ObjectiveTracker.ObjectiveProgressUpdateEvent -= Refresh;
			Images.OnProfileImageUpdated -= OnProfileImageUpdated;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		if (base.Owner != null)
		{
			LazyInit();
			RefreshLocalPlayer();
			RefreshDailyObjectives();
		}
	}

	private void RefreshLocalPlayer()
	{
		profileView.Refresh(Player.LocalPlayer);
		RefreshLeavePartyButton(Player.LocalPlayer);
	}

	private void RefreshLeavePartyButton(Player player)
	{
		leavePartyButton.gameObject.SetActive(player != null && player.PlayerParty.IsInParty);
	}

	public void LeaveParty()
	{
		Player localPlayer = Player.LocalPlayer;
		if (!(localPlayer == null))
		{
			localPlayer.PlayerParty.LeaveCurrentParty();
			Refresh();
		}
	}

	private void RefreshDailyObjectives()
	{
		Player localPlayer = Player.LocalPlayer;
		if (localPlayer == null)
		{
			return;
		}
		dailyObjectivesHeader.text = ((!SingletonMonoBehaviour<TutorialManager>.Instance.IsOOBERunning) ? dailyObjectivesHeaderDaily : dailyObjectivesHeaderOOBE);
		allObjectivesCompleteImage.gameObject.SetActive(localPlayer.ObjectiveTracker.GetAllObjectivesCompleted());
		for (int i = 0; i < dailyObjectiveViews.Length; i++)
		{
			ProgressionManager.ObjectiveTypeDescription description;
			string descriptionText;
			localPlayer.ObjectiveTracker.GetObjectiveDescription(i, out description, out descriptionText);
			if (description != null)
			{
				int progress;
				int total;
				bool objectiveProgress = localPlayer.ObjectiveTracker.GetObjectiveProgress(i, out progress, out total);
				dailyObjectiveViews[i].UpdateProgress(progress, total, objectiveProgress, descriptionText, description.ToolTip);
			}
		}
	}

	private void OnProfileImageUpdated(ulong id, Texture2D image)
	{
		if (id == Player.LocalPlayer.PlayerId)
		{
			profileView.RefreshPlayerIcon(Player.LocalPlayer);
		}
	}
}
