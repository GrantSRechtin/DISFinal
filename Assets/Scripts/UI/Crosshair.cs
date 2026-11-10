using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [SerializeField] PlayerInteractionMode interactionMode;

    [SerializeField] Sprite arrowIn;
    [SerializeField] Sprite arrowOut;
    [SerializeField] Sprite openHand;
    [SerializeField] Sprite closedHand;
    [SerializeField] Sprite line;
    [SerializeField] Color unselectedColor = Color.gray;
    [SerializeField] Color holdColor = Color.whiteSmoke;

    Image top;
    Image bottom;
    Image left;
    Image leftBack;
    Image right;
    Image rightBack;
    GravityController gravityController;

    bool isHolding = true;

    void Awake()
    {
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
        gravityController = GravityController.Instance;
        gravityController.GravLinksChanged     += UpdateAction;
        interactionMode.InteractionModeChanged += SetHolding;
        UpdateAction();
    }

    void OnDisable()
    {
        if (gravityController != null)
        {
            gravityController.GravLinksChanged -= UpdateAction;
            gravityController = null;
        }
        interactionMode.InteractionModeChanged -= SetHolding;
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

        GravLink currentLink = gravityController.gravLinks[gravityController.currentLinkNum];

        switch (currentLink.mode)
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

        int gravitizableCount = currentLink.gravitizables != null ? currentLink.gravitizables.Count: 0;

        Color firstColor = gravitizableCount >= 1 ? currentLink.style.color : unselectedColor;
        Color secondColor = gravitizableCount >= 2 ? currentLink.style.color : unselectedColor;

        // Holding crosshair
        if (isHolding) 
        {
            sprite = openHand;
            firstColor = holdColor;
            secondColor = holdColor;
        }

        UpdateSprite(sprite, firstColor, secondColor);
    }

    void UpdateSprite(Sprite sprite, Color firstColor, Color secondColor)
    {
        left.sprite = sprite;
        leftBack.sprite = sprite;
        right.sprite = sprite;
        rightBack.sprite = sprite;

        top.color = left.color = firstColor;
        bottom.color = right.color = secondColor;
    }

    void SetHolding(bool holding)
    {
        isHolding = holding;
        UpdateAction();
    }
}
