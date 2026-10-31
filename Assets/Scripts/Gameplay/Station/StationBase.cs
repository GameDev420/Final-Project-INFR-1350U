using UnityEngine;

public class StationBase : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var resource = other.GetComponentInParent<Resource>();
        if (resource != null)
        {
            resource.Consume();
        }
    }
}
