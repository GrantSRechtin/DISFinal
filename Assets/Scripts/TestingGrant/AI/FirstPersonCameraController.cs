using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraController : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Renderer playerBodyRenderer;
    [SerializeField, Min(0f)] float eyeHeight = 1.65f;
    [SerializeField, Range(-89f, 89f)] float pitch = 0f;
    [SerializeField, Min(0f)] float mouseSensitivity = 0.12f;
    [SerializeField, Min(0f)] float gamepadTurnSpeed = 180f;
    [SerializeField, Min(0f)] float positionDamping = 20f;
    [SerializeField] bool lockCursor = true;

    float yaw;
    bool bodyRendererWasEnabled;

    void Awake()
    {
        if (target == null)
        {
            Debug.LogError($"{nameof(FirstPersonCameraController)} requires a target.", this);
            enabled = false;
            return;
        }

        yaw = target.eulerAngles.y;
        if (playerBodyRenderer != null)
        {
            bodyRendererWasEnabled = playerBodyRenderer.enabled;
            playerBodyRenderer.enabled = false;
        }

        SetCursorLocked(lockCursor);
    }

    void OnDisable()
    {
        SetCursorLocked(false);
        if (playerBodyRenderer != null)
            playerBodyRenderer.enabled = bodyRendererWasEnabled;
    }

    void LateUpdate()
    {
        ReadLookInput();

        var desiredPosition = target.position + Vector3.up * eyeHeight;
        var blend = 1f - Mathf.Exp(-positionDamping * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
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
            pitch = Mathf.Clamp(pitch - mouseDelta.y * mouseSensitivity, -89f, 89f);
        }

        if (Gamepad.current != null)
        {
            var stick = Gamepad.current.rightStick.ReadValue();
            yaw += stick.x * gamepadTurnSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch - stick.y * gamepadTurnSpeed * Time.deltaTime, -89f, 89f);
        }
    }

    static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
