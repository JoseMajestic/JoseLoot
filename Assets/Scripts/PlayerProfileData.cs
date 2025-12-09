using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Datos del perfil del jugador que se guardan persistentemente.
/// Incluye los items equipados con su GUID y nivel.
/// </summary>
[System.Serializable]
public class PlayerProfileData
{
    [System.Serializable]
    public class EquippedItemData
    {
        public string slotType; // "Arma", "ArmaSecundaria", etc.
        public string itemInstanceId; // GUID del ItemInstance
        public int level; // Nivel actual del item
        public string itemName; // Nombre del ItemData para referencia
    }

    public List<EquippedItemData> equippedItems = new List<EquippedItemData>();

    // SOLUCIÓN: Datos del inventario completo (serializado)
    // Array de strings, cada string es "nombreItemData|nivel|id|version" o null si el slot está vacío
    public string[] inventoryData = new string[InventoryManager.INVENTORY_SIZE];

    /// <summary>
    /// Guarda el estado actual del equipo en el perfil.
    /// </summary>
    public void SaveEquipmentState(EquipmentManager equipmentManager)
    {
        if (equipmentManager == null)
            return;

        equippedItems.Clear();

        // Guardar cada slot de equipo
        foreach (EquipmentManager.EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
        {
            ItemInstance item = equipmentManager.GetEquippedItem(slotType);
            if (item != null && item.IsValid())
            {
                EquippedItemData data = new EquippedItemData
                {
                    slotType = slotType.ToString(),
                    itemInstanceId = item.GetInstanceId(),
                    level = item.currentLevel,
                    itemName = item.GetItemName()
                };
                equippedItems.Add(data);
            }
        }
    }

    /// <summary>
    /// Obtiene el nivel guardado de un item equipado por su GUID.
    /// </summary>
    public int GetEquippedItemLevel(string instanceId)
    {
        foreach (var itemData in equippedItems)
        {
            if (itemData.itemInstanceId == instanceId)
            {
                return itemData.level;
            }
        }
        return -1; // No encontrado
    }

    /// <summary>
    /// Carga el estado del equipo desde el perfil.
    /// Busca los items en el inventario por su GUID y los equipa en los slots correspondientes.
    /// </summary>
    /// <param name="equipmentManager">El EquipmentManager a actualizar</param>
    /// <param name="inventoryManager">El InventoryManager para buscar items por GUID</param>
    /// <param name="itemDatabase">El ItemDatabase para deserializar items si es necesario</param>
    /// <param name="silent">Si es true, no dispara eventos de inventario (útil para carga inicial)</param>
    public void LoadEquipmentState(EquipmentManager equipmentManager, InventoryManager inventoryManager, ItemDatabase itemDatabase, bool silent = false)
    {
        if (equipmentManager == null || inventoryManager == null || itemDatabase == null)
            return;

        if (equippedItems == null || equippedItems.Count == 0)
            return;

        // Para cada item equipado guardado, buscarlo en el inventario y equiparlo
        foreach (var equippedItemData in equippedItems)
        {
            // Parsear el tipo de slot
            if (!System.Enum.TryParse<EquipmentManager.EquipmentSlotType>(equippedItemData.slotType, out EquipmentManager.EquipmentSlotType slotType))
            {
                Debug.LogWarning($"No se pudo parsear el tipo de slot: {equippedItemData.slotType}");
                continue;
            }

            // Buscar el item en el inventario por su GUID
            ItemInstance itemToEquip = null;
            for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
            {
                ItemInstance item = inventoryManager.GetItem(i);
                if (item != null && item.IsValid() && item.GetInstanceId() == equippedItemData.itemInstanceId)
                {
                    // Encontrado el item en el inventario
                    // Asegurar que el nivel esté actualizado
                    if (item.currentLevel != equippedItemData.level)
                    {
                        item.SetLevel(equippedItemData.level);
                    }
                    itemToEquip = item;
                    break;
                }
            }

            // Si no se encontró en el inventario, intentar deserializarlo desde los datos guardados
            // (esto puede pasar si el item fue eliminado del inventario pero estaba equipado)
            if (itemToEquip == null)
            {
                // Buscar en el inventario por nombre y nivel (fallback)
                // Esto es menos seguro pero puede ayudar en casos de migración de datos
                Debug.LogWarning($"Item equipado con GUID {equippedItemData.itemInstanceId} no encontrado en inventario. Intentando buscar por nombre...");
                
                for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
                {
                    ItemInstance item = inventoryManager.GetItem(i);
                    if (item != null && item.IsValid() && 
                        item.GetItemName() == equippedItemData.itemName && 
                        item.currentLevel == equippedItemData.level)
                    {
                        itemToEquip = item;
                        // Actualizar el GUID del item para que coincida con el guardado
                        // (esto puede pasar si el item fue deserializado sin el GUID correcto)
                        break;
                    }
                }
            }

            // Si encontramos el item, equiparlo
            if (itemToEquip != null && itemToEquip.IsValid())
            {
                // SOLUCIÓN CRÍTICA: NO remover el item del inventario cuando se carga la partida
                // Los items equipados deben permanecer en el inventario con el panel de "equipado" visible
                // Esto permite que el grid de forja muestre todos los items (equipados y no equipados)
                // El item estará tanto en el inventario como equipado lógicamente
                
                // SOLUCIÓN: Equipar el item sin removerlo del inventario y sin disparar eventos si estamos en modo silencioso
                // Usar SetEquippedItem directamente en lugar de EquipItem para evitar eventos
                if (silent)
                {
                    // Equipar directamente sin disparar eventos
                    // NO remover del inventario - el item debe permanecer visible
                    ItemInstance previousItem = equipmentManager.GetEquippedItem(slotType);
                    equipmentManager.SetEquippedItemDirectly(slotType, itemToEquip);
                }
                else
                {
                    // Equipar normalmente (dispara eventos)
                    // En modo normal, EquipItem() tampoco remueve el item del inventario
                    // (según el código actual, el item se mantiene en el inventario)
                    equipmentManager.EquipItem(itemToEquip, slotType);
                }
            }
            else
            {
                Debug.LogWarning($"No se pudo encontrar el item equipado '{equippedItemData.itemName}' (GUID: {equippedItemData.itemInstanceId}) en el inventario. El slot {equippedItemData.slotType} quedará vacío.");
            }
        }
    }

    /// <summary>
    /// Guarda el estado actual del inventario en el perfil.
    /// </summary>
    public void SaveInventoryState(InventoryManager inventoryManager)
    {
        if (inventoryManager == null)
            return;

        // SOLUCIÓN: Serializar el inventario completo usando el método existente
        string[] serialized = inventoryManager.SerializeInventory();
        inventoryData = serialized;
    }

    /// <summary>
    /// Carga el estado del inventario desde el perfil.
    /// </summary>
    /// <param name="inventoryManager">El InventoryManager a actualizar</param>
    /// <param name="silent">Si es true, no dispara eventos (útil para carga inicial)</param>
    public void LoadInventoryState(InventoryManager inventoryManager, bool silent = false)
    {
        if (inventoryManager == null || inventoryData == null)
            return;

        // SOLUCIÓN: Deserializar el inventario completo usando el método existente
        // Solo cargar si hay datos guardados (no es un perfil nuevo)
        if (inventoryData.Length == InventoryManager.INVENTORY_SIZE)
        {
            inventoryManager.DeserializeInventory(inventoryData, silent: silent);
        }
    }
}

