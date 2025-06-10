using UnityEngine;

public abstract class ValueCurve<T>
{
	[SerializeField]
	protected AnimationCurve curve;

	[SerializeField]
	protected T minValue;

	[SerializeField]
	protected T maxValue;

	[SerializeField]
	protected float duration;

	public T Value { get; private set; }

	public float Duration
	{
		get
		{
			return duration;
		}
	}

	public T Evaluate(float t)
	{
		Value = Interpolate(minValue, maxValue, curve.Evaluate(t));
		return Value;
	}

	public T EvaluateNonNormalized(float t)
	{
		return Evaluate(t / Duration);
	}

	protected abstract T Interpolate(T lhs, T rhs, float t);
}
