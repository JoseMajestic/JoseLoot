using UnityEngine;
using System.Globalization;

/// <summary>
/// Utilidad estática para formatear números grandes con K (miles) y M (millones).
/// Evita problemas de visualización cuando los números son muy grandes.
/// </summary>
public static class NumberFormatter
{
    /// <summary>
    /// Formatea un número entero usando K para miles y M para millones.
    /// Ejemplos: 1000 → "1.0K", 1000000 → "1.0M", 500 → "500"
    /// </summary>
    /// <param name="value">Valor numérico a formatear</param>
    /// <returns>String formateado con K o M si es necesario</returns>
    public static string FormatNumber(int value)
    {
        if (value >= 1000000)
        {
            // Millones: mostrar con 1 decimal
            float millions = value / 1000000f;
            return $"{millions:F1}M";
        }
        else if (value >= 1000)
        {
            // Miles: mostrar con 1 decimal
            float thousands = value / 1000f;
            return $"{thousands:F1}K";
        }
        else
        {
            // Menor a 1000: mostrar normal
            return value.ToString();
        }
    }

    /// <summary>
    /// Formatea un número flotante usando K para miles y M para millones.
    /// </summary>
    /// <param name="value">Valor numérico a formatear</param>
    /// <returns>String formateado con K o M si es necesario</returns>
    public static string FormatNumber(float value)
    {
        return FormatNumber(Mathf.RoundToInt(value));
    }

    private const float CombatStatToPercentFactor = 1f / 1000f;

    /// <summary>
    /// Convierte un valor de stat de combate (0-100000) a porcentaje legible.
    /// </summary>
    public static string FormatCombatPercent(int statValue)
    {
        float percent = statValue * CombatStatToPercentFactor;
        return string.Format(CultureInfo.InvariantCulture, "{0:0.###}%", percent);
    }

    /// <summary>
    /// Igual que FormatCombatPercent pero añade signo positivo cuando corresponde.
    /// </summary>
    public static string FormatCombatPercentWithSign(int statValue)
    {
        string formatted = FormatCombatPercent(statValue);
        if (statValue > 0 && !formatted.StartsWith("+"))
        {
            return "+" + formatted;
        }
        return formatted;
    }
}

