using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona la UI del inventario, conectando los botones y textos con el InventoryManager.
/// Cada slot requiere un Button y un TextMeshPro (arrays paralelos por índice).
/// </summary>
public class InventoryUIManager : MonoBehaviour
{
    // Diccionario de colores para cada tipo de rareza
    private static readonly Dictionary<string, Color> RarityColors = new Dictionary<string, Color>
    {
        { "Comun", new Color(0.7f, 0.7f, 0.7f, 1f) },           // Gris
        { "Común", new Color(0.7f, 0.7f, 0.7f, 1f) },           // Gris (con tilde)
        { "Raro", new Color(0.2f, 0.6f, 1f, 1f) },              // Azul
        { "Rara", new Color(0.2f, 0.6f, 1f, 1f) },              // Azul (femenino)
        { "Epico", new Color(0.8f, 0.2f, 0.9f, 1f) },           // Púrpura
        { "Épico", new Color(0.8f, 0.2f, 0.9f, 1f) },           // Púrpura (con tilde)
        { "Epica", new Color(0.8f, 0.2f, 0.9f, 1f) },           // Púrpura (femenino)
        { "Épica", new Color(0.8f, 0.2f, 0.9f, 1f) },          // Púrpura (femenino con tilde)
        { "Legendario", new Color(1f, 0.5f, 0f, 1f) },          // Naranja/Dorado
        { "Legendaria", new Color(1f, 0.5f, 0f, 1f) },          // Naranja/Dorado (femenino)
        { "Demoniaco", new Color(0.6f, 0f, 0.2f, 1f) },         // Rojo oscuro/Sangre
        { "Demoníaco", new Color(0.6f, 0f, 0.2f, 1f) },         // Rojo oscuro/Sangre (con tilde)
        { "Demoniaca", new Color(0.6f, 0f, 0.2f, 1f) },         // Rojo oscuro/Sangre (femenino)
        { "Demoníaca", new Color(0.6f, 0f, 0.2f, 1f) },         // Rojo oscuro/Sangre (femenino con tilde)
        { "Extremo", new Color(0f, 1f, 0.8f, 1f) },              // Cian brillante
        { "Extrema", new Color(0f, 1f, 0.8f, 1f) }               // Cian brillante (femenino)
    };
    [Header("Referencias")]
    [Tooltip("Referencia al InventoryManager")]
    [SerializeField] private InventoryManager inventoryManager;
    
    [Tooltip("Referencia al EquipmentManager (necesario para detectar items equipados)")]
    [SerializeField] private EquipmentManager equipmentManager;

    [Tooltip("Referencia al ItemImprovementSystem (opcional, para detectar mejoras de items)")]
    [SerializeField] private ItemImprovementSystem improvementSystem;

    [Tooltip("Referencia al PlayerMoney (para mostrar dinero y vender items)")]
    [SerializeField] private PlayerMoney playerMoney;

    [Tooltip("Referencia al ShopService (para calcular precio de venta)")]
    [SerializeField] private ShopService shopService;

    [Header("Slots del Inventario")]
    [Tooltip("Arrastra aquí todos los botones en orden (136 slots total). Cada botón debe tener componente Image para el sprite del item.")]
    [SerializeField] private Button[] slotButtons = new Button[InventoryManager.INVENTORY_SIZE];
    
    [Tooltip("Arrastra aquí todos los textos en orden (136 slots total). Cada texto muestra el nivel del objeto en formato 'Nv. [número]'.")]
    [SerializeField] private TextMeshProUGUI[] levelTexts = new TextMeshProUGUI[InventoryManager.INVENTORY_SIZE];
    
    [Tooltip("Arrastra aquí todos los paneles en orden (136 slots total). Cada panel se muestra cuando el item de ese slot está equipado (ocultos por defecto).")]
    [SerializeField] private GameObject[] panelsEquipped = new GameObject[InventoryManager.INVENTORY_SIZE];

    [Header("Visor de Detalles del Item")]
    [Tooltip("Panel donde se muestra el sprite del item en grande cuando se selecciona un slot")]
    [SerializeField] private GameObject detailViewPanel;
    
    [Tooltip("Image dentro del panel donde se muestra el sprite del item en grande")]
    [SerializeField] private Image detailViewImage;

    [Header("Textos de Estadísticas")]
    [Tooltip("Texto que muestra el Nivel del item seleccionado")]
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Tooltip("Texto que muestra el HP del item seleccionado")]
    [SerializeField] private TextMeshProUGUI hpText;
    
    [Tooltip("Texto que muestra el Mana del item seleccionado")]
    [SerializeField] private TextMeshProUGUI manaText;
    
    [Tooltip("Texto que muestra el Ataque del item seleccionado")]
    [SerializeField] private TextMeshProUGUI ataqueText;
    
    [Tooltip("Texto que muestra la Defensa del item seleccionado")]
    [SerializeField] private TextMeshProUGUI defensaText;
    
    [Tooltip("Texto que muestra la Velocidad de Ataque del item seleccionado")]
    [SerializeField] private TextMeshProUGUI velocidadAtaqueText;
    
    [Tooltip("Texto que muestra el Ataque Crítico del item seleccionado")]
    [SerializeField] private TextMeshProUGUI ataqueCriticoText;
    
    [Tooltip("Texto que muestra el Daño Crítico del item seleccionado")]
    [SerializeField] private TextMeshProUGUI danoCriticoText;
    
    [Tooltip("Texto que muestra la Suerte del item seleccionado")]
    [SerializeField] private TextMeshProUGUI suerteText;
    
    [Tooltip("Texto que muestra la Destreza del item seleccionado")]
    [SerializeField] private TextMeshProUGUI destrezaText;
    
    [Tooltip("Texto que muestra la Rareza del item seleccionado")]
    [SerializeField] private TextMeshProUGUI rarezaText;
    
    [Tooltip("Texto que muestra el título/nombre del item seleccionado")]
    [SerializeField] private TextMeshProUGUI itemTitleText;

    [Tooltip("Texto que muestra la Descripción del item seleccionado")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Tooltip("Texto que muestra el Tipo/Clase del item seleccionado (Arma, Escudo, Armadura, etc.)")]
    [SerializeField] private TextMeshProUGUI itemTypeText;

    [Tooltip("Texto que muestra el Precio de Venta del item seleccionado")]
    [SerializeField] private TextMeshProUGUI sellPriceText;

    [Header("Botones de Acción")]
    [Tooltip("Botón para vender el item visualizado en el visor")]
    [SerializeField] private Button sellButton;

    [Header("Panel de Confirmación de Venta")]
    [Tooltip("Panel de confirmación que se muestra al intentar vender un item equipado")]
    [SerializeField] private GameObject confirmSellPanel;

    [Tooltip("Botón de Aceptar en el panel de confirmación (vende el item equipado)")]
    [SerializeField] private Button confirmSellButton;

    [Tooltip("Botón de Cancelar en el panel de confirmación (cierra el panel sin vender)")]
    [SerializeField] private Button cancelSellButton;

    [Header("UI de Dinero del Jugador")]
    [Tooltip("Texto que muestra las monedas del jugador (siempre visible, se actualiza automáticamente)")]
    [SerializeField] private TextMeshProUGUI playerMoneyText;

    [Tooltip("Formato para mostrar el dinero (ej: '{0}' o '{0} monedas'). {0} será reemplazado por el número")]
    [SerializeField] private string moneyFormat = "{0}";

    [Tooltip("Si es true, formatea el número con separadores de miles (ej: 1,000)")]
    [SerializeField] private bool formatMoneyWithThousands = true;

    private InventorySlot[] slotComponents; // Componentes InventorySlot de cada botón
    private InventorySlot currentlySelectedSlot; // Slot actualmente seleccionado
    private ItemInstance currentlyViewedItem; // Item actualmente visualizado en el visor
    private System.Action onInventoryChangedHandler; // Referencia al handler para poder desuscribirse
    
    // CORRECCIÓN: Variables para el panel de confirmación de venta de items equipados
    private ItemInstance pendingSellItem; // Item que se intenta vender (si está equipado)
    private int pendingSellSlotIndex; // Índice del slot del item que se intenta vender
    
    // Estado guardado del inventario para comparación
    private ItemInstance[] savedVisualState; // Estado VISUAL de los slots la última vez que se cerró el panel (lo que cada slot mostraba)
    private bool initialized; // Evita inicializar más de una vez cuando el GO estaba inactivo

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Start()
    {
        // Awake ya garantiza EnsureInitialized() incluso si estaba inactivo
        EnsureInitialized();
    }

    /// <summary>
    /// Se llama cuando el GameObject se activa.
    /// Refresca la UI para asegurar que se muestre correctamente aunque el panel estuviera desactivado al inicio.
    /// Compara el estado actual del inventario con el guardado y refresca si hay cambios.
    /// </summary>
    private void OnEnable()
    {
        EnsureInitialized();
        
        // SOLUCIÓN: Sincronizar niveles desde el perfil guardado sin recargar todo el inventario
        // Esto asegura que los niveles estén actualizados incluso si el panel estaba inactivo
        // cuando se mejoró un objeto, pero sin sobrescribir el inventario en memoria
        if (GameDataManager.Instance != null && inventoryManager != null)
        {
            // Sincronizar niveles desde PlayerPrefs sin recargar todo
            GameDataManager.Instance.SyncInventoryLevelsFromProfile();
        }
        
        // SOLUCIÓN ARQUITECTÓNICA: Polling para detectar y corregir discrepancias de nivel
        // Compara el nivel actual del ItemInstance en memoria con el nivel mostrado en el slot
        // Si hay diferencia, fuerza actualización
        PollAndUpdateAllSlots();
        
        // Si el inventario ya está inicializado, refrescar la UI
        // Esto es importante si el panel estaba desactivado al inicio
        if (inventoryManager != null && slotComponents != null)
        {
            // SOLUCIÓN: SIEMPRE forzar actualización completa de todos los slots al abrir el panel
            // Esto asegura que todos los niveles y sprites se actualicen correctamente
            // Los niveles ahora están sincronizados desde el perfil guardado
            StartCoroutine(RefreshAllSlotsWhenReady(forceRefresh: true));
        }
    }

    /// <summary>
    /// Se llama cuando el GameObject se desactiva.
    /// Guarda el estado VISUAL actual de los slots (lo que cada slot está mostrando) para comparación la próxima vez que se abra.
    /// </summary>
    private void OnDisable()
    {
        SaveVisualState();
    }

    private void EnsureInitialized()
    {
        if (initialized)
            return;

        // Configurar botón de vender (no depende de GameDataManager)
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellButtonClicked);
        }

        // CORRECCIÓN: Configurar botones del panel de confirmación
        if (confirmSellButton != null)
        {
            confirmSellButton.onClick.AddListener(OnConfirmSellEquippedItem);
        }

        if (cancelSellButton != null)
        {
            cancelSellButton.onClick.AddListener(OnCancelSellEquippedItem);
        }

        // Asegurar que el panel de confirmación esté oculto al inicio
        if (confirmSellPanel != null)
        {
            confirmSellPanel.SetActive(false);
        }

        // Inicializar componentes InventorySlot (no depende de GameDataManager)
        InitializeSlotComponents();

        // Suscribirse a eventos del inventario (no depende de GameDataManager)
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded += OnItemAdded;
            inventoryManager.OnItemRemoved += OnItemRemoved;
            // Guardar referencia al handler para poder desuscribirse correctamente
            onInventoryChangedHandler = () => RefreshAllSlots(false);
            inventoryManager.OnInventoryChanged += onInventoryChangedHandler;
        }

        // Suscribirse a eventos del equipo (no depende de GameDataManager)
        if (equipmentManager != null)
        {
            equipmentManager.OnItemEquipped += OnItemEquipped;
            equipmentManager.OnItemUnequipped += OnItemUnequipped;
            equipmentManager.OnEquipmentChanged += RefreshEquippedPanels;
        }

        // Suscribirse a eventos de mejora de items (para actualizar niveles cuando se mejora desde Forja)
        if (improvementSystem != null)
        {
            improvementSystem.OnItemImproved += OnItemImproved;
        }

        // Inicializar referencias de GameDataManager después de un frame
        // Esto asegura que GameDataManager esté completamente inicializado
        StartCoroutine(InitializeGameDataManagerReferences());
        
        // Refrescar todos los slots al inicio (después de un frame para asegurar orden)
        StartCoroutine(RefreshAfterFrame());

        initialized = true;
    }

    /// <summary>
    /// Guarda el estado VISUAL actual de los slots (lo que cada slot está mostrando).
    /// Esto se guarda después de refrescar, para comparar con el estado visual la próxima vez que se abra el panel.
    /// </summary>
    private void SaveVisualState()
    {
        if (slotComponents == null)
            return;

        savedVisualState = new ItemInstance[InventoryManager.INVENTORY_SIZE];
        for (int i = 0; i < InventoryManager.INVENTORY_SIZE && i < slotComponents.Length; i++)
        {
            if (slotComponents[i] != null)
            {
                // Guardar lo que el slot está mostrando visualmente (su currentItem)
                savedVisualState[i] = slotComponents[i].GetItem();
            }
            else
            {
                savedVisualState[i] = null;
            }
        }
    }

    /// <summary>
    /// Compara el estado VISUAL guardado (lo que los slots mostraban) con el estado REAL del inventario.
    /// Detecta cambios: nuevos items, items eliminados, o items mejorados (cambio de nivel).
    /// </summary>
    /// <returns>True si hay cambios que requieren refresco completo, false si no hay cambios</returns>
    private bool CompareVisualStateWithInventory()
    {
        if (inventoryManager == null || slotComponents == null)
            return true; // Si no hay inventario o slots, refrescar por seguridad

        // Si no hay estado visual guardado, siempre refrescar
        if (savedVisualState == null)
        {
            return true;
        }

        // Comparar el estado visual guardado (lo que los slots mostraban) con el estado real del inventario
        for (int i = 0; i < InventoryManager.INVENTORY_SIZE && i < slotComponents.Length; i++)
        {
            // Estado REAL del inventario (lo que debería mostrarse)
            ItemInstance realItem = inventoryManager.GetItem(i);
            
            // Estado VISUAL guardado (lo que el slot mostraba la última vez)
            ItemInstance savedVisualItem = savedVisualState[i];

            // Si el item cambió (nuevo item, item eliminado, o referencia diferente)
            if (realItem != savedVisualItem)
            {
                return true; // Hay cambios, necesita refresco
            }

            // Si ambos items existen, comparar por identidad y nivel
            if (realItem != null && realItem.IsValid() && savedVisualItem != null && savedVisualItem.IsValid())
            {
                // Si no son la misma instancia, hay cambios
                if (!realItem.IsSameInstance(savedVisualItem))
                {
                    return true;
                }

                // Si el nivel cambió (item mejorado), hay cambios
                if (realItem.currentLevel != savedVisualItem.currentLevel)
                {
                    return true;
                }
            }
        }

        // No hay cambios
        return false;
    }

    /// <summary>
    /// Refresca todos los slots cuando los componentes están listos.
    /// SOLUCIÓN 4: Asignación diferida - Verifica que el Canvas y todos los Image estén completamente listos antes de asignar sprites.
    /// </summary>
    /// <param name="forceRefresh">Si es true, fuerza actualización completa incluso si no hay cambios detectados</param>
    private IEnumerator RefreshAllSlotsWhenReady(bool forceRefresh = true)
    {
        if (inventoryManager == null || slotComponents == null)
            yield break;

        // SOLUCIÓN 4: Esperar a que el Canvas esté completamente activo y listo
        Canvas canvas = GetComponentInParent<Canvas>();
        while (canvas == null || !canvas.isActiveAndEnabled)
        {
            yield return null;
            canvas = GetComponentInParent<Canvas>();
        }
        
        // SOLUCIÓN 4: Esperar a que todos los componentes Image estén activos y listos
        bool allImagesReady = false;
        int maxWaitFrames = 10; // Límite de seguridad para evitar bucles infinitos
        int waitFrames = 0;
        
        while (!allImagesReady && waitFrames < maxWaitFrames)
        {
            allImagesReady = true;
            
            // Verificar que todos los slots tengan sus Image components listos
            for (int i = 0; i < slotComponents.Length && i < InventoryManager.INVENTORY_SIZE; i++)
            {
                if (slotComponents[i] != null)
                {
                    // Obtener el Image del slot
                    Button slotButton = slotComponents[i].GetComponent<Button>();
                    if (slotButton != null)
                    {
                        Image slotImage = slotButton.GetComponent<Image>();
                        if (slotImage == null)
                        {
                            slotImage = slotButton.GetComponentInChildren<Image>();
                        }
                        
                        // Verificar que el Image esté activo y listo
                        if (slotImage == null || !slotImage.gameObject.activeInHierarchy || !slotImage.enabled)
                        {
                            allImagesReady = false;
                            break;
                        }
                    }
                    else
                    {
                        allImagesReady = false;
                        break;
                    }
                }
            }
            
            if (!allImagesReady)
            {
                yield return null;
                waitFrames++;
            }
        }
        
        // Esperar frames adicionales para asegurar que Unity procesó completamente la activación
        // Aumentar el tiempo de espera para dar más tiempo a Unity
        yield return null;
        yield return null;
        yield return null; // Frame adicional
        yield return new WaitForEndOfFrame(); // Esperar hasta el final del frame
        
        // SOLUCIÓN ESTRUCTURAL: Forzar actualización completa de todos los slots si hay cambios o si se fuerza
        // Esto lee directamente desde InventoryManager y fuerza la actualización de sprites y textos
        RefreshAllSlots(forceRefreshAll: forceRefresh);
        
        // SOLUCIÓN: Forzar actualización de todos los TextMeshProUGUI después de refrescar
        // Esto asegura que los textos se rendericen correctamente incluso si el panel estaba inactivo
        ForceUpdateAllLevelTexts();
        
        // Guardar el nuevo estado VISUAL después de refrescar (lo que los slots están mostrando ahora)
        SaveVisualState();
        
        // Esperar otro frame para que Unity procese los cambios de sprites
        yield return null;
        
        // SOLUCIÓN SENCILLA: Refresco final agresivo de todos los textos después de que todo esté listo
        // Esto lee directamente desde el inventario y actualiza todos los textos sin pasar por SetItem()
        // Se ejecuta al final para asegurar que los niveles ya están sincronizados y los componentes están activos
        yield return null; // Frame adicional para asegurar que todo se procesó
        yield return new WaitForEndOfFrame(); // Esperar hasta el final del frame
        
        ForceRefreshAllLevelTextsDirectly();
        
        // Forzar actualización del canvas para asegurar que los sprites se rendericen
        UnityEngine.Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// SOLUCIÓN ARQUITECTÓNICA: Polling para detectar y corregir discrepancias de nivel.
    /// Compara el nivel actual del ItemInstance en memoria con el nivel mostrado en el slot.
    /// Si hay diferencia, fuerza actualización. Se llama en OnEnable() después de sincronizar niveles.
    /// </summary>
    private void PollAndUpdateAllSlots()
    {
        if (slotComponents == null || inventoryManager == null)
            return;

        for (int i = 0; i < slotComponents.Length && i < InventoryManager.INVENTORY_SIZE; i++)
        {
            if (slotComponents[i] != null)
            {
                ItemInstance slotItem = slotComponents[i].GetItem();
                ItemInstance inventoryItem = inventoryManager.GetItem(i);
                
                // Si hay item en inventario y en slot, comparar niveles
                if (slotItem != null && slotItem.IsValid() && 
                    inventoryItem != null && inventoryItem.IsValid() &&
                    slotItem.IsSameInstance(inventoryItem))
                {
                    // Si el nivel en memoria es diferente al mostrado, forzar actualización
                    // Esto puede pasar si el panel estaba inactivo cuando se mejoró el item
                    if (slotItem.currentLevel != inventoryItem.currentLevel)
                    {
                        // El nivel cambió, forzar actualización
                        slotComponents[i].SetItem(inventoryItem, forceUpdate: true);
                    }
                }
                // Si hay item en inventario pero el slot está vacío (o viceversa), actualizar
                else if ((inventoryItem != null && inventoryItem.IsValid()) != 
                         (slotItem != null && slotItem.IsValid()))
                {
                    // Hay discrepancia, forzar actualización
                    slotComponents[i].SetItem(inventoryItem, forceUpdate: true);
                }
            }
        }
    }

    /// <summary>
    /// SOLUCIÓN: Fuerza la actualización de todos los slots directamente, sin simular clicks.
    /// Lee el ItemInstance desde el inventario y actualiza los visuales (sprites y textos) directamente.
    /// Se ejecuta al final de RefreshAllSlotsWhenReady() para asegurar que todo esté listo.
    /// Esta solución evita depender del EventSystem que requiere clicks reales del mouse.
    /// </summary>
    private void ForceRefreshAllLevelTextsDirectly()
    {
        if (slotComponents == null || inventoryManager == null)
            return;

        // Iterar sobre todos los slots y actualizar los visuales directamente
        for (int i = 0; i < slotComponents.Length && i < InventoryManager.INVENTORY_SIZE; i++)
        {
            if (slotComponents[i] != null)
            {
                // Asegurar que el slot esté activo
                if (!slotComponents[i].gameObject.activeSelf)
                {
                    slotComponents[i].gameObject.SetActive(true);
                }

                // Leer el ItemInstance actualizado directamente desde el inventario
                ItemInstance item = inventoryManager.GetItem(i);
                
                // SOLUCIÓN: FORZAR actualización completa del slot (incluyendo currentItem y visuales)
                // Usar forceUpdate: true para asegurar que se actualice incluso si el item es el mismo
                // Esto garantiza que currentItem esté actualizado antes de llamar a ForceUpdateVisualsDirectly()
                if (item != null && item.IsValid())
                {
                    slotComponents[i].SetItem(item, forceUpdate: true); // FORZAR actualización completa
                }
                else
                {
                    // Slot vacío: limpiar también con forceUpdate
                    slotComponents[i].SetItem(null, forceUpdate: true);
                }
                
                // SOLUCIÓN: Forzar actualización directa de los visuales (sprites y textos)
                // Esto evita depender del EventSystem y del estado del componente que requiere un click real
                // ForceUpdateVisualsDirectly() llama internamente a UpdateVisuals(forceRefresh: true)
                // que actualiza tanto sprites como textos de forma agresiva
                slotComponents[i].ForceUpdateVisualsDirectly();
            }
        }
    }

    /// <summary>
    /// Refresca la UI cuando el panel se activa.
    /// Fuerza una actualización completa de todos los slots sin optimizaciones.
    /// SOLUCIÓN: Ya no se usa porque RefreshAllSlotsWhenReady() maneja todo.
    /// </summary>
    private IEnumerator RefreshUIOnEnable()
    {
        // Este método ya no es necesario porque RefreshAllSlotsWhenReady() hace todo el trabajo
        // Pero lo mantenemos por compatibilidad y como respaldo
        yield return null;
        
        if (inventoryManager == null || slotComponents == null)
            yield break;
        
        // Refrescar paneles de equipado
        RefreshEquippedPanels();
        
        // Limpiar el visor si no hay item seleccionado
        if (currentlySelectedSlot == null || currentlyViewedItem == null || !currentlyViewedItem.IsValid())
        {
            ClearDetailView();
        }
    }

    /// <summary>
    /// Inicializa las referencias de GameDataManager después de esperar un frame.
    /// Esto asegura que GameDataManager.Awake() se haya ejecutado antes.
    /// </summary>
    private IEnumerator InitializeGameDataManagerReferences()
    {
        // Esperar un frame para asegurar que GameDataManager esté inicializado
        yield return null;

        // Obtener referencias desde GameDataManager (única fuente de verdad)
        if (GameDataManager.Instance != null)
        {
            // Obtener PlayerMoney si no está asignado manualmente
            if (playerMoney == null)
            {
                playerMoney = GameDataManager.Instance.PlayerMoney;
                if (playerMoney == null)
                {
                    Debug.LogError("InventoryUIManager: PlayerMoney no está asignado en GameDataManager. Asigna la referencia en el Inspector de GameDataManager.");
                }
            }

            // Obtener ShopService si no está asignado manualmente
            if (shopService == null)
            {
                shopService = GameDataManager.Instance.ShopService;
                if (shopService == null)
                {
                    Debug.LogWarning("InventoryUIManager: ShopService no está asignado en GameDataManager. La venta de items puede no funcionar correctamente.");
                }
            }
        }
        else
        {
            Debug.LogError("InventoryUIManager: GameDataManager.Instance es null después de un frame. Asegúrate de que existe un GameObject con GameDataManager en la escena.");
        }

        // Suscribirse a eventos de dinero del jugador (después de obtener la referencia)
        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged += UpdatePlayerMoneyDisplay;
            // Actualizar display inicial
            UpdatePlayerMoneyDisplay(playerMoney.GetMoney());
        }
    }

    private IEnumerator RefreshAfterFrame()
    {
        yield return null; // Esperar un frame para que InventoryManager cargue items
        RefreshAllSlots();
        
        // Ocultar todos los paneles de equipado al inicio
        for (int i = 0; i < panelsEquipped.Length && i < InventoryManager.INVENTORY_SIZE; i++)
        {
            if (panelsEquipped[i] != null)
            {
                panelsEquipped[i].SetActive(false);
            }
        }
        
        // Refrescar paneles de equipado después de un frame adicional
        yield return null;
        RefreshEquippedPanels();
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos de dinero
        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged -= UpdatePlayerMoneyDisplay;
        }

        // Desuscribirse de eventos del botón de vender
        if (sellButton != null)
        {
            sellButton.onClick.RemoveListener(OnSellButtonClicked);
        }

        // CORRECCIÓN: Desuscribirse de eventos de los botones del panel de confirmación
        if (confirmSellButton != null)
        {
            confirmSellButton.onClick.RemoveListener(OnConfirmSellEquippedItem);
        }

        if (cancelSellButton != null)
        {
            cancelSellButton.onClick.RemoveListener(OnCancelSellEquippedItem);
        }

        // Desuscribirse de eventos del inventario
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded -= OnItemAdded;
            inventoryManager.OnItemRemoved -= OnItemRemoved;
            if (onInventoryChangedHandler != null)
            {
                inventoryManager.OnInventoryChanged -= onInventoryChangedHandler;
            }
        }

        // Desuscribirse de eventos del equipo
        if (equipmentManager != null)
        {
            equipmentManager.OnItemEquipped -= OnItemEquipped;
            equipmentManager.OnItemUnequipped -= OnItemUnequipped;
            equipmentManager.OnEquipmentChanged -= RefreshEquippedPanels;
        }

        // Desuscribirse de eventos de mejora de items
        if (improvementSystem != null)
        {
            improvementSystem.OnItemImproved -= OnItemImproved;
        }

        // Desuscribirse de eventos de los slots
        if (slotComponents != null)
        {
            foreach (InventorySlot slot in slotComponents)
            {
                if (slot != null)
                {
                    slot.OnSlotClicked -= OnSlotClicked;
                }
            }
        }
    }

    /// <summary>
    /// Inicializa los componentes InventorySlot de cada botón.
    /// </summary>
    private void InitializeSlotComponents()
    {
        slotComponents = new InventorySlot[InventoryManager.INVENTORY_SIZE];

        for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
        {
            if (i < slotButtons.Length && slotButtons[i] != null)
            {
                // Obtener o añadir componente InventorySlot al botón
                InventorySlot slot = slotButtons[i].GetComponent<InventorySlot>();
                if (slot == null)
                {
                    slot = slotButtons[i].gameObject.AddComponent<InventorySlot>();
                }

                // Asignar el texto del nivel si está configurado (mismo índice en el array paralelo)
                if (i < levelTexts.Length && levelTexts[i] != null)
                {
                    slot.SetLevelText(levelTexts[i]);
                }

                // Suscribirse al evento de clic del slot
                slot.OnSlotClicked += OnSlotClicked;

                slot.Initialize(i, inventoryManager);
                slotComponents[i] = slot;
            }
        }
    }

    /// <summary>
    /// Se llama cuando se añade un item al inventario.
    /// </summary>
    private void OnItemAdded(int slotIndex, ItemInstance itemInstance)
    {
        if (slotIndex >= 0 && slotIndex < slotComponents.Length && slotComponents[slotIndex] != null)
        {
            // Forzar actualización cuando se añade un item nuevo (especialmente desde cofres)
            slotComponents[slotIndex].SetItem(itemInstance, forceUpdate: true);
        }
    }

    /// <summary>
    /// Se llama cuando se remueve un item del inventario.
    /// </summary>
    private void OnItemRemoved(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotComponents.Length && slotComponents[slotIndex] != null)
        {
            slotComponents[slotIndex].SetItem(null);
        }
    }

    /// <summary>
    /// Refresca todos los slots del inventario.
    /// Se llama cuando cambia el inventario (reorganización, carga, etc.).
    /// Preserva el visor de detalles si hay un item visualizado.
    /// Evita actualizaciones innecesarias comparando items antes de actualizar.
    /// </summary>
    /// <param name="forceRefreshAll">Si es true, fuerza actualización completa de todos los slots sin comparaciones (útil al reactivar panel)</param>
    private void RefreshAllSlots(bool forceRefreshAll = false)
    {
        if (inventoryManager == null)
            return;

        // Guardar el item visualizado antes de refrescar
        ItemInstance savedViewedItem = currentlyViewedItem;
        InventorySlot savedSelectedSlot = currentlySelectedSlot;

        // Si forceRefreshAll es true, actualizar TODOS los slots sin comparaciones
        // Esto es necesario cuando se reactiva el panel para asegurar que todos los sprites se carguen correctamente
        if (forceRefreshAll)
        {
            // Forzar actualización completa de todos los slots desde el inventario
            for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
            {
                if (i < slotComponents.Length && slotComponents[i] != null)
                {
                    ItemInstance item = inventoryManager.GetItem(i);
                    // SIEMPRE forzar actualización cuando se reactiva el panel
                    slotComponents[i].SetItem(item, forceUpdate: true);
                }
            }

            // Restaurar el visor si había un item visualizado
            if (savedSelectedSlot != null && savedViewedItem != null && savedViewedItem.IsValid())
            {
                // Buscar el slot que ahora contiene el item visualizado (puede haber cambiado de posición)
                bool found = false;
                for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
                {
                    if (i < slotComponents.Length && slotComponents[i] != null)
                    {
                        ItemInstance currentItem = slotComponents[i].GetItem();
                        if (currentItem != null && currentItem.IsValid() &&
                            savedViewedItem != null && savedViewedItem.IsValid() &&
                            savedViewedItem.IsSameInstance(currentItem))
                        {
                            // Encontrado el slot con el mismo item, restaurar el visor
                            UpdateDetailView(slotComponents[i]);
                            found = true;
                            break;
                        }
                    }
                }
                
                // Si no se encontró el item, limpiar el visor
                if (!found)
                {
                    ClearDetailView();
                }
            }
            else
            {
                // Si no había item visualizado, asegurar que el visor esté limpio
                ClearDetailView();
            }

            // Refrescar paneles de equipado
            RefreshEquippedPanels();
            return;
        }

        // Lógica normal con comparaciones (optimizada para eventos normales)
        // Guardar estados de los items antes de refrescar (para evitar actualizaciones innecesarias)
        ItemInstance[] savedItems = new ItemInstance[InventoryManager.INVENTORY_SIZE];
        for (int i = 0; i < InventoryManager.INVENTORY_SIZE && i < slotComponents.Length; i++)
        {
            if (slotComponents[i] != null)
            {
                savedItems[i] = slotComponents[i].GetItem();
            }
        }

        // Actualizar solo los slots que realmente cambiaron
        for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
        {
            if (i < slotComponents.Length && slotComponents[i] != null)
            {
                // SOLUCIÓN: Leer siempre el item actualizado directamente desde InventoryManager
                // Esto asegura que si un item fue mejorado, leemos el nivel actualizado
                ItemInstance item = inventoryManager.GetItem(i);
                
                // Solo actualizar si el item realmente cambió (comparación por referencia)
                ItemInstance previousItem = savedItems[i];
                
                // SOLUCIÓN: Mejorar la comparación para detectar discrepancias
                // Si hay un item en el inventario pero el slot visual está vacío (o viceversa), actualizar
                bool needsUpdate = false;
                
                if (item != previousItem) // Comparación por referencia (más rápido y preciso)
                {
                    needsUpdate = true;
                }
                else if (item != null && item.IsValid() && (previousItem == null || !previousItem.IsValid()))
                {
                    // Hay un item válido en el inventario pero el slot visual está vacío
                    needsUpdate = true;
                }
                else if ((item == null || !item.IsValid()) && previousItem != null && previousItem.IsValid())
                {
                    // El slot visual tiene un item pero el inventario está vacío
                    needsUpdate = true;
                }
                // SOLUCIÓN ESTRUCTURAL: Si el item es el mismo pero el nivel cambió (item mejorado), forzar actualización
                else if (item != null && item.IsValid() && previousItem != null && previousItem.IsValid())
                {
                    if (item.IsSameInstance(previousItem) && item.currentLevel != previousItem.currentLevel)
                    {
                        needsUpdate = true; // El item fue mejorado, necesita actualizar el nivel
                    }
                }
                
                if (needsUpdate)
                {
                    // SOLUCIÓN ESTRUCTURAL: Usar forceUpdate para recargar sprites y actualizar niveles
                    slotComponents[i].SetItem(item, forceUpdate: true);
                }
            }
        }

        // Restaurar el visor si había un item visualizado
        if (savedSelectedSlot != null && savedViewedItem != null && savedViewedItem.IsValid())
        {
            // Buscar el slot que ahora contiene el item visualizado (puede haber cambiado de posición)
            bool found = false;
            for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
            {
                if (i < slotComponents.Length && slotComponents[i] != null)
                {
                    ItemInstance currentItem = slotComponents[i].GetItem();
                    if (currentItem != null && currentItem.IsValid() &&
                        savedViewedItem != null && savedViewedItem.IsValid() &&
                        savedViewedItem.IsSameInstance(currentItem))
                    {
                        // Encontrado el slot con el mismo item, restaurar el visor
                        UpdateDetailView(slotComponents[i]);
                        found = true;
                        break;
                    }
                }
            }
            
            // Si no se encontró el item, limpiar el visor
            if (!found)
            {
                ClearDetailView();
            }
        }
        else
        {
            // Si no había item visualizado, asegurar que el visor esté limpio
            ClearDetailView();
        }
    }

    /// <summary>
    /// Se llama cuando se hace clic en un slot del inventario.
    /// Actualiza el visor de detalles con la información del item seleccionado.
    /// </summary>
    private void OnSlotClicked(InventorySlot slot)
    {
        // Deseleccionar el slot anterior
        if (currentlySelectedSlot != null && currentlySelectedSlot != slot)
        {
            currentlySelectedSlot.SetSelected(false);
        }

        // Seleccionar el nuevo slot
        currentlySelectedSlot = slot;
        if (slot != null)
        {
            slot.SetSelected(true);
        }

        // Actualizar el visor de detalles
        UpdateDetailView(slot);
    }

    /// <summary>
    /// Actualiza el visor de detalles con la información del item seleccionado.
    /// </summary>
    private void UpdateDetailView(InventorySlot slot)
    {
        ItemInstance item = slot != null ? slot.GetItem() : null;
        currentlyViewedItem = item; // Guardar el item visualizado

        if (item == null || !item.IsValid())
        {
            // No hay item seleccionado, limpiar el visor
            ClearDetailView();
            
            // Limpiar selección en EquipmentManager
            if (equipmentManager != null)
            {
                equipmentManager.SetSelectedItemForEquip(null);
            }
            return;
        }

        // Mostrar el sprite del item en el panel grande
        if (detailViewImage != null)
        {
            Sprite itemSprite = item.GetItemSprite();
            if (itemSprite != null)
            {
                detailViewImage.sprite = itemSprite;
                detailViewImage.enabled = true;
            }
            else
            {
                detailViewImage.sprite = null;
                detailViewImage.enabled = false;
            }
        }

        // Mostrar el panel si está configurado
        if (detailViewPanel != null)
        {
            detailViewPanel.SetActive(true);
        }

        // SOLUCIÓN CRÍTICA: Leer el item actualizado directamente desde el inventario
        // Esto asegura que tenemos la referencia más reciente con el nivel y stats actualizados
        // El problema es que cuando se mejora un objeto, el ItemInstance se actualiza pero el slot
        // puede tener una referencia antigua, por lo que necesitamos leer desde el inventario
        ItemInstance updatedItem = item;
        if (currentlySelectedSlot != null && inventoryManager != null)
        {
            int slotIndex = currentlySelectedSlot.GetSlotIndex();
            ItemInstance inventoryItem = inventoryManager.GetItem(slotIndex);
            if (inventoryItem != null && inventoryItem.IsValid() && inventoryItem.IsSameInstance(item))
            {
                // Usar el item del inventario que tiene el nivel más reciente
                updatedItem = inventoryItem;
            }
        }
        
        // Obtener las stats finales del item actualizado
        ItemStats stats = updatedItem.GetFinalStats();
        ItemData baseItem = updatedItem.baseItem;

        // Título del item (nombre con color de rareza)
        if (itemTitleText != null)
        {
            itemTitleText.text = baseItem != null ? baseItem.itemName : item.GetItemName();
            if (baseItem != null)
            {
                Color rarityColor = GetRarityColor(baseItem.rareza);
                itemTitleText.color = rarityColor;
            }
        }

        // Actualizar todos los textos de estadísticas usando el item actualizado
        if (levelText != null)
            levelText.text = $"Nv. {updatedItem.currentLevel}";

        if (hpText != null)
            hpText.text = stats.hp.ToString();

        if (manaText != null)
            manaText.text = stats.mana.ToString();

        if (ataqueText != null)
            ataqueText.text = stats.ataque.ToString();

        if (defensaText != null)
            defensaText.text = stats.defensa.ToString();

        if (velocidadAtaqueText != null)
            velocidadAtaqueText.text = stats.velocidadAtaque.ToString();

        if (ataqueCriticoText != null)
            ataqueCriticoText.text = stats.ataqueCritico.ToString();

        if (danoCriticoText != null)
            danoCriticoText.text = stats.danoCritico.ToString();

        if (suerteText != null)
            suerteText.text = stats.suerte.ToString();

        if (destrezaText != null)
            destrezaText.text = stats.destreza.ToString();

        if (rarezaText != null && baseItem != null)
        {
            rarezaText.text = baseItem.rareza;
            // Aplicar color según la rareza
            Color rarityColor = GetRarityColor(baseItem.rareza);
            rarezaText.color = rarityColor;
        }

        if (descriptionText != null && baseItem != null)
        {
            descriptionText.text = baseItem.description;
        }

        // Mostrar tipo/clase del item
        if (itemTypeText != null && baseItem != null)
        {
            string itemTypeName = GetItemTypeName(baseItem);
            itemTypeText.text = $"Tipo: {itemTypeName}";
        }

        // Calcular y mostrar precio de venta
        if (sellPriceText != null)
        {
            int sellPrice = CalculateSellPrice(item);
            string formattedPrice = formatMoneyWithThousands ? FormatNumber(sellPrice) : sellPrice.ToString();
            sellPriceText.text = $"Precio de Venta: {formattedPrice}";
        }

        // Establecer el item visualizado en EquipmentManager para que los botones Equipar/Quitar funcionen
        if (equipmentManager != null)
        {
            equipmentManager.SetSelectedItemForEquip(item);
        }
    }

    /// <summary>
    /// Limpia el visor de detalles cuando no hay item seleccionado.
    /// </summary>
    private void ClearDetailView()
    {
        if (detailViewImage != null)
        {
            detailViewImage.sprite = null;
            detailViewImage.enabled = false;
        }

        if (detailViewPanel != null)
        {
            detailViewPanel.SetActive(false);
        }

        // Limpiar todos los textos
        if (levelText != null) levelText.text = "Nv. 0";
        if (hpText != null) hpText.text = "0";
        if (manaText != null) manaText.text = "0";
        if (ataqueText != null) ataqueText.text = "0";
        if (defensaText != null) defensaText.text = "0";
        if (velocidadAtaqueText != null) velocidadAtaqueText.text = "0";
        if (ataqueCriticoText != null) ataqueCriticoText.text = "0";
        if (danoCriticoText != null) danoCriticoText.text = "0";
        if (suerteText != null) suerteText.text = "0";
        if (destrezaText != null) destrezaText.text = "0";
        if (rarezaText != null)
        {
            rarezaText.text = "";
            rarezaText.color = Color.white; // Restaurar color por defecto
        }

        if (itemTitleText != null)
        {
            itemTitleText.text = "";
            itemTitleText.color = Color.white;
        }

        if (descriptionText != null)
        {
            descriptionText.text = "";
        }

        if (itemTypeText != null)
        {
            itemTypeText.text = "";
        }

        if (sellPriceText != null)
        {
            sellPriceText.text = "";
        }
    }

    /// <summary>
    /// Se llama cuando se equipa un item. Actualiza el panel de "equipado" en el slot correspondiente.
    /// </summary>
    private void OnItemEquipped(EquipmentManager.EquipmentSlotType slot, ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
            return;

        // Buscar el slot del inventario que contiene este item
        UpdateEquippedPanelForItem(itemInstance, true);
    }

    /// <summary>
    /// Se llama cuando se mejora un item. Refresca todos los slots para actualizar los niveles.
    /// </summary>
    private void OnItemImproved(ItemInstance itemInstance, int previousLevel, int newLevel)
    {
        // SOLUCIÓN: Forzar actualización completa de todos los slots cuando se mejora un item
        // Esto asegura que los niveles se actualicen correctamente sin necesidad de hacer clic
        RefreshAllSlots(forceRefreshAll: true);
        
        // SOLUCIÓN: Forzar actualización de todos los TextMeshProUGUI después de refrescar
        // Esto asegura que los textos se rendericen correctamente incluso si el panel estaba inactivo
        ForceUpdateAllLevelTexts();
        
        // SOLUCIÓN CRÍTICA: Si el objeto mejorado es el que está siendo visualizado en el visor,
        // actualizar el visor para mostrar los nuevos datos (nivel y stats actualizados)
        // El problema es que cuando se mejora un objeto, el ItemInstance se actualiza pero el visor
        // no se actualiza automáticamente, por lo que muestra los valores antiguos
        if (currentlyViewedItem != null && currentlyViewedItem.IsValid() && 
            currentlyViewedItem.IsSameInstance(itemInstance))
        {
            // El objeto visualizado fue mejorado, actualizar el visor con los nuevos datos
            // Leer el item actualizado directamente desde el inventario para asegurar que tenemos
            // la referencia más reciente con el nivel y stats actualizados
            if (currentlySelectedSlot != null)
            {
                int slotIndex = currentlySelectedSlot.GetSlotIndex();
                ItemInstance updatedItem = inventoryManager.GetItem(slotIndex);
                if (updatedItem != null && updatedItem.IsValid() && updatedItem.IsSameInstance(itemInstance))
                {
                    // Actualizar el slot con el item actualizado
                    currentlySelectedSlot.SetItem(updatedItem, forceUpdate: true);
                    // Actualizar el visor con los nuevos datos
                    UpdateDetailView(currentlySelectedSlot);
                }
            }
        }
    }

    /// <summary>
    /// Fuerza la actualización de todos los textos de nivel en los slots del inventario.
    /// Útil cuando el panel estaba inactivo durante una mejora de item.
    /// </summary>
    private void ForceUpdateAllLevelTexts()
    {
        if (slotComponents == null)
            return;

        for (int i = 0; i < slotComponents.Length && i < InventoryManager.INVENTORY_SIZE; i++)
        {
            if (slotComponents[i] != null)
            {
                ItemInstance item = slotComponents[i].GetItem();
                if (item != null && item.IsValid())
                {
                    // SOLUCIÓN: Leer el item actualizado directamente desde el inventario
                    // Esto asegura que tenemos el nivel más reciente
                    ItemInstance inventoryItem = inventoryManager.GetItem(i);
                    if (inventoryItem != null && inventoryItem.IsValid())
                    {
                        // Forzar actualización del slot para asegurar que el texto se renderice
                        slotComponents[i].SetItem(inventoryItem, forceUpdate: true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Se llama cuando se desequipa un item. Actualiza el panel de "equipado" en el slot correspondiente.
    /// NO limpia el visor porque el item sigue en el inventario, solo se desequipó.
    /// </summary>
    private void OnItemUnequipped(EquipmentManager.EquipmentSlotType slot, ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
            return;

        // Ocultar el panel de equipado
        UpdateEquippedPanelForItem(itemInstance, false);

        // NO limpiar el visor - el item sigue en el inventario y puede seguir visualizándose
        // Si el item desequipado es el que está visualizado, el visor se mantiene mostrándolo
    }

    /// <summary>
    /// Actualiza el panel de "equipado" para un item específico.
    /// Busca el slot del inventario que contiene este item y muestra/oculta el panel.
    /// </summary>
    private void UpdateEquippedPanelForItem(ItemInstance itemInstance, bool isEquipped)
    {
        if (itemInstance == null || !itemInstance.IsValid() || inventoryManager == null)
            return;

        // Buscar en todos los slots del inventario
        for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
        {
            if (i < slotComponents.Length && slotComponents[i] != null)
            {
                ItemInstance slotItem = slotComponents[i].GetItem();
                
                // Comparar instancias por identidad (instanceId) o referencia
                bool isSameItem = slotItem != null && slotItem.IsValid() && slotItem.IsSameInstance(itemInstance);
                
                if (isSameItem)
                {
                    // Este es el slot que contiene el item, actualizar su panel
                    if (i < panelsEquipped.Length && panelsEquipped[i] != null)
                    {
                        panelsEquipped[i].SetActive(isEquipped);
                        Debug.Log($"Panel de equipado {(isEquipped ? "mostrado" : "ocultado")} para slot {i}");
                    }
                    return; // Encontrado, salir
                }
            }
        }
        
        Debug.LogWarning($"No se encontró el item equipado en el inventario para actualizar el panel. Item: {itemInstance.GetItemName()}");
    }

    /// <summary>
    /// Refresca todos los paneles de "equipado" según el estado actual del equipo.
    /// </summary>
    private void RefreshEquippedPanels()
    {
        if (equipmentManager == null || inventoryManager == null)
            return;

        // Obtener todos los items equipados
        List<ItemInstance> equippedItems = new List<ItemInstance>();
        // CORRECCIÓN: Iterar sobre todos los slots disponibles (10 en lugar de 6)
        foreach (EquipmentManager.EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
        {
            ItemInstance equippedItem = equipmentManager.GetEquippedItem(slotType);
            if (equippedItem != null && equippedItem.IsValid())
            {
                equippedItems.Add(equippedItem);
            }
        }

        // Primero, ocultar todos los paneles
        for (int i = 0; i < panelsEquipped.Length && i < InventoryManager.INVENTORY_SIZE; i++)
        {
            if (panelsEquipped[i] != null)
            {
                panelsEquipped[i].SetActive(false);
            }
        }

        // Luego, mostrar los paneles de los items que están equipados
        foreach (ItemInstance equippedItem in equippedItems)
        {
            UpdateEquippedPanelForItem(equippedItem, true);
        }
    }

    /// <summary>
    /// Obtiene el color correspondiente a una rareza.
    /// Si la rareza no está en el diccionario, retorna blanco por defecto.
    /// </summary>
    private Color GetRarityColor(string rarity)
    {
        if (string.IsNullOrEmpty(rarity))
            return Color.white;

        // Buscar la rareza en el diccionario (case-insensitive)
        string rarityKey = rarity.Trim();
        
        if (RarityColors.ContainsKey(rarityKey))
        {
            return RarityColors[rarityKey];
        }

        // Si no se encuentra exactamente, buscar sin importar mayúsculas/minúsculas
        foreach (var kvp in RarityColors)
        {
            if (string.Equals(kvp.Key, rarityKey, System.StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        // Color por defecto si no se encuentra
        return Color.white;
    }

    /// <summary>
    /// Obtiene el nombre del tipo/clase del item en español.
    /// Considera el ItemType y el nombre del item para casos especiales (Escudo, Casco, Botas, etc.).
    /// </summary>
    private string GetItemTypeName(ItemData itemData)
    {
        if (itemData == null)
            return "Desconocido";

        string itemNameLower = itemData.itemName.ToLower();

        // Casos especiales basados en el nombre del item
        if (itemNameLower.Contains("escudo"))
            return "Escudo";
        
        if (itemNameLower.Contains("casco") || itemNameLower.Contains("sombrero"))
            return "Casco";
        
        if (itemNameLower.Contains("armadura") || itemNameLower.Contains("pechera"))
            return "Armadura";
        
        if (itemNameLower.Contains("botas"))
            return "Botas";
        
        if (itemNameLower.Contains("guantes"))
            return "Guantes";
        
        if (itemNameLower.Contains("cinturón") || itemNameLower.Contains("cinturon"))
            return "Cinturón";
        
        if (itemNameLower.Contains("arma"))
            return "Arma";

        // Si no coincide con casos especiales, usar el ItemType
        switch (itemData.itemType)
        {
            case ItemType.Montura:
                return "Montura";
            case ItemType.Casco:
                return "Casco";
            case ItemType.Collar:
                return "Collar";
            case ItemType.Arma:
                return "Arma";
            case ItemType.Armadura:
                return "Armadura";
            case ItemType.Escudo:
                return "Escudo";
            case ItemType.Guantes:
                return "Guantes";
            case ItemType.Cinturon:
                return "Cinturón";
            case ItemType.Anillo:
                return "Anillo";
            case ItemType.Botas:
                return "Botas";
            case ItemType.Otros:
                return "Otros";
            default:
                return "Desconocido";
        }
    }

    /// <summary>
    /// Calcula el precio de venta de un ItemInstance.
    /// Usa ShopService si está disponible, o calcula directamente.
    /// </summary>
    private int CalculateSellPrice(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
            return 0;

        // Si tenemos ShopService, usar su método
        if (shopService != null)
        {
            return shopService.CalculateSellPrice(itemInstance);
        }

        // Si no, calcular directamente (misma lógica que ShopService)
        ItemData baseItem = itemInstance.baseItem;
        if (baseItem == null)
            return 0;

        // Precio base: 50% del precio de compra del ItemData base
        int baseSellPrice = Mathf.Max(1, baseItem.price / 2);

        // Bonificación por nivel: +5% por cada nivel por encima del nivel 1
        float levelMultiplier = 1.0f + ((itemInstance.currentLevel - 1) * 0.05f);

        int finalPrice = Mathf.RoundToInt(baseSellPrice * levelMultiplier);
        return Mathf.Max(1, finalPrice);
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón Vender.
    /// Vende el item visualizado en el visor.
    /// </summary>
    private void OnSellButtonClicked()
    {
        if (currentlyViewedItem == null || !currentlyViewedItem.IsValid())
        {
            Debug.LogWarning("No hay item visualizado para vender. Selecciona un item del inventario primero.");
            return;
        }

        if (currentlySelectedSlot == null)
        {
            Debug.LogWarning("No hay slot seleccionado para vender.");
            return;
        }

        int slotIndex = currentlySelectedSlot.GetSlotIndex();
        if (slotIndex < 0 || slotIndex >= InventoryManager.INVENTORY_SIZE)
        {
            Debug.LogWarning($"Índice de slot inválido: {slotIndex}");
            return;
        }

        // Verificar que el item en el slot sea el mismo que el visualizado
        ItemInstance itemInSlot = inventoryManager.GetItem(slotIndex);
        if (itemInSlot == null || !itemInSlot.IsValid() ||
            itemInSlot.baseItem != currentlyViewedItem.baseItem ||
            itemInSlot.currentLevel != currentlyViewedItem.currentLevel)
        {
            Debug.LogWarning("El item en el slot no coincide con el item visualizado.");
            return;
        }

        // CORRECCIÓN: Verificar si el item está equipado
        if (equipmentManager != null)
        {
            EquipmentManager.EquipmentSlotType? equippedSlot = FindEquippedSlot(itemInSlot);
            if (equippedSlot.HasValue)
            {
                // El item está equipado, mostrar panel de confirmación
                if (confirmSellPanel != null)
                {
                    // Guardar el item y el slot para venderlo después de confirmar
                    pendingSellItem = itemInSlot;
                    pendingSellSlotIndex = slotIndex;
                    
                    // Mostrar el panel de confirmación
                    confirmSellPanel.SetActive(true);
                    Debug.Log($"Item '{itemInSlot.GetItemName()}' está equipado. Mostrando panel de confirmación...");
                }
                else
                {
                    Debug.LogWarning("Panel de confirmación no asignado. Desequipando y vendiendo directamente...");
                    // Si no hay panel, desequipar y vender directamente (comportamiento anterior)
                    equipmentManager.UnequipItem(equippedSlot.Value);
                    SellItemDirectly(itemInSlot, slotIndex);
                }
                return; // Salir aquí, la venta se completará cuando el usuario confirme
            }
        }

        // Si no está equipado, vender directamente
        SellItemDirectly(itemInSlot, slotIndex);
    }

    /// <summary>
    /// Actualiza el display de monedas del jugador.
    /// Se llama automáticamente cuando cambia el dinero.
    /// </summary>
    private void UpdatePlayerMoneyDisplay(int newAmount)
    {
        if (playerMoneyText == null)
            return;

        string formattedAmount = formatMoneyWithThousands ? FormatNumber(newAmount) : newAmount.ToString();
        playerMoneyText.text = string.Format(moneyFormat, formattedAmount);
    }

    /// <summary>
    /// Formatea un número con separadores de miles.
    /// </summary>
    private string FormatNumber(int number)
    {
        return number.ToString("N0");
    }

    /// <summary>
    /// CORRECCIÓN: Busca en qué slot de equipo está equipado un item específico.
    /// </summary>
    /// <param name="itemInstance">ItemInstance a buscar</param>
    /// <returns>El slot donde está equipado, o null si no está equipado</returns>
    private EquipmentManager.EquipmentSlotType? FindEquippedSlot(ItemInstance itemInstance)
    {
        if (equipmentManager == null || itemInstance == null || !itemInstance.IsValid())
            return null;

        // Buscar en todos los slots de equipo
        foreach (EquipmentManager.EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
        {
            ItemInstance equippedItem = equipmentManager.GetEquippedItem(slotType);
            if (equippedItem != null && equippedItem.IsValid() && equippedItem.IsSameInstance(itemInstance))
            {
                return slotType;
            }
        }

        return null;
    }

    /// <summary>
    /// CORRECCIÓN: Vende un item directamente sin mostrar panel de confirmación.
    /// Se usa cuando el item no está equipado o cuando el usuario confirma la venta de un item equipado.
    /// </summary>
    private void SellItemDirectly(ItemInstance itemInSlot, int slotIndex)
    {
        // CORRECCIÓN: Verificar si el item está equipado y desequiparlo antes de vender
        if (equipmentManager != null)
        {
            EquipmentManager.EquipmentSlotType? equippedSlot = FindEquippedSlot(itemInSlot);
            if (equippedSlot.HasValue)
            {
                Debug.Log($"Item '{itemInSlot.GetItemName()}' está equipado en el slot {equippedSlot.Value}. Desequipando antes de vender...");
                equipmentManager.UnequipItem(equippedSlot.Value);
            }
        }

        // Calcular precio de venta
        int sellPrice = CalculateSellPrice(itemInSlot);

        // Verificar que tenemos PlayerMoney
        if (playerMoney == null)
        {
            Debug.LogError("PlayerMoney no está asignado. No se puede vender el item.");
            return;
        }

        // Remover el item del inventario
        if (inventoryManager.RemoveItem(slotIndex))
        {
            // Añadir dinero al jugador
            playerMoney.AddMoney(sellPrice);

            Debug.Log($"Item '{itemInSlot.GetItemName()}' (nivel {itemInSlot.currentLevel}) vendido por {sellPrice} monedas.");

            // SOLUCIÓN: Limpiar también la selección en EquipmentManager para evitar duplicación al equipar
            if (equipmentManager != null)
            {
                equipmentManager.SetSelectedItemForEquip(null);
            }

            // Limpiar el visor ya que el item ya no existe
            ClearDetailView();
            currentlyViewedItem = null;
            currentlySelectedSlot = null;
        }
        else
        {
            Debug.LogError($"Error al remover el item del inventario en el slot {slotIndex}.");
        }
    }

    /// <summary>
    /// CORRECCIÓN: Se llama cuando el usuario confirma la venta de un item equipado.
    /// Vende el item y lo desequipa.
    /// </summary>
    private void OnConfirmSellEquippedItem()
    {
        if (confirmSellPanel != null)
        {
            confirmSellPanel.SetActive(false);
        }

        if (pendingSellItem != null && pendingSellItem.IsValid())
        {
            // Vender el item (ya incluye desequipar si está equipado)
            SellItemDirectly(pendingSellItem, pendingSellSlotIndex);
            
            // Limpiar las variables pendientes
            pendingSellItem = null;
            pendingSellSlotIndex = -1;
        }
        else
        {
            Debug.LogWarning("No hay item pendiente para vender.");
        }
    }

    /// <summary>
    /// CORRECCIÓN: Se llama cuando el usuario cancela la venta de un item equipado.
    /// Cierra el panel sin vender el item.
    /// </summary>
    private void OnCancelSellEquippedItem()
    {
        if (confirmSellPanel != null)
        {
            confirmSellPanel.SetActive(false);
        }

        // Limpiar las variables pendientes
        pendingSellItem = null;
        pendingSellSlotIndex = -1;
        
        Debug.Log("Venta de item equipado cancelada.");
    }
}

