using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongBall : Tool {
    [System.Serializable]
    private class TrailType
    {
        public ColliderImpactType ImpactType;

        public TrailRenderer TrailRenderer;
    }

    [SerializeField]
    private TrailType[] trails;

    private TrailRenderer _trail;

    private TrailRenderer trail
    {
        get
        {
            return _trail;
        }
        set
        {
            if (_trail != null && _trail.enabled)
            {
                _trail.enabled = false;
            }
            _trail = value;
            if (_trail != null)
            {
                _trail.enabled = true;
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        TrailType[] array = trails;
        foreach (TrailType trailType in array)
        {
            if (trailType.TrailRenderer != null)
            {
                trailType.TrailRenderer.enabled = false;
            }
        }
    }

    public void ClearTrails()
    {
        TrailType[] array = trails;
        foreach (TrailType trailType in array)
        {
            if (trailType.TrailRenderer != null)
            {
                trailType.TrailRenderer.Clear();
            }
        }
    }

    protected override void OnToolCollisionEnter(Tool hitTool, Vector3 point, Collision collision)
    {
        base.OnToolCollisionEnter(hitTool, point, collision);
        ToolCollider component = collision.collider.GetComponent<ToolCollider>();
        if (!(component != null))
        {
            return;
        }
        TrailType[] array = trails;
        foreach (TrailType trailType in array)
        {
            if (trailType.ImpactType == component.ImpactType)
            {
                trail = trailType.TrailRenderer;
            }
        }
    }
}
