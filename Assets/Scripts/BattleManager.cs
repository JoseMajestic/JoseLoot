using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona el panel de selección de enemigos y configuración de batalla.
/// Permite seleccionar un enemigo, ver sus detalles y recompensas, e iniciar el combate.
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("Paneles")]
    [Tooltip("Panel donde se muestra la imagen del jugador")]
    [SerializeField] private GameObject playerPanel;
    
    [Tooltip("Panel donde se muestra la imagen del enemigo seleccionado")]
    [SerializeField] private GameObject enemyPanel;
    
    [Header("Botones de Enemigos")]
    [Tooltip("Array de botones de enemigos (cada posición del array contiene el botón y la instancia EnemyData asociada)")]
    [SerializeField] private EnemyButtonData[] enemyButtons;
    
    [Header("Textos de Información del Enemigo")]
    [Tooltip("Texto con los detalles del enemigo")]
    [SerializeField] private TextMeshProUGUI enemyDetailsText;
    
    [Tooltip("Texto con el nivel del enemigo (opcional - solo se usa si el enemigo tiene nivel)")]
    [SerializeField] private TextMeshProUGUI enemyLevelText;
    
    [Tooltip("Texto con el nombre del enemigo")]
    [SerializeField] private TextMeshProUGUI enemyNameText;
    
    [Header("Recompensas")]
    [Tooltip("Texto que muestra las monedas de recompensa")]
    [SerializeField] private TextMeshProUGUI rewardText;
    
    [Header("Botones")]
    [Tooltip("Botón para iniciar el combate")]
    [SerializeField] private Button battleButton;
    
    [Header("Panel de Combate")]
    [Tooltip("Panel donde se abre todo el combate (contiene todos los objetos de la escena de combate)")]
    [SerializeField] private GameObject combatPanel;
    
    [Header("Referencias")]
    [Tooltip("Referencia a la base de datos de enemigos")]
    [SerializeField] private EnemyDatabase enemyDatabase;
    
    [Tooltip("Para obtener stats del jugador")]
    [SerializeField] private EquipmentManager equipmentManager;
    
    [Tooltip("Para agregar monedas al ganar")]
    [SerializeField] private PlayerMoney playerMoney;
    
    // Enemigo actualmente seleccionado
    private EnemyData selectedEnemy = null;

    private void Start()
    {
        InitializeButtons();
        RefreshEnemyButtons();
    }

    /// <summary>
    /// Inicializa los listeners de los botones.
    /// </summary>
    private void InitializeButtons()
    {
        if (battleButton != null)
        {
            battleButton.onClick.AddListener(OnBattleButtonClicked);
        }

        // Inicializar listeners de botones de enemigos
        if (enemyButtons != null)
        {
            for (int i = 0; i < enemyButtons.Length; i++)
            {
                int index = i; // Capturar índice para el closure
                if (enemyButtons[i].button != null)
                {
                    enemyButtons[i].button.onClick.AddListener(() => OnEnemyButtonClicked(index));
                }
            }
        }
    }

    /// <summary>
    /// Obtiene el perfil del jugador desde GameDataManager.
    /// </summary>
    private PlayerProfileData GetPlayerProfile()
    {
        if (GameDataManager.Instance != null)
        {
            return GameDataManager.Instance.GetPlayerProfile();
        }
        return new PlayerProfileData();
    }

    /// <summary>
    /// Refresca los botones de enemigos según los enemigos asignados en el Inspector.
    /// Verifica si cada enemigo está desbloqueado antes de activar su botón.
    /// </summary>
    private void RefreshEnemyButtons()
    {
        if (enemyButtons == null)
            return;

        PlayerProfileData playerProfile = GetPlayerProfile();

        // Activar/desactivar botones según si el enemigo asignado está desbloqueado
        for (int i = 0; i < enemyButtons.Length; i++)
        {
            if (enemyButtons[i].button != null)
            {
                EnemyData enemy = enemyButtons[i].enemyData;
                
                if (enemy != null)
                {
                    // Verificar si el enemigo está desbloqueado
                    bool isUnlocked = enemy.requiredLevel == 0 || playerProfile.IsEnemyLevelUnlocked(enemy.requiredLevel);
                    
                    enemyButtons[i].button.gameObject.SetActive(isUnlocked);
                    
                    // Opcional: Actualizar texto del botón con el nombre del enemigo
                    var buttonText = enemyButtons[i].button.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = enemy.enemyName;
                    }
                }
                else
                {
                    // Si no hay enemigo asignado, desactivar el botón
                    enemyButtons[i].button.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Se llama cuando se hace clic en un botón de enemigo.
    /// </summary>
    private void OnEnemyButtonClicked(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= enemyButtons.Length)
        {
            Debug.LogWarning($"BattleManager: Índice de botón inválido: {buttonIndex}");
            return;
        }

        if (enemyButtons[buttonIndex].enemyData == null)
        {
            Debug.LogWarning($"BattleManager: No hay enemigo asignado al botón en el índice {buttonIndex}.");
            return;
        }

        selectedEnemy = enemyButtons[buttonIndex].enemyData;
        DisplayEnemyInfo(selectedEnemy);
    }

    /// <summary>
    /// Muestra la información del enemigo seleccionado en la UI.
    /// </summary>
    private void DisplayEnemyInfo(EnemyData enemy)
    {
        if (enemy == null)
            return;

        // Mostrar imagen del enemigo
        if (enemyPanel != null)
        {
            Image enemyImage = enemyPanel.GetComponentInChildren<Image>();
            if (enemyImage != null && enemy.enemySprite != null)
            {
                enemyImage.sprite = enemy.enemySprite;
            }
        }

        // Mostrar nombre del enemigo
        if (enemyNameText != null)
        {
            enemyNameText.text = enemy.enemyName;
        }

        // Mostrar nivel del enemigo (si existe y el texto está asignado)
        if (enemyLevelText != null)
        {
            if (enemy.level > 0)
            {
                enemyLevelText.text = $"Nivel: {enemy.level}";
                enemyLevelText.gameObject.SetActive(true);
            }
            else
            {
                enemyLevelText.gameObject.SetActive(false);
            }
        }

        // Mostrar detalles del enemigo
        if (enemyDetailsText != null)
        {
            enemyDetailsText.text = $"HP: {enemy.hp}\n" +
                                   $"Ataque: {enemy.ataque}\n" +
                                   $"Defensa: {enemy.defensa}\n" +
                                   $"Velocidad: {enemy.velocidadAtaque}";
        }

        // Mostrar recompensa
        if (rewardText != null)
        {
            rewardText.text = $"Recompensa: {enemy.rewardCoins} monedas";
        }

        // Habilitar botón de batalla
        if (battleButton != null)
        {
            battleButton.interactable = true;
        }
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón de batalla.
    /// </summary>
    private void OnBattleButtonClicked()
    {
        if (selectedEnemy == null)
        {
            Debug.LogWarning("BattleManager: No hay enemigo seleccionado.");
            return;
        }

        if (combatPanel == null)
        {
            Debug.LogError("BattleManager: CombatPanel no está asignado.");
            return;
        }

        // Ocultar panel de selección de enemigos
        gameObject.SetActive(false);

        // Mostrar panel de combate
        combatPanel.SetActive(true);

        // Pasar el enemigo seleccionado al CombatManager
        CombatManager combatManager = combatPanel.GetComponent<CombatManager>();
        if (combatManager != null)
        {
            combatManager.StartCombat(selectedEnemy);
        }
        else
        {
            Debug.LogError("BattleManager: CombatManager no encontrado en el panel de combate.");
        }
    }

    /// <summary>
    /// Se llama cuando el panel se activa.
    /// </summary>
    private void OnEnable()
    {
        // Refrescar botones de enemigos (por si se desbloqueó alguno nuevo)
        RefreshEnemyButtons();
        
        // Limpiar selección
        selectedEnemy = null;
        
        // Deshabilitar botón de batalla hasta que se seleccione un enemigo
        if (battleButton != null)
        {
            battleButton.interactable = false;
        }
    }

    private void OnDestroy()
    {
        // Limpiar listeners
        if (battleButton != null)
        {
            battleButton.onClick.RemoveAllListeners();
        }

        if (enemyButtons != null)
        {
            foreach (var enemyButtonData in enemyButtons)
            {
                if (enemyButtonData.button != null)
                {
                    enemyButtonData.button.onClick.RemoveAllListeners();
                }
            }
        }
    }
}

