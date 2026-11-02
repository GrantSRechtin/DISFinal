using System.Collections.Generic;
using UnityEngine;

public enum GravityMode { None, Pull, Push }

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