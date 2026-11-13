using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Turns player intent into commands on IGravitizable objects.
/// Holds only "which objects am I holding"; the objects own everything else.
/// </summary>
public class PlayerHold : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInputHandler input;
    [SerializeField] Transform head;
    [SerializeField] Transform holdPosition;

    [Header("Settings")]
    [SerializeField] float pickupRange = 4f;
    [SerializeField] float pickupForce = 150f;

    public bool IsHolding => heldObject != null;

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

    void OnEnable()
    {
        input.InteractPressed  += OnInteractPressed;
        input.InteractReleased += OnInteractReleased;
    }

    void OnDisable()
    {
        input.InteractPressed  -= OnInteractPressed;
        input.InteractReleased -= OnInteractReleased;
    }

    void FixedUpdate()
    {
        if (!heldObject) return;
        MoveObject();
    }

    void OnInteractPressed()
    {
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

    void OnInteractReleased()
    {
        if (!heldObject) return;

        heldRigidbody.useGravity = true;
        heldRigidbody.mass = heldMass;
        heldRigidbody.linearDamping = prevLinearDamping;
        heldRigidbody.transform.parent = holdPosition;
        
        StopObject();

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

    void StopObject()
    {
        Rigidbody playerRb = holdPosition.GetComponentInParent<Rigidbody>();
        heldRigidbody.linearVelocity = playerRb.GetPointVelocity(heldRigidbody.worldCenterOfMass);
    }

    GameObject RaycastForHoldableGameObject()
    {
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit, pickupRange)) return null;
        if (!hit.collider.TryGetComponent<IHoldable>(out IHoldable holdable)) return null;
        return hit.collider.gameObject;
    }
}