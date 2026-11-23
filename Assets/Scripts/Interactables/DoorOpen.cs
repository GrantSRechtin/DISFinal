using System.Collections;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    
    [SerializeField] bool isEntrance;
    [SerializeField] private PressurePlate activationObject;
    [SerializeField] private GameObject barrierAndTrigger;
    [SerializeField] private AudioClip stoneGrindingClip;

    private const float OpenOffset = 4.7f;
    private Vector3 closedPosition;
    private Coroutine movementCoroutine;
    private Collider barrierCollider;

    void Awake()
    {
        if (barrierAndTrigger != null)
        {
            barrierCollider = barrierAndTrigger.GetComponent<Collider>();
        }

        if (!isEntrance)
        {
            if (barrierCollider == null)
            {
                Debug.LogError("Exit doors require a barrier with a Collider.", this);
            }
            else
            {
                barrierCollider.isTrigger = false;
            }
        }
    }

    void Start()
    {
        closedPosition = transform.position;

        if (isEntrance)
        {
            transform.position = closedPosition + Vector3.up * OpenOffset;
            MoveDoorTo(closedPosition, false);
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

    void OpenDoor()
    {
        MoveDoorTo(closedPosition + Vector3.up * OpenOffset, true);
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

        SetBarrierTrigger(false);
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
        SetBarrierTrigger(endOpen);
    }

    void SetBarrierTrigger(bool isTrigger)
    {
        if (!isEntrance && barrierCollider != null)
        {
            barrierCollider.isTrigger = isTrigger;
        }
    }

}
