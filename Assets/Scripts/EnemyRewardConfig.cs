using System;
using UnityEngine;

/// <summary>
/// Configuración de recompensas para un enemigo específico.
/// Define qué tiers puede dar y cuántos objetos.
/// </summary>
[Serializable]
public class EnemyRewardConfig
{
    [Header("Configuración Básica")]
    [Tooltip("Número de objetos que otorga al vencer (0 = solo monedas)")]
    [Range(0, 10)]
    public int rewardItemCount = 0;
    
    [Tooltip("Tiers permitidos para este enemigo (deja vacío para usar todos los tiers con probabilidades globales)")]
    [Range(1, 5)]
    public int[] allowedTiers = new int[0];
    
    [Tooltip("Si es true, distribuye los objetos equitativamente entre los tiers permitidos. Si es false, elige aleatoriamente")]
    public bool distributeEvenly = true;
    
    [Header("Modo Avanzado")]
    [Tooltip("Forzar objetos específicos (anula la selección aleatoria si está configurado)")]
    public ItemData[] forcedRewards = new ItemData[0];
    
    /// <summary>
    /// Valida que la configuración sea correcta.
    /// </summary>
    public bool Validate()
    {
        if (rewardItemCount < 0)
        {
            Debug.LogWarning("rewardItemCount no puede ser negativo");
            return false;
        }
        
        if (allowedTiers != null)
        {
            foreach (int tier in allowedTiers)
            {
                if (tier < 1 || tier > 5)
                {
                    Debug.LogWarning($"Tier inválido en allowedTiers: {tier}. Los tiers válidos son 1-5");
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Obtiene una descripción de la configuración para debug.
    /// </summary>
    public string GetDebugInfo()
    {
        string info = $"{rewardItemCount} objetos";
        
        if (allowedTiers != null && allowedTiers.Length > 0)
        {
            info += " de tiers [";
            for (int i = 0; i < allowedTiers.Length; i++)
            {
                if (i > 0) info += ", ";
                info += allowedTiers[i];
            }
            info += "]";
            info += distributeEvenly ? " (distribución equitativa)" : " (distribución aleatoria)";
        }
        else
        {
            info += " de todos los tiers (probabilidades globales)";
        }
        
        if (forcedRewards != null && forcedRewards.Length > 0)
        {
            info += $" + {forcedRewards.Length} objetos forzados";
        }
        
        return info;
    }
}
