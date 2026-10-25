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
    Color firstColor = new Color(59f/255f,66f/255f,136f/255f,1); // Color.skyBlue;
    Color secondColor = new Color(149f/255f,80f/255f,20f/255f,1); // Color.orange;

    Color uiDefault = Color.gray8;

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

        // Apply forces
        if (pulling || pushing)
        {
            DirectionalForce(firstObject, secondObject);
            DirectionalForce(secondObject, firstObject);
        }

        CheckAction();
    }

    void DirectionalForce(GameObject victim, GameObject target)
    {
        // Apply forces
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
        Renderer cubeRenderer = firstObject.GetComponent<Renderer>();
        HashSet<Renderer> renderers = new HashSet<Renderer>(firstObject.GetComponentsInChildren<Renderer>());
        renderers.Remove(cubeRenderer);
        Renderer borderRenderer = renderers.ToArray()[0];

        storedFirstColor = borderRenderer.material.color;

        borderRenderer.material.color = firstColor;
        borderRenderer.material.SetColor("_EmissionColor", firstColor * 0.5f);
        borderRenderer.enabled = true;

        top.color = firstColor;
        right.color = firstColor;
    }

    void DeselectFirst()
    {
        if (!firstObject) { return; }

        Renderer cubeRenderer = firstObject.GetComponent<Renderer>();
        HashSet<Renderer> renderers = new HashSet<Renderer>(firstObject.GetComponentsInChildren<Renderer>());
        renderers.Remove(cubeRenderer);
        Renderer borderRenderer = renderers.ToArray()[0];

        borderRenderer.material.color = storedFirstColor;
        borderRenderer.enabled = false;

        top.color = uiDefault;
        right.color = uiDefault;

        firstObject = null;
    }

    void SelectSecond()
    {
        Renderer cubeRenderer = secondObject.GetComponent<Renderer>();
        HashSet<Renderer> renderers = new HashSet<Renderer>(secondObject.GetComponentsInChildren<Renderer>());
        renderers.Remove(cubeRenderer);
        Renderer borderRenderer = renderers.ToArray()[0];

        storedSecondColor = borderRenderer.material.color;
        
        borderRenderer.material.color = secondColor;
        borderRenderer.material.SetColor("_EmissionColor", secondColor * 0.5f);
        borderRenderer.enabled = true;

        bottom.color = secondColor;
        left.color = secondColor;
    }

    void DeselectSecond()
    {
        if (!secondObject) { return; }

        Renderer cubeRenderer = secondObject.GetComponent<Renderer>();
        HashSet<Renderer> renderers = new HashSet<Renderer>(secondObject.GetComponentsInChildren<Renderer>());
        renderers.Remove(cubeRenderer);
        Renderer borderRenderer = renderers.ToArray()[0];

        borderRenderer.material.color = storedSecondColor;
        borderRenderer.enabled = false;

        bottom.color = uiDefault;
        left.color = uiDefault;

        secondObject = null;
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
