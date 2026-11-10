using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Turns player intent into commands on IGravitizable objects.
/// Holds only "which objects am I holding"; the objects own everything else.
/// </summary>
public class InteractionModeHold : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInputHandler input;
    [SerializeField] Transform head;
    [SerializeField] Transform holdPosition;

    [Header("Settings")]
    [SerializeField] float pickupRange = 5f;
    [SerializeField] float pickupForce = 150f;

    bool holding = true;

    GameObject heldObject;
    Rigidbody heldRigidbody;
    IHoldable heldScript;
    float heldMass;

    float prevLinearDamping;
    Quaternion prevRotation;

    void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputHandler>();
    }

    void FixedUpdate()
    {
        if (!heldObject || !holding) return;
        MoveObject();
    }

    void OnDisable()
    {
        input.InteractPressed  -= Grab;
        input.InteractReleased -= Drop;
    }

    public void Grab()
    {
        if (!holding) return;

        heldObject = RaycastForHoldableGameObject();

        if (!heldObject) return;

        prevRotation = heldObject.transform.rotation;

        heldRigidbody = heldObject.GetComponent<Rigidbody>();
        heldRigidbody.useGravity = false;

        prevLinearDamping = heldRigidbody.linearDamping;
        heldMass = heldRigidbody.mass;

        heldRigidbody.mass = 0.00001f;
        heldRigidbody.linearDamping = 10;
        heldRigidbody.transform.parent = holdPosition;

        heldScript = heldObject.GetComponentInParent<IHoldable>();
        heldScript.IsHeld = true;
    }

    public void Drop()
    {
        if (!heldObject || !holding) return;

        heldRigidbody.useGravity = true;
        heldRigidbody.mass = heldMass;
        heldRigidbody.linearDamping = prevLinearDamping;
        heldRigidbody.transform.parent = holdPosition;
        heldRigidbody = null;

        heldScript.IsHeld = false;
        heldScript = null;

        heldObject.transform.parent = null;
        heldObject = null;
    }

    void MoveObject()
    {
        heldObject.transform.rotation = prevRotation;
        if (Vector3.Distance(heldObject.transform.position, holdPosition.position) > 0.1f)
        {
            Vector3 moveDirection = holdPosition.position - heldObject.transform.position;
            heldRigidbody.AddForce(moveDirection * pickupForce, ForceMode.Acceleration);
        }
    }

    GameObject RaycastForHoldableGameObject()
    {
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit, pickupRange)) return null;
        if (!hit.collider.TryGetComponent<IHoldable>(out IHoldable holdable)) return null;
        return hit.collider.gameObject;
    }
}