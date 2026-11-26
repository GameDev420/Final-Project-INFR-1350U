using UnityEngine;

public class PlayerRaycaster : MonoBehaviour
{
    public struct RaycastResult
    {
        public bool Hit;
        public RaycastHit RaycastHit;

        public RaycastResult(bool hit, RaycastHit raycastHit)
        {
            Hit = hit;
            RaycastHit = raycastHit;
        }
    }

    [Header("Forward Raycast Config")]
    [SerializeField] float range = 2.0f;
    [SerializeField] LayerMask interactableMask;
    [SerializeField] LayerMask physicsHitMask;
    RaycastResult forwardResult;

    bool forwardRaycastThisFrame;
    void Update()
    {
        forwardRaycastThisFrame = false;
    }

    public RaycastResult RaycastForward()
    {
        if (!forwardRaycastThisFrame)
        {
            forwardRaycastThisFrame = true;
            var hit = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var raycastHit, range, physicsHitMask) && raycastHit.collider != null && interactableMask.IsInLayerMask(raycastHit.collider.gameObject.layer);

            forwardResult = new RaycastResult(hit, raycastHit);
        }
        return forwardResult;
    }

    public RaycastResult CustomRaycastForward(float range)
    {
        var hit = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var raycastHit, range, physicsHitMask) && raycastHit.collider != null && interactableMask.IsInLayerMask(raycastHit.collider.gameObject.layer);
        return new RaycastResult(hit, raycastHit);
    }
}
