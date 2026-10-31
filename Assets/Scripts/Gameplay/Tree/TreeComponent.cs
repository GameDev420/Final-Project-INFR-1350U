using UnityEngine;

public abstract class TreeComponent : MonoBehaviour
{
    [SerializeField] int health;

    public bool IsDead => health <= 0;

    public Resource.ResourceType resourceType;

    public virtual void Chop(int damage)
    {
        health -= damage;
        if (health <= 0)
            FullyChopped();
    }

    public abstract void FullyChopped();
}
