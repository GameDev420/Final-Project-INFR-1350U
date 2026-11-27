using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] InventoryComponent inventory;
    [SerializeField] RectTransform itemSlotParent;
    [SerializeField] ItemSlotUI itemSlotPrefab;
    ItemSlotUI[] spawnedSlots;

    private void Start()
    {
        if (inventory != null) Init(inventory);
    }

    public void Init(InventoryComponent inventory)
    {
        // Honestly not sure how List.capacity works
        spawnedSlots = new ItemSlotUI[Mathf.Max(inventory.InventoryData.capacity, inventory.InventoryData.slots.Length)];

        for (int i = 0; i < inventory.InventoryData.slots.Length; i++)
        {
            var slotUI = Instantiate(itemSlotPrefab, itemSlotParent);
            slotUI.Init(inventory.InventoryData.slots[i]);
            spawnedSlots[i] = slotUI;
        }
    }
}
