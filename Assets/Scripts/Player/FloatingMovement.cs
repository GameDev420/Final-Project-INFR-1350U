using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FloatingMovement : MonoBehaviour
{
    #region Fields

    [SerializeField] float moveSpeed = 10.0f;
    CharacterController controller;

    Vector2 moveInput;
    float verticalMoveInput;

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        controller.Move(moveSpeed * Time.deltaTime * controller.transform.TransformDirection(new Vector3(moveInput.x, verticalMoveInput, moveInput.y).normalized));
    }

    #endregion

    #region Input Methods

    public void MoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void VerticalMoveInput(InputAction.CallbackContext context)
    {
        verticalMoveInput = context.ReadValue<float>();
    }

    #endregion
}
