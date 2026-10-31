using UnityEngine;
using TMPro;

public class ShopItemButton : MonoBehaviour
{
    [SerializeField] Resource.ResourceType resourceType;

    [SerializeField] bool oneTimePurchase = true;
    [SerializeField] string itemName;
    [SerializeField] int itemCost;
    [SerializeField] TMP_Text text;
    [Space]
    [SerializeField] GameObject unlockable;

    public UnityEngine.Events.UnityEvent<int> onPurchased;

    private void Awake()
    {
        text.text = itemName + ": " + (resourceType == Resource.ResourceType.Money ? "$" : "") + itemCost + (resourceType != Resource.ResourceType.Money ? " " + resourceType.ToString() : "");
        if (unlockable != null) unlockable.SetActive(false);
    }

    public void TryPurchase()
    {
        int currentAmount = -1;

        switch (resourceType)
        {
            case Resource.ResourceType.Wood:
                currentAmount = ResourceManager.Instance.Wood;
                break;

            case Resource.ResourceType.Money:
                currentAmount = ResourceManager.Instance.Money;
                break;

            case Resource.ResourceType.Stone:
                currentAmount = ResourceManager.Instance.Stone;
                break;
        }

        if (currentAmount >= itemCost)
        {
            switch (resourceType)
            {
                case Resource.ResourceType.Wood:
                    ResourceManager.Instance.RemoveWood(itemCost);
                    break;

                case Resource.ResourceType.Money:
                    ResourceManager.Instance.RemoveMoney(itemCost);
                    break;
                case Resource.ResourceType.Stone:
                    ResourceManager.Instance.RemoveStone(itemCost);
                    break;
            }

            if (unlockable != null) unlockable.SetActive(true);

            if (onPurchased != null) onPurchased.Invoke(itemCost);

            if (oneTimePurchase) Destroy(gameObject);
        }
    }
}
