using UnityEngine;

/// <summary>
/// Turns player intent into commands on IGravitizable objects.
/// Holds only "which objects am I holding"; the objects own everything else.
/// </summary>
public class SelectionLine : MonoBehaviour
{
    public IGravitizable first;
    public IGravitizable second;

    public Transform head;

    LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void FixedUpdate()
    {
        if (first == null) { Destroy(gameObject); }

        Vector3 firstPoint = first.Position;

        Vector3 frontPoint = head.position + head.forward * 10f;

        Vector3 secondPoint = second == null ? frontPoint : second.Position;
        
        Vector3[] points = {firstPoint, secondPoint};
        lineRenderer.SetPositions(points);
    }
}