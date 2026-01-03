using UnityEngine;

/// <summary>
/// Configuración de probabilidad por tipo de objeto y rareza dentro de un tier.
/// Crea un asset por tier y asígnalo al RewardTierDatabase.
/// </summary>
[CreateAssetMenu(menuName = "Loot/Tier Drop Config", fileName = "TierDropConfig")]
public class TierDropConfig : ScriptableObject
{
    public ItemTypeWeight[] itemTypeWeights;
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
public class RarityWeight
{
    [Tooltip("Identificador exacto de la rareza (ej. \"Plebeius\")")]
    public string rarityId;
    [Tooltip("Peso relativo. Si todos los pesos valen 1, la probabilidad es uniforme.")]
    public int weight = 1;
}
