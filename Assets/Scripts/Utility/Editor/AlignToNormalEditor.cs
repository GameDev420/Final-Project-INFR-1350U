using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AlignToNormal))]
[CanEditMultipleObjects]
public class AlignToNormalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(5.0f);

        if (GUILayout.Button(new GUIContent("Align", "Sets this object's position and rotation (if enabled) to the below point of collision and normal direction respectively.")))
        {
            if (targets != null && targets.Length > 1)
            {
                for (int i = 0; i < targets.Length; i++)
                    TryAlign(targets[i]);
            }
            else TryAlign(target);
        }
    }

    static void TryAlign(Object obj)
    {
        var component = obj as AlignToNormal;
        if (obj != null)
        {
            component.Align();
        }
    }
}
