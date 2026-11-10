using System.Collections;
using UnityEngine;

public class MouseHover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;

    float hoverDistance = 5f;
    bool highlighting = false;

    private void Update()
    {
        if(!highlighting)
        {
            IGravitizable gravitizable = RaycastForGravitizable();
            if (gravitizable != null && !gravitizable.IsSelected)
            {
                gravitizable.SetFrame(Color.white, true);
                highlighting = true;
                StartCoroutine(Unhighlight(gravitizable));
            }
        }
    }

    IEnumerator Unhighlight(IGravitizable gravitizable)
    {
        while (ReferenceEquals(gravitizable, RaycastForGravitizable()) && !gravitizable.IsSelected)
        {
            yield return new WaitForEndOfFrame();
        }
        if (!gravitizable.IsSelected)
        {
            gravitizable.SetFrame(Color.white, false);
        }
        highlighting = false;
    }

    IGravitizable RaycastForGravitizable()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, hoverDistance)) return null;
        return hit.collider.GetComponentInParent<IGravitizable>();
    }

    public void SetHoverDistance(float distance)
    {
        hoverDistance = distance;
    }
}