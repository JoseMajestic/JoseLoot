using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Gestiona el perfil del jugador (Hero), mostrando:
/// - Items equipados en cada slot con sus niveles
/// - Monedas del jugador
/// - Se actualiza automáticamente cuando se equipa/desequipa o cambian las monedas
/// - La información persiste aunque el panel se desactive
/// </summary>
public class HeroProfileManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al EquipmentManager")]
    [SerializeField] private EquipmentManager equipmentManager;

    [Tooltip("Referencia al PlayerMoney")]
    [SerializeField] private PlayerMoney playerMoney;

    [Header("UI de Monedas")]
    [Tooltip("Texto que muestra las monedas del jugador")]
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("UI de Slots de Equipo")]
    [Tooltip("Image que muestra el sprite del item equipado en el slot Arma")]
    [SerializeField] private Image armaImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Arma")]
    [SerializeField] private TextMeshProUGUI armaLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot ArmaSecundaria")]
    [SerializeField] private Image armaSecundariaImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot ArmaSecundaria")]
    [SerializeField] private TextMeshProUGUI armaSecundariaLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Sombrero")]
    [SerializeField] private Image sombreroImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Sombrero")]
    [SerializeField] private TextMeshProUGUI sombreroLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Pechera")]
    [SerializeField] private Image pecheraImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Pechera")]
    [SerializeField] private TextMeshProUGUI pecheraLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Botas")]
    [SerializeField] private Image botasImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Botas")]
    [SerializeField] private TextMeshProUGUI botasLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Montura")]
    [SerializeField] private Image monturaImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Montura")]
    [SerializeField] private TextMeshProUGUI monturaLevelText;

    [Header("UI de Características del Héroe")]
    [Tooltip("Texto que muestra el HP total del héroe (base + equipo)")]
    [SerializeField] private TextMeshProUGUI hpText;
    
    [Tooltip("Texto que muestra el Mana total del héroe (base + equipo)")]
    [SerializeField] private TextMeshProUGUI manaText;
    
    [Tooltip("Texto que muestra el Ataque total del héroe (base + equipo)")]
    [SerializeField] private TextMeshProUGUI ataqueText;
    
    [Tooltip("Texto que muestra la Defensa total del héroe (base + equipo)")]
    [SerializeField] private TextMeshProUGUI defensaText;
    
    [Tooltip("Texto que muestra la Velocidad de Ataque total del héroe (base + equipo)")]
    [SerializeField] private TextMeshProUGUI velocidadAtaqueText;
    
    [Tooltip("Texto que muestra el Ataque Crítico total del héroe (base + equipo)")]
    [SerializeField] private TextMeshProUGUI ataqueCriticoText;
    
    [Tooltip("Texto que muestra el Daño Crítico total del héroe (base + equipo)")]
    [SerializeField] private TextMeshProUGUI danoCriticoText;
    
    [Tooltip("Texto que muestra la Suerte total del héroe (base + equipo)")]
    [SerializeField] private TextMeshProUGUI suerteText;
    
    [Tooltip("Texto que muestra la Destreza total del héroe (base + equipo)")]
    [SerializeField] private TextMeshProUGUI destrezaText;

    [Header("Stats Base del Héroe")]
    [Tooltip("HP base del héroe (sin equipo)")]
    [SerializeField] private int baseHP = 10;
    
    [Tooltip("Mana base del héroe (sin equipo)")]
    [SerializeField] private int baseMana = 0;
    
    [Tooltip("Ataque base del héroe (sin equipo)")]
    [SerializeField] private int baseAtaque = 1;
    
    [Tooltip("Defensa base del héroe (sin equipo)")]
    [SerializeField] private int baseDefensa = 1;
    
    [Tooltip("Velocidad de Ataque base del héroe (sin equipo)")]
    [SerializeField] private int baseVelocidadAtaque = 0;
    
    [Tooltip("Ataque Crítico base del héroe (sin equipo)")]
    [SerializeField] private int baseAtaqueCritico = 1;
    
    [Tooltip("Daño Crítico base del héroe (sin equipo)")]
    [SerializeField] private int baseDanoCritico = 0;
    
    [Tooltip("Suerte base del héroe (sin equipo)")]
    [SerializeField] private int baseSuerte = 0;
    
    [Tooltip("Destreza base del héroe (sin equipo)")]
    [SerializeField] private int baseDestreza = 0;

    private void Start()
    {
        // Obtener referencias desde GameDataManager si no están asignadas
        if (equipmentManager == null)
        {
            if (GameDataManager.Instance != null)
            {
                equipmentManager = GameDataManager.Instance.EquipmentManager;
            }
            if (equipmentManager == null)
            {
                Debug.LogError("HeroProfileManager: EquipmentManager no está asignado. Asigna la referencia en el Inspector o asegúrate de que GameDataManager tenga EquipmentManager.");
            }
        }

        if (playerMoney == null)
        {
            if (GameDataManager.Instance != null)
            {
                playerMoney = GameDataManager.Instance.PlayerMoney;
            }
            if (playerMoney == null)
            {
                Debug.LogError("HeroProfileManager: PlayerMoney no está asignado. Asigna la referencia en el Inspector o asegúrate de que GameDataManager tenga PlayerMoney.");
            }
        }

        // Suscribirse a eventos del equipo
        if (equipmentManager != null)
        {
            equipmentManager.OnItemEquipped += OnItemEquipped;
            equipmentManager.OnItemUnequipped += OnItemUnequipped;
            equipmentManager.OnEquipmentChanged += OnEquipmentChanged;
        }

        // Suscribirse a eventos de monedas
        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged += OnMoneyChanged;
        }

        // Refrescar toda la UI al inicio (después de un frame para asegurar orden)
        StartCoroutine(RefreshAfterFrame());
    }

    /// <summary>
    /// Refresca la UI después de esperar un frame.
    /// </summary>
    private System.Collections.IEnumerator RefreshAfterFrame()
    {
        yield return null; // Esperar un frame
        RefreshAllEquipmentSlots();
        RefreshMoney();
        RefreshHeroStats();
    }

    /// <summary>
    /// Se llama cuando el GameObject se activa.
    /// Refresca la UI para asegurar que se muestre correctamente aunque el panel estuviera desactivado al inicio.
    /// </summary>
    private void OnEnable()
    {
        // Si el equipo ya está inicializado, refrescar la UI
        if (equipmentManager != null)
        {
            // SOLUCIÓN: Usar corrutina para esperar a que los componentes estén listos
            // Unity necesita tiempo para inicializar los componentes Image cuando se activa el GameObject
            StartCoroutine(RefreshEquipmentSlotsWhenReady());
        }
    }

    /// <summary>
    /// Refresca los slots de equipo cuando los componentes están listos.
    /// SOLUCIÓN 4: Asignación diferida - Verifica que el Canvas y todos los Image estén completamente listos antes de asignar sprites.
    /// </summary>
    private IEnumerator RefreshEquipmentSlotsWhenReady()
    {
        if (equipmentManager == null)
            yield break;

        // SOLUCIÓN 4: Esperar a que el Canvas esté completamente activo y listo
        Canvas canvas = GetComponentInParent<Canvas>();
        while (canvas == null || !canvas.isActiveAndEnabled)
        {
            yield return null;
            canvas = GetComponentInParent<Canvas>();
        }
        
        // SOLUCIÓN 4: Esperar a que todos los componentes Image de los slots de equipo estén activos y listos
        bool allImagesReady = false;
        int maxWaitFrames = 10; // Límite de seguridad para evitar bucles infinitos
        int waitFrames = 0;
        
        while (!allImagesReady && waitFrames < maxWaitFrames)
        {
            allImagesReady = true;
            
            // Verificar que todos los Image de los slots estén listos
            Image[] equipmentImages = { armaImage, armaSecundariaImage, sombreroImage, pecheraImage, botasImage, monturaImage };
            
            foreach (Image slotImage in equipmentImages)
            {
                if (slotImage == null || !slotImage.gameObject.activeInHierarchy || !slotImage.enabled)
                {
                    allImagesReady = false;
                    break;
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
        
        // Refrescar todos los slots de equipo (con forceRefresh=true)
        RefreshAllEquipmentSlots();
        
        // Refrescar monedas
        RefreshMoney();
        
        // Refrescar características del héroe
        RefreshHeroStats();
        
        // Esperar otro frame para que Unity procese los cambios de sprites
        yield return null;
        
        // Forzar actualización del canvas para asegurar que los sprites se rendericen
        UnityEngine.Canvas.ForceUpdateCanvases();
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos del equipo
        if (equipmentManager != null)
        {
            equipmentManager.OnItemEquipped -= OnItemEquipped;
            equipmentManager.OnItemUnequipped -= OnItemUnequipped;
            equipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
        }

        // Desuscribirse de eventos de monedas
        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged -= OnMoneyChanged;
        }
    }

    /// <summary>
    /// Se llama cuando se equipa un item. Actualiza el slot correspondiente y las características.
    /// </summary>
    private void OnItemEquipped(EquipmentManager.EquipmentSlotType slot, ItemInstance itemInstance)
    {
        // Forzar actualización inmediata del slot específico
        UpdateEquipmentSlot(slot, itemInstance, forceRefresh: true);
        
        // SOLUCIÓN: Forzar actualización de todos los slots después de un frame
        // Esto asegura que los sprites se carguen correctamente, similar a como funciona en el inventario
        StartCoroutine(ForceRefreshAllEquipmentSlotsAfterFrame());
        
        RefreshHeroStats(); // Actualizar características cuando se equipa
    }

    /// <summary>
    /// Se llama cuando se desequipa un item. Limpia el slot correspondiente y actualiza las características.
    /// </summary>
    private void OnItemUnequipped(EquipmentManager.EquipmentSlotType slot, ItemInstance itemInstance)
    {
        UpdateEquipmentSlot(slot, null, forceRefresh: true); // Forzar actualización del sprite
        RefreshHeroStats(); // Actualizar características cuando se desequipa
    }

    /// <summary>
    /// Fuerza la actualización de todos los slots de equipo después de esperar un frame.
    /// Similar a como funciona en el inventario cuando se hace clic en un slot.
    /// </summary>
    private IEnumerator ForceRefreshAllEquipmentSlotsAfterFrame()
    {
        // Esperar un frame para que Unity procese la asignación del sprite
        yield return null;
        
        // Forzar actualización de todos los slots de equipo
        RefreshAllEquipmentSlots();
        
        // Esperar otro frame y forzar actualización del canvas
        yield return null;
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Se llama cuando cambia el equipo (equipar/desequipar).
    /// </summary>
    private void OnEquipmentChanged()
    {
        RefreshHeroStats(); // Actualizar características cuando cambia el equipo
    }

    /// <summary>
    /// Se llama cuando cambian las monedas del jugador.
    /// </summary>
    private void OnMoneyChanged(int newMoney)
    {
        RefreshMoney();
    }

    /// <summary>
    /// Actualiza un slot específico del equipo en la UI.
    /// </summary>
    private void UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType slot, ItemInstance itemInstance, bool forceRefresh = false)
    {
        Image slotImage = null;
        TextMeshProUGUI slotLevelText = null;

        // Obtener referencias al Image y Text según el slot
        switch (slot)
        {
            case EquipmentManager.EquipmentSlotType.Arma:
                slotImage = armaImage;
                slotLevelText = armaLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.ArmaSecundaria:
                slotImage = armaSecundariaImage;
                slotLevelText = armaSecundariaLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Sombrero:
                slotImage = sombreroImage;
                slotLevelText = sombreroLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Pechera:
                slotImage = pecheraImage;
                slotLevelText = pecheraLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Botas:
                slotImage = botasImage;
                slotLevelText = botasLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Montura:
                slotImage = monturaImage;
                slotLevelText = monturaLevelText;
                break;
        }

        // Actualizar Image
        if (slotImage != null)
        {
            // SOLUCIÓN MEJORADA: No salir temprano - forzar la asignación incluso si el GameObject no está completamente activo
            // Unity puede reportar que no está activo incluso cuando debería estarlo
            
            if (itemInstance != null && itemInstance.IsValid())
            {
                // SOLUCIÓN ESTRUCTURAL: Siempre obtener el sprite fresco desde ItemData (como una "instancia/clon" fresca)
                // Esto asegura que el sprite se recargue incluso si Unity lo limpió al desactivar el panel
                Sprite itemSprite = itemInstance.GetItemSprite();
                if (itemSprite != null)
                {
                    // SOLUCIÓN ESTRUCTURAL: Si forceRefresh es true, limpiar primero el sprite para forzar actualización completa
                    // Esto asegura que Unity recargue el sprite incluso si estaba asignado antes
                    if (forceRefresh)
                    {
                        slotImage.sprite = null;
                        // Forzar actualización del canvas después de limpiar
                        Canvas.ForceUpdateCanvases();
                    }
                    
                    // Asegurar que el componente Image esté habilitado y activo
                    if (!slotImage.enabled)
                    {
                        slotImage.enabled = true;
                    }
                    
                    // Asegurar que el GameObject esté activo
                    if (!slotImage.gameObject.activeSelf)
                    {
                        slotImage.gameObject.SetActive(true);
                    }
                    
                    // Asignar el sprite fresco desde ItemData - hacerlo de manera forzada
                    slotImage.sprite = itemSprite;
                    
                    // SOLUCIÓN: Verificar que el sprite se asignó correctamente
                    if (slotImage.sprite != itemSprite)
                    {
                        // Si no se asignó, intentar de nuevo forzando
                        slotImage.sprite = null;
                        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(slotImage.rectTransform);
                        slotImage.sprite = itemSprite;
                    }
                    
                    // Forzar actualización del canvas para asegurar que el sprite se renderice
                    UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(slotImage);
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(slotImage.rectTransform);
                    // También forzar actualización inmediata del canvas (método estático)
                    Canvas.ForceUpdateCanvases();
                }
                else
                {
                    // Si el item no tiene sprite, limpiar para mostrar el sprite por defecto
                    slotImage.sprite = null;
                    slotImage.enabled = true; // Mantener habilitado para mostrar sprite por defecto
                }
            }
            else
            {
                // Slot vacío: limpiar sprite para que vuelva al sprite por defecto del Image
                slotImage.sprite = null;
                slotImage.enabled = true; // Mantener habilitado para mostrar sprite por defecto
            }
        }

        // Actualizar Text del nivel
        if (slotLevelText != null)
        {
            if (itemInstance != null && itemInstance.IsValid())
            {
                slotLevelText.text = $"Nv. {itemInstance.currentLevel}";
                slotLevelText.gameObject.SetActive(true);
            }
            else
            {
                slotLevelText.text = "";
                slotLevelText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Refresca todos los slots de equipo según el estado actual.
    /// SOLUCIÓN ESTRUCTURAL: Fuerza la recarga de sprites desde EquipmentManager (como "instancia/clon" fresca).
    /// Similar a como funciona en el inventario - lee directamente desde EquipmentManager y fuerza actualización.
    /// </summary>
    private void RefreshAllEquipmentSlots()
    {
        if (equipmentManager == null)
            return;

        // SOLUCIÓN ESTRUCTURAL: Leer directamente desde EquipmentManager (como una "instancia/clon" fresca)
        // Esto asegura que siempre leemos los objetos equipados actuales, no referencias antiguas
        // y forzar actualización de todos los slots con forceRefresh=true
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Arma, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Arma), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.ArmaSecundaria, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.ArmaSecundaria), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Sombrero, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Sombrero), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Pechera, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Pechera), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Botas, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Botas), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Montura, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Montura), forceRefresh: true);
        
        // Forzar actualización del canvas después de actualizar todos los slots
        // Esto asegura que todos los sprites se rendericen correctamente
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Refresca el texto de monedas.
    /// </summary>
    private void RefreshMoney()
    {
        if (moneyText != null && playerMoney != null)
        {
            moneyText.text = playerMoney.GetMoney().ToString();
        }
    }

    /// <summary>
    /// Calcula y muestra las características totales del héroe (base + equipo).
    /// </summary>
    private void RefreshHeroStats()
    {
        if (equipmentManager == null)
            return;

        // Obtener stats del equipo
        EquipmentStats equipmentStats = equipmentManager.GetTotalEquipmentStats();

        // Calcular stats totales (base del héroe + equipo)
        int totalHP = baseHP + equipmentStats.hp;
        int totalMana = baseMana + equipmentStats.mana;
        int totalAtaque = baseAtaque + equipmentStats.ataque;
        int totalDefensa = baseDefensa + equipmentStats.defensa;
        int totalVelocidadAtaque = baseVelocidadAtaque + equipmentStats.velocidadAtaque;
        int totalAtaqueCritico = baseAtaqueCritico + equipmentStats.ataqueCritico;
        int totalDanoCritico = baseDanoCritico + equipmentStats.danoCritico;
        int totalSuerte = baseSuerte + equipmentStats.suerte;
        int totalDestreza = baseDestreza + equipmentStats.destreza;

        // Actualizar textos
        if (hpText != null)
            hpText.text = totalHP.ToString();
        
        if (manaText != null)
            manaText.text = totalMana.ToString();
        
        if (ataqueText != null)
            ataqueText.text = totalAtaque.ToString();
        
        if (defensaText != null)
            defensaText.text = totalDefensa.ToString();
        
        if (velocidadAtaqueText != null)
            velocidadAtaqueText.text = totalVelocidadAtaque.ToString();
        
        if (ataqueCriticoText != null)
            ataqueCriticoText.text = totalAtaqueCritico.ToString();
        
        if (danoCriticoText != null)
            danoCriticoText.text = totalDanoCritico.ToString();
        
        if (suerteText != null)
            suerteText.text = totalSuerte.ToString();
        
        if (destrezaText != null)
            destrezaText.text = totalDestreza.ToString();
    }
}

