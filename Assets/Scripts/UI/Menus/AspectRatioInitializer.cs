using UnityEngine;

/// <summary>
/// Ensures the game's resolution fits the desired aspect ratio upon starting application.
/// Also serves the purpose of applying saved settings on game startup, since I'm too lazy to make a dedicated class for that.
/// </summary>
public class AspectRatioInitializer : MonoBehaviour
{
    public static AspectRatioInitializer Instance;

    [SerializeField] private SFXInfo[] savedMixerGroupReferences;

    private void Awake()
    {
        if (Application.isEditor) return;

        if (Instance != null)
        {
            StopAllCoroutines();
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        var savedRes = new Resolution()
        {
            width = PlayerPrefs.GetInt(SettingsController.WidthPlayerPrefsKey, SettingsController.UnsavedIntValue),
            height = PlayerPrefs.GetInt(SettingsController.HeightPlayerPrefsKey, SettingsController.UnsavedIntValue),
            refreshRateRatio = new RefreshRate()
            {
                numerator = (uint)PlayerPrefs.GetInt(SettingsController.RefreshRateNumeratorPlayerPrefsKey, SettingsController.UnsavedIntValue),
                denominator = (uint)PlayerPrefs.GetInt(SettingsController.RefreshRateDenominatorPlayerPrefsKey, SettingsController.UnsavedIntValue)
            }
        };

        var savedFullScreenMode = PlayerPrefs.GetInt(SettingsController.FullScreenModePlayerPrefsKey, SettingsController.UnsavedIntValue);

        // Log error to console if neither the saved resolution and available resolutions could be applied.
        if (!TryApplyResolution(savedRes, savedFullScreenMode == SettingsController.UnsavedIntValue ? Screen.fullScreenMode : (FullScreenMode)savedFullScreenMode, true) && !AspectRatioUtil.TryForceAspectRatio())
            Debug.LogError("Correct aspect ratio could not be applied. Some objects may not be fully visible.", this);
    }

    private bool IsResolutionAvailable(Resolution res)
    {
        // First, ensure resolution is valid (all positive values).
        if (res.width < 0 || res.height < 0 || res.refreshRateRatio.numerator <= 0 || res.refreshRateRatio.denominator <= 0)
        {
            Debug.LogError("The given resolution is numerically invalid (negative dimensions and/or aspect ratio fraction).", this);
            return false;
        }

        // Then loop through each available screen resolution to check if it can be applied.
        for (int i = 0; i < Screen.resolutions.Length; i++)
            if (AspectRatioUtil.CompareResolution(res, Screen.resolutions[i]))
                return true;

        return false;
    }
    private bool TryApplyResolution(Resolution res, FullScreenMode mode, bool filterRes = true)
    {
        if ((filterRes && AspectRatioUtil.FilterResolution(res)) || !IsResolutionAvailable(res))
        {
            Debug.LogError($"Could not apply the given resolution ({GetResString(res)}) because it was{(filterRes ? " either filtered out," : string.Empty)} invalid or unavailable on this system.");
            return false;
        }

        Screen.SetResolution(res.width, res.height, AspectRatioUtil.GetAllowedFullScreenMode(mode), res.refreshRateRatio);
        return true;
    }

    public static string GetResString(Resolution res) => $"{res.width}x{res.height} of aspect {AspectRatioUtil.GetAspectRatio(res):F2}";
}