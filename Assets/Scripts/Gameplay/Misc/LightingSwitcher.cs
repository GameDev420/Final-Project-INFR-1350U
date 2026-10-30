using UnityEngine;

// Just realized while writing this that we could also just add an enum for which lighting model to use in the shader...
// maybe we can do that later, instead, because this could definitely be tedious as scope increases.
public class LightingSwitcher : MonoBehaviour
{
    [SerializeField] MeshRenderer[] renderers;

    private void OnValidate()
    {
        if ((renderers == null || renderers.Length == 0) && TryGetComponent(out MeshRenderer r))
        {
            renderers = new MeshRenderer[1];
            renderers[0] = r;
        }
    }

    void OnEnable() => LightingModelNotifier.SubscribeToModelChange(OnLightChanged);
    void OnDisable() => LightingModelNotifier.UnsubscribeToModelChange(OnLightChanged);

    void OnLightChanged(LightingModelNotifier.ToggleData data)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r != null && r.material != null && r.material.HasFloat(data.FloatName)) r.material.SetFloat(data.FloatName, data.FloatValue);
        }
    }
}
