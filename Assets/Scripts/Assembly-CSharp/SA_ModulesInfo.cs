using System;
using System.Reflection;
using UnityEngine;

public class SA_ModulesInfo
{
	public const int VERSION_UNDEFINED = 0;

	public const string VERSION_UNDEFINED_STRING = "Undefined";

	public static string FB_SDK_VersionCode
	{
		get
		{
			string result = "Undefined";
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type type = assembly.GetType("Facebook.FBBuildVersionAttribute");
				Type type2 = assembly.GetType("Facebook.IFacebook");
				if (type2 == null || type == null)
				{
					continue;
				}
				MethodInfo method = type.GetMethod("GetVersionAttributeOfType", BindingFlags.Static | BindingFlags.Public);
				if (method == null)
				{
					break;
				}
				object obj = method.Invoke(null, new object[1] { type2 });
				PropertyInfo property = type.GetProperty("SdkVersion");
				if (obj != null && property != null)
				{
					string text = property.GetValue(obj, null) as string;
					if (text != null)
					{
						result = text;
					}
				}
				break;
			}
			Type type3 = Type.GetType("Facebook.Unity.FacebookSdkVersion");
			if (type3 != null)
			{
				PropertyInfo property2 = type3.GetProperty("Build", BindingFlags.Static | BindingFlags.Public);
				if (property2 != null)
				{
					result = (string)property2.GetValue(null, null);
				}
			}
			return result;
		}
	}

	public static int FB_SDK_MajorVersionCode
	{
		get
		{
			string fB_SDK_VersionCode = FB_SDK_VersionCode;
			int result = 0;
			if (fB_SDK_VersionCode.Equals("Undefined"))
			{
				return result;
			}
			try
			{
				string[] array = fB_SDK_VersionCode.Split('.');
				result = Convert.ToInt32(array[0]);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("FB_SDK_MajorVersionCode failed: " + ex.Message);
			}
			return result;
		}
	}
}
