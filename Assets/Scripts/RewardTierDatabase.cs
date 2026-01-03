using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de gestión de tiers para recompensas de combate.
/// Permite categorizar objetos por tiers y asignar recompensas aleatorias basadas en configuración de enemigos.
/// </summary>
[CreateAssetMenu(fileName = "Reward Tier Database", menuName = "Combate/Reward Tier Database")]
public class RewardTierDatabase : ScriptableObject
{
    [System.Serializable]
    public class RarityWeight
    {
        [Tooltip("Identificador exacto de la rareza (ej. \"Plebeius\")")]
        public string rarityId;
        [Tooltip("Peso relativo. Si todos los pesos valen 1, la probabilidad es uniforme.")]
        public int weight = 1;
    }

    [System.Serializable]
    public class ItemTypeWeight
    {
        public ItemType itemType;
        [Tooltip("Peso relativo para este tipo dentro del tier.")]
        public int weight = 1;
        [Tooltip("Pesos de rareza específicos para este tipo (opcional).")]
        public RarityWeight[] rarityWeights;
    }

    [System.Serializable]
    public class TierDropConfig
    {
        [Tooltip("Distribución de tipos de objeto dentro de este tier.")]
        public ItemTypeWeight[] itemTypeWeights;
    }

    [Header("Configuración de Tiers")]
    [Tooltip("Número de tiers disponibles (ej: 5 tiers del 1 al 5)")]
    [SerializeField] private int tierCount = 5;
    
    [Tooltip("Arrays de objetos por tier. Arrastra aquí los ItemData correspondientes a cada tier.")]
    [SerializeField] private ItemData[] tier1Items;
    [SerializeField] private ItemData[] tier2Items;
    [SerializeField] private ItemData[] tier3Items;
    [SerializeField] private ItemData[] tier4Items;
    [SerializeField] private ItemData[] tier5Items;
    [Header("Configuración avanzada por tier (opcional)")]
    [SerializeField] private TierDropConfig tier1Config;
    [SerializeField] private TierDropConfig tier2Config;
    [SerializeField] private TierDropConfig tier3Config;
    [SerializeField] private TierDropConfig tier4Config;
    [SerializeField] private TierDropConfig tier5Config;
    
    [Header("Configuración de Probabilidades")]
    [Tooltip("Probabilidad base para cada tier (debe sumar 100).")]
    [Range(0, 100)]
    [SerializeField] private int tier1Probability = 40;
    [Range(0, 100)]
    [SerializeField] private int tier2Probability = 30;
    [Range(0, 100)]
    [SerializeField] private int tier3Probability = 20;
    [Range(0, 100)]
    [SerializeField] private int tier4Probability = 8;
    [Range(0, 100)]
    [SerializeField] private int tier5Probability = 2;
    
    // Cache para arrays de tiers
    private ItemData[][] tierArrays;
    private int[] tierProbabilities;
    
    private void OnEnable()
    {
        InitializeArrays();
    }
    
    /// <summary>
    /// Inicializa los arrays cache para acceso rápido.
    /// </summary>
    private void InitializeArrays()
    {
        tierArrays = new ItemData[tierCount][];
        tierArrays[0] = tier1Items;
        tierArrays[1] = tier2Items;
        tierArrays[2] = tier3Items;
        tierArrays[3] = tier4Items;
        tierArrays[4] = tier5Items;
        
        tierProbabilities = new int[tierCount];
        tierProbabilities[0] = tier1Probability;
        tierProbabilities[1] = tier2Probability;
        tierProbabilities[2] = tier3Probability;
        tierProbabilities[3] = tier4Probability;
        tierProbabilities[4] = tier5Probability;
    }
    
    /// <summary>
    /// Obtiene un objeto aleatorio de un tier específico.
    /// </summary>
    /// <param name="tier">Tier del 1 al 5</param>
    /// <returns>ItemData aleatorio del tier especificado, o null si no hay objetos</returns>
    public ItemData GetRandomItemFromTier(int tier)
    {
        if (tier < 1 || tier > tierCount)
        {
            Debug.LogWarning($"Tier inválido: {tier}. Los tiers válidos son 1-{tierCount}");
            return null;
        }
        
        int tierIndex = tier - 1;
        ItemData[] items = tierArrays[tierIndex];
        
        if (items == null || items.Length == 0)
        {
            Debug.LogWarning($"No hay objetos configurados para el tier {tier}");
            return null;
        }
        
        // Filtrar objetos no nulos
        List<ItemData> validItems = new List<ItemData>();
        foreach (var item in items)
        {
            if (item != null)
                validItems.Add(item);
        }
        
        if (validItems.Count == 0)
        {
            Debug.LogWarning($"No hay objetos válidos en el tier {tier}");
            return null;
        }
        
        // Seleccionar objeto aleatorio
        int randomIndex = Random.Range(0, validItems.Count);
        return validItems[randomIndex];
    }

    /// <summary>
    /// Obtiene un objeto aleatorio de un tier aplicando pesos por tipo y rareza.
    /// Si no hay configuración válida, cae de nuevo en GetRandomItemFromTier.
    /// </summary>
    public ItemData GetRandomItemFromTierWeightedByType(int tier)
    {
        if (tier < 1 || tier > tierCount)
            return GetRandomItemFromTier(tier);

        TierDropConfig config = GetTierConfig(tier);
        if (config == null || config.itemTypeWeights == null || config.itemTypeWeights.Length == 0)
            return GetRandomItemFromTier(tier);

        int tierIndex = tier - 1;
        ItemData[] tierItems = tierArrays[tierIndex];
        if (tierItems == null || tierItems.Length == 0)
            return GetRandomItemFromTier(tier);

        Dictionary<ItemType, List<ItemData>> buckets = BuildBucketsByType(tierItems);
        ItemTypeWeight selectedType = PickItemType(config.itemTypeWeights, buckets);
        if (selectedType == null)
            return GetRandomItemFromTier(tier);

        List<ItemData> typePool = buckets[selectedType.itemType];
        List<ItemData> filteredByRarity = FilterByRarity(typePool, selectedType.rarityWeights);

        List<ItemData> finalPool = filteredByRarity.Count > 0 ? filteredByRarity : typePool;
        if (finalPool.Count == 0)
            return GetRandomItemFromTier(tier);

        int randomIndex = Random.Range(0, finalPool.Count);
        return finalPool[randomIndex];
    }
    
    /// <summary>
    /// Obtiene un tier aleatorio basado en las probabilidades configuradas.
    /// </summary>
    /// <returns>Número de tier del 1 al 5</returns>
    public int GetRandomTier()
    {
        int totalProbability = 0;
        foreach (int prob in tierProbabilities)
        {
            totalProbability += prob;
        }
        
        if (totalProbability == 0)
        {
            Debug.LogWarning("Las probabilidades de tier suman 0. Usando distribución uniforme.");
            return Random.Range(1, tierCount + 1);
        }
        
        int randomValue = Random.Range(0, totalProbability);
        int currentSum = 0;
        
        for (int i = 0; i < tierCount; i++)
        {
            currentSum += tierProbabilities[i];
            if (randomValue < currentSum)
            {
                return i + 1; // +1 porque los tiers son 1-based
            }
        }
        
        return tierCount; // Fallback al último tier
    }
    
    /// <summary>
    /// Genera recompensas basadas en la configuración de un enemigo.
    /// </summary>
    /// <param name="enemyRewards">Configuración de recompensas del enemigo</param>
    /// <returns>Lista de objetos obtenidos</returns>
    public List<ItemData> GenerateRewards(EnemyRewardConfig enemyRewards)
    {
        List<ItemData> rewards = new List<ItemData>();
        
        if (enemyRewards == null)
        {
            Debug.LogWarning("EnemyRewardConfig es nulo");
            return rewards;
        }
        
        int totalRewards = enemyRewards.rewardItemCount;
        int[] allowedTiers = enemyRewards.allowedTiers;
        
        if (totalRewards <= 0)
        {
            return rewards; // Sin recompensas de objetos
        }
        
        // Estrategia de distribución
        if (allowedTiers == null || allowedTiers.Length == 0)
        {
            // Sin tiers específicos - usar probabilidades globales
            for (int i = 0; i < totalRewards; i++)
            {
                int randomTier = GetRandomTier();
                ItemData item = GetRandomItemFromTier(randomTier);
                if (item != null)
                    rewards.Add(item);
            }
        }
        else
        {
            // Con tiers específicos - distribuir según estrategia
            if (enemyRewards.distributeEvenly && allowedTiers.Length > 1)
            {
                // Distribución equitativa: uno de cada tier permitido
                int rewardsPerTier = Mathf.CeilToInt((float)totalRewards / allowedTiers.Length);
                
                for (int i = 0; i < totalRewards && i < allowedTiers.Length * rewardsPerTier; i++)
                {
                    int tierIndex = i % allowedTiers.Length;
                    int tier = allowedTiers[tierIndex];
                    ItemData item = GetRandomItemFromTier(tier);
                    if (item != null)
                        rewards.Add(item);
                }
            }
            else
            {
                // Distribución aleatoria desde los tiers permitidos
                for (int i = 0; i < totalRewards; i++)
                {
                    int randomTierIndex = Random.Range(0, allowedTiers.Length);
                    int tier = allowedTiers[randomTierIndex];
                    ItemData item = GetRandomItemFromTier(tier);
                    if (item != null)
                        rewards.Add(item);
                }
            }
        }
        
        return rewards;
    }
    
    /// <summary>
    /// Valida que la configuración de tiers sea correcta.
    /// </summary>
    public bool ValidateConfiguration()
    {
        bool isValid = true;
        
        // Verificar que cada tier tenga objetos
        for (int i = 0; i < tierCount; i++)
        {
            ItemData[] items = tierArrays[i];
            if (items == null || items.Length == 0)
            {
                Debug.LogWarning($"El tier {i + 1} no tiene objetos configurados");
                isValid = false;
            }
            else
            {
                int validItems = 0;
                foreach (var item in items)
                {
                    if (item != null)
                        validItems++;
                }
                
                if (validItems == 0)
                {
                    Debug.LogWarning($"El tier {i + 1} no tiene objetos válidos (todos son nulos)");
                    isValid = false;
                }
            }
        }
        
        // Verificar que las probabilidades sumen 100
        int totalProbability = 0;
        foreach (int prob in tierProbabilities)
        {
            totalProbability += prob;
        }
        
        if (totalProbability != 100)
        {
            Debug.LogWarning($"Las probabilidades de tier suman {totalProbability}, deberían sumar 100");
            isValid = false;
        }
        
        if (isValid)
        {
            Debug.Log("Configuración de RewardTierDatabase validada correctamente");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Obtiene información para debug de todos los tiers.
    /// </summary>
    public void DebugTierInfo()
    {
        Debug.Log("=== Reward Tier Database Info ===");
        for (int i = 0; i < tierCount; i++)
        {
            ItemData[] items = tierArrays[i];
            int validItems = 0;
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item != null)
                        validItems++;
                }
            }
            Debug.Log($"Tier {i + 1}: {validItems} objetos válidos, {tierProbabilities[i]}% probabilidad");
        }
    }

    private TierDropConfig GetTierConfig(int tier)
    {
        switch (tier)
        {
            case 1: return tier1Config;
            case 2: return tier2Config;
            case 3: return tier3Config;
            case 4: return tier4Config;
            case 5: return tier5Config;
            default: return null;
        }
    }

    private Dictionary<ItemType, List<ItemData>> BuildBucketsByType(ItemData[] tierItems)
    {
        Dictionary<ItemType, List<ItemData>> buckets = new Dictionary<ItemType, List<ItemData>>();
        if (tierItems == null)
            return buckets;

        foreach (var item in tierItems)
        {
            if (item == null)
                continue;

            ItemType type = item.itemType;
            if (!buckets.TryGetValue(type, out var list))
            {
                list = new List<ItemData>();
                buckets[type] = list;
            }
            list.Add(item);
        }

        return buckets;
    }

    private ItemTypeWeight PickItemType(ItemTypeWeight[] weights, Dictionary<ItemType, List<ItemData>> buckets)
    {
        List<ItemTypeWeight> validWeights = new List<ItemTypeWeight>();
        int totalWeight = 0;

        foreach (var weight in weights)
        {
            if (weight == null || weight.weight <= 0)
                continue;

            if (buckets.TryGetValue(weight.itemType, out var list) && list.Count > 0)
            {
                validWeights.Add(weight);
                totalWeight += weight.weight;
            }
        }

        if (totalWeight <= 0 || validWeights.Count == 0)
            return null;

        int randomValue = Random.Range(0, totalWeight);
        int currentSum = 0;
        foreach (var weight in validWeights)
        {
            currentSum += weight.weight;
            if (randomValue < currentSum)
            {
                return weight;
            }
        }

        return validWeights[validWeights.Count - 1];
    }

    private List<ItemData> FilterByRarity(List<ItemData> items, RarityWeight[] rarityWeights)
    {
        List<ItemData> result = new List<ItemData>();
        if (items == null || items.Count == 0 || rarityWeights == null || rarityWeights.Length == 0)
            return result;

        List<RarityWeight> validWeights = new List<RarityWeight>();
        int totalWeight = 0;
        foreach (var rarity in rarityWeights)
        {
            if (rarity == null || rarity.weight <= 0 || string.IsNullOrWhiteSpace(rarity.rarityId))
                continue;

            bool exists = items.Exists(item => item != null && item.rareza == rarity.rarityId);
            if (exists)
            {
                validWeights.Add(rarity);
                totalWeight += rarity.weight;
            }
        }

        if (totalWeight <= 0 || validWeights.Count == 0)
            return result;

        int randomValue = Random.Range(0, totalWeight);
        int currentSum = 0;
        string selectedRarity = validWeights[validWeights.Count - 1].rarityId;
        foreach (var rarity in validWeights)
        {
            currentSum += rarity.weight;
            if (randomValue < currentSum)
            {
                selectedRarity = rarity.rarityId;
                break;
            }
        }

        foreach (var item in items)
        {
            if (item != null && item.rareza == selectedRarity)
            {
                result.Add(item);
            }
        }

        return result;
    }
}
