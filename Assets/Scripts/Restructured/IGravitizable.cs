using UnityEngine;

public enum GravityMode { None, Pull, Push }
public enum SelectionSlot { None, First, Second }

/// <summary>
/// How a selected object should look. Owned by whoever does the selecting
/// (the player actions script), so object frames and crosshair UI stay in sync.
/// </summary>
[System.Serializable]
public struct SelectionStyle
{
    public Color color;
    public Vector3 frameScale;

    public SelectionStyle(Color color, Vector3 frameScale)
    {
        this.color = color;
        this.frameScale = frameScale;
    }
}

/// <summary>
/// Anything the player can select and link with gravity.
/// Callers only ever talk to this interface, never to a concrete class.
/// </summary>
public interface IGravitizable
{
    /// <summary>World position other objects are pulled toward / pushed from.</summary>
    Vector3 Position { get; }

    // --- Selection ---
    SelectionSlot Slot { get; }
    bool IsSelected { get; }
    void Select(SelectionSlot slot, SelectionStyle style);
    /// <summary>Also disconnects from any partner.</summary>
    void Deselect();

    // --- Link ---
    IGravitizable Partner { get; }
    /// <summary>Links both objects with Mode None. Calling it on either side is enough.</summary>
    void Connect(IGravitizable partner);
    /// <summary>Unlinks both objects and sets Mode to None. Calling it on either side is enough.</summary>
    void Disconnect();

    // --- Gravity ---
    /// <summary>Always shared with the partner. None whenever there is no partner.</summary>
    GravityMode Mode { get; }
    /// <summary>Sets the mode on both linked objects. Ignored if there is no partner.</summary>
    void SetMode(GravityMode mode);
    /// <summary>Back to None while staying linked.</summary>
    void Neutralize();
}