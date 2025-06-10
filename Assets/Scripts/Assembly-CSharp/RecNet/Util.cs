using System;
using System.Collections.Generic;

namespace RecNet
{
	public class Util
	{
		public static T GetKey<T>(string key, Dictionary<string, object> dict)
		{
			return (T)Convert.ChangeType(dict[key], typeof(T));
		}

		public static DateTime GetDateTimeKey(string key, Dictionary<string, object> dict)
		{
			return new DateTime(GetKey<long>(key, dict), DateTimeKind.Utc).ToLocalTime();
		}
	}
}
