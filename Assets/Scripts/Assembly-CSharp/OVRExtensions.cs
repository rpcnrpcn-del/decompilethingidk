using UnityEngine;
using UnityEngine.VR;

public static class OVRExtensions
{
	public static OVRPose ToTrackingSpacePose(this Transform transform)
	{
		OVRPose oVRPose = default(OVRPose);
		oVRPose.position = InputTracking.GetLocalPosition(VRNode.Head);
		oVRPose.orientation = InputTracking.GetLocalRotation(VRNode.Head);
		return oVRPose * transform.ToHeadSpacePose();
	}

	public static OVRPose ToHeadSpacePose(this Transform transform)
	{
		return Camera.current.transform.ToOVRPose().Inverse() * transform.ToOVRPose();
	}

	internal static OVRPose ToOVRPose(this Transform t, bool isLocal = false)
	{
		OVRPose result = default(OVRPose);
		result.orientation = ((!isLocal) ? t.rotation : t.localRotation);
		result.position = ((!isLocal) ? t.position : t.localPosition);
		return result;
	}

	internal static void FromOVRPose(this Transform t, OVRPose pose, bool isLocal = false)
	{
		if (isLocal)
		{
			t.localRotation = pose.orientation;
			t.localPosition = pose.position;
		}
		else
		{
			t.rotation = pose.orientation;
			t.position = pose.position;
		}
	}

	internal static OVRPose ToOVRPose(this OVRPlugin.Posef p)
	{
		return new OVRPose
		{
			position = new Vector3(p.Position.x, p.Position.y, 0f - p.Position.z),
			orientation = new Quaternion(0f - p.Orientation.x, 0f - p.Orientation.y, p.Orientation.z, p.Orientation.w)
		};
	}

	internal static OVRTracker.Frustum ToFrustum(this OVRPlugin.Frustumf f)
	{
		return new OVRTracker.Frustum
		{
			nearZ = f.zNear,
			farZ = f.zFar,
			fov = new Vector2
			{
				x = 57.29578f * f.fovX,
				y = 57.29578f * f.fovY
			}
		};
	}

	internal static Color FromColorf(this OVRPlugin.Colorf c)
	{
		return new Color
		{
			r = c.r,
			g = c.g,
			b = c.b,
			a = c.a
		};
	}

	internal static OVRPlugin.Colorf ToColorf(this Color c)
	{
		return new OVRPlugin.Colorf
		{
			r = c.r,
			g = c.g,
			b = c.b,
			a = c.a
		};
	}

	internal static Vector3 FromVector3f(this OVRPlugin.Vector3f v)
	{
		return new Vector3
		{
			x = v.x,
			y = v.y,
			z = v.z
		};
	}

	internal static Vector3 FromFlippedZVector3f(this OVRPlugin.Vector3f v)
	{
		return new Vector3
		{
			x = v.x,
			y = v.y,
			z = 0f - v.z
		};
	}

	internal static OVRPlugin.Vector3f ToVector3f(this Vector3 v)
	{
		return new OVRPlugin.Vector3f
		{
			x = v.x,
			y = v.y,
			z = v.z
		};
	}

	internal static OVRPlugin.Vector3f ToFlippedZVector3f(this Vector3 v)
	{
		return new OVRPlugin.Vector3f
		{
			x = v.x,
			y = v.y,
			z = 0f - v.z
		};
	}

	internal static Quaternion FromQuatf(this OVRPlugin.Quatf q)
	{
		return new Quaternion
		{
			x = q.x,
			y = q.y,
			z = q.z,
			w = q.w
		};
	}

	internal static OVRPlugin.Quatf ToQuatf(this Quaternion q)
	{
		return new OVRPlugin.Quatf
		{
			x = q.x,
			y = q.y,
			z = q.z,
			w = q.w
		};
	}
}
