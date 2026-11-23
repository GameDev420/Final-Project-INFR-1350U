using UnityEngine;

public static class Utility
{
    public static void SetCursorState(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }

    public static bool IsInLayerMask(int layer, LayerMask layermask)
    {
        return (layermask.value & (1 << layer)) != 0;
    }
}
