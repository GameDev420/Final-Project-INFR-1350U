using UnityEngine;
using UnityEngine.UI;
using static InventoryComponent;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Image[] itemImages = new Image[0];
    InventorySlot slotData;

    public void Init(InventorySlot slot)
    {
        slotData = slot;

        var icon = slotData.inventoryItem.item != null ? slotData.inventoryItem.item.itemIcon : null;
        var alphaMul = slotData.inventoryItem.item != null ? 1f : 0f;
        for (int i = 0; i < itemImages.Length; i++)
        {
            if (itemImages[i] != null)
            {
                var img = itemImages[i];
                img.sprite = icon;
                img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a * alphaMul);
            }
        }
    }
}
