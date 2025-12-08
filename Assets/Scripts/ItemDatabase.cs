using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base de datos de todos los ItemData disponibles en el juego.
/// ScriptableObject que permite arrastrar todos los objetos desde el Inspector.
/// Los ItemData aquí almacenados siempre permanecen en nivel 1 (datos base inmutables).
/// </summary>
[CreateAssetMenu(fileName = "Item Database", menuName = "Inventario/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("Catálogo de Objetos")]
    [Tooltip("Arrastra aquí todos los ItemData desde Assets/Items. Estos son los datos base (nivel 1) que nunca cambian.")]
    [SerializeField] private ItemData[] items = new ItemData[0];

    // Cache para búsqueda rápida por nombre
    private Dictionary<string, ItemData> itemCache = null;
    private bool cacheDirty = true;

    /// <summary>
    /// Obtiene todos los items del catálogo.
    /// </summary>
    public ItemData[] GetAllItems()
    {
        if (items == null)
            return new ItemData[0];

        return items;
    }

    /// <summary>
    /// Obtiene un item por su nombre (busca en el nombre del ScriptableObject o en itemName).
    /// </summary>
    public ItemData GetItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            return null;

        BuildCacheIfNeeded();

        // Buscar por nombre del ScriptableObject (name)
        if (itemCache.ContainsKey(itemName))
        {
            return itemCache[itemName];
        }

        // Buscar por itemName (nombre del objeto en el juego)
        foreach (var item in items)
        {
            if (item != null && item.itemName == itemName)
            {
                return item;
            }
        }

        Debug.LogWarning($"No se encontró el item '{itemName}' en la base de datos.");
        return null;
    }

    /// <summary>
    /// Obtiene un item por su índice en el array.
    /// </summary>
    public ItemData GetItemByIndex(int index)
    {
        if (items == null || index < 0 || index >= items.Length)
            return null;

        return items[index];
    }

    /// <summary>
    /// Obtiene el número total de items en la base de datos.
    /// </summary>
    public int GetItemCount()
    {
        if (items == null)
            return 0;

        return items.Length;
    }

    /// <summary>
    /// Filtra items por tipo.
    /// </summary>
    public List<ItemData> GetItemsByType(ItemType type)
    {
        List<ItemData> filtered = new List<ItemData>();

        if (items == null)
            return filtered;

        foreach (var item in items)
        {
            if (item != null && item.itemType == type)
            {
                filtered.Add(item);
            }
        }

        return filtered;
    }

    /// <summary>
    /// Filtra items por rareza.
    /// </summary>
    public List<ItemData> GetItemsByRarity(string rarity)
    {
        List<ItemData> filtered = new List<ItemData>();

        if (items == null)
            return filtered;

        foreach (var item in items)
        {
            if (item != null && item.rareza == rarity)
            {
                filtered.Add(item);
            }
        }

        return filtered;
    }

    /// <summary>
    /// Valida que no haya items duplicados en el catálogo.
    /// </summary>
    public bool ValidateDatabase()
    {
        if (items == null || items.Length == 0)
        {
            Debug.LogWarning("La base de datos está vacía.");
            return false;
        }

        HashSet<string> seenNames = new HashSet<string>();
        HashSet<string> seenItemNames = new HashSet<string>();
        bool hasDuplicates = false;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                Debug.LogWarning($"El item en el índice {i} es nulo.");
                continue;
            }

            // Verificar duplicados por nombre del ScriptableObject
            if (seenNames.Contains(items[i].name))
            {
                Debug.LogWarning($"Item duplicado encontrado (nombre ScriptableObject): '{items[i].name}' en el índice {i}.");
                hasDuplicates = true;
            }
            else
            {
                seenNames.Add(items[i].name);
            }

            // Verificar duplicados por itemName
            if (seenItemNames.Contains(items[i].itemName))
            {
                Debug.LogWarning($"Item duplicado encontrado (itemName): '{items[i].itemName}' en el índice {i}.");
                hasDuplicates = true;
            }
            else
            {
                seenItemNames.Add(items[i].itemName);
            }
        }

        if (!hasDuplicates)
        {
            Debug.Log($"Base de datos validada correctamente. {items.Length} items únicos.");
        }

        return !hasDuplicates;
    }

    /// <summary>
    /// Construye el cache de búsqueda si es necesario.
    /// </summary>
    private void BuildCacheIfNeeded()
    {
        if (itemCache == null || cacheDirty)
        {
            itemCache = new Dictionary<string, ItemData>();

            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item != null && !itemCache.ContainsKey(item.name))
                    {
                        itemCache[item.name] = item;
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

    /// <summary>
    /// Obtiene items ordenados de peor a mejor (para cofres).
    /// Ordena por rareza y luego por stats totales.
    /// </summary>
    public List<ItemData> GetItemsSortedWorstToBest()
    {
        List<ItemData> sortedItems = new List<ItemData>();

        if (items == null)
            return sortedItems;

        // Añadir todos los items no nulos
        foreach (var item in items)
        {
            if (item != null)
            {
                sortedItems.Add(item);
            }
        }

        // Ordenar: primero por rareza (Comun < Raro < Epico < Legendario)
        // Luego por stats totales (suma de todas las stats)
        sortedItems.Sort((a, b) =>
        {
            // Comparar por rareza primero
            int rarityComparison = CompareRarity(a.rareza, b.rareza);
            if (rarityComparison != 0)
                return rarityComparison;

            // Si tienen la misma rareza, comparar por stats totales
            int statsA = GetTotalStats(a);
            int statsB = GetTotalStats(b);
            return statsA.CompareTo(statsB);
        });

        return sortedItems;
    }

    /// <summary>
    /// Compara dos rarezas y retorna -1 si a < b, 0 si igual, 1 si a > b.
    /// </summary>
    private int CompareRarity(string rarityA, string rarityB)
    {
        int valueA = GetRarityValue(rarityA);
        int valueB = GetRarityValue(rarityB);
        return valueA.CompareTo(valueB);
    }

    /// <summary>
    /// Obtiene un valor numérico para la rareza (mayor = mejor).
    /// </summary>
    private int GetRarityValue(string rarity)
    {
        if (string.IsNullOrEmpty(rarity))
            return 0;

        string rarityLower = rarity.ToLower();
        if (rarityLower.Contains("comun") || rarityLower.Contains("común"))
            return 1;
        if (rarityLower.Contains("raro") || rarityLower.Contains("rara"))
            return 2;
        if (rarityLower.Contains("epico") || rarityLower.Contains("épico") || rarityLower.Contains("epica") || rarityLower.Contains("épica"))
            return 3;
        if (rarityLower.Contains("legendario") || rarityLower.Contains("legendaria"))
            return 4;

        return 0; // Desconocido
    }

    /// <summary>
    /// Calcula la suma total de todas las stats de un item.
    /// </summary>
    private int GetTotalStats(ItemData item)
    {
        if (item == null)
            return 0;

        return item.hp + item.mana + item.ataque + item.defensa + 
               item.velocidadAtaque + item.ataqueCritico + item.danoCritico + 
               item.suerte + item.destreza;
    }
}

