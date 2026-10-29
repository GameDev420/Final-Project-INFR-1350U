using UnityEngine;

public class LightingModelNotifier : MonoBehaviour
{
    static LightingModelNotifier instance;

    public delegate void ModelChangeHandler(int modelIndex);
    ModelChangeHandler onModelChanged;

    [SerializeField] int defaultModelIndex = 0;

    int modelIndex;
    public int ModelIndex => modelIndex;

    void Awake()
    {
        if (instance == null) instance = this;
        else Debug.LogWarning("Two LightingModelNotifiers coexist.", this);

        modelIndex = defaultModelIndex;
    }

    void OnDestroy()
    {
        if (instance == this) onModelChanged = null;
    }

    public void ChangeLightingModel(int ModelIndex)
    {
        modelIndex = ModelIndex;

        if (onModelChanged != null) onModelChanged.Invoke(ModelIndex);
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
