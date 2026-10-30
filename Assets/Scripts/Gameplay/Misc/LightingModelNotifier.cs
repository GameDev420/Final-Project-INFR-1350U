using UnityEngine;

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


    void Awake()
    {
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
    }

    public void ToggleFloat(int ToggleIndex)
    {
        if (ToggleIndex >= toggles.Length || string.IsNullOrEmpty(toggles[ToggleIndex].FloatName)) return;

        toggles[ToggleIndex].FloatValue = toggles[ToggleIndex].FloatValue == 1 ? 0 : 1;

        if (onModelChanged != null) onModelChanged.Invoke(toggles[ToggleIndex]);
    }

    public static void SubscribeToModelChange(ModelChangeHandler action)
    {
        if (instance != null && action != null) instance.onModelChanged += action;
    }
    public static void UnsubscribeToModelChange(ModelChangeHandler action)
    {
        if (instance != null && instance.onModelChanged != null && action != null) instance.onModelChanged -= action;
    }
}
