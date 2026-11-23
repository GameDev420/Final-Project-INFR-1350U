using UnityEngine;

[CreateAssetMenu(fileName = "ResourceType", menuName = "Scriptable Objects/ResourceType")]
public class ResourceType : ScriptableObject
{
    public string resourceName;
    public Sprite resourceIcon;
    [TextArea(2, 4)] public string resourceDescription;
    [Space]
    public string resourceAmountPrefix;
    public string resourceAmountSuffix;

    public string GetFormattedAmount(int amount)
    {
        return $"{resourceAmountPrefix}{amount}{resourceAmountSuffix}";
    }
}
