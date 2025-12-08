using UnityEngine;
using TMPro;

/// <summary>
/// Gestiona la UI de monedas del jugador.
/// Actualiza automáticamente el texto cuando cambian las monedas.
/// </summary>
public class MoneyUIManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al PlayerMoney")]
    [SerializeField] private PlayerMoney playerMoney;

    [Header("UI")]
    [Tooltip("Text o TextMeshPro donde se muestra el dinero (si es null, busca automáticamente)")]
    [SerializeField] private TextMeshProUGUI moneyText;

    [Tooltip("Texto de formato (ej: '{0} monedas' o '{0}'). {0} será reemplazado por el número")]
    [SerializeField] private string moneyFormat = "{0}";

    [Header("Configuración")]
    [Tooltip("Si es true, formatea el número con separadores de miles (ej: 1,000)")]
    [SerializeField] private bool formatWithThousands = true;

    private void Start()
    {
        // Buscar TextMeshPro si no está asignado (no depende de GameDataManager)
        if (moneyText == null)
        {
            moneyText = GetComponent<TextMeshProUGUI>();
            if (moneyText == null)
            {
                moneyText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        // Obtener PlayerMoney desde GameDataManager (esperar un frame para asegurar inicialización)
        StartCoroutine(InitializePlayerMoney());
    }

    /// <summary>
    /// Inicializa PlayerMoney desde GameDataManager después de esperar un frame.
    /// </summary>
    private System.Collections.IEnumerator InitializePlayerMoney()
    {
        // Esperar un frame para asegurar que GameDataManager esté inicializado
        yield return null;

        // Obtener PlayerMoney desde GameDataManager si no está asignado manualmente
        if (playerMoney == null)
        {
            if (GameDataManager.Instance != null)
            {
                playerMoney = GameDataManager.Instance.PlayerMoney;
                if (playerMoney == null)
                {
                    Debug.LogError("MoneyUIManager: PlayerMoney no está asignado en GameDataManager. Asigna la referencia en el Inspector de GameDataManager.");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("MoneyUIManager: GameDataManager.Instance es null. Asegúrate de que existe un GameObject con GameDataManager en la escena.");
                yield break;
            }
        }

        // Suscribirse a eventos
        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged += UpdateMoneyDisplay;
            // Actualizar display inicial
            UpdateMoneyDisplay(playerMoney.GetMoney());
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos
        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged -= UpdateMoneyDisplay;
        }
    }

    /// <summary>
    /// Actualiza el display de monedas.
    /// </summary>
    private void UpdateMoneyDisplay(int newAmount)
    {
        if (moneyText == null)
            return;

        string formattedAmount = formatWithThousands ? FormatNumber(newAmount) : newAmount.ToString();
        moneyText.text = string.Format(moneyFormat, formattedAmount);
    }

    /// <summary>
    /// Formatea un número con separadores de miles.
    /// </summary>
    private string FormatNumber(int number)
    {
        return number.ToString("N0");
    }

    /// <summary>
    /// Actualiza manualmente el display (útil para forzar actualización).
    /// </summary>
    [ContextMenu("Actualizar Display")]
    public void RefreshDisplay()
    {
        if (playerMoney != null)
        {
            UpdateMoneyDisplay(playerMoney.GetMoney());
        }
    }
}

