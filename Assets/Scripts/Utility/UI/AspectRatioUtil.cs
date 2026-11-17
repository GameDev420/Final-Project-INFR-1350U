using UnityEngine;

// Brief context: after writing over 70 lines, I just remembered the object.Equals method exists... forgive me please.
public static class AspectRatioUtil
{
    // Half-baked solution to only allow 16:9 aspect ratios
    public const int HorizontalAspect = 16, VerticalAspect = 9;
    public const FullScreenMode DefaultFullScreenMode = FullScreenMode.ExclusiveFullScreen;
    public static readonly FullScreenMode[] AllowedFullScreenModes =
    {
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.Windowed
    };

    /// <summary>
    /// Attempts to force the application window.
    /// </summary>
    /// <returns>Returns whether the resolution was changed to (or already in) the correct aspect ratio.</returns>
    private static bool TryForceAspectRatio(int ratioWidth, int ratioHeight, FullScreenMode forcedFullScreenMode)
    {
        var desiredRatio = GetAspectRatio(ratioWidth, ratioHeight);

        // Return if already in correct aspect ratio.
        if (!FilterResolution(Screen.width, Screen.height, desiredRatio)) return true;

        // Loop through all available resolutions (starting from the highest)
        for (int i = Screen.resolutions.Length - 1; i >= 0; i--)
        {
            var res = Screen.resolutions[i];

            if (FilterResolution(res, desiredRatio)) continue;

            // If resolution passes filtering, apply it and return.
            Screen.SetResolution(res.width, res.height, forcedFullScreenMode, res.refreshRateRatio);
            return true;
        }

        Debug.LogError("No aspect ratio could be forced from utils!");
        return false;
    }

    private static bool TryForceAspectRatio(int ratioWidth, int ratioHeight) => TryForceAspectRatio(ratioWidth, ratioHeight, GetAllowedFullScreenMode(Screen.fullScreenMode));

    // Public overloads only use the constant desired ratios, though this could be changed if used for future games.
    // (hence why the private methods exist for any ratio)
    public static bool TryForceAspectRatio() => TryForceAspectRatio(HorizontalAspect, VerticalAspect);
    public static bool TryForceAspectRatio(FullScreenMode desiredFullScreenMode, bool forceMode = false) => TryForceAspectRatio(HorizontalAspect, VerticalAspect, forceMode ? desiredFullScreenMode : GetAllowedFullScreenMode(desiredFullScreenMode));


    public static float GetAspectRatio(int width, int height) => (float)width / height;
    public static float GetAspectRatio(Resolution res) => GetAspectRatio(res.width, res.height);

    /// <summary>
    /// Returns true if the given resolution should be removed from the resolutions array and dropdown (currently only due to certain aspect ratios).
    /// </summary>
    public static bool FilterResolution(Resolution res) => FilterResolution(res, GetAspectRatio(HorizontalAspect, VerticalAspect));
    private static bool FilterResolution(int width, int height, float desiredRatio) => !CompareAspectRatio(GetAspectRatio(width, height), desiredRatio);

    // This is more of a micro-optimization for the ForceAspectRatio method, allowing the desired aspect ratio to only be calculated once while looping through system resolutions.
    private static bool FilterResolution(Resolution res, float desiredRatio) => !CompareAspectRatio(res, desiredRatio);

    // First method is always core/base logic
    // Method overloads below it are purely for convenience (and potentially easier debugging)

    /// <summary>Returns whether both of the given resolutions pixel dimensions equivalent.</summary>
    public static bool CompareResolutionPixels(int widthA, int heightA, int widthB, int heightB) => widthA == widthB && heightA == heightB;
    /// <summary>Returns whether both of the given resolutions pixel dimensions equivalent.</summary>
    public static bool CompareResolutionPixels(Resolution a, Resolution b) => CompareResolutionPixels(a.width, a.height, b.width, b.height);
    /// <summary>Returns whether both of the given resolutions pixel dimensions equivalent.</summary>
    public static bool CompareResolutionPixels(Resolution a, int widthB, int heightB) => CompareResolutionPixels(a.width, a.height, widthB, heightB);

    /// <summary>Returns whether the given resolution's defining values are equivalent, namely the resolution dimensions and refresh rate.</summary>
    public static bool CompareResolution(Resolution a, Resolution b) => CompareResolutionPixels(a, b) && CompareRefreshRate(a.refreshRateRatio, b.refreshRateRatio);

    public static bool CompareAspectRatio(float a, float b) => Mathf.Approximately(a, b);
    public static bool CompareAspectRatio(int widthA, int heightA, int widthB, int heightB) => CompareAspectRatio(GetAspectRatio(widthA, heightA), GetAspectRatio(widthB, heightB));
    public static bool CompareAspectRatio(Resolution a, Resolution b) => CompareAspectRatio(GetAspectRatio(a.width, a.height), GetAspectRatio(b.width, b.height));
    public static bool CompareAspectRatio(Resolution a, int widthB, int heightB) => CompareAspectRatio(GetAspectRatio(a.width, a.height), GetAspectRatio(widthB, heightB));
    public static bool CompareAspectRatio(Resolution a, float ratioB) => CompareAspectRatio(GetAspectRatio(a), ratioB);
    public static bool CompareAspectRatio(int widthA, int heightA, float ratioB) => CompareAspectRatio(GetAspectRatio(widthA, heightA), ratioB);

    public static bool CompareRefreshRate(RefreshRate a, RefreshRate b) => (a.numerator == b.numerator && a.denominator == b.denominator) || Mathf.Approximately((float)a.value, (float)b.value);

    public static Resolution GetEffectiveWindowResolution() => new() { width = Screen.width, height = Screen.height, refreshRateRatio = Screen.currentResolution.refreshRateRatio };

    public static bool IsFullScreenModeAllowed(FullScreenMode mode)
    {
        for (int i = 0; i < AllowedFullScreenModes.Length; i++)
        {
            if (AllowedFullScreenModes[i] == mode)
                return true;
        }

        return false;
    }

    /// <summary>If the given FullScreenMode is allowed, simply returns it back. Otherwise, the game's Default FullScreenMode will be returned.</summary>
    public static FullScreenMode GetAllowedFullScreenMode(FullScreenMode mode) => IsFullScreenModeAllowed(mode) ? mode : DefaultFullScreenMode;

    // The UnsavedIntValue check is kinda redundant, but may be of use if the default int value is positive.
#pragma warning disable CS0652 // Comparison to integral constant is useless; the constant is outside the range of the type
    public static bool IsRefreshRateValid(RefreshRate refreshRate)
        => refreshRate.numerator != 0 && refreshRate.numerator != SettingsController.UnsavedIntValue && refreshRate.denominator != 0 && refreshRate.denominator != SettingsController.UnsavedIntValue;
#pragma warning restore CS0652 // Comparison to integral constant is useless; the constant is outside the range of the type
}