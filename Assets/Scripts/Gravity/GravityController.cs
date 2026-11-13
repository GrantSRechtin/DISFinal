using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GravityController : RewindableBase
{
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

    // ------------- Checks -------------

    void FixedUpdate()
    {
        for (int i = 0; i < 3; i++)
        {
            CheckLinkLength(i);
        }
    }

    // ---------- Stored stuff ---------
    public float maxLinkDistance = 20f;
    public int currentLinkNum = 0;
    public GravLink[] gravLinks = new GravLink[3];

    public event Action GravLinksChanged;

    List<Timestep> timesteps = new();

    public override void OnTimeStarted()
    {
        base.OnTimeStarted();
        GravLinksChanged?.Invoke();
    }

    public override void OnStoreTimestep()
    {
        Timestep timestep = new()
        {
            CurrentLinkNum = currentLinkNum,
            GravLinks = new List<IGravitizable>[gravLinks.Length],
            GravModes = new GravityMode[gravLinks.Length]
        };

        for (int i = 0; i < gravLinks.Length; i++)
        {
            timestep.GravLinks[i] = new List<IGravitizable>(gravLinks[i].gravitizables);
            timestep.GravModes[i] = gravLinks[i].mode;
        }

        timesteps.Add(timestep);
    }

    public override void OnRewindTimestep(int steps)
    {
        if (timesteps.Count == 0) return;

        Timestep timestep;
        if (timesteps.Count > steps)
        {
            int lastIndex = timesteps.Count - steps;
            timestep = timesteps[lastIndex];

            while(timesteps.Count > lastIndex+1)
            {
                timesteps.RemoveAt(timesteps.Count-1);
            }
            timesteps.RemoveAt(lastIndex);
        }
        else
        {
            int lastIndex = 0;
            timestep = timesteps[lastIndex];
            timesteps.Clear();
        }

        currentLinkNum = timestep.CurrentLinkNum;
        for (int i = 0; i < gravLinks.Length; i++)
        {
            gravLinks[i].gravitizables.Clear();
            gravLinks[i].gravitizables.AddRange(timestep.GravLinks[i]);
            gravLinks[i].mode = timestep.GravModes[i];

            SelectionLine line = gravLinks[i].line.GetComponent<SelectionLine>();
            line.a = timestep.GravLinks[i].Count > 0 ? timestep.GravLinks[i][0] : null;
            line.b = timestep.GravLinks[i].Count > 1 ? timestep.GravLinks[i][1] : null;
            line.head = head;
            line.GetComponent<LineRenderer>().enabled = timestep.GravLinks[i].Count > 0;
        }

        GravLinksChanged?.Invoke();
    }

    public void StartCurrentLink(IGravitizable gravitizableA, Vector3 anchorPoint)
    {
        ClearCurrentLink();

        SelectionStyle style = gravLinks[currentLinkNum].style;

        gravLinks[currentLinkNum].gravitizables.Add(gravitizableA);

        gravitizableA.Select(style);
        gravitizableA.SetAnchor(anchorPoint, currentLinkNum);

        SelectionLine lineScript = gravLinks[currentLinkNum].line.GetComponent<SelectionLine>();
        lineScript.a = gravitizableA;
        lineScript.head = head;
        lineScript.GetComponent<LineRenderer>().enabled = true;

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
        SelectionLine lineScript = gravLinks[linkNum].line.GetComponent<SelectionLine>();
        lineScript.a = null;
        lineScript.b = null;
        lineScript.GetComponent<LineRenderer>().enabled = false;

        GravLinksChanged?.Invoke();
    }

    public GravLink ChangeCurrentLink(int newLinkNum)
    {
        currentLinkNum = newLinkNum;

        GravLinksChanged?.Invoke();
        return gravLinks[currentLinkNum];
    }

    public void CheckLinkLength(int linkNum)
    {
        if (gravLinks[linkNum].Length() > maxLinkDistance)
        {
            ClearLink(linkNum);
        }
    }
}