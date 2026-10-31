using UnityEngine;
using UnityEngine.InputSystem;

public class TreeChopper : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] int damage = 1;
    [SerializeField] int stoneDamage = 0;
    [SerializeField] float maxChopRange = 2.0f;
    [SerializeField] LayerMask chopMask;
    //[SerializeField] DamageNumbersPro.DamageNumber damageNumberPrefab;
    
    public void OnChopInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var hit, maxChopRange, chopMask))
        {
            var tree = hit.transform.GetComponentInParent<TreeComponent>();
            if (tree != null && !tree.IsDead)
            {
                switch (tree.resourceType)
                {
                    case Resource.ResourceType.Wood:
                        tree.Chop(damage);
                        //if (damageNumberPrefab != null) damageNumberPrefab.Spawn(hit.point, damage);
                        break;
                    case Resource.ResourceType.Stone:
                        tree.Chop(stoneDamage);
                        //if (damageNumberPrefab != null) damageNumberPrefab.Spawn(hit.point, stoneDamage);
                        break;

                    // Cannot directly harvest money from objects
                    case Resource.ResourceType.Money:
                        break;
                }
            }
        }
    }

    public void SetDamage(int _damage) => damage = _damage;
    public void IncreaseDamage(int increase) => damage += increase;

    public void SetStoneDamage(int _stoneDamage) => stoneDamage = _stoneDamage;
    public void IncreaseStoneDamage(int increase) => stoneDamage += increase;
}
