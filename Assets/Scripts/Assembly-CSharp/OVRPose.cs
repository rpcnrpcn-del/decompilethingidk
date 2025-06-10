using System;
using UnityEngine;

[Serializable]
public struct OVRPose
{
	public Vector3 position;

	public Quaternion orientation;

	public static OVRPose identity
	{
		get
		{
			return new OVRPose
			{
				position = Vector3.zero,
				orientation = Quaternion.identity
			};
		}
	}

	public override bool Equals(object obj)
	{
		return obj is OVRPose && this == (OVRPose)obj;
	}

	public override int GetHashCode()
	{
		return position.GetHashCode() ^ orientation.GetHashCode();
	}

	public static bool operator ==(OVRPose x, OVRPose y)
	{
		return x.position == y.position && x.orientation == y.orientation;
	}

	public static bool operator !=(OVRPose x, OVRPose y)
	{
		return !(x == y);
	}

	public static OVRPose operator *(OVRPose lhs, OVRPose rhs)
	{
		return new OVRPose
		{
			position = lhs.position + lhs.orientation * rhs.position,
			orientation = lhs.orientation * rhs.orientation
		};
	}

	public OVRPose Inverse()
	{
		OVRPose result = default(OVRPose);
		result.orientation = Quaternion.Inverse(orientation);
		result.position = result.orientation * -position;
		return result;
	}

	internal OVRPose flipZ()
	{
		OVRPose result = this;
		result.position.z = 0f - result.position.z;
		result.orientation.z = 0f - result.orientation.z;
		result.orientation.w = 0f - result.orientation.w;
		return result;
	}

	internal OVRPlugin.Posef ToPosef()
	{
		return new OVRPlugin.Posef
		{
			Position = position.ToVector3f(),
			Orientation = orientation.ToQuatf()
		};
	}
}
