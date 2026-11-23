using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TreeChopper : MonoBehaviour
{
    [System.Serializable]
    public struct ResourceDamage
    {
        [SerializeField] ResourceType type;
        [SerializeField] int baseDamage;
        readonly List<int> damageModifiers;

        public readonly ResourceType Type => type;
        public readonly int BaseDamage => baseDamage;

        public ResourceDamage(ResourceType _type, int _baseDamage)
        {
            type = _type;
            baseDamage = _baseDamage;
            damageModifiers = new List<int>();
        }

        public readonly int GetDamage()
        {
            int totalDamage = baseDamage;
            if (damageModifiers != null)
            {
                foreach (var modifier in damageModifiers)
                {
                    totalDamage += modifier;
                }
            }
            return totalDamage;
        }
    }


    [Header("Config")]
    [SerializeField] ResourceDamage[] resourceDamages = new ResourceDamage[0];
    [SerializeField] float maxChopRange = 2.0f;
    [SerializeField] LayerMask chopMask;
    //[SerializeField] DamageNumbersPro.DamageNumber damageNumberPrefab;
    readonly Dictionary<ResourceType, ResourceDamage> resourceDamageDict = new();

    private void Awake()
    {
        resourceDamageDict.Clear();

        if (resourceDamages == null) resourceDamages = new ResourceDamage[0];
        foreach (var resourceDamage in resourceDamages)
        {
            if (resourceDamage.Type != null) resourceDamageDict.Add(resourceDamage.Type, resourceDamage);
        }
    }

    public void OnChopInput(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled || !context.performed) return;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var hit, maxChopRange, chopMask))
        {
            var tree = hit.transform.GetComponentInParent<ResourceComponent>();
            if (tree != null && !tree.IsDead && tree.ResourceType != null && resourceDamageDict.TryGetValue(tree.ResourceType, out var resourceDamage))
            {
                tree.Chop(resourceDamage.GetDamage());
            }
        }
    }
}
