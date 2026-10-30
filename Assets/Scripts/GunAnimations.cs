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
    [SerializeField] GameObject[] cartridges;

    PlayerInputHandler input;
    GravityController gravityController;

    void Awake()
    {
        input = GetComponentInParent<PlayerInputHandler>();
    }

    void Start()
    {
        OnChangeCurrentLink(0);
    }

    void OnEnable()
    {
        input.ChangeCurrentLinkPerformed            += OnChangeCurrentLink;
        gravityController = GravityController.Instance;
        gravityController.GravLinksChanged += OnGravLinksChanged;
    }

    void OnDisable()
    {
        input.ChangeCurrentLinkPerformed            -= OnChangeCurrentLink;
        if (gravityController != null)
        {
            gravityController.GravLinksChanged -= OnGravLinksChanged;
            gravityController = null;
        }
    }

    void OnChangeCurrentLink(int newLinkNum)
    {
        foreach (GameObject cartridge in cartridges)
        {
            SetFrame(cartridge, Color.gray2, true);
        }

        SetFrame(cartridges[newLinkNum], Color.white, true);
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

    void OnGravLinksChanged()
    {
        for(int i = 0; i < 3; i++)
        {
            bool active = gravityController.gravLinks[i].Active();
            Color linkColor = gravityController.gravLinks[i].style.color;
            Color newColor = active ? linkColor : Color.white;
            UpdateCartridge(i, newColor);
        }
    }

    void UpdateCartridge(int cartridge, Color color)
    {
        cartridges[cartridge].GetComponent<Renderer>().material.color = color;
    }
}