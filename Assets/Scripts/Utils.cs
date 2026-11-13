using System.Collections.Generic;
using UnityEngine;

public enum GravityMode { None, Pull, Push }

public static class Utils
{
    public static IGravitizable RaycastForGravitizable(Transform rayOrigin, bool isHolding = false, float maxDistance = Mathf.Infinity)
    {
        if (isHolding) return null;
        if (!Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, maxDistance)) return null;
        return hit.collider.GetComponentInParent<IGravitizable>();
    }

    public static IHoldable RaycastForHoldable(Transform rayOrigin, bool isHolding = false, float maxDistance = Mathf.Infinity)
    {
        if (isHolding) return null;
        if (!Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, maxDistance)) return null;
        return hit.collider.GetComponentInParent<IHoldable>();
    }

    public static float RoundTo0625(float value) => Mathf.Round(value / 0.625f) * 0.625f;

    public static GameObject FindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child != parent && child.name == childName) return child.gameObject;
        }

        return null;
    }
}

[System.Serializable]
public struct SelectionStyle
{
    public Color color;
    public Vector3 frameScale;

    public SelectionStyle(Color color, Vector3 frameScale)
    {
        this.color = color;
        this.frameScale = frameScale;
    }
}

[System.Serializable]
public struct GravLink
{
    public List<IGravitizable> gravitizables;

    public GravityMode mode;
    public SelectionStyle style;
    public GameObject line;

    public GravLink(SelectionStyle style, GameObject line, int linkNum)
    {
        gravitizables = new();
        mode = GravityMode.None;

        this.style = style;
        this.line = line;

        // Currently since the grav link doesn't store linkNum this could go in controller
        var lineScript = line.GetComponent<SelectionLine>();
        lineScript.linkNum = linkNum;

        var lineRenderer = line.GetComponent<LineRenderer>();
        lineRenderer.startColor = style.color;
        lineRenderer.endColor = style.color;
    }

    public readonly bool Active()
    {
        return gravitizables.Count == 2;
    }

    public readonly bool Contains(IGravitizable gravitizable)
    {
        return gravitizables.Contains(gravitizable);
    }

    public readonly IGravitizable GetPartner(IGravitizable gravitizable)
    {
        if (!gravitizables.Contains(gravitizable)) return null;
        foreach (IGravitizable i in gravitizables)
        {
            if (!ReferenceEquals(i, gravitizable))
            {
                return i;
            }
        }
        return null;
    }
}