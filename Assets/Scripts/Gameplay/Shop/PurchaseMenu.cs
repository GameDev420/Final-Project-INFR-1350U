using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PurchaseMenu : MonoBehaviour
{
    [SerializeField] InputActionReference toggleShopInput;

    private void Awake()
    {
        toggleShopInput.action.Enable();
        toggleShopInput.action.performed += OnToggleShopInputPerformed;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        toggleShopInput.action.performed -= OnToggleShopInputPerformed;
        toggleShopInput.action.Disable();
    }

    void OnToggleShopInputPerformed(InputAction.CallbackContext context)
    {
        gameObject.SetActive(!gameObject.activeSelf);

        Cursor.lockState = gameObject.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = gameObject.activeSelf;
    }
}
