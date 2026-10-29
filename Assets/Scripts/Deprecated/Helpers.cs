using UnityEngine;
using UnityEngine.UI;

public class GravSlot
{
    public GravObject first;
    public GravObject second;
}

public class GravObject
{
    public GameObject targetObject;
    public SelectionVisuals visuals;
}

public class SelectionVisuals
{
    public GameObject targetObject; // Will Remove
    public Color color;
    public Vector3 frameScale;
    public Image actionImage;
    public Image borderImage;
}