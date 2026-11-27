using Assets.Scripts.Utility.Physics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(PlayerRaycaster))]
public class PlayerPhysicsInteractor : MonoBehaviour
{
    [System.Serializable]
    struct Selection
    {
        public Rigidbody body;
        public Vector3 grabPoint; // relative to the selectedInteractable
        public float grabDist;
        public Vector3 prevTargetPos;
        public Quaternion localGrabRot;
    }

    [Header("Physics Config")]
    [Tooltip("Essentially springiness, how much force is applied to reach the target position.")]
    [SerializeField] float positionKP = 5.0f;
    [Tooltip("Damping, how much the system resists changes in velocity.")]
    [SerializeField] float positionKD = 0.0f;
    [Space]
    [Tooltip("Damping, how much the system resists changes in velocity.")]
    [SerializeField] float rotationKP = 5.0f;
    [Tooltip("Damping, how much the system resists changes in angular velocity.")]
    [SerializeField] float rotationKD = 0.0f;
    [Space]
    [Tooltip("Multiplier for the velocity when releasing an object.")]
    [SerializeField] float throwVelocityMultiplier = 0.3f;
    [SerializeField] float maxThrowVelocity = Mathf.Infinity;
    SPDVector3CalculatorV2 spdVelocityCalculator;
    SPDQuaternionCalculatorV2 spdRotationCalculator;
    Selection mainSelection;
    PlayerRaycaster raycaster;

    [Header("Misc")]
    [SerializeField] bool enableOutline = true;

    [Header("Events")]
    [SerializeField] UnityEvent<Rigidbody> onHoverTargetChanged;
    public UnityEvent<Rigidbody> OnHoverTargetChanged => onHoverTargetChanged;
    Rigidbody lastFrameHoverTarget;

    void Awake()
    {
        raycaster = GetComponent<PlayerRaycaster>();

        spdVelocityCalculator = new SPDVector3CalculatorV2()
        {
            dt = Time.fixedDeltaTime,
            kd = positionKD,
            kp = positionKP,
            mass = 1.0f
        };
        spdRotationCalculator = new SPDQuaternionCalculatorV2()
        {
            dt = Time.fixedDeltaTime,
            kd = rotationKD,
            kp = rotationKP,
            body_intertiaTensorRotation_dyn = Quaternion.identity,
            inertiaTensor = Vector3.one
        };
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        spdVelocityCalculator.kp = positionKP;
        spdVelocityCalculator.kd = positionKD;
        spdRotationCalculator.kp = rotationKP;
        spdRotationCalculator.kd = rotationKD;
    }
#endif

    void FixedUpdate()
    {
        if (mainSelection.body != null)
        {
            var desiredPoint = Camera.main.transform.position + Camera.main.transform.forward * mainSelection.grabDist;
            var currentGrabPoint = mainSelection.body.transform.TransformPoint(mainSelection.grabPoint);

            var targetRot = Camera.main.transform.rotation * mainSelection.localGrabRot;

            var targetPosVel = (desiredPoint - mainSelection.prevTargetPos) / spdVelocityCalculator.dt;
            var targetRotVel = Vector3.zero;

            mainSelection.prevTargetPos = desiredPoint;

            spdVelocityCalculator.mass = mainSelection.body.mass;

            // Apply SPD forces
            mainSelection.body.linearVelocity = spdVelocityCalculator.ComputSPDForce(currentGrabPoint, mainSelection.body.linearVelocity, desiredPoint, targetPosVel);
            mainSelection.body.angularVelocity = spdRotationCalculator.ComputSPDTorque(mainSelection.body.rotation, mainSelection.body.angularVelocity, targetRot, targetRotVel);
        }
    }

    void LateUpdate()
    {
        var target = raycaster.RaycastForward();
        if (target.RaycastHit.rigidbody != lastFrameHoverTarget) ChangeHoverTarget(target.RaycastHit.rigidbody);
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            var forwardRaycast = raycaster.RaycastForward();

            if (forwardRaycast.Hit && forwardRaycast.RaycastHit.rigidbody != null && !forwardRaycast.RaycastHit.rigidbody.isKinematic)
            {
                mainSelection.body = forwardRaycast.RaycastHit.rigidbody;
                mainSelection.grabPoint = forwardRaycast.RaycastHit.rigidbody.transform.InverseTransformPoint(forwardRaycast.RaycastHit.point);
                mainSelection.grabDist = Vector3.Distance(Camera.main.transform.position, forwardRaycast.RaycastHit.point);

                mainSelection.localGrabRot = Quaternion.Inverse(forwardRaycast.RaycastHit.rigidbody.transform.rotation) * Camera.main.transform.rotation;

                mainSelection.prevTargetPos = Camera.main.transform.position + Camera.main.transform.forward * mainSelection.grabDist;
            }
        }
        else if (context.canceled)
        {
            if (mainSelection.body != null)
            {
                mainSelection.body.linearVelocity *= throwVelocityMultiplier;

                // Limit the throw velocity to the maximum
                if (mainSelection.body.linearVelocity.sqrMagnitude > maxThrowVelocity * maxThrowVelocity) mainSelection.body.linearVelocity = Vector3.ClampMagnitude(mainSelection.body.linearVelocity, maxThrowVelocity);
                mainSelection.body = null;
            }
        }
    }

    void ChangeHoverTarget(Rigidbody newTarget)
    {
        // Disable outline on the previous target if it exists
        if (enableOutline) ChangeOutline(lastFrameHoverTarget, false);

        lastFrameHoverTarget = newTarget;
        if (onHoverTargetChanged != null) onHoverTargetChanged.Invoke(newTarget);

        // Enable outline on the new target if it exists
        if (enableOutline) ChangeOutline(newTarget, true);
    }

    static void ChangeOutline(Rigidbody target, bool enable)
    {
        if (target == null) return;

        var lightingSwitcher = target.GetComponentInChildren<LightingSwitcher>();
        if (lightingSwitcher != null)
        {
            if (enable)
                lightingSwitcher.EnableOutline();
            else
                lightingSwitcher.DisableOutline();
        }
    }
}
