using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Gravitizable : MonoBehaviour
{
    [SerializeField] Transform head;
    [SerializeField] float gravitationalForce = 40;
    [SerializeField] float distanceScaling = 0.5f;
    [SerializeField] float minDist = 10f;
    [SerializeField] float pushMultiplier = -0.625f;

    [SerializeField] Image top;
    [SerializeField] Image bottom;
    
    [SerializeField] Image left;
    [SerializeField] Image leftBack;

    [SerializeField] Image right;
    [SerializeField] Image rightBack;

    [SerializeField] Sprite arrowIn;
    [SerializeField] Sprite arrowOut;
    [SerializeField] Sprite line;

    private class SelectionSlot
    {
        public GameObject targetObject;
        public Color color;
        public Vector3 frameScale;
        public Image actionImage;
        public Image borderImage;
    }

    SelectionSlot firstSelection = new();
    SelectionSlot secondSelection = new();

    Color uiDefaultColor = Color.gray8;

    bool pulling = false;
    bool pushing = false;

    void Awake()
    {
        firstSelection.color = new Color(0f,1f,1f,1);
        firstSelection.frameScale = new(1.011f,1.011f,1.011f);
        firstSelection.borderImage = top;
        firstSelection.actionImage = left;

        secondSelection.color = new Color(1f,0f,1f,1);
        secondSelection.frameScale = new(1.01f,1.01f,1.01f);
        secondSelection.borderImage = bottom;
        secondSelection.actionImage = right;

        top.color = uiDefaultColor;
        bottom.color = uiDefaultColor;
        left.color = uiDefaultColor;
        right.color = uiDefaultColor;

        ActionChange(line);
    }

    void OnLeftSelect()
    {
        Deselect(firstSelection);

        Ray ray = new Ray(head.position, head.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            GameObject obj = hit.collider.gameObject;

            if (obj.CompareTag("Gravitizable") && obj != secondSelection.targetObject)
            {
                firstSelection.targetObject = obj;
                Select(firstSelection);
            }
        }
    }

    void OnRightSelect()
    {
        Deselect(secondSelection);

        Ray ray = new Ray(head.position, head.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            GameObject obj = hit.collider.gameObject;

            if (obj.CompareTag("Gravitizable") && obj != firstSelection.targetObject)
            {
                secondSelection.targetObject = obj;
                Select(secondSelection);
            }        
        }
    }

    void OnGravitize()
    {
        bool objects = firstSelection.targetObject && secondSelection.targetObject;
        bool active = pulling || pushing;

        pulling = objects && !active ? true : false;
        pushing = false;
    }

    void OnRepell()
    {
        bool objects = firstSelection.targetObject && secondSelection.targetObject;
        bool active = pulling || pushing;

        pushing = objects && !active ? true : false;
        pulling = false;
    }

    void FixedUpdate()
    {
        GameObject first = firstSelection.targetObject;
        GameObject second = secondSelection.targetObject;

        if (!first || !second)
        {
            pulling = false;
            pushing = false;
        }

        if (pulling || pushing)
        {
            DirectionalForce(first, second);
            DirectionalForce(second, first);
        }

        CheckAction();
    }

    void DirectionalForce(GameObject victim, GameObject target)
    {
        Vector3 direction = target.transform.position - victim.transform.position;

        float distance = Mathf.Max(direction.magnitude, minDist);
        float powerRatio = 1/Mathf.Pow(distance-minDist+1, distanceScaling);

        Vector3 force = direction.normalized * gravitationalForce * powerRatio;
        force = (force.magnitude < 5) ? Vector3.zero : force;

        if (pushing)
        {
            force *= pushMultiplier;
        }

        victim.GetComponent<Rigidbody>().AddForce(force, ForceMode.Force);
    }

    void Select(SelectionSlot selection)
    {
        GameObject frame = GetFrameObject(selection.targetObject);
        UpdateFrame(frame, selection.color, true);

        frame.transform.localScale = selection.frameScale;
        selection.actionImage.color = selection.color;
        selection.borderImage.color = selection.color;
    }

    void Deselect(SelectionSlot selection)
    {
        if (!selection.targetObject) return;

        GameObject frame = GetFrameObject(selection.targetObject);
        UpdateFrame(frame, Color.white, false);

        selection.actionImage.color = uiDefaultColor;
        selection.borderImage.color = uiDefaultColor;
        selection.targetObject = null;
    }

    GameObject GetFrameObject(GameObject parentObject)
    {
        foreach (Transform child in parentObject.GetComponentsInChildren<Transform>(true))
        {
            if (child != parentObject.transform && child.CompareTag("Frame"))
                return child.gameObject;
        }

        return null;
    }

    void UpdateFrame(GameObject frame, Color color, bool enabled)
    {
        foreach (Renderer renderer in frame.GetComponentsInChildren<Renderer>(true))
        {
            renderer.material.color = color;
            renderer.material.SetColor("_EmissionColor", color * 1.5f);
            
            renderer.enabled = enabled;
        }
    }

    // Crosshair Changes
    void CheckAction()
    {
        if (pulling)
        {
            ActionChange(arrowIn);
        }
        else if (pushing)
        {
            ActionChange(arrowOut);
        }
        else
        {
            ActionChange(line);
        }
    }

    void ActionChange(Sprite newSprite)
    {
        left.sprite = newSprite;
        leftBack.sprite = newSprite;

        right.sprite = newSprite;
        rightBack.sprite = newSprite;
    }
}
