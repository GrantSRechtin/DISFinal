using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    // [SerializeField] private Transform target;
    // [SerializeField] private float distance = 5f;
    [SerializeField] private float rotationSpeed = 60f;
    // [SerializeField] private Vector3 basicOffset = new(0,0,-1);

    private float yaw;
    private float pitch;

    private InputAction lookAction;

    private void Awake()
    {
        SetCursorLocked(true);
        lookAction = InputSystem.actions.FindAction("Look");
        yaw = 0f;
        pitch = 180f;
    }

    private void Update()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        pitch += -lookInput.y * Time.deltaTime * rotationSpeed * 1f;
        yaw += lookInput.x * Time.deltaTime * rotationSpeed;

        pitch = Mathf.Clamp(pitch, -30f, 80f);

        transform.rotation = Quaternion.Euler(new(pitch,yaw,0));
    }

    public Quaternion GetDirection()
    {
        return Quaternion.AngleAxis(yaw, Vector3.up);
    }

    static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public void SetSensitivity(float value)
    {
        rotationSpeed = value;
    }
}
