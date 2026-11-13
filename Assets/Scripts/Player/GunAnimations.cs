using System;
using System.Collections;
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
    Transform[] cartridgeBodies;
    Vector3[] bodyBoundsMin;
    Vector3[] bodyInitialScales;
    Vector3[] bodyInitialPositions;
    int[] bodyLengthAxes;

    void Awake()
    {
        input = GetComponentInParent<PlayerInputHandler>();
    }

    void Start()
    {
        CacheCartridgeBodies();
        OnChangeCurrentLink(0);
    }

    void OnEnable()
    {
        input.ChangeCurrentLinkPerformed   += OnChangeCurrentLink;

        gravityController = GravityController.Instance;
        gravityController.GravLinksChanged += OnGravLinksChanged;
    }

    void OnDisable()
    {
        input.ChangeCurrentLinkPerformed -= OnChangeCurrentLink;

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
        GameObject frame = Utils.FindChild(cartridge.transform, "Frame");
        HashSet<Renderer> renderers = frame.GetComponentsInChildren<Renderer>().ToHashSet();        
        foreach (Renderer r in renderers)
        {
            r.material.color = color;
            r.material.SetColor("_EmissionColor", color * 1.5f);
            r.enabled = visible;
        }
    }

    void OnGravLinksChanged()
    {
        StartCoroutine(ChangeAfterUpdate());
    }

    IEnumerator ChangeAfterUpdate()
    {
        yield return new WaitForEndOfFrame();
        for(int i = 0; i < 3; i++)
        {
            bool active = gravityController.gravLinks[i].Active();
            Color linkColor = gravityController.gravLinks[i].style.color;
            Color newColor = active ? linkColor : Color.white;
            UpdateCartridgeColor(i, newColor);
        }
    }

    void UpdateCartridgeColor(int linkNum, Color color)
    {
        GameObject cartridge = cartridges[linkNum];
        GameObject body = Utils.FindChild(cartridge.transform, "Body");

        body.GetComponent<Renderer>().material.color = color;
    }

    void FixedUpdate()
    {
        UpdateCartridgeSize(0);
        UpdateCartridgeSize(1);
        UpdateCartridgeSize(2);
    }

    void UpdateCartridgeSize(int linkNum)
    {
        Transform body = cartridgeBodies[linkNum];
        if (body == null) return;

        float linkLength = gravityController.gravLinks[linkNum].Length();
        float sizeRatio = Mathf.Clamp01(1 - (linkLength / gravityController.maxLinkDistance));

        int lengthAxis = bodyLengthAxes[linkNum];
        Vector3 scale = bodyInitialScales[linkNum];
        scale[lengthAxis] *= sizeRatio;
        body.localScale = scale;

        Vector3 axis = Vector3.zero;
        axis[lengthAxis] = 1;
        Vector3 parentAxis = body.localRotation * axis;
        float minExtent = bodyBoundsMin[linkNum][lengthAxis] * bodyInitialScales[linkNum][lengthAxis];
        body.localPosition = bodyInitialPositions[linkNum] + parentAxis * ((1 - sizeRatio) * minExtent);
    }

    void CacheCartridgeBodies()
    {
        int count = cartridges.Length;
        cartridgeBodies = new Transform[count];
        bodyBoundsMin = new Vector3[count];
        bodyInitialScales = new Vector3[count];
        bodyInitialPositions = new Vector3[count];
        bodyLengthAxes = new int[count];

        for (int i = 0; i < count; i++)
        {
            GameObject body = Utils.FindChild(cartridges[i].transform, "Body");
            MeshFilter meshFilter = body != null ? body.GetComponent<MeshFilter>() : null;
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogError($"Cartridge {i} is missing a Body mesh.", cartridges[i]);
                continue;
            }

            Bounds bounds = meshFilter.sharedMesh.bounds;
            Vector3 size = bounds.size;
            int lengthAxis = size.x >= size.y && size.x >= size.z ? 0 : size.y >= size.z ? 1 : 2;

            cartridgeBodies[i] = body.transform;
            bodyBoundsMin[i] = bounds.min;
            bodyInitialScales[i] = body.transform.localScale;
            bodyInitialPositions[i] = body.transform.localPosition;
            bodyLengthAxes[i] = lengthAxis;
        }
    }
}