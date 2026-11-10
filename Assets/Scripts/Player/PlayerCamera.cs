using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orbit-style camera rotation. Reads look intent from PlayerInputHandler,
/// never touches the Input System directly.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlayerCamera : RewindableBase
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 60f;

    private float yaw;
    private float pitch;
    private List<Timestep> timesteps = new();

    private void Awake()
    {
        SetCursorLocked(true);
        yaw = 0f;
        pitch = 180f;
    }

    private void Update()
    {
        if (IsTimeStopped) return;

        Vector2 lookInput = input.Look;

        pitch += -lookInput.y * Time.deltaTime * rotationSpeed * 1f;
        yaw += lookInput.x * Time.deltaTime * rotationSpeed;

        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(new(pitch, yaw, 0));
    }

    public override void OnStoreTimestep()
    {
        timesteps.Add(new Timestep
        {
            Rotation = transform.rotation
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

        transform.rotation = timestep.Rotation;

        Vector3 eulerAngles = timestep.Rotation.eulerAngles;
        pitch = Mathf.DeltaAngle(0f, eulerAngles.x);
        yaw = Mathf.DeltaAngle(0f, eulerAngles.y);
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