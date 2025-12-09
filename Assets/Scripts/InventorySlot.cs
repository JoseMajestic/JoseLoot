using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Representa un slot individual del inventario en la UI.
/// El botón tiene su propia Image que se usa para el sprite del item.
/// El texto dentro del botón muestra el nivel del objeto en formato "Nv. [número]".
/// </summary>
public class InventorySlot : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Referencia al componente TextMeshPro que muestra el nivel del objeto (opcional, se busca automáticamente si no se asigna)")]
    [SerializeField] private TextMeshProUGUI levelText;

    private Button slotButton;
    private Image buttonImage; // Image del botón (se obtiene automáticamente)
    private ItemInstance currentItem;
    private int slotIndex;
    private InventoryManager inventoryManager;
    private bool isSelected = false;
    
    // SOLUCIÓN: Guardar el nivel y versión del item para detectar cambios
    // Esto permite detectar cuando el ItemInstance en memoria cambió aunque la referencia sea la misma
    private int lastKnownLevel = -1;
    private int lastKnownVersion = -1;

    // Eventos
    public System.Action<InventorySlot> OnSlotClicked;
    public System.Action<InventorySlot, ItemInstance> OnItemChanged;
    
    // SOLUCIÓN ARQUITECTÓNICA: Referencia al item anterior para desuscripción
    private ItemInstance previousItemForUnsubscribe = null;

    private void Awake()
    {
        // Obtener el Button del mismo GameObject
        slotButton = GetComponent<Button>();
        if (slotButton == null)
        {
            slotButton = GetComponentInChildren<Button>();
        }

        // Obtener el Image del Button (el botón tiene su propia imagen)
        if (slotButton != null)
        {
            buttonImage = slotButton.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = slotButton.GetComponentInChildren<Image>();
            }
            slotButton.onClick.AddListener(OnSlotClick);
        }

        // Si no se asignó el levelText en el Inspector, intentar encontrarlo automáticamente
        if (levelText == null)
        {
            levelText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    /// <summary>
    /// Establece la referencia al texto del nivel (llamado desde InventoryUIManager).
    /// </summary>
    public void SetLevelText(TextMeshProUGUI text)
    {
        levelText = text;
    }

    /// <summary>
    /// SOLUCIÓN SENCILLA: Fuerza la actualización del texto del nivel directamente.
    /// Lee el nivel desde el ItemInstance actual y actualiza el texto sin pasar por SetItem().
    /// </summary>
    public void ForceRefreshLevelTextDirectly()
    {
        if (levelText == null)
        {
            // Intentar encontrar el texto si no está asignado
            levelText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (levelText == null)
            return;

        // Asegurar que el componente esté activo y habilitado
        if (!levelText.gameObject.activeSelf)
        {
            levelText.gameObject.SetActive(true);
        }
        if (!levelText.enabled)
        {
            levelText.enabled = true;
        }

        // Leer el nivel directamente desde el ItemInstance actual
        if (currentItem != null && currentItem.IsValid())
        {
            // Limpiar primero para forzar actualización
            levelText.text = "";
            Canvas.ForceUpdateCanvases();
            
            // Actualizar con el nivel actual del ItemInstance
            levelText.text = $"Nv. {currentItem.currentLevel}";
            
            // Forzar actualización del TextMeshProUGUI
            levelText.ForceMeshUpdate();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(levelText.rectTransform);
            Canvas.ForceUpdateCanvases();
            
            // Registrar para rebuild del canvas
            if (levelText.canvas != null)
            {
                UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(levelText);
            }
        }
        else
        {
            // Slot vacío: limpiar texto
            levelText.text = "";
            levelText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// SOLUCIÓN: Fuerza la actualización de los visuales directamente, sin pasar por SetSelected.
    /// Útil cuando se llama desde corrutinas o cuando el componente puede no estar completamente listo.
    /// Esto evita depender del EventSystem y del estado del componente que requiere un click real del mouse.
    /// </summary>
    public void ForceUpdateVisualsDirectly()
    {
        // SOLUCIÓN CRÍTICA: Asegurar que buttonImage esté inicializado ANTES de llamar a UpdateVisuals()
        // Igual que HeroProfileManager que tiene referencias directas, aquí debemos obtenerlas dinámicamente
        // pero asegurarnos de que estén disponibles antes de usarlas
        if (buttonImage == null)
        {
            if (slotButton == null)
            {
                slotButton = GetComponent<Button>();
                if (slotButton == null)
                {
                    slotButton = GetComponentInChildren<Button>();
                }
            }
            
            if (slotButton != null)
            {
                buttonImage = slotButton.GetComponent<Image>();
                if (buttonImage == null)
                {
                    buttonImage = slotButton.GetComponentInChildren<Image>();
                }
            }
        }
        
        // Asegurar que el componente esté activo y habilitado
        if (levelText != null)
        {
            if (!levelText.gameObject.activeSelf)
            {
                levelText.gameObject.SetActive(true);
            }
            if (!levelText.enabled)
            {
                levelText.enabled = true;
            }
        }
        
        if (buttonImage != null)
        {
            if (!buttonImage.gameObject.activeSelf)
            {
                buttonImage.gameObject.SetActive(true);
            }
            if (!buttonImage.enabled)
            {
                buttonImage.enabled = true;
            }
        }
        
        // Llamar directamente a UpdateVisuals con forceRefresh: true
        // Esto fuerza la actualización completa de sprites y textos sin depender del EventSystem
        // IGUAL que HeroProfileManager que asigna directamente el sprite
        UpdateVisuals(forceRefresh: true);
    }

    /// <summary>
    /// Inicializa el slot con su índice y referencia al InventoryManager.
    /// </summary>
    public void Initialize(int index, InventoryManager manager)
    {
        slotIndex = index;
        inventoryManager = manager;
        UpdateVisuals();
    }

    /// <summary>
    /// Establece el item que se muestra en este slot.
    /// </summary>
    public void SetItem(ItemInstance itemInstance, bool forceUpdate = false)
    {
        // SOLUCIÓN ESTRUCTURAL: Si forceUpdate es true, SIEMPRE actualizar sin importar nada más
        // Esto es crítico cuando se abre el panel después de una mejora, para asegurar que los textos se actualicen
        if (forceUpdate)
        {
            // Si forceUpdate es true, actualizar siempre, pero primero actualizar los valores conocidos
            // para que la próxima vez se detecten cambios correctamente
            if (currentItem == itemInstance && currentItem != null && currentItem.IsValid() && itemInstance != null && itemInstance.IsValid())
            {
                // Misma referencia: actualizar valores conocidos y forzar actualización visual
                lastKnownLevel = itemInstance.currentLevel;
                lastKnownVersion = itemInstance.GetVersion();
            }
            
            // SOLUCIÓN ARQUITECTÓNICA: Desuscribirse del item anterior antes de cambiar
            if (previousItemForUnsubscribe != null && previousItemForUnsubscribe.IsValid())
            {
                previousItemForUnsubscribe.OnLevelChanged -= OnItemLevelChanged;
            }
            
            previousItemForUnsubscribe = currentItem;
            currentItem = itemInstance;
            
            // SOLUCIÓN ARQUITECTÓNICA: Suscribirse al nuevo item para recibir notificaciones de cambio de nivel
            if (itemInstance != null && itemInstance.IsValid())
            {
                itemInstance.OnLevelChanged += OnItemLevelChanged;
                lastKnownLevel = itemInstance.currentLevel;
                lastKnownVersion = itemInstance.GetVersion();
            }
            else
            {
                lastKnownLevel = -1;
                lastKnownVersion = -1;
            }
            
            UpdateVisuals(forceRefresh: true);
            OnItemChanged?.Invoke(this, itemInstance);
            return; // Salir temprano cuando forceUpdate es true
        }
        
        // Lógica normal cuando forceUpdate es false
        bool needsUpdate = currentItem != itemInstance;
        
        // SOLUCIÓN CRÍTICA: Detectar cambios por versión además de por referencia
        // El problema es que cuando se mejora un objeto, el ItemInstance se actualiza en memoria (misma referencia)
        // pero el texto visual no se actualiza porque Unity no detecta el cambio
        // La versión cambia cuando el nivel cambia, permitiendo detectar mejoras sin cambiar el GUID
        if (!needsUpdate && currentItem != null && currentItem.IsValid() && itemInstance != null && itemInstance.IsValid())
        {
            // Si es la misma instancia (mismo GUID) pero la versión cambió, necesita actualizar
            if (currentItem.IsSameInstance(itemInstance) && currentItem.HasChangedVersion(itemInstance))
            {
                needsUpdate = true; // El item fue mejorado, necesita actualizar el nivel y stats
            }
            // También verificar por nivel (compatibilidad hacia atrás)
            else if (currentItem.IsSameInstance(itemInstance) && currentItem.currentLevel != itemInstance.currentLevel)
            {
                needsUpdate = true; // El item fue mejorado, necesita actualizar el nivel
            }
        }
        
        // SOLUCIÓN ADICIONAL: Si la referencia es la misma pero el nivel o versión cambió, forzar actualización
        // Esto es crítico cuando SyncInventoryLevelsFromProfile() actualiza el nivel del ItemInstance en memoria
        // El slot tiene la misma referencia pero el nivel interno cambió, así que necesitamos actualizar el texto
        if (!needsUpdate && currentItem == itemInstance && currentItem != null && currentItem.IsValid() && itemInstance != null && itemInstance.IsValid())
        {
            // Comparar el nivel y versión actuales con los últimos conocidos
            // Si el nivel o versión cambió, necesita actualizar
            int currentLevel = itemInstance.currentLevel;
            int currentVersion = itemInstance.GetVersion();
            
            if (lastKnownLevel != currentLevel || lastKnownVersion != currentVersion)
            {
                needsUpdate = true; // El nivel o versión cambió, actualizar el texto
            }
        }
        
        if (needsUpdate)
        {
            // SOLUCIÓN ARQUITECTÓNICA: Desuscribirse del item anterior antes de cambiar
            if (previousItemForUnsubscribe != null && previousItemForUnsubscribe.IsValid())
            {
                previousItemForUnsubscribe.OnLevelChanged -= OnItemLevelChanged;
            }
            
            previousItemForUnsubscribe = currentItem;
            currentItem = itemInstance;
            
            // SOLUCIÓN ARQUITECTÓNICA: Suscribirse al nuevo item para recibir notificaciones de cambio de nivel
            if (itemInstance != null && itemInstance.IsValid())
            {
                itemInstance.OnLevelChanged += OnItemLevelChanged;
                lastKnownLevel = itemInstance.currentLevel;
                lastKnownVersion = itemInstance.GetVersion();
            }
            else
            {
                lastKnownLevel = -1;
                lastKnownVersion = -1;
            }
            
            UpdateVisuals(forceRefresh: false);
            OnItemChanged?.Invoke(this, itemInstance);
        }
    }

    /// <summary>
    /// SOLUCIÓN ARQUITECTÓNICA: Se llama cuando el nivel del ItemInstance cambia.
    /// Fuerza la actualización visual del slot automáticamente.
    /// </summary>
    private void OnItemLevelChanged(ItemInstance item, int oldLevel, int newLevel)
    {
        // Solo actualizar si este slot tiene este item
        if (currentItem == item)
        {
            // Actualizar valores conocidos
            lastKnownLevel = newLevel;
            lastKnownVersion = item.GetVersion();
            
            // Forzar actualización visual
            UpdateVisuals(forceRefresh: true);
        }
    }

    /// <summary>
    /// Obtiene el ItemInstance actual en este slot.
    /// </summary>
    public ItemInstance GetItem()
    {
        return currentItem;
    }

    /// <summary>
    /// Obtiene el ItemData base del item en este slot (para compatibilidad).
    /// </summary>
    public ItemData GetBaseItem()
    {
        return currentItem != null && currentItem.IsValid() ? currentItem.baseItem : null;
    }

    /// <summary>
    /// Obtiene el índice de este slot.
    /// </summary>
    public int GetSlotIndex()
    {
        return slotIndex;
    }

    /// <summary>
    /// Selecciona o deselecciona este slot.
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals(forceRefresh: true); // Forzar actualización del sprite cuando se selecciona
    }

    /// <summary>
    /// Verifica si este slot está seleccionado.
    /// </summary>
    public bool IsSelected()
    {
        return isSelected;
    }

    /// <summary>
    /// Fuerza la recarga del sprite desde ItemData.
    /// Útil cuando el panel se reactiva y Unity puede haber limpiado los sprites.
    /// </summary>
    public void ForceRefreshSprite()
    {
        // Asegurar que buttonImage esté inicializado
        if (buttonImage == null && slotButton != null)
        {
            buttonImage = slotButton.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = slotButton.GetComponentInChildren<Image>();
            }
        }

        if (buttonImage == null)
            return;

        // Si hay un item, forzar recarga del sprite desde ItemData
        if (currentItem != null && currentItem.IsValid())
        {
            // Obtener sprite fresco desde ItemData (como si fuera una nueva "instancia")
            Sprite itemSprite = currentItem.GetItemSprite();
            
            if (itemSprite != null)
            {
                // Forzar asignación del sprite (incluso si ya está asignado)
                buttonImage.sprite = null; // Limpiar primero para forzar actualización
                buttonImage.sprite = itemSprite; // Asignar el sprite fresco
                buttonImage.enabled = true;
                
                // Forzar actualización del canvas
                if (buttonImage.canvas != null)
                {
                    UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(buttonImage);
                }
            }
        }
    }

    /// <summary>
    /// Actualiza la visualización del slot.
    /// Usa la Image del Button para mostrar el sprite del item.
    /// Actualiza el texto del nivel en formato "Nv. [número]".
    /// </summary>
    private void UpdateVisuals(bool forceRefresh = false)
    {
        // SOLUCIÓN CRÍTICA: Asegurar que buttonImage esté inicializado (por si el panel estaba desactivado)
        // Igual que HeroProfileManager que tiene referencias directas, aquí las obtenemos dinámicamente
        // pero debemos asegurarnos de que se obtengan correctamente cada vez
        if (buttonImage == null && slotButton != null)
        {
            buttonImage = slotButton.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = slotButton.GetComponentInChildren<Image>();
            }
        }
        
        // SOLUCIÓN: Si aún es null, intentar obtener el Button primero
        if (buttonImage == null)
        {
            if (slotButton == null)
            {
                slotButton = GetComponent<Button>();
                if (slotButton == null)
                {
                    slotButton = GetComponentInChildren<Button>();
                }
            }
            
            if (slotButton != null)
            {
                buttonImage = slotButton.GetComponent<Image>();
                if (buttonImage == null)
                {
                    buttonImage = slotButton.GetComponentInChildren<Image>();
                }
            }
        }

        if (currentItem == null || !currentItem.IsValid())
        {
            // Slot vacío
            if (buttonImage != null)
            {
                // Limpiar el sprite para que vuelva al sprite por defecto del botón
                buttonImage.sprite = null;
                // El botón mostrará su sprite por defecto (el que tiene configurado en el componente Image)
            }

            if (levelText != null)
            {
                levelText.text = ""; // Ocultar texto cuando no hay item
                levelText.gameObject.SetActive(false);
            }
        }
        else
        {
            // Slot ocupado
            if (buttonImage != null)
            {
                // SOLUCIÓN MEJORADA: No salir temprano - forzar la asignación incluso si el GameObject no está completamente activo
                // Unity puede reportar que no está activo incluso cuando debería estarlo
                
                // SOLUCIÓN: Siempre obtener el sprite fresco desde ItemData
                // Esto asegura que el sprite se recargue incluso si Unity lo limpió al desactivar el panel
                Sprite itemSprite = currentItem.GetItemSprite();
                
                // SOLUCIÓN ESTRUCTURAL: Siempre obtener el sprite fresco desde ItemData (como una "instancia/clon" fresca)
                // Esto asegura que el sprite se recargue incluso si Unity lo limpió al desactivar el panel
                // IGUAL que hace HeroProfileManager.UpdateEquipmentSlot() que funciona correctamente
                if (itemSprite != null)
                {
                    // SOLUCIÓN ESTRUCTURAL: Si forceRefresh es true, limpiar primero el sprite para forzar actualización completa
                    // Esto asegura que Unity recargue el sprite incluso si estaba asignado antes
                    // IGUAL que HeroProfileManager línea 767-772
                    if (forceRefresh)
                    {
                        buttonImage.sprite = null;
                        // Forzar actualización del canvas después de limpiar
                        Canvas.ForceUpdateCanvases();
                    }
                    
                    // Asegurar que el componente Image esté habilitado y activo
                    // IGUAL que HeroProfileManager líneas 775-784
                    if (!buttonImage.enabled)
                    {
                        buttonImage.enabled = true;
                    }
                    
                    // Asegurar que el GameObject esté activo
                    if (!buttonImage.gameObject.activeSelf)
                    {
                        buttonImage.gameObject.SetActive(true);
                    }
                    
                    // Asignar el sprite fresco desde ItemData - hacerlo de manera forzada
                    // IGUAL que HeroProfileManager línea 787
                    buttonImage.sprite = itemSprite;
                    
                    // SOLUCIÓN: Verificar que el sprite se asignó correctamente
                    // IGUAL que HeroProfileManager líneas 790-796
                    if (buttonImage.sprite != itemSprite)
                    {
                        // Si no se asignó, intentar de nuevo forzando
                        buttonImage.sprite = null;
                        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(buttonImage.rectTransform);
                        buttonImage.sprite = itemSprite;
                    }
                    
                    // Forzar actualización del canvas para asegurar que el sprite se renderice
                    // IGUAL que HeroProfileManager líneas 799-802
                    UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(buttonImage);
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(buttonImage.rectTransform);
                    // También forzar actualización inmediata del canvas (método estático)
                    Canvas.ForceUpdateCanvases();
                }
                else
                {
                    // Si el sprite es null pero hay item, limpiar para mostrar sprite por defecto
                    buttonImage.sprite = null;
                    buttonImage.enabled = true;
                    Debug.LogWarning($"Item '{currentItem.GetItemName()}' no tiene sprite asignado en ItemData.");
                }
            }

            if (levelText != null)
            {
                // SOLUCIÓN: Asegurar que el TextMeshProUGUI esté activo y habilitado antes de actualizar
                // Esto es crítico cuando el panel estaba inactivo durante la mejora del item
                if (!levelText.gameObject.activeSelf)
                {
                    levelText.gameObject.SetActive(true);
                }
                if (!levelText.enabled)
                {
                    levelText.enabled = true;
                }
                
                // SOLUCIÓN CRÍTICA: Si forceRefresh es true, limpiar primero el texto para forzar actualización completa
                // Esto es similar a cómo se fuerza la actualización del sprite
                // El problema es que el texto solo se actualiza cuando pasa por el visor porque SetSelected() 
                // llama a UpdateVisuals(forceRefresh: true), lo que fuerza la actualización del texto
                if (forceRefresh)
                {
                    levelText.text = "";
                    Canvas.ForceUpdateCanvases();
                }
                
                // SOLUCIÓN ARQUITECTÓNICA: SIEMPRE leer el nivel directamente desde el ItemInstance
                // No usar valores cacheados para el texto final - siempre leer fresco
                // Los valores cacheados (lastKnownLevel) solo se usan para detectar si necesita actualizar
                int currentLevel = currentItem.currentLevel; // Leer fresco, no usar lastKnownLevel
                levelText.text = $"Nv. {currentLevel}";
                
                // SOLUCIÓN: Forzar actualización del TextMeshProUGUI para asegurar que se renderice
                // Similar a cómo se fuerza la actualización del sprite cuando forceRefresh es true
                if (forceRefresh)
                {
                    // Forzar actualización del layout similar a las imágenes
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(levelText.rectTransform);
                }
                
                // Forzar actualización del TextMeshProUGUI (siempre, no solo si está activo)
                // El problema es que isActiveAndEnabled puede ser false si el panel se está activando
                // pero el componente aún no está completamente listo
                levelText.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases();
                
                // SOLUCIÓN ADICIONAL: Forzar actualización del canvas del texto específicamente
                if (levelText.canvas != null)
                {
                    UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(levelText);
                }
            }
        }
    }

    /// <summary>
    /// Maneja el clic en el slot.
    /// </summary>
    private void OnSlotClick()
    {
        OnSlotClicked?.Invoke(this);
    }

    /// <summary>
    /// Muestra información del item en un tooltip (si está implementado).
    /// </summary>
    public void ShowTooltip()
    {
        if (currentItem != null && currentItem.IsValid())
        {
            ItemData baseItem = currentItem.baseItem;
            ItemStats stats = currentItem.GetFinalStats();
            // TODO: Implementar sistema de tooltip
            Debug.Log($"Item: {currentItem.GetItemName()}\nNivel: {currentItem.currentLevel}\nDescripción: {baseItem.description}\nPrecio base: {baseItem.price}\nStats: Ataque={stats.ataque}, Defensa={stats.defensa}, HP={stats.hp}");
        }
    }
}

