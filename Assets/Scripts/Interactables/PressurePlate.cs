using System;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public event Action Activated;
    public event Action Deactivated;
    public float pressDepth = 0.1f;
    [SerializeField] Transform plateToMove;
    Vector3 originalPosition;
    Vector3 pressedPosition;

    void Start()
    {
        if (plateToMove == null) plateToMove = transform;
        originalPosition = plateToMove.position;
        pressedPosition = originalPosition - new Vector3(0, pressDepth, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.GetComponent<IGravitizable>() != null)
        {
            plateToMove.position = pressedPosition;
            Activated?.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<IGravitizable>() != null)
        {
            plateToMove.position = originalPosition;
            Deactivated?.Invoke();
        }
    }
}
        