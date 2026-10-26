using UnityEngine;

public class SelfGravitizable : MonoBehaviour
{
    [SerializeField] GameObject connectedGravitizable;

    [SerializeField] bool _active = false;
    [SerializeField] float _mass = 1;

    [SerializeField] float gravitationalForce = 10;

    public float Mass => _mass;
    public bool Active => _active;

    // Components
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!connectedGravitizable) { return; }
        SelfGravitizable gravitizable = connectedGravitizable.GetComponent<SelfGravitizable>();

        if (!_active || !gravitizable.Active) { return; }

        Vector3 direction = connectedGravitizable.transform.position - transform.position;
        Vector3 force = direction * gravitationalForce;

        rb.AddForce(force, ForceMode.Force);
    }
}
