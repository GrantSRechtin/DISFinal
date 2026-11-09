using UnityEngine;

/// <summary>
/// A physics object that can be selected, linked to a partner, and pulled toward
/// or pushed from it. Owns its own frame and forces.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class DynamicGravitizable : SingleAnchorGravitizable, IHoldable
{
    [Header("Gravity")]
    [SerializeField] float gravitationalForce = 40f;
    [SerializeField] float distanceScaling = 0.5f;
    [SerializeField] float minDist = 10f;
    [SerializeField] float pushMultiplier = -0.625f;
    [SerializeField] float minForce = 5f;

    Rigidbody rb;

    public bool IsHeld { get; set; }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (LinkNums.Count <= 0 || IsTimeStopped || IsHeld) return;

        rb.AddForce(LinkForces(), ForceMode.Force);
    }

    // ---------------- Physics ----------------

    Vector3 LinkForces()
    {
        Vector3 totalForce = Vector3.zero;
        foreach(int linkNum in LinkNums)
        {
            Vector3 anchor = Partners[linkNum].GetAnchor(linkNum);
            GravityMode mode = Modes[linkNum];
            totalForce += CalculateForce(anchor, mode);
        }
        return totalForce; 
    }

    Vector3 CalculateForce(Vector3 targetPosition, GravityMode mode)
    {
        Vector3 direction = targetPosition - transform.position;

        float maxD = 20f;
        float minD = 2f;
        float maxForce = 50f;

        float newDistance = Mathf.Clamp(direction.magnitude, minD, maxD);

        float newPowerRatio = newDistance/maxD;
        if (mode == GravityMode.Push) newPowerRatio = 1f-newPowerRatio;

        Vector3 newForce = direction.normalized * newPowerRatio * maxForce;
        if (newForce.magnitude < minForce) return Vector3.zero;

        if (mode == GravityMode.Push) newForce *= pushMultiplier;
        if (mode == GravityMode.None) newForce = Vector3.zero;

        return newForce;

        // Old Force Stuff

        // float distance = Mathf.Max(direction.magnitude, minDist);
        // float powerRatio = 1f / Mathf.Pow(distance - minDist + 1f, distanceScaling);

        // Vector3 force = direction.normalized * gravitationalForce * powerRatio;
        // if (force.magnitude < minForce) return Vector3.zero;

        // if (mode == GravityMode.Push) force *= pushMultiplier;
        // if (mode == GravityMode.None) force = Vector3.zero;

        // return force;
    }

    // ----------------- Rewindable -----------------

    public override void OnTimeStarted()
    {
        rb.WakeUp();
        base.OnTimeStarted();
    }

    public override void OnTimeStopped()
    {
        rb.Sleep();
        base.OnTimeStopped();
    }

}