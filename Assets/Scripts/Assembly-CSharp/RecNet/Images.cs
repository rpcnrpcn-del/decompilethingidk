using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RecNet
{
	public class Images
	{
		private class CachedImage
		{
			public string lastModified;

			public Texture2D texture;
		}

		public delegate void ProfileImageUpdatedCallback(ulong id, Texture2D image);

		private const string IMAGES_API = "api/images/";

		private static Dictionary<ulong, CachedImage> profileImages = new Dictionary<ulong, CachedImage>();

		public static event ProfileImageUpdatedCallback OnProfileImageUpdated;

		public static IEnumerator GetProfileImage(ulong id, Core.ApiCallback<byte[]> callback)
		{
			string requestUri = string.Format("{0}v1/profile/{1}", "api/images/", id);
			return Core.Get(requestUri, delegate(UnityWebRequest www)
			{
				string error = Core.GetError(www);
				Core.SafeInvoke(callback, error, www.downloadHandler.data);
			});
		}

		public static IEnumerator SetLocalProfileImage(byte[] image, Core.ApiCallback callback)
		{
			string requestUri = string.Format("{0}v2/profile", "api/images/");
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddBinaryData("image", image);
			return Core.Post(requestUri, wWWForm, delegate(string error)
			{
				RefreshCachedProfileImage(Profiles.LocalProfile.Id);
				Core.SafeInvoke(callback, error);
			});
		}

		public static Texture2D GetProfileImageFromCache(ulong id)
		{
			CachedImage value;
			return (!profileImages.TryGetValue(id, out value)) ? null : value.texture;
		}

		public static void RefreshCachedProfileImage(ulong id)
		{
			string requestUri = string.Format("{0}v1/profile/{1}", "api/images/", id);
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			CachedImage cachedImage;
			if (profileImages.TryGetValue(id, out cachedImage))
			{
				list.Add(new KeyValuePair<string, string>("If-Modified-Since", cachedImage.lastModified));
			}
			Core.Get(requestUri, list, delegate(UnityWebRequest www)
			{
				if (www.responseCode == 200)
				{
					if (!profileImages.TryGetValue(id, out cachedImage))
					{
						cachedImage = new CachedImage();
						profileImages.Add(id, cachedImage);
					}
					cachedImage.lastModified = www.GetResponseHeader("LAST-MODIFIED");
					try
					{
						if (cachedImage.texture == null)
						{
							cachedImage.texture = new Texture2D(1, 1);
						}
						cachedImage.texture.LoadImage(www.downloadHandler.data);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						UnityEngine.Object.Destroy(cachedImage.texture);
						cachedImage.texture = null;
					}
					RaiseProfileImageUpdatedEvent(id, cachedImage.texture);
				}
			});
		}

		private static void RaiseProfileImageUpdatedEvent(ulong id, Texture2D image)
		{
			try
			{
				if (Images.OnProfileImageUpdated != null)
				{
					Images.OnProfileImageUpdated(id, image);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
