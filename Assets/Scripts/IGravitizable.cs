using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Anything the player can select and link with gravity.
/// Callers only ever talk to this interface, never to a concrete class.
/// </summary>
public interface IGravitizable
{
    /// <summary>World position other objects are pulled toward / pushed from.</summary>
    Vector3 Position { get; }

    // --- Selection ---
    SelectionSlot Slot { get; }
    bool IsSelected { get; }
    void Select(SelectionSlot slot, SelectionStyle style);
    /// <summary>Also disconnects from any partner.</summary>
    void Deselect();
    void SetFrame(Color color, bool visible);

    // --- Link ---
    int LinkNum { get; }
    IGravitizable Partner { get; }
    /// <summary>Links both objects with Mode None. Calling it on either side is enough.</summary>
    void Connect(IGravitizable partner, int linkNum);
    /// <summary>Unlinks both objects and sets Mode to None. Calling it on either side is enough.</summary>
    void Disconnect();

    // --- Gravity ---
    /// <summary>Always shared with the partner. None whenever there is no partner.</summary>
    GravityMode Mode { get; }
    /// <summary>Sets the mode on both linked objects. Ignored if there is no partner.</summary>
    void SetMode(GravityMode mode);
    /// <summary>Back to None while staying linked.</summary>
    void Neutralize();
}

public class GravitizableBase : MonoBehaviour, IGravitizable
{
    [Header("Selection Frame")]
    [SerializeField] GameObject frame;

    Renderer[] frameRenderers = System.Array.Empty<Renderer>();

    // --- IGravitizable ---
    public int LinkNum { get; private set; } = -1;
    public Vector3 Position => transform.position;
    public SelectionSlot Slot { get; private set; } = SelectionSlot.None;
    public bool IsSelected => Slot != SelectionSlot.None;
    public IGravitizable Partner { get; private set; }
    public GravityMode Mode { get; private set; } = GravityMode.None;

    protected virtual void Awake()
    {
        if (frame == null) frame = FindFrame();
        if (frame != null) frameRenderers = frame.GetComponentsInChildren<Renderer>(true);

        // Start unselected
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

    public void Select(SelectionSlot slot, SelectionStyle style)
    {
        if (slot == SelectionSlot.None)
        {
            Deselect();
            return;
        }

        Slot = slot;

        if (frame != null) frame.transform.localScale = style.frameScale;
        SetFrame(style.color, true);
    }

    public void Deselect()
    {
        if (!IsSelected) return;

        Disconnect();
        Slot = SelectionSlot.None;
        SetFrame(Color.white, false);
    }

    // ---------------- Link ----------------

    public void Connect(IGravitizable partner, int linkNum)
    {
        if (partner == null || ReferenceEquals(partner, this)) return;

        // Already linked: also stops the mirrored call below from recursing forever
        if (ReferenceEquals(Partner, partner)) return;

        // Switching to a different partner: release the old one first
        if (Partner != null) Disconnect();

        LinkNum = linkNum;
        Partner = partner;
        Mode = GravityMode.None;

        partner.Connect(this, LinkNum); // mirror on the other side
    }

    public void Disconnect()
    {
        if (Partner == null) return;

        LinkNum = -1;
        IGravitizable oldPartner = Partner;
        Partner = null;
        Mode = GravityMode.None; // losing the partner always means None

        oldPartner.Disconnect(); // mirror on the other side (returns early once cleared)
    }

    // ---------------- Gravity mode ----------------

    public void SetMode(GravityMode mode)
    {
        if (Partner == null) return; // nothing to pull toward / push from
        if (mode == Mode) return;    // also stops the mirrored call from recursing

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