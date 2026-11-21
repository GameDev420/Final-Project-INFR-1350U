using UnityEngine;

public partial class TerrainGenerator
{
    [Header("Object Alignment")]
    [SerializeField] float addedHeight = 5.0f;
    [SerializeField] bool alignPosition = true;
    [SerializeField] bool alignRotation = true;
    [SerializeField] Transform player;
    [SerializeField] Transform[] objectsToAlign = new Transform[0];
    [Tooltip("Will align all children of the specified transforms.")]
    [SerializeField] Transform[] childrenToAlign = new Transform[0];

    void ValidateObjects()
    {
        // Raise all objects by addedHeight to account for height differences in the new terrain mesh
        if (Application.isPlaying) MoveObject(player);

        for (int i = 0; i < objectsToAlign.Length; i++)
            MoveObject(objectsToAlign[i]);

        for (int i = 0; i < childrenToAlign.Length; i++)
        {
            var parent = childrenToAlign[i];
            if (parent == null) continue;
            
            for (int j = 0; j < parent.childCount; j++)
                MoveObject(parent.GetChild(j));
        }

        // Resync transforms after moving objects (before performing alignment raycasts)
        // Also ensures transforms are only synced once, instead of once per object, which is more efficient
        Physics.SyncTransforms();

        // Align all objects to the new terrain mesh
        if (Application.isPlaying) AlignObject(player, true);

        // Honestly not sure why this works, but player will teleport upward otherwise
        Physics.SyncTransforms();

        for (int i = 0; i < objectsToAlign.Length; i++)
            AlignObject(objectsToAlign[i]);

        for (int i = 0; i < childrenToAlign.Length; i++)
        {
            var parent = childrenToAlign[i];
            if (parent == null) continue;

            for (int j = 0; j < parent.childCount; j++)
                AlignObject(parent.GetChild(j));
        }
    }

    void MoveObject(Transform obj)
    {
        if (obj == null) return;
        obj.position += Vector3.up * addedHeight;
    }

    void AlignObject(Transform obj, bool player = false)
    {
        if (obj == null) return;
        AlignToNormal.Align(obj, alignPosition, alignRotation && !player, false);
    }
}
