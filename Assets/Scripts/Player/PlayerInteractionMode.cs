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

    bool isHolding = true;

    public event Action<bool> InteractionModeChanged;

    void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();
    }

    void OnEnable()
    {
        input.ModeSwitchPressed += OnModeSwitchPressed;
        InteractionModeChanged?.Invoke(isHolding);
    }

    void OnDisable()
    {
        input.ModeSwitchPressed -= OnModeSwitchPressed;
    }

    void OnModeSwitchPressed()
    {
        isHolding = !isHolding;
        InteractionModeChanged?.Invoke(isHolding);
    }
}