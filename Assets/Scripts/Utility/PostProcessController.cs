using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// WIP controller for dynamic post processing at runtime.
/// Great for added juice and improved feedback for players.
/// </summary>
public class PostProcessController : MonoBehaviour
{
    #region Data Structures

    /*public interface IPostProcessChange
    {
        public uint Priority { get; }
        public bool UnscaledTime { get; }
        public float SpeedMultiplier { get; }
    }*/

    struct ChromaticAberrationChange : IEquatable<ChromaticAberrationChange>, IComparable<ChromaticAberrationChange>
    {
        public float NewIntensity;
        public uint Priority;
        public bool UnscaledTime;
        public float SpeedMultiplier;
        public ChromaticAberrationChange(float _NewIntensity, uint _Priority, bool _UnscaledTime = true, float _SpeedMultiplier = 1.0f)
        {
            NewIntensity = _NewIntensity;
            Priority = _Priority;
            UnscaledTime = _UnscaledTime;
            SpeedMultiplier = _SpeedMultiplier;
        }

        public readonly int CompareTo(ChromaticAberrationChange other) => Priority == other.Priority ? 0 : (Priority < other.Priority ? -1 : 1);

        public readonly bool Equals(ChromaticAberrationChange other) => NewIntensity == other.NewIntensity && Priority == other.Priority && UnscaledTime == other.UnscaledTime && SpeedMultiplier == other.SpeedMultiplier;
    }
    struct BloomChange : IEquatable<BloomChange>, IComparable<BloomChange>
    {
        public float NewIntensity;
        public uint Priority;
        public bool UnscaledTime;
        public float SpeedMultiplier;
        public BloomChange(float _NewIntensity, uint _Priority, bool _UnscaledTime = true, float _SpeedMultiplier = 1.0f)
        {
            NewIntensity = _NewIntensity;
            Priority = _Priority;
            UnscaledTime = _UnscaledTime;
            SpeedMultiplier = _SpeedMultiplier;
        }

        public readonly int CompareTo(BloomChange other) => Priority == other.Priority ? 0 : (Priority < other.Priority ? -1 : 1);

        public readonly bool Equals(BloomChange other) => NewIntensity == other.NewIntensity && Priority == other.Priority && UnscaledTime == other.UnscaledTime && SpeedMultiplier == other.SpeedMultiplier;
    }

    [Serializable]
	public struct Property<T>
	{
		public bool overrideProperty;
		public T value;
		
		public Property(bool OverrideProperty, T Value)
		{
			overrideProperty = OverrideProperty;
			value = Value;
		}
	}
    [Serializable]
	public struct ColorProperty
	{
		public bool overrideProperty;
		[ColorUsage(true, true)] public Color value;
		
		public ColorProperty(bool OverrideProperty, Color Value)
		{
			overrideProperty = OverrideProperty;
			value = Value;
		}
	}
    [Serializable]
    public struct ColorAdjustmentChange
    {
		public float speedMultiplier;
		[Space]
		public Property<float> postExposure;
		public Property<float> contrast;
		public ColorProperty colorFilter;
		public Property<float> hueShift;
		public Property<float> saturation;

        public ColorAdjustmentChange(float SpeedMultiplier, Property<float> PostExposure, Property<float> Contrast, ColorProperty ColorFilter, Property<float> HueShift, Property<float> Saturation)
		{
			speedMultiplier = SpeedMultiplier;
			postExposure = PostExposure;
			contrast = Contrast;
			colorFilter = ColorFilter;
			hueShift = HueShift;
			saturation = Saturation;
		}
    }

    #endregion

    #region Properties

    public static PostProcessController Instance;

    [SerializeField] Volume targetVolume;
    [Header("Chromatic Aberration")]
    [SerializeField] float chromaticAberrationSmoothTime = 0.2f;
    [SerializeField] float maxChromaticAberrationSmoothSpeed = Mathf.Infinity;

    [Header("Bloom")]
    [SerializeField] float bloomSmoothTime = 0.125f;
    [SerializeField] float maxBloomSmoothSpeed = Mathf.Infinity;

    [Header("Color Adjustment")]
    [SerializeField] float colorAdjustmentSmoothTime = 0.3f;
    [SerializeField] float maxColorAdjustmentSmoothSpeed = Mathf.Infinity;

    #endregion

    #region Private Fields

    Coroutine chromAberCoroutine, bloomCoroutine, colorAdjustmentCoroutine;

    // Chromatic Aberration
    ChromaticAberration chromAberEffect;
    float initChromAberIntensity;
    float chromAberVelocity;
    public float InitialChromaticAberrationIntensity => initChromAberIntensity;
    readonly List<ChromaticAberrationChange> chromAberChanges = new();

    // Bloom
    Bloom bloomEffect;
    float initBloomIntensity;
    float bloomVelocity;
    public float InitialBloomIntensity => initBloomIntensity;
    readonly List<BloomChange> bloomChanges = new();

    // Color Adjustment
    ColorAdjustments colorAdjustmentsEffect;
    ColorAdjustmentChange initColorAdjustments;
    float postExposureVelocity;
    float contrastVelocity;
    Color colorFilterVelocity;
    float hueShiftVelocity;
    float saturationVelocity;
    readonly List<ColorAdjustmentChange> colorAdjustmentsChanges = new();

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogError("Two PostProcessController's coexist.", this);

        targetVolume.profile.TryGet(out bloomEffect);
        targetVolume.profile.TryGet(out chromAberEffect);

        chromAberEffect.intensity.overrideState = true;
        initChromAberIntensity = chromAberEffect.intensity.GetValue<float>();

        bloomEffect.intensity.overrideState = true;
        initBloomIntensity = bloomEffect.intensity.GetValue<float>();
    }

    private void OnDestroy()
    {
        chromAberEffect.intensity.value = initChromAberIntensity;
        bloomEffect.intensity.value = initBloomIntensity;
    }

    private void Update()
    {
        // Chromatic Aberration
        var desiredChromAber = chromAberChanges.Count > 0 ? chromAberChanges[^1].NewIntensity : initChromAberIntensity;
        var chromAberUnscaledTime = chromAberChanges.Count == 0 || chromAberChanges[^1].UnscaledTime;
        var chromAberSpeedMultiplier = chromAberChanges.Count > 0 ? chromAberChanges[^1].SpeedMultiplier : 1.0f;
        chromAberEffect.intensity.value = Mathf.SmoothDamp(chromAberEffect.intensity.GetValue<float>(), desiredChromAber, ref chromAberVelocity, chromaticAberrationSmoothTime / chromAberSpeedMultiplier, maxChromaticAberrationSmoothSpeed, chromAberUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);

        // Bloom
        var desiredBloom = bloomChanges.Count > 0 ? bloomChanges[^1].NewIntensity : initBloomIntensity;
        var bloomUnscaledTime = bloomChanges.Count == 0 || bloomChanges[^1].UnscaledTime;
        var bloomSpeedMultiplier = bloomChanges.Count > 0 ? bloomChanges[^1].SpeedMultiplier : 1.0f;
        bloomEffect.intensity.value = Mathf.SmoothDamp(bloomEffect.intensity.GetValue<float>(), desiredBloom, ref bloomVelocity, bloomSmoothTime / bloomSpeedMultiplier, maxBloomSmoothSpeed, bloomUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
    
        // Color Adjustments
        if (colorAdjustmentsChanges.Count > 0)
        {
            var desiredColorAdjustments = colorAdjustmentsChanges[^1];
            var colorAdjustmentsUnscaledTime = desiredColorAdjustments.speedMultiplier == 0.0f || desiredColorAdjustments.speedMultiplier == Mathf.Infinity;
            var colorAdjustmentsSpeedMultiplier = desiredColorAdjustments.speedMultiplier;

            // All written by auto-complete AI (was gonna do it manually anyway, but it saved me time, however unoptimized this code structure is)
            if (desiredColorAdjustments.postExposure.overrideProperty)
                colorAdjustmentsEffect.postExposure.value = Mathf.SmoothDamp(colorAdjustmentsEffect.postExposure.GetValue<float>(), desiredColorAdjustments.postExposure.value, ref postExposureVelocity, colorAdjustmentSmoothTime / colorAdjustmentsSpeedMultiplier, maxColorAdjustmentSmoothSpeed, colorAdjustmentsUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            if (desiredColorAdjustments.contrast.overrideProperty)
                colorAdjustmentsEffect.contrast.value = Mathf.SmoothDamp(colorAdjustmentsEffect.contrast.GetValue<float>(), desiredColorAdjustments.contrast.value, ref contrastVelocity, colorAdjustmentSmoothTime / colorAdjustmentsSpeedMultiplier, maxColorAdjustmentSmoothSpeed, colorAdjustmentsUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            if (desiredColorAdjustments.colorFilter.overrideProperty)
                colorAdjustmentsEffect.colorFilter.value = Color.Lerp(colorAdjustmentsEffect.colorFilter.GetValue<Color>(), desiredColorAdjustments.colorFilter.value, (colorAdjustmentSmoothTime / colorAdjustmentsSpeedMultiplier) * (colorAdjustmentsUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));
            if (desiredColorAdjustments.hueShift.overrideProperty)
                colorAdjustmentsEffect.hueShift.value = Mathf.SmoothDamp(colorAdjustmentsEffect.hueShift.GetValue<float>(), desiredColorAdjustments.hueShift.value, ref hueShiftVelocity, colorAdjustmentSmoothTime / colorAdjustmentsSpeedMultiplier, maxColorAdjustmentSmoothSpeed, colorAdjustmentsUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            if (desiredColorAdjustments.saturation.overrideProperty)
                colorAdjustmentsEffect.saturation.value = Mathf.SmoothDamp(colorAdjustmentsEffect.saturation.GetValue<float>(), desiredColorAdjustments.saturation.value, ref saturationVelocity, colorAdjustmentSmoothTime / colorAdjustmentsSpeedMultiplier, maxColorAdjustmentSmoothSpeed, colorAdjustmentsUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }
    }

    #endregion


    #region Public Methods

    /*

    /// <summary>
    /// Note that the effect's default priority is -1, so anything higher will override it, so anything lower would have no effect.
    /// </summary>
    public void AddChromaticAbberationChange(float newIntensity, uint priority, bool unscaledTime = true, float speedMultiplier = 1.0f)
    {
        chromAberChanges.Add(new(newIntensity, priority, unscaledTime, speedMultiplier));
        if (chromAberChanges.Count > 1) chromAberChanges.Sort();
    }
    public void RemoveChromaticAbberationChange(float newIntensity, uint priority, bool unscaledTime = true, float speedMultiplier = 1.0f)
    {
        var change = new ChromaticAberrationChange(newIntensity, priority, unscaledTime, speedMultiplier);
        if (chromAberChanges.Contains(change)) chromAberChanges.Remove(change);
        if (chromAberChanges.Count > 1) chromAberChanges.Sort();
    }

    public void AddBloomChange(float newIntensity, uint priority, bool unscaledTime = true, float speedMultiplier = 1.0f)
    {
        bloomChanges.Add(new(newIntensity, priority, unscaledTime, speedMultiplier));
        if (bloomChanges.Count > 1) bloomChanges.Sort();
    }
    public void RemoveBloomChange(float newIntensity, uint priority, bool unscaledTime = true, float speedMultiplier = 1.0f)
    {
        var change = new BloomChange(newIntensity, priority, unscaledTime, speedMultiplier);
        if (bloomChanges.Contains(change)) bloomChanges.Remove(change);
        if (bloomChanges.Count > 1) bloomChanges.Sort();
    }

    */

    public void AddChromaticAberrationChange(float newIntensity, float duration, bool unscaledTime = true, float speedMultiplier = 1.0f)
    {
        if (chromAberCoroutine != null) StopCoroutine(chromAberCoroutine);
        chromAberCoroutine = StartCoroutine(PlayChromaticAberrationChange(newIntensity, duration, unscaledTime, speedMultiplier));
    }
    public void AddBloomChange(float newIntensity, float duration, bool unscaledTime = true, float speedMultiplier = 1.0f)
    {
        if (bloomCoroutine != null) StopCoroutine(bloomCoroutine);
        bloomCoroutine = StartCoroutine(PlayBloomChange(newIntensity, duration, unscaledTime, speedMultiplier));
    }
    public void AddColorAdjustmentChange(ColorAdjustmentChange change, float duration, bool unscaledTime = true)
    {
        if (colorAdjustmentCoroutine != null) StopCoroutine(colorAdjustmentCoroutine);
        colorAdjustmentCoroutine = StartCoroutine(PlayColorAdjustmentChange(change, duration, unscaledTime));
    }

    #endregion

    #region Coroutines

    IEnumerator PlayChromaticAberrationChange(float newIntensity, float duration, bool unscaledTime = true, float speedMultiplier = 1.0f)
    {
        chromAberChanges.Clear();

        var change = new ChromaticAberrationChange(newIntensity, 99, unscaledTime, speedMultiplier);
        chromAberChanges.Add(change);

        if (unscaledTime) yield return new WaitForSecondsRealtime(duration);
        else yield return new WaitForSeconds(duration);

        chromAberChanges.Remove(change);
    }

    IEnumerator PlayBloomChange(float newIntensity, float duration, bool unscaledTime = true, float speedMultiplier = 1.0f)
    {
        bloomChanges.Clear();

        var change = new BloomChange(newIntensity, 99, unscaledTime, speedMultiplier);
        bloomChanges.Add(change);

        if (unscaledTime) yield return new WaitForSecondsRealtime(duration);
        else yield return new WaitForSeconds(duration);

        bloomChanges.Remove(change);
    }

    IEnumerator PlayColorAdjustmentChange(ColorAdjustmentChange change, float duration, bool unscaledTime)
    {
        colorAdjustmentsChanges.Clear();
        colorAdjustmentsChanges.Add(change);

        if (unscaledTime) yield return new WaitForSecondsRealtime(duration);
        else yield return new WaitForSeconds(duration);

        colorAdjustmentsChanges.Remove(change);
    }

    #endregion
}