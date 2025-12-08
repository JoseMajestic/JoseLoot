using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la armería/tienda del juego.
/// Según reglas: Armería: cuadrícula fija, no scrollable. Catálogo: 20 "lonchas" (sets) + consumibles.
/// </summary>
public class ShopService : MonoBehaviour
{
    [Header("Configuración de la Armería")]
    [Tooltip("Catálogo de items disponibles en la tienda (20 sets + consumibles)")]
    [SerializeField] private List<ItemData> shopCatalog = new List<ItemData>();

    [Header("Referencias")]
    [Tooltip("Referencia al InventoryManager para añadir items comprados")]
    [SerializeField] private InventoryManager inventoryManager;

    [Tooltip("Referencia al sistema de dinero del jugador")]
    [SerializeField] private PlayerMoney playerMoney; // TODO: Crear o conectar con el sistema de dinero

    // Eventos
    public System.Action<ItemData> OnItemPurchased;
    public System.Action<ItemData> OnItemSold;
    public System.Action OnShopCatalogChanged;

    /// <summary>
    /// Inicializa el catálogo de la tienda.
    /// </summary>
    public void InitializeShop(List<ItemData> catalog)
    {
        if (catalog == null)
        {
            Debug.LogWarning("Catálogo de tienda nulo. Inicializando lista vacía.");
            shopCatalog = new List<ItemData>();
        }
        else
        {
            shopCatalog = new List<ItemData>(catalog);
        }
        OnShopCatalogChanged?.Invoke();
    }

    /// <summary>
    /// Obtiene el catálogo completo de la tienda.
    /// </summary>
    public List<ItemData> GetShopCatalog()
    {
        return new List<ItemData>(shopCatalog);
    }

    /// <summary>
    /// Obtiene un item del catálogo por índice.
    /// </summary>
    public ItemData GetShopItem(int index)
    {
        if (index < 0 || index >= shopCatalog.Count)
            return null;
        return shopCatalog[index];
    }

    /// <summary>
    /// Compra un item de la tienda.
    /// </summary>
    /// <returns>True si la compra fue exitosa, false si falló (dinero insuficiente, inventario lleno, etc.)</returns>
    public bool PurchaseItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("Intentando comprar un item nulo.");
            return false;
        }

        if (!shopCatalog.Contains(item))
        {
            Debug.LogWarning($"El item '{item.itemName}' no está disponible en la tienda.");
            return false;
        }

        // Validar dinero suficiente
        if (playerMoney == null)
        {
            Debug.LogError("PlayerMoney no está asignado. No se puede validar el dinero.");
            return false;
        }

        if (playerMoney.GetMoney() < item.price)
        {
            Debug.LogWarning($"Dinero insuficiente. Necesitas {item.price}, tienes {playerMoney.GetMoney()}.");
            return false;
        }

        // Validar espacio en inventario
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager no está asignado. No se puede añadir el item al inventario.");
            return false;
        }

        if (!inventoryManager.HasSpace())
        {
            Debug.LogWarning("El inventario está lleno. No se puede comprar el item.");
            return false;
        }

        // Realizar la compra
        playerMoney.SubtractMoney(item.price);
        int slotIndex = inventoryManager.AddItem(item);

        if (slotIndex >= 0)
        {
            OnItemPurchased?.Invoke(item);
            Debug.Log($"Item '{item.itemName}' comprado exitosamente y añadido al inventario en el slot {slotIndex}.");
            return true;
        }
        else
        {
            // Reembolsar si falló al añadir al inventario
            playerMoney.AddMoney(item.price);
            Debug.LogError($"Error al añadir el item '{item.itemName}' al inventario. Compra cancelada y dinero reembolsado.");
            return false;
        }
    }

    /// <summary>
    /// Vende un item del inventario a la tienda (usando ItemInstance).
    /// </summary>
    /// <param name="inventorySlotIndex">Índice del slot en el inventario</param>
    /// <returns>True si la venta fue exitosa, false si falló.</returns>
    public bool SellItem(int inventorySlotIndex)
    {
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager no está asignado. No se puede vender el item.");
            return false;
        }

        // Obtener el ItemInstance del inventario
        ItemInstance itemInstance = inventoryManager.GetItem(inventorySlotIndex);
        if (itemInstance == null || !itemInstance.IsValid())
        {
            Debug.LogWarning($"No hay item válido en el slot {inventorySlotIndex} para vender.");
            return false;
        }

        // Calcular precio de venta basado en el ItemInstance
        int sellPrice = CalculateSellPrice(itemInstance);

        // Remover del inventario
        if (inventoryManager.RemoveItem(inventorySlotIndex))
        {
            // Añadir dinero
            if (playerMoney != null)
            {
                playerMoney.AddMoney(sellPrice);
            }

            OnItemSold?.Invoke(itemInstance.baseItem);
            Debug.Log($"Item '{itemInstance.GetItemName()}' (nivel {itemInstance.currentLevel}) vendido por {sellPrice}.");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Vende un ItemInstance específico del inventario (método alternativo).
    /// </summary>
    /// <param name="itemInstance">ItemInstance a vender</param>
    /// <param name="inventorySlotIndex">Índice del slot en el inventario</param>
    /// <returns>True si la venta fue exitosa, false si falló.</returns>
    public bool SellItem(ItemInstance itemInstance, int inventorySlotIndex)
    {
        if (itemInstance == null || !itemInstance.IsValid())
        {
            Debug.LogWarning("Intentando vender un ItemInstance nulo o inválido.");
            return false;
        }

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager no está asignado. No se puede vender el item.");
            return false;
        }

        // Verificar que el item esté en el inventario en el slot especificado
        ItemInstance itemInSlot = inventoryManager.GetItem(inventorySlotIndex);
        if (itemInSlot == null || !itemInSlot.IsValid() || itemInSlot.baseItem != itemInstance.baseItem)
        {
            Debug.LogWarning($"El item en el slot {inventorySlotIndex} no coincide con el item a vender.");
            return false;
        }

        // Calcular precio de venta
        int sellPrice = CalculateSellPrice(itemInstance);

        // Remover del inventario
        if (inventoryManager.RemoveItem(inventorySlotIndex))
        {
            // Añadir dinero
            if (playerMoney != null)
            {
                playerMoney.AddMoney(sellPrice);
            }

            OnItemSold?.Invoke(itemInstance.baseItem);
            Debug.Log($"Item '{itemInstance.GetItemName()}' (nivel {itemInstance.currentLevel}) vendido por {sellPrice}.");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calcula el precio de venta de un ItemInstance.
    /// Considera el nivel del item para calcular el precio (items de mayor nivel valen más).
    /// </summary>
    public int CalculateSellPrice(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
            return 0;

        ItemData baseItem = itemInstance.baseItem;
        if (baseItem == null)
            return 0;

        // Precio base: 50% del precio de compra del ItemData base
        int baseSellPrice = Mathf.Max(1, baseItem.price / 2);

        // Bonificación por nivel: +5% por cada nivel por encima del nivel 1
        // Ejemplo: nivel 10 = +45% (9 niveles * 5%)
        float levelMultiplier = 1.0f + ((itemInstance.currentLevel - 1) * 0.05f);

        int finalPrice = Mathf.RoundToInt(baseSellPrice * levelMultiplier);
        return Mathf.Max(1, finalPrice);
    }

    /// <summary>
    /// Calcula el precio de venta de un ItemData (método legacy para compatibilidad).
    /// </summary>
    [System.Obsolete("Usar CalculateSellPrice(ItemInstance) en su lugar. Este método es para compatibilidad.")]
    private int CalculateSellPrice(ItemData item)
    {
        // TODO(KIRBY-APPROVAL): Confirmar fórmula exacta de precio de venta
        // Por ahora: 50% del precio de compra
        return Mathf.Max(1, item.price / 2);
    }

    /// <summary>
    /// Filtra el catálogo por tipo de item.
    /// </summary>
    public List<ItemData> GetItemsByType(ItemType type)
    {
        List<ItemData> filtered = new List<ItemData>();
        foreach (var item in shopCatalog)
        {
            if (item != null && item.itemType == type)
            {
                filtered.Add(item);
            }
        }
        return filtered;
    }

    /// <summary>
    /// Filtra el catálogo por rareza.
    /// </summary>
    public List<ItemData> GetItemsByRarity(string rarity)
    {
        List<ItemData> filtered = new List<ItemData>();
        foreach (var item in shopCatalog)
        {
            if (item != null && item.rareza == rarity)
            {
                filtered.Add(item);
            }
        }
        return filtered;
    }

    /// <summary>
    /// Filtra el catálogo por nivel máximo requerido.
    /// </summary>
    public List<ItemData> GetItemsByMaxLevel(int maxLevel)
    {
        List<ItemData> filtered = new List<ItemData>();
        foreach (var item in shopCatalog)
        {
            if (item != null && item.nivel <= maxLevel)
            {
                filtered.Add(item);
            }
        }
        return filtered;
    }
}

