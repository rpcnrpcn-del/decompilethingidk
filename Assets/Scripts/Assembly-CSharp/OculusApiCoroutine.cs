using Oculus.Platform;
using UnityEngine;

public class OculusApiCoroutine : CustomYieldInstruction
{
	public Message Result { get; set; }

	public override bool keepWaiting
	{
		get
		{
			return Result == null;
		}
	}

	public OculusApiCoroutine(Request request)
	{
		request.OnComplete(Callback);
	}

	private void Callback(Message message)
	{
		Result = message;
	}
}
public class OculusApiCoroutine<T> : CustomYieldInstruction
{
	public Message<T> Result { get; set; }

	public override bool keepWaiting
	{
		get
		{
			return Result == null;
		}
	}

	public OculusApiCoroutine(Request<T> request)
	{
		request.OnComplete(Callback);
	}

	private void Callback(Message<T> message)
	{
		Result = message;
	}
}
