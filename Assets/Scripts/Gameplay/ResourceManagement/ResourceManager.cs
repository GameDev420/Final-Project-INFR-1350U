using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;
    
    [SerializeField] int wood = 0;
    [SerializeField] int money = 0;
    [SerializeField] int stone = 0;

    public int Wood => wood;
    public int Money => money;
    public int Stone => stone;

    public UnityEngine.Events.UnityEvent<int> onWoodChanged;
    public UnityEngine.Events.UnityEvent<int> onMoneyChanged;
    public UnityEngine.Events.UnityEvent<int> onStoneChanged;

    private void Awake()
    {
        Instance = this;
    }

    public void AddWood(int amount)
    {
        wood += amount;
        if (onWoodChanged != null) onWoodChanged.Invoke(wood);
    }
    public void RemoveWood(int amount)
    {
        wood -= amount;
        if (onWoodChanged != null) onWoodChanged.Invoke(wood);
    }
    public void AddMoney(int amount)
    {
        money += amount;
        if (onMoneyChanged != null) onMoneyChanged.Invoke(money);
    }
    public void RemoveMoney(int amount)
    {
        money -= amount;
        if (onMoneyChanged != null) onMoneyChanged.Invoke(money);
    }

    public void AddStone(int amount)
    {
        stone += amount;
        if (onStoneChanged != null) onStoneChanged.Invoke(stone);
    }
    public void RemoveStone(int amount)
    {
        stone -= amount;
        if (onStoneChanged != null) onStoneChanged.Invoke(stone);
    }
}
