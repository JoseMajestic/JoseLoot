using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Define una bonificación de set que se activa con un mínimo de piezas equipadas.
/// </summary>
[System.Serializable]
public class SetBonus
{
    [Tooltip("Nombre del set (e.g., 'Abismo (Infernum)')")]
    public string setName;

    [Tooltip("Número mínimo de piezas para activar esta bonificación")]
    public int minPieces;

    [Tooltip("Bonificaciones de stats cuando se activa")]
    public ItemStats bonusStats;

    [Tooltip("Descripción opcional de la bonificación")]
    public string description;
}

/// <summary>
/// Contenedor para bonificaciones de un set específico.
/// </summary>
[CreateAssetMenu(fileName = "SetBonuses", menuName = "Game/Set Bonuses", order = 1)]
public class SetBonusesData : ScriptableObject
{
    [Tooltip("Nombre del set")]
    public string setName;

    [Tooltip("Rareza mínima (inclusive) para que una pieza cuente en este set (ej: 'Legionarius')")]
    public string minRarity = "Plebeius";

    [Tooltip("Lista de bonificaciones ordenadas por minPieces")]
    public List<SetBonus> bonuses = new List<SetBonus>();

    /// <summary>
    /// Determina si el identificador proporcionado contiene todos los tokens requeridos
    /// para considerar que pertenece a este set o a uno de sus aliases definidos en los bonuses.
    /// </summary>
    public bool MatchesIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return false;

        if (ContainsAllTokens(identifier, setName))
            return true;

        foreach (var bonus in bonuses)
        {
            if (bonus != null && ContainsAllTokens(identifier, bonus.setName))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Obtiene las bonificaciones activas para un número dado de piezas equipadas.
    /// </summary>
    public List<SetBonus> GetActiveBonuses(int equippedPieces)
    {
        List<SetBonus> active = new List<SetBonus>();
        foreach (var bonus in bonuses)
        {
            if (equippedPieces >= bonus.minPieces)
            {
                active.Add(bonus);
            }
        }
        return active;
    }

    /// <summary>
    /// Obtiene las stats totales de bonificación para un número dado de piezas.
    /// </summary>
    public ItemStats GetTotalBonusStats(int equippedPieces)
    {
        ItemStats total = new ItemStats();
        var active = GetActiveBonuses(equippedPieces);
        foreach (var bonus in active)
        {
            total += bonus.bonusStats;
        }
        return total;
    }

    private static bool ContainsAllTokens(string source, string pattern)
    {
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(pattern))
            return false;

        string sourceLower = source.ToLowerInvariant();
        var tokens = pattern.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            if (!sourceLower.Contains(token.ToLowerInvariant()))
            {
                return false;
            }
        }

        return tokens.Length > 0;
    }
}
