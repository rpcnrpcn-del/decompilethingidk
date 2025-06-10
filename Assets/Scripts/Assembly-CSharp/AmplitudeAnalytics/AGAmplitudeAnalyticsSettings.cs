using UnityEngine;

namespace AmplitudeAnalytics
{
	public class AGAmplitudeAnalyticsSettings : MonoBehaviour
	{
		[SerializeField]
		private AmplitudeAnalyticsClient.Settings editorSettings;

		[SerializeField]
		private bool forceEditorSettingsForDevelopers;

		public AmplitudeAnalyticsClient.Settings EditorSettings
		{
			get
			{
				return editorSettings;
			}
		}

		public bool ForceEditorSettingsForDevelopers
		{
			get
			{
				return forceEditorSettingsForDevelopers;
			}
		}
	}
}
