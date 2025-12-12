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
    
    [Header("Energía")]
    [Tooltip("Texto para mostrar la energía actual del héroe")]
    [SerializeField] private TextMeshProUGUI heroEnergyText;
    
    [Tooltip("Texto para mostrar el coste de energía de la batalla")]
    [SerializeField] private TextMeshProUGUI battleEnergyCostText;
    
    [Tooltip("Coste de energía para iniciar una batalla")]
    [SerializeField] private int battleEnergyCost = 10;
    
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
    
    // Referencia al sistema de energía
    private EnergySystem energySystem;

    private void Start()
    {
        InitializeButtons();
        RefreshEnemyButtons();
        
        // Obtener referencia al sistema de energía
        energySystem = FindFirstObjectByType<EnergySystem>();
        
        // Actualizar UI de energía inicial
        RefreshEnergyUI();
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
        // Si no hay GameDataManager, crear perfil básico con nivel 0 desbloqueado
        PlayerProfileData profile = new PlayerProfileData();
        profile.UnlockEnemyLevel(0); // Desbloquear nivel 0 por defecto
        return profile;
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
                    bool isUnlocked = false;
                    
                    // El primer enemigo (índice 0) siempre está desbloqueado
                    if (i == 0)
                    {
                        isUnlocked = true;
                    }
                    else
                    {
                        // Para los demás, verificar si el enemigo anterior fue derrotado
                        EnemyData previousEnemy = enemyButtons[i - 1].enemyData;
                        if (previousEnemy != null)
                        {
                            isUnlocked = playerProfile.IsEnemyDefeated(previousEnemy.enemyName);
                        }
                        else
                        {
                            // Si no hay enemigo anterior asignado, no desbloquear
                            isUnlocked = false;
                        }
                    }
                    
                    // Asegurar que el botón esté visible
                    enemyButtons[i].button.gameObject.SetActive(true);
                    // Deshabilitar/habilitar según si está desbloqueado
                    enemyButtons[i].button.interactable = isUnlocked;
                    
                    // Opcional: Actualizar texto del botón con el nombre del enemigo
                    var buttonText = enemyButtons[i].button.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = enemy.enemyName;
                    }
                }
                else
                {
                    // Si no hay enemigo asignado, ocultar el botón completamente
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

        // Actualizar UI de energía cuando se selecciona un enemigo
        RefreshEnergyUI();
        
        // Actualizar estado del botón de batalla (verifica energía)
        UpdateBattleButtonState();
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

        // Verificar y gastar energía ANTES de iniciar el combate
        if (energySystem != null)
        {
            if (!energySystem.CanAfford(battleEnergyCost))
            {
                Debug.LogWarning($"BattleManager: No hay suficiente energía. Requerida: {battleEnergyCost}");
                return;
            }
            
            // Gastar energía
            if (!energySystem.SpendEnergy(battleEnergyCost))
            {
                Debug.LogWarning("BattleManager: No se pudo gastar energía.");
                return;
            }
            
            // Actualizar UI de energía
            RefreshEnergyUI();
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
        
        // Actualizar UI de energía
        RefreshEnergyUI();
        
        // Actualizar estado del botón de batalla
        UpdateBattleButtonState();
    }
    
    /// <summary>
    /// Actualiza los textos de energía del héroe.
    /// </summary>
    private void RefreshEnergyUI()
    {
        if (energySystem == null)
            return;
        
        int currentEnergy = energySystem.GetCurrentEnergy();
        int maxEnergy = energySystem.GetMaxEnergy();
        
        Debug.Log($"[ENERGY DEBUG] BattleManager.RefreshEnergyUI - Leyendo energía: {currentEnergy} / {maxEnergy}");
        
        if (heroEnergyText != null)
        {
            string textBefore = heroEnergyText.text;
            heroEnergyText.text = $"Energía: {currentEnergy} / {maxEnergy}";
            if (textBefore != heroEnergyText.text)
            {
                Debug.Log($"[ENERGY DEBUG] BattleManager - Texto actualizado: '{textBefore}' -> '{heroEnergyText.text}'");
            }
        }
        
        if (battleEnergyCostText != null)
        {
            battleEnergyCostText.text = $"Coste: {battleEnergyCost}";
        }
        
        // Actualizar estado del botón de batalla según energía disponible
        UpdateBattleButtonState();
    }
    
    /// <summary>
    /// Actualiza el estado del botón de batalla según si hay suficiente energía.
    /// </summary>
    private void UpdateBattleButtonState()
    {
        if (battleButton == null)
            return;
        
        // El botón solo se habilita si:
        // 1. Hay un enemigo seleccionado
        // 2. Hay suficiente energía
        bool canBattle = selectedEnemy != null;
        
        if (canBattle && energySystem != null)
        {
            canBattle = energySystem.CanAfford(battleEnergyCost);
        }
        
        battleButton.interactable = canBattle;
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

