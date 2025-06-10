public static class DefaultVFXSettings
{
	private static DefaultVFXConfig ConfigAsset = CustomSettingsLoader.LoadConfig<DefaultVFXConfig>();

	private const string ConfigsAssetFilePath = "Assets/Core/Content/Configs/";

	public static bool TryLoadDefaultVFX(FxType vfxType, out VFXAssets vfxAsset)
	{
		vfxAsset = null;
		if (ConfigAsset != null)
		{
			for (int i = 0; i < ConfigAsset.DefaultVFXs.Length; i++)
			{
				if (ConfigAsset.DefaultVFXs[i].VfxType == vfxType)
				{
					vfxAsset = ConfigAsset.DefaultVFXs[i];
					return true;
				}
			}
		}
		return false;
	}
}
