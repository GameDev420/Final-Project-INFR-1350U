using UnityEngine;

public class TreeLogComponent : ResourceComponent
{
    [SerializeField] int numToSpawn = 3;
    [SerializeField] ResourceComponent smallerPrefab;
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
