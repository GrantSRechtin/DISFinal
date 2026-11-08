using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turns player intent into commands on IGravitizable objects.
/// Holds only "which objects am I holding"; the objects own everything else.
/// </summary>
public class SelectionLine : RewindableBase
{
    public IGravitizable a;
    public IGravitizable b;

    public Transform head;
    public int linkNum;

    LineRenderer lineRenderer;
    List<Timestep> timesteps = new();

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        lineRenderer.enabled = true;
    }

    public override void OnStoreTimestep()
    {
        timesteps.Add(new Timestep
        {
            LineA = a,
            LineB = b,
            LineVisible = lineRenderer.enabled
        });
    }

    public override void OnRewindTimestep()
    {
        if (timesteps.Count == 0) return;

        int lastIndex = timesteps.Count - 1;
        Timestep timestep = timesteps[lastIndex];
        timesteps.RemoveAt(lastIndex);

        a = timestep.LineA;
        b = timestep.LineB;
        lineRenderer.enabled = timestep.LineVisible;
    }

    void FixedUpdate()
    {
        if (a == null) { return; }

        Vector3 firstPoint = a.GetAnchor(linkNum);

        Vector3 frontFloatingPoint = head.position + head.forward * 10f;

        Vector3 frontClosePoint = LookingPoint();
        Vector3 frontPoint = frontClosePoint != Vector3.zero ? frontClosePoint : frontFloatingPoint;

        Vector3 secondPoint = b == null ? frontPoint : b.GetAnchor(linkNum);
        
        Vector3[] points = {firstPoint, secondPoint};
        lineRenderer.SetPositions(points);
    }

    Vector3 LookingPoint()
    {
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit, 10f)) return Vector3.zero;
        return hit.point;
    }
}