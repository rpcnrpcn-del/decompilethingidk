using UnityEngine;

public class GoogleAnalyticsUseExample : MonoBehaviour
{
	private void Start()
	{
		GoogleAnalytics.StartTracking();
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 150f, 50f), "Page Hit"))
		{
			GoogleAnalytics.Client.SendPageHit("mydemo.com ", "/home", "homepage", string.Empty, string.Empty);
		}
		if (GUI.Button(new Rect(10f, 70f, 150f, 50f), "Event Hit"))
		{
			GoogleAnalytics.Client.SendEventHit("video", "play", "holiday", 300);
		}
		if (GUI.Button(new Rect(10f, 130f, 150f, 50f), "Transaction Hit"))
		{
			GoogleAnalytics.Client.SendTransactionHit("12345", "westernWear", "EUR", 50f, 32f, 12f);
		}
		if (GUI.Button(new Rect(10f, 190f, 150f, 50f), "Item Hit"))
		{
			GoogleAnalytics.Client.SendItemHit("12345", "sofa", "u3eqds43", 300f, 2, "furniture", "EUR");
		}
		if (GUI.Button(new Rect(190f, 10f, 150f, 50f), "Social Hit"))
		{
			GoogleAnalytics.Client.SendSocialHit("like", "facebook", "/home ");
		}
		if (GUI.Button(new Rect(190f, 70f, 150f, 50f), "Exception Hit"))
		{
			GoogleAnalytics.Client.SendExceptionHit("IOException", true);
		}
		if (GUI.Button(new Rect(190f, 130f, 150f, 50f), "Timing Hit"))
		{
			GoogleAnalytics.Client.SendUserTimingHit("jsonLoader", "load", 5000, "jQuery");
		}
		if (GUI.Button(new Rect(190f, 190f, 150f, 50f), "Screen Hit"))
		{
			GoogleAnalytics.Client.SendScreenHit("MainMenu");
		}
	}

	public void CustomBuildersExamples()
	{
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.PAGEVIEW);
		GoogleAnalytics.Client.SetDocumentHostName("mydemo.com");
		GoogleAnalytics.Client.SetDocumentPath("/home");
		GoogleAnalytics.Client.SetDocumentTitle("homepage");
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.EVENT);
		GoogleAnalytics.Client.SetEventCategory("video");
		GoogleAnalytics.Client.SetEventAction("play");
		GoogleAnalytics.Client.SetEventLabel("holiday");
		GoogleAnalytics.Client.SetEventValue(300);
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.PAGEVIEW);
		GoogleAnalytics.Client.SetDocumentHostName("mydemo.com");
		GoogleAnalytics.Client.SetDocumentPath("/receipt");
		GoogleAnalytics.Client.SetDocumentTitle("Receipt Page");
		GoogleAnalytics.Client.SetTransactionID("T12345");
		GoogleAnalytics.Client.SetTransactionAffiliation("Google Store - Online");
		GoogleAnalytics.Client.SetTransactionRevenue(37.39f);
		GoogleAnalytics.Client.SetTransactionTax(2.85f);
		GoogleAnalytics.Client.SetTransactionShipping(5.34f);
		GoogleAnalytics.Client.SetTransactionCouponCode("SUMMER2013");
		GoogleAnalytics.Client.SetProductAction("purchase");
		GoogleAnalytics.Client.SetProductSKU(1, "P12345");
		GoogleAnalytics.Client.SetSetProductName(1, "Android Warhol T-Shirt");
		GoogleAnalytics.Client.SetProductCategory(1, "Apparel");
		GoogleAnalytics.Client.SetProductBrand(1, "Google");
		GoogleAnalytics.Client.SetProductVariant(1, "Black");
		GoogleAnalytics.Client.SetProductPosition(1, 1);
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.EVENT);
		GoogleAnalytics.Client.SetEventCategory("Ecommerce");
		GoogleAnalytics.Client.SetEventAction("Refund");
		GoogleAnalytics.Client.SetNonInteractionFlag();
		GoogleAnalytics.Client.SetTransactionID("T12345");
		GoogleAnalytics.Client.SetProductAction("refund");
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.EVENT);
		GoogleAnalytics.Client.SetEventCategory("Ecommerce");
		GoogleAnalytics.Client.SetEventAction("Refund");
		GoogleAnalytics.Client.SetNonInteractionFlag();
		GoogleAnalytics.Client.SetTransactionID("T12345");
		GoogleAnalytics.Client.SetProductAction("refund");
		GoogleAnalytics.Client.SetProductSKU(1, "P12345");
		GoogleAnalytics.Client.SetProductQuantity(1, 1);
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.PAGEVIEW);
		GoogleAnalytics.Client.SetDocumentHostName("mydemo.com");
		GoogleAnalytics.Client.SetDocumentPath("/receipt");
		GoogleAnalytics.Client.SetDocumentTitle("Receipt Page");
		GoogleAnalytics.Client.SetTransactionID("T12345");
		GoogleAnalytics.Client.SetTransactionAffiliation("Google Store - Online");
		GoogleAnalytics.Client.SetTransactionRevenue(37.39f);
		GoogleAnalytics.Client.SetTransactionTax(2.85f);
		GoogleAnalytics.Client.SetTransactionShipping(5.34f);
		GoogleAnalytics.Client.SetTransactionCouponCode("SUMMER2013");
		GoogleAnalytics.Client.SetProductAction("purchase");
		GoogleAnalytics.Client.SetProductSKU(1, "P12345");
		GoogleAnalytics.Client.SetSetProductName(1, "Android Warhol T-Shirt");
		GoogleAnalytics.Client.SetProductCategory(1, "Apparel");
		GoogleAnalytics.Client.SetProductBrand(1, "Google");
		GoogleAnalytics.Client.SetProductVariant(1, "Black");
		GoogleAnalytics.Client.SetProductPrice(1, 29.9f);
		GoogleAnalytics.Client.SetProductQuantity(1, 1);
		GoogleAnalytics.Client.SetCheckoutStep(1);
		GoogleAnalytics.Client.SetCheckoutStepOption("Visa");
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.EVENT);
		GoogleAnalytics.Client.SetEventCategory("Checkout");
		GoogleAnalytics.Client.SetEventAction("Option");
		GoogleAnalytics.Client.SetProductAction("checkout_option");
		GoogleAnalytics.Client.SetCheckoutStep(2);
		GoogleAnalytics.Client.SetCheckoutStepOption("FedEx");
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.PAGEVIEW);
		GoogleAnalytics.Client.SetDocumentHostName("mydemo.com");
		GoogleAnalytics.Client.SetDocumentPath("/home");
		GoogleAnalytics.Client.SetDocumentTitle("homepage");
		GoogleAnalytics.Client.SetPromotionID(1, "PROMO_1234");
		GoogleAnalytics.Client.SetPromotionName(1, "Summer Sale");
		GoogleAnalytics.Client.SetPromotionCreative(1, "summer_banner2");
		GoogleAnalytics.Client.SetPromotionPosition(1, "banner_slot1");
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.EVENT);
		GoogleAnalytics.Client.SetEventCategory("Internal Promotions");
		GoogleAnalytics.Client.SetEventAction("click");
		GoogleAnalytics.Client.SetEventLabel("Summer Sale");
		GoogleAnalytics.Client.SetPromotionAction("click");
		GoogleAnalytics.Client.SetPromotionID(1, "PROMO_1234");
		GoogleAnalytics.Client.SetPromotionName(1, "Summer Sale");
		GoogleAnalytics.Client.SetPromotionCreative(1, "summer_banner2");
		GoogleAnalytics.Client.SetPromotionPosition(1, "banner_slot1");
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.TRANSACTION);
		GoogleAnalytics.Client.SetTransactionID("12345");
		GoogleAnalytics.Client.SetTransactionAffiliation("westernWear");
		GoogleAnalytics.Client.SetTransactionRevenue(50f);
		GoogleAnalytics.Client.SetTransactionShipping(32f);
		GoogleAnalytics.Client.SetTransactionTax(12f);
		GoogleAnalytics.Client.SetCurrencyCode("EUR");
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.ITEM);
		GoogleAnalytics.Client.SetTransactionID("12345");
		GoogleAnalytics.Client.SetItemName("sofa");
		GoogleAnalytics.Client.SetItemPrice(300f);
		GoogleAnalytics.Client.SetItemQuantity(2);
		GoogleAnalytics.Client.SetItemCode("u3eqds43");
		GoogleAnalytics.Client.SetItemCategory("furniture");
		GoogleAnalytics.Client.SetCurrencyCode("EUR");
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.SOCIAL);
		GoogleAnalytics.Client.SetSocialAction("like");
		GoogleAnalytics.Client.SetSocialNetwork("facebook");
		GoogleAnalytics.Client.SetSocialActionTarget("/home  ");
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.EXCEPTION);
		GoogleAnalytics.Client.SetExceptionDescription("IOException");
		GoogleAnalytics.Client.SetIsFatalException(true);
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.TIMING);
		GoogleAnalytics.Client.SetUserTimingCategory("jsonLoader");
		GoogleAnalytics.Client.SetUserTimingVariableName("load");
		GoogleAnalytics.Client.SetUserTimingTime(5000);
		GoogleAnalytics.Client.SetUserTimingLabel("jQuery");
		GoogleAnalytics.Client.SetDNSTime(100);
		GoogleAnalytics.Client.SetPageDownloadTime(20);
		GoogleAnalytics.Client.SetRedirectResponseTime(32);
		GoogleAnalytics.Client.SetTCPConnectTime(56);
		GoogleAnalytics.Client.SetServerResponseTime(12);
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.PAGEVIEW);
		GoogleAnalytics.Client.SetDocumentHostName("mydemo.com");
		GoogleAnalytics.Client.SetDocumentPath("/home");
		GoogleAnalytics.Client.SetDocumentTitle("homepage");
		GoogleAnalytics.Client.Send();
		GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.PAGEVIEW);
		GoogleAnalytics.Client.AppendData("dh", "mydemo.com");
		GoogleAnalytics.Client.AppendData("dp", "/home");
		GoogleAnalytics.Client.AppendData("dt", "homepage");
		GoogleAnalytics.Client.Send();
	}
}
