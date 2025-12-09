using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona el inventario del jugador (8x17 = 136 slots, scrollable).
/// Trabaja con ItemInstance (instancias con nivel propio) en lugar de ItemData directamente.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("Configuración del Inventario")]
    [Tooltip("Tamaño del inventario: 8 columnas x 17 filas = 136 slots")]
    public const int INVENTORY_COLS = 8;
    public const int INVENTORY_ROWS = 17;
    public const int INVENTORY_SIZE = 136; // 8 x 17 = 136

    [Header("Referencias")]
    [Tooltip("ItemDatabase para deserializar items (requerido para cargar desde BD)")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Items de Prueba")]
    [Tooltip("Arrastra aquí ItemData para probar añadirlos al inventario desde el Inspector")]
    [SerializeField] private ItemData[] testItems = new ItemData[0];

    [Tooltip("Si está marcado, carga automáticamente los Test Items al hacer Play")]
    [SerializeField] private bool loadTestItemsOnPlay = false;

    // Inventario: array de ItemInstance (null = slot vacío)
    private ItemInstance[] inventory = new ItemInstance[INVENTORY_SIZE];

    // Eventos para notificar cambios en el inventario
    public System.Action<int, ItemInstance> OnItemAdded;
    public System.Action<int> OnItemRemoved;
    public System.Action OnInventoryChanged;

    private void Start()
    {
        // SOLUCIÓN: Solo cargar test items si el inventario está vacío
        // Esto evita que los test items sobrescriban el inventario cargado desde PlayerPrefs
        if (loadTestItemsOnPlay)
        {
            // Verificar si el inventario está vacío antes de cargar test items
            bool inventoryIsEmpty = true;
            for (int i = 0; i < INVENTORY_SIZE; i++)
            {
                if (inventory[i] != null && inventory[i].IsValid())
                {
                    inventoryIsEmpty = false;
                    break;
                }
            }
            
            // Solo cargar test items si el inventario está vacío
            // Si hay datos guardados (cargados desde PlayerPrefs), no sobrescribirlos
            if (inventoryIsEmpty)
            {
                AddAllTestItems();
            }
            else
            {
                Debug.Log("Inventario ya contiene datos guardados. No se cargarán test items para evitar sobrescribir el progreso.");
            }
        }
    }

    /// <summary>
    /// Obtiene el item en una posición específica del inventario.
    /// </summary>
    public ItemInstance GetItem(int index)
    {
        if (index < 0 || index >= INVENTORY_SIZE)
            return null;
        return inventory[index];
    }

    /// <summary>
    /// Obtiene el item en una posición específica (columna, fila).
    /// </summary>
    public ItemInstance GetItem(int col, int row)
    {
        int index = row * INVENTORY_COLS + col;
        return GetItem(index);
    }

    /// <summary>
    /// Añade un ItemInstance al inventario en el primer slot disponible.
    /// </summary>
    /// <returns>Índice donde se añadió el item, o -1 si el inventario está lleno.</returns>
    public int AddItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
        {
            Debug.LogWarning("Intentando añadir un ItemInstance nulo o inválido al inventario.");
            return -1;
        }

        // Buscar el primer slot vacío
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            if (inventory[i] == null || !inventory[i].IsValid())
            {
                inventory[i] = itemInstance;
                OnItemAdded?.Invoke(i, itemInstance);
                OnInventoryChanged?.Invoke();
                return i;
            }
        }

        Debug.LogWarning("El inventario está lleno. No se puede añadir el item: " + itemInstance.GetItemName());
        return -1;
    }

    /// <summary>
    /// Añade un ItemData al inventario creando una nueva instancia (nivel 1).
    /// </summary>
    /// <returns>Índice donde se añadió el item, o -1 si el inventario está lleno.</returns>
    public int AddItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("Intentando añadir un ItemData nulo al inventario.");
            return -1;
        }

        ItemInstance newInstance = new ItemInstance(itemData);
        return AddItem(newInstance);
    }

    /// <summary>
    /// Añade un ItemInstance en una posición específica del inventario.
    /// </summary>
    /// <returns>True si se añadió correctamente, false si el slot está ocupado.</returns>
    public bool AddItemAt(ItemInstance itemInstance, int index)
    {
        if (itemInstance == null || !itemInstance.IsValid())
        {
            Debug.LogWarning("Intentando añadir un ItemInstance nulo o inválido al inventario.");
            return false;
        }

        if (index < 0 || index >= INVENTORY_SIZE)
        {
            Debug.LogWarning($"Índice fuera de rango: {index}. Rango válido: 0-{INVENTORY_SIZE - 1}");
            return false;
        }

        if (inventory[index] != null && inventory[index].IsValid())
        {
            Debug.LogWarning($"El slot {index} ya está ocupado por: {inventory[index].GetItemName()}");
            return false;
        }

        inventory[index] = itemInstance;
        OnItemAdded?.Invoke(index, itemInstance);
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Añade un ItemData en una posición específica del inventario creando una nueva instancia.
    /// </summary>
    /// <returns>True si se añadió correctamente, false si el slot está ocupado.</returns>
    public bool AddItemAt(ItemData itemData, int index)
    {
        if (itemData == null)
        {
            Debug.LogWarning("Intentando añadir un ItemData nulo al inventario.");
            return false;
        }

        ItemInstance newInstance = new ItemInstance(itemData);
        return AddItemAt(newInstance, index);
    }

    /// <summary>
    /// Elimina un item del inventario en la posición especificada.
    /// </summary>
    public bool RemoveItem(int index)
    {
        if (index < 0 || index >= INVENTORY_SIZE)
        {
            Debug.LogWarning($"Índice fuera de rango: {index}");
            return false;
        }

        if (inventory[index] == null || !inventory[index].IsValid())
        {
            Debug.LogWarning($"El slot {index} ya está vacío.");
            return false;
        }

        inventory[index] = null;
        OnItemRemoved?.Invoke(index);
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// SOLUCIÓN: Elimina un item del inventario sin disparar eventos.
    /// Útil para carga silenciosa del equipo.
    /// </summary>
    public bool RemoveItemSilent(int index)
    {
        if (index < 0 || index >= INVENTORY_SIZE)
        {
            Debug.LogWarning($"Índice fuera de rango: {index}");
            return false;
        }

        if (inventory[index] == null || !inventory[index].IsValid())
        {
            Debug.LogWarning($"El slot {index} ya está vacío.");
            return false;
        }

        inventory[index] = null;
        // No disparar eventos
        return true;
    }

    /// <summary>
    /// Mueve un item de una posición a otra en el inventario.
    /// </summary>
    public bool MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= INVENTORY_SIZE || toIndex < 0 || toIndex >= INVENTORY_SIZE)
        {
            Debug.LogWarning("Índices fuera de rango para mover item.");
            return false;
        }

        if (inventory[fromIndex] == null || !inventory[fromIndex].IsValid())
        {
            Debug.LogWarning($"No hay item en la posición origen {fromIndex}.");
            return false;
        }

        // Si el destino está ocupado, intercambiar
        ItemInstance temp = inventory[toIndex];
        inventory[toIndex] = inventory[fromIndex];
        inventory[fromIndex] = temp;

        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Verifica si el inventario tiene espacio disponible.
    /// </summary>
    public bool HasSpace()
    {
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            if (inventory[i] == null || !inventory[i].IsValid())
                return true;
        }
        return false;
    }

    /// <summary>
    /// Obtiene el número de slots ocupados.
    /// </summary>
    public int GetOccupiedSlots()
    {
        int count = 0;
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            if (inventory[i] != null && inventory[i].IsValid())
                count++;
        }
        return count;
    }

    /// <summary>
    /// Obtiene el número de slots libres.
    /// </summary>
    public int GetFreeSlots()
    {
        return INVENTORY_SIZE - GetOccupiedSlots();
    }

    /// <summary>
    /// Obtiene todos los items del inventario (sin nulls).
    /// </summary>
    public List<ItemInstance> GetAllItems()
    {
        List<ItemInstance> items = new List<ItemInstance>();
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            if (inventory[i] != null && inventory[i].IsValid())
            {
                items.Add(inventory[i]);
            }
        }
        return items;
    }

    /// <summary>
    /// Limpia todo el inventario.
    /// </summary>
    public void ClearInventory()
    {
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            inventory[i] = null;
        }
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Limpia el inventario sin disparar eventos.
    /// Uso interno para reorganización.
    /// </summary>
    internal void ClearInventorySilently()
    {
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            inventory[i] = null;
        }
        // NO disparar OnInventoryChanged
    }

    /// <summary>
    /// Añade un item sin disparar eventos (para reorganización interna).
    /// </summary>
    internal int AddItemSilently(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
            return -1;

        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            if (inventory[i] == null || !inventory[i].IsValid())
            {
                inventory[i] = itemInstance;
                // NO disparar OnItemAdded ni OnInventoryChanged
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Notifica que el inventario cambió (útil después de operaciones silenciosas).
    /// </summary>
    internal void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Añade un item de prueba desde el array testItems.
    /// </summary>
    /// <param name="testItemIndex">Índice del item en el array testItems</param>
    public void AddTestItem(int testItemIndex)
    {
        if (testItems == null || testItemIndex < 0 || testItemIndex >= testItems.Length)
        {
            Debug.LogWarning($"Índice de test item inválido: {testItemIndex}");
            return;
        }

        ItemData testItem = testItems[testItemIndex];
        if (testItem == null)
        {
            Debug.LogWarning($"El test item en el índice {testItemIndex} es nulo.");
            return;
        }

        int addedIndex = AddItem(testItem);
        if (addedIndex >= 0)
        {
            Debug.Log($"Test item '{testItem.itemName}' añadido al inventario en el slot {addedIndex}.");
        }
    }

    /// <summary>
    /// Añade todos los items de prueba al inventario (útil para testing).
    /// Desactiva temporalmente la reorganización automática para añadir todos los items de una vez.
    /// </summary>
    public void AddAllTestItems()
    {
        if (testItems == null || testItems.Length == 0)
        {
            Debug.LogWarning("No hay items de prueba configurados.");
            return;
        }

        // Obtener referencia al InventoryAutoOrganizer para desactivar reorganización temporalmente
        InventoryAutoOrganizer organizer = null;
        if (GameDataManager.Instance != null)
        {
            organizer = GameDataManager.Instance.InventoryAutoOrganizer;
        }
        
        if (organizer == null)
        {
            organizer = FindFirstObjectByType<InventoryAutoOrganizer>();
        }

        // Desactivar reorganización automática temporalmente
        bool wasAutoOrganizeEnabled = false;
        if (organizer != null)
        {
            organizer.DisableAutoOrganize();
            wasAutoOrganizeEnabled = true;
        }

        // Añadir todos los items sin reorganizar
        int addedCount = 0;
        for (int i = 0; i < testItems.Length; i++)
        {
            if (testItems[i] != null)
            {
                if (AddItem(testItems[i]) >= 0)
                {
                    addedCount++;
                }
            }
        }

        // Reorganizar una sola vez al final si había auto-organización activada
        if (organizer != null && wasAutoOrganizeEnabled)
        {
            organizer.ForceOrganize();
            organizer.EnableAutoOrganize();
        }

        Debug.Log($"Se añadieron {addedCount} items de prueba al inventario.");
    }

    /// <summary>
    /// Serializa el inventario para guardar en BD.
    /// Formato: array de strings, cada string es "nombreItemData|nivel" o null si el slot está vacío.
    /// </summary>
    public string[] SerializeInventory()
    {
        string[] serialized = new string[INVENTORY_SIZE];
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            if (inventory[i] != null && inventory[i].IsValid())
            {
                serialized[i] = inventory[i].Serialize();
            }
            else
            {
                serialized[i] = null;
            }
        }
        return serialized;
    }

    /// <summary>
    /// Deserializa el inventario desde BD.
    /// Requiere ItemDatabase para buscar los ItemData por nombre.
    /// </summary>
    /// <param name="serialized">Array de strings serializados</param>
    /// <param name="silent">Si es true, no dispara OnInventoryChanged (útil para carga inicial)</param>
    public void DeserializeInventory(string[] serialized, bool silent = false)
    {
        if (serialized == null || serialized.Length != INVENTORY_SIZE)
        {
            Debug.LogError("Datos de inventario inválidos para deserializar.");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase no está asignado. No se puede deserializar el inventario.");
            return;
        }

        // Cargar inventario
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            if (string.IsNullOrEmpty(serialized[i]))
            {
                inventory[i] = null;
            }
            else
            {
                ItemInstance instance = new ItemInstance();
                if (instance.Deserialize(serialized[i], itemDatabase))
                {
                    inventory[i] = instance;
                }
                else
                {
                    Debug.LogWarning($"No se pudo deserializar el item en el slot {i}: '{serialized[i]}'");
                    inventory[i] = null;
                }
            }
        }

        // SOLUCIÓN: Solo disparar evento si no es carga silenciosa
        // Esto evita que se refresque la UI antes de que el inventario esté completamente cargado
        if (!silent)
        {
            OnInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Método de compatibilidad: deserializa desde formato antiguo (solo nombres de ItemData).
    /// Convierte automáticamente a ItemInstance nivel 1.
    /// </summary>
    [System.Obsolete("Usar DeserializeInventory(string[]) en su lugar. Este método es para compatibilidad con datos antiguos.")]
    public void DeserializeInventory(string[] serialized, ItemData[] itemDatabase)
    {
        if (serialized == null || serialized.Length != INVENTORY_SIZE)
        {
            Debug.LogError("Datos de inventario inválidos para deserializar.");
            return;
        }

        // Crear diccionario para búsqueda rápida
        Dictionary<string, ItemData> itemDict = new Dictionary<string, ItemData>();
        if (itemDatabase != null)
        {
            foreach (var item in itemDatabase)
            {
                if (item != null)
                    itemDict[item.name] = item;
            }
        }

        // Cargar inventario (convertir a ItemInstance nivel 1)
        for (int i = 0; i < INVENTORY_SIZE; i++)
        {
            if (string.IsNullOrEmpty(serialized[i]))
            {
                inventory[i] = null;
            }
            else if (itemDict.ContainsKey(serialized[i]))
            {
                // Crear instancia nivel 1 desde ItemData
                inventory[i] = new ItemInstance(itemDict[serialized[i]], 1);
            }
            else
            {
                Debug.LogWarning($"No se encontró el item '{serialized[i]}' en la base de datos.");
                inventory[i] = null;
            }
        }

        OnInventoryChanged?.Invoke();
    }
}
