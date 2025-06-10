using System;
using UnityEngine;

namespace AmplitudeAnalytics
{
	public class Constants
	{
		public const int API_VERSION = 2;

		public const int EVENT_UPLOAD_MAX_BATCHES_PER_SECOND = 100;

		public const int EVENT_UPLOAD_MAX_EVENTS_PER_SECOND = 1000;

		public const long SESSION_TIMEOUT_MILLIS = 1800000L;

		public const int MAX_STRING_LENGTH = 1024;

		public const int MAX_PROPERTY_KEYS = 1000;

		public const string SEQUENCE_NUMBER_KEY = "amplitude_sequence_number";

		public const string START_SESSION_EVENT = "session_start";

		public const string END_SESSION_EVENT = "session_end";

		public const int MAX_BATCH_SIZE = 10;

		public static readonly Func<AmplitudeAnalyticsEvent.DeviceInfo> DefaultUnityDeviceInfo = delegate
		{
			string os_name;
			string os_version;
			try
			{
				OperatingSystem oSVersion = Environment.OSVersion;
				os_name = oSVersion.VersionString;
				os_version = oSVersion.Version.ToString();
			}
			catch
			{
				os_name = SystemInfo.operatingSystem;
				os_version = string.Empty;
			}
			return new AmplitudeAnalyticsEvent.DeviceInfo
			{
				platform = SystemInfo.operatingSystemFamily.ToString(),
				os_name = os_name,
				os_version = os_version,
				device_brand = SystemInfo.deviceType.ToString(),
				device_model = SystemInfo.deviceModel,
				carrier = string.Empty
			};
		};
	}
}
