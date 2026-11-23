using UnityEngine;

public static class LayerMaskExtensions
{
    public static bool IsInLayerMask(this LayerMask layerMask, int layer) => Utility.IsInLayerMask(layer, layerMask);
}
