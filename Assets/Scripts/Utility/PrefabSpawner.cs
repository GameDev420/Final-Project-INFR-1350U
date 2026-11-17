#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField] int numToSpawn = 10;
    [Tooltip("Note that this is technically doubled, since the value can be either negative OR positive.")]
    [SerializeField] Vector3 spawnArea;
    [SerializeField] float maxSpawnDistance = Mathf.Infinity;
    [SerializeField] Vector3 spawnOffset;
    [SerializeField] LayerMask hitMask;
    [SerializeField] LayerMask acceptableMask;
    [SerializeField] GameObject prefab;
    [Space]
    [SerializeField] Transform prefabSpawnParent;

    [ContextMenu("Spawn")]
    public void SpawnAll()
    {
        for (int i = 0; i < numToSpawn; i++)
            SpawnSingle();
    }

    void SpawnSingle()
    {
        bool spawned = false;

        const int maxAttempts = 100;
        int attempts = 0;
        while (!spawned && attempts < maxAttempts)
        {
            attempts++;
            var randomOffset = new Vector3(Random.Range(-spawnArea.x, spawnArea.x), Random.Range(-spawnArea.y, spawnArea.y), Random.Range(-spawnArea.z, spawnArea.z));

            if (Physics.Raycast(transform.position + randomOffset, -transform.up, out var hit, maxSpawnDistance, hitMask) && (acceptableMask.value & (1 << hit.transform.gameObject.layer)) != 0)
            {
                var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, prefabSpawnParent);
                obj.transform.SetPositionAndRotation(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                obj.transform.position = obj.transform.TransformPoint(spawnOffset);

                spawned = true;
            }
        }

        if (!spawned) Debug.LogWarning($"Failed to spawn {prefab.name} after {maxAttempts} attempts.");
    }
}
#endif