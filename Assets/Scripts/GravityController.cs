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

        gravLinks[0] = new GravLink(firstStyle, Instantiate(linePrefab), 0);
        gravLinks[1] = new GravLink(secondStyle, Instantiate(linePrefab), 1);
        gravLinks[2] = new GravLink(thirdStyle, Instantiate(linePrefab), 2);
    }

    // ---------- Stored stuff ----------
    public int currentLinkNum = 0;
    public GravLink[] gravLinks = new GravLink[3];

    public event Action GravLinksChanged;

    public void StartCurrentLink(IGravitizable gravitizableA, Vector3 anchorPoint)
    {
        ClearCurrentLink();

        SelectionStyle style = gravLinks[currentLinkNum].style;

        gravLinks[currentLinkNum].gravitizables.Add(gravitizableA);

        gravitizableA.Select(style);
        gravitizableA.SetAnchor(anchorPoint, currentLinkNum);

        SelectionLine lineScript = gravLinks[currentLinkNum].line.GetComponent<SelectionLine>();
        lineScript.enabled = true;

        lineScript.a = gravitizableA;
        lineScript.head = head;

        GravLinksChanged?.Invoke();
    }

    public void EndCurrentLink(IGravitizable gravitizableB, Vector3 anchorPoint)
    {
        SelectionStyle style = gravLinks[currentLinkNum].style;

        gravLinks[currentLinkNum].gravitizables.Add(gravitizableB);
        
        gravitizableB.Select(style);
        gravitizableB.SetAnchor(anchorPoint, currentLinkNum);
        gravitizableB.Connect(gravLinks[currentLinkNum].gravitizables[0], currentLinkNum);

        SelectionLine lineScript = gravLinks[currentLinkNum].line.GetComponent<SelectionLine>();

        lineScript.b = gravitizableB;

        GravLinksChanged?.Invoke();
    }

    public void SetCurrentLinkMode(GravityMode newMode)
    {        
        if (!gravLinks[currentLinkNum].Active()) { return; }

        gravLinks[currentLinkNum].mode = newMode;

        GravLinksChanged?.Invoke();
    }

    public void ClearCurrentLink()
    {
        ClearLink(currentLinkNum);
    }

    public void ClearLink(int linkNum)
    {
        foreach (IGravitizable gravitizable in gravLinks[linkNum].gravitizables)
        {
            gravitizable?.Disconnect(linkNum);
        }

        gravLinks[linkNum].gravitizables.Clear();
        gravLinks[linkNum].mode = GravityMode.None;
        gravLinks[linkNum].line.GetComponent<SelectionLine>().enabled = false;

        GravLinksChanged?.Invoke();
    }

    public GravLink ChangeCurrentLink(int newLinkNum)
    {
        currentLinkNum = newLinkNum;

        GravLinksChanged?.Invoke();
        return gravLinks[currentLinkNum];
    }
}