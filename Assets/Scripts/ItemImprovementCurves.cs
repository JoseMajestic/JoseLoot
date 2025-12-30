using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>
/// Define una curva de mejora irregular para todos los items forjados y
/// asigna multiplicadores por set/tier para escalarla desde Aprendiz I hasta Apocalipsis III.
/// </summary>
public static class ItemImprovementCurves
{
    public const int MaxLevel = 1000;

    private const int TargetTotalBonus = 1000;
    private const int MaxIncrementPerLevel = 3;
    private const int BaseCurveLength = MaxLevel - 1; // El nivel 1 no recibe bonificación
    private const int DefaultMaxZeroRun = 2;
    private const int ExtraZeroRunForFirstStat = 3;
    private const int RandomSeed = 20251230;
    private const int StatCount = 9;

    private static readonly int[,] cumulativeBonusByStat = BuildStatCumulativeCurves();
    private static readonly List<SetTierDefinition> orderedSetTiers = BuildSetTierDefinitions();

    /// <summary>
    /// Obtiene la bonificación acumulada de un stat específico para un item y nivel concretos.
    /// </summary>
    public static int GetStatBonus(ItemData itemData, int level, ItemStatType statType)
    {
        if (itemData == null)
            return 0;

        int statIndex = (int)statType;
        if (statIndex < 0 || statIndex >= StatCount)
            return 0;

        int clampedLevel = Mathf.Clamp(level, 1, MaxLevel);
        int baseBonus = cumulativeBonusByStat[statIndex, clampedLevel];
        int multiplier = GetSetMultiplier(itemData);
        return baseBonus * multiplier;
    }

    /// <summary>
    /// Devuelve el factor de multiplicación asociado a un ScriptableObject concreto (set y tier).
    /// </summary>
    public static int GetSetMultiplier(ItemData itemData)
    {
        if (itemData == null)
            return 1;

        string normalizedName = NormalizeItemName(itemData);

        foreach (var definition in orderedSetTiers)
        {
            if (!normalizedName.Contains(definition.SetToken))
                continue;

            for (int tierIndex = 0; tierIndex < definition.TierTokens.Length; tierIndex++)
            {
                if (normalizedName.Contains(definition.TierTokens[tierIndex]))
                {
                    return definition.BaseMultiplier + tierIndex;
                }
            }
        }

        return 1;
    }

    private static int[,] BuildStatCumulativeCurves()
    {
        int[,] cumulative = new int[StatCount, MaxLevel + 1];

        for (int statIndex = 0; statIndex < StatCount; statIndex++)
        {
            int[] increments = GenerateIncrementCurve(statIndex);
            int[] statCumulative = BuildCumulativeCurve(increments);

            for (int level = 1; level <= MaxLevel; level++)
            {
                cumulative[statIndex, level] = statCumulative[level];
            }
        }

        return cumulative;
    }

    private static int[] GenerateIncrementCurve(int statIndex)
    {
        int[] increments = new int[BaseCurveLength];
        int statSeed = RandomSeed + (statIndex + 1) * 9973;
        var rng = new System.Random(statSeed);
        int remainingSum = TargetTotalBonus;
        int zeroRun = 0;
        int maxZeroRun = statIndex == 0 ? ExtraZeroRunForFirstStat : DefaultMaxZeroRun;

        for (int i = 0; i < BaseCurveLength; i++)
        {
            int slotsLeft = BaseCurveLength - i;
            int minValue = Math.Max(0, remainingSum - (slotsLeft - 1) * MaxIncrementPerLevel);
            int maxValue = Math.Min(MaxIncrementPerLevel, remainingSum);

            int adjustedMin = zeroRun >= maxZeroRun ? Math.Max(1, minValue) : minValue;

            // Introducir una ligera preferencia para valores diferentes entre stats
            int preferred = (i + statIndex) % (MaxIncrementPerLevel + 1);
            preferred = Mathf.Clamp(preferred, adjustedMin, maxValue);

            int value = RandomRangeInclusive(rng, adjustedMin, maxValue);
            if (Math.Abs(value - preferred) > 1 && preferred >= adjustedMin && preferred <= maxValue && rng.NextDouble() < 0.35)
            {
                value = preferred;
            }

            // Asegurar que el valor elegido no impida alcanzar la suma objetivo
            int maxFuture = (slotsLeft - 1) * MaxIncrementPerLevel;
            while (remainingSum - value > maxFuture && value > adjustedMin)
            {
                value--;
            }

            increments[i] = value;
            remainingSum -= value;
            zeroRun = value == 0 ? zeroRun + 1 : 0;
        }

        if (remainingSum != 0)
        {
            increments[BaseCurveLength - 1] += remainingSum;
        }

        return increments;
    }

    private static int[] BuildCumulativeCurve(int[] increments)
    {
        int[] cumulative = new int[MaxLevel + 1];
        cumulative[1] = 0;

        for (int level = 2; level <= MaxLevel; level++)
        {
            cumulative[level] = cumulative[level - 1] + increments[level - 2];
        }

        return cumulative;
    }

    private static int RandomRangeInclusive(System.Random rng, int min, int max)
    {
        if (min > max)
            return min;

        return rng.Next(min, max + 1);
    }

    private static List<SetTierDefinition> BuildSetTierDefinitions()
    {
        var definitions = new List<SetTierDefinition>();

        var orderedSets = new (string setToken, string[] tiers)[]
        {
            ("tiro", new[] { "rudimentum", "veteranum", "imperium" }),
            ("augur", new[] { "omen", "ritus", "oraculum" }),
            ("explorator", new[] { "peregrinus", "venator", "praedator" }),
            ("legionarius", new[] { "cohors", "centuria", "legio" }),
            ("roma", new[] { "virtus", "honor", "gloria" }),
            ("speculator", new[] { "umbra", "silentium", "occultus" }),
            ("colossus", new[] { "moles", "titanus", "colossus" }),
            ("pontifex", new[] { "sacrum", "divinus", "sanctus" }),
            ("infernum", new[] { "limbus", "inferus", "abyssus" }),
            ("ultima roma", new[] { "exitium", "cataclysmus", "aeternitas" })
        };

        int multiplier = 1;
        foreach (var set in orderedSets)
        {
            definitions.Add(new SetTierDefinition(
                NormalizeText(set.setToken),
                NormalizeTiers(set.tiers),
                multiplier));
            multiplier += set.tiers.Length;
        }

        return definitions;
    }

    private static string[] NormalizeTiers(string[] tiers)
    {
        string[] normalized = new string[tiers.Length];
        for (int i = 0; i < tiers.Length; i++)
        {
            normalized[i] = NormalizeText(tiers[i]);
        }
        return normalized;
    }

    private static string NormalizeItemName(ItemData itemData)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(itemData.itemName))
        {
            builder.Append(itemData.itemName);
            builder.Append('|');
        }

        if (!string.IsNullOrEmpty(itemData.name))
        {
            builder.Append(itemData.name);
        }

        return NormalizeText(builder.ToString());
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        string normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (char c in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString();
    }

    public enum ItemStatType
    {
        Hp = 0,
        Mana = 1,
        Ataque = 2,
        Defensa = 3,
        VelocidadAtaque = 4,
        AtaqueCritico = 5,
        DanoCritico = 6,
        Suerte = 7,
        Destreza = 8
    }

    private readonly struct SetTierDefinition
    {
        public SetTierDefinition(string setToken, string[] tierTokens, int baseMultiplier)
        {
            SetToken = setToken;
            TierTokens = tierTokens;
            BaseMultiplier = baseMultiplier;
        }

        public string SetToken { get; }
        public string[] TierTokens { get; }
        public int BaseMultiplier { get; }
    }
}
