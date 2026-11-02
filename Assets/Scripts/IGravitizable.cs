using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IGravitizable
{
    Vector3 Position { get; } // World position

    // Selection
    bool IsSelected { get; }
    void Select(SelectionStyle style);
    void CheckDeselect();
    void SetFrame(Color color, bool visible);

    // Link
    HashSet<int> LinkNums { get; }
    Dictionary<int, IGravitizable> Partners { get; }
    void Connect(IGravitizable partner, int linkNum);
    void Disconnect(int linkNum);

    // Anchor
    Dictionary<int, Vector3> Anchors { get; }
    void SetAnchor(Vector3 anchorPoint, int linkNum);
    Vector3 GetAnchor(int linkNum);

    // Gravity
    Dictionary<int, GravityMode> Modes { get; }
    void SetMode(GravityMode mode, int linkNum);
    void Neutralize(int linkNum);
}

public class GravitizableBase : MonoBehaviour, IGravitizable
{
    [Header("Selection Frame")]
    [SerializeField] GameObject frame;

    Renderer[] frameRenderers = System.Array.Empty<Renderer>();
    GravityController gravityController;

    public HashSet<int> LinkNums { get; private set; } = new();
    public Dictionary<int, IGravitizable> Partners { get; private set; } = new();
    public Dictionary<int, GravityMode> Modes { get; private set; } = new();
    public Dictionary<int, Vector3> Anchors { get; private set; }= new();

    public Vector3 Position => transform.position;
    public bool IsSelected { get; private set; } = false;

    protected virtual void Awake()
    {
        if (frame == null) frame = FindFrame();
        if (frame != null) frameRenderers = frame.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in frameRenderers) r.enabled = false;
    }

    void OnEnable()
    {
        gravityController = GravityController.Instance;
        gravityController.GravLinksChanged += OnGravLinksChanged;
    }

    void OnDisable()
    {   
        if (gravityController != null)
        {
            DisconnectAll();
            CheckDeselect();

            gravityController.GravLinksChanged -= OnGravLinksChanged;
            gravityController = null;
        }
    }

    // ---------------- Selection ----------------

    public void Select(SelectionStyle style)
    {
        IsSelected = true;
        SetFrame(style.color, style.frameScale, true);
    }

    public void CheckDeselect()
    {
        GravityController instance = GravityController.Instance;
        GravLink link = instance.gravLinks[instance.currentLinkNum];

        if (LinkNums.Count > 0 || link.gravitizables.Count > 0) return;
        
        IsSelected = false;
        SetFrame(Color.white, false);
    }

    void OnGravLinksChanged()
    {
        GravLink[] links = GravityController.Instance.gravLinks;

        // update links, modes, anchors, & partners
        foreach(int linkNum in LinkNums)
        {
            GravityController instance = GravityController.Instance;
            GravLink link = links[linkNum];
            if (!link.Active() || !link.Contains(this)) 
            {
                Disconnect(linkNum);
                continue;
            }

            Partners[linkNum] = link.GetPartner(this);
            Modes[linkNum] = instance.gravLinks[linkNum].mode;
        }

        CheckDeselect();
        UpdateFrameColor();
    }

    // ---------------- Link ----------------

    public void Connect(IGravitizable partner, int linkNum)
    {
        // Make sure not already partner (a1,b1 && a1,b1) or self
        if (Partners.ContainsValue(partner) || ReferenceEquals(partner, this)) return; // May need to loop + reference equals if contain doesn't work

        // Disconnect if something already in slot
        if (LinkNums.Contains(linkNum)) Disconnect(linkNum);

        LinkNums.Add(linkNum);
        Partners[linkNum] = partner;
        Modes[linkNum] = GravityMode.None;
        // Anchors[linkNum] = something

        partner.Connect(this, linkNum);
    }

    public void Disconnect(int linkNum)
    {
        if (!LinkNums.Contains(linkNum)) return;

        IGravitizable oldPartner = Partners[linkNum];

        LinkNums.Remove(linkNum);
        Partners.Remove(linkNum);
        Modes.Remove(linkNum);
        Anchors.Remove(linkNum);

        oldPartner.Disconnect(linkNum);
    }

    void DisconnectAll()
    {
        foreach(int linkNum in LinkNums)
        {
            Partners.Remove(linkNum);
            Modes.Remove(linkNum);
            Anchors.Remove(linkNum);
        }

        LinkNums.Clear();
    }

    // ------------------ Anchors ------------------
    public void SetAnchor(Vector3 anchorPoint, int linkNum)
    {
        Anchors[linkNum] = anchorPoint;
    }
    public virtual Vector3 GetAnchor(int linkNum)
    {
        return Anchors[linkNum];
    }

    // ---------------- Gravity mode ----------------

    public void SetMode(GravityMode mode, int linkNum)
    {
        if (!LinkNums.Contains(linkNum)) return;
        Modes[linkNum] = mode;
    }

    public void Neutralize(int linkNum) => SetMode(GravityMode.None, linkNum);

    // ---------------- Frame visuals ----------------

    GameObject FindFrame()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child != transform && child.CompareTag("Frame"))
                return child.gameObject;
        }
        return null;
    }

    public void SetFrame(Color color, bool visible)
    {
        foreach (Renderer r in frameRenderers)
        {
            r.material.color = color;
            r.material.SetColor("_EmissionColor", color * 1.5f);
            r.enabled = visible;
        }
    }

    void SetFrame(Color color, Vector3 scale, bool visible)
    {
        if (frame != null) frame.transform.localScale = scale;
        foreach (Renderer r in frameRenderers)
        {
            r.material.color = color;
            r.material.SetColor("_EmissionColor", color * 1.5f);
            r.enabled = visible;
        }
    }

    void UpdateFrameColor()
    {
        int i = 0;
        Color32 combinedColor = Color.white;
        foreach(int linkNum in LinkNums)
        {
            Color color = GravityController.Instance.gravLinks[linkNum].style.color;

            float proportion = 1f/(i+1f); i++;
            Color lerpedColor = Color32.Lerp(combinedColor, color,  proportion);

            combinedColor = i == 0 ? color : lerpedColor;
        }
        SetFrame(combinedColor, IsSelected);
    }
}