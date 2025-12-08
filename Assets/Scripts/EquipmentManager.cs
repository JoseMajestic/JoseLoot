using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el equipo del jugador (slots: Arma, ArmaSecundaria, Sombrero, Pechera, Botas, Montura).
/// Según reglas: Slots: Arma, ArmaSecundaria, Sombrero, Pechera, Botas, Montura.
/// Trabaja con ItemInstance (instancias con nivel propio) en lugar de ItemData directamente.
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    [System.Serializable]
    public enum EquipmentSlotType
    {
        Arma,           // Arma principal
        ArmaSecundaria, // Arma secundaria
        Sombrero,       // Casco/Sombrero
        Pechera,        // Armadura/Pechera
        Botas,          // Botas
        Montura         // Montura
    }

    [Header("Referencias")]
    [Tooltip("ItemDatabase para deserializar items (requerido para cargar desde BD)")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Tooltip("Referencia al InventoryAutoOrganizer (opcional, para auto-organizar al desequipar)")]
    [SerializeField] private InventoryAutoOrganizer inventoryAutoOrganizer;

    [Tooltip("Referencia al InventoryManager (necesario para añadir items al inventario al desequipar)")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Botones de Acción")]
    [Tooltip("Botón para equipar el item visualizado en el visor")]
    [SerializeField] private Button equipButton;

    [Tooltip("Botón para quitar el item visualizado en el visor (si está equipado)")]
    [SerializeField] private Button unequipButton;

    [Header("Slots de Equipo")]
    [SerializeField] private ItemInstance arma;
    [SerializeField] private ItemInstance armaSecundaria;
    [SerializeField] private ItemInstance sombrero;
    [SerializeField] private ItemInstance pechera;
    [SerializeField] private ItemInstance botas;
    [SerializeField] private ItemInstance montura;

    // Eventos para notificar cambios en el equipo
    public System.Action<EquipmentSlotType, ItemInstance> OnItemEquipped;
    public System.Action<EquipmentSlotType, ItemInstance> OnItemUnequipped;
    public System.Action OnEquipmentChanged;

    private ItemInstance selectedItemForEquip; // Item seleccionado para equipar
    private EquipmentSlotType selectedSlotForEquip; // Slot seleccionado para equipar

    private void Start()
    {
        // Configurar botones
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        }

        if (unequipButton != null)
        {
            unequipButton.onClick.AddListener(OnUnequipButtonClicked);
        }
    }

    private void OnDestroy()
    {
        // Limpiar listeners de botones
        if (equipButton != null)
        {
            equipButton.onClick.RemoveListener(OnEquipButtonClicked);
        }

        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveListener(OnUnequipButtonClicked);
        }
    }

    /// <summary>
    /// Equipa un ItemInstance en el slot especificado.
    /// </summary>
    /// <returns>El ItemInstance que estaba equipado anteriormente (si había uno), o null.</returns>
    public ItemInstance EquipItem(ItemInstance itemInstance, EquipmentSlotType slot)
    {
        if (itemInstance == null || !itemInstance.IsValid())
        {
            Debug.LogWarning("Intentando equipar un ItemInstance nulo o inválido.");
            return null;
        }

        // Validar que el item sea del tipo correcto para el slot
        if (!IsValidItemForSlot(itemInstance.baseItem, slot))
        {
            Debug.LogWarning($"El item '{itemInstance.GetItemName()}' no es válido para el slot {slot}.");
            return null;
        }

        ItemInstance previousItem = GetEquippedItem(slot);

        // Equipar el nuevo item
        SetEquippedItem(slot, itemInstance);

        OnItemEquipped?.Invoke(slot, itemInstance);
        if (previousItem != null)
        {
            OnItemUnequipped?.Invoke(slot, previousItem);
        }
        OnEquipmentChanged?.Invoke();

        return previousItem;
    }

    /// <summary>
    /// Equipa un ItemData creando una nueva instancia (nivel 1).
    /// </summary>
    /// <returns>El ItemInstance que estaba equipado anteriormente (si había uno), o null.</returns>
    public ItemInstance EquipItem(ItemData itemData, EquipmentSlotType slot)
    {
        if (itemData == null)
        {
            Debug.LogWarning("Intentando equipar un ItemData nulo.");
            return null;
        }

        ItemInstance newInstance = new ItemInstance(itemData);
        return EquipItem(newInstance, slot);
    }

    /// <summary>
    /// Desequipa un item del slot especificado.
    /// </summary>
    /// <returns>El ItemInstance que se desequipó, o null si el slot estaba vacío.</returns>
    public ItemInstance UnequipItem(EquipmentSlotType slot)
    {
        ItemInstance item = GetEquippedItem(slot);
        if (item == null || !item.IsValid())
        {
            Debug.LogWarning($"No hay item equipado en el slot {slot}.");
            return null;
        }

        SetEquippedItem(slot, null);
        OnItemUnequipped?.Invoke(slot, item);
        OnEquipmentChanged?.Invoke();

        // Auto-organizar inventario si está configurado (el item se añadirá al inventario desde fuera)
        if (inventoryAutoOrganizer != null)
        {
            // Nota: La auto-organización se ejecutará cuando el item se añada al inventario
            // mediante el evento OnItemAdded del InventoryManager
        }

        return item;
    }

    /// <summary>
    /// Obtiene el ItemInstance equipado en el slot especificado.
    /// </summary>
    public ItemInstance GetEquippedItem(EquipmentSlotType slot)
    {
        switch (slot)
        {
            case EquipmentSlotType.Arma:
                return arma;
            case EquipmentSlotType.ArmaSecundaria:
                return armaSecundaria;
            case EquipmentSlotType.Sombrero:
                return sombrero;
            case EquipmentSlotType.Pechera:
                return pechera;
            case EquipmentSlotType.Botas:
                return botas;
            case EquipmentSlotType.Montura:
                return montura;
            default:
                return null;
        }
    }

    /// <summary>
    /// Establece el ItemInstance en el slot especificado (uso interno).
    /// </summary>
    private void SetEquippedItem(EquipmentSlotType slot, ItemInstance itemInstance)
    {
        switch (slot)
        {
            case EquipmentSlotType.Arma:
                arma = itemInstance;
                break;
            case EquipmentSlotType.ArmaSecundaria:
                armaSecundaria = itemInstance;
                break;
            case EquipmentSlotType.Sombrero:
                sombrero = itemInstance;
                break;
            case EquipmentSlotType.Pechera:
                pechera = itemInstance;
                break;
            case EquipmentSlotType.Botas:
                botas = itemInstance;
                break;
            case EquipmentSlotType.Montura:
                montura = itemInstance;
                break;
        }
    }

    /// <summary>
    /// Valida si un item puede ser equipado en un slot específico.
    /// Mapea los items del proyecto (Arma, Armadura, Botas, Casco, Cinturon, Escudo, Guantes)
    /// a los slots del juego (Arma, ArmaSecundaria, Sombrero, Pechera, Botas, Montura).
    /// </summary>
    private bool IsValidItemForSlot(ItemData item, EquipmentSlotType slot)
    {
        if (item == null) return false;

        string itemNameLower = item.itemName.ToLower();

        switch (slot)
        {
            case EquipmentSlotType.Arma:
                // Solo armas van en el slot principal
                return item.itemType == ItemType.Weapon || itemNameLower.Contains("arma");
            
            case EquipmentSlotType.ArmaSecundaria:
                // Escudos o armas secundarias
                return itemNameLower.Contains("escudo") || 
                       (item.itemType == ItemType.Weapon && itemNameLower.Contains("arma"));
            
            case EquipmentSlotType.Sombrero:
                // Cascos van en el slot de sombrero
                return itemNameLower.Contains("casco") || itemNameLower.Contains("sombrero");
            
            case EquipmentSlotType.Pechera:
                // Armaduras van en el slot de pechera
                return itemNameLower.Contains("armadura") || itemNameLower.Contains("pechera");
            
            case EquipmentSlotType.Botas:
                // Botas van en el slot de botas
                return itemNameLower.Contains("botas");
            
            case EquipmentSlotType.Montura:
                // Monturas o accesorios especiales
                return item.itemType == ItemType.Accessory || itemNameLower.Contains("montura");
            
            default:
                return false;
        }
    }

    /// <summary>
    /// Calcula las estadísticas totales del equipo equipado.
    /// Usa ItemInstance.GetFinalStats() para obtener stats según el nivel de cada item.
    /// </summary>
    public EquipmentStats GetTotalEquipmentStats()
    {
        EquipmentStats stats = new EquipmentStats();

        AddItemStats(arma, ref stats);
        AddItemStats(armaSecundaria, ref stats);
        AddItemStats(sombrero, ref stats);
        AddItemStats(pechera, ref stats);
        AddItemStats(botas, ref stats);
        AddItemStats(montura, ref stats);

        return stats;
    }

    /// <summary>
    /// Añade las estadísticas de un ItemInstance a las estadísticas totales.
    /// </summary>
    private void AddItemStats(ItemInstance itemInstance, ref EquipmentStats stats)
    {
        if (itemInstance == null || !itemInstance.IsValid())
            return;

        ItemStats itemStats = itemInstance.GetFinalStats();

        stats.hp += itemStats.hp;
        stats.mana += itemStats.mana;
        stats.ataque += itemStats.ataque;
        stats.defensa += itemStats.defensa;
        stats.velocidadAtaque += itemStats.velocidadAtaque;
        stats.ataqueCritico += itemStats.ataqueCritico;
        stats.danoCritico += itemStats.danoCritico;
        stats.suerte += itemStats.suerte;
        stats.destreza += itemStats.destreza;
    }

    /// <summary>
    /// Serializa el equipo para guardar en BD.
    /// Formato: array de strings, cada string es "nombreItemData|nivel" o null si el slot está vacío.
    /// </summary>
    public string[] SerializeEquipment()
    {
        return new string[]
        {
            arma != null && arma.IsValid() ? arma.Serialize() : null,
            armaSecundaria != null && armaSecundaria.IsValid() ? armaSecundaria.Serialize() : null,
            sombrero != null && sombrero.IsValid() ? sombrero.Serialize() : null,
            pechera != null && pechera.IsValid() ? pechera.Serialize() : null,
            botas != null && botas.IsValid() ? botas.Serialize() : null,
            montura != null && montura.IsValid() ? montura.Serialize() : null
        };
    }

    /// <summary>
    /// Deserializa el equipo desde BD.
    /// Requiere ItemDatabase para buscar los ItemData por nombre.
    /// </summary>
    public void DeserializeEquipment(string[] serialized)
    {
        if (serialized == null || serialized.Length != 6)
        {
            Debug.LogError("Datos de equipo inválidos para deserializar.");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase no está asignado. No se puede deserializar el equipo.");
            return;
        }

        // Deserializar cada slot
        arma = DeserializeSlot(serialized[0]);
        armaSecundaria = DeserializeSlot(serialized[1]);
        sombrero = DeserializeSlot(serialized[2]);
        pechera = DeserializeSlot(serialized[3]);
        botas = DeserializeSlot(serialized[4]);
        montura = DeserializeSlot(serialized[5]);

        OnEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// Deserializa un slot individual.
    /// </summary>
    private ItemInstance DeserializeSlot(string serialized)
    {
        if (string.IsNullOrEmpty(serialized))
        {
            return null;
        }

        ItemInstance instance = new ItemInstance();
        if (instance.Deserialize(serialized, itemDatabase))
        {
            return instance;
        }
        else
        {
            Debug.LogWarning($"No se pudo deserializar el slot: '{serialized}'");
            return null;
        }
    }

    /// <summary>
    /// Método de compatibilidad: deserializa desde formato antiguo (solo nombres de ItemData).
    /// Convierte automáticamente a ItemInstance nivel 1.
    /// </summary>
    [System.Obsolete("Usar DeserializeEquipment(string[]) en su lugar. Este método es para compatibilidad con datos antiguos.")]
    public void DeserializeEquipment(string[] serialized, ItemData[] itemDatabase)
    {
        if (serialized == null || serialized.Length != 6)
        {
            Debug.LogError("Datos de equipo inválidos para deserializar.");
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

        // Cargar equipo (convertir a ItemInstance nivel 1)
        arma = DeserializeSlotLegacy(serialized[0], itemDict);
        armaSecundaria = DeserializeSlotLegacy(serialized[1], itemDict);
        sombrero = DeserializeSlotLegacy(serialized[2], itemDict);
        pechera = DeserializeSlotLegacy(serialized[3], itemDict);
        botas = DeserializeSlotLegacy(serialized[4], itemDict);
        montura = DeserializeSlotLegacy(serialized[5], itemDict);

        OnEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// Deserializa un slot desde formato antiguo.
    /// </summary>
    private ItemInstance DeserializeSlotLegacy(string serialized, Dictionary<string, ItemData> itemDict)
    {
        if (string.IsNullOrEmpty(serialized))
        {
            return null;
        }

        if (itemDict.ContainsKey(serialized))
        {
            // Crear instancia nivel 1 desde ItemData
            return new ItemInstance(itemDict[serialized], 1);
        }
        else
        {
            Debug.LogWarning($"No se encontró el item '{serialized}' en la base de datos.");
            return null;
        }
    }

    /// <summary>
    /// Establece el item visualizado para equipar/desequipar (llamado desde InventoryUIManager cuando se visualiza un item).
    /// Determina automáticamente el slot según el tipo de item.
    /// </summary>
    public void SetSelectedItemForEquip(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
        {
            selectedItemForEquip = null;
            return;
        }

        selectedItemForEquip = itemInstance;
        
        // Determinar automáticamente el slot según el tipo de item
        selectedSlotForEquip = DetermineSlotForItem(itemInstance.baseItem);
    }

    /// <summary>
    /// Determina el slot de equipo apropiado para un item según su tipo/nombre.
    /// </summary>
    private EquipmentSlotType DetermineSlotForItem(ItemData itemData)
    {
        if (itemData == null) return EquipmentSlotType.Arma;
        
        string itemNameLower = itemData.itemName.ToLower();
        
        // Lógica para determinar el slot según el nombre/tipo del item
        if (itemNameLower.Contains("escudo"))
            return EquipmentSlotType.ArmaSecundaria;
        if (itemNameLower.Contains("casco") || itemNameLower.Contains("sombrero"))
            return EquipmentSlotType.Sombrero;
        if (itemNameLower.Contains("armadura") || itemNameLower.Contains("pechera"))
            return EquipmentSlotType.Pechera;
        if (itemNameLower.Contains("botas"))
            return EquipmentSlotType.Botas;
        if (itemData.itemType == ItemType.Accessory || itemNameLower.Contains("montura"))
            return EquipmentSlotType.Montura;
        
        // Por defecto: Arma
        return EquipmentSlotType.Arma;
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón Equipar.
    /// Solo funciona si hay un item visualizado en el visor.
    /// </summary>
    private void OnEquipButtonClicked()
    {
        if (selectedItemForEquip == null || !selectedItemForEquip.IsValid())
        {
            Debug.LogWarning("No hay item visualizado para equipar. Selecciona un item del inventario primero.");
            return;
        }

        // Equipar el item en el slot determinado automáticamente
        ItemInstance previousItem = EquipItem(selectedItemForEquip, selectedSlotForEquip);

        // Si había un item previo válido, añadirlo al inventario
        // SOLUCIÓN: Validar que previousItem sea válido antes de añadirlo (evita el warning)
        // SOLUCIÓN: Verificar que el item no esté ya en el inventario para evitar duplicación
        if (previousItem != null && previousItem.IsValid() && inventoryManager != null)
        {
            // Verificar si el item ya está en el inventario (para evitar duplicación)
            bool itemAlreadyInInventory = false;
            for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
            {
                ItemInstance invItem = inventoryManager.GetItem(i);
                if (invItem != null && invItem.IsValid() && 
                    invItem.baseItem == previousItem.baseItem &&
                    invItem.currentLevel == previousItem.currentLevel)
                {
                    // El item ya está en el inventario (mismo ItemData y nivel)
                    itemAlreadyInInventory = true;
                    break;
                }
            }
            
            // Solo añadir si no está ya en el inventario
            if (!itemAlreadyInInventory)
            {
                inventoryManager.AddItem(previousItem);
            }
            else
            {
                Debug.Log($"El item '{previousItem.GetItemName()}' ya está en el inventario. No se añadirá para evitar duplicación.");
            }
        }

        // El item equipado se mantiene en el inventario (no se remueve)
        // El panel de "equipado" se actualizará automáticamente mediante los eventos
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón Quitar.
    /// Solo funciona si hay un item visualizado en el visor y está equipado.
    /// </summary>
    private void OnUnequipButtonClicked()
    {
        if (selectedItemForEquip == null || !selectedItemForEquip.IsValid())
        {
            Debug.LogWarning("No hay item visualizado para desequipar. Selecciona un item del inventario primero.");
            return;
        }

        // Buscar en qué slot está equipado este item
        EquipmentSlotType slotToUnequip = FindSlotWithItem(selectedItemForEquip);
        
        // Verificar si realmente está equipado
        ItemInstance equippedInSlot = GetEquippedItem(slotToUnequip);
        if (equippedInSlot == null || !equippedInSlot.IsValid() ||
            equippedInSlot.baseItem != selectedItemForEquip.baseItem ||
            equippedInSlot.currentLevel != selectedItemForEquip.currentLevel)
        {
            Debug.LogWarning("El item visualizado no está equipado.");
            return;
        }

        // Desequipar el item
        ItemInstance unequippedItem = UnequipItem(slotToUnequip);

        // NO añadir al inventario porque el item YA ESTÁ en el inventario
        // El item se mantiene en el inventario cuando se equipa (no se remueve)
        // Solo se actualiza la UI para ocultar el panel de "equipado" mediante los eventos
        // Los eventos OnItemUnequipped y OnEquipmentChanged ya se dispararon en UnequipItem()
        // y el InventoryUIManager actualizará automáticamente el panel de "equipado"
    }

    /// <summary>
    /// Busca en qué slot de equipo está un item específico.
    /// </summary>
    private EquipmentSlotType FindSlotWithItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
            return EquipmentSlotType.Arma;

        // Buscar en todos los slots
        for (int i = 0; i < 6; i++)
        {
            EquipmentSlotType slotType = (EquipmentSlotType)i;
            ItemInstance equipped = GetEquippedItem(slotType);
            if (equipped != null && equipped.IsValid() &&
                equipped.baseItem == itemInstance.baseItem &&
                equipped.currentLevel == itemInstance.currentLevel)
            {
                return slotType;
            }
        }
        
        // Si no se encuentra, retornar el slot determinado por el tipo (para equipar)
        return DetermineSlotForItem(itemInstance.baseItem);
    }
}

/// <summary>
/// Estructura para almacenar las estadísticas totales del equipo.
/// </summary>
[System.Serializable]
public struct EquipmentStats
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
}
