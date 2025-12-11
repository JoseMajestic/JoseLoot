using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base de datos de todos los EnemyData disponibles en el juego.
/// ScriptableObject que permite arrastrar todos los enemigos desde el Inspector.
/// </summary>
[CreateAssetMenu(fileName = "Enemy Database", menuName = "Combate/Enemy Database")]
public class EnemyDatabase : ScriptableObject
{
    [Header("Catálogo de Enemigos")]
    [Tooltip("Arrastra aquí todos los EnemyData desde Assets/Enemies.")]
    [SerializeField] private EnemyData[] enemies = new EnemyData[0];

    // Cache para búsqueda rápida por nombre
    private Dictionary<string, EnemyData> enemyCache = null;
    private bool cacheDirty = true;

    /// <summary>
    /// Obtiene todos los enemigos del catálogo.
    /// </summary>
    public EnemyData[] GetAllEnemies()
    {
        if (enemies == null)
            return new EnemyData[0];

        return enemies;
    }

    /// <summary>
    /// Obtiene un enemigo por su nombre.
    /// </summary>
    public EnemyData GetEnemyByName(string enemyName)
    {
        if (string.IsNullOrEmpty(enemyName))
            return null;

        BuildCacheIfNeeded();

        // Buscar por nombre del ScriptableObject (name)
        if (enemyCache.ContainsKey(enemyName))
        {
            return enemyCache[enemyName];
        }

        // Buscar por enemyName (nombre del enemigo en el juego)
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.enemyName == enemyName)
            {
                return enemy;
            }
        }

        Debug.LogWarning($"No se encontró el enemigo '{enemyName}' en la base de datos.");
        return null;
    }

    /// <summary>
    /// Obtiene un enemigo por su índice en el array.
    /// </summary>
    public EnemyData GetEnemyByIndex(int index)
    {
        if (enemies == null || index < 0 || index >= enemies.Length)
            return null;

        return enemies[index];
    }

    /// <summary>
    /// Obtiene el número total de enemigos en la base de datos.
    /// </summary>
    public int GetEnemyCount()
    {
        if (enemies == null)
            return 0;

        return enemies.Length;
    }

    /// <summary>
    /// Filtra enemigos por nivel requerido.
    /// </summary>
    public List<EnemyData> GetEnemiesByLevel(int level)
    {
        List<EnemyData> filtered = new List<EnemyData>();

        if (enemies == null)
            return filtered;

        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.requiredLevel <= level)
            {
                filtered.Add(enemy);
            }
        }

        return filtered;
    }

    /// <summary>
    /// Construye el cache de búsqueda si es necesario.
    /// </summary>
    private void BuildCacheIfNeeded()
    {
        if (enemyCache == null || cacheDirty)
        {
            enemyCache = new Dictionary<string, EnemyData>();

            if (enemies != null)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy != null && !enemyCache.ContainsKey(enemy.name))
                    {
                        enemyCache[enemy.name] = enemy;
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



