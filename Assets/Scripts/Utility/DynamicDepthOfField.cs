using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Assets.Scripts.Utility;

public class DynamicDepthOfField : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Volume volume;
    [SerializeField] PlayerRaycaster raycaster;
    DepthOfField depthOfField;
    [Header("Config")]
    [SerializeField] [Min(0.1f)] float maxRaycastDistance = 20.0f;
    [SerializeField] [Min(0.0f)] float distanceMultiplier = 1.15f;
    [SerializeField] SmoothedValueConfig smoothConfig = new() { MaxSmoothSpeed = Mathf.Infinity, SmoothTime = 0.4f };
    SmoothedFloat smoothFocusDistance;
    float initialFocusDistance;

    void OnEnable()
    {
        smoothFocusDistance = new SmoothedFloat(smoothConfig);

        if (volume != null)
        {
            if (volume.profile.TryGet(out depthOfField))
            {
                smoothFocusDistance.SmoothedValue = smoothFocusDistance.TargetValue = depthOfField.focusDistance.value;
                initialFocusDistance = depthOfField.focusDistance.value;
            }
        }
    }

    void LateUpdate()
    {
        if (depthOfField == null || raycaster == null) return;

        var raycast = raycaster.CustomRaycastForward(maxRaycastDistance);
        smoothFocusDistance.TargetValue = raycast.Hit ? raycast.RaycastHit.distance * distanceMultiplier : initialFocusDistance;

        smoothFocusDistance.Smooth(Time.deltaTime);
        depthOfField.focusDistance.value = smoothFocusDistance.SmoothedValue;
    }
}
