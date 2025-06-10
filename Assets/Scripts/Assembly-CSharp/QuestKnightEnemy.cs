using UnityEngine;

public class QuestKnightEnemy : QuestGroundEnemy
{
	public override void AuthorityApplyDamage(int damage, Vector3 collisionForce, Vector3 collisionPosition)
	{
		if (Vector3.Dot((collisionPosition - rigidbody.position).normalized, base.transform.forward) <= 0f)
		{
			damage = 0;
			collisionForce *= 0.25f;
		}
		base.AuthorityApplyDamage(damage, collisionForce, collisionPosition);
	}
}
