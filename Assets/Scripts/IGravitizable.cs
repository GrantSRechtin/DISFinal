using Unity.VisualScripting;
using UnityEngine;

public interface IGravitizable
{
    Vector3 Position { get; } // World position

    // Selection
    bool IsSelected { get; }
    void Select(SelectionStyle style);
    void Deselect();
    void SetFrame(Color color, bool visible);

    // Link
    int LinkNum { get; }
    // int[] LinkNums { get; }
    IGravitizable Partner { get; }
    // IGravitizable Parter(int linkNum);
    void Connect(IGravitizable partner, int linkNum);
    void Disconnect();
    // void Disconnect(int linkNum);

    // Anchor
    Vector3 GetAnchor();
    // Vector3 GetAnchor(int linkNum);
    void SetAnchor(Vector3 anchorPoint);
    // void GetAnchor(Vector3 anchorPoint, int linkNum);

    // Gravity
    GravityMode Mode { get; }
    // GravityMode Mode(int linkNum);
    void SetMode(GravityMode mode);
    // void SetMode(GravityMode mode, int linkNum);
    void Neutralize();
    // void Neutralize(int linkNum);
}

public class GravitizableBase : MonoBehaviour, IGravitizable
{
    [Header("Selection Frame")]
    [SerializeField] GameObject frame;

    Renderer[] frameRenderers = System.Array.Empty<Renderer>();
    Vector3 anchorPosition;

    public int LinkNum { get; private set; } = -1;
    public Vector3 Position => transform.position;
    public bool IsSelected { get; private set; } = false;
    public IGravitizable Partner { get; private set; }
    public GravityMode Mode { get; private set; } = GravityMode.None;

    protected virtual void Awake()
    {
        if (frame == null) frame = FindFrame();
        if (frame != null) frameRenderers = frame.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in frameRenderers) r.enabled = false;
    }

    // protected virtual since
    // - can be overridden
    // - not public
    protected virtual void OnDisable()
    {
        Deselect();
        Disconnect();
    }

    // ---------------- Selection ----------------

    public void Select(SelectionStyle style)
    {
        IsSelected = true;

        if (frame != null) frame.transform.localScale = style.frameScale;
        SetFrame(style.color, true);
    }

    public void Deselect()
    {
        if (!IsSelected) return;

        Disconnect();
        IsSelected = false;
        
        SetFrame(Color.white, false);
    }

    // ---------------- Link ----------------

    public void Connect(IGravitizable partner, int linkNum)
    {
        if (partner == null || ReferenceEquals(partner, this)) return;

        if (ReferenceEquals(Partner, partner)) return;
        if (Partner != null) Disconnect();

        LinkNum = linkNum;
        Partner = partner;
        Mode = GravityMode.None;

        partner.Connect(this, LinkNum);
    }

    public void Disconnect()
    {
        if (Partner == null) return;

        LinkNum = -1;
        IGravitizable oldPartner = Partner;
        Partner = null;
        Mode = GravityMode.None;

        oldPartner.Disconnect();
    }
    public virtual Vector3 GetAnchor()
    {
        return anchorPosition;
    }
    public void SetAnchor(Vector3 anchorPoint)
    {
        anchorPosition = anchorPoint;
    }

    // ---------------- Gravity mode ----------------

    public void SetMode(GravityMode mode)
    {
        if (Partner == null) return;
        if (mode == Mode) return;

        Mode = mode;
    }

    public void Neutralize() => SetMode(GravityMode.None);

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
}