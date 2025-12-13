using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona el panel de gimnasio (Upgrade) con botones para mejorar stats del héroe.
/// Cada botón mejora un stat específico basado en el valor del atributo de crianza asociado.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    // ===== REFERENCIAS =====
    private GameDataManager gameDataManager;
    private EnergySystem energySystem;
    
    // ===== PANEL Y BOTÓN DE CIERRE =====
    [Header("Panel y Botón de Cierre")]
    [Tooltip("Panel General Upgrade")]
    [SerializeField] private GameObject upgradePanel;
    
    [Tooltip("Botón para cerrar panel y volver a Breed")]
    [SerializeField] private Button closeUpgradeButton;
    
    [Header("Energía")]
    [Tooltip("Texto para mostrar la energía actual y máxima (ej: 75 / 100)")]
    [SerializeField] private TextMeshProUGUI energyText;
    
    // ===== BOTONES Y TEXTOS DE HP =====
    [Header("HP")]
    [Tooltip("Botón de mejora de HP")]
    [SerializeField] private Button hpButton;
    
    [Tooltip("Texto de coste de energía de HP (siempre 10)")]
    [SerializeField] private TextMeshProUGUI hpCostText;
    
    [Tooltip("Texto de nivel actual de HP")]
    [SerializeField] private TextMeshProUGUI hpCurrentLevelText;
    
    [Tooltip("Texto de siguiente nivel de HP")]
    [SerializeField] private TextMeshProUGUI hpNextLevelText;
    
    [Tooltip("Texto de bonus total acumulado de HP")]
    [SerializeField] private TextMeshProUGUI hpTotalBonusText;
    
    // ===== BOTONES Y TEXTOS DE MANA =====
    [Header("Mana")]
    [Tooltip("Botón de mejora de Mana")]
    [SerializeField] private Button manaButton;
    
    [Tooltip("Texto de coste de energía de Mana")]
    [SerializeField] private TextMeshProUGUI manaCostText;
    
    [Tooltip("Texto de nivel actual de Mana")]
    [SerializeField] private TextMeshProUGUI manaCurrentLevelText;
    
    [Tooltip("Texto de siguiente nivel de Mana")]
    [SerializeField] private TextMeshProUGUI manaNextLevelText;
    
    [Tooltip("Texto de bonus total acumulado de Mana")]
    [SerializeField] private TextMeshProUGUI manaTotalBonusText;
    
    // ===== BOTONES Y TEXTOS DE ATAQUE =====
    [Header("Ataque")]
    [Tooltip("Botón de mejora de Ataque")]
    [SerializeField] private Button attackButton;
    
    [Tooltip("Texto de coste de energía de Ataque")]
    [SerializeField] private TextMeshProUGUI attackCostText;
    
    [Tooltip("Texto de nivel actual de Ataque")]
    [SerializeField] private TextMeshProUGUI attackCurrentLevelText;
    
    [Tooltip("Texto de siguiente nivel de Ataque")]
    [SerializeField] private TextMeshProUGUI attackNextLevelText;
    
    [Tooltip("Texto de bonus total acumulado de Ataque")]
    [SerializeField] private TextMeshProUGUI attackTotalBonusText;
    
    // ===== BOTONES Y TEXTOS DE DEFENSA =====
    [Header("Defensa")]
    [Tooltip("Botón de mejora de Defensa")]
    [SerializeField] private Button defenseButton;
    
    [Tooltip("Texto de coste de energía de Defensa")]
    [SerializeField] private TextMeshProUGUI defenseCostText;
    
    [Tooltip("Texto de nivel actual de Defensa")]
    [SerializeField] private TextMeshProUGUI defenseCurrentLevelText;
    
    [Tooltip("Texto de siguiente nivel de Defensa")]
    [SerializeField] private TextMeshProUGUI defenseNextLevelText;
    
    [Tooltip("Texto de bonus total acumulado de Defensa")]
    [SerializeField] private TextMeshProUGUI defenseTotalBonusText;
    
    // ===== BOTONES Y TEXTOS DE SKILL =====
    [Header("Skill")]
    [Tooltip("Botón de mejora de Skill")]
    [SerializeField] private Button skillButton;
    
    [Tooltip("Texto de coste de energía de Skill")]
    [SerializeField] private TextMeshProUGUI skillCostText;
    
    [Tooltip("Texto de nivel actual de Skill")]
    [SerializeField] private TextMeshProUGUI skillCurrentLevelText;
    
    [Tooltip("Texto de siguiente nivel de Skill")]
    [SerializeField] private TextMeshProUGUI skillNextLevelText;
    
    [Tooltip("Texto de bonus total acumulado de Skill")]
    [SerializeField] private TextMeshProUGUI skillTotalBonusText;
    
    // ===== BOTONES Y TEXTOS DE CRITICAL ATTACK =====
    [Header("Critical Attack")]
    [Tooltip("Botón de mejora de Critical Attack")]
    [SerializeField] private Button criticalAttackButton;
    
    [Tooltip("Texto de coste de energía de Critical Attack")]
    [SerializeField] private TextMeshProUGUI criticalAttackCostText;
    
    [Tooltip("Texto de nivel actual de Critical Attack")]
    [SerializeField] private TextMeshProUGUI criticalAttackCurrentLevelText;
    
    [Tooltip("Texto de siguiente nivel de Critical Attack")]
    [SerializeField] private TextMeshProUGUI criticalAttackNextLevelText;
    
    [Tooltip("Texto de bonus total acumulado de Critical Attack")]
    [SerializeField] private TextMeshProUGUI criticalAttackTotalBonusText;
    
    // ===== BOTONES Y TEXTOS DE CRITICAL DAMAGE =====
    [Header("Critical Damage")]
    [Tooltip("Botón de mejora de Critical Damage")]
    [SerializeField] private Button criticalDamageButton;
    
    [Tooltip("Texto de coste de energía de Critical Damage")]
    [SerializeField] private TextMeshProUGUI criticalDamageCostText;
    
    [Tooltip("Texto de nivel actual de Critical Damage")]
    [SerializeField] private TextMeshProUGUI criticalDamageCurrentLevelText;
    
    [Tooltip("Texto de siguiente nivel de Critical Damage")]
    [SerializeField] private TextMeshProUGUI criticalDamageNextLevelText;
    
    [Tooltip("Texto de bonus total acumulado de Critical Damage")]
    [SerializeField] private TextMeshProUGUI criticalDamageTotalBonusText;
    
    // ===== BOTONES Y TEXTOS DE ATTACK SPEED =====
    [Header("Attack Speed")]
    [Tooltip("Botón de mejora de Attack Speed")]
    [SerializeField] private Button attackSpeedButton;
    
    [Tooltip("Texto de coste de energía de Attack Speed")]
    [SerializeField] private TextMeshProUGUI attackSpeedCostText;
    
    [Tooltip("Texto de nivel actual de Attack Speed")]
    [SerializeField] private TextMeshProUGUI attackSpeedCurrentLevelText;
    
    [Tooltip("Texto de siguiente nivel de Attack Speed")]
    [SerializeField] private TextMeshProUGUI attackSpeedNextLevelText;
    
    [Tooltip("Texto de bonus total acumulado de Attack Speed")]
    [SerializeField] private TextMeshProUGUI attackSpeedTotalBonusText;
    
    // ===== BOTONES Y TEXTOS DE LUCK =====
    [Header("Luck")]
    [Tooltip("Botón de mejora de Luck")]
    [SerializeField] private Button luckButton;
    
    [Tooltip("Texto de coste de energía de Luck")]
    [SerializeField] private TextMeshProUGUI luckCostText;
    
    [Tooltip("Texto de nivel actual de Luck")]
    [SerializeField] private TextMeshProUGUI luckCurrentLevelText;
    
    [Tooltip("Texto de siguiente nivel de Luck")]
    [SerializeField] private TextMeshProUGUI luckNextLevelText;
    
    [Tooltip("Texto de bonus total acumulado de Luck")]
    [SerializeField] private TextMeshProUGUI luckTotalBonusText;
    
    // ===== CONSTANTES =====
    private const int ENERGY_COST = 10;
    private const int MAX_LEVEL_NORMAL = 999; // HP, Mana, Ataque, Defensa, Skill
    private const int MAX_LEVEL_CRITICAL = 100; // Critical Attack, Critical Damage, Attack Speed, Luck
    
    // ===== ENUM PARA TIPOS DE STATS =====
    private enum StatType
    {
        HP,
        Mana,
        Attack,
        Defense,
        Skill,
        CriticalAttack,
        CriticalDamage,
        AttackSpeed,
        Luck
    }
    
    private void Start()
    {
        // Obtener referencias
        gameDataManager = GameDataManager.Instance;
        energySystem = FindFirstObjectByType<EnergySystem>();
        
        if (gameDataManager == null)
        {
            Debug.LogError("UpgradeManager: GameDataManager no encontrado.");
            return;
        }
        
        // Configurar botones
        SetupButtons();
        
        // Actualizar UI inicial
        RefreshAllButtons();
        RefreshEnergyUI();
        UpdateButtonsState();
    }
    
    /// <summary>
    /// Se llama cuando el panel se activa.
    /// </summary>
    private void OnEnable()
    {
        // Actualizar energía y estado de botones cuando se abre el panel
        RefreshEnergyUI();
        UpdateButtonsState();
    }
    
    /// <summary>
    /// Configura los listeners de los botones.
    /// </summary>
    private void SetupButtons()
    {
        if (closeUpgradeButton != null)
            closeUpgradeButton.onClick.AddListener(OnCloseButtonClicked);
        
        if (hpButton != null)
            hpButton.onClick.AddListener(() => OnStatButtonClicked(StatType.HP));
        if (manaButton != null)
            manaButton.onClick.AddListener(() => OnStatButtonClicked(StatType.Mana));
        if (attackButton != null)
            attackButton.onClick.AddListener(() => OnStatButtonClicked(StatType.Attack));
        if (defenseButton != null)
            defenseButton.onClick.AddListener(() => OnStatButtonClicked(StatType.Defense));
        if (skillButton != null)
            skillButton.onClick.AddListener(() => OnStatButtonClicked(StatType.Skill));
        if (criticalAttackButton != null)
            criticalAttackButton.onClick.AddListener(() => OnStatButtonClicked(StatType.CriticalAttack));
        if (criticalDamageButton != null)
            criticalDamageButton.onClick.AddListener(() => OnStatButtonClicked(StatType.CriticalDamage));
        if (attackSpeedButton != null)
            attackSpeedButton.onClick.AddListener(() => OnStatButtonClicked(StatType.AttackSpeed));
        if (luckButton != null)
            luckButton.onClick.AddListener(() => OnStatButtonClicked(StatType.Luck));
    }
    
    /// <summary>
    /// Cierra el panel y vuelve a Breed.
    /// </summary>
    public void OnCloseButtonClicked()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Maneja el clic en un botón de stat.
    /// </summary>
    private void OnStatButtonClicked(StatType statType)
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Verificar energía
        if (energySystem == null || !energySystem.CanAfford(ENERGY_COST))
        {
            Debug.LogWarning("UpgradeManager: No hay suficiente energía.");
            return;
        }
        
        // Obtener nivel actual y máximo
        int currentLevel = GetCurrentLevel(profile, statType);
        int maxLevel = GetMaxLevel(statType);
        
        // Verificar límite
        if (currentLevel >= maxLevel)
        {
            Debug.LogWarning($"UpgradeManager: {statType} ya está al máximo nivel ({maxLevel}).");
            return;
        }
        
        // Obtener atributo de crianza asociado
        int attributeValue = GetAssociatedAttributeValue(profile, statType);
        
        // Calcular mejora
        int improvement = GetImprovementAmount(attributeValue);
        
        // SOLUCIÓN: Permitir mejoras aunque el atributo esté en 0 (solo dará +0)
        // Mostrar warning pero continuar
        if (improvement <= 0)
        {
            Debug.LogWarning($"UpgradeManager: El atributo asociado está muy bajo. Mejora: {improvement} (se aplicará mejora de +0, pero se gastará energía)");
            // NO retornar, permitir continuar para que se gaste energía y se suba de nivel
        }
        
        // Gastar energía
        if (!energySystem.SpendEnergy(ENERGY_COST))
        {
            Debug.LogWarning("UpgradeManager: No se pudo gastar energía.");
            return;
        }
        
        // Aplicar mejora
        int newLevel = currentLevel + 1;
        SetLevel(profile, statType, newLevel);
        
        // Actualizar bonus total
        int totalBonus = GetTotalBonus(profile, statType);
        SetTotalBonus(profile, statType, totalBonus + improvement);
        
        // Guardar cambios
        gameDataManager.SavePlayerProfile();
        
        // Actualizar UI
        RefreshButtonInfo(statType);
        RefreshEnergyUI();
        UpdateButtonsState();
        
        Debug.Log($"UpgradeManager: {statType} mejorado a nivel {newLevel} (+{improvement}).");
    }
    
    /// <summary>
    /// Obtiene el valor del atributo de crianza asociado a un stat.
    /// </summary>
    private int GetAssociatedAttributeValue(PlayerProfileData profile, StatType statType)
    {
        switch (statType)
        {
            case StatType.HP:
                return profile.breedHunger;
            case StatType.Mana:
                return profile.breedDiscipline;
            case StatType.Attack:
            case StatType.Defense:
            case StatType.CriticalAttack:
                return profile.breedWork;
            case StatType.Skill:
            case StatType.CriticalDamage:
                return profile.breedDiscipline;
            case StatType.AttackSpeed:
                return profile.breedEnergy;
            case StatType.Luck:
                return profile.breedHappiness;
            default:
                return 0;
        }
    }
    
    /// <summary>
    /// Calcula la cantidad de mejora según el valor del atributo.
    /// </summary>
    private int GetImprovementAmount(int attributeValue)
    {
        if (attributeValue >= 90) return 4;
        if (attributeValue >= 75) return 3;
        if (attributeValue >= 50) return 2;
        if (attributeValue >= 25) return 1;
        return 0;
    }
    
    /// <summary>
    /// Obtiene el nivel actual de un stat.
    /// </summary>
    private int GetCurrentLevel(PlayerProfileData profile, StatType statType)
    {
        switch (statType)
        {
            case StatType.HP:
                return profile.gymHPLevel;
            case StatType.Mana:
                return profile.gymManaLevel;
            case StatType.Attack:
                return profile.gymAttackLevel;
            case StatType.Defense:
                return profile.gymDefenseLevel;
            case StatType.Skill:
                return profile.gymSkillLevel;
            case StatType.CriticalAttack:
                return profile.gymCriticalAttackLevel;
            case StatType.CriticalDamage:
                return profile.gymCriticalDamageLevel;
            case StatType.AttackSpeed:
                return profile.gymAttackSpeedLevel;
            case StatType.Luck:
                return profile.gymLuckLevel;
            default:
                return 0;
        }
    }
    
    /// <summary>
    /// Establece el nivel de un stat.
    /// </summary>
    private void SetLevel(PlayerProfileData profile, StatType statType, int level)
    {
        switch (statType)
        {
            case StatType.HP:
                profile.gymHPLevel = level;
                break;
            case StatType.Mana:
                profile.gymManaLevel = level;
                break;
            case StatType.Attack:
                profile.gymAttackLevel = level;
                break;
            case StatType.Defense:
                profile.gymDefenseLevel = level;
                break;
            case StatType.Skill:
                profile.gymSkillLevel = level;
                break;
            case StatType.CriticalAttack:
                profile.gymCriticalAttackLevel = level;
                break;
            case StatType.CriticalDamage:
                profile.gymCriticalDamageLevel = level;
                break;
            case StatType.AttackSpeed:
                profile.gymAttackSpeedLevel = level;
                break;
            case StatType.Luck:
                profile.gymLuckLevel = level;
                break;
        }
    }
    
    /// <summary>
    /// Obtiene el bonus total acumulado de un stat.
    /// </summary>
    private int GetTotalBonus(PlayerProfileData profile, StatType statType)
    {
        switch (statType)
        {
            case StatType.HP:
                return profile.gymHPTotalBonus;
            case StatType.Mana:
                return profile.gymManaTotalBonus;
            case StatType.Attack:
                return profile.gymAttackTotalBonus;
            case StatType.Defense:
                return profile.gymDefenseTotalBonus;
            case StatType.Skill:
                return profile.gymSkillTotalBonus;
            case StatType.CriticalAttack:
                return profile.gymCriticalAttackTotalBonus;
            case StatType.CriticalDamage:
                return profile.gymCriticalDamageTotalBonus;
            case StatType.AttackSpeed:
                return profile.gymAttackSpeedTotalBonus;
            case StatType.Luck:
                return profile.gymLuckTotalBonus;
            default:
                return 0;
        }
    }
    
    /// <summary>
    /// Establece el bonus total acumulado de un stat.
    /// </summary>
    private void SetTotalBonus(PlayerProfileData profile, StatType statType, int bonus)
    {
        switch (statType)
        {
            case StatType.HP:
                profile.gymHPTotalBonus = bonus;
                break;
            case StatType.Mana:
                profile.gymManaTotalBonus = bonus;
                break;
            case StatType.Attack:
                profile.gymAttackTotalBonus = bonus;
                break;
            case StatType.Defense:
                profile.gymDefenseTotalBonus = bonus;
                break;
            case StatType.Skill:
                profile.gymSkillTotalBonus = bonus;
                break;
            case StatType.CriticalAttack:
                profile.gymCriticalAttackTotalBonus = bonus;
                break;
            case StatType.CriticalDamage:
                profile.gymCriticalDamageTotalBonus = bonus;
                break;
            case StatType.AttackSpeed:
                profile.gymAttackSpeedTotalBonus = bonus;
                break;
            case StatType.Luck:
                profile.gymLuckTotalBonus = bonus;
                break;
        }
    }
    
    /// <summary>
    /// Obtiene el nivel máximo de un stat.
    /// </summary>
    private int GetMaxLevel(StatType statType)
    {
        switch (statType)
        {
            case StatType.HP:
            case StatType.Mana:
            case StatType.Attack:
            case StatType.Defense:
            case StatType.Skill:
                return MAX_LEVEL_NORMAL;
            case StatType.CriticalAttack:
            case StatType.CriticalDamage:
            case StatType.AttackSpeed:
            case StatType.Luck:
                return MAX_LEVEL_CRITICAL;
            default:
                return 999;
        }
    }
    
    /// <summary>
    /// Actualiza la información de todos los botones.
    /// </summary>
    public void RefreshAllButtons()
    {
        RefreshButtonInfo(StatType.HP);
        RefreshButtonInfo(StatType.Mana);
        RefreshButtonInfo(StatType.Attack);
        RefreshButtonInfo(StatType.Defense);
        RefreshButtonInfo(StatType.Skill);
        RefreshButtonInfo(StatType.CriticalAttack);
        RefreshButtonInfo(StatType.CriticalDamage);
        RefreshButtonInfo(StatType.AttackSpeed);
        RefreshButtonInfo(StatType.Luck);
        
        // Actualizar estado de botones y energía
        RefreshEnergyUI();
        UpdateButtonsState();
    }
    
    /// <summary>
    /// Actualiza la información de un botón específico.
    /// </summary>
    private void RefreshButtonInfo(StatType statType)
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        int currentLevel = GetCurrentLevel(profile, statType);
        int maxLevel = GetMaxLevel(statType);
        int totalBonus = GetTotalBonus(profile, statType);
        int attributeValue = GetAssociatedAttributeValue(profile, statType);
        int nextImprovement = GetImprovementAmount(attributeValue);
        
        // Actualizar textos según el tipo
        switch (statType)
        {
            case StatType.HP:
                if (hpCostText != null)
                    hpCostText.text = ENERGY_COST.ToString();
                if (hpCurrentLevelText != null)
                    hpCurrentLevelText.text = $"HP: {currentLevel}";
                if (hpNextLevelText != null)
                    hpNextLevelText.text = currentLevel >= maxLevel ? "MAX" : $"Nivel {currentLevel + 1} (+{nextImprovement})";
                if (hpTotalBonusText != null)
                    hpTotalBonusText.text = $"+{totalBonus}";
                break;
            case StatType.Mana:
                if (manaCostText != null)
                    manaCostText.text = ENERGY_COST.ToString();
                if (manaCurrentLevelText != null)
                    manaCurrentLevelText.text = $"Mana: {currentLevel}";
                if (manaNextLevelText != null)
                    manaNextLevelText.text = currentLevel >= maxLevel ? "MAX" : $"Nivel {currentLevel + 1} (+{nextImprovement})";
                if (manaTotalBonusText != null)
                    manaTotalBonusText.text = $"+{totalBonus}";
                break;
            case StatType.Attack:
                if (attackCostText != null)
                    attackCostText.text = ENERGY_COST.ToString();
                if (attackCurrentLevelText != null)
                    attackCurrentLevelText.text = $"Ataque: {currentLevel}";
                if (attackNextLevelText != null)
                    attackNextLevelText.text = currentLevel >= maxLevel ? "MAX" : $"Nivel {currentLevel + 1} (+{nextImprovement})";
                if (attackTotalBonusText != null)
                    attackTotalBonusText.text = $"+{totalBonus}";
                break;
            case StatType.Defense:
                if (defenseCostText != null)
                    defenseCostText.text = ENERGY_COST.ToString();
                if (defenseCurrentLevelText != null)
                    defenseCurrentLevelText.text = $"Defensa: {currentLevel}";
                if (defenseNextLevelText != null)
                    defenseNextLevelText.text = currentLevel >= maxLevel ? "MAX" : $"Nivel {currentLevel + 1} (+{nextImprovement})";
                if (defenseTotalBonusText != null)
                    defenseTotalBonusText.text = $"+{totalBonus}";
                break;
            case StatType.Skill:
                if (skillCostText != null)
                    skillCostText.text = ENERGY_COST.ToString();
                if (skillCurrentLevelText != null)
                    skillCurrentLevelText.text = $"Skill: {currentLevel}";
                if (skillNextLevelText != null)
                    skillNextLevelText.text = currentLevel >= maxLevel ? "MAX" : $"Nivel {currentLevel + 1} (+{nextImprovement})";
                if (skillTotalBonusText != null)
                    skillTotalBonusText.text = $"+{totalBonus}";
                break;
            case StatType.CriticalAttack:
                if (criticalAttackCostText != null)
                    criticalAttackCostText.text = ENERGY_COST.ToString();
                if (criticalAttackCurrentLevelText != null)
                    criticalAttackCurrentLevelText.text = $"Crit. Ataque: {currentLevel}";
                if (criticalAttackNextLevelText != null)
                    criticalAttackNextLevelText.text = currentLevel >= maxLevel ? "MAX" : $"Nivel {currentLevel + 1} (+{nextImprovement})";
                if (criticalAttackTotalBonusText != null)
                    criticalAttackTotalBonusText.text = $"+{totalBonus}";
                break;
            case StatType.CriticalDamage:
                if (criticalDamageCostText != null)
                    criticalDamageCostText.text = ENERGY_COST.ToString();
                if (criticalDamageCurrentLevelText != null)
                    criticalDamageCurrentLevelText.text = $"Crit. Daño: {currentLevel}";
                if (criticalDamageNextLevelText != null)
                    criticalDamageNextLevelText.text = currentLevel >= maxLevel ? "MAX" : $"Nivel {currentLevel + 1} (+{nextImprovement})";
                if (criticalDamageTotalBonusText != null)
                    criticalDamageTotalBonusText.text = $"+{totalBonus}";
                break;
            case StatType.AttackSpeed:
                if (attackSpeedCostText != null)
                    attackSpeedCostText.text = ENERGY_COST.ToString();
                if (attackSpeedCurrentLevelText != null)
                    attackSpeedCurrentLevelText.text = $"Vel. Ataque: {currentLevel}";
                if (attackSpeedNextLevelText != null)
                    attackSpeedNextLevelText.text = currentLevel >= maxLevel ? "MAX" : $"Nivel {currentLevel + 1} (+{nextImprovement})";
                if (attackSpeedTotalBonusText != null)
                    attackSpeedTotalBonusText.text = $"+{totalBonus}";
                break;
            case StatType.Luck:
                if (luckCostText != null)
                    luckCostText.text = ENERGY_COST.ToString();
                if (luckCurrentLevelText != null)
                    luckCurrentLevelText.text = $"Suerte: {currentLevel}";
                if (luckNextLevelText != null)
                    luckNextLevelText.text = currentLevel >= maxLevel ? "MAX" : $"Nivel {currentLevel + 1} (+{nextImprovement})";
                if (luckTotalBonusText != null)
                    luckTotalBonusText.text = $"+{totalBonus}";
                break;
        }
    }
    
    /// <summary>
    /// Actualiza el texto de energía actual/máxima.
    /// </summary>
    private void RefreshEnergyUI()
    {
        if (energySystem == null || energyText == null)
            return;
        
        int currentEnergy = energySystem.GetCurrentEnergy();
        int maxEnergy = energySystem.GetMaxEnergy();
        
        energyText.text = $"{currentEnergy} / {maxEnergy}";
    }
    
    /// <summary>
    /// Actualiza el estado de todos los botones según la energía disponible.
    /// Los botones se deshabilitan si no hay suficiente energía (10) o si el stat está al máximo nivel.
    /// </summary>
    private void UpdateButtonsState()
    {
        if (energySystem == null)
            return;
        
        bool hasEnoughEnergy = energySystem.CanAfford(ENERGY_COST);
        
        PlayerProfileData profile = gameDataManager?.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Actualizar estado de cada botón
        UpdateButtonState(StatType.HP, hpButton, profile, hasEnoughEnergy);
        UpdateButtonState(StatType.Mana, manaButton, profile, hasEnoughEnergy);
        UpdateButtonState(StatType.Attack, attackButton, profile, hasEnoughEnergy);
        UpdateButtonState(StatType.Defense, defenseButton, profile, hasEnoughEnergy);
        UpdateButtonState(StatType.Skill, skillButton, profile, hasEnoughEnergy);
        UpdateButtonState(StatType.CriticalAttack, criticalAttackButton, profile, hasEnoughEnergy);
        UpdateButtonState(StatType.CriticalDamage, criticalDamageButton, profile, hasEnoughEnergy);
        UpdateButtonState(StatType.AttackSpeed, attackSpeedButton, profile, hasEnoughEnergy);
        UpdateButtonState(StatType.Luck, luckButton, profile, hasEnoughEnergy);
    }
    
    /// <summary>
    /// Actualiza el estado de un botón específico.
    /// </summary>
    private void UpdateButtonState(StatType statType, Button button, PlayerProfileData profile, bool hasEnoughEnergy)
    {
        if (button == null)
            return;
        
        int currentLevel = GetCurrentLevel(profile, statType);
        int maxLevel = GetMaxLevel(statType);
        
        // El botón está habilitado si:
        // 1. Hay suficiente energía
        // 2. El stat no está al máximo nivel
        bool isEnabled = hasEnoughEnergy && currentLevel < maxLevel;
        
        button.interactable = isEnabled;
    }
}

