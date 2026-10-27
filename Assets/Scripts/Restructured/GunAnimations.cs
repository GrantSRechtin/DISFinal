using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Turns player intent into commands on IGravitizable objects.
/// Holds only "which objects am I holding"; the objects own everything else.
/// </summary>
public class GunAnimations : MonoBehaviour
{
    [Header("References")]
    // [SerializeField] PlayerInputHandler input;
    // [SerializeField] PlayerActions actions;
    [SerializeField] GameObject[] cartridges;

    [Header("Selection Styles")]
    [SerializeField] SelectionStyle firstStyle  = new(new Color(0f, 1f, 1f), Vector3.one * 1.011f);
    [SerializeField] SelectionStyle secondStyle = new(new Color(1f, 0f, 1f), Vector3.one * 1.01f);

    PlayerInputHandler input;
    PlayerActions actions;

    void Awake()
    {
        input = GetComponentInParent<PlayerInputHandler>();
        actions = GetComponentInParent<PlayerActions>();
    }

    void OnEnable()
    {
        input.CartridgeChangePerformed += OnCartridgeChange;
        actions.StateChanged           += OnStateChange;
    }

    void OnDisable()
    {
        input.CartridgeChangePerformed -= OnCartridgeChange;
        actions.StateChanged           -= OnStateChange;
    }

    void OnCartridgeChange(int newCartridge)
    {
        foreach (GameObject cartridge in cartridges)
        {
            SetFrame(cartridge, Color.white, false);
        }

        SetFrame(cartridges[newCartridge-1], Color.white, true);
    }

    void SetFrame(GameObject cartridge, Color color, bool visible)
    {
        HashSet<Renderer> renderers = cartridge.GetComponentsInChildren<Renderer>().ToHashSet();

        Renderer baseRenderer = cartridge.GetComponent<Renderer>();

        renderers.Remove(baseRenderer);
        
        foreach (Renderer r in renderers)
        {
            r.material.color = color;
            r.material.SetColor("_EmissionColor", color * 1.5f);
            r.enabled = visible;
        }
    }

    void OnStateChange()
    {
        for(int i = 1; i < 4; i++)
        {
            Color color = actions.CartridgeState(i) ? firstStyle.color : Color.gray7;
            UpdateCartridge(i-1, color);
        }
    }

    void UpdateCartridge(int cartridge, Color color)
    {
        cartridges[cartridge].GetComponent<Renderer>().material.color = color;
    }
}