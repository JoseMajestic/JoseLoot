using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el equipo del jugador (10 slots: Montura, Casco, Collar, Arma, Armadura, Escudo, Guantes, Cinturon, Anillo, Botas).
/// Trabaja con ItemInstance (instancias con nivel propio) en lugar de ItemData directamente.
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    [System.Serializable]
    public enum EquipmentSlotType
    {
        Montura,        // Montura (slot 1)
        Casco,          // Casco (slot 2)
        Collar,         // Collar (slot 3)
        Arma,           // Arma (slot 4)
        Armadura,       // Armadura (slot 5)
        Escudo,         // Escudo (slot 6)
        Guantes,        // Guantes (slot 7)
        Cinturon,       // Cinturón (slot 8)
        Anillo,         // Anillo (slot 9)
        Botas           // Botas (slot 10)
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
    [SerializeField] private ItemInstance montura;
    [SerializeField] private ItemInstance casco;
    [SerializeField] private ItemInstance collar;
    [SerializeField] private ItemInstance arma;
    [SerializeField] private ItemInstance armadura;
    [SerializeField] private ItemInstance escudo;
    [SerializeField] private ItemInstance guantes;
    [SerializeField] private ItemInstance cinturon;
    [SerializeField] private ItemInstance anillo;
    [SerializeField] private ItemInstance botas;

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

        // Guardar perfil del jugador después de equipar
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SavePlayerProfile();
        }

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

        // Guardar perfil del jugador después de desequipar
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SavePlayerProfile();
        }

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
            case EquipmentSlotType.Montura:
                return montura;
            case EquipmentSlotType.Casco:
                return casco;
            case EquipmentSlotType.Collar:
                return collar;
            case EquipmentSlotType.Arma:
                return arma;
            case EquipmentSlotType.Armadura:
                return armadura;
            case EquipmentSlotType.Escudo:
                return escudo;
            case EquipmentSlotType.Guantes:
                return guantes;
            case EquipmentSlotType.Cinturon:
                return cinturon;
            case EquipmentSlotType.Anillo:
                return anillo;
            case EquipmentSlotType.Botas:
                return botas;
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
            case EquipmentSlotType.Montura:
                montura = itemInstance;
                break;
            case EquipmentSlotType.Casco:
                casco = itemInstance;
                break;
            case EquipmentSlotType.Collar:
                collar = itemInstance;
                break;
            case EquipmentSlotType.Arma:
                arma = itemInstance;
                break;
            case EquipmentSlotType.Armadura:
                armadura = itemInstance;
                break;
            case EquipmentSlotType.Escudo:
                escudo = itemInstance;
                break;
            case EquipmentSlotType.Guantes:
                guantes = itemInstance;
                break;
            case EquipmentSlotType.Cinturon:
                cinturon = itemInstance;
                break;
            case EquipmentSlotType.Anillo:
                anillo = itemInstance;
                break;
            case EquipmentSlotType.Botas:
                botas = itemInstance;
                break;
        }
    }

    /// <summary>
    /// SOLUCIÓN: Establece el ItemInstance en el slot especificado sin disparar eventos.
    /// Útil para carga silenciosa del equipo desde el perfil guardado.
    /// </summary>
    public void SetEquippedItemDirectly(EquipmentSlotType slot, ItemInstance itemInstance)
    {
        SetEquippedItem(slot, itemInstance);
        // No disparar eventos - esto es para carga silenciosa
    }

    /// <summary>
    /// Valida si un item puede ser equipado en un slot específico.
    /// Mapea los items del proyecto (Montura, Casco, Collar, Arma, Armadura, Escudo, Guantes, Cinturon, Anillo, Botas)
    /// a los slots del juego (Montura, Casco, Collar, Arma, Armadura, Escudo, Guantes, Cinturon, Anillo, Botas).
    /// </summary>
    private bool IsValidItemForSlot(ItemData item, EquipmentSlotType slot)
    {
        if (item == null) return false;

        string itemNameLower = item.itemName.ToLower();

        switch (slot)
        {
            case EquipmentSlotType.Montura:
                return item.itemType == ItemType.Montura || itemNameLower.Contains("montura");
            
            case EquipmentSlotType.Casco:
                return item.itemType == ItemType.Casco || itemNameLower.Contains("casco") || itemNameLower.Contains("sombrero");
            
            case EquipmentSlotType.Collar:
                return item.itemType == ItemType.Collar || itemNameLower.Contains("collar");
            
            case EquipmentSlotType.Arma:
                return item.itemType == ItemType.Arma || itemNameLower.Contains("arma");
            
            case EquipmentSlotType.Armadura:
                return item.itemType == ItemType.Armadura || itemNameLower.Contains("armadura") || itemNameLower.Contains("pechera");
            
            case EquipmentSlotType.Escudo:
                return item.itemType == ItemType.Escudo || itemNameLower.Contains("escudo");
            
            case EquipmentSlotType.Guantes:
                return item.itemType == ItemType.Guantes || itemNameLower.Contains("guantes");
            
            case EquipmentSlotType.Cinturon:
                return item.itemType == ItemType.Cinturon || itemNameLower.Contains("cinturon") || itemNameLower.Contains("cinturón");
            
            case EquipmentSlotType.Anillo:
                return item.itemType == ItemType.Anillo || itemNameLower.Contains("anillo");
            
            case EquipmentSlotType.Botas:
                return item.itemType == ItemType.Botas || itemNameLower.Contains("botas");
            
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

        AddItemStats(montura, ref stats);
        AddItemStats(casco, ref stats);
        AddItemStats(collar, ref stats);
        AddItemStats(arma, ref stats);
        AddItemStats(armadura, ref stats);
        AddItemStats(escudo, ref stats);
        AddItemStats(guantes, ref stats);
        AddItemStats(cinturon, ref stats);
        AddItemStats(anillo, ref stats);
        AddItemStats(botas, ref stats);

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
            montura != null && montura.IsValid() ? montura.Serialize() : null,
            casco != null && casco.IsValid() ? casco.Serialize() : null,
            collar != null && collar.IsValid() ? collar.Serialize() : null,
            arma != null && arma.IsValid() ? arma.Serialize() : null,
            armadura != null && armadura.IsValid() ? armadura.Serialize() : null,
            escudo != null && escudo.IsValid() ? escudo.Serialize() : null,
            guantes != null && guantes.IsValid() ? guantes.Serialize() : null,
            cinturon != null && cinturon.IsValid() ? cinturon.Serialize() : null,
            anillo != null && anillo.IsValid() ? anillo.Serialize() : null,
            botas != null && botas.IsValid() ? botas.Serialize() : null
        };
    }

    /// <summary>
    /// Deserializa el equipo desde BD.
    /// Requiere ItemDatabase para buscar los ItemData por nombre.
    /// </summary>
    public void DeserializeEquipment(string[] serialized)
    {
        if (serialized == null || serialized.Length != 10)
        {
            Debug.LogError("Datos de equipo inválidos para deserializar. Se esperan 10 slots.");
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase no está asignado. No se puede deserializar el equipo.");
            return;
        }

        // Deserializar cada slot (orden: Montura, Casco, Collar, Arma, Armadura, Escudo, Guantes, Cinturon, Anillo, Botas)
        montura = DeserializeSlot(serialized[0]);
        casco = DeserializeSlot(serialized[1]);
        collar = DeserializeSlot(serialized[2]);
        arma = DeserializeSlot(serialized[3]);
        armadura = DeserializeSlot(serialized[4]);
        escudo = DeserializeSlot(serialized[5]);
        guantes = DeserializeSlot(serialized[6]);
        cinturon = DeserializeSlot(serialized[7]);
        anillo = DeserializeSlot(serialized[8]);
        botas = DeserializeSlot(serialized[9]);

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
        if (serialized == null || (serialized.Length != 6 && serialized.Length != 10))
        {
            Debug.LogError("Datos de equipo inválidos para deserializar. Se esperan 6 (formato antiguo) o 10 (formato nuevo) slots.");
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
        // Compatibilidad: si viene con 6 slots (formato antiguo), mapear a los nuevos slots
        if (serialized.Length == 6)
        {
            // Formato antiguo: Arma, ArmaSecundaria, Sombrero, Pechera, Botas, Montura
            // Mapear a nuevo formato: Montura, Casco, Collar, Arma, Armadura, Escudo, Guantes, Cinturon, Anillo, Botas
            montura = DeserializeSlotLegacy(serialized[5], itemDict); // Montura (antiguo slot 5)
            casco = DeserializeSlotLegacy(serialized[2], itemDict); // Sombrero -> Casco (antiguo slot 2)
            collar = null; // Nuevo slot, no hay datos antiguos
            arma = DeserializeSlotLegacy(serialized[0], itemDict); // Arma (antiguo slot 0)
            armadura = DeserializeSlotLegacy(serialized[3], itemDict); // Pechera -> Armadura (antiguo slot 3)
            escudo = DeserializeSlotLegacy(serialized[1], itemDict); // ArmaSecundaria -> Escudo (antiguo slot 1, asumiendo que eran escudos)
            guantes = null; // Nuevo slot, no hay datos antiguos
            cinturon = null; // Nuevo slot, no hay datos antiguos
            anillo = null; // Nuevo slot, no hay datos antiguos
            botas = DeserializeSlotLegacy(serialized[4], itemDict); // Botas (antiguo slot 4)
        }
        else if (serialized.Length == 10)
        {
            // Formato nuevo: Montura, Casco, Collar, Arma, Armadura, Escudo, Guantes, Cinturon, Anillo, Botas
            montura = DeserializeSlotLegacy(serialized[0], itemDict);
            casco = DeserializeSlotLegacy(serialized[1], itemDict);
            collar = DeserializeSlotLegacy(serialized[2], itemDict);
            arma = DeserializeSlotLegacy(serialized[3], itemDict);
            armadura = DeserializeSlotLegacy(serialized[4], itemDict);
            escudo = DeserializeSlotLegacy(serialized[5], itemDict);
            guantes = DeserializeSlotLegacy(serialized[6], itemDict);
            cinturon = DeserializeSlotLegacy(serialized[7], itemDict);
            anillo = DeserializeSlotLegacy(serialized[8], itemDict);
            botas = DeserializeSlotLegacy(serialized[9], itemDict);
        }

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
    /// Orden: Montura, Casco, Collar, Arma, Armadura, Escudo, Guantes, Cinturon, Anillo, Botas
    /// </summary>
    private EquipmentSlotType DetermineSlotForItem(ItemData itemData)
    {
        if (itemData == null) return EquipmentSlotType.Arma;
        
        // Usar el itemType primero (más preciso)
        switch (itemData.itemType)
        {
            case ItemType.Montura:
                return EquipmentSlotType.Montura;
            case ItemType.Casco:
                return EquipmentSlotType.Casco;
            case ItemType.Collar:
                return EquipmentSlotType.Collar;
            case ItemType.Arma:
                return EquipmentSlotType.Arma;
            case ItemType.Armadura:
                return EquipmentSlotType.Armadura;
            case ItemType.Escudo:
                return EquipmentSlotType.Escudo;
            case ItemType.Guantes:
                return EquipmentSlotType.Guantes;
            case ItemType.Cinturon:
                return EquipmentSlotType.Cinturon;
            case ItemType.Anillo:
                return EquipmentSlotType.Anillo;
            case ItemType.Botas:
                return EquipmentSlotType.Botas;
            case ItemType.Otros:
                // Items no equipables, no se puede determinar slot
                return EquipmentSlotType.Arma; // Por defecto, pero no debería equiparse
        }
        
        // Fallback al nombre (por compatibilidad con items antiguos)
        string itemNameLower = itemData.itemName.ToLower();
        if (itemNameLower.Contains("montura"))
            return EquipmentSlotType.Montura;
        if (itemNameLower.Contains("casco") || itemNameLower.Contains("sombrero"))
            return EquipmentSlotType.Casco;
        if (itemNameLower.Contains("collar"))
            return EquipmentSlotType.Collar;
        if (itemNameLower.Contains("arma"))
            return EquipmentSlotType.Arma;
        if (itemNameLower.Contains("armadura") || itemNameLower.Contains("pechera"))
            return EquipmentSlotType.Armadura;
        if (itemNameLower.Contains("escudo"))
            return EquipmentSlotType.Escudo;
        if (itemNameLower.Contains("guantes"))
            return EquipmentSlotType.Guantes;
        if (itemNameLower.Contains("cinturon") || itemNameLower.Contains("cinturón"))
            return EquipmentSlotType.Cinturon;
        if (itemNameLower.Contains("anillo"))
            return EquipmentSlotType.Anillo;
        if (itemNameLower.Contains("botas"))
            return EquipmentSlotType.Botas;
        
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
                if (invItem != null && invItem.IsValid() && previousItem.IsSameInstance(invItem))
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
            !equippedInSlot.IsSameInstance(selectedItemForEquip))
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
        for (int i = 0; i < 10; i++)
        {
            EquipmentSlotType slotType = (EquipmentSlotType)i;
            ItemInstance equipped = GetEquippedItem(slotType);
            if (equipped != null && equipped.IsValid() &&
                equipped.IsSameInstance(itemInstance))
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
