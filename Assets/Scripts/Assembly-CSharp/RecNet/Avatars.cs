using System;
using System.Collections;
using System.Collections.Generic;
using GAMiniJSON;
using UnityEngine;

namespace RecNet
{
	public class Avatars
	{
		public class GiftPackage : IRecNetObject
		{
			public long Id { get; private set; }

			public string AvatarItemDesc { get; private set; }

			public int Xp { get; private set; }

			public bool HasAvatarItem
			{
				get
				{
					return !string.IsNullOrEmpty(AvatarItemDesc);
				}
			}

			public bool Consumed { get; set; }

			public void Deserialize(Dictionary<string, object> dict)
			{
				Id = Util.GetKey<long>("Id", dict);
				AvatarItemDesc = Util.GetKey<string>("AvatarItemDesc", dict);
				Xp = Util.GetKey<int>("Xp", dict);
				Consumed = false;
			}

			public Dictionary<string, object> Serialize()
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("Id", Id);
				dictionary.Add("AvatarItemDesc", AvatarItemDesc);
				dictionary.Add("Xp", Xp);
				return dictionary;
			}
		}

		private class UnlockedAvatarItem : IRecNetObject
		{
			public string AvatarItemDesc { get; set; }

			public int UnlockedLevel { get; set; }

			public void Deserialize(Dictionary<string, object> dict)
			{
				AvatarItemDesc = Util.GetKey<string>("AvatarItemDesc", dict);
				UnlockedLevel = Util.GetKey<int>("UnlockedLevel", dict);
			}

			public Dictionary<string, object> Serialize()
			{
				throw new NotImplementedException();
			}
		}

		private const string AVATAR_API = "api/avatar/";

		private static bool localAvatarSaveInProgress;

		private static bool localAvatarSaveRequested;

		public static List<GiftPackage> GiftPackages { get; private set; }

		public static Avatar LocalAvatar { get; private set; }

		public static IEnumerator DowloadGiftPackages(Core.ApiCallback callback)
		{
			string requestUri = string.Format("{0}v2/gifts", "api/avatar/");
			return Core.Get(requestUri, delegate(string error, List<GiftPackage> newGifts)
			{
				if (string.IsNullOrEmpty(error))
				{
					GiftPackages = newGifts;
					Core.SafeInvoke(callback, null);
				}
				else
				{
					Debug.LogError("Failed to download gifts: " + error);
					Core.SafeInvoke(callback, "Failed to download gifts");
				}
			});
		}

		public static IEnumerator LocalCreateGiftPackage(string avatarItemDesc, int xp, Core.ApiCallback<GiftPackage> callback)
		{
			string requestUri = string.Format("{0}v2/gifts/create", "api/avatar/");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("AvatarItemDesc", avatarItemDesc);
			dictionary.Add("Xp", xp.ToString());
			return Core.Post(requestUri, dictionary, delegate(string error, GiftPackage newGift)
			{
				if (string.IsNullOrEmpty(error))
				{
					if (!GiftPackages.Contains(newGift))
					{
						GiftPackages.Add(newGift);
						Core.SafeInvoke(callback, null, newGift);
					}
					else
					{
						Core.SafeInvoke(callback, "Created duplicate gift!", null);
					}
				}
				else
				{
					Debug.LogError("Failed to create new gift: " + error);
					Core.SafeInvoke(callback, "Failed to create new gift.", null);
				}
			});
		}

		public static IEnumerator LocalConsumeGiftPackage(GiftPackage gift, int unlockedLevel, Core.ApiCallback callback)
		{
			string requestUri = string.Format("{0}v2/gifts/consume/", "api/avatar/");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Id", gift.Id.ToString());
			dictionary.Add("UnlockedLevel", unlockedLevel.ToString());
			return Core.Post(requestUri, dictionary, delegate(string error)
			{
				if (string.IsNullOrEmpty(error))
				{
					gift.Consumed = true;
					GiftPackages.RemoveAt(0);
					Core.SafeInvoke(callback, null);
				}
				else
				{
					Core.SafeInvoke(callback, error);
				}
			});
		}

		public static IEnumerator DowloadUnlockedAvatarItems(Core.ApiCallback<List<OutfitSelection>> callback)
		{
			string requestUri = string.Format("{0}v3/items", "api/avatar/");
			yield return Core.Get(requestUri, delegate(string error, List<UnlockedAvatarItem> items)
			{
				if (string.IsNullOrEmpty(error))
				{
					List<OutfitSelection> list = new List<OutfitSelection>(items.Count);
					foreach (UnlockedAvatarItem item in items)
					{
						OutfitSelection outfitSelection = OutfitSelection.Parse(item.AvatarItemDesc);
						if (outfitSelection != null)
						{
							outfitSelection.UnlockedLevel = item.UnlockedLevel;
							list.Add(outfitSelection);
						}
					}
					Core.SafeInvoke(callback, null, list);
				}
				else
				{
					Debug.LogError("Failed to download unlocked avatar items: " + error);
					Core.SafeInvoke(callback, "Failed to download unlocked avatar items", null);
				}
			});
		}

		public static IEnumerator DownloadLocalAvatar(Core.ApiCallback callback)
		{
			string requestUri = string.Format("{0}v2", "api/avatar/");
			return Core.Get(requestUri, delegate(string error, Avatar avatar)
			{
				if (string.IsNullOrEmpty(error))
				{
					LocalAvatar = avatar;
					Core.SafeInvoke(callback, null);
				}
				else
				{
					Debug.LogError("Failed to load Rec Room player avatar settings: " + error);
					Core.SafeInvoke(callback, "Failed to load Rec Room player avatar settings");
				}
			});
		}

		public static void SaveLocalAvatarSettings()
		{
			if (localAvatarSaveInProgress)
			{
				localAvatarSaveRequested = true;
				return;
			}
			localAvatarSaveRequested = false;
			string requestUri = string.Format("{0}v2/set", "api/avatar/");
			Dictionary<string, object> obj = LocalAvatar.Serialize();
			string json = Json.Serialize(obj);
			Core.Post(requestUri, json, delegate(string wwwError)
			{
				localAvatarSaveInProgress = false;
				if (!string.IsNullOrEmpty(wwwError))
				{
					Debug.LogError("Failed to upload Rec Room player avatar changes: " + wwwError);
					localAvatarSaveRequested = true;
				}
				if (localAvatarSaveRequested)
				{
					SaveLocalAvatarSettings();
				}
			});
		}
	}
}
