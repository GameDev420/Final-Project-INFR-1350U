using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using static InventoryComponent;

[RequireComponent(typeof(PlayerRaycaster))]
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

    [Header("Required References")]
    [SerializeField] InventoryComponent inventoryComponent;
    InventoryItem equippedItem;

    [Header("Config")]
    [SerializeField] ResourceDamage[] resourceDamages = new ResourceDamage[0];
    //[SerializeField] DamageNumbersPro.DamageNumber damageNumberPrefab;
    readonly Dictionary<ResourceType, ResourceDamage> resourceDamageDict = new();
    PlayerRaycaster raycaster;

    [Header("Animation")]
    //[SerializeField] string chopTriggerName = "Fire";
    [SerializeField] Animator anim;

    private void Awake()
    {
        resourceDamageDict.Clear();

        raycaster = GetComponent<PlayerRaycaster>();

        if (resourceDamages == null) resourceDamages = new ResourceDamage[0];
        foreach (var resourceDamage in resourceDamages)
        {
            if (resourceDamage.Type != null) resourceDamageDict.Add(resourceDamage.Type, resourceDamage);
        }
    }

    private void OnEnable()
    {
        if (inventoryComponent != null) inventoryComponent.OnItemEquipped.AddListener(OnItemEquipped);
    }
    private void OnDisable()
    {
        if (inventoryComponent != null) inventoryComponent.OnItemEquipped.RemoveListener(OnItemEquipped);
    }

    public void OnChopInput(InputAction.CallbackContext context)
    {
        if (!isActiveAndEnabled || !context.performed) return;

        var forwardRaycast = raycaster.RaycastForward();

        if (forwardRaycast.Hit)
        {
            var tree = forwardRaycast.RaycastHit.transform.GetComponentInParent<ResourceComponent>();
            if (tree != null && !tree.IsDead && tree.ResourceType != null && resourceDamageDict.TryGetValue(tree.ResourceType, out var resourceDamage))
            {
                tree.Chop(resourceDamage.GetDamage());
            }
        }
    }

    void OnItemEquipped(InventoryItem item)
    {
        equippedItem = item;
    }
}
