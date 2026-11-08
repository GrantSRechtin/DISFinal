using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rigidbody-based ball movement. Reads intent from PlayerInputHandler,
/// never touches the Input System directly.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : RewindableBase
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerCamera playerCamera;

    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float accelerationRate = 20f;
    [SerializeField] private float decelerationRate = 25f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpPower = 0.75f;
    [SerializeField] private float gravityPower = 10f;

    private Rigidbody rb;
    private bool jumpRequested;
    readonly List<Timestep> timesteps = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        input.JumpPressed += RequestJump;
    }

    protected override void OnDisable()
    {
        input.JumpPressed -= RequestJump;
        base.OnDisable();
    }

    private void RequestJump()
    {
        if (!IsTimeStopped) jumpRequested = true;
    }

    private void FixedUpdate()
    {
        if (IsTimeStopped) return;

        ApplyMovement();
        ApplyJump();
    }

    public override void OnStoreTimestep()
    {
        timesteps.Add(new Timestep
        {
            Position = rb.position,
            Rotation = rb.rotation,
            LinearVelocity = rb.linearVelocity,
            AngularVelocity = rb.angularVelocity
        });
    }

    public override void OnRewindTimestep()
    {
        if (timesteps.Count == 0) return;

        int lastIndex = timesteps.Count - 1;
        Timestep timestep = timesteps[lastIndex];
        timesteps.RemoveAt(lastIndex);

        rb.position = timestep.Position;
        rb.rotation = timestep.Rotation;
        rb.linearVelocity = timestep.LinearVelocity;
        rb.angularVelocity = timestep.AngularVelocity;
        jumpRequested = false;
    }

    private void ApplyMovement()
    {
        Vector2 moveInput = Vector2.ClampMagnitude(input.Move, 1f);

        Vector3 inputDirection = new(moveInput.x, 0f, moveInput.y);
        Vector3 targetVelocity = playerCamera.GetDirection() * inputDirection * speed;

        Vector3 horizontalVelocity = new(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 requiredAcceleration = (targetVelocity - horizontalVelocity) / Time.fixedDeltaTime;
        float accelerationLimit = moveInput.sqrMagnitude > 0f ? accelerationRate : decelerationRate;
        requiredAcceleration = Vector3.ClampMagnitude(requiredAcceleration, accelerationLimit);

        requiredAcceleration += Vector3.down * gravityPower;

        rb.AddForce(requiredAcceleration, ForceMode.Acceleration);
    }

    private void ApplyJump()
    {
        if (!jumpRequested) return;
        jumpRequested = false;

        rb.AddForce(jumpPower * Vector3.up, ForceMode.Impulse);
    }
}