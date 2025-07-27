using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class RagdollBodyPart : MonoBehaviour {
    private Rigidbody rigidbody;
    private BoxCollider boxCollider;
    void Awake ()
    {
        base.gameObject.layer = 27; // DynamicPhysicsIgnoreEnemyPhysics
        this.boxCollider = base.GetComponent<BoxCollider>();
        this.boxCollider.enabled = false;
        this.rigidbody = base.GetComponent<Rigidbody>();
        this.rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        this.rigidbody.isKinematic = true;
        this.rigidbody.useGravity = false;
    }
    public void CopyRigidbodySettings(Rigidbody bodyRigidbody)
    {
        this.rigidbody.mass = bodyRigidbody.mass;
        this.rigidbody.drag = bodyRigidbody.drag;
        this.rigidbody.angularDrag = bodyRigidbody.angularDrag;
    }
    public void ApplyForce(Vector3 force)
    {
        this.boxCollider.enabled = true;
        this.rigidbody.useGravity = true;
        this.rigidbody.isKinematic = false;
        this.rigidbody.AddForce(force, ForceMode.VelocityChange);
    }
}
