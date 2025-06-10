using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmplitudeAnalytics
{
	[Serializable]
	public class AmplitudeAnalyticsEvent
	{
		[Serializable]
		public class DeviceInfo
		{
			public string platform;

			public string os_name;

			public string os_version;

			public string device_brand;

			public string device_manufacturer;

			public string device_model;

			public string device_type;

			public string carrier;

			public void AddToEvent(Dictionary<string, object> event_properties)
			{
				event_properties["platform"] = platform;
				event_properties["os_name"] = os_name;
				event_properties["os_version"] = os_version;
				event_properties["device_brand"] = device_brand;
				event_properties["device_manufacturer"] = device_manufacturer;
				event_properties["device_model"] = device_model;
				event_properties["device_type"] = device_type;
				event_properties["carrier"] = carrier;
			}
		}

		[Serializable]
		public class GeographicInfo
		{
			public string country;

			public string region;

			public string city;

			public string designatedMarketArea;

			public void AddToEvent(Dictionary<string, object> event_properties)
			{
				event_properties["country"] = country;
				event_properties["region"] = region;
				event_properties["city"] = city;
				event_properties["dma"] = designatedMarketArea;
			}
		}

		[Serializable]
		public class RevenueData
		{
			public float price;

			public int quantity = 1;

			public float revenue;

			public string productId;

			public string revenueType;

			public void AddToEvent(Dictionary<string, object> event_properties)
			{
				event_properties["price"] = price;
				event_properties["quantity"] = quantity;
				event_properties["revenue"] = revenue;
				event_properties["productId"] = productId;
				event_properties["revenueType"] = revenueType;
			}
		}

		public string uuid;

		public string event_type;

		public string user_id;

		public string device_id;

		public Dictionary<string, object> event_properties;

		public Dictionary<string, object> user_properties;

		public Dictionary<string, object> groups;

		public DeviceInfo deviceInfo;

		public GeographicInfo geographicInfo;

		public RevenueData revenueData;

		public string language;

		public float? location_lat;

		public float? location_lng;

		public string ip;

		public string idfa;

		public string adid;

		public long time;

		public long sequenceNumber;

		public long sessionId;

		public string buildVersion;

		public string UserId
		{
			get
			{
				return user_id;
			}
		}

		internal AmplitudeAnalyticsEvent(string event_type, long sessionId, long sequenceNumber, string user_id = null)
		{
			this.event_type = event_type;
			this.user_id = user_id;
			this.sessionId = sessionId;
			this.sequenceNumber = sequenceNumber;
			device_id = SystemInfo.deviceUniqueIdentifier;
			uuid = Guid.NewGuid().ToString();
			buildVersion = BuildSettings.Version;
			time = AmplitudeAnalyticsClient.UTCMillisSinceEpoch();
		}

		public AmplitudeAnalyticsEvent WithDeviceInfo(DeviceInfo deviceInfo)
		{
			this.deviceInfo = deviceInfo;
			return this;
		}

		public AmplitudeAnalyticsEvent WithGeographicInfo(GeographicInfo geographicInfo)
		{
			this.geographicInfo = geographicInfo;
			return this;
		}

		public AmplitudeAnalyticsEvent WithProperty<T>(string property, T value) where T : struct
		{
			return _WithProperty(property, value);
		}

		public AmplitudeAnalyticsEvent WithProperty(string property, string value)
		{
			return _WithProperty(property, TruncateString(value));
		}

		private AmplitudeAnalyticsEvent _WithProperty(string property, object value)
		{
			if (event_properties == null)
			{
				event_properties = new Dictionary<string, object>();
			}
			event_properties[property] = value;
			return this;
		}

		private string TruncateString(string value)
		{
			if (value != null && value.Length > 1024)
			{
				value = value.Substring(0, 1024);
			}
			return value;
		}

		public AmplitudeAnalyticsEvent WithUserProperty(string property, string value)
		{
			return _WithUserProperty(property, TruncateString(value));
		}

		public AmplitudeAnalyticsEvent WithUserProperty<T>(string property, T value) where T : struct
		{
			return _WithUserProperty(property, value);
		}

		private AmplitudeAnalyticsEvent _WithUserProperty(string property, object value)
		{
			if (user_properties == null)
			{
				user_properties = new Dictionary<string, object>();
			}
			user_properties[property] = value;
			return this;
		}

		public Dictionary<string, object> ToJsonDictionary()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["event_type"] = event_type;
			dictionary["event_time"] = time;
			dictionary["time"] = time;
			dictionary["session_id"] = sessionId;
			dictionary["event_id"] = sequenceNumber;
			dictionary["sequence_number"] = sequenceNumber;
			dictionary["app_version"] = buildVersion;
			dictionary["insert_id"] = uuid;
			AddIfNotNull("user_id", user_id, dictionary);
			AddIfNotNull("device_id", device_id, dictionary);
			AddIfNotNull("language", language, dictionary);
			if (deviceInfo != null)
			{
				deviceInfo.AddToEvent(dictionary);
			}
			if (geographicInfo != null)
			{
				geographicInfo.AddToEvent(dictionary);
			}
			if (revenueData != null)
			{
				revenueData.AddToEvent(dictionary);
			}
			AddDictIfNotEmpty("event_properties", event_properties, dictionary);
			AddDictIfNotEmpty("user_properties", user_properties, dictionary);
			AddDictIfNotEmpty("groups", groups, dictionary);
			return dictionary;
		}

		private void AddDictIfNotEmpty(string key, Dictionary<string, object> innerDict, Dictionary<string, object> outerDict)
		{
			if (innerDict != null && innerDict.Count > 0 && innerDict.Count < 1000)
			{
				outerDict[key] = innerDict;
			}
		}

		private void AddIfNotNull(string key, string value, Dictionary<string, object> dict)
		{
			if (value != null)
			{
				dict[key] = value;
			}
		}
	}
}
