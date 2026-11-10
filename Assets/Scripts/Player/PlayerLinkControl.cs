using System;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Turns player intent into commands on IGravitizable objects.
/// Holds only "which objects am I holding"; the objects own everything else.
/// </summary>
public class PlayerLinkControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInputHandler input;
    [SerializeField] Transform head;

    void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();
    }

    void OnDisable()
    {
        input.InteractPressed          -= StartLink;
        input.InteractReleased         -= EndLink;
        input.GravityControlPressed    -= OnGravityControl;
        input.GravityReleasePressed    -= OnGravityRelease;
        input.ChangeCurrentLinkPressed -= OnChangeCurrentLink;
    }

    // Press: start a new pair
    public void StartLink()
    {
        GravityController.Instance.ClearCurrentLink();

        IGravitizable gravitizableA = RaycastForGravitizable();
        if (gravitizableA == null) { return; }

        GravityController.Instance.StartCurrentLink(gravitizableA, RaycastPoint());
    }

    // Release: finish the pair, or cancel it
    public void EndLink()
    {
        GravityController gravityController = GravityController.Instance;
        GravLink currentLink = gravityController.gravLinks[gravityController.currentLinkNum];

        if (currentLink.gravitizables.Count == 0) return;

        IGravitizable gravitizableB = RaycastForGravitizable();
        if (gravitizableB == null || ReferenceEquals(gravitizableB, currentLink.gravitizables[0]))
        {
            GravityController.Instance.ClearCurrentLink();
            return;
        }

        GravityController.Instance.EndCurrentLink(gravitizableB, RaycastPoint());
    }

    // Scroll up = Push, scroll down = Pull. Ignored by the object if it isn't linked.
    public void OnGravityControl(float direction)
    {
        GravityController.Instance.SetCurrentLinkMode(direction > 0f ? GravityMode.Push : GravityMode.Pull);
    }

    public void OnGravityRelease()
    {
        GravityController.Instance.SetCurrentLinkMode(GravityMode.None);
    }

    public void OnChangeCurrentLink(int newLinkNum)
    {
        GravityController.Instance.ChangeCurrentLink(newLinkNum);
    }

    IGravitizable RaycastForGravitizable()
    {
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit)) return null;
        return hit.collider.GetComponentInParent<IGravitizable>();
    }

    Vector3 RaycastPoint()
    {
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit)) return Vector3.zero;

        Vector3 p = hit.point;
        Vector3 anchorPoint = new(
            RoundTo0625(p.x),
            RoundTo0625(p.y),
            RoundTo0625(p.z)
        );

        return anchorPoint;
    }

    static float RoundTo0625(float value) => Mathf.Round(value / 0.625f) * 0.625f;
}