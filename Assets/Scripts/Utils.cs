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
    public IGravitizable a;
    public IGravitizable b;

    public GravityMode mode;
    public SelectionStyle style;
    public GameObject line;

    public GravLink(SelectionStyle style, GameObject line, int linkNum)
    {
        a = null;
        b = null;
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
        return a != null && b != null;
    }
}