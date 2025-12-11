using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base de datos de todos los AttackData disponibles en el juego.
/// ScriptableObject que permite arrastrar todos los ataques desde el Inspector.
/// </summary>
[CreateAssetMenu(fileName = "Attack Database", menuName = "Combate/Attack Database")]
public class AttackDatabase : ScriptableObject
{
    [Header("Catálogo de Ataques")]
    [Tooltip("Arrastra aquí todos los AttackData desde Assets/Attacks.")]
    [SerializeField] private AttackData[] attacks = new AttackData[0];

    // Cache para búsqueda rápida por nombre
    private Dictionary<string, AttackData> attackCache = null;
    private bool cacheDirty = true;

    /// <summary>
    /// Obtiene todos los ataques del catálogo.
    /// </summary>
    public AttackData[] GetAllAttacks()
    {
        if (attacks == null)
            return new AttackData[0];

        return attacks;
    }

    /// <summary>
    /// Obtiene un ataque por su nombre.
    /// </summary>
    public AttackData GetAttackByName(string attackName)
    {
        if (string.IsNullOrEmpty(attackName))
            return null;

        BuildCacheIfNeeded();

        // Buscar por nombre del ScriptableObject (name)
        if (attackCache.ContainsKey(attackName))
        {
            return attackCache[attackName];
        }

        // Buscar por attackName (nombre del ataque en el juego)
        foreach (var attack in attacks)
        {
            if (attack != null && attack.attackName == attackName)
            {
                return attack;
            }
        }

        Debug.LogWarning($"No se encontró el ataque '{attackName}' en la base de datos.");
        return null;
    }

    /// <summary>
    /// Obtiene un ataque por su índice en el array.
    /// </summary>
    public AttackData GetAttackByIndex(int index)
    {
        if (attacks == null || index < 0 || index >= attacks.Length)
            return null;

        return attacks[index];
    }

    /// <summary>
    /// Obtiene el número total de ataques en la base de datos.
    /// </summary>
    public int GetAttackCount()
    {
        if (attacks == null)
            return 0;

        return attacks.Length;
    }

    /// <summary>
    /// Construye el cache de búsqueda si es necesario.
    /// </summary>
    private void BuildCacheIfNeeded()
    {
        if (attackCache == null || cacheDirty)
        {
            attackCache = new Dictionary<string, AttackData>();

            if (attacks != null)
            {
                foreach (var attack in attacks)
                {
                    if (attack != null && !attackCache.ContainsKey(attack.name))
                    {
                        attackCache[attack.name] = attack;
                    }
                }
            }

            cacheDirty = false;
        }
    }

    /// <summary>
    /// Marca el cache como sucio cuando se modifica el array desde el Inspector.
    /// Unity llama a este método cuando se modifica el ScriptableObject.
    /// </summary>
    private void OnValidate()
    {
        cacheDirty = true;
    }
}



