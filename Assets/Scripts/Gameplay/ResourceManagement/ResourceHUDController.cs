using UnityEngine;
using TMPro;

[DefaultExecutionOrder(1)]
public class ResourceHUDController : MonoBehaviour
{
    [SerializeField] string woodPrefix = "Wood: ";
    [SerializeField] TMP_Text woodText;
    [Space]
    [SerializeField] string moneyPrefix = "$";
    [SerializeField] TMP_Text moneyText;
    [Space]
    [SerializeField] string stonePrefix = "Stone: ";
    [SerializeField] TMP_Text stoneText;

    private void OnEnable()
    {
        var rm = ResourceManager.Instance;
        if (rm != null)
        {
            rm.onWoodChanged.AddListener(OnWoodChanged);
            rm.onMoneyChanged.AddListener(OnMoneyChanged);
            rm.onStoneChanged.AddListener(OnStoneChanged);
        }

        OnWoodChanged(0);
        OnMoneyChanged(0);
        OnStoneChanged(0);
    }
    private void OnDisable()
    {
        var rm = ResourceManager.Instance;
        if (rm != null)
        {
            rm.onWoodChanged.RemoveListener(OnWoodChanged);
            rm.onMoneyChanged.RemoveListener(OnMoneyChanged);
            rm.onStoneChanged.RemoveListener(OnStoneChanged);
        }
    }

    void OnWoodChanged(int wood)
    {
        woodText.text = woodPrefix + wood;
    }
    void OnMoneyChanged(int money)
    {
        moneyText.text = moneyPrefix + money;
    }
    void OnStoneChanged(int stone)
    {
        stoneText.text = stonePrefix + stone;
    }
}
