using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject player;
    public GameObject objectToActivate;
    public float pressDepth = 0.1f;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private bool isPressed = false;

    void Start()
    {
        originalPosition = transform.position;
        pressedPosition = originalPosition - new Vector3(0, pressDepth, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            transform.position = pressedPosition;
            isPressed = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            transform.position = originalPosition;
            isPressed = false;
        }
    }
}
        