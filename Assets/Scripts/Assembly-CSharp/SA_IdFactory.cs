using System;
using System.Linq;
using UnityEngine;

public class SA_IdFactory
{
	private const string PP_ID_KEY = "SA_IdFactory_Key";

	public static int NextId
	{
		get
		{
			int num = 1;
			if (PlayerPrefs.HasKey("SA_IdFactory_Key"))
			{
				num = PlayerPrefs.GetInt("SA_IdFactory_Key");
				num++;
			}
			PlayerPrefs.SetInt("SA_IdFactory_Key", num);
			return num;
		}
	}

	public static string RandomString
	{
		get
		{
			string element = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			System.Random random = new System.Random();
			return new string((from s in Enumerable.Repeat(element, 8)
				select s[random.Next(s.Length)]).ToArray());
		}
	}
}
