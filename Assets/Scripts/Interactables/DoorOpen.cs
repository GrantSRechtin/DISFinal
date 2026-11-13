using System.Collections;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] bool isEntrance;
    [SerializeField] private PressurePlate activationObject;

    private float originY= 2;
    private float entranceDoorStart = 6.15f;
    private float openHeight = 5f;

    void Start()
    {
        if (isEntrance)
        {
            transform.position = new Vector3(transform.position.x, entranceDoorStart, transform.position.z);
            StartCoroutine(MoveToOrigin());
        }
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
        StartCoroutine(MoveToPosition(transform.position + Vector3.up * openHeight));
    }

    IEnumerator MoveToOrigin()
    {
        Vector3 targetPosition = new Vector3(transform.position.x, originY, transform.position.z);
        yield return MoveToPosition(targetPosition);
    }

    IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        const float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        transform.position = targetPosition;
    }
}
