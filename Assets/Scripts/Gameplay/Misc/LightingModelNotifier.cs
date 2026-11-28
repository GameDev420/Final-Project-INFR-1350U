using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-5)]
public class LightingModelNotifier : MonoBehaviour
{
    [System.Serializable]
    public struct ToggleData
    {
        public string FloatName;
        public float FloatValue;

        public ToggleData(string _FloatName, float _FloatValue)
        {
            FloatName = _FloatName;
            FloatValue = _FloatValue;
        }
    }

    // Singleton
    static LightingModelNotifier instance;

    // Model Change Event
    public delegate void ModelChangeHandler(ToggleData data);
    ModelChangeHandler onModelChanged;

    // Serialized Parameters
    [SerializeField] ToggleData[] toggles = new ToggleData[5];

    // Not sure how necessary this is, but hopefully it helps.
    readonly Dictionary<string, float> initialGlobalShaderValues = new();


    void Awake()
    {
        initialGlobalShaderValues.Clear();

        for (int i = 0; i < toggles.Length; i++)
        {
            if (!string.IsNullOrEmpty(toggles[i].FloatName) && !initialGlobalShaderValues.ContainsKey(toggles[i].FloatName))
            {
                initialGlobalShaderValues.Add(toggles[i].FloatName, toggles[i].FloatValue);
            }
        }

        if (instance == null) instance = this;
        else Debug.LogWarning("Two LightingModelNotifiers coexist.", this);
    }

    private void Start()
    {
        if (onModelChanged == null) return;

        for (int i = 0; i < toggles.Length; i++)
            if (!string.IsNullOrEmpty(toggles[i].FloatName))
                onModelChanged.Invoke(toggles[i]);
    }

    void OnDestroy()
    {
        // Make sure to remove any remaining subscribers
        if (instance == this) onModelChanged = null;

        // Restore initial global shader values
        foreach (var kvp in initialGlobalShaderValues)
        {
            Shader.SetGlobalFloat(kvp.Key, kvp.Value);
        }
        initialGlobalShaderValues.Clear();
    }

    public void ToggleFloat(int ToggleIndex)
    {
        if (ToggleIndex >= toggles.Length || string.IsNullOrEmpty(toggles[ToggleIndex].FloatName)) return;

        toggles[ToggleIndex].FloatValue = toggles[ToggleIndex].FloatValue == 1 ? 0 : 1;

        if (onModelChanged != null) onModelChanged.Invoke(toggles[ToggleIndex]);

        // Used for grass, since it uses global shader properties (because scripts aren't allowed on terrain grass)
        Shader.SetGlobalFloat(toggles[ToggleIndex].FloatName, toggles[ToggleIndex].FloatValue);
    }

    public static void SubscribeToModelChange(ModelChangeHandler action)
    {
        if (instance != null && action != null) instance.onModelChanged += action;
    }
    public static void UnsubscribeToModelChange(ModelChangeHandler action)
    {
        if (instance != null && instance.onModelChanged != null && action != null) instance.onModelChanged -= action;
    }


    // Only use as a last resort - not dangerous, just not ideal
    [ContextMenu("Reset to Shader Global Defaults")]
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    static void ResetToGlobalDefaults()
    {
        Shader.SetGlobalFloat("_UseAlbedo", 1.0f);
        Shader.SetGlobalFloat("_UseNormal", 1.0f);
        Shader.SetGlobalFloat("_UseLambert", 1.0f);
        Shader.SetGlobalFloat("_UseAmbient", 1.0f);
        Shader.SetGlobalFloat("_UseSpecular", 1.0f);
        Shader.SetGlobalFloat("_UseShadows", 1.0f);
        Shader.SetGlobalFloat("_UseFog", 1.0f);
        Shader.SetGlobalFloat("_UseAdditionalLights", 1.0f);
    }
}
