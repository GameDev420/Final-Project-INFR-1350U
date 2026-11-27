using UnityEngine;
using System.Collections.Generic;

// Just realized while writing this that we could also just add an enum for which lighting model to use in the shader...
// maybe we can do that later, instead, because this could definitely be tedious as scope increases.
public class LightingSwitcher : MonoBehaviour
{
    // Not the greatest solution, but I'm lazy
    const string OutlineToggleFloatName = "_UseOutline";

    [SerializeField] MeshRenderer[] renderers;
    [SerializeField] string[] exceptions = new string[0];
    float[] initOutlineValues = new float[0];
    readonly Dictionary<string, float> initialMaterialValues = new Dictionary<string, float>();

    private void OnValidate()
    {
        if ((renderers == null || renderers.Length == 0) && TryGetComponent(out MeshRenderer r))
        {
            renderers = new MeshRenderer[1];
            renderers[0] = r;
        }
    }

    private void Awake()
    {
        initialMaterialValues.Clear();

        if (renderers != null && renderers.Length > 0)
        {
            initOutlineValues = new float[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r != null && r.material != null && r.material.HasFloat(OutlineToggleFloatName))
                    initOutlineValues[i] = r.material.GetFloat(OutlineToggleFloatName);
            }
        }
    }

    void OnEnable() => LightingModelNotifier.SubscribeToModelChange(OnLightChanged);
    void OnDisable() => LightingModelNotifier.UnsubscribeToModelChange(OnLightChanged);

    void OnDestroy()
    {
        // Restore initial shader values
        // Not sure how much this matters since Unity instantiates a new material instance for each renderer, but just in case.
        if (renderers != null && renderers.Length > 0)
        {
            foreach (var kvp in initialMaterialValues)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    var r = renderers[i];
                    if (r != null && r.material != null && r.material.HasFloat(kvp.Key))
                    {
                        r.material.SetFloat(kvp.Key, kvp.Value);
                    }
                }
            }
        }
    }

    bool IsException(string floatName)
    {
        if (exceptions == null || exceptions.Length == 0) return false;
        for (int i = 0; i < exceptions.Length; i++)
            if (exceptions[i] == floatName)
                return true;
        return false;
    }

    void OnLightChanged(LightingModelNotifier.ToggleData data)
    {
        if (IsException(data.FloatName)) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r != null && r.material != null && r.material.HasFloat(data.FloatName))
            {
                if (!initialMaterialValues.ContainsKey(data.FloatName)) initialMaterialValues.Add(data.FloatName, r.material.GetFloat(data.FloatName));

                r.material.SetFloat(data.FloatName, data.FloatValue);
            }
        }
    }

    public void ChangeLightToggle(LightingModelNotifier.ToggleData data) => OnLightChanged(data);

    public void EnableOutline()
    {
        if (IsException(OutlineToggleFloatName)) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (i >= initOutlineValues.Length || initOutlineValues[i] == 1.0f) continue;

            var r = renderers[i];
            if (r != null && r.material != null && r.material.HasFloat(OutlineToggleFloatName))
            {
                float currentValue = r.material.GetFloat(OutlineToggleFloatName);
                r.material.SetFloat(OutlineToggleFloatName, currentValue == 0.0f ? 1.0f : 0.0f);

                if (!initialMaterialValues.ContainsKey(OutlineToggleFloatName)) initialMaterialValues.Add(OutlineToggleFloatName, currentValue);
            }
        }
    }
    public void DisableOutline()
    {
        if (IsException(OutlineToggleFloatName)) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r != null && r.material != null && r.material.HasFloat(OutlineToggleFloatName))
            {
                float currentValue = r.material.GetFloat(OutlineToggleFloatName);
                r.material.SetFloat(OutlineToggleFloatName, currentValue == 0.0f ? 1.0f : 0.0f);
            }
        }
    }
}
