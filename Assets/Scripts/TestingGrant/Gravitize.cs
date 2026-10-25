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

    GameObject firstObject;
    GameObject secondObject;
    Color storedFirstColor;
    Color storedSecondColor;
    Color firstColor = new Color(0f,1f,1f,1);
    Color secondColor = new Color(1f,0f,1f,1);
    Color uiDefault = Color.gray8;
    Vector3 firstBorderScale = new(1.011f,1.011f,1.011f);
    Vector3 secondBorderScale = new(1.01f,1.01f,1.01f);

    bool pulling = false;
    bool pushing = false;

    void Awake()
    {
        firstObject = null;
        secondObject = null;

        top.color = uiDefault;
        bottom.color = uiDefault;
        left.color = uiDefault;
        right.color = uiDefault;

        ActionChange(line);
    }

    void OnLeftSelect()
    {
        // Remove current firstObject
        DeselectFirst();

        Ray ray = new Ray(head.position, head.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            GameObject obj = hit.collider.gameObject;

            if (obj.CompareTag("Gravitizable") && obj != secondObject)
            {
                if (firstObject)
                {
                    firstObject.GetComponent<Renderer>().material.color = storedFirstColor;
                }

                firstObject = obj;
                SelectFirst();
            }
        }
    }

    void OnRightSelect()
    {
        // Remove current firstObject
        DeselectSecond();

        Ray ray = new Ray(head.position, head.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            GameObject obj = hit.collider.gameObject;

            if (obj.CompareTag("Gravitizable") && obj != firstObject)
            {
                if (secondObject)
                {
                    secondObject.GetComponent<Renderer>().material.color = storedSecondColor;
                }

                secondObject = obj;
                SelectSecond();
            }        
        }
    }

    void OnGravitize()
    {
        if (!pulling && firstObject && secondObject)
        {
            pushing = false;
            pulling = true;
        }
        else
        {
            pushing = false;
            pulling = false;
        }
    }

    void OnRepell()
    {
        if (!pushing && firstObject && secondObject)
        {
            pulling = false;
            pushing = true;
        }
        else
        {
            pulling = false;
            pushing = false;
        }
    }

    void FixedUpdate()
    {
        if (!firstObject || !secondObject)
        {
            pulling = false;
            pushing = false;
        }

        if (pulling || pushing)
        {
            DirectionalForce(firstObject, secondObject);
            DirectionalForce(secondObject, firstObject);
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

    void SelectFirst()
    {
        GameObject frame = GetFrameObject(firstObject);
        frame.transform.localScale = firstBorderScale;

        Renderer frameRenderer = frame.GetComponentInChildren<Renderer>();

        storedFirstColor = frameRenderer.material.color;

        frameRenderer.material.color = firstColor;
        frameRenderer.material.SetColor("_EmissionColor", firstColor * 1.5f);
        frameRenderer.enabled = true;

        top.color = firstColor;
        right.color = firstColor;
    }

    void DeselectFirst()
    {
        if (!firstObject) { return; }

        GameObject frame = GetFrameObject(firstObject);
        Renderer frameRenderer = frame.GetComponentInChildren<Renderer>();

        frameRenderer.material.color = storedFirstColor;
        frameRenderer.enabled = false;

        top.color = uiDefault;
        right.color = uiDefault;

        firstObject = null;
    }

    void SelectSecond()
    {
        GameObject frame = GetFrameObject(secondObject);
        frame.transform.localScale = secondBorderScale;

        Renderer frameRenderer = frame.GetComponentInChildren<Renderer>();

        storedSecondColor = frameRenderer.material.color;
        
        frameRenderer.material.color = secondColor;
        frameRenderer.material.SetColor("_EmissionColor", secondColor * 1.5f);
        frameRenderer.enabled = true;

        bottom.color = secondColor;
        left.color = secondColor;
    }

    void DeselectSecond()
    {
        if (!secondObject) { return; }

        GameObject frame = GetFrameObject(secondObject);
        Renderer frameRenderer = frame.GetComponentInChildren<Renderer>();

        frameRenderer.material.color = storedSecondColor;
        frameRenderer.enabled = false;

        bottom.color = uiDefault;
        left.color = uiDefault;

        secondObject = null;
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
