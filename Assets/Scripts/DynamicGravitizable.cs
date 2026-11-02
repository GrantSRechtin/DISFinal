using UnityEngine;

/// <summary>
/// A physics object that can be selected, linked to a partner, and pulled toward
/// or pushed from it. Owns its own frame and forces.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class DynamicGravitizable : GravitizableBase
{
    [Header("Gravity")]
    [SerializeField] float gravitationalForce = 40f;
    [SerializeField] float distanceScaling = 0.5f;
    [SerializeField] float minDist = 10f;
    [SerializeField] float pushMultiplier = -0.625f;
    [SerializeField] float minForce = 5f;

    Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Partner == null || Mode == GravityMode.None) return;

        rb.AddForce(CalculateForce(Partner.GetAnchor()), ForceMode.Force);
    }
    public override Vector3 GetAnchor()
    {
        return transform.position;
    }

    // ---------------- Physics (unchanged from the original player script) ----------------

    Vector3 CalculateForce(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;

        float distance = Mathf.Max(direction.magnitude, minDist);
        float powerRatio = 1f / Mathf.Pow(distance - minDist + 1f, distanceScaling);

        Vector3 force = direction.normalized * gravitationalForce * powerRatio;
        if (force.magnitude < minForce) return Vector3.zero;

        if (Mode == GravityMode.Push) force *= pushMultiplier;

        return force;
    }
}