using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;
    
    [SerializeField] int wood = 0;
    [SerializeField] int money = 0;
    [SerializeField] int stone = 0;
    [SerializeField] ResourceType woodType;
    [SerializeField] ResourceType moneyType;
    [SerializeField] ResourceType stoneType;

    public int Wood => wood;
    public int Money => money;
    public int Stone => stone;
    public ResourceType WoodType => woodType;
    public ResourceType MoneyType => moneyType;
    public ResourceType StoneType => stoneType;

    public UnityEvent<int> onWoodChanged;
    public UnityEvent<int> onMoneyChanged;
    public UnityEvent<int> onStoneChanged;

    private void Awake()
    {
        Instance = this;
    }

    public void AddResource(ResourceType type, int amount)
    {
        // Definitely not sustainable, but better than previous implementation.
        if (type == woodType)
        {
            wood += amount;
            if (onWoodChanged != null) onWoodChanged.Invoke(wood);
        }
        else if (type == moneyType)
        {
            money += amount;
            if (onMoneyChanged != null) onMoneyChanged.Invoke(money);
        }
        else if (type == stoneType)
        {
            stone += amount;
            if (onStoneChanged != null) onStoneChanged.Invoke(stone);
        }
    }
    public void RemoveResource(ResourceType type, int amount) => AddResource(type, -amount);

    public int GetResourceAmount(ResourceType type)
    {
        if (type == woodType) return wood;
        if (type == moneyType) return money;
        if (type == stoneType) return stone;
        return 0;
    }
}
