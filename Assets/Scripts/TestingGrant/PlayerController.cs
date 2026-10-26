using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private CameraController ballCamera;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float accelerationRate = 20f;
    [SerializeField] private float decelerationRate = 25f;
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private float gravityPower = 1f;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction jumpAction;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Player/Move", true);
        jumpAction = InputSystem.actions.FindAction("Player/Jump", true);
    }

    private void Update()
    {
        moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        if (jumpAction.WasPressedThisFrame())
        {
            rb.AddForce(jumpPower * Vector3.up, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        Vector3 inputDirection = new(moveInput.x, 0f, moveInput.y);
        Vector3 targetVelocity = ballCamera.GetDirection() * inputDirection * speed;

        Vector3 horizontalVelocity = new(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 requiredAcceleration = (targetVelocity - horizontalVelocity) / Time.fixedDeltaTime;
        float accelerationLimit = moveInput.sqrMagnitude > 0f ? accelerationRate : decelerationRate;
        requiredAcceleration = Vector3.ClampMagnitude(requiredAcceleration, accelerationLimit);

        requiredAcceleration += Vector3.down * gravityPower;

        rb.AddForce(requiredAcceleration, ForceMode.Acceleration);
    }
}
