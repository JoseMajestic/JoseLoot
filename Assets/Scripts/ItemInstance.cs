using System;
using UnityEngine;

/// <summary>
/// Representa una instancia de un objeto con nivel propio del jugador.
/// Separa los datos base (ItemData ScriptableObject, siempre nivel 1) 
/// de los datos del jugador (nivel actual, stats calculadas).
/// </summary>
[System.Serializable]
public class ItemInstance
{
    [Header("Referencia al Item Base")]
    [Tooltip("Referencia al ScriptableObject ItemData base (nivel 1, inmutable)")]
    public ItemData baseItem;

    [Header("Datos de Instancia del Jugador")]
    [Tooltip("Nivel actual del objeto (1-999). El ItemData base siempre está en nivel 1.")]
    [Range(1, 999)]
    public int currentLevel = 1;

    [Header("Identidad única")]
    [SerializeField, Tooltip("Identificador único de la instancia (NO cambia, identifica el objeto físico)")]
    private string instanceId;

    [Header("Versión del objeto")]
    [SerializeField, Tooltip("Versión que cambia cuando el nivel cambia (para detectar actualizaciones visuales)")]
    private int version = 1;

    // SOLUCIÓN ARQUITECTÓNICA: Evento que se dispara cuando el nivel cambia
    // Permite que los slots se suscriban directamente a cambios en el ItemInstance
    public System.Action<ItemInstance, int, int> OnLevelChanged; // item, nivelAnterior, nivelNuevo

    /// <summary>
    /// Constructor por defecto (requerido para serialización).
    /// </summary>
    public ItemInstance()
    {
        baseItem = null;
        currentLevel = 1;
        GenerateInstanceIdIfNeeded();
    }

    /// <summary>
    /// Constructor que crea una instancia desde un ItemData base (nivel 1).
    /// </summary>
    public ItemInstance(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogError("No se puede crear ItemInstance con ItemData nulo.");
            baseItem = null;
            currentLevel = 1;
            return;
        }

        baseItem = itemData;
        currentLevel = 1; // Todas las instancias nuevas empiezan en nivel 1
        GenerateInstanceIdIfNeeded();
    }

    /// <summary>
    /// Constructor que crea una instancia desde un ItemData base con un nivel específico.
    /// </summary>
    public ItemInstance(ItemData itemData, int level)
    {
        if (itemData == null)
        {
            Debug.LogError("No se puede crear ItemInstance con ItemData nulo.");
            baseItem = null;
            currentLevel = 1;
            return;
        }

        baseItem = itemData;
        currentLevel = Mathf.Clamp(level, 1, 999);
        GenerateInstanceIdIfNeeded();
    }

    /// <summary>
    /// Obtiene las estadísticas finales del objeto según su nivel actual.
    /// Calcula los stats base + bonificaciones por nivel.
    /// </summary>
    public ItemStats GetFinalStats()
    {
        if (baseItem == null)
        {
            Debug.LogWarning("ItemInstance sin baseItem. Retornando stats vacías.");
            return new ItemStats();
        }

        ItemStats stats = GetStatsAtLevel(currentLevel);
        return stats;
    }

    /// <summary>
    /// Obtiene las estadísticas del objeto a un nivel específico.
    /// Los stats aumentan de forma aditiva (puntos fijos, no porcentual).
    /// </summary>
    /// <param name="level">Nivel para calcular stats (1-999)</param>
    public ItemStats GetStatsAtLevel(int level)
    {
        if (baseItem == null)
        {
            return new ItemStats();
        }

        level = Mathf.Clamp(level, 1, 999);

        // Stats base del ItemData (nivel 1)
        ItemStats stats = new ItemStats
        {
            hp = baseItem.hp,
            mana = baseItem.mana,
            ataque = baseItem.ataque,
            defensa = baseItem.defensa,
            velocidadAtaque = baseItem.velocidadAtaque,
            ataqueCritico = baseItem.ataqueCritico,
            danoCritico = baseItem.danoCritico,
            suerte = baseItem.suerte,
            destreza = baseItem.destreza
        };

        // Bonificaciones por nivel (aditivas, no porcentuales)
        // TODO(KIRBY-APPROVAL): Confirmar fórmula exacta de bonificación por nivel
        // Por ahora: cada nivel añade una cantidad fija de stats según el tipo de stat
        // Esta es una implementación placeholder hasta confirmar la fórmula exacta
        int levelsAboveBase = level - 1; // Niveles por encima del nivel 1

        if (levelsAboveBase > 0)
        {
            // Bonificación base por nivel (ajustar según reglas del juego)
            // Por ahora: +1 punto por cada stat principal por nivel
            stats.hp += levelsAboveBase;
            stats.mana += levelsAboveBase;
            stats.ataque += levelsAboveBase;
            stats.defensa += levelsAboveBase;
            stats.velocidadAtaque += levelsAboveBase / 2; // Mitad para velocidad
            stats.ataqueCritico += levelsAboveBase / 5; // Menos frecuente
            stats.danoCritico += levelsAboveBase / 5;
            stats.suerte += levelsAboveBase / 3;
            stats.destreza += levelsAboveBase / 2;
        }

        return stats;
    }

    /// <summary>
    /// Aumenta el nivel del objeto en 1 (hasta máximo 999).
    /// También incrementa la versión para que los slots detecten el cambio.
    /// </summary>
    /// <returns>True si se aumentó el nivel, false si ya está en el máximo.</returns>
    public bool LevelUp()
    {
        if (currentLevel >= 999)
        {
            Debug.LogWarning($"El objeto '{baseItem?.itemName}' ya está en el nivel máximo (999).");
            return false;
        }

        int oldLevel = currentLevel;
        currentLevel++;
        version++; // Incrementar versión cuando el nivel cambia para que los slots detecten el cambio
        
        // SOLUCIÓN ARQUITECTÓNICA: Disparar evento cuando el nivel cambia
        OnLevelChanged?.Invoke(this, oldLevel, currentLevel);
        
        return true;
    }

    /// <summary>
    /// Establece el nivel del objeto directamente.
    /// También incrementa la versión si el nivel cambió.
    /// </summary>
    /// <param name="level">Nivel a establecer (1-999)</param>
    public void SetLevel(int level)
    {
        int newLevel = Mathf.Clamp(level, 1, 999);
        if (newLevel != currentLevel)
        {
            int oldLevel = currentLevel;
            currentLevel = newLevel;
            version++; // Incrementar versión cuando el nivel cambia
            
            // SOLUCIÓN ARQUITECTÓNICA: Disparar evento cuando el nivel cambia
            OnLevelChanged?.Invoke(this, oldLevel, currentLevel);
        }
    }

    /// <summary>
    /// Serializa la instancia para guardar en BD.
    /// Formato: "nombreItemData|nivel|id|version"
    /// </summary>
    public string Serialize()
    {
        if (baseItem == null)
        {
            return "";
        }

        GenerateInstanceIdIfNeeded();
        return $"{baseItem.name}|{currentLevel}|{instanceId}|{version}";
    }

    /// <summary>
    /// Deserializa la instancia desde BD.
    /// Requiere ItemDatabase para buscar el ItemData por nombre.
    /// </summary>
    /// <param name="serialized">String en formato "nombreItemData|nivel|id"</param>
    /// <param name="itemDatabase">ItemDatabase para buscar el ItemData</param>
    /// <returns>True si se deserializó correctamente, false si falló</returns>
    public bool Deserialize(string serialized, ItemDatabase itemDatabase)
    {
        if (string.IsNullOrEmpty(serialized))
        {
            baseItem = null;
            currentLevel = 1;
            return false;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase es nulo. No se puede deserializar ItemInstance.");
            return false;
        }

        string[] parts = serialized.Split('|');
        if (parts.Length < 2)
        {
            Debug.LogWarning($"Formato de serialización inválido: '{serialized}'. Formato esperado: 'nombreItemData|nivel|id|version'");
            return false;
        }

        string itemName = parts[0];
        if (!int.TryParse(parts[1], out int level))
        {
            Debug.LogWarning($"Nivel inválido en serialización: '{parts[1]}'");
            return false;
        }

        baseItem = itemDatabase.GetItemByName(itemName);
        if (baseItem == null)
        {
            Debug.LogWarning($"No se encontró el ItemData '{itemName}' en la base de datos.");
            return false;
        }

        currentLevel = Mathf.Clamp(level, 1, 999);

        // Si hay un tercer componente, úsalo como instanceId; si no, genera uno nuevo (compatibilidad hacia atrás)
        if (parts.Length >= 3 && !string.IsNullOrEmpty(parts[2]))
        {
            instanceId = parts[2];
        }
        else
        {
            GenerateInstanceIdIfNeeded();
        }

        // Si hay un cuarto componente, úsalo como version; si no, inicializa en 1 (compatibilidad hacia atrás)
        if (parts.Length >= 4 && !string.IsNullOrEmpty(parts[3]) && int.TryParse(parts[3], out int savedVersion))
        {
            version = savedVersion;
        }
        else
        {
            version = 1; // Por defecto, versión 1 para items antiguos sin versión
        }

        return true;
    }

    /// <summary>
    /// Obtiene el identificador único de la instancia.
    /// </summary>
    public string GetInstanceId()
    {
        GenerateInstanceIdIfNeeded();
        return instanceId;
    }

    /// <summary>
    /// Compara identidad exacta de instancias (referencia o instanceId).
    /// El GUID no cambia, por lo que identifica el mismo objeto físico incluso si el nivel cambió.
    /// </summary>
    public bool IsSameInstance(ItemInstance other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other == null)
            return false;
        if (!string.IsNullOrEmpty(instanceId) && !string.IsNullOrEmpty(other.instanceId))
            return instanceId == other.instanceId;
        return false;
    }

    /// <summary>
    /// Verifica si la versión del objeto cambió (útil para detectar mejoras sin cambiar el GUID).
    /// </summary>
    public bool HasChangedVersion(ItemInstance other)
    {
        if (other == null)
            return true;
        return version != other.version;
    }

    /// <summary>
    /// Obtiene la versión actual del objeto.
    /// </summary>
    public int GetVersion()
    {
        return version;
    }

    private void GenerateInstanceIdIfNeeded()
    {
        if (string.IsNullOrEmpty(instanceId))
        {
            instanceId = Guid.NewGuid().ToString("N");
        }
    }

    /// <summary>
    /// Verifica si la instancia es válida (tiene ItemData base).
    /// </summary>
    public bool IsValid()
    {
        return baseItem != null;
    }

    /// <summary>
    /// Obtiene el nombre del objeto (del ItemData base).
    /// </summary>
    public string GetItemName()
    {
        return baseItem != null ? baseItem.itemName : "Item Inválido";
    }

    /// <summary>
    /// Obtiene el sprite del objeto (del ItemData base).
    /// </summary>
    public Sprite GetItemSprite()
    {
        return baseItem != null ? baseItem.itemSprite : null;
    }
}

/// <summary>
/// Estructura para almacenar las estadísticas de un item.
/// </summary>
[System.Serializable]
public struct ItemStats
{
    public int hp;
    public int mana;
    public int ataque;
    public int defensa;
    public int velocidadAtaque;
    public int ataqueCritico;
    public int danoCritico;
    public int suerte;
    public int destreza;

    /// <summary>
    /// Suma dos ItemStats.
    /// </summary>
    public static ItemStats operator +(ItemStats a, ItemStats b)
    {
        return new ItemStats
        {
            hp = a.hp + b.hp,
            mana = a.mana + b.mana,
            ataque = a.ataque + b.ataque,
            defensa = a.defensa + b.defensa,
            velocidadAtaque = a.velocidadAtaque + b.velocidadAtaque,
            ataqueCritico = a.ataqueCritico + b.ataqueCritico,
            danoCritico = a.danoCritico + b.danoCritico,
            suerte = a.suerte + b.suerte,
            destreza = a.destreza + b.destreza
        };
    }
}

