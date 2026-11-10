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
    [SerializeField] Crosshair crosshair;

    InteractionModeHold hold;
    InteractionModeLink link;
    GunAnimations gunAnimations;

    bool isHolding = true;

    public event Action<bool> InteractionModeChanged;

    void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();

        hold = GetComponent<InteractionModeHold>();
        link = GetComponent<InteractionModeLink>();
        gunAnimations = GetComponentInChildren<GunAnimations>();
    }

    void OnEnable()
    {
        input.ModeSwitchPressed += OnModeSwitchPressed;
        ConnectEvents();
    }

    void OnDisable()
    {
        input.ModeSwitchPressed -= OnModeSwitchPressed;
    }

    void OnModeSwitchPressed()
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
        input.InteractPressed          += link.StartLink;
        input.InteractReleased         += link.EndLink;
        input.InteractPressed          -= hold.Grab;
        input.InteractReleased         -= hold.Drop;

        input.GravityControlPressed    += link.OnGravityControl;
        input.GravityReleasePressed    += link.OnGravityRelease;
        input.ChangeCurrentLinkPressed += link.OnChangeCurrentLink;

        InteractionModeChanged?.Invoke(false);
    }

    private void SetInteractToHolding()
    {
        input.InteractPressed          -= link.StartLink;
        input.InteractReleased         -= link.EndLink;
        input.InteractPressed          += hold.Grab;
        input.InteractReleased         += hold.Drop;

        input.GravityControlPressed    -= link.OnGravityControl;
        input.GravityReleasePressed    -= link.OnGravityRelease;
        input.ChangeCurrentLinkPressed -= link.OnChangeCurrentLink;
        
        InteractionModeChanged?.Invoke(true);
    }
}