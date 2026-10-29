using UnityEngine;

// Just realized while writing this that we could also just add an enum for which lighting model to use in the shader...
// maybe we can do that later, instead, because this could definitely be tedious as scope increases.
public class LightingSwitcher : MonoBehaviour
{
    [SerializeField] MeshRenderer[] renderers;
    [SerializeField] Material[] lightingModelMaterials;

    void OnEnable() => LightingModelNotifier.SubscribeToModelChange(OnLightChanged);
    void OnDisable() => LightingModelNotifier.UnsubscribeToModelChange(OnLightChanged);

    void OnLightChanged(int modelIndex)
    {
        if (modelIndex < lightingModelMaterials.Length)
        {
            var mat = lightingModelMaterials[modelIndex];

            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null)
                    renderers[i].sharedMaterial = mat;
        }
    }
}
