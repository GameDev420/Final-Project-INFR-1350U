using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Assets.Scripts.Utility;
using FirstGearGames.SmoothCameraShaker;

public class PlayerMovement : MonoBehaviour
{
    const float MarginOfError = 0.001f;

    [Header("Move Config")]
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] SmoothedValueConfig inputSmoothConfig;

    [Header("Jump Config")]
    [SerializeField] float jumpHeight;
    [SerializeField] float gravityMultiplier = 1.0f;

    [Header("References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform forwardDirection;
    [SerializeField] ShakeData jumpShakeData;
    [SerializeField] ShakeData landShakeData;

    [Header("Events")]
    [SerializeField] UnityEvent onJump;
    [SerializeField] UnityEvent onLand;

    public UnityEvent OnJumpCallback => onJump;
    public UnityEvent OnLandCallback => onLand;

    Vector2 moveInput;
    SmoothedVector2 smoothedInput;
    Vector3 velocity;
    bool prevGrounded;

    public CharacterController Controller => controller;
    public Vector2 MovementInput => moveInput;
    public SmoothedVector2 SmoothedMovementInput => smoothedInput;

    public void MoveInput(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled) return;
        moveInput = smoothedInput.TargetValue = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1.0f);
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled) return;

        if (context.performed && controller.isGrounded)
        {
            // Formula: v = sqrt(h * -2 * g)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityMultiplier * Physics.gravity.y);
            if (jumpShakeData != null) CameraShakerHandler.Shake(jumpShakeData);
            if (onJump != null) onJump.Invoke();
        }
    }

    private void Awake()
    {
        smoothedInput = new SmoothedVector2() { Config = inputSmoothConfig, SmoothedValue = moveInput, TargetValue = moveInput };
    }

    private void Update()
    {
        smoothedInput.Smooth(Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0.0f)
        {
            velocity.y = -2f; // small downward force to keep player locked to ground (especially slopes)
        }

        // use up (or down) direction if we're looking straight down (or up)
        var localDir = Mathf.Abs(forwardDirection.forward.x) > MarginOfError || Mathf.Abs(forwardDirection.forward.z) > MarginOfError ? new Vector3(smoothedInput.SmoothedValue.x, 0.0f, smoothedInput.SmoothedValue.y) : (forwardDirection.localEulerAngles.x < 180.0f ? new Vector3(smoothedInput.SmoothedValue.x, smoothedInput.SmoothedValue.y, 0.0f) : new Vector3(smoothedInput.SmoothedValue.x, -smoothedInput.SmoothedValue.y, 0.0f));
        // Another attmept, didn't work as well though
        //var localDir = new Vector3(smoothedInput.SmoothedValue.x, (Mathf.Sin(forwardDirection.localEulerAngles.x) * 0.5f + 0.5f) * smoothedInput.SmoothedValue.y, (Mathf.Cos(smoothedInput.SmoothedValue.x) * 0.5f + 0.5f) * smoothedInput.SmoothedValue.y);

        // Pretty unoptimized, but gets the job done
        controller.Move(moveSpeed * Time.deltaTime * Vector3.ClampMagnitude(Vector3.ProjectOnPlane(forwardDirection.TransformDirection(localDir), controller.transform.up).normalized * smoothedInput.SmoothedValue.magnitude, 1.0f));

        // Apply continuous gravity over time
        velocity.y += gravityMultiplier * Time.deltaTime * Physics.gravity.y;

        // Pass the final velocity vector to the CharacterController
        controller.Move(velocity * Time.deltaTime);

        if (controller.isGrounded && !prevGrounded) OnLand();

        prevGrounded = controller.isGrounded;
    }

    void OnLand()
    {
        CameraShakerHandler.FadeOut();
        if (landShakeData != null) CameraShakerHandler.Shake(landShakeData);
        if (onLand != null) onLand.Invoke();
    }
}