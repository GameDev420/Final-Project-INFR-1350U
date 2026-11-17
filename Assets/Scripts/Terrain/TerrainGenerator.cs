using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter))]
public partial class TerrainGenerator : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] bool generatedOnValidate = false;
    
    private void OnValidate()
    {
        if (generatedOnValidate && Application.isEditor) Generate();
    }

    [ContextMenu("Generate")]
    void Generate()
    {
        var mesh = GetMesh();
        GetComponent<MeshFilter>().mesh = mesh;
        if (TryGetComponent(out MeshCollider meshCollider)) meshCollider.sharedMesh = mesh;

        ValidateObjects();
    }

    public void OnGenerateInput(InputAction.CallbackContext context)
    {
        if (context.performed) Generate();
    }
}
