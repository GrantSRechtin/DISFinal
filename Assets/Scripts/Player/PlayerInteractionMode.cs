using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Turns player intent into commands on IGravitizable objects.
/// Holds only "which objects am I holding"; the objects own everything else.
/// </summary>
public class PlayerInteractionMode : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInputHandler input;

    PlayerGrab playerGrab;
    PlayerLinkControl playerLinkControl;
    GunAnimations gunAnimations;

    bool isHolding = true;

    void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();

        playerGrab = GetComponent<PlayerGrab>();
        playerLinkControl = GetComponent<PlayerLinkControl>();
        gunAnimations = GetComponentInChildren<GunAnimations>();
    }

    void OnEnable()
    {
        input.SelectPressed += OnSelectPressed;
        ConnectEvents();
    }

    void OnDisable()
    {
        input.SelectPressed -= OnSelectPressed;
    }

    void OnSelectPressed()
    {
        isHolding = !isHolding;
        
        ConnectEvents();
    }

    void ConnectEvents()
    {
        if (isHolding) SetInteractToHolding();
        else SetInteractToLinking();
    }

    private void SetInteractToLinking()
    {
        input.InteractPressed          += playerLinkControl.StartLink;
        input.InteractReleased         += playerLinkControl.EndLink;
        input.InteractPressed          -= playerGrab.Grab;
        input.InteractReleased         -= playerGrab.Drop;

        input.GravityControlPressed    += playerLinkControl.OnGravityControl;
        input.GravityReleasePressed    += playerLinkControl.OnGravityRelease;
        input.ChangeCurrentLinkPressed += playerLinkControl.OnChangeCurrentLink;

        input.ChangeCurrentLinkPressed += gunAnimations.OnChangeCurrentLink;

        // Highlight selected slot
        gunAnimations.SetHolding(false);
    }

    private void SetInteractToHolding()
    {
        input.InteractPressed          -= playerLinkControl.StartLink;
        input.InteractReleased         -= playerLinkControl.EndLink;
        input.InteractPressed          += playerGrab.Grab;
        input.InteractReleased         += playerGrab.Drop;

        input.GravityControlPressed    -= playerLinkControl.OnGravityControl;
        input.GravityReleasePressed    -= playerLinkControl.OnGravityRelease;
        input.ChangeCurrentLinkPressed -= playerLinkControl.OnChangeCurrentLink;

        input.ChangeCurrentLinkPressed -= gunAnimations.OnChangeCurrentLink;
        
        // Highlight cap
        gunAnimations.SetHolding(true);
    }
}