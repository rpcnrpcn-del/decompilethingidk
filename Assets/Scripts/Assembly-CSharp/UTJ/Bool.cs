namespace UTJ
{
	public struct Bool
	{
		private byte v;

		public static Bool True
		{
			get
			{
				Bool result = default(Bool);
				result.v = 1;
				return result;
			}
		}

		public static implicit operator bool(Bool v)
		{
			return v.v != 0;
		}

		public static implicit operator Bool(bool v)
		{
			Bool result = default(Bool);
			result.v = (byte)(v ? 1 : 0);
			return result;
		}
	}
}
