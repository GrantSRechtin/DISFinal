using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [SerializeField] PlayerActions actions;
    [SerializeField] Sprite arrowIn;
    [SerializeField] Sprite arrowOut;
    [SerializeField] Sprite line;
    [SerializeField] Color unselectedColor = Color.gray;

    Image top;
    Image bottom;
    Image left;
    Image leftBack;
    Image right;
    Image rightBack;

    void Awake()
    {
        if (actions == null) actions = FindFirstObjectByType<PlayerActions>();

        if (actions == null)
        {
            Debug.LogError("Crosshair requires an active PlayerActions component.", this);
            enabled = false;
            return;
        }

        top = FindImage("Top");
        bottom = FindImage("Bottom");
        left = FindImage("Left");
        leftBack = FindImage("LeftBack");
        right = FindImage("Right");
        rightBack = FindImage("RightBack");

        if (top == null || bottom == null || left == null || leftBack == null || right == null || rightBack == null)
        {
            enabled = false;
            return;
        }

        if (arrowIn == null || arrowOut == null || line == null)
        {
            Debug.LogError("Crosshair requires Pull, Push, and neutral sprites.", this);
            enabled = false;
        }
    }

    void OnEnable()
    {
        if (actions == null) return;

        actions.StateChanged += UpdateAction;
        UpdateAction();
    }

    void OnDisable()
    {
        if (actions != null) actions.StateChanged -= UpdateAction;
    }

    Image FindImage(string imageName)
    {
        foreach (Image image in GetComponentsInChildren<Image>(true))
        {
            if (image.name == imageName) return image;
        }

        Debug.LogError($"Crosshair is missing an Image named '{imageName}'.", this);
        return null;
    }

    void UpdateAction()
    {
        Sprite sprite;

        switch (actions.CurrentMode)
        {
            case GravityMode.Pull:
                sprite = arrowIn;
                break;
            case GravityMode.Push:
                sprite = arrowOut;
                break;
            default:
                sprite = line;
                break;
        }

        left.sprite = sprite;
        leftBack.sprite = sprite;
        right.sprite = sprite;
        rightBack.sprite = sprite;

        Color firstColor = actions.HasFirstSelection ? actions.FirstSelectionColor : unselectedColor;
        Color secondColor = actions.HasSecondSelection ? actions.SecondSelectionColor : unselectedColor;

        top.color = left.color = firstColor;
        bottom.color = right.color = secondColor;
    }
}
