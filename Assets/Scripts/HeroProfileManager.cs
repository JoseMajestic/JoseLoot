using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq;

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

    [Tooltip("Referencia al ItemImprovementSystem (opcional, para detectar mejoras de items)")]
    [SerializeField] private ItemImprovementSystem improvementSystem;

    [Header("UI de Monedas")]
    [Tooltip("Texto que muestra las monedas del jugador")]
    [SerializeField] private TextMeshProUGUI moneyText;

    [Tooltip("Texto que muestra el tiempo jugado total")]
    [SerializeField] private TextMeshProUGUI totalPlayTimeText;

    [Header("UI de Estadísticas")]
    [Tooltip("Texto que muestra el total de enfrentamientos")]
    [SerializeField] private TextMeshProUGUI totalClashesText;

    [Tooltip("Texto que muestra el total de cofres abiertos")]
    [SerializeField] private TextMeshProUGUI totalOpenChestsText;

    [Tooltip("Texto que muestra el total de peleas ganadas")]
    [SerializeField] private TextMeshProUGUI totalWonFightsText;

    [Tooltip("Texto que muestra el total de peleas perdidas")]
    [SerializeField] private TextMeshProUGUI totalLostFightsText;

    [Tooltip("Texto que muestra el título de clase del héroe (Primera Clase, Segunda Clase, etc.)")]
    [SerializeField] private TextMeshProUGUI classTitleText;

    [Header("UI de Experiencia del Héroe")]
    [Tooltip("Slider que muestra la experiencia del héroe")]
    [SerializeField] private Slider heroExperienceSlider;

    [Tooltip("Texto que muestra la experiencia actual y necesaria (ej: 150/200)")]
    [SerializeField] private TextMeshProUGUI heroExperienceText;

    [Tooltip("Texto que muestra el nivel actual del héroe")]
    [SerializeField] private TextMeshProUGUI heroLevelText;

    [Header("UI de Slots de Equipo")]
    [Tooltip("Image que muestra el sprite del item equipado en el slot Montura")]
    [SerializeField] private Image monturaImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Montura")]
    [SerializeField] private TextMeshProUGUI monturaLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Casco")]
    [SerializeField] private Image cascoImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Casco")]
    [SerializeField] private TextMeshProUGUI cascoLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Collar")]
    [SerializeField] private Image collarImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Collar")]
    [SerializeField] private TextMeshProUGUI collarLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Arma")]
    [SerializeField] private Image armaImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Arma")]
    [SerializeField] private TextMeshProUGUI armaLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Armadura")]
    [SerializeField] private Image armaduraImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Armadura")]
    [SerializeField] private TextMeshProUGUI armaduraLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Escudo")]
    [SerializeField] private Image escudoImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Escudo")]
    [SerializeField] private TextMeshProUGUI escudoLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Guantes")]
    [SerializeField] private Image guantesImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Guantes")]
    [SerializeField] private TextMeshProUGUI guantesLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Cinturón")]
    [SerializeField] private Image cinturonImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Cinturón")]
    [SerializeField] private TextMeshProUGUI cinturonLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Anillo")]
    [SerializeField] private Image anilloImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Anillo")]
    [SerializeField] private TextMeshProUGUI anilloLevelText;

    [Tooltip("Image que muestra el sprite del item equipado en el slot Botas")]
    [SerializeField] private Image botasImage;
    
    [Tooltip("Texto que muestra el nivel del item equipado en el slot Botas")]
    [SerializeField] private TextMeshProUGUI botasLevelText;

    [Header("Panel Detallado de Equipo")]
    [Tooltip("Panel que se mostrará mientras se mantiene pulsado un slot")]
    [SerializeField] private GameObject equipmentDetailPanel;
    [Tooltip("Texto donde se mostrarán los valores detallados del item")]
    [SerializeField] private TextMeshProUGUI equipmentDetailText;
    [Tooltip("Opcional: CanvasGroup para controlar la visibilidad por alpha (si no se asigna se usará SetActive)")]
    [SerializeField] private CanvasGroup equipmentDetailCanvasGroup;
    [Tooltip("Botones asociados a cada slot para detectar el hold")]
    [SerializeField] private HeroProfileEquipmentSlotButton[] equipmentSlotButtons;

    [Header("Panel de Sets")]
    [Tooltip("Panel contenedor de la información de sets (opcional)")]
    [SerializeField] private GameObject setBonusPanel;
    [Tooltip("CanvasGroup opcional para animar el panel de sets")]
    [SerializeField] private CanvasGroup setBonusPanelCanvasGroup;
    [Tooltip("Texto donde se muestra el nombre del set y sus piezas")]
    [SerializeField] private TextMeshProUGUI setNameAndPiecesText;
    [Tooltip("Texto donde se muestran las bonificaciones del set")]
    [SerializeField] private TextMeshProUGUI setBonusesText;

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

    [Header("Tiempo Jugado")]
    [Tooltip("Tiempo total jugado en segundos (se actualizará desde el sistema de guardado)")]
    [SerializeField] private float totalPlayTimeSeconds = 0f;

    [Tooltip("Si está marcado, el tiempo se incrementa automáticamente cada segundo")]
    [SerializeField] private bool autoIncrementTime = true;

    private Coroutine timeUpdateCoroutine;

    // Diccionario para guardar los sprites por defecto de cada slot
    private Dictionary<Image, Sprite> defaultSlotSprites = new Dictionary<Image, Sprite>();
    private HeroProfileEquipmentSlotButton activeSlotButton = null;
    
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

        // Suscribirse a eventos de mejora de items (para actualizar niveles cuando se mejora desde Forja)
        if (improvementSystem != null)
        {
            improvementSystem.OnItemImproved += OnItemImproved;
        }
        
        // SOLUCIÓN ARQUITECTÓNICA: Suscribirse a OnLevelChanged de items equipados para recibir notificaciones directas
        // Esto asegura que cuando un item equipado cambia de nivel, el panel se actualice inmediatamente
        if (equipmentManager != null)
        {
            // Suscribirse a cambios de nivel de todos los items equipados actuales
            SubscribeToEquippedItemsLevelChanges();
        }

        // Guardar los sprites por defecto de todos los slots antes de cualquier modificación
        SaveDefaultSlotSprites();

        InitializeEquipmentSlotButtons();

        // Refrescar toda la UI al inicio (después de un frame para asegurar orden)
        StartCoroutine(RefreshAfterFrame());

        // Iniciar actualización continua del tiempo si está habilitado
        if (autoIncrementTime && totalPlayTimeText != null)
        {
            StartTimeUpdate();
        }
        else if (totalPlayTimeText != null)
        {
            // Si no se auto-incrementa, actualizar una vez con el tiempo actual
            UpdateTotalPlayTime(totalPlayTimeSeconds);
        }

        // Actualizar estadísticas al inicio
        RefreshStatistics();
        
        // Actualizar experiencia al inicio
        RefreshHeroExperience();
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
        // SOLUCIÓN CRÍTICA: Sincronizar niveles de items equipados desde el perfil guardado
        // Esto asegura que los niveles estén actualizados incluso si el panel estaba inactivo
        // cuando se mejoró un objeto en la forja
        if (GameDataManager.Instance != null && equipmentManager != null)
        {
            GameDataManager.Instance.SyncEquippedItemLevelsFromProfile();
        }
        
        // SOLUCIÓN ARQUITECTÓNICA: Polling para detectar y corregir discrepancias de nivel en slots de equipo
        // Compara el nivel actual del ItemInstance en memoria con el nivel mostrado en el slot
        // Si hay diferencia, fuerza actualización
        PollAndUpdateEquipmentSlots();
        
        // Si el equipo ya está inicializado, refrescar la UI
        if (equipmentManager != null)
        {
            // SOLUCIÓN: Usar corrutina para esperar a que los componentes estén listos
            // Unity necesita tiempo para inicializar los componentes Image cuando se activa el GameObject
            StartCoroutine(RefreshEquipmentSlotsWhenReady());
        }

        // Reiniciar actualización del tiempo si está habilitado
        if (autoIncrementTime && totalPlayTimeText != null && timeUpdateCoroutine == null)
        {
            StartTimeUpdate();
        }
    }

    private void OnDisable()
    {
        // Detener actualización del tiempo cuando el panel se desactiva
        if (timeUpdateCoroutine != null)
        {
            StopCoroutine(timeUpdateCoroutine);
            timeUpdateCoroutine = null;
        }
    }

    /// <summary>
    /// Refresca los slots de equipo cuando los componentes están listos.
    /// SOLUCIÓN 4: Asignación diferida - Verifica que el Canvas y todos los Image estén completamente listos antes de asignar sprites.
    /// SIEMPRE fuerza la actualización completa de todos los slots al abrir el panel.
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
            Image[] equipmentImages = { monturaImage, cascoImage, collarImage, armaImage, armaduraImage, escudoImage, guantesImage, cinturonImage, anilloImage, botasImage };
            
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
        
        // SOLUCIÓN: SIEMPRE forzar actualización completa de todos los slots al abrir el panel
        // Esto asegura que todos los niveles y sprites se actualicen correctamente
        RefreshAllEquipmentSlots();
        
        // Refrescar monedas
        RefreshMoney();
        
        // Refrescar características del héroe
        RefreshHeroStats();
        
        // Refrescar estadísticas
        RefreshStatistics();
        
        // Refrescar experiencia
        RefreshHeroExperience();
        
        // Esperar otro frame para que Unity procese los cambios de sprites
        yield return null;
        
        // SOLUCIÓN: Forzar actualización de todos los textos de nivel después de que los componentes estén listos
        // Esto asegura que los textos se rendericen correctamente incluso si el panel estaba inactivo
        ForceUpdateAllLevelTexts();
        
        // SOLUCIÓN SENCILLA: Refresco final agresivo de todos los textos después de que todo esté listo
        // Esto lee directamente desde EquipmentManager y actualiza todos los textos sin pasar por UpdateEquipmentSlot()
        // Se ejecuta al final para asegurar que los niveles ya están sincronizados y los componentes están activos
        yield return null; // Frame adicional para asegurar que todo se procesó
        yield return new WaitForEndOfFrame(); // Esperar hasta el final del frame
        
        ForceRefreshAllEquipmentLevelTextsDirectly();
        
        // Forzar actualización del canvas para asegurar que los sprites se rendericen
        UnityEngine.Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// SOLUCIÓN ARQUITECTÓNICA: Polling para detectar y corregir discrepancias de nivel en slots de equipo.
    /// Compara el nivel actual del ItemInstance en memoria con el nivel mostrado en el slot.
    /// Si hay diferencia, fuerza actualización. Se llama en OnEnable().
    /// </summary>
    private void PollAndUpdateEquipmentSlots()
    {
        if (equipmentManager == null)
            return;

        // Iterar sobre todos los slots de equipo
        for (int i = 0; i < 10; i++)
        {
            EquipmentManager.EquipmentSlotType slotType = (EquipmentManager.EquipmentSlotType)i;
            ItemInstance equippedItem = equipmentManager.GetEquippedItem(slotType);
            
            if (equippedItem != null && equippedItem.IsValid())
            {
                // Obtener el nivel actual mostrado en el slot (si existe)
                // Para esto, necesitamos leer el nivel desde el TextMeshProUGUI o forzar actualización
                // Como no tenemos acceso directo al nivel mostrado, simplemente forzamos actualización
                // si el item está equipado (el polling detectará cambios si los hay)
                UpdateEquipmentSlot(slotType, equippedItem, forceRefresh: true);
            }
        }
    }

    /// <summary>
    /// SOLUCIÓN SENCILLA: Fuerza la actualización de todos los textos de nivel de equipamiento directamente.
    /// Lee el ItemInstance desde EquipmentManager y actualiza el texto sin pasar por UpdateEquipmentSlot().
    /// Se ejecuta al final de RefreshEquipmentSlotsWhenReady() para asegurar que todo esté listo.
    /// </summary>
    private void ForceRefreshAllEquipmentLevelTextsDirectly()
    {
        if (equipmentManager == null)
            return;

        // Iterar sobre todos los slots de equipo y actualizar el texto directamente
        for (int i = 0; i < 10; i++)
        {
            EquipmentManager.EquipmentSlotType slotType = (EquipmentManager.EquipmentSlotType)i;
            ItemInstance equippedItem = equipmentManager.GetEquippedItem(slotType);
            
            if (equippedItem != null && equippedItem.IsValid())
            {
                // Obtener referencias al Image y Text según el slot
                TextMeshProUGUI slotLevelText = null;
                
                switch (slotType)
                {
                    case EquipmentManager.EquipmentSlotType.Montura:
                        slotLevelText = monturaLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Casco:
                        slotLevelText = cascoLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Collar:
                        slotLevelText = collarLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Arma:
                        slotLevelText = armaLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Armadura:
                        slotLevelText = armaduraLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Escudo:
                        slotLevelText = escudoLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Guantes:
                        slotLevelText = guantesLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Cinturon:
                        slotLevelText = cinturonLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Anillo:
                        slotLevelText = anilloLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Botas:
                        slotLevelText = botasLevelText;
                        break;
                }

                // SOLUCIÓN SENCILLA: Actualizar el texto directamente desde el ItemInstance
                if (slotLevelText != null)
                {
                    // Asegurar que el componente esté activo y habilitado
                    if (!slotLevelText.gameObject.activeSelf)
                    {
                        slotLevelText.gameObject.SetActive(true);
                    }
                    if (!slotLevelText.enabled)
                    {
                        slotLevelText.enabled = true;
                    }

                    // Limpiar primero para forzar actualización
                    slotLevelText.text = "";
                    Canvas.ForceUpdateCanvases();
                    
                    // Actualizar con el nivel actual del ItemInstance
                    slotLevelText.text = $"Nv. {equippedItem.currentLevel}";
                    
                    // Forzar actualización del TextMeshProUGUI
                    slotLevelText.ForceMeshUpdate();
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(slotLevelText.rectTransform);
                    Canvas.ForceUpdateCanvases();
                    
                    // Registrar para rebuild del canvas
                    if (slotLevelText.canvas != null)
                    {
                        UnityEngine.UI.CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(slotLevelText);
                    }
                }
            }
            else
            {
                // Slot vacío: limpiar texto
                TextMeshProUGUI slotLevelText = null;
                switch (slotType)
                {
                    case EquipmentManager.EquipmentSlotType.Montura:
                        slotLevelText = monturaLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Casco:
                        slotLevelText = cascoLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Collar:
                        slotLevelText = collarLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Arma:
                        slotLevelText = armaLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Armadura:
                        slotLevelText = armaduraLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Escudo:
                        slotLevelText = escudoLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Guantes:
                        slotLevelText = guantesLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Cinturon:
                        slotLevelText = cinturonLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Anillo:
                        slotLevelText = anilloLevelText;
                        break;
                    case EquipmentManager.EquipmentSlotType.Botas:
                        slotLevelText = botasLevelText;
                        break;
                }
                
                if (slotLevelText != null)
                {
                    slotLevelText.text = "";
                    slotLevelText.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Fuerza la actualización de todos los textos de nivel en los slots de equipo.
    /// Útil cuando el panel estaba inactivo durante una mejora de item.
    /// </summary>
    private void ForceUpdateAllLevelTexts()
    {
        if (equipmentManager == null)
            return;

        // Forzar actualización de todos los slots de equipo para asegurar que los textos se rendericen
        // CORRECCIÓN: Iterar sobre todos los slots disponibles (10 en lugar de 6)
        foreach (EquipmentManager.EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
        {
            ItemInstance equippedItem = equipmentManager.GetEquippedItem(slotType);
            if (equippedItem != null && equippedItem.IsValid())
            {
                // Leer el item actualizado directamente desde EquipmentManager y forzar actualización
                UpdateEquipmentSlot(slotType, equippedItem, forceRefresh: true);
            }
        }
    }

    private void OnDestroy()
    {
        // SOLUCIÓN ARQUITECTÓNICA: Desuscribirse de OnLevelChanged de items equipados
        UnsubscribeFromEquippedItemsLevelChanges();
        
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

        // Desuscribirse de eventos de mejora de items
        if (improvementSystem != null)
        {
            improvementSystem.OnItemImproved -= OnItemImproved;
        }

        // Detener actualización del tiempo
        if (timeUpdateCoroutine != null)
        {
            StopCoroutine(timeUpdateCoroutine);
            timeUpdateCoroutine = null;
        }

        activeSlotButton = null;
    }

    /// <summary>
    /// Se llama cuando se equipa un item. Actualiza el slot correspondiente y las características.
    /// </summary>
    private void OnItemEquipped(EquipmentManager.EquipmentSlotType slot, ItemInstance itemInstance)
    {
        // SOLUCIÓN ARQUITECTÓNICA: Suscribirse a OnLevelChanged del item equipado
        if (itemInstance != null && itemInstance.IsValid())
        {
            itemInstance.OnLevelChanged += OnEquippedItemLevelChanged;
        }
        
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
        // SOLUCIÓN ARQUITECTÓNICA: Desuscribirse de OnLevelChanged del item desequipado
        if (itemInstance != null && itemInstance.IsValid())
        {
            itemInstance.OnLevelChanged -= OnEquippedItemLevelChanged;
        }
        
        UpdateEquipmentSlot(slot, null, forceRefresh: true); // Forzar actualización del sprite
        
        // SOLUCIÓN: Usar corrutina para refrescar stats después de un frame, similar a OnItemEquipped
        // Esto asegura que EquipmentManager haya actualizado sus stats internos antes de refrescar la UI
        StartCoroutine(RefreshHeroStatsAfterFrame());
    }
    
    /// <summary>
    /// SOLUCIÓN ARQUITECTÓNICA: Se llama cuando el nivel de un item equipado cambia.
    /// Fuerza la actualización del slot correspondiente en el panel de héroe.
    /// </summary>
    private void OnEquippedItemLevelChanged(ItemInstance item, int oldLevel, int newLevel)
    {
        if (equipmentManager == null || item == null || !item.IsValid())
            return;
        
        // Buscar en qué slot está equipado este item
        // CORRECCIÓN: Iterar sobre todos los slots disponibles (10 en lugar de 6)
        foreach (EquipmentManager.EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
        {
            ItemInstance equippedItem = equipmentManager.GetEquippedItem(slotType);
            
            if (equippedItem != null && equippedItem.IsValid() && equippedItem.IsSameInstance(item))
            {
                // El item está equipado en este slot, forzar actualización
                UpdateEquipmentSlot(slotType, equippedItem, forceRefresh: true);
                RefreshHeroStats(); // Actualizar stats porque el nivel cambió
                break;
            }
        }
    }
    
    /// <summary>
    /// SOLUCIÓN ARQUITECTÓNICA: Suscribe a OnLevelChanged de todos los items equipados actuales.
    /// </summary>
    private void SubscribeToEquippedItemsLevelChanges()
    {
        if (equipmentManager == null)
            return;
        
        // Suscribirse a cambios de nivel de todos los items equipados
        // CORRECCIÓN: Iterar sobre todos los slots disponibles (10 en lugar de 6)
        foreach (EquipmentManager.EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
        {
            ItemInstance equippedItem = equipmentManager.GetEquippedItem(slotType);
            
            if (equippedItem != null && equippedItem.IsValid())
            {
                equippedItem.OnLevelChanged += OnEquippedItemLevelChanged;
            }
        }
    }
    
    /// <summary>
    /// SOLUCIÓN ARQUITECTÓNICA: Desuscribe de OnLevelChanged de todos los items equipados.
    /// </summary>
    private void UnsubscribeFromEquippedItemsLevelChanges()
    {
        if (equipmentManager == null)
            return;
        
        // Desuscribirse de cambios de nivel de todos los items equipados
        // CORRECCIÓN: Iterar sobre todos los slots disponibles (10 en lugar de 6)
        foreach (EquipmentManager.EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
        {
            ItemInstance equippedItem = equipmentManager.GetEquippedItem(slotType);
            
            if (equippedItem != null && equippedItem.IsValid())
            {
                equippedItem.OnLevelChanged -= OnEquippedItemLevelChanged;
            }
        }
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
    /// Refresca las stats del héroe después de esperar un frame.
    /// Esto asegura que EquipmentManager haya actualizado sus stats internos.
    /// </summary>
    private IEnumerator RefreshHeroStatsAfterFrame()
    {
        yield return null;
        RefreshHeroStats();
    }

    private void InitializeEquipmentSlotButtons()
    {
        if (equipmentSlotButtons == null || equipmentSlotButtons.Length == 0)
            return;

        foreach (var slotButton in equipmentSlotButtons)
        {
            if (slotButton == null)
                continue;

            slotButton.SetHeroProfileManager(this);
        }

        HideEquipmentDetailPanel();
    }

    private void ShowEquipmentDetailPanel(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
        {
            HideEquipmentDetailPanel();
            return;
        }

        string detailText = BuildDetailText(itemInstance);
        if (equipmentDetailText != null)
        {
            equipmentDetailText.text = detailText;
        }

        UpdateSetBonusPanel(itemInstance);

        if (equipmentDetailCanvasGroup != null)
        {
            equipmentDetailCanvasGroup.alpha = 1f;
            equipmentDetailCanvasGroup.blocksRaycasts = true;
            equipmentDetailCanvasGroup.interactable = true;
        }
        else if (equipmentDetailPanel != null && !equipmentDetailPanel.activeSelf)
        {
            equipmentDetailPanel.SetActive(true);
        }
    }

    private void HideEquipmentDetailPanel()
    {
        if (equipmentDetailCanvasGroup != null)
        {
            equipmentDetailCanvasGroup.alpha = 0f;
            equipmentDetailCanvasGroup.blocksRaycasts = false;
            equipmentDetailCanvasGroup.interactable = false;
        }
        else if (equipmentDetailPanel != null)
        {
            equipmentDetailPanel.SetActive(false);
        }

        if (equipmentDetailText != null)
        {
            equipmentDetailText.text = string.Empty;
        }

        HideSetBonusPanel();
    }

    private void UpdateSetBonusPanel(ItemInstance itemInstance)
    {
        if (!HasSetBonusUI())
            return;

        var setData = FindSetDataForItem(itemInstance);
        if (setData == null)
        {
            HideSetBonusPanel();
            return;
        }

        int equippedPieces;
        string piecesText = BuildSetPiecesText(setData, itemInstance, out equippedPieces);
        if (setNameAndPiecesText != null)
        {
            setNameAndPiecesText.text = piecesText;
        }

        if (setBonusesText != null)
        {
            setBonusesText.text = BuildSetBonusesText(setData, equippedPieces);
        }

        ShowSetBonusPanel();
    }

    private bool HasSetBonusUI()
    {
        return setBonusPanel != null ||
               setBonusPanelCanvasGroup != null ||
               setNameAndPiecesText != null ||
               setBonusesText != null;
    }

    private void ShowSetBonusPanel()
    {
        if (setBonusPanelCanvasGroup != null)
        {
            setBonusPanelCanvasGroup.alpha = 1f;
            setBonusPanelCanvasGroup.blocksRaycasts = true;
            setBonusPanelCanvasGroup.interactable = true;
        }
        else if (setBonusPanel != null)
        {
            setBonusPanel.SetActive(true);
        }
    }

    private void HideSetBonusPanel()
    {
        if (setBonusPanelCanvasGroup != null)
        {
            setBonusPanelCanvasGroup.alpha = 0f;
            setBonusPanelCanvasGroup.blocksRaycasts = false;
            setBonusPanelCanvasGroup.interactable = false;
        }
        else if (setBonusPanel != null)
        {
            setBonusPanel.SetActive(false);
        }

        if (setNameAndPiecesText != null)
        {
            setNameAndPiecesText.text = string.Empty;
        }

        if (setBonusesText != null)
        {
            setBonusesText.text = string.Empty;
        }
    }

    private SetBonusesData FindSetDataForItem(ItemInstance itemInstance)
    {
        if (itemInstance?.baseItem == null || SetBonusManager.Instance == null)
            return null;

        string identifier = itemInstance.baseItem.itemName;
        foreach (var setData in SetBonusManager.Instance.allSetBonuses)
        {
            if (setData == null)
                continue;

            if (setData.MatchesIdentifier(identifier) && ItemMeetsSetRequirements(setData, itemInstance))
            {
                return setData;
            }
        }

        return null;
    }

    private string BuildSetPiecesText(SetBonusesData setData, ItemInstance sourceItem, out int equippedPieces)
    {
        equippedPieces = 0;
        List<string> pieceNames = new List<string>();

        if (equipmentManager != null)
        {
            foreach (EquipmentManager.EquipmentSlotType slot in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
            {
                ItemInstance equippedItem = equipmentManager.GetEquippedItem(slot);
                if (ItemMatchesSet(setData, equippedItem))
                {
                    equippedPieces++;
                    pieceNames.Add(equippedItem.GetItemName());
                }
            }
        }
        else if (ItemMatchesSet(setData, sourceItem))
        {
            equippedPieces = 1;
            pieceNames.Add(sourceItem.GetItemName());
        }

        string header = $"{setData.setName} ({equippedPieces} piezas)";
        string piecesList = pieceNames.Count > 0 ? string.Join(", ", pieceNames.Distinct()) : "Sin piezas equipadas de este set";
        return $"{header}\n{piecesList}";
    }

    private string BuildSetBonusesText(SetBonusesData setData, int equippedPieces)
    {
        if (setData == null || setData.bonuses == null || setData.bonuses.Count == 0)
            return "No hay bonificaciones registradas para este set.";

        StringBuilder sb = new StringBuilder();
        var orderedBonuses = setData.bonuses.OrderBy(b => b.minPieces);

        foreach (var bonus in orderedBonuses)
        {
            if (bonus == null)
                continue;

            bool isActive = equippedPieces >= bonus.minPieces;
            string statsText = FormatItemStats(bonus.bonusStats);
            string lineContent = $"{bonus.minPieces} piezas: {statsText}";

            if (!string.IsNullOrEmpty(bonus.description))
            {
                lineContent += $" ({bonus.description})";
            }

            if (isActive)
            {
                sb.AppendLine($"<color=#6CFF9C>{lineContent}</color>");
            }
            else
            {
                sb.AppendLine(lineContent);
            }
        }

        return sb.ToString();
    }

    private bool ItemMatchesSet(SetBonusesData setData, ItemInstance item)
    {
        if (setData == null || item?.baseItem == null)
            return false;

        return setData.MatchesIdentifier(item.baseItem.itemName) && ItemMeetsSetRequirements(setData, item);
    }

    private bool ItemMeetsSetRequirements(SetBonusesData setData, ItemInstance item)
    {
        if (setData == null || item == null)
            return false;

        string itemRarity = item.GetRarity();
        if (string.IsNullOrEmpty(setData.minRarity))
            return true;

        return GetRarityRank(itemRarity) >= GetRarityRank(setData.minRarity);
    }

    private int GetRarityRank(string rarity)
    {
        if (string.IsNullOrEmpty(rarity))
            return 0;

        string rarityLower = rarity.ToLowerInvariant();

        if (rarityLower.Contains("plebeius") || rarityLower.Contains("comun") || rarityLower.Contains("común"))
            return 1;
        if (rarityLower.Contains("auxiliaris") || rarityLower.Contains("magico") || rarityLower.Contains("mágico"))
            return 2;
        if (rarityLower.Contains("legionarius") || rarityLower.Contains("raro") || rarityLower.Contains("rara"))
            return 3;
        if (rarityLower.Contains("veteranus") || rarityLower.Contains("excelente"))
            return 4;
        if (rarityLower.Contains("centurio") || rarityLower.Contains("legendario") || rarityLower.Contains("legendaria"))
            return 5;
        if (rarityLower.Contains("tribunus") || rarityLower.Contains("epico") || rarityLower.Contains("épico") || rarityLower.Contains("epica") || rarityLower.Contains("épica"))
            return 6;
        if (rarityLower.Contains("praetorianus") || rarityLower.Contains("extremo") || rarityLower.Contains("extrema"))
            return 7;
        if (rarityLower.Contains("imperialis") || rarityLower.Contains("demoniaco") || rarityLower.Contains("demoníaco") || rarityLower.Contains("demoniaca") || rarityLower.Contains("demoníaca"))
            return 8;
        if (rarityLower.Contains("augustus") || rarityLower.Contains("celestial"))
            return 9;
        if (rarityLower.Contains("divinus") || rarityLower.Contains("etereo") || rarityLower.Contains("etéreo"))
            return 10;

        return 0;
    }

    private string FormatItemStats(ItemStats stats)
    {
        List<string> parts = new List<string>();

        AppendStatText(parts, "HP", stats.hp);
        AppendStatText(parts, "Mana", stats.mana);
        AppendStatText(parts, "Ataque", stats.ataque);
        AppendStatText(parts, "Defensa", stats.defensa);
        AppendStatText(parts, "Velocidad", stats.velocidadAtaque);
        AppendStatText(parts, "Atk Crítico", stats.ataqueCritico);
        AppendStatText(parts, "Daño Crítico", stats.danoCritico);
        AppendStatText(parts, "Suerte", stats.suerte);
        AppendStatText(parts, "Destreza", stats.destreza);

        return parts.Count > 0 ? string.Join(", ", parts) : "Sin bonificación";
    }

    private void AppendStatText(List<string> parts, string label, int value)
    {
        if (value == 0)
            return;

        parts.Add($"{label} +{value}");
    }

    private const string DetailLabelSeparator = " :    ";

    private string BuildDetailText(ItemInstance itemInstance)
    {
        StringBuilder sb = new StringBuilder();

        string itemName = itemInstance.GetItemName();
        string itemType = GetItemTypeDisplayName(itemInstance);
        string rarityKey = itemInstance.GetRarity();
        string rarityDisplay = !string.IsNullOrEmpty(rarityKey)
            ? RarityColorProvider.GetDisplayName(rarityKey)
            : "-";
        string rarityHex = null;
        if (!string.IsNullOrEmpty(rarityKey))
        {
            rarityHex = RarityColorProvider.GetColorHex(rarityKey);
        }

        ItemStats stats = itemInstance.GetFinalStats();
        int price = CalculateItemPrice(itemInstance);

        string coloredName = !string.IsNullOrEmpty(rarityHex)
            ? $"<color=#{rarityHex}>{itemName}</color>"
            : itemName;

        sb.AppendLine(FormatDetailLine("Nombre", coloredName));
        sb.AppendLine(FormatDetailLine("Calidad", rarityDisplay));
        sb.AppendLine(FormatDetailLine("Tipo", itemType));
        sb.AppendLine(FormatDetailLine("Nivel", NumberFormatter.FormatNumber(itemInstance.currentLevel)));

        string rarityValue = !string.IsNullOrEmpty(rarityHex)
            ? $"<color=#{rarityHex}>{rarityDisplay}</color>"
            : rarityDisplay;
        sb.AppendLine(FormatDetailLine("Rareza", rarityValue));

        AppendStatLineIfPositive(sb, "Ataque", stats.ataque);
        AppendStatLineIfPositive(sb, "Defensa", stats.defensa);
        AppendStatLineIfPositive(sb, "Velocidad Ataque", stats.velocidadAtaque);
        AppendStatLineIfPositive(sb, "Ataque Crítico", stats.ataqueCritico);
        AppendStatLineIfPositive(sb, "Daño Crítico", stats.danoCritico);
        AppendStatLineIfPositive(sb, "Suerte", stats.suerte);
        AppendStatLineIfPositive(sb, "Destreza", stats.destreza);
        sb.Append(FormatDetailLine("Precio", NumberFormatter.FormatNumber(price)));

        return sb.ToString();
    }

    private string FormatNumberWithSign(int value)
    {
        if (value > 0)
        {
            return $"+{NumberFormatter.FormatNumber(value)}";
        }

        return NumberFormatter.FormatNumber(value);
    }

    private void AppendStatLineIfPositive(StringBuilder sb, string label, int value)
    {
        if (value <= 0)
            return;

        sb.AppendLine(FormatDetailLine(label, FormatNumberWithSign(value)));
    }

    private string FormatDetailLine(string label, string value)
    {
        string safeValue = string.IsNullOrEmpty(value) ? "-" : value;
        return $"{label}{DetailLabelSeparator}{safeValue}";
    }

    private string GetItemTypeDisplayName(ItemInstance itemInstance)
    {
        if (itemInstance?.baseItem == null)
            return "-";

        ItemData baseItem = itemInstance.baseItem;
        string itemNameLower = baseItem.itemName?.ToLowerInvariant() ?? string.Empty;

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

        switch (baseItem.itemType)
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
            default:
                return "Otros";
        }
    }

    private int CalculateItemPrice(ItemInstance itemInstance)
    {
        if (itemInstance == null || !itemInstance.IsValid())
            return 0;

        ItemData baseItem = itemInstance.baseItem;
        if (baseItem == null)
            return 0;

        int baseSellPrice = Mathf.Max(1, baseItem.price / 2);
        float levelMultiplier = 1f + ((itemInstance.currentLevel - 1) * 0.05f);
        int finalPrice = Mathf.RoundToInt(baseSellPrice * levelMultiplier);
        return Mathf.Max(1, finalPrice);
    }

    public void OnEquipmentSlotPointerDown(EquipmentManager.EquipmentSlotType slotType, HeroProfileEquipmentSlotButton sourceButton)
    {
        if (sourceButton == null)
            return;

        if (activeSlotButton != null && activeSlotButton != sourceButton)
        {
            OnEquipmentSlotPointerUp(activeSlotButton);
        }

        ItemInstance equippedItem = equipmentManager != null ? equipmentManager.GetEquippedItem(slotType) : null;
        if (equippedItem == null || !equippedItem.IsValid())
        {
            HideEquipmentDetailPanel();
            activeSlotButton = null;
            return;
        }

        activeSlotButton = sourceButton;
        ShowEquipmentDetailPanel(equippedItem);
    }

    public void OnEquipmentSlotPointerUp(HeroProfileEquipmentSlotButton sourceButton)
    {
        if (sourceButton == null)
            return;

        if (activeSlotButton == sourceButton)
        {
            activeSlotButton = null;
            HideEquipmentDetailPanel();
        }
    }

    /// <summary>
    /// Se llama cuando cambia el equipo (equipar/desequipar).
    /// </summary>
    private void OnEquipmentChanged()
    {
        RefreshAllEquipmentSlots();
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
    /// Se llama cuando se mejora un item. Actualiza el slot correspondiente si el item está equipado.
    /// </summary>
    private void OnItemImproved(ItemInstance itemInstance, int previousLevel, int newLevel)
    {
        if (itemInstance == null || !itemInstance.IsValid() || equipmentManager == null)
            return;

        // SOLUCIÓN: Siempre refrescar todos los slots de equipo cuando se mejora un item
        // Esto asegura que si el panel está activo, se actualice inmediatamente
        // Y si el panel está inactivo, se actualizará cuando se abra (OnEnable llama a RefreshAllEquipmentSlots)
        RefreshAllEquipmentSlots();
        RefreshHeroStats();
        
        // También buscar específicamente el slot donde está equipado para forzar actualización
        // CORRECCIÓN: Iterar sobre todos los slots disponibles (10 en lugar de 6)
        foreach (EquipmentManager.EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
        {
            ItemInstance equippedItem = equipmentManager.GetEquippedItem(slotType);
            
            // Si el item mejorado está equipado en este slot, forzar actualización específica
            if (equippedItem != null && equippedItem.IsValid() && equippedItem.IsSameInstance(itemInstance))
            {
                // SOLUCIÓN: Leer el item actualizado directamente desde EquipmentManager
                // Esto asegura que tenemos la referencia más reciente con el nivel actualizado
                ItemInstance updatedItem = equipmentManager.GetEquippedItem(slotType);
                
                // Forzar actualización del slot para mostrar el nuevo nivel
                UpdateEquipmentSlot(slotType, updatedItem, forceRefresh: true);
                
                // Forzar actualización del canvas después de un frame
                StartCoroutine(ForceUpdateAfterFrame());
                break;
            }
        }
    }

    /// <summary>
    /// Fuerza actualización del canvas después de un frame para asegurar que los cambios se rendericen.
    /// </summary>
    private IEnumerator ForceUpdateAfterFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
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
            case EquipmentManager.EquipmentSlotType.Montura:
                slotImage = monturaImage;
                slotLevelText = monturaLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Casco:
                slotImage = cascoImage;
                slotLevelText = cascoLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Collar:
                slotImage = collarImage;
                slotLevelText = collarLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Arma:
                slotImage = armaImage;
                slotLevelText = armaLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Armadura:
                slotImage = armaduraImage;
                slotLevelText = armaduraLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Escudo:
                slotImage = escudoImage;
                slotLevelText = escudoLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Guantes:
                slotImage = guantesImage;
                slotLevelText = guantesLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Cinturon:
                slotImage = cinturonImage;
                slotLevelText = cinturonLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Anillo:
                slotImage = anilloImage;
                slotLevelText = anilloLevelText;
                break;
            case EquipmentManager.EquipmentSlotType.Botas:
                slotImage = botasImage;
                slotLevelText = botasLevelText;
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
                // Slot vacío: restaurar el sprite por defecto del Image en lugar de poner null
                if (defaultSlotSprites.ContainsKey(slotImage))
                {
                    // Restaurar el sprite por defecto que tenía el Image al inicio
                    slotImage.sprite = defaultSlotSprites[slotImage];
                }
                else
                {
                    // Si no se guardó un sprite por defecto, no modificar el sprite actual
                    // Esto permite que el sprite configurado en Unity se mantenga
                    // No poner null para evitar que se borre el sprite del Image
                    slotImage.sprite = null;
                }
                slotImage.enabled = true; // Mantener habilitado para mostrar sprite por defecto
            }
        }

        // Actualizar Text del nivel
        if (slotLevelText != null)
        {
            if (itemInstance != null && itemInstance.IsValid())
            {
                // SOLUCIÓN: Asegurar que el TextMeshProUGUI esté activo y habilitado antes de actualizar
                // Esto es crítico cuando el panel estaba inactivo durante la mejora del item
                if (!slotLevelText.gameObject.activeSelf)
                {
                    slotLevelText.gameObject.SetActive(true);
                }
                if (!slotLevelText.enabled)
                {
                    slotLevelText.enabled = true;
                }
                
                // SOLUCIÓN: Usar siempre el nivel actual del ItemInstance directamente
                // EquipmentManager tiene la referencia actualizada al ItemInstance, así que siempre será correcto
                // El perfil guardado es solo para persistencia, pero para mostrar usamos el ItemInstance actual
                slotLevelText.text = $"Nv. {itemInstance.currentLevel}";
                
                // Forzar actualización del TextMeshProUGUI para asegurar que se renderice
                // Solo funciona si el componente está activo y habilitado
                if (slotLevelText.isActiveAndEnabled)
                {
                    slotLevelText.ForceMeshUpdate();
                    Canvas.ForceUpdateCanvases();
                }
            }
            else
            {
                slotLevelText.text = "";
                slotLevelText.gameObject.SetActive(false);
            }
        }
        
        if (activeSlotButton != null && activeSlotButton.SlotType == slot)
        {
            if (itemInstance != null && itemInstance.IsValid())
            {
                ShowEquipmentDetailPanel(itemInstance);
            }
            else
            {
                HideEquipmentDetailPanel();
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
        // Orden: Montura, Casco, Collar, Arma, Armadura, Escudo, Guantes, Cinturon, Anillo, Botas
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Montura, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Montura), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Casco, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Casco), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Collar, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Collar), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Arma, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Arma), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Armadura, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Armadura), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Escudo, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Escudo), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Guantes, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Guantes), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Cinturon, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Cinturon), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Anillo, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Anillo), forceRefresh: true);
        UpdateEquipmentSlot(EquipmentManager.EquipmentSlotType.Botas, 
            equipmentManager.GetEquippedItem(EquipmentManager.EquipmentSlotType.Botas), forceRefresh: true);
        
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
    /// Inicia la actualización continua del tiempo jugado.
    /// </summary>
    private void StartTimeUpdate()
    {
        if (timeUpdateCoroutine != null)
        {
            StopCoroutine(timeUpdateCoroutine);
        }
        timeUpdateCoroutine = StartCoroutine(UpdateTimeContinuously());
    }

    /// <summary>
    /// Corrutina que actualiza el tiempo jugado cada segundo.
    /// </summary>
    private IEnumerator UpdateTimeContinuously()
    {
        while (true)
        {
            // Actualizar el tiempo
            UpdateTotalPlayTime(totalPlayTimeSeconds);
            
            // Incrementar el tiempo si está habilitado
            if (autoIncrementTime)
            {
                totalPlayTimeSeconds += 1f;
            }
            
            // Esperar 1 segundo
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Actualiza el texto del tiempo jugado total con el formato especificado.
    /// Formato: HH:MM:SS hasta 24h, luego D HH:MM:SS, luego M D HH:MM:SS, luego A M D HH:MM:SS
    /// </summary>
    /// <param name="totalPlayTimeSeconds">Tiempo total jugado en segundos</param>
    public void UpdateTotalPlayTime(float totalPlayTimeSeconds)
    {
        if (totalPlayTimeText != null)
        {
            totalPlayTimeText.text = FormatPlayTime(totalPlayTimeSeconds);
        }
    }

    /// <summary>
    /// Formatea el tiempo jugado según las reglas:
    /// - Hasta 24 horas: HH:MM:SS
    /// - Después de 24 horas: D HH:MM:SS (días)
    /// - Después de 30 días: M D HH:MM:SS (meses)
    /// - Después de 12 meses: A M D HH:MM:SS (años)
    /// </summary>
    private string FormatPlayTime(float totalSeconds)
    {
        int totalSecondsInt = Mathf.FloorToInt(totalSeconds);
        
        // Calcular años (12 meses = 1 año)
        int secondsPerMonth = 30 * 24 * 3600; // 30 días en segundos
        int secondsPerYear = 12 * secondsPerMonth; // 12 meses en segundos
        int years = totalSecondsInt / secondsPerYear;
        totalSecondsInt %= secondsPerYear;
        
        // Calcular meses (30 días = 1 mes)
        int months = totalSecondsInt / secondsPerMonth;
        totalSecondsInt %= secondsPerMonth;
        
        // Calcular días (24 horas = 1 día)
        int secondsPerDay = 24 * 3600;
        int days = totalSecondsInt / secondsPerDay;
        totalSecondsInt %= secondsPerDay;
        
        // Calcular horas, minutos y segundos
        int hours = totalSecondsInt / 3600;
        totalSecondsInt %= 3600;
        int minutes = totalSecondsInt / 60;
        int seconds = totalSecondsInt % 60;
        
        // Formatear según las reglas
        if (years > 0)
        {
            // A M D HH:MM:SS
            return $"{years}A {months}M {days}D {hours:00}:{minutes:00}:{seconds:00}";
        }
        else if (months > 0)
        {
            // M D HH:MM:SS
            return $"{months}M {days}D {hours:00}:{minutes:00}:{seconds:00}";
        }
        else if (days > 0)
        {
            // D HH:MM:SS
            return $"{days}D {hours:00}:{minutes:00}:{seconds:00}";
        }
        else
        {
            // HH:MM:SS
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
    }

    /// <summary>
    /// Establece el tiempo total jugado (desde el sistema de guardado).
    /// </summary>
    public void SetTotalPlayTime(float seconds)
    {
        totalPlayTimeSeconds = seconds;
        if (totalPlayTimeText != null)
        {
            UpdateTotalPlayTime(totalPlayTimeSeconds);
        }
    }

    /// <summary>
    /// Obtiene el tiempo total jugado en segundos.
    /// </summary>
    public float GetTotalPlayTime()
    {
        return totalPlayTimeSeconds;
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
        ItemStats heroProgressionStats = new ItemStats();

        // Obtener mejoras de gimnasio desde PlayerProfileData
        int gymHPBonus = 0;
        int gymManaBonus = 0;
        int gymAttackBonus = 0;
        int gymDefenseBonus = 0;
        int gymSkillBonus = 0;
        int gymCriticalAttackBonus = 0;
        int gymCriticalDamageBonus = 0;
        int gymAttackSpeedBonus = 0;
        int gymLuckBonus = 0;
        
        if (GameDataManager.Instance != null)
        {
            PlayerProfileData profile = GameDataManager.Instance.GetPlayerProfile();
            if (profile != null)
            {
                gymHPBonus = profile.gymHPTotalBonus;
                gymManaBonus = profile.gymManaTotalBonus;
                gymAttackBonus = profile.gymAttackTotalBonus;
                gymDefenseBonus = profile.gymDefenseTotalBonus;
                gymSkillBonus = profile.gymSkillTotalBonus;
                gymCriticalAttackBonus = profile.gymCriticalAttackTotalBonus;
                gymCriticalDamageBonus = profile.gymCriticalDamageTotalBonus;
                gymAttackSpeedBonus = profile.gymAttackSpeedTotalBonus;
                gymLuckBonus = profile.gymLuckTotalBonus;
                heroProgressionStats = profile.GetHeroProgressionStats();
            }
        }

        // Calcular stats totales (base del héroe + equipo + mejoras de gimnasio + progresión)
        int totalHP = baseHP + equipmentStats.hp + gymHPBonus + heroProgressionStats.hp;
        int totalMana = baseMana + equipmentStats.mana + gymManaBonus + heroProgressionStats.mana;
        int totalAtaque = baseAtaque + equipmentStats.ataque + gymAttackBonus + heroProgressionStats.ataque;
        int totalDefensa = baseDefensa + equipmentStats.defensa + gymDefenseBonus + heroProgressionStats.defensa;
        int totalVelocidadAtaque = baseVelocidadAtaque + equipmentStats.velocidadAtaque + gymAttackSpeedBonus + heroProgressionStats.velocidadAtaque;
        int totalAtaqueCritico = baseAtaqueCritico + equipmentStats.ataqueCritico + gymCriticalAttackBonus + heroProgressionStats.ataqueCritico;
        int totalDanoCritico = baseDanoCritico + equipmentStats.danoCritico + gymCriticalDamageBonus + heroProgressionStats.danoCritico;
        int totalSuerte = baseSuerte + equipmentStats.suerte + gymLuckBonus + heroProgressionStats.suerte;
        int totalDestreza = baseDestreza + equipmentStats.destreza + gymSkillBonus + heroProgressionStats.destreza;

        // Actualizar textos usando NumberFormatter para números grandes
        if (hpText != null)
            hpText.text = NumberFormatter.FormatNumber(totalHP);
        
        if (manaText != null)
            manaText.text = NumberFormatter.FormatNumber(totalMana);
        
        if (ataqueText != null)
            ataqueText.text = NumberFormatter.FormatNumber(totalAtaque);
        
        if (defensaText != null)
            defensaText.text = NumberFormatter.FormatNumber(totalDefensa);
        
        if (velocidadAtaqueText != null)
            velocidadAtaqueText.text = NumberFormatter.FormatNumber(totalVelocidadAtaque);
        
        if (ataqueCriticoText != null)
            ataqueCriticoText.text = NumberFormatter.FormatNumber(totalAtaqueCritico);
        
        if (danoCriticoText != null)
            danoCriticoText.text = NumberFormatter.FormatNumber(totalDanoCritico);
        
        if (suerteText != null)
            suerteText.text = NumberFormatter.FormatNumber(totalSuerte);
        
        if (destrezaText != null)
            destrezaText.text = NumberFormatter.FormatNumber(totalDestreza);
    }

    /// <summary>
    /// Refresca las estadísticas del jugador (enfrentamientos, cofres, peleas ganadas/perdidas).
    /// Puede ser llamado desde GameDataManager cuando se actualizan las estadísticas.
    /// </summary>
    public void RefreshStatistics()
    {
        if (GameDataManager.Instance == null)
            return;

        PlayerProfileData profile = GameDataManager.Instance.GetPlayerProfile();
        if (profile == null)
            return;

        // Actualizar total de enfrentamientos
        if (totalClashesText != null)
        {
            totalClashesText.text = profile.totalClashes.ToString();
        }

        // Actualizar total de cofres abiertos
        if (totalOpenChestsText != null)
        {
            totalOpenChestsText.text = profile.totalOpenChests.ToString();
        }

        // Actualizar total de peleas ganadas
        if (totalWonFightsText != null)
        {
            totalWonFightsText.text = profile.totalWonFights.ToString();
        }

        // Actualizar total de peleas perdidas
        if (totalLostFightsText != null)
        {
            totalLostFightsText.text = profile.totalLostFights.ToString();
        }

        // Actualizar título de clase
        RefreshClassTitle();
    }

    /// <summary>
    /// Actualiza el título de clase del héroe.
    /// </summary>
    private void RefreshClassTitle()
    {
        if (GameDataManager.Instance == null || classTitleText == null)
            return;

        PlayerProfileData profile = GameDataManager.Instance.GetPlayerProfile();
        if (profile == null)
            return;

        // Usar el nuevo sistema basado en nivel
        string classTitle = EvolutionSystem.GetClassNameByLevel(profile.heroLevel);
        classTitleText.text = classTitle;
    }

    /// <summary>
    /// Refresca la UI de experiencia del héroe.
    /// </summary>
    public void RefreshHeroExperience()
    {
        if (GameDataManager.Instance == null)
            return;

        PlayerProfileData profile = GameDataManager.Instance.GetPlayerProfile();
        if (profile == null)
            return;

        int currentExp = profile.heroExperience;
        int neededExp = profile.GetExperienceNeededForNextLevel();
        int level = profile.heroLevel;

        // Actualizar slider
        if (heroExperienceSlider != null)
        {
            heroExperienceSlider.minValue = 0;
            heroExperienceSlider.maxValue = neededExp;
            heroExperienceSlider.value = currentExp;
        }

        // Actualizar texto de experiencia
        if (heroExperienceText != null)
        {
            heroExperienceText.text = $"{currentExp}/{neededExp}";
        }

        // Actualizar texto de nivel
        if (heroLevelText != null)
        {
            heroLevelText.text = $"Nivel {level}";
        }

        // Actualizar título de clase (por si evolucionó)
        RefreshClassTitle();
    }

    /// <summary>
    /// Guarda los sprites por defecto de todos los slots de equipo.
    /// Estos sprites se restaurarán cuando un slot esté vacío.
    /// </summary>
    private void SaveDefaultSlotSprites()
    {
        Image[] equipmentImages = { 
            monturaImage, cascoImage, collarImage, armaImage, armaduraImage, 
            escudoImage, guantesImage, cinturonImage, anilloImage, botasImage 
        };
        
        foreach (Image slotImage in equipmentImages)
        {
            if (slotImage != null && slotImage.sprite != null)
            {
                // Guardar el sprite por defecto del Image
                defaultSlotSprites[slotImage] = slotImage.sprite;
            }
        }
    }
}

