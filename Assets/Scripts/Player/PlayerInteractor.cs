using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("General Config")]
    [SerializeField] float range = 2.0f;
    [SerializeField] LayerMask interactableMask;

    [Header("PhysicsConfig")]
    [SerializeField] float pullForce = 1.0f;

    [Header("Input Config")]
    [SerializeField] UnityEngine.InputSystem.InputActionReference interactInput;

    [SerializeField] InputActionReference thirdHandInput;
    
    bool thirdHand = false;

    Rigidbody selectedInteractable;
    Vector3 grabPoint; // relative to the selectedInteractable
    float grabDist;

    Rigidbody TH_selectedInteractable;
    Vector3 TH_grabPoint;
    float TH_grabDist;
    

    private void OnEnable()
    {
        interactInput.action.Enable();
        interactInput.action.performed += OnInteractInputPerformed;
        interactInput.action.canceled += OnInteractInputCanceled;

        thirdHandInput.action.Enable();
        thirdHandInput.action.performed += OnThirdHandInputPerformed;
        thirdHandInput.action.canceled += OnThirdHandInputCanceled;
    }
    private void OnDisable()
    {
        interactInput.action.performed -= OnInteractInputPerformed;
        interactInput.action.canceled -= OnInteractInputCanceled;
        interactInput.action.Disable();

        thirdHandInput.action.performed -= OnThirdHandInputPerformed;
        thirdHandInput.action.canceled -= OnThirdHandInputCanceled;
        thirdHandInput.action.Disable();
    }

    private void FixedUpdate()
    {
        if (selectedInteractable != null)
        {
            var desiredPoint = Camera.main.transform.position + Camera.main.transform.forward * grabDist;
            var currentGrabPoint = selectedInteractable.transform.TransformPoint(grabPoint);
            selectedInteractable.AddForceAtPosition(pullForce * Time.fixedDeltaTime * (desiredPoint - currentGrabPoint).normalized, currentGrabPoint, ForceMode.Acceleration);
        }

        if (TH_selectedInteractable != null)
        {
            var desiredPoint = Camera.main.transform.position + Camera.main.transform.forward * TH_grabDist;
            var currentGrabPoint = TH_selectedInteractable.transform.TransformPoint(TH_grabPoint);
            TH_selectedInteractable.AddForceAtPosition(pullForce * Time.fixedDeltaTime * (desiredPoint - currentGrabPoint).normalized, currentGrabPoint, ForceMode.Acceleration);
        }
    }

    void OnInteractInputPerformed(InputAction.CallbackContext context)
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var hit, range, interactableMask) && hit.rigidbody != null)
        {
            selectedInteractable = hit.rigidbody;
            grabPoint = hit.rigidbody.transform.InverseTransformPoint(hit.point);
            grabDist = Vector3.Distance(Camera.main.transform.position, hit.point);
        }
    }
    void OnInteractInputCanceled(InputAction.CallbackContext context)
    {
        selectedInteractable = null;
    }

    void OnThirdHandInputPerformed(InputAction.CallbackContext context)
    {
        if (!thirdHand) return;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var hit, range, interactableMask) && hit.rigidbody != null)
        {
            TH_selectedInteractable = hit.rigidbody;
            TH_grabPoint = hit.rigidbody.transform.InverseTransformPoint(hit.point);
            TH_grabDist = Vector3.Distance(Camera.main.transform.position, hit.point);
        }
    }
    void OnThirdHandInputCanceled(InputAction.CallbackContext context)
    {
        if (!thirdHand) return;
        
        TH_selectedInteractable = null;
    }

    public void SetThirdHand(bool enable) => thirdHand = enable;
}
