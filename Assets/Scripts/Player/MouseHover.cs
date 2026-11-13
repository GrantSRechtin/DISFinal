using System.Collections;
using UnityEngine;

public class MouseHover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private GameObject crosshair;

    private GameObject hand;
    private GameObject handBack;

    private PlayerHold playerHold;

    readonly float linkHoverDistance = 20f;
    readonly float holdHoverDistance = 4f;
    bool linkHighlighting = false;
    bool holdHighlighting = false;

    private void Awake()
    {
        if (crosshair == null)
        {
            Crosshair crosshairComponent = FindFirstObjectByType<Crosshair>();
            if (crosshairComponent != null) crosshair = crosshairComponent.gameObject;
        }

        hand = Utils.FindChild(crosshair.transform, "Grab");
        handBack = Utils.FindChild(crosshair.transform, "GrabBack");

        playerHold = GetComponentInParent<PlayerHold>();
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
            IHoldable holdable = Utils.RaycastForHoldable(transform, maxDistance: holdHoverDistance);
            if (holdable != null)
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
            GravityController gc = GravityController.Instance;
            float hoverDistance = (gc.gravLinks[gc.currentLinkNum].gravitizables.Count == 1) ? Mathf.Infinity : linkHoverDistance;

            IGravitizable gravitizable = Utils.RaycastForGravitizable(transform, isHolding: playerHold.IsHolding, maxDistance: hoverDistance);
            if (gravitizable != null && !gravitizable.IsSelected && gc.gravLinks[gc.currentLinkNum].Length() < gc.maxLinkDistance)
            {
                gravitizable.SetFrame(Color.white, true);
                linkHighlighting = true;
                StartCoroutine(GravitizableUnhighlight(gravitizable, hoverDistance));
            }
        }
    }

    IEnumerator GravitizableUnhighlight(IGravitizable gravitizable, float hoverDistance)
    {
        while (
            ReferenceEquals(gravitizable, Utils.RaycastForGravitizable(transform, isHolding: playerHold.IsHolding, maxDistance: hoverDistance)) 
            && !gravitizable.IsSelected)
        {
            yield return new WaitForEndOfFrame();
        }
        if (!gravitizable.IsSelected)
        {
            gravitizable.SetFrame(Color.white, false);
        }
        linkHighlighting = false;
    }

    IEnumerator HoldableUnhighlight(IHoldable holdable)
    {
        while (playerHold.IsHolding || ReferenceEquals(holdable, Utils.RaycastForHoldable(transform, isHolding: playerHold.IsHolding, maxDistance: holdHoverDistance)))
        {
            yield return new WaitForEndOfFrame();
        }
        hand.SetActive(false);
        handBack.SetActive(false);
        holdHighlighting = false;
    }

}