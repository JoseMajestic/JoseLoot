using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sistema de cofres que entrega items ordenados de peor a mejor.
/// Al abrir un cofre, muestra el item obtenido y espera confirmación antes de añadirlo al inventario.
/// </summary>
public class ChestManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al ItemDatabase")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Tooltip("Referencia al InventoryManager")]
    [SerializeField] private InventoryManager inventoryManager;

    [Tooltip("Referencia al PlayerMoney")]
    [SerializeField] private PlayerMoney playerMoney;

    [Tooltip("Referencia al InventoryAutoOrganizer")]
    [SerializeField] private InventoryAutoOrganizer inventoryAutoOrganizer;

    [Header("Configuración de Cofres")]
    [Tooltip("Coste en monedas para abrir un cofre")]
    [SerializeField] private int chestCost = 1000;

    [Tooltip("Número de items que se entregan por cofre")]
    [SerializeField] private int itemsPerChest = 1;

    [Tooltip("Si es true, los items se entregan de peor a mejor")]
    [SerializeField] private bool worstToBest = true;

    [Header("UI")]
    [Tooltip("Botón para abrir el cofre")]
    [SerializeField] private Button openChestButton;

    [Tooltip("Texto que muestra el precio en monedas del cofre")]
    [SerializeField] private TextMeshProUGUI priceText;

    [Tooltip("Panel donde se muestra la imagen del objeto obtenido")]
    [SerializeField] private GameObject itemDisplayPanel;

    [Tooltip("Image dentro del panel donde se muestra el sprite del objeto obtenido")]
    [SerializeField] private Image itemDisplayImage;

    [Tooltip("Texto que muestra el nombre del objeto obtenido")]
    [SerializeField] private TextMeshProUGUI itemNameText;

    [Tooltip("Botón de aceptar para confirmar el item obtenido")]
    [SerializeField] private Button acceptButton;

    [Tooltip("Panel de animación que se muestra al abrir cofre (opcional, para compatibilidad)")]
    [SerializeField] private GameObject chestAnimationPanel;

    [Tooltip("Slot UI donde se muestra el item obtenido (opcional, para compatibilidad)")]
    [SerializeField] private GameObject itemSlotUI;

    // Estado interno
    private List<ItemData> availableItems = new List<ItemData>();
    private int currentItemIndex = 0;
    private ItemInstance currentObtainedItem = null; // Item actualmente mostrado (aún no añadido al inventario)

    // Eventos
    public System.Action<ItemInstance> OnItemObtained;
    public System.Action OnChestOpened;
    public System.Action OnChestOpenFailed;

    private void Start()
    {
        // Cargar items disponibles desde ItemDatabase
        LoadAvailableItems();

        // Configurar botón de abrir cofre
        if (openChestButton != null)
        {
            openChestButton.onClick.AddListener(OnOpenChestButtonClicked);
        }

        // Configurar botón de aceptar
        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            acceptButton.gameObject.SetActive(false); // Ocultar inicialmente
        }

        // Ocultar panel de item inicialmente
        if (itemDisplayPanel != null)
        {
            itemDisplayPanel.SetActive(false);
        }

        // Actualizar precio inicial
        UpdatePriceDisplay();
    }

    /// <summary>
    /// Actualiza el texto del precio del cofre.
    /// </summary>
    private void UpdatePriceDisplay()
    {
        if (priceText != null)
        {
            priceText.text = chestCost.ToString();
        }
    }

    /// <summary>
    /// Carga los items disponibles desde ItemDatabase y los ordena.
    /// </summary>
    private void LoadAvailableItems()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase no está asignado. No se pueden cargar items para cofres.");
            return;
        }

        // Obtener items ordenados de peor a mejor
        if (worstToBest)
        {
            availableItems = itemDatabase.GetItemsSortedWorstToBest();
        }
        else
        {
            // Si no es worstToBest, invertir el orden
            availableItems = itemDatabase.GetItemsSortedWorstToBest();
            availableItems.Reverse();
        }

        currentItemIndex = 0;
        Debug.Log($"ChestManager: {availableItems.Count} items cargados para cofres.");
    }

    /// <summary>
    /// Intenta abrir un cofre.
    /// Muestra el item obtenido pero NO lo añade al inventario hasta que se presione aceptar.
    /// </summary>
    /// <returns>True si se abrió exitosamente, false si falló</returns>
    public bool OpenChest()
    {
        // Validar dinero suficiente
        if (playerMoney == null)
        {
            Debug.LogError("PlayerMoney no está asignado. No se puede abrir cofre.");
            OnChestOpenFailed?.Invoke();
            return false;
        }

        if (playerMoney.GetMoney() < chestCost)
        {
            Debug.LogWarning($"Dinero insuficiente para abrir cofre. Necesitas {chestCost}, tienes {playerMoney.GetMoney()}.");
            OnChestOpenFailed?.Invoke();
            return false;
        }

        // Validar espacio en inventario
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager no está asignado. No se puede abrir cofre.");
            OnChestOpenFailed?.Invoke();
            return false;
        }

        if (!inventoryManager.HasSpace())
        {
            Debug.LogWarning("El inventario está lleno. No se puede abrir cofre.");
            OnChestOpenFailed?.Invoke();
            return false;
        }

        // Validar que hay items disponibles
        if (availableItems == null || availableItems.Count == 0)
        {
            Debug.LogWarning("No hay items disponibles en el ItemDatabase.");
            OnChestOpenFailed?.Invoke();
            return false;
        }

        // Validar que no hay un item pendiente de aceptar
        if (currentObtainedItem != null)
        {
            Debug.LogWarning("Ya hay un item pendiente de aceptar. Acepta el item actual antes de abrir otro cofre.");
            OnChestOpenFailed?.Invoke();
            return false;
        }

        // Cobrar el coste del cofre
        playerMoney.SubtractMoney(chestCost);

        // Obtener el siguiente item (pero NO añadirlo al inventario todavía)
        currentObtainedItem = GetNextItem();
        
        if (currentObtainedItem == null || !currentObtainedItem.IsValid())
        {
            Debug.LogWarning("No se pudo obtener un item válido del cofre.");
            OnChestOpenFailed?.Invoke();
            // Devolver el dinero si no se pudo obtener el item
            playerMoney.AddMoney(chestCost);
            return false;
        }

        // Mostrar el item obtenido en el panel
        ShowObtainedItem(currentObtainedItem);

        OnChestOpened?.Invoke();
        Debug.Log($"Cofre abierto. Item obtenido: {currentObtainedItem.GetItemName()} (nivel {currentObtainedItem.currentLevel}). Esperando confirmación...");
        return true;
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón de abrir cofre.
    /// </summary>
    private void OnOpenChestButtonClicked()
    {
        OpenChest();
    }

    /// <summary>
    /// Muestra el item obtenido en el panel de visualización.
    /// </summary>
    private void ShowObtainedItem(ItemInstance item)
    {
        if (item == null || !item.IsValid())
            return;

        // Mostrar panel
        if (itemDisplayPanel != null)
        {
            itemDisplayPanel.SetActive(true);
        }

        // Mostrar sprite del item
        if (itemDisplayImage != null)
        {
            Sprite itemSprite = item.GetItemSprite();
            if (itemSprite != null)
            {
                itemDisplayImage.sprite = itemSprite;
                itemDisplayImage.enabled = true;
            }
            else
            {
                Debug.LogWarning($"El item '{item.GetItemName()}' no tiene sprite asignado.");
                itemDisplayImage.sprite = null;
            }
        }

        // Mostrar nombre del item
        if (itemNameText != null)
        {
            itemNameText.text = item.GetItemName();
            itemNameText.gameObject.SetActive(true);
        }

        // Mostrar botón de aceptar
        if (acceptButton != null)
        {
            acceptButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón de aceptar.
    /// Añade el item al inventario y oculta el panel.
    /// </summary>
    private void OnAcceptButtonClicked()
    {
        if (currentObtainedItem == null || !currentObtainedItem.IsValid())
        {
            Debug.LogWarning("No hay item para aceptar.");
            return;
        }

        // Validar que todavía hay espacio en el inventario
        if (inventoryManager == null || !inventoryManager.HasSpace())
        {
            Debug.LogWarning("El inventario está lleno. No se puede añadir el item.");
            return;
        }

        // Añadir el item al inventario
        int slotIndex = inventoryManager.AddItem(currentObtainedItem);
        if (slotIndex >= 0)
        {
            OnItemObtained?.Invoke(currentObtainedItem);
            Debug.Log($"Item añadido al inventario: {currentObtainedItem.GetItemName()} (nivel {currentObtainedItem.currentLevel})");

            // Auto-organizar inventario si está configurado
            if (inventoryAutoOrganizer != null)
            {
                inventoryAutoOrganizer.OrganizeInventory();
            }
        }
        else
        {
            Debug.LogWarning("No se pudo añadir el item al inventario.");
            return;
        }

        // Ocultar panel y limpiar
        HideObtainedItem();

        // Limpiar referencia al item
        currentObtainedItem = null;
    }

    /// <summary>
    /// Oculta el panel de visualización del item.
    /// </summary>
    private void HideObtainedItem()
    {
        // Ocultar panel
        if (itemDisplayPanel != null)
        {
            itemDisplayPanel.SetActive(false);
        }

        // Limpiar sprite
        if (itemDisplayImage != null)
        {
            itemDisplayImage.sprite = null;
            itemDisplayImage.enabled = false;
        }

        // Limpiar nombre del item
        if (itemNameText != null)
        {
            itemNameText.text = "";
            itemNameText.gameObject.SetActive(false);
        }

        // Ocultar botón de aceptar
        if (acceptButton != null)
        {
            acceptButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Obtiene el siguiente item de la lista (aleatorio en lugar de siempre el primero).
    /// SOLUCIÓN: Ahora obtiene un item aleatorio en lugar de siempre el primero.
    /// </summary>
    private ItemInstance GetNextItem()
    {
        if (availableItems == null || availableItems.Count == 0)
        {
            Debug.LogWarning("No hay items disponibles.");
            return null;
        }

        // SOLUCIÓN: Obtener un item aleatorio en lugar de siempre el primero
        int randomIndex = Random.Range(0, availableItems.Count);
        ItemData itemData = availableItems[randomIndex];

        // Incrementar índice para la próxima vez (aunque ahora usamos aleatorio)
        currentItemIndex++;
        if (currentItemIndex >= availableItems.Count)
        {
            currentItemIndex = 0;
        }

        if (itemData == null)
        {
            Debug.LogWarning("ItemData nulo encontrado en la lista de items disponibles.");
            return null;
        }

        // Crear nueva instancia (nivel 1)
        return new ItemInstance(itemData);
    }

    /// <summary>
    /// Muestra la animación del cofre abriéndose (método legacy, mantenido para compatibilidad).
    /// </summary>
    private void ShowChestAnimation(List<ItemInstance> obtainedItems)
    {
        if (chestAnimationPanel != null)
        {
            chestAnimationPanel.SetActive(true);
            Debug.Log("Animación de cofre mostrada.");
        }
    }

    /// <summary>
    /// Obtiene el coste actual de abrir un cofre.
    /// </summary>
    public int GetChestCost()
    {
        return chestCost;
    }

    /// <summary>
    /// Verifica si se puede abrir un cofre (dinero suficiente y espacio en inventario).
    /// </summary>
    public bool CanOpenChest()
    {
        if (playerMoney == null || inventoryManager == null)
            return false;

        // También verificar que no hay un item pendiente de aceptar
        if (currentObtainedItem != null)
            return false;

        return playerMoney.GetMoney() >= chestCost && inventoryManager.HasSpace();
    }

    /// <summary>
    /// Reinicia el índice de items (útil para testing o reiniciar progresión).
    /// </summary>
    public void ResetItemIndex()
    {
        currentItemIndex = 0;
        Debug.Log("Índice de items del cofre reiniciado.");
    }

    private void OnDestroy()
    {
        // Desuscribirse de los botones
        if (openChestButton != null)
        {
            openChestButton.onClick.RemoveListener(OnOpenChestButtonClicked);
        }

        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveListener(OnAcceptButtonClicked);
        }
    }
}
