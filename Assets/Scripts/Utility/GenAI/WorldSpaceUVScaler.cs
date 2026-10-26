using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WorldSpaceUVScaler : MonoBehaviour
{
    public float uvScale = 1.0f; // Multiplier for texture tiling in world space
    public Vector3 uvPlaneAxis = new Vector3(1, 0, 1); // Which axes to project (e.g., X and Z)

    void Start()
    {
        ApplyWorldSpaceUVs();
    }

    [ContextMenu("Apply World Space UVs")]
    public void ApplyWorldSpaceUVs()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            // Transform vertex position from local space to world space
            Vector3 worldVertex = transform.TransformPoint(vertices[i]);

            // Project world coordinates onto 2D UV space based on selected axes
            uvs[i] = new Vector2(
                worldVertex.x * uvPlaneAxis.x + worldVertex.y * uvPlaneAxis.y,
                worldVertex.z * uvPlaneAxis.z + worldVertex.y * uvPlaneAxis.y
            ) * uvScale;
        }

        mesh.uv = uvs;
    }
}