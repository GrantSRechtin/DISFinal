using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public Vector3 pressedPositionOffset = new Vector3(0, -0.05f, 0);
    private Vector3 originalPosition;
    private int objectsOnPlate = 0;

    void Start()
    {
        originalPosition = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Untagged") || other.CompareTag("Weight"))
        {
            objectsOnPlate++;
            transform.position = originalPosition + pressedPositionOffset;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Untagged") || other.CompareTag("Weight"))
        {
            objectsOnPlate--;
            if (objectsOnPlate <= 0)
            {
                transform.position = originalPosition;
            }
        }
    }
}
