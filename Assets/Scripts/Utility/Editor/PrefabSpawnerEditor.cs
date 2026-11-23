using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabSpawner))]
public class PrefabSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var spawner = (PrefabSpawner)target;

        if (spawner == null) return;

        GUILayout.Space(5.0f);
        if (GUILayout.Button(new GUIContent("Spawn")))
        {
            spawner.SpawnAll();
        }
    }
}
