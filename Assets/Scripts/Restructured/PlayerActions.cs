using System;
using Unity.Mathematics;
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

    void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();
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

        IGravitizable gravitizableA = RaycastForGravitizable();
        if (gravitizableA == null) { return; }

        GravityController.Instance.StartCurrentLink(gravitizableA);
    }

    // Release: finish the pair, or cancel it
    void OnSelectReleased()
    {
        GravityController gravityController = GravityController.Instance;
        GravLink currentLink = gravityController.gravLinks[gravityController.currentLinkNum];

        if (currentLink.a == null) return;

        IGravitizable gravitizableB = RaycastForGravitizable();
        if (gravitizableB == null || ReferenceEquals(gravitizableB, currentLink.a))
        {
            GravityController.Instance.ClearCurrentLink();
            return;
        }

        GravityController.Instance.EndCurrentLink(gravitizableB);
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

    IGravitizable RaycastForGravitizable()
    {
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit)) return null;
        return hit.collider.GetComponentInParent<IGravitizable>();
    }
}