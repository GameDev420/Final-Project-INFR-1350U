using UnityEngine;

public class AlignToNormal : MonoBehaviour
{
    [SerializeField] bool position = true;
    [SerializeField] bool rotation = true;
    [SerializeField] bool localDown = false;

    [ContextMenu("Align to Surface")]
    public void Align()
    {
        if (!position && !rotation) return;

        var dir = localDown ? -transform.up : Vector3.down;
        
        if (Physics.Raycast(transform.position, dir, out var hit))
        {
            if (position) transform.position = hit.point;
            if (rotation) transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal);
        }
    }
}
