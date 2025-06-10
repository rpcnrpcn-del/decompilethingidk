using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExitGames.Client.Photon;
using UnityEngine;

public static class UnityExtensions
{
	public static float SmallNumber = 0.0001f;

	public static float SmallDegree = 1f;

	public static void ThrowIfNull<T>(this Component component, T value, string message) where T : class
	{
		if (value == null)
		{
			throw new NullReferenceException(message);
		}
	}

	public static void SetLayerRecursively(this GameObject gameObject, Layers layer)
	{
		gameObject.layer = (int)layer;
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetLayerRecursively(layer);
		}
	}

	public static void ReplaceLayerRecursively(this GameObject gameObject, Layers currentLayer, Layers desiredLayer)
	{
		if (gameObject.layer == (int)currentLayer)
		{
			gameObject.layer = (int)desiredLayer;
		}
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.ReplaceLayerRecursively(currentLayer, desiredLayer);
		}
	}

	public static bool IsInLayerMask(this GameObject gameObject, LayerMasks layerMask)
	{
		return ((uint)(1 << gameObject.layer) & (uint)layerMask) != 0;
	}

	public static T GetComponentInParents<T>(this GameObject gameObject) where T : Component
	{
		Transform transform = gameObject.transform;
		while (transform != null)
		{
			T component = transform.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			transform = transform.parent;
		}
		return (T)null;
	}

	public static Color WithAlpha(this Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, alpha);
	}

	public static int AsLayerMask(this Layers layer)
	{
		return 1 << (int)layer;
	}

	public static void HideLayer(this Camera cam, Layers layer)
	{
		cam.HideLayerMask(layer.AsLayerMask());
	}

	public static void ShowLayer(this Camera cam, Layers layer)
	{
		cam.ShowLayerMask(layer.AsLayerMask());
	}

	public static void HideLayerMask(this Camera cam, int layerMask)
	{
		cam.cullingMask &= ~layerMask;
	}

	public static void ShowLayerMask(this Camera cam, int layerMask)
	{
		cam.cullingMask |= layerMask;
	}

	public static void SetKeywordEnabled(this Material mat, string keyword, bool enabled)
	{
		if (enabled)
		{
			mat.EnableKeyword(keyword);
		}
		else
		{
			mat.DisableKeyword(keyword);
		}
	}

	public static AnimationCurve DefaultAnimationCurve()
	{
		return AnimationCurve.Linear(0f, 0f, 1f, 1f);
	}

	public static void ClearVelocity(this Rigidbody rb)
	{
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}

	public static Vector3 ProjectVectorToPlane(Vector3 source, Plane plane)
	{
		return source - Vector3.Dot(source, plane.normal) * plane.normal;
	}

	public static Quaternion FromToRotation(Quaternion from, Quaternion to)
	{
		return to * Quaternion.Inverse(from);
	}

	public static Quaternion QuaternionFromAngularVelocity(Vector3 angularVelocity)
	{
		return Quaternion.AngleAxis(angularVelocity.magnitude * 57.29578f, angularVelocity.normalized);
	}

	public static Vector3 AngularVelocityFromTo(Quaternion from, Quaternion to)
	{
		float angle;
		Vector3 axis;
		FromToRotation(from, to).ToAngleAxis(out angle, out axis);
		if (angle > 180f)
		{
			angle -= 360f;
		}
		return angle * axis * ((float)Math.PI / 180f);
	}

	public static Vector3 TransformToWorldSpace(this Vector3 v, Vector3 localSpaceOrigin, Vector3 localSpaceRight, Vector3 localSpaceUp, Vector3 localSpaceForward)
	{
		return localSpaceOrigin + v.x * localSpaceRight + v.y * localSpaceUp + v.z * localSpaceForward;
	}

	public static Vector3 TransformToWorldSpace(this Vector3 v, Vector3 localSpaceOrigin, Quaternion localSpaceRotation)
	{
		return localSpaceOrigin + localSpaceRotation * v;
	}

	public static Quaternion InverseTransformRotation(this Transform transform, Quaternion rotation)
	{
		return Quaternion.Inverse(transform.rotation) * rotation;
	}

	public static Quaternion TransformRotation(this Transform transform, Quaternion rotation)
	{
		return transform.rotation * rotation;
	}

	public static Quaternion TransformRotation(this Quaternion parent, Quaternion rotation)
	{
		return parent * rotation;
	}

	public static void SetCustomProperties<T>(this Room room, string propertyName, T newPropertyValue, T expectedValues)
	{
		if (room != null)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(propertyName, newPropertyValue);
			Hashtable hashtable2 = new Hashtable();
			hashtable2.Add(propertyName, expectedValues);
			room.SetCustomProperties(hashtable, hashtable2);
		}
	}

	public static void SetCustomProperties<T>(this Room room, string propertyName, T newPropertyValue)
	{
		if (room != null)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(propertyName, newPropertyValue);
			room.SetCustomProperties(hashtable);
		}
	}

	public static string GetCustomPropertyString(this PhotonPlayer player, string propertyName)
	{
		if (player != null && player.customProperties.ContainsKey(propertyName))
		{
			return player.customProperties[propertyName] as string;
		}
		return null;
	}

	public static void SetCustomProperties<T>(this PhotonPlayer player, string propertyName, T newPropertyValue, T expectedValues)
	{
		if (player != null)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(propertyName, newPropertyValue);
			Hashtable hashtable2 = new Hashtable();
			hashtable2.Add(propertyName, expectedValues);
			player.SetCustomProperties(hashtable, hashtable2);
		}
	}

	public static void SetCustomProperties<T>(this PhotonPlayer player, string propertyName, T newPropertyValue)
	{
		if (player != null)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(propertyName, newPropertyValue);
			player.SetCustomProperties(hashtable);
		}
	}

	public static void RemoveCustomProperties(this PhotonPlayer player, string propertyName)
	{
		if (player != null)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(propertyName, null);
			player.SetCustomProperties(hashtable);
		}
	}

	public static void RemoveCustomProperties(this Room room, string propertyName)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add(propertyName, null);
		room.SetCustomProperties(hashtable);
	}

	public static Player ToPlayer(this PhotonPlayer photonPlayer)
	{
		return (photonPlayer == null) ? null : (photonPlayer.TagObject as Player);
	}

	public static T Random<T>(this List<T> list)
	{
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count - 1)];
		}
		return default(T);
	}

	public static T Random<T>(this T[] list)
	{
		if (list.Length > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Length - 1)];
		}
		return default(T);
	}

	public static bool InsideLineSegment(this Vector3 position, Vector3 pointA, Vector3 pointB, Vector3 lineAB, out float distanceVariance)
	{
		Vector3 lhs = position - pointA;
		Vector3 vector = pointA + lineAB * Vector3.Dot(lhs, lineAB);
		float sqrMagnitude = (vector - pointA).sqrMagnitude;
		float sqrMagnitude2 = (vector - pointB).sqrMagnitude;
		float sqrMagnitude3 = (pointA - pointB).sqrMagnitude;
		distanceVariance = Mathf.Max(sqrMagnitude, sqrMagnitude2) / sqrMagnitude3;
		return sqrMagnitude <= sqrMagnitude3 && sqrMagnitude2 <= sqrMagnitude3;
	}

	[Obsolete("Use ColorVault and save the guid instead of the color values - bilal")]
	public static string ToPropertyString(this Color color)
	{
		return string.Format("{0},{1},{2},{3}", color.r, color.g, color.b, color.a);
	}

	[Obsolete("Use ColorVault and save the guid instead of the color values - bilal")]
	public static Color ParseColorPropertyString(string propertyString)
	{
		Color white = Color.white;
		string[] array = propertyString.Split(',');
		if (array != null && array.Length == 4)
		{
			try
			{
				white.r = float.Parse(array[0]);
				white.g = float.Parse(array[1]);
				white.b = float.Parse(array[2]);
				white.a = float.Parse(array[3]);
			}
			catch
			{
			}
		}
		return white;
	}

	public static void CopyMap<T1, T2>(this Dictionary<T1, T2> original, ref Dictionary<T1, T2> copy)
	{
		foreach (KeyValuePair<T1, T2> item in original)
		{
			if (!copy.ContainsKey(item.Key))
			{
				copy.Add(item.Key, item.Value);
			}
		}
	}

	public static float CombinedStaticFriction(this PhysicMaterial thisMaterial, PhysicMaterial otherMaterial)
	{
		if (thisMaterial == null && otherMaterial == null)
		{
			return 0f;
		}
		if (otherMaterial == null)
		{
			return thisMaterial.staticFriction;
		}
		if (thisMaterial == null)
		{
			return otherMaterial.staticFriction;
		}
		return CombinePhysicsMaterialProperty(thisMaterial.staticFriction, otherMaterial.staticFriction, thisMaterial.frictionCombine);
	}

	public static float CombinedDynamicFriction(this PhysicMaterial thisMaterial, PhysicMaterial otherMaterial)
	{
		if (thisMaterial == null && otherMaterial == null)
		{
			return 0f;
		}
		if (otherMaterial == null)
		{
			return thisMaterial.dynamicFriction;
		}
		if (thisMaterial == null)
		{
			return otherMaterial.dynamicFriction;
		}
		return CombinePhysicsMaterialProperty(thisMaterial.dynamicFriction, otherMaterial.dynamicFriction, thisMaterial.frictionCombine);
	}

	public static float CombinedBounciness(this Collider thisCollider, Collider otherCollider)
	{
		PhysicMaterial thisMaterial = ((!(thisCollider == null)) ? thisCollider.material : null);
		PhysicMaterial otherMaterial = ((!(otherCollider == null)) ? otherCollider.material : null);
		return thisMaterial.CombinedBounciness(otherMaterial);
	}

	public static float CombinedBounciness(this PhysicMaterial thisMaterial, PhysicMaterial otherMaterial)
	{
		if (thisMaterial == null && otherMaterial == null)
		{
			return 1f;
		}
		if (otherMaterial == null)
		{
			return thisMaterial.bounciness;
		}
		if (thisMaterial == null)
		{
			return otherMaterial.bounciness;
		}
		return CombinePhysicsMaterialProperty(thisMaterial.bounciness, otherMaterial.bounciness, thisMaterial.bounceCombine);
	}

	private static float CombinePhysicsMaterialProperty(float lhs, float rhs, PhysicMaterialCombine combineFunction)
	{
		float result = 0f;
		switch (combineFunction)
		{
		case PhysicMaterialCombine.Average:
			result = (lhs + rhs) / 2f;
			break;
		case PhysicMaterialCombine.Maximum:
			result = Mathf.Max(lhs, rhs);
			break;
		case PhysicMaterialCombine.Minimum:
			result = Mathf.Min(lhs, rhs);
			break;
		case PhysicMaterialCombine.Multiply:
			result = lhs * rhs;
			break;
		}
		return result;
	}

	public static void SortByDistanceToCenter(this RaycastHit[] hits, int count)
	{
		if (count <= 1)
		{
			return;
		}
		float[] array = new float[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = (hits[i].collider.transform.position - hits[i].point).sqrMagnitude;
		}
		for (int j = 1; j < count; j++)
		{
			int num = j;
			while (num > 0 && array[num] < array[num - 1])
			{
				hits.Exchange(num, j);
				array.Exchange(num, j);
				num--;
			}
		}
	}

	public static void Exchange<T>(this T[] array, int i, int j)
	{
		T val = array[i];
		array[i] = array[j];
		array[j] = val;
	}

	public static Tool GetColliderTool(this Collision collision)
	{
		return collision.collider.GetColliderTool();
	}

	public static Tool GetColliderTool(this Collider collider)
	{
		ToolCollider toolCollider = null;
		return collider.GetColliderTool(out toolCollider);
	}

	public static Tool GetColliderTool(this Collider collider, out ToolCollider toolCollider)
	{
		toolCollider = collider.GetComponent<ToolCollider>();
		if (toolCollider == null)
		{
			return null;
		}
		return toolCollider.Tool;
	}

	public static Player GetColliderPlayer(this Collision collision, out Player.BodyPart bodyPart)
	{
		return collision.collider.GetColliderPlayer(out bodyPart);
	}

	public static Player GetColliderPlayer(this Collider collider, out Player.BodyPart bodyPart)
	{
		PlayerCollider component = collider.GetComponent<PlayerCollider>();
		if (component == null)
		{
			bodyPart = Player.BodyPart.None;
			return null;
		}
		bodyPart = component.BodyPart;
		return component.ThisPlayer;
	}

	public static Enemy GetColliderEnemy(this Collision collision)
	{
		return collision.collider.GetColliderEnemy();
	}

	public static Enemy GetColliderEnemy(this Collider collider)
	{
		EnemyCollider component = collider.GetComponent<EnemyCollider>();
		return (!(component != null)) ? null : component.ThisEnemy;
	}

	public static PhotonView GetColliderPhotonView(this Collision collision)
	{
		return collision.collider.GetColliderPhotonView();
	}

	public static PhotonView GetColliderPhotonView(this Collider collider)
	{
		return collider.gameObject.GetComponentInParents<PhotonView>();
	}

	public static Killzone GetColliderKillzone(this Collider collider)
	{
		return collider.gameObject.GetComponent<Killzone>();
	}

	public static bool CheckOverlap(this BoxCollider collider, int layerMask, QueryTriggerInteraction triggerInteraction)
	{
		Vector3 center = collider.transform.TransformPoint(collider.center);
		Quaternion rotation = collider.transform.rotation;
		Vector3 halfExtents = collider.size / 2f;
		return Physics.CheckBox(center, halfExtents, rotation, layerMask, triggerInteraction);
	}

	public static bool CheckOverlap(this CapsuleCollider collider, int layerMask, QueryTriggerInteraction triggerInteraction)
	{
		Vector3 direction = Vector3.right;
		if (collider.direction == 1)
		{
			direction = Vector3.up;
		}
		else if (collider.direction == 2)
		{
			direction = Vector3.forward;
		}
		Vector3 normalized = collider.transform.TransformDirection(direction).normalized;
		float num = Mathf.Max(0f, collider.height - 2f * collider.radius);
		Vector3 vector = collider.transform.TransformPoint(collider.center);
		return Physics.CheckCapsule(vector - normalized * num, vector + normalized * num, collider.radius, layerMask, triggerInteraction);
	}

	public static Quaternion SmoothDamp(Quaternion current, Quaternion target, ref Vector3 eulerAngularVelocity, float smoothTime)
	{
		Vector3 eulerAngles = current.eulerAngles;
		Vector3 eulerAngles2 = target.eulerAngles;
		eulerAngles.x = Mathf.SmoothDampAngle(eulerAngles.x, eulerAngles2.x, ref eulerAngularVelocity.x, smoothTime);
		eulerAngles.y = Mathf.SmoothDampAngle(eulerAngles.y, eulerAngles2.y, ref eulerAngularVelocity.y, smoothTime);
		eulerAngles.z = Mathf.SmoothDampAngle(eulerAngles.z, eulerAngles2.z, ref eulerAngularVelocity.z, smoothTime);
		return Quaternion.Euler(eulerAngles);
	}

	public static T2[] ToArray<T1, T2>(this Dictionary<T1, T2>.ValueCollection valueCollection)
	{
		if (valueCollection.Count > 0)
		{
			T2[] array = new T2[valueCollection.Count];
			valueCollection.CopyTo(array, 0);
			return array;
		}
		return null;
	}

	public static Vector3 ValueOrZeroIfBogus(this Vector3 vector)
	{
		return (!vector.IsNaN() && !vector.IsInfinity()) ? vector : Vector3.zero;
	}

	public static bool IsNaN(this Vector3 vector)
	{
		return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
	}

	public static bool IsInfinity(this Vector3 vector)
	{
		return float.IsInfinity(vector.x) || float.IsInfinity(vector.y) || float.IsInfinity(vector.z);
	}

	public static Quaternion ValueOrIdentityIfBogus(this Quaternion quaternion)
	{
		return (!quaternion.IsNaN() && !quaternion.IsInfinity()) ? quaternion : Quaternion.identity;
	}

	public static bool IsNaN(this Quaternion quaternion)
	{
		return float.IsNaN(quaternion.x) || float.IsNaN(quaternion.y) || float.IsNaN(quaternion.z) || float.IsNaN(quaternion.w);
	}

	public static bool IsInfinity(this Quaternion quaternion)
	{
		return float.IsInfinity(quaternion.x) || float.IsInfinity(quaternion.y) || float.IsInfinity(quaternion.z) || float.IsInfinity(quaternion.w);
	}

	public static Vector3 MultiplyComponents(this Vector3 me, Vector3 other)
	{
		return new Vector3(me.x * other.x, me.y * other.y, me.z * other.z);
	}

	public static float MinComponent(this Vector3 me)
	{
		return (me.x > me.y) ? ((!(me.y > me.z)) ? me.y : me.z) : ((!(me.x > me.z)) ? me.x : me.z);
	}

	public static bool Contains<T>(this T[] array, T item)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Equals(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool WithinBounds(this Vector2 bounds, float value)
	{
		float num;
		float num2;
		if (bounds.x > bounds.y)
		{
			num = bounds.y;
			num2 = bounds.x;
		}
		else
		{
			num = bounds.x;
			num2 = bounds.y;
		}
		return value >= num && value <= num2;
	}

	public static bool IsHand(this Player.BodyPart part)
	{
		return part == Player.BodyPart.LeftHand || part == Player.BodyPart.RightHand;
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		int num = list.Count;
		System.Random random = new System.Random();
		while (num > 1)
		{
			int index = random.Next(0, num);
			num--;
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static T LastItem<T>(this IList<T> list)
	{
		return list[list.Count - 1];
	}

	public static void ShuffleArray<T>(T[] array)
	{
		ShuffleArray(array, (int)DateTime.Now.Ticks);
	}

	public static void ShuffleArray<T>(T[] array, int randomSeed)
	{
		int num = array.Length;
		UnityEngine.Random.InitState(randomSeed);
		while (num > 1)
		{
			int num2 = UnityEngine.Random.Range(0, num--);
			T val = array[num];
			array[num] = array[num2];
			array[num2] = val;
		}
	}

	public static string GetGameObjectHierarchy(this GameObject go)
	{
		Transform transform = go.transform;
		string text = transform.name;
		transform = transform.parent;
		while (transform != null)
		{
			text = transform.name + " -> " + text;
			transform = transform.parent;
		}
		return text;
	}

	public static float ReMapRange(this float value, Vector2 range, Vector2 newRange)
	{
		float num = range.y - range.x;
		float num2 = (value - range.x) / num;
		float num3 = newRange.y - newRange.x;
		return newRange.x + num3 * num2;
	}

	public static string ToString(this Guid guid)
	{
		return guid.ToString("N");
	}

	public static bool GetCombinedRendererBounds(List<Renderer> renderers, out Bounds bounds, List<Renderer> additionalRenderers = null)
	{
		bool flag = false;
		bounds = default(Bounds);
		if (additionalRenderers == null)
		{
			additionalRenderers = new List<Renderer>();
		}
		foreach (Renderer item in renderers.Concat(additionalRenderers))
		{
			if (!(item is ParticleSystemRenderer) && !(item is TrailRenderer))
			{
				if (!flag)
				{
					bounds = item.bounds;
					flag = true;
				}
				else if (item != null)
				{
					bounds.Encapsulate(item.bounds);
				}
			}
		}
		return flag;
	}

	public static string ToTimeString(this float seconds)
	{
		int num = Mathf.RoundToInt(seconds);
		return (num / 60).ToString("D2") + ":" + (num % 60).ToString("D2");
	}

	public static void SetColorAlpha(this Renderer rend, float alpha)
	{
		Color color = rend.material.color;
		color.a = alpha;
		rend.material.color = color;
	}

	public static int AnchorIndex(this TextAnchor anchor)
	{
		switch (anchor)
		{
		case TextAnchor.UpperLeft:
		case TextAnchor.UpperCenter:
		case TextAnchor.UpperRight:
			return 1;
		default:
			return 0;
		case TextAnchor.LowerLeft:
		case TextAnchor.LowerCenter:
		case TextAnchor.LowerRight:
			return -1;
		}
	}

	public static float AngleSignedVector3(this Vector3 from, Vector3 to, Vector3? up = null)
	{
		up = ((!up.HasValue) ? Vector3.up : up.Value);
		return Vector3.Angle(from, to) * Mathf.Sign(Vector3.Dot(Vector3.Cross(from, to), up.GetValueOrDefault()));
	}

	public static float AngleSignedVector2(this Vector2 from, Vector2 to)
	{
		Vector3 vector = from;
		Vector3 to2 = to;
		return vector.AngleSignedVector3(to2, Vector3.forward);
	}

	public static bool IsValidEmail(string email)
	{
		Regex regex = new Regex("^[\\w!#$%&'*+\\-/=?\\^_`{|}~]+(\\.[\\w!#$%&'*+\\-/=?\\^_`{|}~]+)*@((([\\-\\w]+\\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\\.){3}[0-9]{1,3}))$");
		Match match = regex.Match(email);
		return match.Success;
	}

	public static string Truncate(this string value, int maxLength, string appendIfLong = "")
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		return (value.Length > maxLength) ? (value.Substring(0, maxLength) + appendIfLong) : value;
	}

	public static Color ChangeAlpha(this Color color, float newAlpha)
	{
		Color result = color;
		result.a = newAlpha;
		return result;
	}

	public static V IfNotNull<T, V>(this T t, Func<T, V> todo, V defaultValue = default(V)) where T : class
	{
		if (t != null)
		{
			return todo(t);
		}
		return defaultValue;
	}

	public static void DoIfNotNull<T>(this T t, Action<T> todo) where T : class
	{
		if (t != null)
		{
			todo(t);
		}
	}
}
