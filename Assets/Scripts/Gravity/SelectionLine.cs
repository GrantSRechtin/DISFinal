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

    public float length = 0;

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

    public override void OnRewindTimestep(int steps)
    {
        if (timesteps.Count == 0) return;

        Timestep timestep;
        if (timesteps.Count > steps)
        {
            int lastIndex = timesteps.Count - steps;
            timestep = timesteps[lastIndex];

            while(timesteps.Count > lastIndex+1)
            {
                timesteps.RemoveAt(timesteps.Count-1);
            }
            timesteps.RemoveAt(lastIndex);
        }
        else
        {
            int lastIndex = 0;
            timestep = timesteps[lastIndex];
            timesteps.Clear();
        }

        a = timestep.LineA;
        b = timestep.LineB;
        lineRenderer.enabled = timestep.LineVisible;
    }

    void FixedUpdate()
    {
        if (a == null) 
        { 
            length = 0;
            return; 
        }

        Vector3 firstPoint = a.GetAnchor(linkNum);

        Vector3 frontFloatingPoint = head.position + head.forward * 10f;

        Vector3 frontClosePoint = LookingPoint();
        Vector3 frontPoint = frontClosePoint != Vector3.zero ? frontClosePoint : frontFloatingPoint;

        // If longer do something
        Vector3 linkDirection = (firstPoint - frontPoint).normalized;
        float linkLength = Mathf.Clamp(
            (firstPoint - frontPoint).magnitude, 0, GravityController.Instance.maxLinkDistance);

        frontPoint = firstPoint - (linkDirection * linkLength);

        length = b == null ? linkLength : (a.GetAnchor(linkNum) - b.GetAnchor(linkNum)).magnitude;
        Vector3 secondPoint = b == null ? frontPoint : b.GetAnchor(linkNum);
        
        Vector3[] points = {firstPoint, secondPoint};
        lineRenderer.SetPositions(points);
    }

    Vector3 LookingPoint()
    {
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit, GravityController.Instance.maxLinkDistance)) return Vector3.zero;
        return hit.point;
    }
}