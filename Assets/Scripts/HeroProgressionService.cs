using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Loads hero level progression increments from a CSV and applies them to the player profile.
/// Each level (2..1000) grants irregular stat bonuses that sum ~66k per stat by level 1000.
/// </summary>
public class HeroProgressionService : MonoBehaviour
{
    [Header("Progression Data")]
    [Tooltip("CSV file with columns: Level,HP,Mana,Ataque,Defensa,VelocidadAtaque,AtaqueCritico,DanoCritico,Destreza,Suerte")]
    [SerializeField] private TextAsset progressionCsv;

    private readonly Dictionary<int, StatIncrements> levelIncrements = new Dictionary<int, StatIncrements>();
    private bool isLoaded;

    private const int MinLevelWithBonus = 2;
    private const int MaxSupportedLevel = 1000;

    private void Awake()
    {
        EnsureLoaded();
    }

    private void EnsureLoaded()
    {
        if (isLoaded)
        {
            return;
        }

        levelIncrements.Clear();

        if (progressionCsv == null)
        {
            Debug.LogError("HeroProgressionService: progressionCsv is not assigned.");
            return;
        }

        var lines = progressionCsv.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogError("HeroProgressionService: CSV appears to be empty or header-only.");
            return;
        }

        // Skip header (line 0)
        for (int i = 1; i < lines.Length; i++)
        {
            var columns = lines[i].Split(',');
            if (columns.Length < 10)
            {
                Debug.LogWarning($"HeroProgressionService: line {i + 1} has insufficient columns ({columns.Length}). Skipping.");
                continue;
            }

            if (!int.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int level))
            {
                Debug.LogWarning($"HeroProgressionService: could not parse level on line {i + 1}. Value: '{columns[0]}'");
                continue;
            }

            if (level < MinLevelWithBonus || level > MaxSupportedLevel)
            {
                continue;
            }

            try
            {
                var inc = new StatIncrements
                {
                    hp = int.Parse(columns[1], CultureInfo.InvariantCulture),
                    mana = int.Parse(columns[2], CultureInfo.InvariantCulture),
                    ataque = int.Parse(columns[3], CultureInfo.InvariantCulture),
                    defensa = int.Parse(columns[4], CultureInfo.InvariantCulture),
                    velocidadAtaque = int.Parse(columns[5], CultureInfo.InvariantCulture),
                    ataqueCritico = int.Parse(columns[6], CultureInfo.InvariantCulture),
                    danoCritico = int.Parse(columns[7], CultureInfo.InvariantCulture),
                    destreza = int.Parse(columns[8], CultureInfo.InvariantCulture),
                    suerte = int.Parse(columns[9], CultureInfo.InvariantCulture)
                };

                levelIncrements[level] = inc;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HeroProgressionService: failed parsing stats on line {i + 1}. Error: {ex.Message}");
            }
        }

        isLoaded = true;
    }

    /// <summary>
    /// Applies any pending level bonuses up to the specified level.
    /// </summary>
    public void ApplyLevelBonuses(PlayerProfileData profile, int targetLevel)
    {
        if (profile == null)
        {
            return;
        }

        EnsureLoaded();
        if (!isLoaded || levelIncrements.Count == 0)
        {
            return;
        }

        targetLevel = Mathf.Clamp(targetLevel, MinLevelWithBonus, MaxSupportedLevel);
        if (profile.heroProgressionLevelApplied >= targetLevel)
        {
            return;
        }

        int startLevel = Mathf.Max(profile.heroProgressionLevelApplied + 1, MinLevelWithBonus);
        for (int level = startLevel; level <= targetLevel; level++)
        {
            if (levelIncrements.TryGetValue(level, out var inc))
            {
                profile.heroBonusHp += inc.hp;
                profile.heroBonusMana += inc.mana;
                profile.heroBonusAtaque += inc.ataque;
                profile.heroBonusDefensa += inc.defensa;
                profile.heroBonusVelocidadAtaque += inc.velocidadAtaque;
                profile.heroBonusAtaqueCritico += inc.ataqueCritico;
                profile.heroBonusDanoCritico += inc.danoCritico;
                profile.heroBonusDestreza += inc.destreza;
                profile.heroBonusSuerte += inc.suerte;
            }
        }

        profile.heroProgressionLevelApplied = Mathf.Max(profile.heroProgressionLevelApplied, targetLevel);
    }

    [Serializable]
    private struct StatIncrements
    {
        public int hp;
        public int mana;
        public int ataque;
        public int defensa;
        public int velocidadAtaque;
        public int ataqueCritico;
        public int danoCritico;
        public int destreza;
        public int suerte;
    }
}
