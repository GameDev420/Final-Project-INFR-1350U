using UnityEngine;

public partial class TerrainGenerator
{
    [Header("Mesh")]
    [SerializeField] Vector2Int cells = new(10, 10);
    [SerializeField] Vector2 cellSize = new(1.0f, 1.0f);
    [SerializeField] float noiseHeight = 1.0f;
    [SerializeField] Vector2 noiseOffset = new(0.0f, 0.0f);
    [SerializeField] Vector2 noiseScale = new(1.0f, 1.0f);
    [Tooltip("Only works in playmode.")]
    [SerializeField] bool randomizeOffset = false;
    
    Mesh GetMesh()
    {
        var mesh = new Mesh
        {
            name = "CustomTerrain",
            // Required for over 65535 vertices, which may be the case since this is unoptimized for large cell counts
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        if (randomizeOffset && Application.isPlaying) noiseOffset = new Vector2(Random.Range(-10000.0f, 10000.0f), Random.Range(-10000.0f, 10000.0f));

        var vertices = new Vector3[cells.x * cells.y * 4];
        var normals = new Vector3[vertices.Length];
        var uvs = new Vector2[vertices.Length];
        var triangles = new int[cells.x * cells.y * 6];

        for (int i = 0; i < cells.x; i++)
        {
            for (int j = 0; j < cells.y; j++)
            {
                // Generate vertices for each cell
                var index = (i * cells.y + j) * 4;

                // TODO: reuse old vertices instead of generating new ones corners/edges for each cell
                vertices[index + 0] = new Vector3(i * cellSize.x, GetHeight(i * cellSize.x, j * cellSize.y), j * cellSize.y);
                vertices[index + 1] = new Vector3((i + 1) * cellSize.x, GetHeight((i + 1) * cellSize.x, j * cellSize.y), j * cellSize.y);
                vertices[index + 2] = new Vector3(i * cellSize.x, GetHeight(i * cellSize.x, (j + 1) * cellSize.y), (j + 1) * cellSize.y);
                vertices[index + 3] = new Vector3((i + 1) * cellSize.x, GetHeight((i + 1) * cellSize.x, (j + 1) * cellSize.y), (j + 1) * cellSize.y);

                // Generate triangles for each cell
                var triangleIndex = (i * cells.y + j) * 6;
                triangles[triangleIndex + 0] = index + 2;
                triangles[triangleIndex + 1] = index + 1;
                triangles[triangleIndex + 2] = index + 0;

                triangles[triangleIndex + 3] = index + 1;
                triangles[triangleIndex + 4] = index + 2;
                triangles[triangleIndex + 5] = index + 3;

                // Generate normals for each vertex
                var n = Vector3.Cross(vertices[index + 2] - vertices[index + 1], vertices[index + 3] - vertices[index + 1]).normalized;
                normals[index + 0] = n;
                normals[index + 1] = n;
                normals[index + 2] = n;
                normals[index + 3] = n;
            }
        }

        // Generate UVs for each vertex
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.normals = normals;

        mesh.RecalculateTangents(UnityEngine.Rendering.MeshUpdateFlags.Default);

        return mesh;
    }

    float GetHeight(float x, float z)
        => Mathf.PerlinNoise(x * noiseScale.x + noiseOffset.x, z * noiseScale.y + noiseOffset.y) * noiseHeight * 0.5f
        + Mathf.PerlinNoise(x * noiseScale.x + noiseOffset.x * 55.3f, z * noiseScale.y + noiseOffset.y * 92.2f) * noiseHeight * 0.5f;
}
