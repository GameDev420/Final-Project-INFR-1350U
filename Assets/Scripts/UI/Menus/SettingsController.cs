using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class SettingsController : MonoBehaviour
{
    #region Data Structures

    [System.Serializable]
    public struct AudioMixerControlData
    {
        [Tooltip("Has no effect on the game, only exists for better organization in the inspector.")]
        public string name;
        public Slider volumeSlider;
        public SFXInfo mixerGroupReference;

        public UnityAction<float> volumeChangedAction;
    }

    #endregion

    #region Variables

    public const string WidthPlayerPrefsKey = "", HeightPlayerPrefsKey = "",
        RefreshRateNumeratorPlayerPrefsKey = "", RefreshRateDenominatorPlayerPrefsKey = "",
        FullScreenModePlayerPrefsKey = "";

    public static string[] VolumePlayerPrefsKeys = { "MasterVolume", "", "" };

    /// <summary>
    /// Note that this is exclusively for resolution/settings menu related data and should NOT be used for anything else retrieved from PlayerPrefs.
    /// </summary>
    public const int UnsavedIntValue = -1;
    public const float DefaultVolumeValue = 0.75f;

    [Header("Volume")]
    [SerializeField] private AudioMixerControlData[] mixerControlData;
    [Header("Resolution")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [Header("Window Options")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown fullscreenModeDropdown;

    private Resolution[] resolutions;

    #endregion

    #region MonoBehaviour Methods

    private void OnEnable()
    {
        UpdateResolutions(out var resIndex);

        for (int i = 0; i < mixerControlData.Length; i++)
        {
            var data = mixerControlData[i];

            // Same as assigning/initializing value if it's null according to VS
            data.volumeChangedAction ??= (value) => OnVolumeChanged(value, data);

            if (data.volumeSlider != null)
            {
                // Initialize mixer volume
                if (i < VolumePlayerPrefsKeys.Length) data.volumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(VolumePlayerPrefsKeys[i], DefaultVolumeValue));
                else data.volumeSlider.SetValueWithoutNotify(DefaultVolumeValue);
                OnVolumeChanged(data.volumeSlider.value, data);

                // Subscribe to callback
                if (data.volumeSlider.onValueChanged != null && data.volumeChangedAction != null)
                    data.volumeSlider.onValueChanged.AddListener(data.volumeChangedAction);
            }
        }

        var currentRes = AspectRatioUtil.GetEffectiveWindowResolution();
        var currentResIndex = -1;
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (AspectRatioUtil.CompareResolution(currentRes, Screen.resolutions[i]))
            {
                currentResIndex = i;
                break;
            }
        }

        if (currentResIndex == -1 || resolutions.Length <= currentResIndex || !AspectRatioUtil.CompareResolution(resolutions[currentResIndex], currentRes)) UpdateResolutions(out currentResIndex);
        resolutionDropdown.SetValueWithoutNotify(ResIndexToDropdownIndex(currentResIndex));
        fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        fullscreenModeDropdown.SetValueWithoutNotify((int)Screen.fullScreenMode);

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        // Initialize fullscreen toggle
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);

        // Initialize fullscreen mode dropdown
        fullscreenModeDropdown.onValueChanged.AddListener(OnFullscreenModeChanged);
        fullscreenModeDropdown.options.Add(new TMP_Dropdown.OptionData("Exclusive Fullscreen"));
        fullscreenModeDropdown.options.Add(new TMP_Dropdown.OptionData("Fullscreen Window"));
        fullscreenModeDropdown.options.Add(new TMP_Dropdown.OptionData("Maximized Window (macOS)"));
        fullscreenModeDropdown.options.Add(new TMP_Dropdown.OptionData("Windowed"));
    }

    private void OnDisable()
    {
        foreach (var data in mixerControlData)
            if (data.volumeSlider != null && data.volumeSlider.onValueChanged != null && data.volumeChangedAction != null)
                data.volumeSlider.onValueChanged.RemoveListener(data.volumeChangedAction);


        if (resolutionDropdown != null && resolutionDropdown.onValueChanged != null) resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        if (fullscreenToggle != null && fullscreenToggle.onValueChanged != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggleChanged);
        if (fullscreenModeDropdown != null && fullscreenModeDropdown.onValueChanged != null) fullscreenModeDropdown.onValueChanged.RemoveListener(OnFullscreenModeChanged);
    }

    private void OnDestroy() => SaveSettingPreferences();

    #endregion

    #region UI Callback Methods

    private void OnVolumeChanged(float value, AudioMixerControlData data)
    {
        // Also got this formula online
        var volumeInDb = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
        data.mixerGroupReference.mixerGroup.audioMixer.SetFloat(data.mixerGroupReference.volumeParameterName, volumeInDb);
    }

    private void OnResolutionChanged(int value)
    {
        // Invert value to compensate for dropdown being in reverse order (shows higher resolutions first)
        var res = resolutions[ResIndexToDropdownIndex(value)];

        Screen.SetResolution(res.width, res.height, (FullScreenMode)fullscreenModeDropdown.value, res.refreshRateRatio);
    }

    private void OnFullscreenToggleChanged(bool value)
    {
        var res = resolutions[resolutionDropdown.value];
        Screen.SetResolution(res.width, res.height, value);
    }

    private void OnFullscreenModeChanged(int value)
    {
        Screen.fullScreenMode = (FullScreenMode)value;
        var isFullscreen = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen || Screen.fullScreenMode == FullScreenMode.FullScreenWindow;

        // Update fullscreen toggle visual (i.e. without triggering callback)
        fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);

        var res = resolutions[ResIndexToDropdownIndex(resolutionDropdown.value)];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
    }

    #endregion

    // Works since dropdown is in reverse order as normal resolutions array.
    // Should also work in reverse since both are the same size.
    private int ResIndexToDropdownIndex(int index) => (resolutions.Length - 1) - index;

    private void UpdateResolutions(out int currentResIndex)
    {
        currentResIndex = -1;

        // Initialize resolution dropdown
        resolutions = Screen.resolutions;

        var currentRes = AspectRatioUtil.GetEffectiveWindowResolution();

        // Got this from Unity Forums
        // https://discussions.unity.com/t/resolution-dropdown-shows-same-resolutions/772595
        for (int i = resolutions.Length - 1; i >= 0; i--) // reverse loop to get higher resolutions at the top
        {
            var res = resolutions[i];
            if (AspectRatioUtil.FilterResolution(res)) continue;

            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(res.ToString()));

            if (currentResIndex == -1 && AspectRatioUtil.CompareResolution(res, currentRes)) currentResIndex = i;
        }
    }

    private void SaveSettingPreferences()
    {
        // Store volume
        for (int i = 0; i < mixerControlData.Length; i++)
        {
            if (i >= VolumePlayerPrefsKeys.Length)
            {
                Debug.LogWarning($"The AudioMixerControlData with index: {i} could not save its volume setting because a PlayerPrefs key doesn't exist for this index.", this);
                continue;
            }

            PlayerPrefs.SetFloat(VolumePlayerPrefsKeys[i], mixerControlData[i].volumeSlider.value);
        }

        // Store resolution
        PlayerPrefs.SetInt(WidthPlayerPrefsKey, Screen.width);
        PlayerPrefs.SetInt(HeightPlayerPrefsKey, Screen.height);

        // Store Refresh Rate
        var numeratorInt = (int)Screen.currentResolution.refreshRateRatio.numerator;
        var denomInt = (int)Screen.currentResolution.refreshRateRatio.denominator;

        // Only save refresh rate if valid uints (should always be the case)
        if (numeratorInt == Screen.currentResolution.refreshRateRatio.numerator && denomInt == Screen.currentResolution.refreshRateRatio.denominator)
        {
            PlayerPrefs.SetInt(RefreshRateNumeratorPlayerPrefsKey, numeratorInt);
            PlayerPrefs.SetInt(RefreshRateDenominatorPlayerPrefsKey, denomInt);
        }

        // Store Full Screen Mode
        PlayerPrefs.SetInt(FullScreenModePlayerPrefsKey, (int)Screen.fullScreenMode);

        // Write preferences to disk
        PlayerPrefs.Save();
    }

    public static void GetSavedSettings(out int windowWidth, out int windowHeight, out RefreshRate refreshRate, out FullScreenMode fullScreenMode, out float[] mixerGroupVolumes)
    {
        windowWidth = PlayerPrefs.GetInt(WidthPlayerPrefsKey, UnsavedIntValue);
        windowHeight = PlayerPrefs.GetInt(HeightPlayerPrefsKey, UnsavedIntValue);
        refreshRate = new()
        {
            numerator = (uint)PlayerPrefs.GetInt(RefreshRateNumeratorPlayerPrefsKey, UnsavedIntValue),
            denominator = (uint)PlayerPrefs.GetInt(RefreshRateDenominatorPlayerPrefsKey, UnsavedIntValue)
        };

        fullScreenMode = (FullScreenMode)PlayerPrefs.GetInt(FullScreenModePlayerPrefsKey, UnsavedIntValue);

        mixerGroupVolumes = new float[VolumePlayerPrefsKeys.Length];
        for (int i = 0; i < VolumePlayerPrefsKeys.Length; i++)
            mixerGroupVolumes[i] = PlayerPrefs.GetFloat(VolumePlayerPrefsKeys[i], DefaultVolumeValue);
    }
}