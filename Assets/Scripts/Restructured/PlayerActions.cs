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

    [Header("Prefab")]
    [SerializeField] GameObject linePrefab;

    [Header("Selection Styles")]
    [SerializeField] SelectionStyle firstStyle  = new(new Color(0f, 1f, 1f), Vector3.one * 1.011f);
    [SerializeField] SelectionStyle secondStyle = new(new Color(1f, 0f, 1f), Vector3.one * 1.01f);

    IGravitizable first;
    IGravitizable second;

    GameObject[] lines = new GameObject[30];
    IGravitizable[] gravitizables = new IGravitizable[6];

    public event Action StateChanged;
    public GravityMode CurrentMode => first != null ? first.Mode : GravityMode.None;
    public bool HasFirstSelection => first != null;
    public bool HasSecondSelection => second != null;
    public Color FirstSelectionColor => firstStyle.color;
    public Color SecondSelectionColor => secondStyle.color;

    float catridge = 1;

    void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();
    }

    void OnEnable()
    {
        input.SelectPressed            += OnSelectPressed;
        input.SelectReleased           += OnSelectReleased;
        input.GravityControlPerformed  += OnGravityControl;
        input.GravityReleasePerformed  += OnGravityRelease;
        input.CartridgeChangePerformed += OnCartridgeChange;
    }

    void OnDisable()
    {
        input.SelectPressed            -= OnSelectPressed;
        input.SelectReleased           -= OnSelectReleased;
        input.GravityControlPerformed  -= OnGravityControl;
        input.GravityReleasePerformed  -= OnGravityRelease;
        input.CartridgeChangePerformed -= OnCartridgeChange;
    }

    // Press: start a new pair
    void OnSelectPressed()
    {
        ClearSelection();

        first = RaycastForGravitizable();
        if (first == null) { return; }

        first.Select(SelectionSlot.First, firstStyle);
        gravitizables[(int) (catridge-1)*2] = first;
        StateChanged?.Invoke();

        int j = (int) (catridge-1) * 10;
        for(int i = j; i < j+10; i++)
        {
            lines[i] = Instantiate(linePrefab, transform.position, quaternion.identity);
            SelectionLine lineScript = lines[i].GetComponent<SelectionLine>();
            lineScript.first = first;
            lineScript.head = head;
        }
    }

    // Release: finish the pair, or cancel it
    void OnSelectReleased()
    {
        if (first == null) return;

        IGravitizable target = RaycastForGravitizable();
        if (target == null || ReferenceEquals(target, first))
        {
            ClearSelection();
            return;
        }

        second = target;
        second.Select(SelectionSlot.Second, secondStyle);
        gravitizables[(int) (catridge-1)*2+1] = second;

        first.Connect(second);

        int j = (int) (catridge-1) * 10;
        for(int i = j; i < j+10; i++)
        {
            SelectionLine lineScript = lines[i].GetComponent<SelectionLine>();
            lineScript.second = second;
        }

        StateChanged?.Invoke();
    }

    // Scroll up = Push, scroll down = Pull. Ignored by the object if it isn't linked.
    void OnGravityControl(float direction)
    {
        first?.SetMode(direction > 0f ? GravityMode.Push : GravityMode.Pull);
        StateChanged?.Invoke();
    }

    void OnGravityRelease()
    {
        first?.Neutralize();
        StateChanged?.Invoke();
    }

    void OnCartridgeChange(int newCartridge)
    {
        catridge = newCartridge;

        first = gravitizables[(int) (catridge-1)*2];
        second = gravitizables[(int) (catridge-1)*2+1];
        StateChanged?.Invoke();
    }

    void ClearSelection()
    {
        first?.Deselect();
        second?.Deselect();
        first = null;
        second = null;
        gravitizables[(int) (catridge-1)*2] = null;
        gravitizables[(int) (catridge-1)*2+1] = null;

        int j = (int) (catridge-1) * 10;
        for(int i = j; i < j+10; i++)
        {
            Destroy(lines[i]);
        }

        StateChanged?.Invoke();
    }

    IGravitizable RaycastForGravitizable()
    {
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit)) return null;
        return hit.collider.GetComponentInParent<IGravitizable>();
    }

    public bool CartridgeState(int index)
    {
        bool firstUsed = gravitizables[(index-1)*2] != null;
        bool secondUsed = gravitizables[(index-1)*2+1] != null;

        return firstUsed && secondUsed;
    }
}