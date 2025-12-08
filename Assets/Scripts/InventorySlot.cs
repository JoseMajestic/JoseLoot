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

    // Eventos
    public System.Action<InventorySlot> OnSlotClicked;
    public System.Action<InventorySlot, ItemInstance> OnItemChanged;

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
        // SOLUCIÓN ESTRUCTURAL: Si forceUpdate es true, siempre actualizar incluso si el item es el mismo
        // Esto es necesario cuando el panel se reactiva porque Unity puede haber limpiado los sprites
        if (forceUpdate || currentItem != itemInstance)
        {
            currentItem = itemInstance;
            UpdateVisuals(forceUpdate);
            OnItemChanged?.Invoke(this, itemInstance);
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
        // SOLUCIÓN: Asegurar que buttonImage esté inicializado (por si el panel estaba desactivado)
        if (buttonImage == null && slotButton != null)
        {
            buttonImage = slotButton.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = slotButton.GetComponentInChildren<Image>();
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
                
                // SOLUCIÓN: Forzar actualización del sprite incluso si ya está asignado
                // Esto es necesario porque Unity puede haber limpiado la referencia al desactivar el panel
                if (itemSprite != null)
                {
                    // SOLUCIÓN ESTRUCTURAL: Si forceRefresh es true, limpiar primero el sprite para forzar actualización completa
                    // Esto asegura que Unity recargue el sprite incluso si estaba asignado antes
                    if (forceRefresh)
                    {
                        buttonImage.sprite = null;
                        // Forzar actualización del canvas después de limpiar
                        Canvas.ForceUpdateCanvases();
                    }
                    
                    // Asegurar que el componente Image esté habilitado y activo
                    if (!buttonImage.enabled)
                    {
                        buttonImage.enabled = true;
                    }
                    
                    // Asegurar que el GameObject esté activo
                    if (!buttonImage.gameObject.activeSelf)
                    {
                        buttonImage.gameObject.SetActive(true);
                    }
                    
                    // Asignar el sprite - hacerlo de manera forzada
                    buttonImage.sprite = itemSprite;
                    
                    // SOLUCIÓN: Verificar que el sprite se asignó correctamente
                    if (buttonImage.sprite != itemSprite)
                    {
                        // Si no se asignó, intentar de nuevo forzando
                        buttonImage.sprite = null;
                        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(buttonImage.rectTransform);
                        buttonImage.sprite = itemSprite;
                    }
                    
                    // Forzar actualización del canvas para asegurar que el sprite se renderice
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
                // Asegurarse de que el texto siempre se muestre si hay item
                levelText.text = $"Nv. {currentItem.currentLevel}";
                levelText.gameObject.SetActive(true); // Forzar activación
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

