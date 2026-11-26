using UnityEngine;
using UnityEngine.Events;

// "Inventory" being like the player's hotbar
public class InventoryComponent : MonoBehaviour
{
    #region Data Structures

    [System.Serializable]
    public struct InventoryItem
    {
        public Item item;
        public int quantity;
        public InventoryItem(Item _item, int _quantity)
        {
            item = _item;
            quantity = _quantity;
        }
    }

    [System.Serializable]
    public struct InventorySlot
    {
        public InventoryItem inventoryItem;
        public int slotIndex;
        public InventorySlot(InventoryItem _inventoryItem, int _slotIndex)
        {
            inventoryItem = _inventoryItem;
            slotIndex = _slotIndex;
        }
    }
    
    [System.Serializable]
    public struct Inventory
    {
        InventoryItem? equipped;
        public InventorySlot[] slots;
        public int capacity;
        public Inventory(int _capacity)
        {
            equipped = null;
            capacity = _capacity;
            slots = new InventorySlot[capacity];
            for (int i = 0; i < capacity; i++)
            {
                slots[i] = new InventorySlot(new InventoryItem(null, 0), i);
            }
        }

        public readonly int AddItem(Item item, int quantity)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].inventoryItem.item == null)
                {
                    slots[i].inventoryItem = new InventoryItem(item, quantity);
                    return i;
                }
            }
            return -1;
        }

        public readonly void RemoveItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return;
            slots[slotIndex].inventoryItem = new InventoryItem(null, 0);
        }

        public readonly void ClearInventory()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].inventoryItem = new InventoryItem(null, 0);
            }
        }
        public void SetEquipped(int index)
        {
            if (index < 0 || index >= slots.Length) return;
            equipped = slots[index].inventoryItem;
        }
        public readonly InventoryItem? GetEquipped() => equipped;
    }

    #endregion

    [SerializeField] int inventoryCapacity = 5;
    [SerializeField] Item startEquip;
    [SerializeField] Inventory inventory = new(0);
    public Inventory InventoryData => inventory;
    
    [Header("Events")]
    [SerializeField] UnityEvent<InventoryItem> onItemEquipped;
    public UnityEvent<InventoryItem> OnItemEquipped => onItemEquipped;

    private void Awake()
    {
        inventory = new Inventory(inventoryCapacity);
        if (startEquip != null) Equip(inventory.AddItem(startEquip, 1));
    }

    void Equip(int index)
    {
        inventory.SetEquipped(index);
        if (onItemEquipped != null) onItemEquipped.Invoke(inventory.GetEquipped().Value);
    }
}
