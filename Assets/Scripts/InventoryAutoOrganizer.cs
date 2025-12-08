using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Reorganiza el inventario automáticamente por categorías (sets).
/// Ordena por nombre del set (según orden definido) y luego por nivel del set (I < II < III) y stats.
/// </summary>
public class InventoryAutoOrganizer : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al InventoryManager")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Configuración")]
    [Tooltip("Si es true, se reorganiza automáticamente al añadir/remover items")]
    [SerializeField] private bool autoOrganizeOnChange = true;
    
    private bool originalAutoOrganizeState; // Guarda el estado original para restaurarlo

    // Orden de sets de peor a mejor (según especificación del usuario)
    private static readonly Dictionary<string, int> SetOrder = new Dictionary<string, int>
    {
        { "Aprendiz", 1 },      // Peor
        { "Abismo", 2 },
        { "Arcano", 3 },
        { "Apocalipsis", 4 },
        { "Cazador", 5 },
        { "Conquistador", 6 },
        { "Fantasma", 7 },
        { "Heroe", 8 },
        { "Serafin", 9 },
        { "Titan", 10 }         // Mejor
    };

    private void OnEnable()
    {
        // Suscribirse a eventos del inventario si está configurado
        if (inventoryManager != null && autoOrganizeOnChange)
        {
            inventoryManager.OnItemAdded += OnItemAdded;
            inventoryManager.OnItemRemoved += OnItemRemoved;
            inventoryManager.OnInventoryChanged += OnInventoryChanged;
        }
    }

    private void OnDisable()
    {
        // Desuscribirse de eventos
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded -= OnItemAdded;
            inventoryManager.OnItemRemoved -= OnItemRemoved;
            inventoryManager.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    /// <summary>
    /// Se llama cuando se añade un item al inventario.
    /// </summary>
    private void OnItemAdded(int slotIndex, ItemInstance itemInstance)
    {
        if (autoOrganizeOnChange)
        {
            OrganizeInventory();
        }
    }

    /// <summary>
    /// Se llama cuando se remueve un item del inventario.
    /// </summary>
    private void OnItemRemoved(int slotIndex)
    {
        if (autoOrganizeOnChange)
        {
            OrganizeInventory();
        }
    }

    /// <summary>
    /// Se llama cuando el inventario cambia (si autoOrganizeOnChange está activado).
    /// </summary>
    private void OnInventoryChanged()
    {
        if (autoOrganizeOnChange)
        {
            OrganizeInventory();
        }
    }

    /// <summary>
    /// Reorganiza el inventario completo.
    /// Ordena por categorías (sets) y luego por nivel del set y stats.
    /// Usa métodos silenciosos para evitar disparar eventos múltiples veces.
    /// </summary>
    public void OrganizeInventory()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager no está asignado. No se puede organizar el inventario.");
            return;
        }

        // Desactivar reorganización automática temporalmente para evitar bucles
        bool wasAutoOrganize = autoOrganizeOnChange;
        autoOrganizeOnChange = false;

        // Obtener todos los items del inventario
        List<ItemInstance> items = inventoryManager.GetAllItems();

        if (items == null || items.Count == 0)
        {
            autoOrganizeOnChange = wasAutoOrganize;
            return;
        }

        // Ordenar items según las reglas
        items = SortItems(items);

        // Limpiar el inventario SIN disparar eventos (método interno)
        inventoryManager.ClearInventorySilently();

        // Añadir items ordenados de vuelta al inventario SIN disparar reorganización
        foreach (ItemInstance item in items)
        {
            if (item != null && item.IsValid())
            {
                inventoryManager.AddItemSilently(item);
            }
        }

        // Disparar un solo evento al final
        inventoryManager.NotifyInventoryChanged();

        // Reactivar reorganización automática
        autoOrganizeOnChange = wasAutoOrganize;

        Debug.Log($"Inventario reorganizado: {items.Count} items ordenados.");
    }

    /// <summary>
    /// Ordena una lista de items según las reglas de organización.
    /// </summary>
    private List<ItemInstance> SortItems(List<ItemInstance> items)
    {
        return items.OrderBy(item => GetSetOrderValue(item))
                    .ThenBy(item => GetSetLevelValue(item))
                    .ThenByDescending(item => GetTotalStatsValue(item))
                    .ToList();
    }

    /// <summary>
    /// Obtiene el valor de orden del set (mayor = mejor).
    /// </summary>
    private int GetSetOrderValue(ItemInstance item)
    {
        if (item == null || !item.IsValid() || item.baseItem == null)
            return 0;

        string itemName = item.baseItem.itemName;
        if (string.IsNullOrEmpty(itemName))
            return 0;

        // Buscar el nombre del set en el nombre del item
        foreach (var setPair in SetOrder)
        {
            if (itemName.Contains(setPair.Key))
            {
                return setPair.Value;
            }
        }

        // Si no se encuentra el set, retornar 0 (peor)
        return 0;
    }

    /// <summary>
    /// Obtiene el valor del nivel del set (I=1, II=2, III=3).
    /// </summary>
    private int GetSetLevelValue(ItemInstance item)
    {
        if (item == null || !item.IsValid() || item.baseItem == null)
            return 0;

        string itemName = item.baseItem.itemName;
        if (string.IsNullOrEmpty(itemName))
            return 0;

        // Buscar el nivel del set (I, II, III) en el nombre
        if (itemName.Contains(" III"))
            return 3;
        if (itemName.Contains(" II"))
            return 2;
        if (itemName.Contains(" I"))
            return 1;

        return 0;
    }

    /// <summary>
    /// Obtiene el valor total de stats del item (para desempate).
    /// </summary>
    private int GetTotalStatsValue(ItemInstance item)
    {
        if (item == null || !item.IsValid())
            return 0;

        ItemStats stats = item.GetFinalStats();
        return stats.hp + stats.mana + stats.ataque + stats.defensa + 
               stats.velocidadAtaque + stats.ataqueCritico + stats.danoCritico + 
               stats.suerte + stats.destreza;
    }

    /// <summary>
    /// Obtiene el nombre del set de un item.
    /// </summary>
    public static string GetSetName(ItemInstance item)
    {
        if (item == null || !item.IsValid() || item.baseItem == null)
            return "Desconocido";

        string itemName = item.baseItem.itemName;
        if (string.IsNullOrEmpty(itemName))
            return "Desconocido";

        // Buscar el nombre del set
        foreach (var setPair in SetOrder)
        {
            if (itemName.Contains(setPair.Key))
            {
                return setPair.Key;
            }
        }

        return "Desconocido";
    }

    /// <summary>
    /// Obtiene el orden de un set (1-10, donde 1 es peor y 10 es mejor).
    /// </summary>
    public static int GetSetOrder(string setName)
    {
        if (string.IsNullOrEmpty(setName))
            return 0;

        if (SetOrder.ContainsKey(setName))
            return SetOrder[setName];

        return 0;
    }

    /// <summary>
    /// Desactiva temporalmente la reorganización automática.
    /// Útil cuando se añaden múltiples items a la vez.
    /// </summary>
    public void DisableAutoOrganize()
    {
        originalAutoOrganizeState = autoOrganizeOnChange;
        autoOrganizeOnChange = false;
    }

    /// <summary>
    /// Reactiva la reorganización automática al estado anterior.
    /// </summary>
    public void EnableAutoOrganize()
    {
        autoOrganizeOnChange = originalAutoOrganizeState;
    }

    /// <summary>
    /// Reorganiza el inventario manualmente (ignora autoOrganizeOnChange).
    /// </summary>
    public void ForceOrganize()
    {
        OrganizeInventory();
    }
}

