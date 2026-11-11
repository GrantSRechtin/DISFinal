using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MouseHover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;

    readonly float linkHoverDistance = 20f;
    readonly float holdHoverDistance = 5f;
    bool linkHighlighting = false;
    bool holdHighlighting = false;

    private void Update()
    {
        if(!holdHighlighting)
        {
            // Hold Highlight
            IHoldable holdable = RaycastForHoldable();
            if (holdable != null && !holdable.IsSelected)
            {
                holdable.SetFrame(Color.black, true);
                holdHighlighting = true;
                // linkHighlighting = false;
                StartCoroutine(HoldableUnhighlight(holdable));
            }
        }
        if(!holdHighlighting && !linkHighlighting)
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
        while (
            ReferenceEquals(gravitizable, RaycastForGravitizable()) 
            && !gravitizable.IsSelected 
        )
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
        if (!holdable.IsSelected && linkHighlighting)
        {
            holdable.SetFrame(Color.white, true);
        }
        else if (!holdable.IsSelected)
        {
            holdable.SetFrame(Color.white, false);
        }
        holdHighlighting = false;
    }

    IHoldable RaycastForHoldable()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, holdHoverDistance)) return null;
        return hit.collider.GetComponentInParent<IHoldable>();
    }
}