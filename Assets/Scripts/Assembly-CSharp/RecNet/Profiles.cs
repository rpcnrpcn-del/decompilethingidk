using System;
using System.Collections;
using System.Collections.Generic;
using GAMiniJSON;
using UnityEngine;

namespace RecNet
{
	public class Profiles
	{
		private class RegistrationEmailResponse : IRecNetObject
		{
			public string Message { get; set; }

			public void Deserialize(Dictionary<string, object> dict)
			{
				Message = Util.GetKey<string>("Message", dict);
			}

			public Dictionary<string, object> Serialize()
			{
				throw new NotImplementedException();
			}
		}

		public delegate void ProfileUpdatedCallback(ulong id, Profile profile);

		private class ObjectiveComplete : IRecNetObject
		{
			public int deltaXp { get; set; }

			public int currentLevel { get; set; }

			public int currentXp { get; set; }

			public int xpRequiredToLevelUp { get; set; }

			public void Deserialize(Dictionary<string, object> dict)
			{
				deltaXp = Util.GetKey<int>("deltaXp", dict);
				currentLevel = Util.GetKey<int>("currentLevel", dict);
				currentXp = Util.GetKey<int>("currentXp", dict);
				xpRequiredToLevelUp = Util.GetKey<int>("xpRequiredToLevelUp", dict);
			}

			public Dictionary<string, object> Serialize()
			{
				throw new NotImplementedException();
			}
		}

		private const string PLAYER_API = "api/players/";

		private const string REPUTATION_API = "api/playerReputation/";

		private static Dictionary<ulong, Profile> profileCache = new Dictionary<ulong, Profile>();

		public static Profile _localProfile { get; private set; }

		public static Profile LocalProfile
		{
			get
			{
				return _localProfile;
			}
			private set
			{
				_localProfile = value;
				if (Profiles.LocalProfileDownloaded != null)
				{
					Profiles.LocalProfileDownloaded(LocalProfile);
				}
			}
		}

		public static event Action<Profile> LocalProfileDownloaded;

		public static event ProfileUpdatedCallback OnProfileUpdated;

		public static event Action<int, int> LocalProfileXpUpdated;

		public static event Action<int> LocalProfileLevelUpdated;

		public static IEnumerator DownloadLocalProfile(Core.ApiCallback callback)
		{
			string wwwError = null;
			if (LocalProfile != null)
			{
				yield return Get(LocalProfile.Id, delegate(string e, Profile p)
				{
					wwwError = e;
					if (p != null)
					{
						LocalProfile = p;
					}
				});
			}
			else
			{
				yield return GetOrCreate(PlatformManager.Instance.CurrentPlatform, PlatformManager.Instance.PlatformProfileId, PlatformManager.Instance.PlatformProfileName, delegate(string e, Profile p)
				{
					wwwError = e;
					if (p != null)
					{
						LocalProfile = p;
					}
				});
			}
			if (!string.IsNullOrEmpty(wwwError))
			{
				Debug.LogError("Failed to load Rec Room player profile: " + wwwError);
				Core.SafeInvoke(callback, "Failed to load Rec Room player profile");
			}
			else
			{
				Core.SafeInvoke(callback, null);
			}
		}

		public static void UpdateLocalProfileKarma(float goodKarmaSeconds)
		{
			string requestUri = string.Format("{0}v1/heal", "api/playerReputation/");
			string value = Mathf.CeilToInt(goodKarmaSeconds / 60f).ToString();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("GoodKarmaMinutes", value);
			Core.Post(requestUri, dictionary, delegate(string wwwError)
			{
				if (!string.IsNullOrEmpty(wwwError))
				{
					Debug.LogError("Failed to update Rec Room player good karma: " + wwwError);
				}
			});
		}

		public static void UpdateLocalProfileReputation(int reputationDelta)
		{
			string requestUri = string.Format("{0}v2/updateReputation", "api/players/");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("reputationDelta", reputationDelta.ToString());
			Core.Post(requestUri, dictionary, delegate(string wwwError)
			{
				if (!string.IsNullOrEmpty(wwwError))
				{
					Debug.LogError("Failed to upload Rec Room player profile reputation changes: " + wwwError);
				}
				else
				{
					LocalProfile.Reputation -= reputationDelta;
				}
			});
		}

		public static IEnumerator SendRegistrationEmail(string submittedEmail, Core.ApiCallback<string> callback)
		{
			string requestUri = string.Format("{0}v2/verify", "api/players/");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Email", submittedEmail);
			return Core.Post(requestUri, dictionary, delegate(string error, RegistrationEmailResponse response)
			{
				string response2 = ((response == null) ? null : response.Message);
				Core.SafeInvoke(callback, error, response2);
			});
		}

		public static IEnumerator Get(ulong id, Core.ApiCallback<Profile> callback)
		{
			string requestUri = string.Format("{0}v1/{1}", "api/players/", id);
			return Core.Get(requestUri, callback);
		}

		public static IEnumerator Get(List<ulong> ids, Core.ApiCallback<List<Profile>> callback)
		{
			string requestUri = string.Format("{0}v1/list", "api/players/");
			string json = Json.Serialize(ids);

			/*string workaroundJson = "";
			foreach (ulong id in ids)
			{
				workaroundJson += $"{id},";
			}
			if (workaroundJson.EndsWith(","))
				workaroundJson.Remove(workaroundJson.Length - 1);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["ids"] = workaroundJson;
            string json = Json.Serialize(dictionary);*/

			return Core.Post(requestUri, json, callback);
		}

		private static IEnumerator Get(PlatformManager.PlatformType platform, ulong platformPlayerId, Core.ApiCallback<Profile> callback)
		{
			string requestUri = string.Format("{0}v2/?p={1}&id={2}", "api/players/", (int)platform, platformPlayerId);
			return Core.Get(requestUri, callback);
		}

		private static IEnumerator GetOrCreate(PlatformManager.PlatformType platform, ulong platformPlayerId, string name, Core.ApiCallback<Profile> callback)
		{
			string requestUri = string.Format("{0}v1/getorcreate", "api/players/");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			int num = (int)platform;
			dictionary.Add("Platform", num.ToString());
			dictionary.Add("PlatformId", platformPlayerId.ToString());
			dictionary.Add("Name", name);
			return Core.Post(requestUri, dictionary, callback);
		}

		public static Profile GetProfileFromCache(ulong id)
		{
			Profile value;
			profileCache.TryGetValue(id, out value);
			return value;
		}

		public static void RefreshCachedProfile(ulong id)
		{
			Get(id, delegate(string error, Profile profile)
			{
				if (string.IsNullOrEmpty(error))
				{
					profileCache[id] = profile;
					RaiseProfileUpdatedEvent(id, profile);
				}
			});
		}

		public static void RefreshCachedProfiles(List<ulong> ids)
		{
			Get(ids, delegate(string error, List<Profile> profiles)
			{
				if (string.IsNullOrEmpty(error))
				{
					foreach (Profile profile in profiles)
					{
						profileCache[profile.Id] = profile;
						RaiseProfileUpdatedEvent(profile.Id, profile);
					}
				}
			});
		}

		private static void RaiseProfileUpdatedEvent(ulong id, Profile profile)
		{
			try
			{
				if (Profiles.OnProfileUpdated != null)
				{
					Profiles.OnProfileUpdated(id, profile);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public static void LocalCompleteObjective(ProgressionManager.ObjectiveType type, int additionalXp, bool inParty)
		{
			CompleteObjective(type, additionalXp, inParty, delegate(string wwwError, ObjectiveComplete response)
			{
				if (!string.IsNullOrEmpty(wwwError))
				{
					Debug.LogError(string.Concat("Failed to complete objective ", type, ". Error :", wwwError));
				}
				else
				{
					LocalProfile.XpRequiredToLevelUp = response.xpRequiredToLevelUp;
					LocalProfile.XP = response.currentXp;
					int num = response.currentLevel - LocalProfile.Level;
					LocalProfile.Level = response.currentLevel;
					if (num > 0 && Profiles.LocalProfileLevelUpdated != null)
					{
						Profiles.LocalProfileLevelUpdated(LocalProfile.Level);
					}
					if (Profiles.LocalProfileXpUpdated != null)
					{
						Profiles.LocalProfileXpUpdated(LocalProfile.XP, response.deltaXp);
					}
				}
			});
		}

		private static IEnumerator CompleteObjective(ProgressionManager.ObjectiveType type, int additionalXp, bool inParty, Core.ApiCallback<ObjectiveComplete> callback)
		{
			string requestUri = string.Format("{0}v2/objective", "api/players/");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			int num = (int)type;
			dictionary.Add("objectiveType", num.ToString());
			dictionary.Add("additionalXp", additionalXp.ToString());
			dictionary.Add("inParty", inParty.ToString());
			return Core.Post(requestUri, dictionary, callback);
		}

		public static void LocalAddScore(string activityName, string category, float score, string comment = null, float? secondaryScore = null)
		{
			string requestUri = string.Format("{0}v1/score", "api/players/");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("SessionId", Core.SessionId.ToString());
			dictionary.Add("Activity", activityName);
			dictionary.Add("Category", category);
			dictionary.Add("Score", score.ToString());
			if (!string.IsNullOrEmpty(comment))
			{
				dictionary.Add("Comment", comment);
			}
			if (secondaryScore.HasValue)
			{
				dictionary.Add("SecondaryScore", secondaryScore.Value.ToString());
			}
			Core.Post(requestUri, dictionary, delegate(string wwwError)
			{
				if (!string.IsNullOrEmpty(wwwError))
				{
					Debug.LogError("Failed to add score : " + wwwError);
				}
			});
		}
	}
}
