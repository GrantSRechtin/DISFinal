using System;
using System.Collections;
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
        Renderer renderer = firstObject.GetComponent<Renderer>();
        storedFirstColor = renderer.material.color;

        renderer.material.color = firstColor;
        top.color = firstColor;
        right.color = firstColor;
    }

    void DeselectFirst()
    {
        if (!firstObject) { return; }

        firstObject.GetComponent<Renderer>().material.color = storedFirstColor;
        top.color = uiDefault;
        right.color = uiDefault;

        firstObject = null;
    }

    void SelectSecond()
    {
        Renderer renderer = secondObject.GetComponent<Renderer>();
        storedSecondColor = renderer.material.color;

        renderer.material.color = secondColor;
        bottom.color = secondColor;
        left.color = secondColor;
    }

    void DeselectSecond()
    {
        if (!secondObject) { return; }

        secondObject.GetComponent<Renderer>().material.color = storedSecondColor;
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
