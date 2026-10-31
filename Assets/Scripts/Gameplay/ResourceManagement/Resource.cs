using UnityEngine;

public class Resource : MonoBehaviour
{
    public enum ResourceType { Wood, Money, Stone }

    [SerializeField] int amount;
    [SerializeField] ResourceType resourceType;

    public void Consume()
    {
        switch (resourceType)
        {
            case ResourceType.Wood:
                ResourceManager.Instance.AddWood(amount);
                break;
            case ResourceType.Money:
                ResourceManager.Instance.AddMoney(amount);
                break;
            case ResourceType.Stone:
                ResourceManager.Instance.AddStone(amount);
                break;
        }

        Destroy(gameObject);
    }
}
