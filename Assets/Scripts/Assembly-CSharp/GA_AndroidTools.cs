public class GA_AndroidTools
{
	private static string ReferalIntentReciever = "com.androidnative.analytics.ReferalIntentReciever";

	public static void RequestReffer()
	{
		CallStatic(ReferalIntentReciever, "RequestReferrer");
	}

	public static void CallStatic(string className, string methodName, params object[] args)
	{
	}
}
