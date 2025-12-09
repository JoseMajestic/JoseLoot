using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script de debug para trackear cuándo los slots de forja están listos para ser repintados
/// con la imagen del item en lugar de la imagen default.
/// </summary>
public class Control : MonoBehaviour
{
    [Header("Referencias a inspeccionar")]
    public ForgeUIManager forgeUI;
    public InventoryManager inventory;
    public EquipmentManager equipment;

    [Header("Configuración de Debug")]
    [Tooltip("Si está activo, monitorea continuamente el estado de los slots")]
    public bool continuousMonitoring = false;
    
    [Tooltip("Intervalo en segundos para el monitoreo continuo (solo si continuousMonitoring está activo)")]
    public float monitoringInterval = 0.5f;

    private Coroutine monitoringCoroutine;
    private Dictionary<int, SlotReadinessState> slotStates = new Dictionary<int, SlotReadinessState>();

    /// <summary>
    /// Estado de preparación de un slot para ser repintado
    /// </summary>
    public class SlotReadinessState
    {
        public int slotIndex;
        public bool canvasReady;
        public bool imageReady;
        public bool itemValid;
        public bool spriteValid;
        public bool spriteAssigned;
        public bool isReady;
        public string currentSpriteName;
        public string expectedSpriteName;
        public string blockingReason;
        public int frameCount;
    }

    private void OnEnable()
    {
        if (continuousMonitoring)
        {
            StartMonitoring();
        }
        else
        {
            CheckAllSlotsReadiness("OnEnable");
        }
    }

    private void OnDisable()
    {
        StopMonitoring();
    }

    /// <summary>
    /// Inicia el monitoreo continuo de los slots
    /// </summary>
    public void StartMonitoring()
    {
        StopMonitoring();
        monitoringCoroutine = StartCoroutine(MonitoringLoop());
    }

    /// <summary>
    /// Detiene el monitoreo continuo
    /// </summary>
    public void StopMonitoring()
    {
        if (monitoringCoroutine != null)
        {
            StopCoroutine(monitoringCoroutine);
            monitoringCoroutine = null;
        }
    }

    /// <summary>
    /// Loop de monitoreo continuo
    /// </summary>
    private IEnumerator MonitoringLoop()
    {
        while (true)
        {
            CheckAllSlotsReadiness("Continuous");
            yield return new WaitForSeconds(monitoringInterval);
        }
    }

    /// <summary>
    /// Verifica el estado de preparación de todos los slots de forja
    /// </summary>
    public void CheckAllSlotsReadiness(string context = "Manual")
    {
        if (forgeUI == null)
        {
            Debug.LogWarning($"[SLOT_READINESS:{context}] ForgeUIManager no asignado");
            return;
        }

        // Obtener los slots usando reflexión
        var slotsField = typeof(ForgeUIManager).GetField("forgeSlots", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var forgeSlots = slotsField != null ? 
            (InventorySlot[])slotsField.GetValue(forgeUI) : null;

        if (forgeSlots == null || forgeSlots.Length == 0)
        {
            Debug.LogWarning($"[SLOT_READINESS:{context}] No se encontraron slots de forja");
            return;
        }

        // Obtener el Canvas
        Canvas canvas = forgeUI.GetComponentInParent<Canvas>();
        bool canvasReady = canvas != null && canvas.isActiveAndEnabled;

        Debug.Log($"[SLOT_READINESS:{context}] ===== INICIO CHECK =====");
        Debug.Log($"[SLOT_READINESS:{context}] Canvas: {(canvas != null ? canvas.name : "NULL")}, " +
                  $"Active: {canvasReady}, Frame: {Time.frameCount}");

        int readyCount = 0;
        int notReadyCount = 0;

        for (int i = 0; i < forgeSlots.Length; i++)
        {
            if (forgeSlots[i] == null) continue;

            SlotReadinessState state = CheckSlotReadiness(forgeSlots[i], i, canvas, context);
            
            if (state.isReady)
                readyCount++;
            else
                notReadyCount++;

            // Guardar estado para comparación
            slotStates[i] = state;
        }

        Debug.Log($"[SLOT_READINESS:{context}] ===== RESUMEN =====");
        Debug.Log($"[SLOT_READINESS:{context}] Slots listos: {readyCount}, No listos: {notReadyCount}, " +
                  $"Total: {readyCount + notReadyCount}");
        Debug.Log($"[SLOT_READINESS:{context}] ===== FIN CHECK =====");
    }

    /// <summary>
    /// Verifica si un slot específico está listo para ser repintado
    /// </summary>
    private SlotReadinessState CheckSlotReadiness(InventorySlot slot, int index, Canvas canvas, string context)
    {
        SlotReadinessState state = new SlotReadinessState
        {
            slotIndex = index,
            frameCount = Time.frameCount
        };

        // 1. Verificar Canvas
        state.canvasReady = canvas != null && canvas.isActiveAndEnabled;
        if (!state.canvasReady)
        {
            state.blockingReason = "Canvas no activo o no habilitado";
            state.isReady = false;
            LogSlotState(slot, state, context);
            return state;
        }

        // 2. Obtener el Button y Image del slot
        Button slotButton = slot.GetComponent<Button>();
        if (slotButton == null)
        {
            state.blockingReason = "Button no encontrado";
            state.isReady = false;
            LogSlotState(slot, state, context);
            return state;
        }

        Image buttonImage = slotButton.GetComponent<Image>();
        if (buttonImage == null)
        {
            buttonImage = slotButton.GetComponentInChildren<Image>();
        }

        if (buttonImage == null)
        {
            state.blockingReason = "Image no encontrado";
            state.isReady = false;
            LogSlotState(slot, state, context);
            return state;
        }

        // 3. Verificar que el Image esté activo y habilitado
        state.imageReady = buttonImage.gameObject.activeInHierarchy && 
                          buttonImage.enabled && 
                          buttonImage.gameObject.activeSelf;
        
        if (!state.imageReady)
        {
            state.blockingReason = $"Image no listo - activeInHierarchy: {buttonImage.gameObject.activeInHierarchy}, " +
                                   $"enabled: {buttonImage.enabled}, activeSelf: {buttonImage.gameObject.activeSelf}";
            state.isReady = false;
            LogSlotState(slot, state, context);
            return state;
        }

        // 4. Obtener el ItemInstance del slot
        ItemInstance item = slot.GetItem();
        state.itemValid = item != null && item.IsValid();

        if (!state.itemValid)
        {
            // Slot vacío - esto es válido, pero no se puede repintar con sprite de item
            state.blockingReason = "Slot vacío (sin ItemInstance válido)";
            state.isReady = false; // No está "listo" para mostrar sprite de item porque no hay item
            LogSlotState(slot, state, context);
            return state;
        }

        // 5. Verificar que el ItemInstance tenga un sprite válido
        Sprite expectedSprite = item.GetItemSprite();
        state.spriteValid = expectedSprite != null;
        state.expectedSpriteName = expectedSprite != null ? expectedSprite.name : "NULL";

        if (!state.spriteValid)
        {
            state.blockingReason = $"ItemInstance no tiene sprite válido - Item: {item.GetItemName()}";
            state.isReady = false;
            LogSlotState(slot, state, context);
            return state;
        }

        // 6. Verificar si el sprite está asignado correctamente
        Sprite currentSprite = buttonImage.sprite;
        state.currentSpriteName = currentSprite != null ? currentSprite.name : "NULL";
        state.spriteAssigned = currentSprite == expectedSprite;

        // 7. Determinar si está listo
        // Está listo si: Canvas OK, Image OK, Item válido, Sprite válido, y el sprite está asignado correctamente
        state.isReady = state.canvasReady && 
                       state.imageReady && 
                       state.itemValid && 
                       state.spriteValid && 
                       state.spriteAssigned;

        if (!state.isReady && state.spriteValid)
        {
            // Si el sprite es válido pero no está asignado, ese es el problema
            if (!state.spriteAssigned)
            {
                state.blockingReason = $"Sprite no asignado - Esperado: {state.expectedSpriteName}, " +
                                      $"Actual: {state.currentSpriteName}";
            }
        }

        LogSlotState(slot, state, context);
        return state;
    }

    /// <summary>
    /// Registra el estado de un slot en el log
    /// </summary>
    private void LogSlotState(InventorySlot slot, SlotReadinessState state, string context)
    {
        string status = state.isReady ? "✓ LISTO" : "✗ NO LISTO";
        string logPrefix = $"[SLOT_READINESS:{context}] Slot[{state.slotIndex}] {status}";

        if (state.isReady)
        {
            Debug.Log($"{logPrefix} - Sprite: {state.currentSpriteName}, Frame: {state.frameCount}");
        }
        else
        {
            Debug.LogWarning($"{logPrefix} - {state.blockingReason}, Frame: {state.frameCount}");
            
            // Log detallado de cada condición
            Debug.Log($"{logPrefix} - Detalles: Canvas={state.canvasReady}, " +
                     $"Image={state.imageReady}, Item={state.itemValid}, " +
                     $"SpriteValid={state.spriteValid}, SpriteAssigned={state.spriteAssigned}");
            
            if (state.itemValid)
            {
                ItemInstance item = slot.GetItem();
                Debug.Log($"{logPrefix} - Item: {item.GetItemName()}, " +
                         $"ExpectedSprite: {state.expectedSpriteName}, " +
                         $"CurrentSprite: {state.currentSpriteName}");
            }
        }
    }

    /// <summary>
    /// Verifica un slot específico por índice
    /// </summary>
    public void CheckSlotReadiness(int slotIndex)
    {
        if (forgeUI == null)
        {
            Debug.LogWarning($"[SLOT_READINESS] ForgeUIManager no asignado");
            return;
        }

        var slotsField = typeof(ForgeUIManager).GetField("forgeSlots", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var forgeSlots = slotsField != null ? 
            (InventorySlot[])slotsField.GetValue(forgeUI) : null;

        if (forgeSlots == null || slotIndex < 0 || slotIndex >= forgeSlots.Length)
        {
            Debug.LogWarning($"[SLOT_READINESS] Slot índice {slotIndex} inválido");
            return;
        }

        if (forgeSlots[slotIndex] == null)
        {
            Debug.LogWarning($"[SLOT_READINESS] Slot[{slotIndex}] es NULL");
            return;
        }

        Canvas canvas = forgeUI.GetComponentInParent<Canvas>();
        CheckSlotReadiness(forgeSlots[slotIndex], slotIndex, canvas, $"Slot{slotIndex}");
    }

    /// <summary>
    /// Llama esto desde UI para cuando abras el panel de forja
    /// </summary>
    public void OnOpenForgePanel()
    {
        Debug.Log("[SLOT_READINESS:OnOpenForgePanel] Panel de forja abierto - verificando slots");
        CheckAllSlotsReadiness("OpenForgePanel");
    }

    /// <summary>
    /// Obtiene el estado guardado de un slot (útil para comparar cambios)
    /// </summary>
    public SlotReadinessState GetSlotState(int slotIndex)
    {
        return slotStates.ContainsKey(slotIndex) ? slotStates[slotIndex] : null;
    }

    /// <summary>
    /// Log detallado cuando se llama ForceRefreshAllSlotVisuals desde ForgeUIManager
    /// </summary>
    public void LogForceRefreshAllSlotVisuals(string context = "Unknown")
    {
        Debug.Log($"[FORGE_REFRESH:{context}] ===== INICIANDO ForceRefreshAllSlotVisuals =====");
        Debug.Log($"[FORGE_REFRESH:{context}] Frame: {Time.frameCount}, Time: {Time.time}");

        if (forgeUI == null)
        {
            Debug.LogWarning($"[FORGE_REFRESH:{context}] ForgeUIManager es NULL");
            return;
        }

        // Obtener los slots usando reflexión
        var slotsField = typeof(ForgeUIManager).GetField("forgeSlots", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var forgeSlots = slotsField != null ? 
            (InventorySlot[])slotsField.GetValue(forgeUI) : null;

        if (forgeSlots == null)
        {
            Debug.LogWarning($"[FORGE_REFRESH:{context}] forgeSlots es NULL");
            return;
        }

        Debug.Log($"[FORGE_REFRESH:{context}] Total slots: {forgeSlots.Length}");

        // Obtener InventoryManager
        var invField = typeof(ForgeUIManager).GetField("inventoryManager", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        InventoryManager invManager = invField != null ? 
            (InventoryManager)invField.GetValue(forgeUI) : null;

        if (invManager == null)
        {
            Debug.LogWarning($"[FORGE_REFRESH:{context}] InventoryManager es NULL");
            return;
        }

        // Obtener Canvas
        Canvas canvas = forgeUI.GetComponentInParent<Canvas>();
        Debug.Log($"[FORGE_REFRESH:{context}] Canvas: {(canvas != null ? canvas.name : "NULL")}, " +
                  $"Active: {(canvas != null && canvas.isActiveAndEnabled)}");

        int maxSlots = Mathf.Min(forgeSlots.Length, InventoryManager.INVENTORY_SIZE);
        Debug.Log($"[FORGE_REFRESH:{context}] Verificando {maxSlots} slots...");

        for (int i = 0; i < maxSlots; i++)
        {
            if (forgeSlots[i] == null)
            {
                Debug.LogWarning($"[FORGE_REFRESH:{context}] Slot[{i}] es NULL");
                continue;
            }

            LogSlotBeforeRefresh(forgeSlots[i], i, invManager, context);
        }
    }

    /// <summary>
    /// Log detallado del estado de un slot ANTES de llamar a SetItem y ForceUpdateVisualsDirectly
    /// </summary>
    private void LogSlotBeforeRefresh(InventorySlot slot, int index, InventoryManager invManager, string context)
    {
        ItemInstance item = invManager != null ? invManager.GetItem(index) : null;
        
        // Obtener buttonImage del slot
        Button slotButton = slot.GetComponent<Button>();
        Image buttonImage = null;
        if (slotButton != null)
        {
            buttonImage = slotButton.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = slotButton.GetComponentInChildren<Image>();
            }
        }

        Sprite currentSprite = buttonImage != null ? buttonImage.sprite : null;
        Sprite expectedSprite = item != null && item.IsValid() ? item.GetItemSprite() : null;

        Debug.Log($"[FORGE_REFRESH:{context}] Slot[{index}] ANTES:");
        Debug.Log($"  - Item: {(item != null && item.IsValid() ? item.GetItemName() : "NULL")}");
        Debug.Log($"  - Item válido: {item != null && item.IsValid()}");
        Debug.Log($"  - Expected Sprite: {(expectedSprite != null ? expectedSprite.name : "NULL")}");
        Debug.Log($"  - Button: {(slotButton != null ? "OK" : "NULL")}");
        Debug.Log($"  - Image: {(buttonImage != null ? "OK" : "NULL")}");
        if (buttonImage != null)
        {
            Debug.Log($"  - Image Active: {buttonImage.gameObject.activeInHierarchy}, " +
                     $"Enabled: {buttonImage.enabled}, ActiveSelf: {buttonImage.gameObject.activeSelf}");
            Debug.Log($"  - Current Sprite: {(currentSprite != null ? currentSprite.name : "NULL")}");
            Debug.Log($"  - Sprite Match: {currentSprite == expectedSprite}");
        }
        Debug.Log($"  - Slot GameObject Active: {slot.gameObject.activeInHierarchy}");
    }

    /// <summary>
    /// Log detallado del estado de un slot DESPUÉS de llamar a SetItem y ForceUpdateVisualsDirectly
    /// </summary>
    public void LogSlotAfterRefresh(InventorySlot slot, int index, InventoryManager invManager, string context)
    {
        ItemInstance item = invManager != null ? invManager.GetItem(index) : null;
        
        // Obtener buttonImage del slot
        Button slotButton = slot.GetComponent<Button>();
        Image buttonImage = null;
        if (slotButton != null)
        {
            buttonImage = slotButton.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = slotButton.GetComponentInChildren<Image>();
            }
        }

        Sprite currentSprite = buttonImage != null ? buttonImage.sprite : null;
        Sprite expectedSprite = item != null && item.IsValid() ? item.GetItemSprite() : null;

        Debug.Log($"[FORGE_REFRESH:{context}] Slot[{index}] DESPUÉS:");
        Debug.Log($"  - Item: {(item != null && item.IsValid() ? item.GetItemName() : "NULL")}");
        Debug.Log($"  - Expected Sprite: {(expectedSprite != null ? expectedSprite.name : "NULL")}");
        Debug.Log($"  - Current Sprite: {(currentSprite != null ? currentSprite.name : "NULL")}");
        Debug.Log($"  - Sprite Match: {currentSprite == expectedSprite}");
        if (buttonImage != null)
        {
            Debug.Log($"  - Image Active: {buttonImage.gameObject.activeInHierarchy}, " +
                     $"Enabled: {buttonImage.enabled}");
        }

        // Verificar si el sprite se asignó correctamente
        if (item != null && item.IsValid() && expectedSprite != null)
        {
            if (currentSprite != expectedSprite)
            {
                Debug.LogError($"[FORGE_REFRESH:{context}] Slot[{index}] ERROR: Sprite NO se asignó correctamente! " +
                              $"Esperado: {expectedSprite.name}, Actual: {(currentSprite != null ? currentSprite.name : "NULL")}");
            }
            else
            {
                Debug.Log($"[FORGE_REFRESH:{context}] Slot[{index}] ✓ Sprite asignado correctamente");
            }
        }
    }

    /// <summary>
    /// Log cuando se llama SetItem en un slot
    /// </summary>
    public void LogSetItem(InventorySlot slot, int index, ItemInstance item, bool forceUpdate, string context)
    {
        Debug.Log($"[FORGE_SETITEM:{context}] Slot[{index}] SetItem llamado:");
        Debug.Log($"  - Item: {(item != null && item.IsValid() ? item.GetItemName() : "NULL")}");
        Debug.Log($"  - ForceUpdate: {forceUpdate}");
        Debug.Log($"  - Frame: {Time.frameCount}");
    }

    /// <summary>
    /// Log cuando se llama ForceUpdateVisualsDirectly en un slot
    /// </summary>
    public void LogForceUpdateVisuals(InventorySlot slot, int index, string context)
    {
        Debug.Log($"[FORGE_FORCEUPDATE:{context}] Slot[{index}] ForceUpdateVisualsDirectly llamado:");
        Debug.Log($"  - Frame: {Time.frameCount}");
        
        // Obtener buttonImage del slot
        Button slotButton = slot.GetComponent<Button>();
        Image buttonImage = null;
        if (slotButton != null)
        {
            buttonImage = slotButton.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = slotButton.GetComponentInChildren<Image>();
            }
        }

        if (buttonImage == null)
        {
            Debug.LogWarning($"[FORGE_FORCEUPDATE:{context}] Slot[{index}] buttonImage es NULL!");
        }
        else
        {
            Debug.Log($"  - Image Active: {buttonImage.gameObject.activeInHierarchy}, " +
                     $"Enabled: {buttonImage.enabled}");
            Debug.Log($"  - Current Sprite: {(buttonImage.sprite != null ? buttonImage.sprite.name : "NULL")}");
        }
    }

    /// <summary>
    /// Log cuando GetItemSprite devuelve null
    /// </summary>
    public void LogGetItemSpriteNull(ItemInstance item, int slotIndex, string context)
    {
        Debug.LogWarning($"[FORGE_SPRITE:{context}] Slot[{slotIndex}] GetItemSprite() devolvió NULL:");
        Debug.LogWarning($"  - Item: {(item != null ? item.GetItemName() : "NULL")}");
        Debug.LogWarning($"  - Item válido: {item != null && item.IsValid()}");
        if (item != null && item.IsValid())
        {
            Debug.LogWarning($"  - baseItem: {(item.baseItem != null ? item.baseItem.itemName : "NULL")}");
            if (item.baseItem != null)
            {
                Debug.LogWarning($"  - baseItem.itemSprite: {(item.baseItem.itemSprite != null ? item.baseItem.itemSprite.name : "NULL")}");
            }
        }
    }
}
