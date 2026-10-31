using UnityEngine;

public class TreeLogComponent : TreeComponent
{
    [SerializeField] int numToSpawn = 3;
    [SerializeField] TreeComponent smallerPrefab;
    [Space]
    [SerializeField] Transform treeBase;
    [SerializeField] Transform treeTip;
    public override void FullyChopped()
    {
        for (int i = 0; i < numToSpawn; i++)
        {
            var obj = Instantiate(smallerPrefab, Vector3.Lerp(treeBase.position, treeTip.position, (float)i / numToSpawn), transform.rotation);
        }

        Destroy(gameObject);
    }
}
