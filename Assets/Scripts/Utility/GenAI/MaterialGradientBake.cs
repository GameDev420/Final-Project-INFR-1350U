using UnityEngine;

[ExecuteInEditMode] // Keeps it updated in the scene view
public class MaterialGradientBake : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Gradient inspectorGradient;
    [SerializeField] private string texturePropertyName = "_GradientTex";
    [SerializeField] private int textureWidth = 256;

    private Texture2D generatedTexture;

    void OnValidate()
    {
        UpdateGradient();
    }

    void Start()
    {
        UpdateGradient();
    }

    void UpdateGradient()
    {
        if (targetRenderer == null || inspectorGradient == null) return;

        // Initialize texture if null
        if (generatedTexture == null)
        {
            generatedTexture = new Texture2D(textureWidth, 1, TextureFormat.RGBA32, false);
            generatedTexture.wrapMode = TextureWrapMode.Clamp;
        }

        // Bake the gradient into the texture pixels
        Color[] pixels = new Color[textureWidth];
        for (int i = 0; i < textureWidth; i++)
        {
            float time = (float)i / (textureWidth - 1);
            pixels[i] = inspectorGradient.Evaluate(time);
        }

        generatedTexture.SetPixels(pixels);
        generatedTexture.Apply();

        // Pass it to the Material Property Block or directly to the material
        targetRenderer.sharedMaterial.SetTexture(texturePropertyName, generatedTexture);
    }
}
