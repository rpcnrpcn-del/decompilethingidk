using UnityEngine;

public static class CustomSettingsLoader
{
	private const string ResourcesDirectoryName = "Resources";

	public static T LoadConfig<T>() where T : ScriptableObject
	{
		return (T)Resources.Load(GetAssetFileName<T>(), typeof(T));
	}

	private static string GetAssetFileName<T>()
	{
		return typeof(T).Name;
	}

	private static string GetTrimmedFilePath(string filePath)
	{
		filePath = filePath.Replace("\\", "/");
		if (filePath.EndsWith("/"))
		{
			filePath.Remove(filePath.Length - 1, 1);
		}
		if (!filePath.EndsWith("Resources"))
		{
			filePath += "/Resources";
		}
		return filePath;
	}
}
