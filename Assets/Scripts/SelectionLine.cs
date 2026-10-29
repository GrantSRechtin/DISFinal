using UnityEngine;

/// <summary>
/// Turns player intent into commands on IGravitizable objects.
/// Holds only "which objects am I holding"; the objects own everything else.
/// </summary>
public class SelectionLine : MonoBehaviour
{
    public IGravitizable a;
    public IGravitizable b;

    public Transform head;

    LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        lineRenderer.enabled = true;
    }

    void OnDisable()
    {
        a = null;
        b = null;
        
        lineRenderer.enabled = false;
    }

    void FixedUpdate()
    {
        if (a == null) { return; }

        Vector3 firstPoint = a.Position;

        Vector3 frontPoint = head.position + head.forward * 10f;

        Vector3 secondPoint = b == null ? frontPoint : b.Position;
        
        Vector3[] points = {firstPoint, secondPoint};
        lineRenderer.SetPositions(points);
    }
}