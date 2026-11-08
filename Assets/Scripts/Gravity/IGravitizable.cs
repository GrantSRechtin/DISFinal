using System.Collections.Generic;
using UnityEngine;

public interface IGravitizable
{
    Vector3 Position { get; } // World position

    // Selection
    bool IsSelected { get; }
    void Select(SelectionStyle style);
    void CheckDeselect();
    void SetFrame(Color color, bool visible);

    // Link
    HashSet<int> LinkNums { get; }
    Dictionary<int, IGravitizable> Partners { get; }
    void Connect(IGravitizable partner, int linkNum);
    void Disconnect(int linkNum);

    // Anchor
    Dictionary<int, Vector3> Anchors { get; }
    void SetAnchor(Vector3 anchorPoint, int linkNum);
    Vector3 GetAnchor(int linkNum);

    // Gravity
    Dictionary<int, GravityMode> Modes { get; }
    void SetMode(GravityMode mode, int linkNum);
    void Neutralize(int linkNum);
}