using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [SerializeField] private float moveUpAmount = 1f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private PressurePlate activationObject;

    void Start()
    {
        if (activationObject == null)
        {
            Debug.LogError("Activation object is not assigned.");
            return;
        }
        activationObject.Activated += OpenDoor;
    }

    void Update()
    {
    }

    void OpenDoor()
    {
        transform.position = Vector3.Lerp(transform.position, transform.position - Vector3.up * moveUpAmount, moveSpeed * Time.deltaTime);
    }
}
