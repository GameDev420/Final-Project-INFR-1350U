using UnityEngine;
using Assets.Scripts.Utility.Physics;

public class SPDComponent : MonoBehaviour
{
    [SerializeField] float kd;
    [SerializeField] float kp;
    [SerializeField] float mass;
    [SerializeField] Transform target;
    [SerializeField] Rigidbody body;
    SPDVector3CalculatorV2 spdCalculator;
    Vector3 prevTargetPos;

    private void Awake()
    {
        spdCalculator = new SPDVector3CalculatorV2()
        {
            dt = Time.fixedDeltaTime,
            kd = kd,
            kp = kp,
            mass = mass
        };
    }

    private void OnValidate()
    {
        spdCalculator = new SPDVector3CalculatorV2()
        {
            dt = Time.fixedDeltaTime,
            kd = kd,
            kp = kp,
            mass = mass
        };
    }

    private void FixedUpdate()
    {
        var targetVel = (target.position - prevTargetPos) / spdCalculator.dt;
        prevTargetPos = target.position;
        body.linearVelocity = spdCalculator.ComputSPDForce(body.position, body.linearVelocity, target.position, targetVel);
    }
}
