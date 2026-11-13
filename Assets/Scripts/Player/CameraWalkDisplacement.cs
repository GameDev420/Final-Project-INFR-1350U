using UnityEngine;
using UnityEngine.Events;
using FirstGearGames.SmoothCameraShaker;

public class CameraWalkDisplacement : MonoBehaviour
{
    const float StopThreshold = 0.01f;

    [SerializeField] bool useSmoothedInput = true;
    [SerializeField] float speed = 1.0f;
    [SerializeField] float heightDisplacement = 1.0f;
    [SerializeField] PlayerMovement movement;
    [SerializeField] Transform m_Camera;
    [SerializeField] ShakeData idleShakeData;
    [SerializeField] ShakeData walkShakeData;
    [SerializeField] ShakeData jumpShakeData;
    [SerializeField] ShakeData landShakeData;
    [Space]
    [SerializeField] UnityEvent onFootstep;
    public UnityEvent OnFootstep => onFootstep;

    float time;
    Vector3 initLocalCamPos;
    Vector2 prevMoveInput;
    Vector2 MoveInput => useSmoothedInput ? movement.SmoothedMovementInput.SmoothedValue : movement.MovementInput;

    float prevLerpTime;
    bool prevLerpDirection; // true is positive, false is negative

    bool jumping = false;

    private void Awake()
    {
        time = 0.0f;
        initLocalCamPos = m_Camera.localPosition;
    }

    private void Start()
    {
        OnStopMove();
    }

    private void Update()
    {
        if (jumping) return;

        if (Mathf.Abs(MoveInput.x) > StopThreshold || Mathf.Abs(MoveInput.y) > StopThreshold)
        {
            time += Time.deltaTime * speed * Mathf.Clamp01(MoveInput.magnitude);

            if (Mathf.Abs(prevMoveInput.x) <= StopThreshold && Mathf.Abs(prevMoveInput.y) <= StopThreshold) OnStartMove();
        }
        else if (Mathf.Abs(prevMoveInput.x) > StopThreshold || Mathf.Abs(prevMoveInput.y) > StopThreshold) OnStopMove();

        var lerpTime = Mathf.Sin(time) * 0.5f + 0.5f;
        m_Camera.localPosition = Vector3.Lerp(initLocalCamPos + 0.5f * heightDisplacement * Vector3.up, initLocalCamPos + 0.5f * heightDisplacement * Vector3.down, lerpTime);

        // On lerp peak or valley
        if (lerpTime > prevLerpTime != prevLerpDirection)
        {
            // If valley
            if (prevLerpDirection) OnStep();
        }

        prevMoveInput = MoveInput;
        prevLerpDirection = lerpTime > prevLerpTime;
        prevLerpTime = lerpTime;
    }

    void OnStartMove()
    {
        CameraShakerHandler.FadeOut();
        CameraShakerHandler.Shake(walkShakeData);
    }
    void OnStopMove()
    {
        CameraShakerHandler.FadeOut();
        if (idleShakeData != null) CameraShakerHandler.Shake(idleShakeData);
    }

    void OnStep()
    {
        if (!jumping && onFootstep != null) onFootstep.Invoke();
    }

    public void OnJump()
    {
        jumping = true;

        CameraShakerHandler.FadeOut();
        if (jumpShakeData != null) CameraShakerHandler.Shake(jumpShakeData);
    }
    public void OnLand()
    {
        jumping = false;

        CameraShakerHandler.FadeOut();
        if (landShakeData != null) CameraShakerHandler.Shake(landShakeData);

        if (MoveInput.sqrMagnitude > 0.01f) OnStartMove();
    }

    private void OnDisable()
    {
        CameraShakerHandler.FadeOut();
    }
}