using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private Vector3 basicOffset = new(0,0,-1);

    private float yaw;
    private float pitch;

    private InputAction lookAction;

    private void Start()
    {
        lookAction = InputSystem.actions.FindAction("Look");
        yaw = 0f;
        pitch = 0f;
    }

    private void Update()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        pitch += -lookInput.y * Time.deltaTime * rotationSpeed;
        pitch = Mathf.Clamp(pitch, -30f, 80f);
        yaw += lookInput.x * Time.deltaTime * rotationSpeed;

        Quaternion pitchRotation = Quaternion.AngleAxis(pitch, Vector3.right);
        Quaternion yawRotation = Quaternion.AngleAxis(yaw, Vector3.up);

        Vector3 offset = basicOffset * distance;
        offset = pitchRotation * offset;
        offset = yawRotation * offset;

        transform.position = target.position + offset;
        transform.rotation = Quaternion.LookRotation(-offset, Vector3.up);
    }

    public Quaternion GetDirection()
    {
        return Quaternion.AngleAxis(yaw, Vector3.up);
    }
}
