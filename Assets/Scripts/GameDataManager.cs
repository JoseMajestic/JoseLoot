using UnityEngine;

/// <summary>
/// Gestor central de datos del juego.
/// Singleton que nunca se desactiva y mantiene referencias a todos los managers.
/// Asegura persistencia de datos aunque los paneles UI se desactiven.
/// </summary>
public class GameDataManager : MonoBehaviour
{
    private static GameDataManager instance;

    [Header("Referencias a Managers")]
    [Tooltip("Referencia al InventoryManager")]
    [SerializeField] private InventoryManager inventoryManager;

    [Tooltip("Referencia al EquipmentManager")]
    [SerializeField] private EquipmentManager equipmentManager;

    [Tooltip("Referencia al ItemImprovementSystem")]
    [SerializeField] private ItemImprovementSystem itemImprovementSystem;

    [Tooltip("Referencia al PlayerMoney")]
    [SerializeField] private PlayerMoney playerMoney;

    [Tooltip("Referencia al ShopService")]
    [SerializeField] private ShopService shopService;

    [Tooltip("Referencia al ItemDatabase")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Tooltip("Referencia al PanelNavigationManager")]
    [SerializeField] private PanelNavigationManager panelNavigationManager;

    [Tooltip("Referencia al ChestManager")]
    [SerializeField] private ChestManager chestManager;

    [Tooltip("Referencia al InventoryAutoOrganizer")]
    [SerializeField] private InventoryAutoOrganizer inventoryAutoOrganizer;

    // Propiedades públicas para acceso desde otros scripts
    public static GameDataManager Instance 
    { 
        get 
        {
            // Si instance es null, intentar encontrarlo (útil si se accede antes de Awake)
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameDataManager>();
                if (instance == null)
                {
                    Debug.LogError("GameDataManager: No se encontró instancia en la escena. Asegúrate de que existe un GameObject con GameDataManager.");
                }
            }
            return instance;
        }
    }
    public InventoryManager InventoryManager => inventoryManager;
    public EquipmentManager EquipmentManager => equipmentManager;
    public ItemImprovementSystem ItemImprovementSystem => itemImprovementSystem;
    public PlayerMoney PlayerMoney => playerMoney;
    public ShopService ShopService => shopService;
    public ItemDatabase ItemDatabase => itemDatabase;
    public PanelNavigationManager PanelNavigationManager => panelNavigationManager;
    public ChestManager ChestManager => chestManager;
    public InventoryAutoOrganizer InventoryAutoOrganizer => inventoryAutoOrganizer;

    private void Awake()
    {
        // Implementar patrón Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Mantener el objeto entre escenas
        }
        else if (instance != this)
        {
            Debug.LogWarning("Ya existe una instancia de GameDataManager. Destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }

        // Validar referencias críticas
        ValidateReferences();
    }

    /// <summary>
    /// Valida que las referencias críticas estén asignadas.
    /// Muestra errores para referencias críticas y warnings para opcionales.
    /// </summary>
    private void ValidateReferences()
    {
        bool hasCriticalErrors = false;
        
        // Referencias CRÍTICAS (el juego no funcionará sin ellas)
        if (inventoryManager == null)
        {
            Debug.LogError("❌ CRÍTICO: GameDataManager - InventoryManager no está asignado. El inventario no funcionará. Asigna la referencia en el Inspector.");
            hasCriticalErrors = true;
        }
        
        if (playerMoney == null)
        {
            Debug.LogError("❌ CRÍTICO: GameDataManager - PlayerMoney no está asignado. El sistema de dinero no funcionará. Asigna la referencia en el Inspector.");
            hasCriticalErrors = true;
        }
        
        // Referencias IMPORTANTES (pueden causar problemas pero no bloquean todo)
        if (equipmentManager == null)
        {
            Debug.LogWarning("⚠️ GameDataManager - EquipmentManager no está asignado. El sistema de equipo puede no funcionar correctamente.");
        }
        
        if (itemDatabase == null)
        {
            Debug.LogWarning("⚠️ GameDataManager - ItemDatabase no está asignado. Puede haber problemas al cargar items desde la base de datos.");
        }
        
        // Mostrar resumen si hay errores críticos
        if (hasCriticalErrors)
        {
            Debug.LogError("🚨 GameDataManager tiene referencias críticas faltantes. El juego puede no funcionar correctamente. Revisa el Inspector y asigna las referencias faltantes.");
        }
    }

    /// <summary>
    /// Inicializa todas las referencias si no están asignadas (búsqueda automática).
    /// </summary>
    [ContextMenu("Auto-Asignar Referencias")]
    public void AutoAssignReferences()
    {
        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();
        
        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>();
        
        if (itemImprovementSystem == null)
            itemImprovementSystem = FindFirstObjectByType<ItemImprovementSystem>();
        
        if (playerMoney == null)
            playerMoney = FindFirstObjectByType<PlayerMoney>();
        
        if (shopService == null)
            shopService = FindFirstObjectByType<ShopService>();
        
        if (panelNavigationManager == null)
            panelNavigationManager = FindFirstObjectByType<PanelNavigationManager>();
        
        if (chestManager == null)
            chestManager = FindFirstObjectByType<ChestManager>();
        
        if (inventoryAutoOrganizer == null)
            inventoryAutoOrganizer = FindFirstObjectByType<InventoryAutoOrganizer>();

        Debug.Log("GameDataManager: Referencias auto-asignadas.");
    }
}

