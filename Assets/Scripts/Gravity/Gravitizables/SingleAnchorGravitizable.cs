using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// A physics object that can be selected, linked to a partner, and pulled toward
/// or pushed from it. Owns its own frame and forces.
/// </summary>
public class SingleAnchorGravitizable : GravitizableBase
{
    [SerializeField] Transform anchorPosition;
    public override Vector3 GetAnchor(int _)
    {
        if (anchorPosition) return anchorPosition.position;
        return transform.position;
    }
}