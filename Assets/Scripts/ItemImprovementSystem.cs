using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>
/// Sistema de mejora de objetos (Forja).
/// Solo mejora objetos equipados (ItemInstance).
/// Aumenta nivel de 1 en 1 hasta máximo 999.
/// Suma puntos fijos de stats (no porcentual) según nivel.
/// </summary>
public class ItemImprovementSystem : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al EquipmentManager para obtener items equipados")]
    [SerializeField] private EquipmentManager equipmentManager;

    [Tooltip("Referencia al sistema de dinero del jugador")]
    [SerializeField] private PlayerMoney playerMoney;

    [Header("Configuración de Mejora")]
    [Tooltip("Nivel máximo al que se puede mejorar un objeto")]
    [Range(1, 999)]
    [SerializeField] private int maxLevel = 999;

    [Header("Coste de Mejora")]
    [Tooltip("Fórmula de coste: coste = baseCost * (nivelActual ^ costMultiplier)")]
    [SerializeField] private int baseCost = 1;

    [Tooltip("Multiplicador de coste por nivel (mayor = más caro subir nivel)")]
    [SerializeField] private float costMultiplier = 1.2f;

    [Header("Coste por Rareza")]
    [Tooltip("Configuración opcional para multiplicadores de coste por rareza")]
    [SerializeField] private List<RarityCostEntry> rarityCostEntries = new List<RarityCostEntry>
    {
        new RarityCostEntry { rarityKey = "Aprendiz", multiplier = 1f },
        new RarityCostEntry { rarityKey = "Auxiliaris", multiplier = 2f },
        new RarityCostEntry { rarityKey = "Legionarius", multiplier = 3f },
        new RarityCostEntry { rarityKey = "Veteranus", multiplier = 4f },
        new RarityCostEntry { rarityKey = "Centurio", multiplier = 5f },
        new RarityCostEntry { rarityKey = "Tribunus", multiplier = 6f },
        new RarityCostEntry { rarityKey = "Praetorianus", multiplier = 7f },
        new RarityCostEntry { rarityKey = "Imperialis", multiplier = 8f },
        new RarityCostEntry { rarityKey = "Augustus", multiplier = 9f },
        new RarityCostEntry { rarityKey = "Apocalipsis", multiplier = 10f },
        new RarityCostEntry { rarityKey = "Abismo", multiplier = 10f }
    };

    // Eventos
    public System.Action<ItemInstance, int, int> OnItemImproved; // item, nivelAnterior, nivelNuevo
    public System.Action<ItemInstance> OnImprovementFailed; // item, razón del fallo

    public int MaxLevel => maxLevel;

    private const float DefaultRarityMultiplier = 1f;
    private readonly Dictionary<string, float> rarityMultiplierLookup = new Dictionary<string, float>();

    private void Awake()
    {
        RebuildRarityLookup();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RebuildRarityLookup();
    }
#endif

    /// <summary>
    /// Mejora un item equipado en el slot especificado.
    /// Solo mejora objetos equipados.
    /// </summary>
    /// <param name="slot">Slot del equipo a mejorar</param>
    /// <returns>True si se mejoró exitosamente, false si falló (no equipado, nivel máximo, dinero insuficiente, etc.)</returns>
    public bool ImproveEquippedItem(EquipmentManager.EquipmentSlotType slot)
    {
        if (equipmentManager == null)
        {
            Debug.LogError("EquipmentManager no está asignado.");
            OnImprovementFailed?.Invoke(null);
            return false;
        }

        ItemInstance itemInstance = equipmentManager.GetEquippedItem(slot);
        if (itemInstance == null || !itemInstance.IsValid())
        {
            Debug.LogWarning($"No hay item equipado en el slot {slot} para mejorar.");
            OnImprovementFailed?.Invoke(itemInstance);
            return false;
        }

        return ImproveItem(itemInstance);
    }

    /// <summary>
    /// Mejora un ItemInstance directamente.
    /// </summary>
    /// <param name="itemInstance">ItemInstance a mejorar</param>
    /// <returns>True si se mejoró exitosamente, false si falló</returns>
    public bool ImproveItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
        {
            Debug.LogWarning("Intentando mejorar un ItemInstance nulo o inválido.");
            OnImprovementFailed?.Invoke(itemInstance);
            return false;
        }

        // Verificar nivel máximo
        if (itemInstance.currentLevel >= maxLevel)
        {
            Debug.LogWarning($"El item '{itemInstance.GetItemName()}' ya está en el nivel máximo ({maxLevel}).");
            OnImprovementFailed?.Invoke(itemInstance);
            return false;
        }

        if (!TryGetHeroLevel(out int heroLevel))
        {
            Debug.LogWarning("ItemImprovementSystem: No se pudo obtener el nivel del héroe.");
            OnImprovementFailed?.Invoke(itemInstance);
            return false;
        }

        int nextLevel = Mathf.Min(itemInstance.currentLevel + 1, maxLevel);
        if (heroLevel < nextLevel)
        {
            Debug.LogWarning($"El héroe necesita nivel {nextLevel} para mejorar '{itemInstance.GetItemName()}' (nivel actual del héroe: {heroLevel}).");
            OnImprovementFailed?.Invoke(itemInstance);
            return false;
        }

        // Calcular coste de mejora
        int improvementCost = CalculateImprovementCost(itemInstance);
        
        // Verificar dinero suficiente
        if (playerMoney == null)
        {
            Debug.LogError("PlayerMoney no está asignado. No se puede validar el dinero.");
            OnImprovementFailed?.Invoke(itemInstance);
            return false;
        }

        if (playerMoney.GetMoney() < improvementCost)
        {
            Debug.LogWarning($"Dinero insuficiente para mejorar. Necesitas {improvementCost}, tienes {playerMoney.GetMoney()}.");
            OnImprovementFailed?.Invoke(itemInstance);
            return false;
        }

        // Realizar la mejora
        int previousLevel = itemInstance.currentLevel;
        playerMoney.SubtractMoney(improvementCost);
        itemInstance.LevelUp();

        // SOLUCIÓN: Guardar el perfil ANTES de invocar el evento
        // Esto asegura que cuando los listeners se ejecuten, el perfil ya esté actualizado
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SavePlayerProfile();
        }

        OnItemImproved?.Invoke(itemInstance, previousLevel, itemInstance.currentLevel);
        Debug.Log($"Item '{itemInstance.GetItemName()}' mejorado de nivel {previousLevel} a {itemInstance.currentLevel}. Coste: {improvementCost}.");

        return true;
    }

    /// <summary>
    /// Calcula el coste total de mejora teniendo en cuenta nivel y rareza.
    /// </summary>
    public int CalculateImprovementCost(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
        {
            return Mathf.Max(1, CalculateLevelCost(1));
        }

        int levelCost = CalculateLevelCost(itemInstance.currentLevel);
        float rarityMultiplier = GetRarityMultiplier(itemInstance);
        int rarityCost = Mathf.RoundToInt(baseCost * rarityMultiplier);
        return Mathf.Max(1, levelCost + rarityCost);
    }

    private int CalculateLevelCost(int currentLevel)
    {
        if (currentLevel < 1)
            currentLevel = 1;

        float cost = baseCost * Mathf.Pow(currentLevel, costMultiplier);
        return Mathf.RoundToInt(cost);
    }

    /// <summary>
    /// Obtiene el coste de mejorar un item equipado en el slot especificado.
    /// </summary>
    /// <param name="slot">Slot del equipo</param>
    /// <returns>Coste en monedas, o -1 si no hay item equipado o está en nivel máximo</returns>
    public int GetImprovementCost(EquipmentManager.EquipmentSlotType slot)
    {
        if (equipmentManager == null)
            return -1;

        ItemInstance itemInstance = equipmentManager.GetEquippedItem(slot);
        if (itemInstance == null || !itemInstance.IsValid())
            return -1;

        if (itemInstance.currentLevel >= maxLevel)
            return -1;

        return CalculateImprovementCost(itemInstance);
    }

    /// <summary>
    /// Obtiene las estadísticas proyectadas de un item si se mejora al siguiente nivel.
    /// </summary>
    /// <param name="itemInstance">ItemInstance a analizar</param>
    /// <returns>ItemStats del siguiente nivel, o stats actuales si está en nivel máximo</returns>
    public ItemStats GetProjectedStats(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
        {
            return new ItemStats();
        }

        int nextLevel = Mathf.Min(itemInstance.currentLevel + 1, maxLevel);
        return itemInstance.GetStatsAtLevel(nextLevel);
    }

    /// <summary>
    /// Obtiene las estadísticas proyectadas de un item equipado en el slot especificado.
    /// </summary>
    /// <param name="slot">Slot del equipo</param>
    /// <returns>ItemStats del siguiente nivel, o stats actuales si no hay item o está en nivel máximo</returns>
    public ItemStats GetProjectedStats(EquipmentManager.EquipmentSlotType slot)
    {
        if (equipmentManager == null)
            return new ItemStats();

        ItemInstance itemInstance = equipmentManager.GetEquippedItem(slot);
        if (itemInstance == null || !itemInstance.IsValid())
            return new ItemStats();

        return GetProjectedStats(itemInstance);
    }

    /// <summary>
    /// Verifica si un item equipado puede ser mejorado.
    /// </summary>
    /// <param name="slot">Slot del equipo</param>
    /// <returns>True si puede ser mejorado (está equipado, no está en nivel máximo, hay dinero suficiente)</returns>
    public bool CanImproveItem(EquipmentManager.EquipmentSlotType slot)
    {
        if (equipmentManager == null || playerMoney == null)
            return false;

        ItemInstance itemInstance = equipmentManager.GetEquippedItem(slot);
        if (itemInstance == null || !itemInstance.IsValid())
            return false;

        if (itemInstance.currentLevel >= maxLevel)
            return false;

        if (!TryGetHeroLevel(out int heroLevel))
            return false;

        int nextLevel = Mathf.Min(itemInstance.currentLevel + 1, maxLevel);
        if (heroLevel < nextLevel)
            return false;

        int cost = CalculateImprovementCost(itemInstance);
        return playerMoney.GetMoney() >= cost;
    }

    /// <summary>
    /// Obtiene información de mejora de un item equipado.
    /// </summary>
    /// <param name="slot">Slot del equipo</param>
    /// <returns>Estructura con información de mejora, o null si no hay item equipado</returns>
    public ImprovementInfo GetImprovementInfo(EquipmentManager.EquipmentSlotType slot)
    {
        if (equipmentManager == null)
            return null;

        ItemInstance itemInstance = equipmentManager.GetEquippedItem(slot);
        if (itemInstance == null || !itemInstance.IsValid())
            return null;

        TryGetHeroLevel(out int heroLevel);
        int nextLevel = Mathf.Min(itemInstance.currentLevel + 1, maxLevel);

        ImprovementInfo info = new ImprovementInfo
        {
            itemInstance = itemInstance,
            currentLevel = itemInstance.currentLevel,
            maxLevel = maxLevel,
            canImprove = itemInstance.currentLevel < maxLevel,
            currentStats = itemInstance.GetFinalStats(),
            projectedStats = GetProjectedStats(itemInstance),
            improvementCost = itemInstance.currentLevel < maxLevel ? CalculateImprovementCost(itemInstance) : -1,
            hasEnoughMoney = playerMoney != null && playerMoney.GetMoney() >= (itemInstance.currentLevel < maxLevel ? CalculateImprovementCost(itemInstance) : 0),
            heroLevel = heroLevel,
            requiredHeroLevelForNextUpgrade = nextLevel,
            heroMeetsLevelRequirement = heroLevel >= nextLevel
        };

        return info;
    }

    private bool TryGetHeroLevel(out int heroLevel)
    {
        heroLevel = 0;
        if (GameDataManager.Instance == null)
            return false;

        PlayerProfileData profile = GameDataManager.Instance.GetPlayerProfile();
        if (profile == null)
            return false;

        heroLevel = profile.heroLevel;
        return true;
    }

    private float GetRarityMultiplier(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
            return DefaultRarityMultiplier;

        string rarityKey = itemInstance.GetRarity();
        if (string.IsNullOrWhiteSpace(rarityKey) && itemInstance.baseItem != null)
        {
            rarityKey = itemInstance.baseItem.rareza;
        }

        return GetRarityMultiplier(rarityKey);
    }

    private float GetRarityMultiplier(string rarityKey)
    {
        string normalized = NormalizeRarityKey(rarityKey);
        if (!string.IsNullOrEmpty(normalized) && rarityMultiplierLookup.TryGetValue(normalized, out float multiplier))
        {
            return multiplier;
        }

        return DefaultRarityMultiplier;
    }

    private void RebuildRarityLookup()
    {
        rarityMultiplierLookup.Clear();

        if (rarityCostEntries == null)
            return;

        foreach (var entry in rarityCostEntries)
        {
            if (entry == null)
                continue;

            string normalized = NormalizeRarityKey(entry.rarityKey);
            if (string.IsNullOrEmpty(normalized))
                continue;

            float multiplier = entry.multiplier <= 0f ? DefaultRarityMultiplier : entry.multiplier;
            rarityMultiplierLookup[normalized] = multiplier;
        }
    }

    private static string NormalizeRarityKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string trimmed = value.Trim().ToLowerInvariant();
        string normalized = trimmed.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (char c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            builder.Append(c);
        }

        return builder.ToString();
    }
}

/// <summary>
/// Estructura con información sobre la mejora de un item.
/// </summary>
[System.Serializable]
public class ImprovementInfo
{
    public ItemInstance itemInstance;
    public int currentLevel;
    public int maxLevel;
    public bool canImprove;
    public ItemStats currentStats;
    public ItemStats projectedStats;
    public int improvementCost;
    public bool hasEnoughMoney;
    public int heroLevel;
    public int requiredHeroLevelForNextUpgrade;
    public bool heroMeetsLevelRequirement;
}

[System.Serializable]
public class RarityCostEntry
{
    [Tooltip("Nombre de la rareza (se normaliza ignorando mayúsculas/acentos)")]
    public string rarityKey;

    [Tooltip("Multiplicador adicional aplicado al coste base")]
    public float multiplier = 1f;
}

