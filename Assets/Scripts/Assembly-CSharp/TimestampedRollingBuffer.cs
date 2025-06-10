public abstract class TimestampedRollingBuffer<T>
{
	private struct BufferEntry
	{
		public T Value;

		public float Timestamp;
	}

	private static int InvalidIndex = -1;

	private static float InvalidTime = -1f;

	private const int MAX_SIZE = 100;

	private BufferEntry[] array;

	private int head;

	public TimestampedRollingBuffer()
		: this(100)
	{
	}

	public TimestampedRollingBuffer(int size)
	{
		array = new BufferEntry[size];
		Clear();
	}

	public void Add(float time, T value)
	{
		array[head].Timestamp = time;
		array[head].Value = value;
		IncrementHead();
	}

	public void Clear()
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Timestamp = InvalidTime;
		}
		head = 0;
	}

	public bool TryGetAverageValueOverTime(float startTime, float endTime, out T value)
	{
		float num = 0f;
		value = ZeroValue();
		int lhsIndex = InvalidIndex;
		int i;
		for (i = 1; i <= array.Length && array[RollingHeadOffset(i)].Timestamp < startTime; i++)
		{
			lhsIndex = RollingHeadOffset(i);
		}
		int invalidIndex = InvalidIndex;
		for (; i <= array.Length && array[RollingHeadOffset(i)].Timestamp <= endTime; i++)
		{
			invalidIndex = RollingHeadOffset(i);
			value = Sum(value, ValueBetweenEntries(lhsIndex, invalidIndex, startTime));
			num += 1f;
			lhsIndex = invalidIndex;
		}
		if (num > 0f)
		{
			value = Scale(value, 1f / num);
			return true;
		}
		return false;
	}

	public T ValueAtTime(float time)
	{
		int lhsIndex = InvalidIndex;
		int rhsIndex = InvalidIndex;
		for (int i = 1; i <= array.Length; i++)
		{
			int num = RollingHeadOffset(i);
			if (array[num].Timestamp <= time)
			{
				lhsIndex = num;
				continue;
			}
			rhsIndex = num;
			break;
		}
		return ValueBetweenEntries(lhsIndex, rhsIndex, time);
	}

	private int RollingHeadOffset(int headOffset)
	{
		return (head + headOffset) % array.Length;
	}

	private void IncrementHead()
	{
		head = RollingHeadOffset(1);
	}

	private T ValueBetweenEntries(int lhsIndex, int rhsIndex, float desiredTime)
	{
		if (lhsIndex == InvalidIndex && rhsIndex == InvalidIndex)
		{
			return ZeroValue();
		}
		if (lhsIndex == InvalidIndex)
		{
			return array[rhsIndex].Value;
		}
		if (rhsIndex == InvalidIndex)
		{
			return array[lhsIndex].Value;
		}
		float timestamp = array[lhsIndex].Timestamp;
		float timestamp2 = array[rhsIndex].Timestamp;
		float t = (desiredTime - timestamp) / (timestamp2 - timestamp);
		return Interpolate(array[lhsIndex].Value, array[rhsIndex].Value, t);
	}

	protected abstract T ZeroValue();

	protected abstract T Interpolate(T lhs, T rhs, float t);

	protected abstract T Scale(T value, float t);

	protected abstract T Sum(T lhs, T rhs);
}
