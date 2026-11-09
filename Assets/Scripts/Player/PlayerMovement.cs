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
        base.OnDisable();
        input.JumpPressed -= RequestJump;
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

    public override void OnTimeStarted()
    {
        base.OnTimeStarted();
        rb.WakeUp();
    }

    public override void OnTimeStopped()
    {
        base.OnTimeStopped();
        rb.Sleep();
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

    public override void OnRewindTimestep(int steps)
    {
        if (timesteps.Count == 0) return;

        Timestep timestep;
        if (timesteps.Count > steps)
        {
            int lastIndex = timesteps.Count - steps;
            timestep = timesteps[lastIndex];

            while(timesteps.Count > lastIndex+1)
            {
                timesteps.RemoveAt(timesteps.Count-1);
            }
            timesteps.RemoveAt(lastIndex);
        }
        else
        {
            int lastIndex = 0;
            timestep = timesteps[lastIndex];
            timesteps.Clear();
        }

        rb.position = timestep.Position;
        rb.rotation = timestep.Rotation;
        rb.linearVelocity = timestep.LinearVelocity;
        rb.angularVelocity = timestep.AngularVelocity;
        jumpRequested = false;
    }

    private void ApplyMovement()
    {
        // current version is ai
        Vector2 moveInput = Vector2.ClampMagnitude(input.Move, 1f);
        Vector3 inputDirection = playerCamera.GetDirection() * new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 horizontalVelocity = new(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (inputDirection.sqrMagnitude > 0f)
        {
            Vector3 targetVelocity = inputDirection * speed;
            bool slowingDown = horizontalVelocity.sqrMagnitude > targetVelocity.sqrMagnitude
                || Vector3.Dot(horizontalVelocity, targetVelocity) < horizontalVelocity.sqrMagnitude;
            float accelerationLimit = slowingDown ? decelerationRate : accelerationRate;
            Vector3 controlledVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                accelerationLimit * Time.fixedDeltaTime);
            rb.AddForce(
                (controlledVelocity - horizontalVelocity) / Time.fixedDeltaTime,
                ForceMode.Acceleration);
        }
        else if (OnGround())
        {
            Vector3 deceleration = -horizontalVelocity / Time.fixedDeltaTime;
            rb.AddForce(Vector3.ClampMagnitude(deceleration, decelerationRate), ForceMode.Acceleration);
        }

        rb.AddForce(Vector3.down * gravityPower, ForceMode.Acceleration);
    }

    private void ApplyJump()
    {
        if (!jumpRequested) return;
        jumpRequested = false;

        if (OnGround()) rb.AddForce(jumpPower * Vector3.up, ForceMode.Impulse);
    }

    private bool OnGround()
    {
        Vector3 overlapBoxCenter = transform.position + (Vector3.down * .1f);
        Vector3 size = new(0.5f,0.125f,0.5f);
        Collider[] groundColliders = Physics.OverlapBox(overlapBoxCenter, size);
        return groundColliders.Length > 1;
    }
}