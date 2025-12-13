using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona el panel de biblioteca donde el jugador puede comprar y desbloquear ataques.
/// Los ataques solo estarán disponibles en CombatManager si han sido desbloqueados aquí.
/// </summary>
public class BibliotecaManager : MonoBehaviour
{
    [System.Serializable]
    public class AttackButtonData
    {
        [Tooltip("Botón del ataque")]
        public Button button;
        
        [Tooltip("Ataque asociado a este botón")]
        public AttackData attackData;
    }

    [Header("Panel")]
    [Tooltip("Panel principal de la biblioteca")]
    [SerializeField] private GameObject bibliotecaPanel;

    [Header("Botones de Ataques")]
    [Tooltip("Array de 20 botones de ataques (uno por cada ataque desbloqueable)")]
    [SerializeField] private AttackButtonData[] attackButtons = new AttackButtonData[20];

    [Header("UI de Detalles")]
    [Tooltip("Texto que muestra los detalles del ataque seleccionado")]
    [SerializeField] private TextMeshProUGUI attackDetailsText;
    
    [Tooltip("Texto que muestra el precio en monedas")]
    [SerializeField] private TextMeshProUGUI priceText;
    
    [Tooltip("Texto que muestra el nivel requerido")]
    [SerializeField] private TextMeshProUGUI requiredLevelText;
    
    [Tooltip("Botón para comprar/desbloquear el ataque seleccionado")]
    [SerializeField] private Button buyButton;

    [Header("Referencias")]
    [Tooltip("Referencia al GameDataManager")]
    private GameDataManager gameDataManager;
    
    [Tooltip("Referencia al PlayerMoney")]
    private PlayerMoney playerMoney;

    // Ataque actualmente seleccionado
    private AttackData selectedAttack = null;
    private int selectedButtonIndex = -1;

    private void Start()
    {
        // Obtener referencias
        gameDataManager = GameDataManager.Instance;
        if (gameDataManager != null)
        {
            playerMoney = gameDataManager.PlayerMoney;
            
            // Suscribirse a cambios de dinero para actualizar el botón de compra
            if (playerMoney != null)
            {
                playerMoney.OnMoneyChanged += OnMoneyChanged;
            }
        }

        // Inicializar botones
        InitializeButtons();
        
        // Actualizar estado inicial de los botones
        RefreshAllButtons();
    }

    /// <summary>
    /// Se llama cuando cambia el dinero del jugador.
    /// </summary>
    private void OnMoneyChanged(int newMoney)
    {
        // Actualizar estado del botón de compra si hay un ataque seleccionado
        if (selectedAttack != null)
        {
            UpdateBuyButtonState();
        }
        
        // Actualizar todos los botones (por si cambió el nivel también)
        RefreshAllButtons();
    }

    /// <summary>
    /// Inicializa los listeners de los botones.
    /// </summary>
    private void InitializeButtons()
    {
        // Inicializar listeners de botones de ataques
        if (attackButtons != null)
        {
            for (int i = 0; i < attackButtons.Length; i++)
            {
                int index = i; // Capturar índice para el closure
                if (attackButtons[i].button != null)
                {
                    attackButtons[i].button.onClick.AddListener(() => OnAttackButtonClicked(index));
                }
            }
        }

        // Inicializar botón de compra
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    /// <summary>
    /// Se llama cuando se hace clic en un botón de ataque.
    /// </summary>
    private void OnAttackButtonClicked(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= attackButtons.Length)
        {
            Debug.LogWarning($"BibliotecaManager: Índice de botón inválido: {buttonIndex}");
            return;
        }

        if (attackButtons[buttonIndex].attackData == null)
        {
            Debug.LogWarning($"BibliotecaManager: El ataque en el índice {buttonIndex} es nulo.");
            return;
        }

        selectedAttack = attackButtons[buttonIndex].attackData;
        selectedButtonIndex = buttonIndex;

        // Mostrar detalles del ataque
        UpdateAttackDetails();

        // Actualizar estado del botón de compra
        UpdateBuyButtonState();
    }

    /// <summary>
    /// Actualiza los detalles del ataque seleccionado en la UI.
    /// </summary>
    private void UpdateAttackDetails()
    {
        if (selectedAttack == null)
            return;

        // Mostrar detalles del ataque
        if (attackDetailsText != null)
        {
            attackDetailsText.text = $"{selectedAttack.attackName}\n\n{selectedAttack.description}";
        }

        // Mostrar precio
        if (priceText != null)
        {
            priceText.text = $"PRECIO MONEDAS: {selectedAttack.unlockPrice}";
        }

        // Mostrar nivel requerido
        if (requiredLevelText != null)
        {
            requiredLevelText.text = $"NV. Requerido: {selectedAttack.requiredHeroLevel}";
        }
    }

    /// <summary>
    /// Actualiza el estado del botón de compra según los requisitos.
    /// </summary>
    private void UpdateBuyButtonState()
    {
        if (buyButton == null || selectedAttack == null || gameDataManager == null)
            return;

        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
        {
            buyButton.interactable = false;
            return;
        }

        // Verificar si el ataque ya está desbloqueado
        bool isUnlocked = profile.IsAttackUnlocked(selectedAttack.attackName);
        if (isUnlocked)
        {
            buyButton.interactable = false;
            if (attackDetailsText != null)
            {
                attackDetailsText.text += "\n\n[YA DESBLOQUEADO]";
            }
            return;
        }

        // Verificar nivel requerido
        bool hasRequiredLevel = selectedAttack.requiredHeroLevel == 0 || 
                               profile.heroLevel >= selectedAttack.requiredHeroLevel;

        // Verificar dinero suficiente
        bool hasEnoughMoney = playerMoney != null && 
                             playerMoney.GetMoney() >= selectedAttack.unlockPrice;

        // El botón está habilitado solo si se cumplen ambos requisitos
        buyButton.interactable = hasRequiredLevel && hasEnoughMoney;
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón de compra.
    /// </summary>
    private void OnBuyButtonClicked()
    {
        if (selectedAttack == null || gameDataManager == null)
        {
            Debug.LogWarning("BibliotecaManager: No hay ataque seleccionado o GameDataManager no está disponible.");
            return;
        }

        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
        {
            Debug.LogWarning("BibliotecaManager: No se pudo obtener el perfil del jugador.");
            return;
        }

        // Verificar si ya está desbloqueado
        if (profile.IsAttackUnlocked(selectedAttack.attackName))
        {
            Debug.LogWarning($"BibliotecaManager: El ataque '{selectedAttack.attackName}' ya está desbloqueado.");
            return;
        }

        // Verificar nivel requerido
        if (selectedAttack.requiredHeroLevel > 0 && profile.heroLevel < selectedAttack.requiredHeroLevel)
        {
            Debug.LogWarning($"BibliotecaManager: Nivel insuficiente. Requerido: {selectedAttack.requiredHeroLevel}, Actual: {profile.heroLevel}");
            return;
        }

        // Verificar dinero suficiente
        if (playerMoney == null)
        {
            Debug.LogError("BibliotecaManager: PlayerMoney no está asignado.");
            return;
        }

        if (playerMoney.GetMoney() < selectedAttack.unlockPrice)
        {
            Debug.LogWarning($"BibliotecaManager: Dinero insuficiente. Requerido: {selectedAttack.unlockPrice}, Actual: {playerMoney.GetMoney()}");
            return;
        }

        // Realizar la compra
        playerMoney.SubtractMoney(selectedAttack.unlockPrice);
        profile.UnlockAttack(selectedAttack.attackName);
        gameDataManager.SavePlayerProfile();

        Debug.Log($"Ataque '{selectedAttack.attackName}' desbloqueado por {selectedAttack.unlockPrice} monedas.");

        // Actualizar UI
        RefreshAllButtons();
        UpdateBuyButtonState();
    }

    /// <summary>
    /// Actualiza el estado de todos los botones según si están desbloqueados y cumplen requisitos.
    /// NOTA: Los botones siempre están habilitados para poder ver los detalles, incluso si no se cumplen los requisitos.
    /// </summary>
    private void RefreshAllButtons()
    {
        if (attackButtons == null)
            return;

        // SOLUCIÓN: Los botones siempre están habilitados para poder hacer clic y ver los detalles
        // El botón de compra es el que se habilita/deshabilita según los requisitos
        foreach (var buttonData in attackButtons)
        {
            if (buttonData.button != null)
            {
                // Siempre habilitado para poder ver los detalles
                buttonData.button.interactable = true;
            }
        }
    }

    /// <summary>
    /// Se llama cuando el panel se activa.
    /// </summary>
    private void OnEnable()
    {
        // Refrescar todos los botones cuando se abre el panel
        RefreshAllButtons();
        
        // Limpiar selección
        selectedAttack = null;
        selectedButtonIndex = -1;
        
        // Limpiar detalles
        if (attackDetailsText != null)
        {
            attackDetailsText.text = "";
        }
        
        if (priceText != null)
        {
            priceText.text = "PRECIO MONEDAS: --";
        }
        
        if (requiredLevelText != null)
        {
            requiredLevelText.text = "NV. Requerido: --";
        }
        
        // Deshabilitar botón de compra
        if (buyButton != null)
        {
            buyButton.interactable = false;
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos
        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged -= OnMoneyChanged;
        }

        // Limpiar listeners
        if (attackButtons != null)
        {
            foreach (var buttonData in attackButtons)
            {
                if (buttonData.button != null)
                {
                    buttonData.button.onClick.RemoveAllListeners();
                }
            }
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
        }
    }
}

