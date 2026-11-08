using UnityEngine;

/// <summary>
/// Orbit-style camera rotation. Reads look intent from PlayerInputHandler,
/// never touches the Input System directly.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 60f;

    private float yaw;
    private float pitch;

    private void Awake()
    {
        SetCursorLocked(true);
        yaw = 0f;
        pitch = 180f;
    }

    private void Update()
    {
        Vector2 lookInput = input.Look;

        pitch += -lookInput.y * Time.deltaTime * rotationSpeed * 1f;
        yaw += lookInput.x * Time.deltaTime * rotationSpeed;

        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(new(pitch, yaw, 0));
    }

    public Quaternion GetDirection()
    {
        return Quaternion.AngleAxis(yaw, Vector3.up);
    }

    public void SetSensitivity(float value)
    {
        rotationSpeed = value;
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}