using System.Collections;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] bool isEntrance;
    [SerializeField] private PressurePlate activationObject;
    [SerializeField] private GameObject barrier;
    [SerializeField] private AudioClip stoneGrindingClip;

    private float originY= 2;
    private float entranceDoorStart = 6.15f;
    private float openHeight = 4.7f;
    private Vector3 closedPosition;
    private Coroutine movementCoroutine;


    void Start()
    {
        SetBarrierActive(true);

        if (isEntrance)
        {
            transform.position = new Vector3(transform.position.x, entranceDoorStart, transform.position.z);
            closedPosition = new Vector3(transform.position.x, originY, transform.position.z);
            MoveDoorTo(closedPosition, false);
        }
        else
        {
            closedPosition = transform.position;
        }
        if (activationObject == null)
        {
            Debug.LogError("Activation object is not assigned.");
            return;
        }
        activationObject.Activated += OpenDoor;
        activationObject.Deactivated += CloseDoor;
    }

    void OnDestroy()
    {
        if (activationObject != null)
        {
            activationObject.Activated -= OpenDoor;
            activationObject.Deactivated -= CloseDoor;
        }
    }

    void Update()
    {
    }

    void OpenDoor()
    {
        MoveDoorTo(closedPosition + Vector3.up * openHeight, true);
    }

    void CloseDoor()
    {
        MoveDoorTo(closedPosition, false);
    }

    void PlayDoorGrindingSound()
    {
        if (stoneGrindingClip != null && SoundFXManager.instance != null)
        {
            SoundFXManager.instance.PlayAttached(stoneGrindingClip, transform);
        }
    }

    void MoveDoorTo(Vector3 targetPosition, bool endOpen)
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        SetBarrierActive(true);
        movementCoroutine = StartCoroutine(MoveToPosition(targetPosition, endOpen));
    }

    IEnumerator MoveToPosition(Vector3 targetPosition, bool endOpen)
    {
        Vector3 startPosition = transform.position;
        if (startPosition != targetPosition)
        {
            PlayDoorGrindingSound();
        }

        const float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        transform.position = targetPosition;
        movementCoroutine = null;
        SetBarrierActive(!endOpen || isEntrance);
    }

    void SetBarrierActive(bool isActive)
    {
        if (barrier != null)
        {
            barrier.SetActive(isActive);
        }
    }
}
