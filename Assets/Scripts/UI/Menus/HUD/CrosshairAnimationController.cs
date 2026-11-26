using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CrosshairAnimationController : MonoBehaviour
{
    [SerializeField] string isHoveringParameterName = "IsHovering";
    [SerializeField] PlayerPhysicsInteractor interactor;
    Animator anim;

    private void Awake() => anim = GetComponent<Animator>();

    void OnEnable()
    {
        if (interactor != null) interactor.OnHoverTargetChanged.AddListener(OnHoverTargetChanged);
    }
    void OnDisable()
    {
        if (interactor != null) interactor.OnHoverTargetChanged.RemoveListener(OnHoverTargetChanged);
    }

    void OnHoverTargetChanged(Rigidbody newHoverTarget)
    {
        if (anim != null) anim.SetBool(isHoveringParameterName, newHoverTarget != null);
    }
}
