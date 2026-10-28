using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float sensitivity = 0.1f;
    [SerializeField] [Range(0.0f, 90.0f)] float maxLookAngle = 90.0f;
    [SerializeField] Transform cam;
    [SerializeField] Transform playerBody;
    Vector2 pointerDelta;
    float xRot;

    public void PointerDelta(InputAction.CallbackContext context)
    {
        pointerDelta += context.ReadValue<Vector2>();
    }

    private void Awake()
    {
        if (cam == null) cam = Camera.main != null ? Camera.main.transform : transform;
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (pointerDelta.x == 0 && pointerDelta.y == 0) return;

        xRot = Mathf.Clamp(xRot - pointerDelta.y * sensitivity, -maxLookAngle, maxLookAngle);
        cam.localRotation = Quaternion.Euler(xRot, cam.localEulerAngles.y, cam.localEulerAngles.z);
        playerBody.Rotate(playerBody.up, pointerDelta.x * sensitivity, Space.World);

        pointerDelta = Vector2.zero;
    }
}