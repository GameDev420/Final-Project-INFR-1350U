using UnityEngine;

public abstract class ResourceComponent : MonoBehaviour
{
    [SerializeField] int health;
    [SerializeField] ResourceType resourceType;
    public ResourceType ResourceType => resourceType;

    public bool IsDead => health <= 0;

    public virtual void Chop(int damage)
    {
        health -= damage;
        if (health <= 0)
            FullyChopped();
    }

    public abstract void FullyChopped();
}
