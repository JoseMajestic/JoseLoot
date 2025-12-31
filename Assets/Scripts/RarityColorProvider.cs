using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centraliza la relación entre nombres de rareza (nueva temática romana + equivalentes antiguos)
/// y los colores que deben utilizarse en toda la UI.
/// </summary>
public static class RarityColorProvider
{
    public struct RarityInfo
    {
        public readonly string DisplayName;
        public readonly Color Color;

        public RarityInfo(string displayName, Color color)
        {
            DisplayName = displayName;
            Color = color;
        }
    }

    private static readonly Dictionary<string, RarityInfo> ColorMap;

    static RarityColorProvider()
    {
        ColorMap = new Dictionary<string, RarityInfo>(System.StringComparer.OrdinalIgnoreCase);

        Register("Plebeius", new Color32(200, 200, 200, 255), "Comun", "Común");
        Register("Auxiliaris", new Color32(70, 110, 255, 255), "Magico", "Mágico");
        Register("Legionarius", new Color32(255, 255, 90, 255), "Raro", "Rara");
        Register("Veteranus", new Color32(80, 220, 80, 255), "Excelente");
        Register("Centurio", new Color32(255, 170, 40, 255), "Legendario", "Legendaria");
        Register("Tribunus", new Color32(180, 120, 255, 255), "Epico", "Épico", "Epica", "Épica");
        Register("Praetorianus", new Color32(255, 90, 90, 255), "Extremo", "Extrema");
        Register("Imperialis", new Color32(180, 40, 40, 255), "Demoniaco", "Demoníaco", "Demoniaca", "Demoníaca");
        Register("Augustus", new Color32(255, 235, 140, 255), "Celestial");
        Register("Divinus", new Color32(160, 160, 255, 180), "Etereo", "Etéreo");
    }

    private static void Register(string primaryName, Color32 color, params string[] aliases)
    {
        if (string.IsNullOrEmpty(primaryName))
            return;

        var info = new RarityInfo(primaryName, color);
        ColorMap[primaryName] = info;

        if (aliases == null)
            return;

        foreach (var alias in aliases)
        {
            if (!string.IsNullOrEmpty(alias))
            {
                ColorMap[alias] = info;
            }
        }
    }

    public static RarityInfo? GetInfo(string rarity)
    {
        if (string.IsNullOrEmpty(rarity))
            return null;

        if (ColorMap.TryGetValue(rarity.Trim(), out var info))
            return info;

        return null;
    }

    /// <summary>
    /// Obtiene el color asociado a la rareza indicada. Retorna blanco si no se encuentra.
    /// </summary>
    public static Color GetColor(string rarity)
    {
        var info = GetInfo(rarity);
        return info?.Color ?? Color.white;
    }

    /// <summary>
    /// Obtiene el nombre público asociado a la rareza. Si no se encuentra, devuelve el valor original.
    /// </summary>
    public static string GetDisplayName(string rarity)
    {
        var info = GetInfo(rarity);
        return info?.DisplayName ?? rarity?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Obtiene el color en formato Hex (RRGGBB) asociado a la rareza.
    /// </summary>
    public static string GetColorHex(string rarity)
    {
        return ColorUtility.ToHtmlStringRGB(GetColor(rarity));
    }
}
