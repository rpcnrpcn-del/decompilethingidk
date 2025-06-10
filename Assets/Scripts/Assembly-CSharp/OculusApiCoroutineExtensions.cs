using Oculus.Platform;

public static class OculusApiCoroutineExtensions
{
	public static OculusApiCoroutine AsCoroutine(this Request request)
	{
		return new OculusApiCoroutine(request);
	}

	public static OculusApiCoroutine<T> AsCoroutine<T>(this Request<T> request)
	{
		return new OculusApiCoroutine<T>(request);
	}
}
