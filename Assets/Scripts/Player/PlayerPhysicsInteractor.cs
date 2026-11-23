using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts.Utility.Physics;

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
        public Vector3 prevTargetRot;
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
    SPDVector3CalculatorV2 spdVelocityCalculator;
    SPDQuaternionCalculatorV2 spdRotationCalculator;
    Selection mainSelection;

    PlayerRaycaster raycaster;

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
            kp = rotationKP
        };
    }

    private void FixedUpdate()
    {
        if (mainSelection.body != null)
        {
            var desiredPoint = Camera.main.transform.position + Camera.main.transform.forward * mainSelection.grabDist;
            var currentGrabPoint = mainSelection.body.transform.TransformPoint(mainSelection.grabPoint);
            var targetPosVel = (desiredPoint - mainSelection.prevTargetPos) / spdVelocityCalculator.dt;
            var targetRotVel = (Camera.main.transform.eulerAngles - mainSelection.prevTargetRot) / spdRotationCalculator.dt;
            
            mainSelection.prevTargetPos = desiredPoint;
            mainSelection.prevTargetRot = Camera.main.transform.eulerAngles;

            spdVelocityCalculator.mass = mainSelection.body.mass;

            // Apply SPD forces
            mainSelection.body.linearVelocity = spdVelocityCalculator.ComputSPDForce(currentGrabPoint, mainSelection.body.linearVelocity, desiredPoint, targetPosVel);
            mainSelection.body.angularVelocity = spdRotationCalculator.ComputSPDTorque(mainSelection.body.rotation, mainSelection.body.angularVelocity, Camera.main.transform.rotation, targetRotVel);
        }
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
            }
        }
        else if (context.canceled)
        {
            mainSelection.body = null;
        }
    }
}
