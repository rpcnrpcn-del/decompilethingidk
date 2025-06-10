using UnityEngine;

public static class ToolJointConstructor
{
	private const bool linearLocked = true;

	private const float linearLimit = 0f;

	private const float linearLimitBounciness = 0f;

	private const float linearLimitContactDistance = 0f;

	private const float linearSpring = 20f;

	private const float linearSpringDamper = 0.25f;

	private const bool angularLocked = false;

	private const float angularLimit = 0f;

	private const float angularLimitBounciness = 0f;

	private const float angularLimitContactDistance = 0f;

	private const float angularSpring = 20f;

	private const float angularSpringDamper = 0.25f;

	public static Joint AddConfigurableJoint(GameObject jointGameObject)
	{
		ConfigurableJoint configurableJoint = jointGameObject.AddComponent<ConfigurableJoint>();
		configurableJoint.xMotion = ConfigurableJointMotion.Locked;
		configurableJoint.yMotion = ConfigurableJointMotion.Locked;
		configurableJoint.zMotion = ConfigurableJointMotion.Locked;
		configurableJoint.linearLimit = new SoftJointLimit
		{
			limit = 0f,
			bounciness = 0f,
			contactDistance = 0f
		};
		configurableJoint.linearLimitSpring = new SoftJointLimitSpring
		{
			spring = 20f,
			damper = 0.25f
		};
		configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
		configurableJoint.angularYMotion = ConfigurableJointMotion.Limited;
		configurableJoint.angularZMotion = ConfigurableJointMotion.Limited;
		SoftJointLimit softJointLimit = new SoftJointLimit
		{
			limit = 0f,
			bounciness = 0f,
			contactDistance = 0f
		};
		configurableJoint.lowAngularXLimit = softJointLimit;
		configurableJoint.highAngularXLimit = softJointLimit;
		configurableJoint.angularYLimit = softJointLimit;
		configurableJoint.angularZLimit = softJointLimit;
		SoftJointLimitSpring softJointLimitSpring = new SoftJointLimitSpring
		{
			spring = 20f,
			damper = 0.25f
		};
		configurableJoint.angularXLimitSpring = softJointLimitSpring;
		configurableJoint.angularYZLimitSpring = softJointLimitSpring;
		return configurableJoint;
	}
}
