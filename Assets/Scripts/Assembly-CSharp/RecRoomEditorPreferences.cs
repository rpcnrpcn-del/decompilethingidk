public static class RecRoomEditorPreferences
{
	private static bool prefsLoaded;

	public static string ConnectToHMDPref = "ConnectToHMD";

	private static bool ConnectToHMD = true;

	private static string UseDeveloperModePref = "UseDeveloperMode";

	public static bool UseDeveloperMode = true;

	public static string CheckActivityPlayerRequirementsPref = "CheckActivityPlayerRequirements";

	public static bool CheckActivityPlayerRequirements = true;

	public static string MasterAutoStartCurrentGamePref = "MasterAutoStartCurrentGame";

	public static bool MasterAutoStartCurrentGame;

	public static string DebugLogRecNetPref = "DebugLogRecNet";

	public static bool DebugLogRecNet;

	public static string EnableGiftSpawnerPref = "EnableGiftSpawner";

	public static bool EnableGiftGenerator;

	public static string PhotonConnectionVersionOverridePref = "PhotonConnectionVersionOverride";

	private static string photonConnectionVersionOverride;
}
