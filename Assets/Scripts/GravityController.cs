using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// A persistent singleton that stores game data across scene loads.
/// Drop it on an empty GameObject in your first scene, or just call
/// GameData.Instance from anywhere and it will create itself.
/// </summary>
public class GravityController : MonoBehaviour
{
    // ---------- Singleton setup ----------
    private static GravityController _instance;

    public static GravityController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GravityController>();

                if (_instance == null)
                {
                    var go = new GameObject("GravityController");
                    _instance = go.AddComponent<GravityController>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        InitialLinkSetup();
    }

    // ---------- Setup ----------
    [SerializeField] SelectionStyle firstStyle  = new(new Color(0f, 1f, 1f), Vector3.one * 1.01f);
    [SerializeField] SelectionStyle secondStyle = new(new Color(1f, 0f, 1f), Vector3.one * 1.011f);
    [SerializeField] SelectionStyle thirdStyle = new(new Color(1f, 1f, 0f), Vector3.one * 1.012f);

    [SerializeField] GameObject linePrefab;
    Transform head;

    private void InitialLinkSetup()
    {
        head = GameObject.FindGameObjectWithTag("Head").GetComponent<Transform>();

        gravLinks[0] = new GravLink(firstStyle, Instantiate(linePrefab));
        gravLinks[1] = new GravLink(secondStyle, Instantiate(linePrefab));
        gravLinks[2] = new GravLink(thirdStyle, Instantiate(linePrefab));
    }

    // ---------- Stored stuff ----------
    public int currentLinkNum = 0;
    public GravLink[] gravLinks = new GravLink[3];

    public event Action GravLinksChanged;

    public void StartCurrentLink(IGravitizable gravitizableA)
    {
        SelectionSlot slot = SelectionSlot.A;
        SelectionStyle style = gravLinks[currentLinkNum].style;

        if (gravitizableA.IsSelected)
        {
            // Find which slot it's currently in and clear that slot
            // I want for gravitizables to have a reference to their link, whether that be the slot number or actual GravLink
            ClearCurrentLink();
            return;
        }

        gravitizableA.Select(slot, style);
        gravLinks[currentLinkNum].a = gravitizableA;

        SelectionLine lineScript = gravLinks[currentLinkNum].line.GetComponent<SelectionLine>();
        lineScript.enabled = true;

        lineScript.a = gravitizableA;
        lineScript.head = head;

        GravLinksChanged?.Invoke();
    }

    public void EndCurrentLink(IGravitizable gravitizableB)
    {
        SelectionSlot slot = SelectionSlot.B;
        SelectionStyle style = gravLinks[currentLinkNum].style;

        if (gravitizableB.IsSelected)
        {
            // Find which slot it's currently in and clear that slot
            if (gravitizableB.LinkNum != -1)
            {
                ClearLink(gravitizableB.LinkNum);
            }
            // return;
        }

        gravitizableB.Select(slot, style);
        gravLinks[currentLinkNum].b = gravitizableB;

        gravLinks[currentLinkNum].b.Connect(gravLinks[currentLinkNum].a, currentLinkNum);

        SelectionLine lineScript = gravLinks[currentLinkNum].line.GetComponent<SelectionLine>();

        lineScript.b = gravitizableB;

        GravLinksChanged?.Invoke();
    }

    public void SetCurrentLinkMode(GravityMode newMode)
    {
        GravLink currentLink = gravLinks[currentLinkNum];
        
        if (!currentLink.Active()) { return; }

        gravLinks[currentLinkNum].mode = newMode;
        gravLinks[currentLinkNum].a.SetMode(newMode);
        gravLinks[currentLinkNum].b.SetMode(newMode);

        GravLinksChanged?.Invoke();
    }

    public void ClearCurrentLink()
    {
        ClearLink(currentLinkNum);
    }

    public void ClearLink(int linkNum)
    {
        gravLinks[linkNum].a?.Deselect();
        gravLinks[linkNum].b?.Deselect();

        gravLinks[linkNum].a = null;
        gravLinks[linkNum].b = null;

        gravLinks[linkNum].mode = GravityMode.None;

        GameObject line = gravLinks[linkNum].line;
        line.GetComponent<SelectionLine>().enabled = false;

        GravLinksChanged?.Invoke();
    }

    public GravLink ChangeCurrentLink(int newLinkNum)
    {
        currentLinkNum = newLinkNum;

        GravLinksChanged?.Invoke();
        return gravLinks[currentLinkNum];
    }
}