using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [System.Serializable]
    public struct LightFlickerConfig
    {
        [Header("Intensity")]
        public float MinIntensityMultiplier;
        public float MaxIntensityMultiplier;

        [Header("Noise")]
        public float NoiseSpeed;
        [Min(0.0f)] public float NoiseScale;
    }

    [Header("Config")]
    [SerializeField] bool independentRandomness = true;
    [SerializeField] LightFlickerConfig flickerConfig = new() { MinIntensityMultiplier = 0.7f, MaxIntensityMultiplier = 1.3f, NoiseSpeed = 1.0f, NoiseScale = 1.0f };
    
    [Header("Height Variation")]
    [SerializeField] float heightVariation = 0.0f;
    [SerializeField] float heightVariationSpeed = 1.0f;

    [Header("References")]
    [SerializeField] Light targetLight;
    float initIntensity;
    float initHeight;
    float time;

	public Light TargetLight => targetLight;
    public LightFlickerConfig FlickerConfig => flickerConfig;

    float randomOffset;
    float randomHeightOffset;

    private void Awake()
    {
        if (targetLight == null) targetLight = GetComponentInChildren<Light>();

        time = 0.0f;
        if (targetLight != null)
        {
            initIntensity = targetLight.intensity;
            initHeight = targetLight.transform.position.y;
        }
        else initHeight = transform.position.y;

        if (independentRandomness)
        {
            randomOffset = Random.Range(0.0f, 9999f);
            randomHeightOffset = Random.Range(0.0f, 9999f);
        }
    }

    private void Update()
    {
        time += Time.deltaTime * flickerConfig.NoiseSpeed;
        var noise = Mathf.PerlinNoise1D(time + randomOffset) * flickerConfig.NoiseScale;

        targetLight.intensity = initIntensity * Mathf.Lerp(flickerConfig.MinIntensityMultiplier, flickerConfig.MaxIntensityMultiplier, noise);

        if (heightVariation > 0.0f)
        {
            var heightOffset = Mathf.PerlinNoise1D(time * heightVariationSpeed + randomHeightOffset) * heightVariation;
            transform.position = new Vector3(transform.position.x, initHeight + heightOffset, transform.position.z);
        }
    }

    public void SetFlickerConfig(LightFlickerConfig config) => flickerConfig = config;
}
