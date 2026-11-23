using UnityEngine;
using TMPro;

public class ShopItemButton : MonoBehaviour
{
    [SerializeField] ResourceType resourceType;

    [SerializeField] bool oneTimePurchase = true;
    [SerializeField] string itemName;
    [SerializeField] int itemCost;
    [SerializeField] TMP_Text text;
    [Space]
    [SerializeField] GameObject unlockable;

    public UnityEngine.Events.UnityEvent<int> onPurchased;

    private void Awake()
    {
        if (resourceType != null) text.text = itemName + ": " + resourceType.GetFormattedAmount(itemCost);
        if (unlockable != null) unlockable.SetActive(false);
    }

    public void TryPurchase()
    {
        if (ResourceManager.Instance != null && ResourceManager.Instance.GetResourceAmount(resourceType) >= itemCost)
        {
            ResourceManager.Instance.RemoveResource(resourceType, itemCost);

            if (unlockable != null) unlockable.SetActive(true);

            if (onPurchased != null) onPurchased.Invoke(itemCost);

            if (oneTimePurchase) Destroy(gameObject);
        }
    }
}
