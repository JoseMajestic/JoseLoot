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

    /// <summary>
    /// Constructor por defecto (requerido para serialización).
    /// </summary>
    public ItemInstance()
    {
        baseItem = null;
        currentLevel = 1;
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
    /// </summary>
    /// <returns>True si se aumentó el nivel, false si ya está en el máximo.</returns>
    public bool LevelUp()
    {
        if (currentLevel >= 999)
        {
            Debug.LogWarning($"El objeto '{baseItem?.itemName}' ya está en el nivel máximo (999).");
            return false;
        }

        currentLevel++;
        return true;
    }

    /// <summary>
    /// Establece el nivel del objeto directamente.
    /// </summary>
    /// <param name="level">Nivel a establecer (1-999)</param>
    public void SetLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, 999);
    }

    /// <summary>
    /// Serializa la instancia para guardar en BD.
    /// Formato: "nombreItemData|nivel"
    /// </summary>
    public string Serialize()
    {
        if (baseItem == null)
        {
            return "";
        }

        return $"{baseItem.name}|{currentLevel}";
    }

    /// <summary>
    /// Deserializa la instancia desde BD.
    /// Requiere ItemDatabase para buscar el ItemData por nombre.
    /// </summary>
    /// <param name="serialized">String en formato "nombreItemData|nivel"</param>
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
        if (parts.Length != 2)
        {
            Debug.LogWarning($"Formato de serialización inválido: '{serialized}'. Formato esperado: 'nombreItemData|nivel'");
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
        return true;
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

