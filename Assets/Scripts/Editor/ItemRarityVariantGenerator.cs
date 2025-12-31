using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Genera copias de todos los ItemData existentes para completar las 10 rarezas
/// (Plebeius → Divinus) aplicando los multiplicadores solicitados.
/// </summary>
public static class ItemRarityVariantGenerator
{
    private const float MultiplierStep = 1.2f;

    private static readonly RarityDefinition[] RaritySequence =
    {
        new RarityDefinition("Plebeius", scalesLevel: true),
        new RarityDefinition("Auxiliaris", scalesLevel: true),
        new RarityDefinition("Legionarius", scalesLevel: true),
        new RarityDefinition("Veteranus", scalesLevel: true),
        new RarityDefinition("Centurio",  scalesLevel: true),
        new RarityDefinition("Tribunus",  scalesLevel: false),
        new RarityDefinition("Praetorianus", scalesLevel: false),
        new RarityDefinition("Imperialis", scalesLevel: false),
        new RarityDefinition("Augustus",  scalesLevel: false),
        new RarityDefinition("Divinus",   scalesLevel: false),
    };

    [MenuItem("Tools/Items/Generate Rarity Variants")]
    public static void GenerateVariants()
    {
        string[] itemGuids = AssetDatabase.FindAssets("t:ItemData");
        if (itemGuids == null || itemGuids.Length == 0)
        {
            Debug.LogWarning("ItemRarityVariantGenerator: No se encontraron ItemData en el proyecto.");
            return;
        }

        int createdAssets = 0;
        foreach (string guid in itemGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ItemData baseItem = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            if (baseItem == null)
                continue;

            for (int rarityIndex = 1; rarityIndex < RaritySequence.Length; rarityIndex++)
            {
                RarityDefinition rarityDef = RaritySequence[rarityIndex];
                string variantName = $"{baseItem.itemName} - {rarityDef.DisplayName}";
                string variantPath = BuildVariantPath(assetPath, rarityDef.DisplayName);

                if (File.Exists(variantPath))
                    continue; // Ya existe la variante, no duplicar

                ItemData variant = ScriptableObject.Instantiate(baseItem);
                ApplyScaling(baseItem, variant, rarityIndex, rarityDef);

                variant.itemName = variantName;
                variant.rareza = rarityDef.DisplayName;

                AssetDatabase.CreateAsset(variant, variantPath);
                createdAssets++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"ItemRarityVariantGenerator: Variantes generadas: {createdAssets}");
    }

    private static void ApplyScaling(ItemData baseItem, ItemData target, int rarityIndex, RarityDefinition rarityDef)
    {
        float factor = Mathf.Pow(MultiplierStep, rarityIndex);

        target.price = Mathf.Max(0, Mathf.RoundToInt(baseItem.price * factor));
        target.hp = Mathf.Max(0, Mathf.RoundToInt(baseItem.hp * factor));
        target.mana = Mathf.Max(0, Mathf.RoundToInt(baseItem.mana * factor));
        target.ataque = Mathf.Max(0, Mathf.RoundToInt(baseItem.ataque * factor));
        target.defensa = Mathf.Max(0, Mathf.RoundToInt(baseItem.defensa * factor));
        target.velocidadAtaque = Mathf.Max(0, Mathf.RoundToInt(baseItem.velocidadAtaque * factor));
        target.ataqueCritico = Mathf.Max(0, Mathf.RoundToInt(baseItem.ataqueCritico * factor));
        target.danoCritico = Mathf.Max(0, Mathf.RoundToInt(baseItem.danoCritico * factor));
        target.suerte = Mathf.Max(0, Mathf.RoundToInt(baseItem.suerte * factor));
        target.destreza = Mathf.Max(0, Mathf.RoundToInt(baseItem.destreza * factor));

        if (rarityDef.ScalesLevel)
        {
            target.nivel = Mathf.Max(1, Mathf.RoundToInt(baseItem.nivel * factor));
            target.requiredHeroLevel = Mathf.Max(1, Mathf.RoundToInt(baseItem.requiredHeroLevel * factor));
        }
        else
        {
            target.nivel = baseItem.nivel;
            target.requiredHeroLevel = baseItem.requiredHeroLevel;
        }
    }

    private static string BuildVariantPath(string originalPath, string rarityName)
    {
        string directory = Path.GetDirectoryName(originalPath);
        string originalFile = Path.GetFileNameWithoutExtension(originalPath);
        string variantFile = $"{originalFile}_{rarityName}.asset";
        string combined = Path.Combine(directory ?? "Assets", variantFile);
        return combined.Replace("\\", "/");
    }

    private readonly struct RarityDefinition
    {
        public string DisplayName { get; }
        public bool ScalesLevel { get; }

        public RarityDefinition(string displayName, bool scalesLevel)
        {
            DisplayName = displayName;
            ScalesLevel = scalesLevel;
        }
    }
}
