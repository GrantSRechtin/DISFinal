using UnityEngine;

/// <summary>
/// Rigidbody-based ball movement. Reads intent from PlayerInputHandler,
/// never touches the Input System directly.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()  => input.JumpPressed += RequestJump;
    private void OnDisable() => input.JumpPressed -= RequestJump;

    private void RequestJump() => jumpRequested = true;

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyJump();
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