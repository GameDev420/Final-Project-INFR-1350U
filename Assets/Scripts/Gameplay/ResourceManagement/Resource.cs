using UnityEngine;

public class Resource : MonoBehaviour
{
    [SerializeField] int amount;
    [SerializeField] ResourceType resourceType;

    public void Consume()
    {
        if (resourceType != null && ResourceManager.Instance != null) ResourceManager.Instance.AddResource(resourceType, amount);
        Destroy(gameObject);
    }
}
