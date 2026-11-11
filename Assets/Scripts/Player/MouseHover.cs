using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MouseHover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private GameObject crosshair;

    private GameObject hand;
    private GameObject handBack;

    readonly float linkHoverDistance = 20f;
    readonly float holdHoverDistance = 5f;
    bool linkHighlighting = false;
    bool holdHighlighting = false;

    private void Awake()
    {
        if (crosshair == null)
        {
            Crosshair crosshairComponent = FindFirstObjectByType<Crosshair>();
            if (crosshairComponent != null) crosshair = crosshairComponent.gameObject;
        }

        hand = FindChild(crosshair.transform, "Grab");
        handBack = FindChild(crosshair.transform, "GrabBack");
    }

    private void Start()
    {
        if (hand != null) hand.SetActive(false);
        if (handBack != null) handBack.SetActive(false);
    }

    private void Update()
    {
        if (!holdHighlighting && hand != null && handBack != null)
        {
            // Hold Highlight
            IHoldable holdable = RaycastForHoldable();
            if (holdable != null && !holdable.IsSelected)
            {
                // Show Crosshair thing
                hand.SetActive(true);
                handBack.SetActive(true);
                holdHighlighting = true;
                StartCoroutine(HoldableUnhighlight(holdable));
            }
        }
        if(!linkHighlighting)
        {
            // Link Highlight
            IGravitizable gravitizable = RaycastForGravitizable();
            if (gravitizable != null && !gravitizable.IsSelected)
            {
                gravitizable.SetFrame(Color.white, true);
                linkHighlighting = true;
                StartCoroutine(GravitizableUnhighlight(gravitizable));
            }
        }
    }

    IEnumerator GravitizableUnhighlight(IGravitizable gravitizable)
    {
        // check both types
        while (ReferenceEquals(gravitizable, RaycastForGravitizable()) && !gravitizable.IsSelected)
        {
            yield return new WaitForEndOfFrame();
        }
        if (!gravitizable.IsSelected)
        {
            gravitizable.SetFrame(Color.white, false);
        }
        linkHighlighting = false;
    }

    IGravitizable RaycastForGravitizable()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, linkHoverDistance)) return null;
        return hit.collider.GetComponentInParent<IGravitizable>();
    }

    IEnumerator HoldableUnhighlight(IHoldable holdable)
    {
        // check both types
        while (ReferenceEquals(holdable, RaycastForHoldable()) && !holdable.IsSelected)
        {
            yield return new WaitForEndOfFrame();
        }
        hand.SetActive(false);
        handBack.SetActive(false);
        holdHighlighting = false;
    }

    IHoldable RaycastForHoldable()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, holdHoverDistance)) return null;
        return hit.collider.GetComponentInParent<IHoldable>();
    }

    private static GameObject FindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child != parent && child.name == childName) return child.gameObject;
        }

        return null;
    }
}