using System.Collections.Generic;
using UnityEngine;

public interface IHoldable
{
    bool IsHeld { get; set; }

    // Selection
    bool IsSelected { get; }
    void Select(SelectionStyle style);
    void CheckDeselect();
    void SetFrame(Color color, bool visible);
}