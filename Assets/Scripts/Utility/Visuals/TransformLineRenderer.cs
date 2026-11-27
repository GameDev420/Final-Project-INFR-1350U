using UnityEngine;

public class TransformLineRenderer : MonoBehaviour
{
    [System.Serializable]
    struct PointData
    {
        public Transform Point;
        public Vector3 GlobalOffset;
        public Vector3 LocalOffset;
    }

    [SerializeField] PointData[] points;
    [SerializeField] LineRenderer lineRenderer;
    Vector3[] positionCache;

    private void Awake()
    {
        lineRenderer.positionCount = points.Length;
        positionCache = new Vector3[points.Length];

        lineRenderer.useWorldSpace = true;
    }

    private void Update()
    {
        UpdateLine();
    }

    void UpdateLine()
    {
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            positionCache[i] = p.Point.TransformPoint(p.LocalOffset) + p.GlobalOffset;
        }

        lineRenderer.SetPositions(positionCache);
    }
}
