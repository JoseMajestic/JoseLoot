using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manager global para bonificaciones de sets.
/// </summary>
public class SetBonusManager : MonoBehaviour
{
    [Tooltip("Datos de bonificaciones de todos los sets")]
    public List<SetBonusesData> allSetBonuses = new List<SetBonusesData>();

    private static SetBonusManager _instance;
    public static SetBonusManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Obtiene los datos de bonificaciones para un set específico.
    /// </summary>
    public SetBonusesData GetSetBonuses(string setName)
    {
        return allSetBonuses.Find(s => s.setName == setName);
    }

    /// <summary>
    /// Calcula bonificaciones totales para múltiples sets equipados.
    /// </summary>
    public ItemStats CalculateTotalSetBonuses(Dictionary<string, int> equippedSets)
    {
        ItemStats total = new ItemStats();
        foreach (var kvp in equippedSets)
        {
            var setData = GetSetBonuses(kvp.Key);
            if (setData != null)
            {
                total += setData.GetTotalBonusStats(kvp.Value);
            }
        }
        return total;
    }
}
