using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI de Forja/Mejora. Muestra stats actuales y proyectadas, coste y permite mejorar.
/// Reutiliza InventorySlot para los botones del grid de forja.
/// </summary>
public class ForgeUIManager : MonoBehaviour
{
    [Header("Referencias lógicas")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PlayerMoney playerMoney;
    [SerializeField] private ItemImprovementSystem improvementSystem;
    [SerializeField] private EquipmentManager equipmentManager;

    [Header("Grid de Forja")]
    [Tooltip("Botones del grid de forja. Se les añadirá/usar InventorySlot.")]
    [SerializeField] private Button[] forgeSlotButtons;
    [Tooltip("Textos de nivel para cada slot (opcional, paralelos a los botones).")]
    [SerializeField] private TextMeshProUGUI[] forgeLevelTexts;
    [Tooltip("Paneles de 'equipado' paralelos al grid de forja.")]
    [SerializeField] private GameObject[] forgePanelsEquipped;

    [Header("Visor")]
    [SerializeField] private GameObject detailViewPanel;
    [SerializeField] private Image detailViewImage;
    [SerializeField] private TextMeshProUGUI itemTitleTextA;
    [SerializeField] private TextMeshProUGUI itemTitleTextB;

    [Header("Stats actuales")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI ataqueText;
    [SerializeField] private TextMeshProUGUI defensaText;
    [SerializeField] private TextMeshProUGUI velocidadAtaqueText;
    [SerializeField] private TextMeshProUGUI ataqueCriticoText;
    [SerializeField] private TextMeshProUGUI danoCriticoText;
    [SerializeField] private TextMeshProUGUI suerteText;
    [SerializeField] private TextMeshProUGUI destrezaText;
    [SerializeField] private TextMeshProUGUI rarezaText;

    [Header("Stats siguientes")]
    [SerializeField] private TextMeshProUGUI nextHpText;
    [SerializeField] private TextMeshProUGUI nextManaText;
    [SerializeField] private TextMeshProUGUI nextAtaqueText;
    [SerializeField] private TextMeshProUGUI nextDefensaText;
    [SerializeField] private TextMeshProUGUI nextVelocidadAtaqueText;
    [SerializeField] private TextMeshProUGUI nextAtaqueCriticoText;
    [SerializeField] private TextMeshProUGUI nextDanoCriticoText;
    [SerializeField] private TextMeshProUGUI nextSuerteText;
    [SerializeField] private TextMeshProUGUI nextDestrezaText;

    [Header("Stats delta (+)")]
    [SerializeField] private TextMeshProUGUI deltaHpText;
    [SerializeField] private TextMeshProUGUI deltaManaText;
    [SerializeField] private TextMeshProUGUI deltaAtaqueText;
    [SerializeField] private TextMeshProUGUI deltaDefensaText;
    [SerializeField] private TextMeshProUGUI deltaVelocidadAtaqueText;
    [SerializeField] private TextMeshProUGUI deltaAtaqueCriticoText;
    [SerializeField] private TextMeshProUGUI deltaDanoCriticoText;
    [SerializeField] private TextMeshProUGUI deltaSuerteText;
    [SerializeField] private TextMeshProUGUI deltaDestrezaText;

    [Header("Panel auxiliar de flechas/visual (activado al seleccionar item)")]
    [SerializeField] private GameObject arrowsPanel;

    [Header("Niveles (duplicados)")]
    [SerializeField] private TextMeshProUGUI nivelActualTextA;
    [SerializeField] private TextMeshProUGUI nivelActualTextB;
    [SerializeField] private TextMeshProUGUI nivelSiguienteTextA;
    [SerializeField] private TextMeshProUGUI nivelSiguienteTextB;

    [Header("Coste de mejora")]
    [SerializeField] private TextMeshProUGUI costUpgradeText;

    [Header("Acción")]
    [SerializeField] private Button upgradeButton;

    private InventorySlot[] forgeSlots;
    private ItemInstance selectedItem;
    private bool initialized;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        EnsureInitialized();
        
        // SOLUCIÓN CRÍTICA: Sincronizar niveles desde el perfil guardado ANTES de refrescar
        // Esto asegura que el inventario esté completamente cargado y sincronizado antes de llenar el grid
        // Igual que hace InventoryUIManager para garantizar que los datos estén disponibles
        if (GameDataManager.Instance != null && inventoryManager != null)
        {
            // Sincronizar niveles desde PlayerPrefs sin recargar todo el inventario
            // Esto recarga el perfil desde PlayerPrefs y asegura que el inventario esté actualizado
            GameDataManager.Instance.SyncInventoryLevelsFromProfile();
        }
   
        
        // SOLUCIÓN ARQUITECTÓNICA: Esperar a que el Canvas y los Image estén completamente listos antes de refrescar
        // PollAndUpdateForgeSlots() se llamará al final de RefreshForgeGridWhenReady() después de que los slots estén inicializados
        // AHORA el inventario ya está sincronizado, así que RefreshForgeGrid() encontrará los items
        StartCoroutine(RefreshForgeGridWhenReady());
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);

        // Desuscribirse de eventos del inventario
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded -= OnInventoryItemAdded;
            inventoryManager.OnInventoryChanged -= RefreshForgeGrid;
        }

        // Desuscribirse de eventos del equipo
        if (equipmentManager != null)
        {
            equipmentManager.OnItemEquipped -= OnItemEquipped;
            equipmentManager.OnItemUnequipped -= OnItemUnequipped;
            equipmentManager.OnEquipmentChanged -= RefreshForgeGrid;
        }

        if (forgeSlots != null)
        {
            foreach (var slot in forgeSlots)
            {
                if (slot != null)
                    slot.OnSlotClicked -= OnForgeSlotClicked;
            }
        }
    }

    private void EnsureInitialized()
    {
        if (initialized)
            return;

        InitializeSlots();

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

        // Suscribirse a eventos del inventario para refrescar automáticamente
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded += OnInventoryItemAdded;
            inventoryManager.OnInventoryChanged += RefreshForgeGrid;
        }

        // Suscribirse a eventos del equipo para refrescar paneles de equipado
        if (equipmentManager != null)
        {
            equipmentManager.OnItemEquipped += OnItemEquipped;
            equipmentManager.OnItemUnequipped += OnItemUnequipped;
            equipmentManager.OnEquipmentChanged += RefreshForgeGrid;
        }

        initialized = true;
    }

    private void InitializeSlots()
    {
        if (forgeSlotButtons == null)
            forgeSlotButtons = new Button[0];

        forgeSlots = new InventorySlot[forgeSlotButtons.Length];
        for (int i = 0; i < forgeSlotButtons.Length; i++)
        {
            var btn = forgeSlotButtons[i];
            if (btn == null) continue;

            var slot = btn.GetComponent<InventorySlot>();
            if (slot == null)
                slot = btn.gameObject.AddComponent<InventorySlot>();

            if (forgeLevelTexts != null && i < forgeLevelTexts.Length && forgeLevelTexts[i] != null)
                slot.SetLevelText(forgeLevelTexts[i]);

            slot.OnSlotClicked += OnForgeSlotClicked;
            slot.Initialize(i, inventoryManager);
            forgeSlots[i] = slot;
        }
    }

    private void OnForgeSlotClicked(InventorySlot slot)
    {
        selectedItem = slot != null ? slot.GetItem() : null;
        UpdateDetailView(selectedItem);
    }

    private void OnInventoryItemAdded(int slotIndex, ItemInstance itemInstance)
    {
        // Refrescar el grid cuando se añade un item
        RefreshForgeGrid();
    }

    private void OnItemEquipped(EquipmentManager.EquipmentSlotType slot, ItemInstance itemInstance)
    {
        // Refrescar el grid cuando se equipa un item
        RefreshForgeGrid();
    }

    private void OnItemUnequipped(EquipmentManager.EquipmentSlotType slot, ItemInstance itemInstance)
    {
        // Refrescar el grid cuando se desequipa un item
        RefreshForgeGrid();
    }

    private IEnumerator RefreshAfterFrame()
    {
        yield return null;
        RefreshForgeGrid();
    }

    /// <summary>
    /// Refresca el grid de forja cuando los componentes están listos.
    /// Espera a que el Canvas y todos los Image estén completamente activos y habilitados antes de asignar sprites.
    /// SIEMPRE fuerza la actualización completa de todos los slots al abrir el panel.
    /// </summary>
    private IEnumerator RefreshForgeGridWhenReady()
    {
        Debug.Log("[FORGE_REFRESH] RefreshForgeGridWhenReady iniciado");
        
        if (inventoryManager == null || forgeSlots == null)
        {
            Debug.LogWarning("[FORGE_REFRESH] inventoryManager o forgeSlots es NULL al inicio");
            yield break;
        }

        // Esperar a que el Canvas esté completamente activo y listo
        Canvas canvas = GetComponentInParent<Canvas>();
        while (canvas == null || !canvas.isActiveAndEnabled)
        {
            yield return null;
            canvas = GetComponentInParent<Canvas>();
        }
        
        // Esperar a que todos los componentes Image estén activos y listos
        bool allImagesReady = false;
        int maxWaitFrames = 10; // Límite de seguridad para evitar bucles infinitos
        int waitFrames = 0;
        
        while (!allImagesReady && waitFrames < maxWaitFrames)
        {
            allImagesReady = true;
            
            // Verificar que todos los slots tengan sus Image components listos
            for (int i = 0; i < forgeSlots.Length; i++)
            {
                if (forgeSlots[i] != null)
                {
                    // Obtener el Image del slot
                    Button slotButton = forgeSlots[i].GetComponent<Button>();
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
        yield return null;
        yield return null;
        yield return null; // Frame adicional
        yield return new WaitForEndOfFrame(); // Esperar hasta el final del frame
        
        // SOLUCIÓN: SIEMPRE forzar actualización completa de todos los slots al abrir el panel
        // Esto asegura que todos los niveles y sprites se actualicen correctamente
        // El mini parpadeo es aceptable ya que los items se mejoran de uno en uno
        Debug.Log("[FORGE_REFRESH] Llamando RefreshForgeGrid()...");
        RefreshForgeGrid();
        Debug.Log("[FORGE_REFRESH] RefreshForgeGrid() completado");
        
        // SOLUCIÓN ARQUITECTÓNICA: Polling después de que los slots estén completamente inicializados
        // Esto asegura que se detecten y corrijan discrepancias de nivel y se inicialicen slots vacíos
        yield return null; // Esperar un frame adicional para asegurar que RefreshForgeGrid() completó
        Debug.Log("[FORGE_REFRESH] Llamando PollAndUpdateForgeSlots()...");
        PollAndUpdateForgeSlots();
        Debug.Log("[FORGE_REFRESH] PollAndUpdateForgeSlots() completado");
        
        // SOLUCIÓN: Forzar actualización visual de todos los slots (igual que hace el inventario)
        // Esto asegura que los sprites se repinten correctamente después de reanudar la partida
        yield return null; // Frame adicional para asegurar que PollAndUpdateForgeSlots() completó
        yield return new WaitForEndOfFrame(); // Esperar hasta el final del frame
        Debug.Log("[FORGE_REFRESH] Llamando ForceRefreshAllSlotVisuals()...");
        ForceRefreshAllSlotVisuals();
        Debug.Log("[FORGE_REFRESH] RefreshForgeGridWhenReady completado");
    }

    /// <summary>
    /// SOLUCIÓN ARQUITECTÓNICA: Polling para detectar y corregir discrepancias de nivel en slots de forja.
    /// Compara el nivel actual del ItemInstance en memoria con el nivel mostrado en el slot.
    /// Si hay diferencia, fuerza actualización. También inicializa slots vacíos.
    /// Se llama al final de RefreshForgeGridWhenReady() después de que los slots estén inicializados.
    /// </summary>
    private void PollAndUpdateForgeSlots()
    {
        if (forgeSlots == null || inventoryManager == null)
            return;

        // Alineación 1:1 por índice con InventoryManager para evitar depender de listas vacías
        int maxSlots = Mathf.Min(forgeSlots.Length, InventoryManager.INVENTORY_SIZE);
        for (int i = 0; i < maxSlots; i++)
        {
            if (forgeSlots[i] == null)
                continue;

            ItemInstance slotItem = forgeSlots[i].GetItem();
            ItemInstance inventoryItem = inventoryManager.GetItem(i);

            bool slotHas = slotItem != null && slotItem.IsValid();
            bool invHas = inventoryItem != null && inventoryItem.IsValid();

            bool needsUpdate = false;
            if (slotHas != invHas)
            {
                needsUpdate = true;
            }
            else if (slotHas && invHas)
            {
                // Misma instancia y nivel, de lo contrario forzar
                if (!slotItem.IsSameInstance(inventoryItem) || slotItem.currentLevel != inventoryItem.currentLevel)
                {
                    needsUpdate = true;
                }
            }

            if (needsUpdate)
            {
                forgeSlots[i].SetItem(inventoryItem, forceUpdate: true);
            }
        }
    }

    private void RefreshForgeGrid()
    {
        if (inventoryManager == null || forgeSlots == null)
            return;

        // Alineación 1:1 con el inventario (mismo índice), igual que InventoryUIManager
        int maxSlots = Mathf.Min(forgeSlots.Length, InventoryManager.INVENTORY_SIZE);
        for (int i = 0; i < maxSlots; i++)
        {
            ItemInstance item = inventoryManager.GetItem(i);

            if (forgeSlots[i] != null)
            {
                // Siempre forceUpdate para que cargue sprites al abrir y tras cargar partida
                forgeSlots[i].SetItem(item, forceUpdate: true);
            }

            // Panel de equipado paralelo (mismo índice)
            if (forgePanelsEquipped != null && i < forgePanelsEquipped.Length && forgePanelsEquipped[i] != null)
            {
                bool isEquipped = false;
                if (equipmentManager != null && item != null && item.IsValid())
                {
                    for (int s = 0; s < 6; s++)
                    {
                        var eq = equipmentManager.GetEquippedItem((EquipmentManager.EquipmentSlotType)s);
                        if (eq != null && eq.IsValid() && eq.IsSameInstance(item))
                        {
                            isEquipped = true;
                            break;
                        }
                    }
                }
                forgePanelsEquipped[i].SetActive(isEquipped);
            }
        }

        // Si hay más slots de forja que tamaño de inventario, limpiar los sobrantes
        for (int i = maxSlots; i < forgeSlots.Length; i++)
        {
            if (forgeSlots[i] != null)
            {
                forgeSlots[i].SetItem(null, forceUpdate: true);
            }
            if (forgePanelsEquipped != null && i < forgePanelsEquipped.Length && forgePanelsEquipped[i] != null)
            {
                forgePanelsEquipped[i].SetActive(false);
            }
        }
        
        // SOLUCIÓN ARQUITECTÓNICA: Forzar actualización final del canvas para asegurar que los sprites se rendericen
        // Esto es crítico cuando se carga la partida guardada
        Canvas.ForceUpdateCanvases();
    }

    private void OnUpgradeClicked()
    {
        if (selectedItem == null || !selectedItem.IsValid())
            return;

        if (improvementSystem == null)
        {
            Debug.LogError("ForgeUIManager: ItemImprovementSystem no asignado.");
            return;
        }

        // Intentar mejorar el item seleccionado
        bool success = improvementSystem.ImproveItem(selectedItem);
        if (success)
        {
            // Refrescar la UI tras la mejora
            UpdateDetailView(selectedItem);
            RefreshForgeGrid();
        }
    }

    private void UpdateDetailView(ItemInstance item)
    {
        if (item == null || !item.IsValid())
        {
            ClearDetailView();
            return;
        }

        // Imagen
        if (detailViewImage != null)
        {
            var sprite = item.GetItemSprite();
            detailViewImage.sprite = sprite;
            detailViewImage.enabled = sprite != null;
        }
        if (detailViewPanel != null) detailViewPanel.SetActive(true);
        if (arrowsPanel != null) arrowsPanel.SetActive(true);

        var baseItem = item.baseItem;
        var stats = item.GetFinalStats();
        var nextStats = improvementSystem != null
            ? improvementSystem.GetProjectedStats(item)
            : item.GetStatsAtLevel(Mathf.Min(item.currentLevel + 1, 999));

        // Título y rareza (duplicado para mejor visibilidad)
        Color titleColor = baseItem != null ? GetRarityColor(baseItem.rareza) : Color.white;
        string titleText = baseItem != null ? baseItem.itemName : item.GetItemName();
        
        if (itemTitleTextA != null)
        {
            itemTitleTextA.text = titleText;
            itemTitleTextA.color = titleColor;
        }
        if (itemTitleTextB != null)
        {
            itemTitleTextB.text = titleText;
            itemTitleTextB.color = titleColor;
        }
        if (rarezaText != null)
        {
            if (baseItem != null)
            {
                rarezaText.text = baseItem.rareza;
                rarezaText.color = GetRarityColor(baseItem.rareza);
            }
            else
            {
                rarezaText.text = "";
                rarezaText.color = Color.white;
            }
        }

        // Stats actuales
        if (hpText != null) hpText.text = stats.hp.ToString();
        if (manaText != null) manaText.text = stats.mana.ToString();
        if (ataqueText != null) ataqueText.text = stats.ataque.ToString();
        if (defensaText != null) defensaText.text = stats.defensa.ToString();
        if (velocidadAtaqueText != null) velocidadAtaqueText.text = stats.velocidadAtaque.ToString();
        if (ataqueCriticoText != null) ataqueCriticoText.text = stats.ataqueCritico.ToString();
        if (danoCriticoText != null) danoCriticoText.text = stats.danoCritico.ToString();
        if (suerteText != null) suerteText.text = stats.suerte.ToString();
        if (destrezaText != null) destrezaText.text = stats.destreza.ToString();

        // Stats siguientes
        if (nextHpText != null) nextHpText.text = nextStats.hp.ToString();
        if (nextManaText != null) nextManaText.text = nextStats.mana.ToString();
        if (nextAtaqueText != null) nextAtaqueText.text = nextStats.ataque.ToString();
        if (nextDefensaText != null) nextDefensaText.text = nextStats.defensa.ToString();
        if (nextVelocidadAtaqueText != null) nextVelocidadAtaqueText.text = nextStats.velocidadAtaque.ToString();
        if (nextAtaqueCriticoText != null) nextAtaqueCriticoText.text = nextStats.ataqueCritico.ToString();
        if (nextDanoCriticoText != null) nextDanoCriticoText.text = nextStats.danoCritico.ToString();
        if (nextSuerteText != null) nextSuerteText.text = nextStats.suerte.ToString();
        if (nextDestrezaText != null) nextDestrezaText.text = nextStats.destreza.ToString();

        // Deltas
        int dHp = nextStats.hp - stats.hp;
        int dMana = nextStats.mana - stats.mana;
        int dAtk = nextStats.ataque - stats.ataque;
        int dDef = nextStats.defensa - stats.defensa;
        int dVel = nextStats.velocidadAtaque - stats.velocidadAtaque;
        int dAtkC = nextStats.ataqueCritico - stats.ataqueCritico;
        int dDmgC = nextStats.danoCritico - stats.danoCritico;
        int dSuerte = nextStats.suerte - stats.suerte;
        int dDest = nextStats.destreza - stats.destreza;

        // Asignar texto y color según el delta
        if (deltaHpText != null)
        {
            deltaHpText.text = FormatDelta(dHp);
            deltaHpText.color = dHp > 0 ? Color.green : Color.white;
        }
        if (deltaManaText != null)
        {
            deltaManaText.text = FormatDelta(dMana);
            deltaManaText.color = dMana > 0 ? Color.green : Color.white;
        }
        if (deltaAtaqueText != null)
        {
            deltaAtaqueText.text = FormatDelta(dAtk);
            deltaAtaqueText.color = dAtk > 0 ? Color.green : Color.white;
        }
        if (deltaDefensaText != null)
        {
            deltaDefensaText.text = FormatDelta(dDef);
            deltaDefensaText.color = dDef > 0 ? Color.green : Color.white;
        }
        if (deltaVelocidadAtaqueText != null)
        {
            deltaVelocidadAtaqueText.text = FormatDelta(dVel);
            deltaVelocidadAtaqueText.color = dVel > 0 ? Color.green : Color.white;
        }
        if (deltaAtaqueCriticoText != null)
        {
            deltaAtaqueCriticoText.text = FormatDelta(dAtkC);
            deltaAtaqueCriticoText.color = dAtkC > 0 ? Color.green : Color.white;
        }
        if (deltaDanoCriticoText != null)
        {
            deltaDanoCriticoText.text = FormatDelta(dDmgC);
            deltaDanoCriticoText.color = dDmgC > 0 ? Color.green : Color.white;
        }
        if (deltaSuerteText != null)
        {
            deltaSuerteText.text = FormatDelta(dSuerte);
            deltaSuerteText.color = dSuerte > 0 ? Color.green : Color.white;
        }
        if (deltaDestrezaText != null)
        {
            deltaDestrezaText.text = FormatDelta(dDest);
            deltaDestrezaText.color = dDest > 0 ? Color.green : Color.white;
        }

        // Niveles
        int nextLevel = Mathf.Min(item.currentLevel + 1, 999);
        if (nivelActualTextA != null) nivelActualTextA.text = $"Nv. {item.currentLevel}";
        if (nivelActualTextB != null) nivelActualTextB.text = $"Nv. {item.currentLevel}";
        if (nivelSiguienteTextA != null) nivelSiguienteTextA.text = $"Nv. {nextLevel}";
        if (nivelSiguienteTextB != null) nivelSiguienteTextB.text = $"Nv. {nextLevel}";

        // Coste de mejora
        if (costUpgradeText != null)
        {
            int upgradeCost = improvementSystem != null ? improvementSystem.CalculateImprovementCost(item.currentLevel) : 0;
            costUpgradeText.text = upgradeCost.ToString();
        }
    }

    private void ClearDetailView()
    {
        if (detailViewImage != null) { detailViewImage.sprite = null; detailViewImage.enabled = false; }
        if (detailViewPanel != null) detailViewPanel.SetActive(false);
        if (arrowsPanel != null) arrowsPanel.SetActive(false);

        if (itemTitleTextA != null) { itemTitleTextA.text = ""; itemTitleTextA.color = Color.white; }
        if (itemTitleTextB != null) { itemTitleTextB.text = ""; itemTitleTextB.color = Color.white; }
        if (rarezaText != null) { rarezaText.text = ""; rarezaText.color = Color.white; }

        if (hpText != null) hpText.text = "0";
        if (manaText != null) manaText.text = "0";
        if (ataqueText != null) ataqueText.text = "0";
        if (defensaText != null) defensaText.text = "0";
        if (velocidadAtaqueText != null) velocidadAtaqueText.text = "0";
        if (ataqueCriticoText != null) ataqueCriticoText.text = "0";
        if (danoCriticoText != null) danoCriticoText.text = "0";
        if (suerteText != null) suerteText.text = "0";
        if (destrezaText != null) destrezaText.text = "0";

        if (nivelActualTextA != null) nivelActualTextA.text = "";
        if (nivelActualTextB != null) nivelActualTextB.text = "";
        if (nivelSiguienteTextA != null) nivelSiguienteTextA.text = "";
        if (nivelSiguienteTextB != null) nivelSiguienteTextB.text = "";

        if (costUpgradeText != null) costUpgradeText.text = "";

        // Limpiar stats siguientes
        if (nextHpText != null) nextHpText.text = "0";
        if (nextManaText != null) nextManaText.text = "0";
        if (nextAtaqueText != null) nextAtaqueText.text = "0";
        if (nextDefensaText != null) nextDefensaText.text = "0";
        if (nextVelocidadAtaqueText != null) nextVelocidadAtaqueText.text = "0";
        if (nextAtaqueCriticoText != null) nextAtaqueCriticoText.text = "0";
        if (nextDanoCriticoText != null) nextDanoCriticoText.text = "0";
        if (nextSuerteText != null) nextSuerteText.text = "0";
        if (nextDestrezaText != null) nextDestrezaText.text = "0";

        // Limpiar deltas y restaurar color blanco
        if (deltaHpText != null) { deltaHpText.text = ""; deltaHpText.color = Color.white; }
        if (deltaManaText != null) { deltaManaText.text = ""; deltaManaText.color = Color.white; }
        if (deltaAtaqueText != null) { deltaAtaqueText.text = ""; deltaAtaqueText.color = Color.white; }
        if (deltaDefensaText != null) { deltaDefensaText.text = ""; deltaDefensaText.color = Color.white; }
        if (deltaVelocidadAtaqueText != null) { deltaVelocidadAtaqueText.text = ""; deltaVelocidadAtaqueText.color = Color.white; }
        if (deltaAtaqueCriticoText != null) { deltaAtaqueCriticoText.text = ""; deltaAtaqueCriticoText.color = Color.white; }
        if (deltaDanoCriticoText != null) { deltaDanoCriticoText.text = ""; deltaDanoCriticoText.color = Color.white; }
        if (deltaSuerteText != null) { deltaSuerteText.text = ""; deltaSuerteText.color = Color.white; }
        if (deltaDestrezaText != null) { deltaDestrezaText.text = ""; deltaDestrezaText.color = Color.white; }

        // Ocultar paneles de equipado del grid
        if (forgePanelsEquipped != null)
        {
            for (int i = 0; i < forgePanelsEquipped.Length; i++)
            {
                if (forgePanelsEquipped[i] != null)
                    forgePanelsEquipped[i].SetActive(false);
            }
        }
    }

    private Color GetRarityColor(string rarity)
    {
        if (string.IsNullOrEmpty(rarity))
            return Color.white;

        switch (rarity.Trim().ToLower())
        {
            case "comun":
            case "común":
                return new Color(0.7f, 0.7f, 0.7f, 1f); // Gris
            case "raro":
            case "rara":
                return new Color(0.2f, 0.6f, 1f, 1f);   // Azul
            case "epico":
            case "épico":
            case "epica":
            case "épica":
                return new Color(0.8f, 0.2f, 0.9f, 1f); // Morado
            case "legendario":
            case "legendaria":
                return new Color(1f, 0.5f, 0f, 1f);     // Naranja/dorado
            case "demoniaco":
            case "demoníaco":
            case "demoniaca":
            case "demoníaca":
                return new Color(0.6f, 0f, 0.2f, 1f);   // Rojo oscuro
            case "extremo":
            case "extrema":
                return new Color(0f, 1f, 0.8f, 1f);     // Cian brillante
            default:
                return Color.white;
        }
    }

    private string FormatDelta(int delta)
    {
        if (delta > 0) return $"+{delta}";
        return delta.ToString();
    }

    /// <summary>
    /// Fuerza la actualización visual de todos los slots de forja.
    /// Similar a ForceRefreshAllLevelTextsDirectly() del inventario.
    /// Esto asegura que los sprites se repinten correctamente después de reanudar la partida.
    /// </summary>
    private void ForceRefreshAllSlotVisuals()
    {
        // LOG: Inicio de ForceRefreshAllSlotVisuals
        Control debugControl = FindFirstObjectByType<Control>();
        if (debugControl != null)
        {
            debugControl.LogForceRefreshAllSlotVisuals("ForceRefreshAllSlotVisuals");
        }

        if (forgeSlots == null || inventoryManager == null)
        {
            Debug.LogWarning("[FORGE_REFRESH] forgeSlots o inventoryManager es NULL");
            return;
        }

        int maxSlots = Mathf.Min(forgeSlots.Length, InventoryManager.INVENTORY_SIZE);
        Debug.Log($"[FORGE_REFRESH] Procesando {maxSlots} slots...");

        for (int i = 0; i < maxSlots; i++)
        {
            if (forgeSlots[i] != null)
            {
                ItemInstance item = inventoryManager.GetItem(i);
                
                // LOG: Antes de SetItem
                if (debugControl != null)
                {
                    debugControl.LogSetItem(forgeSlots[i], i, item, true, "ForceRefreshAllSlotVisuals");
                }

                if (item != null && item.IsValid())
                {
                    // Verificar sprite antes de asignar
                    Sprite itemSprite = item.GetItemSprite();
                    if (itemSprite == null && debugControl != null)
                    {
                        debugControl.LogGetItemSpriteNull(item, i, "ForceRefreshAllSlotVisuals");
                    }

                    // Forzar actualización completa del slot (igual que inventario)
                    forgeSlots[i].SetItem(item, forceUpdate: true);
                    
                    // LOG: Antes de ForceUpdateVisualsDirectly
                    if (debugControl != null)
                    {
                        debugControl.LogForceUpdateVisuals(forgeSlots[i], i, "ForceRefreshAllSlotVisuals");
                    }
                    
                    // Forzar actualización visual directa (esto repinta sprites)
                    forgeSlots[i].ForceUpdateVisualsDirectly();
                    
                    // LOG: Después de ForceUpdateVisualsDirectly
                    if (debugControl != null)
                    {
                        debugControl.LogSlotAfterRefresh(forgeSlots[i], i, inventoryManager, "ForceRefreshAllSlotVisuals");
                    }
                }
                else
                {
                    // Slot vacío: limpiar también con forceUpdate
                    forgeSlots[i].SetItem(null, forceUpdate: true);
                    forgeSlots[i].ForceUpdateVisualsDirectly();
                }
            }
        }
        
        // Forzar actualización del canvas para asegurar que los sprites se rendericen
        Canvas.ForceUpdateCanvases();
        
        Debug.Log("[FORGE_REFRESH] ForceRefreshAllSlotVisuals completado");
    }
}

