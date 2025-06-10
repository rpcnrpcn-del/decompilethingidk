using System.Text;
using UnityEngine;
using UnityEngine.VR;

public class GoogleAnalyticsClient
{
	public enum CustomMetric
	{
		playerCount = 1,
		partyCount = 2
	}

	public enum CustomDimension
	{
		roomName = 1,
		vrDevice = 2
	}

	private const string PROTOCOL_VERSION = "v=1";

	private const string HTTP_URL = "http://www.google-analytics.com/collect";

	private const string HTTPS_URL = "https://ssl.google-analytics.com/collect";

	public string TrackingID;

	public string ClientID;

	public string AppName;

	public string AppVersion;

	private string DefaultHitData;

	private StringBuilder builder = new StringBuilder(256, 8192);

	private GoogleAnalyticsHitType currentHitType;

	private string DataSendUrl;

	public string AnalyticsHost
	{
		get
		{
			return DataSendUrl;
		}
	}

	public GoogleAnalyticsClient(string anonymousClientID)
	{
		ClientID = EscapeString(anonymousClientID, true);
		AppName = EscapeString(GoogleAnalyticsSettings.Instance.AppName, true);
		AppVersion = EscapeString(BuildSettings.Version, true);
		if (GoogleAnalyticsSettings.Instance.UseHTTPS)
		{
			DataSendUrl = "https://ssl.google-analytics.com/collect";
		}
		else
		{
			DataSendUrl = "http://www.google-analytics.com/collect";
		}
		GenerateHeaders(GoogleAnalyticsSettings.Instance.GetCurentProfile().TrackingID);
	}

	public void UpdateHeaders()
	{
		GenerateHeaders(GoogleAnalyticsSettings.Instance.GetCurentProfile().TrackingID);
	}

	public void GenerateHeaders(string trackingId)
	{
		TrackingID = EscapeString(trackingId);
		builder.Length = 0;
		builder.Append("v=1");
		builder.Append("&tid=");
		builder.Append(TrackingID);
		builder.Append("&cid=");
		builder.Append(ClientID);
		builder.Append("&an=");
		builder.Append(AppName);
		builder.Append("&av=");
		builder.Append(AppVersion);
		DefaultHitData = builder.ToString();
		builder.Length = 0;
	}

	public void SetAnonymizeIP()
	{
		builder.Append("&aip=0");
	}

	public void SetQueueTime(int time)
	{
		builder.Append("&qt=");
		builder.Append(time);
	}

	public void StartSession()
	{
		builder.Append("&sc=start");
		if (VRDevice.isPresent)
		{
			string value = VRSettings.loadedDeviceName + "_" + VRDevice.model;
			builder.Append(GetCustomDimensionString(CustomDimension.vrDevice, value));
		}
	}

	public void EndSession()
	{
		builder.Append("&sc=end");
	}

	public void IPOverride(string ip)
	{
		AppendData("uip", ip);
	}

	public void UserAgentOverride(string userAgent)
	{
		AppendData("ua", userAgent);
	}

	public void SetDocumentReferrer(string url)
	{
		AppendData("dr", url, "Document Referrer", 2048);
	}

	public void SetCampaignName(string name)
	{
		AppendData("cn", name, "Campaign Name", 100);
	}

	public void SetCampaignSource(string source)
	{
		AppendData("cs", source, "Campaign Source", 100);
	}

	public void SetCampaignMedium(string medium)
	{
		AppendData("cm", medium, "Campaign Medium", 50);
	}

	public void AddCampaignKeyword(string keyword)
	{
		AppendData("ck", keyword, "Campaign Keyword", 500);
	}

	public void SetCampaignContent(string content)
	{
		AppendData("cc", content, "Campaign Content", 500);
	}

	public void SetCampaignID(string id)
	{
		AppendData("ci", id, "Campaign ID", 500);
	}

	public void SetGoogleAdWordsID(string id)
	{
		AppendData("gclid", id);
	}

	public void SetGoogleDisplayAdsID(string id)
	{
		AppendData("dclid", id);
	}

	public void SetScreenResolution(int width, int height)
	{
		builder.Append("&sr=");
		builder.Append(width);
		builder.Append('x');
		builder.Append(height);
	}

	public void SetViewportSize(int width, int height)
	{
		builder.Append("&vp=");
		builder.Append(width);
		builder.Append('x');
		builder.Append(height);
	}

	public void SetDocumentEncoding(string encoding)
	{
		AppendData("de", encoding);
	}

	public void SetScreenColors(string colorsBuffer)
	{
		AppendData("sd", colorsBuffer);
	}

	public void SetUserLanguage(string lang)
	{
		AppendData("ul", lang);
	}

	public void SetJavaEnablede(bool isEnabled)
	{
		string val = "0";
		if (isEnabled)
		{
			val = "1";
		}
		AppendData("je", val);
	}

	public void SetFlashVersion(string version)
	{
		AppendData("fl", version);
	}

	public void SetHitType(GoogleAnalyticsHitType hit)
	{
		AppendData("t", hit.ToString().ToLower());
	}

	public void SetNoInteractionHit()
	{
		builder.Append("&ni=1");
	}

	public void SetDocumentlocationURL(string url)
	{
		AppendData("dl", url, "Document location URL", 2048);
	}

	public void SetDocumentHostName(string host)
	{
		AppendData("dh", host, "Document Host Name", 100);
	}

	public void SetDocumentPath(string path)
	{
		AppendData("dp", path, "Document Path", 2048);
	}

	public void SetDocumentTitle(string title)
	{
		AppendData("dt", title, "Document Title", 1500);
	}

	public void SetScreenName(string name)
	{
		AppendData("cd", name, "Screen Name", 2048);
	}

	public void SetLinkID(string id)
	{
		AppendData("linkid", id);
	}

	public void SetApplicationName(string name)
	{
		AppendData("an", name, "Application Name", 100);
	}

	public void SetApplicationVersion(string version)
	{
		AppendData("av", version, "Application Version", 100);
	}

	public void SetApplicationInstallerID(string identifier)
	{
		AppendData("aiid", identifier, "Application Installer ID", 150);
	}

	public void SetEventCategory(string ec)
	{
		AppendData("ec", ec, "Event Category", 150, GoogleAnalyticsHitType.EVENT);
	}

	public void SetEventAction(string ea)
	{
		AppendData("ea", ea, "Event Action", 500, GoogleAnalyticsHitType.EVENT);
	}

	public void SetEventLabel(string el)
	{
		AppendData("el", el, "Event Label", 500, GoogleAnalyticsHitType.EVENT);
	}

	public void SetEventValue(int val)
	{
		AppendData("ev", val.ToString(), string.Empty, 0, GoogleAnalyticsHitType.EVENT);
	}

	public void SetTransactionID(string ti)
	{
		AppendData("ti", ti, "Transaction ID", 500, GoogleAnalyticsHitType.TRANSACTION, GoogleAnalyticsHitType.ITEM);
	}

	public void SetTransactionAffiliation(string ta)
	{
		AppendData("ta", ta, "Transaction Affiliation", 500, GoogleAnalyticsHitType.TRANSACTION);
	}

	public void SetTransactionRevenue(float tr)
	{
		AppendData("tr", FloatToCurrency(tr), string.Empty, 0, GoogleAnalyticsHitType.TRANSACTION);
	}

	public void SetTransactionShipping(float ts)
	{
		AppendData("ts", FloatToCurrency(ts), string.Empty, 0, GoogleAnalyticsHitType.TRANSACTION);
	}

	public void SetTransactionTax(float tt)
	{
		AppendData("tt", FloatToCurrency(tt), string.Empty, 0, GoogleAnalyticsHitType.TRANSACTION);
	}

	public void SetTransactionCouponCode(string tcc)
	{
		AppendData("tcc", tcc);
	}

	public void SetItemName(string name)
	{
		AppendData("in", name, "Item Name", 500, GoogleAnalyticsHitType.ITEM);
	}

	public void SetItemPrice(float ip)
	{
		AppendData("ip", FloatToCurrency(ip), string.Empty, 0, GoogleAnalyticsHitType.ITEM);
	}

	public void SetItemQuantity(int iq)
	{
		AppendData("iq", iq.ToString(), string.Empty, 0, GoogleAnalyticsHitType.ITEM);
	}

	public void SetItemCode(string ic)
	{
		AppendData("ic", ic, "Item Code", 500, GoogleAnalyticsHitType.ITEM);
	}

	public void SetItemCategory(string iv)
	{
		AppendData("iv", iv, "Item Category", 500, GoogleAnalyticsHitType.ITEM);
	}

	public void SetCurrencyCode(string cu)
	{
		AppendData("cu", cu, "Currency Code", 10, GoogleAnalyticsHitType.ITEM, GoogleAnalyticsHitType.TRANSACTION);
	}

	public void SetProductSKU(int productIndex, string sku)
	{
		AppendData("pr" + productIndex + "id", sku, "Product SKU", 500);
	}

	public void SetSetProductName(int productIndex, string name)
	{
		AppendData("pr" + productIndex + "nm", name, "Product Name", 500);
	}

	public void SetProductBrand(int productIndex, string brand)
	{
		AppendData("pr" + productIndex + "br", brand, "Product Brand ", 500);
	}

	public void SetProductCategory(int productIndex, string category)
	{
		AppendData("pr" + productIndex + "ca", category, "Product Category ", 500);
	}

	public void SetProductVariant(int productIndex, string variant)
	{
		AppendData("pr" + productIndex + "va", variant, "Product Variant ", 500);
	}

	public void SetProductPrice(int productIndex, float prise)
	{
		AppendData("pr" + productIndex + "pr", FloatToCurrency(prise));
	}

	public void SetProductQuantity(int productIndex, int quantit)
	{
		AppendData("pr" + productIndex + "qt", quantit.ToString());
	}

	public void SetProductCouponCode(int productIndex, string couponCode)
	{
		AppendData("pr" + productIndex + "cc", couponCode, "Product Coupon Code", 500);
	}

	public void SetProductPosition(int productIndex, int pos)
	{
		AppendData("pr" + productIndex + "ps", pos.ToString());
	}

	public void SetProductCustomDimension(int productIndex, int index, string val)
	{
		AppendData("pr" + productIndex + "cd" + index, val);
	}

	public void SetProductCustomMetric(int productIndex, int index, int metric)
	{
		AppendData("pr" + productIndex + "cm" + index, metric.ToString());
	}

	public void SetProductAction(string pa)
	{
		AppendData("pa", pa);
	}

	public void SetProductActionList(string val)
	{
		AppendData("pal", val);
	}

	public void SetCheckoutStep(int cos)
	{
		AppendData("cos", cos.ToString());
	}

	public void SetCheckoutStepOption(string col)
	{
		AppendData("col", col);
	}

	public void SetSocialNetwork(string sn)
	{
		AppendData("sn", sn, "Social Network", 50, GoogleAnalyticsHitType.SOCIAL);
	}

	public void SetSocialAction(string action)
	{
		AppendData("sa", action, "Social Action", 50, GoogleAnalyticsHitType.SOCIAL);
	}

	public void SetSocialActionTarget(string target)
	{
		AppendData("st", target, "Social Action Target", 2048, GoogleAnalyticsHitType.SOCIAL);
	}

	public void SetUserTimingCategory(string category)
	{
		AppendData("utc", category, "User Timing Category", 150, GoogleAnalyticsHitType.TIMING);
	}

	public void SetUserTimingVariableName(string name)
	{
		AppendData("utv", name, "User Timing Variable Name", 500, GoogleAnalyticsHitType.TIMING);
	}

	public void SetUserTimingTime(int time)
	{
		AppendData("utt", time.ToString(), "User Timing Time", 0, GoogleAnalyticsHitType.TIMING);
	}

	public void SetUserTimingLabel(string label)
	{
		AppendData("utl", label, "User Timing Label", 500, GoogleAnalyticsHitType.TIMING);
	}

	public void SetPageLoadTime(int time)
	{
		AppendData("plt", time.ToString(), "Page Load Time", 0, GoogleAnalyticsHitType.TIMING);
	}

	public void SetDNSTime(int time)
	{
		AppendData("dns", time.ToString(), "DNS Time", 0, GoogleAnalyticsHitType.TIMING);
	}

	public void SetPageDownloadTime(int time)
	{
		AppendData("pdt", time.ToString(), string.Empty, 0, GoogleAnalyticsHitType.TIMING);
	}

	public void SetRedirectResponseTime(int time)
	{
		AppendData("rrt", time.ToString(), string.Empty, 0, GoogleAnalyticsHitType.TIMING);
	}

	public void SetTCPConnectTime(int time)
	{
		AppendData("tcp", time.ToString(), string.Empty, 0, GoogleAnalyticsHitType.TIMING);
	}

	public void SetServerResponseTime(int time)
	{
		AppendData("srt", time.ToString(), string.Empty, 0, GoogleAnalyticsHitType.TIMING);
	}

	public void SetPromotionID(int index, string id)
	{
		AppendData("promo" + index + "id", id);
	}

	public void SetPromotionName(int index, string nm)
	{
		AppendData("promo" + index + "nm", nm);
	}

	public void SetPromotionCreative(int index, string cr)
	{
		AppendData("promo" + index + "cr", cr);
	}

	public void SetPromotionPosition(int index, string ps)
	{
		AppendData("promo" + index + "ps", ps);
	}

	public void SetPromotionAction(string promoa)
	{
		AppendData("promoa", promoa);
	}

	public void SetExceptionDescription(string description)
	{
		AppendData("exd", description, "Exception Description", 150, GoogleAnalyticsHitType.EXCEPTION);
	}

	public void SetIsFatalException(bool isFatal)
	{
		string val = "0";
		if (isFatal)
		{
			val = "1";
		}
		AppendData("exf", val, string.Empty, 0, GoogleAnalyticsHitType.EXCEPTION);
	}

	public void SetNonInteractionFlag()
	{
		AppendData("ni", "1");
	}

	public string GetCustomDimensionString(CustomDimension dimension, string value)
	{
		int num = (int)dimension;
		return string.Format("&cd{0}={1}", num.ToString(), value);
	}

	public void SetCustomDimension(CustomDimension dimension, string value)
	{
		int num = (int)dimension;
		AppendData("cd" + num, value, "Custom Dimension", 150);
	}

	public void SetCustomMetric(CustomMetric metric, int value)
	{
		int num = (int)metric;
		AppendData("cm" + num, value.ToString());
	}

	public void SetExperimentID(string id)
	{
		AppendData("xid", id, "Experiment ID", 40);
	}

	public void SetExperimentVariant(string variant)
	{
		AppendData("xvar", variant);
	}

	public void SendPageHit(string host, string page, string title, string description = "", string linkId = "")
	{
		CreateHit(GoogleAnalyticsHitType.PAGEVIEW);
		SetDocumentHostName(host);
		SetDocumentPath(page);
		SetDocumentTitle(title);
		if (!description.Equals(string.Empty))
		{
			SetScreenName(description);
		}
		if (!linkId.Equals(string.Empty))
		{
			SetLinkID(linkId);
		}
		Send();
	}

	public void SendEventHit(string category, string action, string label = "", int val = -1, bool trackLevelName = false)
	{
		CreateHit(GoogleAnalyticsHitType.EVENT);
		SetEventCategory(category);
		SetEventAction(action);
		if (trackLevelName)
		{
			SetScreenName(GoogleAnalyticsSettings.Instance.LevelPrefix + GoogleAnalytics.LoadedLevelName + GoogleAnalyticsSettings.Instance.LevelPostfix);
		}
		if (!label.Equals(string.Empty))
		{
			SetEventLabel(label);
		}
		if (val != -1)
		{
			SetEventValue(val);
		}
		if (PhotonNetwork.inRoom && !PhotonNetwork.offlineMode)
		{
			SetCustomMetric(CustomMetric.playerCount, PhotonNetwork.playerList.Length);
			SetCustomMetric(CustomMetric.partyCount, (Player.LocalPlayer != null) ? Player.LocalPlayer.PlayerParty.PartySize : 0);
			SetCustomDimension(CustomDimension.roomName, PhotonNetwork.room.name);
		}
		Send();
	}

	public void SendTransactionHit(string id, string affiliation = "", string currencyCode = "", float revenue = 0f, float shipping = 0f, float tax = 0f)
	{
		CreateHit(GoogleAnalyticsHitType.TRANSACTION);
		SetTransactionID(id);
		if (!string.IsNullOrEmpty(affiliation))
		{
			SetTransactionAffiliation(affiliation);
		}
		if (!string.IsNullOrEmpty(currencyCode))
		{
			SetCurrencyCode(currencyCode);
		}
		if (revenue != 0f)
		{
			SetTransactionRevenue(revenue);
		}
		if (shipping != 0f)
		{
			SetTransactionShipping(shipping);
		}
		if (tax != 0f)
		{
			SetTransactionTax(tax);
		}
		Send();
	}

	public void SendItemHit(string transactionId, string name, string SKU, float price, int quantity = 1, string category = "", string currencyCode = "")
	{
		CreateHit(GoogleAnalyticsHitType.ITEM);
		SetTransactionID(transactionId);
		SetItemName(name);
		SetItemCode(SKU);
		SetItemPrice(price);
		SetItemQuantity(quantity);
		if (!string.IsNullOrEmpty(category))
		{
			SetItemCategory(category);
		}
		if (!string.IsNullOrEmpty(currencyCode))
		{
			SetCurrencyCode(currencyCode);
		}
		Send();
	}

	public void SendSocialHit(string action, string network, string target)
	{
		CreateHit(GoogleAnalyticsHitType.SOCIAL);
		SetSocialAction(action);
		SetSocialNetwork(network);
		SetSocialActionTarget(target);
		Send();
	}

	public void SendExceptionHit(string description, bool IsFatal = false)
	{
		CreateHit(GoogleAnalyticsHitType.EXCEPTION);
		SetExceptionDescription(description);
		if (IsFatal)
		{
			SetIsFatalException(IsFatal);
		}
		Send();
	}

	public void SendUserTimingHit(string category, string variable, int time, string label)
	{
		CreateHit(GoogleAnalyticsHitType.TIMING);
		SetUserTimingCategory(category);
		SetUserTimingVariableName(variable);
		SetUserTimingTime(time);
		SetUserTimingLabel(label);
		Send();
	}

	public void SendScreenHit(string screenName)
	{
		if (!screenName.Equals("empty"))
		{
			CreateHit(GoogleAnalyticsHitType.APPVIEW);
			SetScreenName(screenName);
			Send();
		}
	}

	public void CreateHit(GoogleAnalyticsHitType hit)
	{
		currentHitType = hit;
		builder.Length = 0;
		builder.Append(DefaultHitData);
		SetHitType(hit);
	}

	public void Send()
	{
		if (!GoogleAnalyticsSettings.Instance.IsDisabled)
		{
			builder.Append("&z=");
			builder.Append(Random.Range(0, int.MaxValue) ^ 0xD60);
			string request = builder.ToString();
			builder.Length = 0;
			GoogleAnalytics.Send(request);
		}
	}

	public WWW GenerateWWW(byte[] data)
	{
		return new WWW(DataSendUrl, data);
	}

	public void AppendData(string protocolToken, string val)
	{
		AppendData(protocolToken, val, string.Empty, 0);
	}

	public void AppendData(string protocolToken, string val, string action, int maxSize, params GoogleAnalyticsHitType[] supportedHitTypes)
	{
		if (supportedHitTypes.Length > 0)
		{
			bool flag = false;
			foreach (GoogleAnalyticsHitType googleAnalyticsHitType in supportedHitTypes)
			{
				if (googleAnalyticsHitType == currentHitType)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				Debug.LogWarning("Google Analytics: " + action + " not supported for hit type  " + currentHitType);
				return;
			}
		}
		string text = EscapeString(val);
		builder.Append("&");
		builder.Append(protocolToken);
		builder.Append("=");
		builder.Append(text);
		if (maxSize > 0)
		{
			CheckDataLength(action, text, maxSize);
		}
	}

	private string FloatToCurrency(float val)
	{
		return val.ToString("n2");
	}

	private void CheckDataLength(string action, string data, int maxLength)
	{
		if (string.IsNullOrEmpty(data))
		{
			Debug.LogWarning("Google Analytics: " + action + " data is null");
		}
		else if (data.Length > maxLength)
		{
			Debug.LogWarning("Google Analytics: " + action + " is too long, max size " + maxLength + " bytes (" + data + ")");
		}
	}

	private string EscapeString(string original)
	{
		return EscapeString(original, false);
	}

	public string EscapeString(string original, bool forced)
	{
		if (forced)
		{
			return WWW.EscapeURL(original);
		}
		if (GoogleAnalyticsSettings.Instance.StringEscaping)
		{
			return WWW.EscapeURL(original);
		}
		return original;
	}
}
