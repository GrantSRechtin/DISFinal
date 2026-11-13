using System;
using UnityEngine;

/// <summary>
/// Turns player intent into commands on IGravitizable objects.
/// Holds only "which objects am I holding"; the objects own everything else.
/// </summary>
public class PlayerActions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInputHandler input;
    [SerializeField] Transform head;

    PlayerHold playerHold;

    void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();
        playerHold = GetComponent<PlayerHold>();
    }

    void OnEnable()
    {
        input.SelectPressed              += OnSelectPressed;
        input.SelectReleased             += OnSelectReleased;
        input.GravityControlPerformed    += OnGravityControl;
        input.GravityReleasePerformed    += OnGravityRelease;
        input.ChangeCurrentLinkPerformed += OnChangeCurrentLink;
    }

    void OnDisable()
    {
        input.SelectPressed              -= OnSelectPressed;
        input.SelectReleased             -= OnSelectReleased;
        input.GravityControlPerformed    -= OnGravityControl;
        input.GravityReleasePerformed    -= OnGravityRelease;
        input.ChangeCurrentLinkPerformed -= OnChangeCurrentLink;
    }

    // Press: start a new pair
    void OnSelectPressed()
    {
        GravityController.Instance.ClearCurrentLink();

        IGravitizable gravitizableA = Utils.RaycastForGravitizable(head, isHolding: playerHold.IsHolding);
        if (gravitizableA == null) { return; }

        GravityController.Instance.StartCurrentLink(gravitizableA, RaycastPoint());
    }

    // Release: finish the pair, or cancel it
    void OnSelectReleased()
    {
        GravityController gravityController = GravityController.Instance;
        GravLink currentLink = gravityController.gravLinks[gravityController.currentLinkNum];

        if (currentLink.gravitizables.Count == 0) return;

        IGravitizable gravitizableB = Utils.RaycastForGravitizable(head, isHolding: playerHold.IsHolding);
        if (gravitizableB == null || ReferenceEquals(gravitizableB, currentLink.gravitizables[0]))
        {
            GravityController.Instance.ClearCurrentLink();
            return;
        }

        GravityController.Instance.EndCurrentLink(gravitizableB, RaycastPoint());
    }

    // Scroll up = Push, scroll down = Pull. Ignored by the object if it isn't linked.
    void OnGravityControl(float direction)
    {
        GravityController.Instance.SetCurrentLinkMode(direction > 0f ? GravityMode.Push : GravityMode.Pull);
    }

    void OnGravityRelease()
    {
        GravityController.Instance.SetCurrentLinkMode(GravityMode.None);
    }

    void OnChangeCurrentLink(int newLinkNum)
    {
        GravityController.Instance.ChangeCurrentLink(newLinkNum);
    }

    Vector3 RaycastPoint()
    {
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit)) return Vector3.zero;

        Vector3 p = hit.point;
        Vector3 anchorPoint = new(
            Utils.RoundTo0625(p.x),
            Utils.RoundTo0625(p.y),
            Utils.RoundTo0625(p.z)
        );

        return anchorPoint;
    }
}