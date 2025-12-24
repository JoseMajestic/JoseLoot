using UnityEngine;
using TMPro;

public class MoneyHUD : MonoBehaviour
{
    [SerializeField] private PlayerMoney playerMoney;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private string moneyFormat = "{0}";
    [SerializeField] private bool formatWithThousands = true;

    private void Awake()
    {
        if (playerMoney == null && GameDataManager.Instance != null)
        {
            playerMoney = GameDataManager.Instance.PlayerMoney;
        }

        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged += OnMoneyChanged;
            OnMoneyChanged(playerMoney.GetMoney());
        }
        else
        {
            Debug.LogError("MoneyHUD: PlayerMoney no asignado");
        }
    }

    private void OnDestroy()
    {
        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged -= OnMoneyChanged;
        }
    }

    private void OnMoneyChanged(int newAmount)
    {
        if (moneyText == null)
            return;

        string formatted = formatWithThousands ? newAmount.ToString("N0") : newAmount.ToString();
        moneyText.text = string.Format(moneyFormat, formatted);
    }
}
