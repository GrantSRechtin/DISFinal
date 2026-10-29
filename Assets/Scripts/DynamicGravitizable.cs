using UnityEngine;

/// <summary>
/// A physics object that can be selected, linked to a partner, and pulled toward
/// or pushed from it. Owns its own frame and forces.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class DynamicGravitizable : MonoBehaviour, IGravitizable
{
    [Header("Gravity")]
    [SerializeField] float gravitationalForce = 40f;
    [SerializeField] float distanceScaling = 0.5f;
    [SerializeField] float minDist = 10f;
    [SerializeField] float pushMultiplier = -0.625f;
    [SerializeField] float minForce = 5f;

    [Header("Selection Frame")]
    [Tooltip("Optional. If empty, the first child tagged \"Frame\" is used.")]
    [SerializeField] GameObject frame;

    Rigidbody rb;
    Renderer[] frameRenderers = System.Array.Empty<Renderer>();

    // --- IGravitizable ---
    public int LinkNum { get; private set; } = -1;
    public Vector3 Position => transform.position;
    public SelectionSlot Slot { get; private set; } = SelectionSlot.None;
    public bool IsSelected => Slot != SelectionSlot.None;
    public IGravitizable Partner { get; private set; }
    public GravityMode Mode { get; private set; } = GravityMode.None;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (frame == null) frame = FindFrame();
        if (frame != null) frameRenderers = frame.GetComponentsInChildren<Renderer>(true);

        // Start unselected
        foreach (Renderer r in frameRenderers) r.enabled = false;
    }

    // Covers being disabled AND destroyed: clears selection and link so nothing
    // (partner or PlayerActions) is left pointing at a dead object
    void OnDisable()
    {
        Deselect();
        Disconnect(); // in case it was linked without being selected
    }

    void FixedUpdate()
    {
        if (Partner == null || Mode == GravityMode.None) return;

        rb.AddForce(CalculateForce(Partner.Position), ForceMode.Force);
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

    // ---------------- Physics (unchanged from the original player script) ----------------

    Vector3 CalculateForce(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;

        float distance = Mathf.Max(direction.magnitude, minDist);
        float powerRatio = 1f / Mathf.Pow(distance - minDist + 1f, distanceScaling);

        Vector3 force = direction.normalized * gravitationalForce * powerRatio;
        if (force.magnitude < minForce) return Vector3.zero;

        if (Mode == GravityMode.Push) force *= pushMultiplier;

        return force;
    }

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