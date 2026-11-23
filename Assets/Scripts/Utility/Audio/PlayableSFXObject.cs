using UnityEngine;

[CreateAssetMenu(fileName = "New SFX Object", menuName = "Scriptable Objects/SFX/Playable SFX Object")]
public class PlayableSFXObject : ScriptableObject
{
    public PlayableSFX SFX;

    private void OnEnable()
    {
        ResetSFX();
    }
    private void OnDisable()
    {
        ResetSFX();
    }

    private void OnValidate()
    {
        ResetSFX();
    }

    public void ResetSFX() // Lazy solution to the problem of SFX time since last played not resetting between play modes scene changes.
    {
        if (SFX != null) SFX.Reset();
    }

    public bool TryPlay(MonoBehaviour instigator, float delay = 0.0f)
    {
        if (SFX != null) return SFX.TryPlay(instigator, delay);
        return false;
    }
}
