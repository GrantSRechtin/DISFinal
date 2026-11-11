using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [SerializeField] private float moveUpAmount = 1f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private GameObject 

    void Start()
    {
        transform.position += Vector3.up * moveUpAmount;
    }

    void Update()
    {
    }
}
