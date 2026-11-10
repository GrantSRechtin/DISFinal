using System.Collections;
using UnityEngine;

/// <summary>
/// Orbit-style camera rotation. Reads look intent from PlayerInputHandler,
/// never touches the Input System directly.
/// </summary>
public class MouseHover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;

    bool highlighting = false;

    private void Update()
    {
        Vector2 lookInput = input.Look;

        if(!highlighting)
        {
            IGravitizable gravitizable = RaycastForGravitizable();
            if (gravitizable != null && !gravitizable.IsSelected)
            {
                Debug.Log("hover");
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
        Debug.Log("unhover");
        highlighting = false;
    }

    IGravitizable RaycastForGravitizable()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit)) return null;
        return hit.collider.GetComponentInParent<IGravitizable>();
    }
}