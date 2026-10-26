using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField, Min(0.1f)] float followDistance = 5f;
    [SerializeField, Min(0f)] float targetHeight = 1.2f;
    [SerializeField, Range(-30f, 70f)] float pitch = 12f;
    [SerializeField, Min(0f)] float mouseSensitivity = 0.12f;
    [SerializeField, Min(0f)] float gamepadTurnSpeed = 180f;
    [SerializeField, Min(0f)] float followDamping = 12f;
    [SerializeField] bool lockCursor = true;

    float yaw;

    void Awake()
    {
        if (target == null)
        {
            Debug.LogError($"{nameof(ThirdPersonCameraController)} requires a target.", this);
            enabled = false;
            return;
        }

        yaw = target.eulerAngles.y;
        SetCursorLocked(lockCursor);
    }

    void OnDisable() => SetCursorLocked(false);

    void LateUpdate()
    {
        ReadLookInput();

        var rotation = Quaternion.Euler(pitch, yaw, 0f);
        var focusPoint = target.position + Vector3.up * targetHeight;
        var desiredPosition = focusPoint - rotation * Vector3.forward * followDistance;

        if (Physics.SphereCast(focusPoint, 0.2f, desiredPosition - focusPoint,
                out var hit, followDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            desiredPosition = focusPoint - (desiredPosition - focusPoint).normalized
                * Mathf.Max(0.1f, hit.distance - 0.1f);
        }

        var blend = 1f - Mathf.Exp(-followDamping * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
        transform.rotation = rotation;
    }

    void ReadLookInput()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetCursorLocked(false);
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            SetCursorLocked(true);

        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            var mouseDelta = Mouse.current.delta.ReadValue();
            yaw += mouseDelta.x * mouseSensitivity;
            pitch = Mathf.Clamp(pitch - mouseDelta.y * mouseSensitivity, -30f, 70f);
        }

        if (Gamepad.current != null)
        {
            var stick = Gamepad.current.rightStick.ReadValue();
            yaw += stick.x * gamepadTurnSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch - stick.y * gamepadTurnSpeed * Time.deltaTime, -30f, 70f);
        }
    }

    static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
