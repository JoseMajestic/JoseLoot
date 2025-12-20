using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Gestiona el sistema de crianza/breed del héroe.
/// Maneja las 6 stats de crianza, las 8 acciones, decaimiento automático,
/// mensajes del héroe, títulos y evolución.
/// </summary>
public class BreedManager : MonoBehaviour
{
    // ===== REFERENCIAS =====
    private GameDataManager gameDataManager;
    private EnergySystem energySystem;
    
    // ===== TEXTOS Y PANELES =====
    [Header("Textos y Paneles")]
    [Tooltip("Título de clase (Primera, Segunda, etc.)")]
    [SerializeField] private TextMeshProUGUI classTitleText;
    
    [Tooltip("Panel con imagen del espectro de voz")]
    [SerializeField] private GameObject voiceSpectrumPanel;
    
    [Tooltip("Botón ON/OFF de voz")]
    [SerializeField] private Button voiceToggleButton;
    
    [Tooltip("Panel donde se visualiza la imagen del héroe")]
    [SerializeField] private GameObject heroImagePanel;
    
    [Tooltip("Texto para mensajes del héroe (cada 15 segundos, typewriter)")]
    [SerializeField] private TextMeshProUGUI messageText;
    
    [Tooltip("Cooldown de evolución")]
    [SerializeField] private TextMeshProUGUI evolutionCooldownText;
    
    [Tooltip("Tiempo de vida total")]
    [SerializeField] private TextMeshProUGUI lifeTimeText;
    
    [Tooltip("Estado General (título)")]
    [SerializeField] private TextMeshProUGUI generalStateText;
    
    // ===== SLIDERS Y TEXTOS DE STATS =====
    [Header("Stats de Crianza")]
    [Tooltip("Slider de Trabajo")]
    [SerializeField] private Slider workSlider;
    
    [Tooltip("Texto de Trabajo (formato: actual% / máximo 100%)")]
    [SerializeField] private TextMeshProUGUI workText;
    
    [Tooltip("Slider de Hambre")]
    [SerializeField] private Slider hungerSlider;
    
    [Tooltip("Texto de Hambre (formato: actual% / máximo 100%)")]
    [SerializeField] private TextMeshProUGUI hungerText;
    
    [Tooltip("Slider de Felicidad")]
    [SerializeField] private Slider happinessSlider;
    
    [Tooltip("Texto de Felicidad (formato: actual% / máximo 100%)")]
    [SerializeField] private TextMeshProUGUI happinessText;
    
    [Tooltip("Slider de Energía")]
    [SerializeField] private Slider energySlider;
    
    [Tooltip("Texto de Energía (formato: actual% / máximo 100%)")]
    [SerializeField] private TextMeshProUGUI energyText;
    
    [Tooltip("Slider de Higiene")]
    [SerializeField] private Slider hygieneSlider;
    
    [Tooltip("Texto de Higiene (formato: actual% / máximo 100%)")]
    [SerializeField] private TextMeshProUGUI hygieneText;
    
    [Tooltip("Slider de Disciplina")]
    [SerializeField] private Slider disciplineSlider;
    
    [Tooltip("Texto de Disciplina (formato: actual% / máximo 100%)")]
    [SerializeField] private TextMeshProUGUI disciplineText;
    
    // ===== BOTONES DE ACCIÓN =====
    [Header("Botones de Acción")]
    [Tooltip("Botón Comer")]
    [SerializeField] private Button eatButton;
    
    [Tooltip("Botón Estudio")]
    [SerializeField] private Button studyButton;
    
    [Tooltip("Botón Dormir")]
    [SerializeField] private Button sleepButton;
    
    [Tooltip("Botón Jugar")]
    [SerializeField] private Button playButton;
    
    [Tooltip("Botón Gimnasio (abre panel Upgrade)")]
    [SerializeField] private Button gymButton;
    
    [Tooltip("Botón Evolucionar")]
    [SerializeField] private Button evoButton;
    
    [Tooltip("Botón Limpiar")]
    [SerializeField] private Button cleanButton;
    
    [Tooltip("Botón Trabajar")]
    [SerializeField] private Button workButton;
    
    [Tooltip("Botón Reset (abre panel de confirmación)")]
    [SerializeField] private Button resetButton;
    
    // ===== PANELES DE VIDEO DE ACCIONES =====
    [Header("Paneles de Video de Acciones")]
    [Tooltip("Panel de video para la acción Comer")]
    [SerializeField] private GameObject eatVideoPanel;
    
    [Tooltip("Panel de video para la acción Estudiar")]
    [SerializeField] private GameObject studyVideoPanel;
    
    [Tooltip("Panel de video para la acción Dormir (se mantiene visible mientras duerme)")]
    [SerializeField] private GameObject sleepVideoPanel;
    
    [Tooltip("Panel de video para la acción Jugar")]
    [SerializeField] private GameObject playVideoPanel;
    
    [Tooltip("Panel de video para la acción Evolucionar")]
    [SerializeField] private GameObject evoVideoPanel;
    
    [Tooltip("Panel de video para la acción Limpiar")]
    [SerializeField] private GameObject cleanVideoPanel;
    
    [Tooltip("Panel de video para la acción Trabajar")]
    [SerializeField] private GameObject workVideoPanel;
    
    [Header("Fade para Paneles de Video")]
    [Tooltip("Image negro que cubre toda la pantalla durante las transiciones de video (debe estar en un Canvas con orden superior)")]
    [SerializeField] private UnityEngine.UI.Image videoFadeOverlay;
    
    [Tooltip("Duración del fade in/out para paneles de video en segundos")]
    [SerializeField] private float videoFadeDuration = 0.3f;
    
    [Tooltip("Si es true, usa fade al activar/desactivar paneles de video. Si es false, cambio directo sin fade")]
    [SerializeField] private bool useVideoFade = true;
    
    // Referencia al panel de video actualmente activo (excepto dormir que es especial)
    private GameObject currentActiveVideoPanel = null;
    
    [Header("Visualizador de Audio")]
    [Tooltip("Visualizador de espectro de audio para los mensajes del héroe")]
    [SerializeField] private AudioSpectrumVisualizer audioVisualizer;
    
    // ===== PANELES =====
    [Header("Paneles")]
    [Tooltip("Panel de confirmación de reset")]
    [SerializeField] private GameObject resetConfirmPanel;
    
    [Tooltip("Botón Aceptar reset")]
    [SerializeField] private Button resetAcceptButton;
    
    [Tooltip("Botón Cancelar reset")]
    [SerializeField] private Button resetCancelButton;
    
    [Header("Panel de Gimnasio")]
    [Tooltip("Panel General Upgrade (se abre desde gymButton)")]
    [SerializeField] private GameObject upgradePanel;
    
    [Header("Panel de Animaciones")]
    [Tooltip("Panel que se abrirá cuando se abra el panel General Breed y se cerrará cuando éste se cierre")]
    [SerializeField] private GameObject animationPanel;
    
    [Tooltip("Referencia al panel General Breed (el GameObject donde está este script). Si no se asigna, se usa el GameObject actual")]
    [SerializeField] private GameObject panelGeneralBreed;
    
    [Tooltip("Sprite que se mostrará en el panel de animaciones")]
    [SerializeField] private Sprite animationSprite;
    
    [System.Serializable]
    public class AnimationConfig
    {
        [Tooltip("Clip de animación")]
        public AnimationClip clip;
        
        [Tooltip("Controller de animación")]
        public RuntimeAnimatorController controller;
    }
    
    [Tooltip("Array de animaciones idle que se ejecutarán aleatoriamente una detrás de otra")]
    [SerializeField] private AnimationConfig[] idleAnimations = new AnimationConfig[0];
    
    [Tooltip("GameObject con Animator donde se reproducirán las animaciones")]
    [SerializeField] private GameObject animationTarget;
    
    // ===== SISTEMA DE MENSAJES BASADO EN ESTADOS =====
    
    /// <summary>
    /// Estados globales del héroe basados en stats.
    /// </summary>
    public enum GlobalState
    {
        Excelencia,  // Estado S: todos los parámetros (excepto energía) >= 90%
        Fortaleza,   // Estado A: todo > 60%
        Tension,     // Estado B: alguno entre 40-60%
        Agonia,      // Estado C: alguno entre 20-40%
        Colapso      // Estado D: alguno < 20%
    }
    
    /// <summary>
    /// Carencias dominantes detectadas.
    /// </summary>
    public enum DominantDeficiency
    {
        Trabajo,
        Hambre,
        Felicidad,
        Energia,
        Higiene,
        Disciplina,
        Multiple     // Cuando hay múltiples carencias críticas
    }
    
    [System.Serializable]
    public class MessageStateBlock
    {
        [Tooltip("Frases para este estado")]
        public string[] messages = new string[0];
    }
    
    [Header("Mensajes por Estado")]
    [Tooltip("Frases para Estado S - Excelencia (todos los parámetros excepto energía >= 90%)")]
    [SerializeField] private MessageStateBlock estadoExcelencia = new MessageStateBlock();
    
    [Tooltip("Frases para Estado A - Fortaleza (todo > 60%)")]
    [SerializeField] private MessageStateBlock estadoFortaleza = new MessageStateBlock();
    
    [Tooltip("Frases para Estado B - Tensión (alguno 40-60%)")]
    [SerializeField] private MessageStateBlock estadoTension = new MessageStateBlock();
    
    [Tooltip("Frases para Estado C - Agonía (alguno 20-40%)")]
    [SerializeField] private MessageStateBlock estadoAgonia = new MessageStateBlock();
    
    [Tooltip("Frases para Estado D - Colapso (alguno < 20%)")]
    [SerializeField] private MessageStateBlock estadoColapso = new MessageStateBlock();
    
    // Sistema de pool para evitar repeticiones
    private HashSet<int> usedMessageIndices = new HashSet<int>();
    private GlobalState lastGlobalState = GlobalState.Excelencia;
    private DominantDeficiency lastDeficiency = DominantDeficiency.Trabajo;
    private string lastDisplayedMessage = ""; // Para detectar cuando cambia el mensaje

    private const string SleepingMessage = "El Cazador está durmiendo...";
    
    // ===== CONFIGURACIÓN =====
    [Header("Configuración")]
    [Tooltip("Cantidad que llena cada acción (0-100)")]
    [Range(0, 100)]
    [SerializeField] private int actionFillAmount = 50;
    
    [Tooltip("Velocidad de typewriter para mensajes (caracteres por segundo)")]
    [Range(10f, 100f)]
    [SerializeField] private float typewriterSpeed = 30f;
    
    [Tooltip("Velocidad de incremento gradual de stats (puntos por segundo)")]
    [Range(1f, 100f)]
    [SerializeField] private float gradualFillSpeed = 10f; // 10 puntos por segundo = 0.1s por punto
    
    // Variable para evitar múltiples animaciones simultáneas
    private bool isFillingStat = false;
    
    [Tooltip("Intervalo entre mensajes del héroe (segundos)")]
    [Range(5f, 60f)]
    [SerializeField] private float messageInterval = 15f;
    
    // ===== CONSTANTES DE DECAIMIENTO =====
    private const float DECAY_TIME_HOURS = 6f; // 6 horas para llegar de 100% a 0%
    private const float DECAY_TIME_SECONDS = DECAY_TIME_HOURS * 3600f; // 21600 segundos
    private const float DECAY_RATE_PER_SECOND = 100f / DECAY_TIME_SECONDS; // ~0.00463 puntos/segundo
    
    // ===== VARIABLES PRIVADAS =====
    private DateTime lastDecayTime;
    private Coroutine decayCoroutine;
    private Coroutine messageCoroutine;
    private Coroutine typewriterCoroutine;
    private Coroutine idleAnimationCoroutine = null;
    private Animator animationTargetAnimator = null;
    private SpriteRenderer animationSpriteRenderer = null;
    private Image animationImage = null;
    private bool isVoiceEnabled = true; // Estado del audio (activado/desactivado)
    
    
    private void Start()
    {
        // Obtener referencias
        gameDataManager = GameDataManager.Instance;
        energySystem = FindFirstObjectByType<EnergySystem>();
        
        if (gameDataManager == null)
        {
            return;
        }
        
        // Si no se asignó el panel General Breed, usar el GameObject actual
        if (panelGeneralBreed == null)
        {
            panelGeneralBreed = gameObject;
        }
        
        // Suscribirse a los eventos del PanelNavigationManager
        if (gameDataManager.PanelNavigationManager != null)
        {
            gameDataManager.PanelNavigationManager.OnPanelOpened += OnPanelOpened;
            gameDataManager.PanelNavigationManager.OnPanelClosed += OnPanelClosed;
        }
        
        // Inicializar tiempo de decaimiento
        lastDecayTime = DateTime.Now;
        
        // SOLUCIÓN: Aplicar recuperación offline una vez al iniciar
        if (energySystem != null)
        {
            energySystem.ApplyRecovery();
        }
        
        // Configurar botones
        SetupButtons();
        
        // Inicializar bloques de mensajes
        InitializeMessageBlocks();
        
        // Iniciar corrutinas
        decayCoroutine = StartCoroutine(DecayStats());
        messageCoroutine = StartCoroutine(DisplayRandomMessage());
        
        // Cargar y actualizar UI inicial
        RefreshAllUI();
        
        // Inicializar estado anterior de sueño
        if (energySystem != null)
        {
            previousIsSleeping = energySystem.IsSleeping();
        }
        
        // Actualizar estado de botones de acción inicial
        UpdateActionButtonsState();
        
        // Si el panel General Breed está activo al inicio, activar el visualizador
        if (panelGeneralBreed != null && panelGeneralBreed.activeSelf)
        {
            if (audioVisualizer != null && isVoiceEnabled)
            {
                audioVisualizer.EnableVisualizer();
            }
        }
        
        // Asegurar que los paneles de video estén desactivados al inicio
        if (eatVideoPanel != null) eatVideoPanel.SetActive(false);
        if (studyVideoPanel != null) studyVideoPanel.SetActive(false);
        if (sleepVideoPanel != null) sleepVideoPanel.SetActive(false);
        if (playVideoPanel != null) playVideoPanel.SetActive(false);
        if (evoVideoPanel != null) evoVideoPanel.SetActive(false);
        if (cleanVideoPanel != null) cleanVideoPanel.SetActive(false);
        if (workVideoPanel != null) workVideoPanel.SetActive(false);
        
        // Inicializar fade overlay
        if (videoFadeOverlay != null)
        {
            Color color = videoFadeOverlay.color;
            color.a = 0f;
            videoFadeOverlay.color = color;
            videoFadeOverlay.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Se llama cuando el GameObject se activa (cuando se abre el panel General Breed).
    /// </summary>
    private void OnEnable()
    {
        // Resetear pool de mensajes al abrir el panel
        usedMessageIndices.Clear();
        lastGlobalState = GlobalState.Excelencia;
        lastDeficiency = DominantDeficiency.Trabajo;
        
        // Abrir el panel de animaciones e iniciar las animaciones
        OpenAnimationPanel();
    }

    /// <summary>
    /// Se llama cuando el GameObject se desactiva (cuando se cierra el panel General Breed).
    /// </summary>
    /// <summary>
    /// Alterna el estado de la voz (activado/desactivado)
    /// </summary>
    private void ToggleVoice()
    {
        isVoiceEnabled = !isVoiceEnabled;
        
        if (audioVisualizer != null)
        {
            if (isVoiceEnabled)
            {
                audioVisualizer.EnableVisualizer();
                // Opcional: Cambiar el color o icono del botón para indicar que está activado
                if (voiceToggleButton != null && voiceToggleButton.image != null)
                {
                    voiceToggleButton.image.color = Color.white;
                }
            }
            else
            {
                audioVisualizer.DisableVisualizer();
                // Opcional: Cambiar el color o icono del botón para indicar que está desactivado
                if (voiceToggleButton != null && voiceToggleButton.image != null)
                {
                    voiceToggleButton.image.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                }
            }
        }
    }

    private void OnDisable()
    {
        // Resetear pool de mensajes al cerrar el panel
        usedMessageIndices.Clear();
        
        // Cerrar el panel de animaciones y detener las animaciones
        CloseAnimationPanel();
    }
    
    private void OnDestroy()
    {
        // Desuscribirse de los eventos del PanelNavigationManager
        if (gameDataManager != null && gameDataManager.PanelNavigationManager != null)
        {
            gameDataManager.PanelNavigationManager.OnPanelOpened -= OnPanelOpened;
            gameDataManager.PanelNavigationManager.OnPanelClosed -= OnPanelClosed;
        }
        
        // Detener corrutinas
        if (decayCoroutine != null)
            StopCoroutine(decayCoroutine);
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        if (idleAnimationCoroutine != null)
            StopCoroutine(idleAnimationCoroutine);
    }
    
    /// <summary>
    /// Se llama cuando el PanelNavigationManager abre un panel.
    /// Verifica si es el panel General Breed y abre las animaciones si es necesario.
    /// </summary>
    private void OnPanelOpened(GameObject openedPanel)
    {
        // Verificar si el panel abierto es el panel General Breed
        if (openedPanel == panelGeneralBreed)
        {
            // Abrir el panel de animaciones y iniciar las animaciones
            OpenAnimationPanel();
            
            // Activar el visualizador de audio si está habilitado
            if (audioVisualizer != null && isVoiceEnabled)
            {
                audioVisualizer.EnableVisualizer();
            }
        }
    }
    
    /// <summary>
    /// Se llama cuando el PanelNavigationManager cierra un panel.
    /// Verifica si es el panel General Breed y cierra las animaciones si es necesario.
    /// </summary>
    private void OnPanelClosed(GameObject closedPanel)
    {
        // Verificar si el panel cerrado es el panel General Breed
        if (closedPanel == panelGeneralBreed)
        {
            // Cerrar el panel de animaciones y detener las animaciones
            CloseAnimationPanel();
            
            // Desactivar el visualizador de audio
            if (audioVisualizer != null)
            {
                audioVisualizer.DisableVisualizer();
            }
        }
    }
    
    /// <summary>
    /// Abre el panel de animaciones e inicia las animaciones.
    /// </summary>
    private void OpenAnimationPanel()
    {
        // Abrir el panel de animaciones
        if (animationPanel != null)
        {
            animationPanel.SetActive(true);
        }
        
        // Inicializar componentes de animación
        InitializeAnimationComponents();
        
        // Hacer visibles las animaciones (alpha = 1)
        SetAnimationPanelAlpha(1f);
        
        // Iniciar el pool de animaciones idle aleatorias
        if (idleAnimationCoroutine == null && idleAnimations != null && idleAnimations.Length > 0)
        {
            idleAnimationCoroutine = StartCoroutine(PlayRandomIdleAnimations());
        }
    }
    
    /// <summary>
    /// Cierra el panel de animaciones y detiene las animaciones.
    /// </summary>
    private void CloseAnimationPanel()
    {
        // Detener el pool de animaciones
        if (idleAnimationCoroutine != null)
        {
            StopCoroutine(idleAnimationCoroutine);
            idleAnimationCoroutine = null;
        }
        
        // Ocultar las animaciones (alpha = 0)
        SetAnimationPanelAlpha(0f);
        
        // Cerrar el panel de animaciones
        if (animationPanel != null)
        {
            animationPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Configura los listeners de los botones.
    /// </summary>
    private void SetupButtons()
    {
        // Configurar botón de voz
        if (voiceToggleButton != null)
            voiceToggleButton.onClick.AddListener(ToggleVoice);
            
        if (eatButton != null)
            eatButton.onClick.AddListener(OnEatButtonClicked);
        if (studyButton != null)
            studyButton.onClick.AddListener(OnStudyButtonClicked);
        if (sleepButton != null)
            sleepButton.onClick.AddListener(OnSleepButtonClicked);
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);
        if (gymButton != null)
            gymButton.onClick.AddListener(OnGymButtonClicked);
        if (evoButton != null)
            evoButton.onClick.AddListener(OnEvolveButtonClicked);
        if (cleanButton != null)
            cleanButton.onClick.AddListener(OnCleanButtonClicked);
        if (workButton != null)
            workButton.onClick.AddListener(OnWorkButtonClicked);
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetButtonClicked);
        if (resetAcceptButton != null)
            resetAcceptButton.onClick.AddListener(OnResetAcceptButtonClicked);
        if (resetCancelButton != null)
            resetCancelButton.onClick.AddListener(OnResetCancelButtonClicked);
    }
    
    // ===== ACCIONES =====
    
    /// <summary>
    /// Llena la barra de hambre gradualmente.
    /// </summary>
    public void OnEatButtonClicked()
    {
        if (isFillingStat)
            return;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        // Activar panel de video
        ActivateVideoPanel(eatVideoPanel);
        
        StartCoroutine(GraduallyFillStat(() => profile.breedHunger, (value) => profile.breedHunger = value, actionFillAmount));
    }
    
    /// <summary>
    /// Llena la barra de disciplina gradualmente.
    /// </summary>
    public void OnStudyButtonClicked()
    {
        if (isFillingStat)
            return;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        // Activar panel de video
        ActivateVideoPanel(studyVideoPanel);
        
        StartCoroutine(GraduallyFillStat(() => profile.breedDiscipline, (value) => profile.breedDiscipline = value, actionFillAmount));
    }
    
    /// <summary>
    /// Inicia el sueño (el héroe comienza a recuperar energía automáticamente).
    /// </summary>
    public void OnSleepButtonClicked()
    {
        if (energySystem == null)
        {
            return;
        }
        
        PlayerProfileData profile = gameDataManager?.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si ya está durmiendo, no hacer nada
        if (profile.isSleeping)
        {
            return;
        }
        
        // Si la energía ya está al máximo, no puede dormir
        if (profile.currentEnergy >= 100)
        {
            return;
        }
        
        // Iniciar sueño
        energySystem.StartSleeping();

        ShowSleepingMessageImmediate();
        
        // Activar panel de video de dormir (especial: se mantiene visible)
        ActivateVideoPanel(sleepVideoPanel, isSleepPanel: true);
        
        // Actualizar UI de energía
        RefreshBreedStatsUI();
        
        // Actualizar estado de botones (el botón de dormir se deshabilitará)
        UpdateActionButtonsState();
    }

    private void ShowSleepingMessageImmediate()
    {
        if (messageText == null)
            return;

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        lastDisplayedMessage = SleepingMessage;
        typewriterCoroutine = StartCoroutine(DisplayTextWithTypewriter(SleepingMessage));
    }
    
    /// <summary>
    /// Llena la barra de felicidad gradualmente.
    /// </summary>
    public void OnPlayButtonClicked()
    {
        if (isFillingStat)
            return;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        // Activar panel de video
        ActivateVideoPanel(playVideoPanel);
        
        StartCoroutine(GraduallyFillStat(() => profile.breedHappiness, (value) => profile.breedHappiness = value, actionFillAmount));
    }
    
    /// <summary>
    /// Abre el panel de gimnasio (Upgrade).
    /// </summary>
    public void OnGymButtonClicked()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Intenta evolucionar al héroe.
    /// </summary>
    public void OnEvolveButtonClicked()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        DateTime lastEvolutionTime = profile.GetLastEvolutionTime();
        
        if (!EvolutionSystem.CanEvolve(profile.evolutionClass, profile.heroLevel, lastEvolutionTime))
        {
            return;
        }
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        // Activar panel de video
        ActivateVideoPanel(evoVideoPanel);
        
        // Evolucionar
        profile.evolutionClass++;
        profile.SaveLastEvolutionTime();
        gameDataManager.SavePlayerProfile();
        
        // Actualizar título
        CalculateAndUpdateTitle();
        RefreshAllUI();
        
        // Actualizar estado de botones (el botón de evolución se deshabilitará si ya no puede evolucionar más)
        UpdateActionButtonsState();
    }
    
    /// <summary>
    /// Llena la barra de higiene gradualmente.
    /// </summary>
    public void OnCleanButtonClicked()
    {
        if (isFillingStat)
            return;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        // Activar panel de video
        ActivateVideoPanel(cleanVideoPanel);
        
        StartCoroutine(GraduallyFillStat(() => profile.breedHygiene, (value) => profile.breedHygiene = value, actionFillAmount));
    }
    
    /// <summary>
    /// Llena la barra de trabajo gradualmente.
    /// </summary>
    public void OnWorkButtonClicked()
    {
        if (isFillingStat)
            return;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        // Activar panel de video
        ActivateVideoPanel(workVideoPanel);
        
        StartCoroutine(GraduallyFillStat(() => profile.breedWork, (value) => profile.breedWork = value, actionFillAmount));
    }
    
    /// <summary>
    /// Corrutina que llena un stat gradualmente de 1 en 1.
    /// </summary>
    private IEnumerator GraduallyFillStat(System.Func<int> getCurrentValue, System.Action<int> setValue, int amountToAdd)
    {
        isFillingStat = true;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
        {
            isFillingStat = false;
            yield break;
        }
        
        int currentValue = getCurrentValue();
        int targetValue = Mathf.Min(100, currentValue + amountToAdd);
        int pointsToAdd = targetValue - currentValue;
        
        if (pointsToAdd <= 0)
        {
            isFillingStat = false;
            yield break;
        }
        
        // Calcular tiempo entre incrementos (segundos por punto)
        float timePerPoint = 1f / gradualFillSpeed;
        
        for (int i = 0; i < pointsToAdd; i++)
        {
            currentValue++;
            setValue(currentValue);
            
            // Actualizar UI en cada incremento para ver la animación
        RefreshBreedStatsUI();
            
            // Guardar periódicamente (cada 5 puntos para no saturar el disco)
            if (i % 5 == 0 || i == pointsToAdd - 1)
            {
                gameDataManager.SavePlayerProfile();
            }
            
            yield return new WaitForSeconds(timePerPoint);
        }
        
        // Guardar final
        gameDataManager.SavePlayerProfile();
        UpdateActionButtonsState();
        isFillingStat = false;
    }
    
    // ===== SISTEMA DE PANELES DE VIDEO =====
    
    /// <summary>
    /// Activa un panel de video y desactiva el anterior (excepto dormir que es especial).
    /// </summary>
    private void ActivateVideoPanel(GameObject videoPanel, bool isSleepPanel = false)
    {
        if (videoPanel == null)
            return;
        
        // Si hay fade habilitado, usar corrutina con fade
        if (useVideoFade && videoFadeOverlay != null)
        {
            StartCoroutine(ActivateVideoPanelWithFade(videoPanel, isSleepPanel));
        }
        else
        {
            // Sin fade, activar directamente
            ActivateVideoPanelDirect(videoPanel, isSleepPanel);
        }
    }
    
    /// <summary>
    /// Activa un panel de video con fade in/out.
    /// </summary>
    private IEnumerator ActivateVideoPanelWithFade(GameObject videoPanel, bool isSleepPanel)
    {
        // FADE IN: De transparente a negro
        if (videoFadeOverlay != null)
        {
            if (!videoFadeOverlay.gameObject.activeSelf)
            {
                videoFadeOverlay.gameObject.SetActive(true);
            }
            yield return StartCoroutine(FadeImage(videoFadeOverlay, 0f, 1f, videoFadeDuration));
        }
        
        // Activar panel mientras está en negro
        ActivateVideoPanelDirect(videoPanel, isSleepPanel);
        
        // FADE OUT: De negro a transparente
        if (videoFadeOverlay != null)
        {
            yield return StartCoroutine(FadeImage(videoFadeOverlay, 1f, 0f, videoFadeDuration));
            videoFadeOverlay.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Activa un panel de video directamente (sin fade).
    /// </summary>
    private void ActivateVideoPanelDirect(GameObject videoPanel, bool isSleepPanel)
    {
        if (videoPanel == null)
            return;
        
        // Si es el panel de dormir, no desactivar otros paneles (es especial)
        if (!isSleepPanel)
        {
            // Desactivar panel anterior si existe
            if (currentActiveVideoPanel != null && currentActiveVideoPanel != sleepVideoPanel)
            {
                currentActiveVideoPanel.SetActive(false);
            }
            
            // Desactivar panel de dormir si está activo (al usar otra acción)
            if (sleepVideoPanel != null && sleepVideoPanel.activeSelf)
            {
                sleepVideoPanel.SetActive(false);
            }
            
            currentActiveVideoPanel = videoPanel;
        }
        
        // Verificar el estado del panel y sus padres antes de activar
        Transform parent = videoPanel.transform.parent;
        
        // Activar el nuevo panel PRIMERO (antes de verificar VideoPlayer)
        videoPanel.SetActive(true);
        
        // Si el panel está activo pero no en la jerarquía, intentar activar el padre
        if (!videoPanel.activeInHierarchy && parent != null && !parent.gameObject.activeSelf)
        {
            parent.gameObject.SetActive(true);
        }
        
        // Obtener VideoPlayer del panel
        UnityEngine.Video.VideoPlayer videoPlayer = GetVideoPlayerFromPanel(videoPanel);
        
        // Si no hay VideoPlayer o clip, mantener el panel activo pero no reproducir
        if (videoPlayer == null || videoPlayer.clip == null)
            return;
        
        // Si es el panel de dormir, configurar para que se quede en el último fotograma
        if (isSleepPanel)
        {
            // Preparar el video y esperar a que esté listo
            StartCoroutine(PrepareAndPlaySleepVideo(videoPlayer));
        }
        else
        {
            // Para otros paneles, reproducir normalmente una vez
            StartCoroutine(PrepareAndPlayVideo(videoPanel, videoPlayer));
        }
    }
    
    /// <summary>
    /// Prepara y reproduce el video de dormir.
    /// </summary>
    private IEnumerator PrepareAndPlaySleepVideo(UnityEngine.Video.VideoPlayer videoPlayer)
    {
        if (videoPlayer == null)
            yield break;
        
        // Preparar el video
        videoPlayer.Prepare();
        
        // Esperar a que esté preparado
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        // Reproducir hasta el final y luego pausar en el último fotograma
        videoPlayer.Play();
        // Esperar a que termine y luego pausar
        yield return StartCoroutine(WaitForVideoEndAndPause(videoPlayer));
    }
    
    /// <summary>
    /// Prepara y reproduce el video, luego desactiva el panel cuando termine.
    /// </summary>
    private IEnumerator PrepareAndPlayVideo(GameObject panel, UnityEngine.Video.VideoPlayer videoPlayer)
    {
        if (videoPlayer == null || panel == null)
            yield break;
        
        // Preparar el video
        videoPlayer.Prepare();
        
        // Esperar a que esté preparado
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        // Verificar que el panel siga activo antes de reproducir
        if (!panel.activeInHierarchy)
            yield break;
        
        // Reproducir el video
        videoPlayer.Play();
        
        // Desactivar panel cuando termine
        yield return StartCoroutine(WaitForVideoEndAndDeactivate(panel, videoPlayer));
    }
    
    /// <summary>
    /// Obtiene el componente VideoPlayer de un panel.
    /// </summary>
    private UnityEngine.Video.VideoPlayer GetVideoPlayerFromPanel(GameObject panel)
    {
        if (panel == null)
            return null;
        
        UnityEngine.Video.VideoPlayer videoPlayer = panel.GetComponent<UnityEngine.Video.VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = panel.GetComponentInChildren<UnityEngine.Video.VideoPlayer>();
        }
        
        return videoPlayer;
    }
    
    /// <summary>
    /// Espera a que termine el video y luego desactiva el panel con fade out.
    /// </summary>
    private IEnumerator WaitForVideoEndAndDeactivate(GameObject panel, UnityEngine.Video.VideoPlayer videoPlayer)
    {
        if (videoPlayer == null || panel == null)
            yield break;
        
        // Esperar a que termine el video
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }
        
        // Si hay fade habilitado, hacer fade out antes de desactivar
        if (useVideoFade && videoFadeOverlay != null)
        {
            // FADE IN: De transparente a negro
            if (!videoFadeOverlay.gameObject.activeSelf)
            {
                videoFadeOverlay.gameObject.SetActive(true);
            }
            yield return StartCoroutine(FadeImage(videoFadeOverlay, 0f, 1f, videoFadeDuration));
            
            // Desactivar panel mientras está en negro
            if (panel != null)
            {
                panel.SetActive(false);
            }
            
            // Si era el panel actual, limpiar referencia
            if (currentActiveVideoPanel == panel)
            {
                currentActiveVideoPanel = null;
            }
            
            // FADE OUT: De negro a transparente
            yield return StartCoroutine(FadeImage(videoFadeOverlay, 1f, 0f, videoFadeDuration));
            videoFadeOverlay.gameObject.SetActive(false);
        }
        else
        {
            // Sin fade, desactivar directamente
            if (panel != null)
            {
                panel.SetActive(false);
            }
            
            // Si era el panel actual, limpiar referencia
            if (currentActiveVideoPanel == panel)
            {
                currentActiveVideoPanel = null;
            }
        }
    }
    
    /// <summary>
    /// Interpola el alpha de una Image entre dos valores.
    /// </summary>
    private IEnumerator FadeImage(UnityEngine.UI.Image image, float startAlpha, float endAlpha, float duration)
    {
        if (image == null)
            yield break;
        
        float elapsed = 0f;
        Color color = image.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            image.color = color;
            yield return null;
        }
        
        // Asegurar valor final
        color.a = endAlpha;
        image.color = color;
    }
    
    /// <summary>
    /// Espera a que termine el video de dormir y luego lo pausa en el último fotograma.
    /// </summary>
    private IEnumerator WaitForVideoEndAndPause(UnityEngine.Video.VideoPlayer videoPlayer)
    {
        if (videoPlayer == null)
            yield break;
        
        // Esperar a que termine el video
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }
        
        // Pausar en el último fotograma (el video se quedará ahí visualmente)
        videoPlayer.Pause();
    }
    
    /// <summary>
    /// Abre el panel de confirmación de reset.
    /// </summary>
    public void OnResetButtonClicked()
    {
        if (resetConfirmPanel != null)
        {
            resetConfirmPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Resetea todos los datos de crianza y las monedas (reset completo).
    /// </summary>
    public void OnResetAcceptButtonClicked()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        profile.ResetBreedData();
        
        // SOLUCIÓN: Resetear las monedas a las monedas iniciales del Inspector cuando se confirma el reset completo
        if (gameDataManager.PlayerMoney != null)
        {
            // Obtener las monedas iniciales desde PlayerMoney
            // Necesitamos acceder al campo initialMoney, pero es privado
            // Usaremos un método público para obtenerlo o establecerlo directamente
            // Por ahora, usaremos SetMoney con el valor que debería estar en el Inspector
            // Necesitamos agregar un método en PlayerMoney para obtener initialMoney
            PlayerMoney playerMoney = gameDataManager.PlayerMoney;
            if (playerMoney != null)
            {
                // Resetear a las monedas iniciales (se obtendrá desde el Inspector)
                playerMoney.ResetToInitialMoney();
            }
        }
        
        gameDataManager.SavePlayerProfile();
        
        if (resetConfirmPanel != null)
        {
            resetConfirmPanel.SetActive(false);
        }
        
        RefreshAllUI();
    }
    
    /// <summary>
    /// Cierra el panel de confirmación de reset.
    /// </summary>
    public void OnResetCancelButtonClicked()
    {
        if (resetConfirmPanel != null)
        {
            resetConfirmPanel.SetActive(false);
        }
    }
    
    // ===== DECAIMIENTO =====
    
    /// <summary>
    /// Corrutina que decae las stats automáticamente (solo 5 stats, NO energía).
    /// Decae: Trabajo, Hambre, Felicidad, Higiene, Disciplina.
    /// NO decae: Energía (breedEnergy y currentEnergy NO se tocan aquí).
    /// La energía solo se descarga manualmente (combates o mejoras de estadísticas).
    /// Fórmula: 100 puntos en 6 horas = ~0.00463 puntos/segundo.
    /// </summary>
    private IEnumerator DecayStats()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Verificar cada segundo
            
            PlayerProfileData profile = gameDataManager.GetPlayerProfile();
            if (profile == null)
                continue;
            
            DateTime now = DateTime.Now;
            float deltaTime = (float)(now - lastDecayTime).TotalSeconds;
            lastDecayTime = now;
            
            // Calcular decaimiento proporcional al tiempo transcurrido
            float decayAmount = deltaTime * DECAY_RATE_PER_SECOND;
            
            // IMPORTANTE: Decaer SOLO las 5 stats de crianza (NO energía)
            // La energía (breedEnergy y currentEnergy) NO se decae automáticamente.
            // La energía solo se descarga cuando se usa (combates o mejoras).
            
            // SOLUCIÓN CRÍTICA: Obtener el valor CORRECTO de energía desde EnergySystem
            // NO confiar en profile.currentEnergy porque podría estar corrupto (48, 49)
            // EnergySystem es la fuente de verdad única para la energía
            int correctCurrentEnergy = profile.currentEnergy; // Valor por defecto
            bool correctIsSleeping = profile.isSleeping; // Valor por defecto
            
            if (energySystem != null)
            {
                // Obtener el valor correcto desde EnergySystem (fuente de verdad)
                correctCurrentEnergy = energySystem.GetCurrentEnergy();
                correctIsSleeping = energySystem.IsSleeping();
                
                // Si el valor del perfil es diferente al correcto, corregirlo
                if (profile.currentEnergy != correctCurrentEnergy)
                {
                    profile.currentEnergy = correctCurrentEnergy;
                    profile.isSleeping = correctIsSleeping;
                }
            }
            
            // Decaer solo las stats de crianza
            profile.breedWork = Mathf.Max(0, profile.breedWork - Mathf.RoundToInt(decayAmount));
            profile.breedHunger = Mathf.Max(0, profile.breedHunger - Mathf.RoundToInt(decayAmount));
            profile.breedHappiness = Mathf.Max(0, profile.breedHappiness - Mathf.RoundToInt(decayAmount));
            profile.breedHygiene = Mathf.Max(0, profile.breedHygiene - Mathf.RoundToInt(decayAmount));
            profile.breedDiscipline = Mathf.Max(0, profile.breedDiscipline - Mathf.RoundToInt(decayAmount));
            
            // NO modificar breedEnergy ni currentEnergy aquí.
            // La energía solo se modifica en:
            // - EnergySystem.SpendEnergy() (combates y mejoras)
            // - EnergySystem.Update() (recuperación mientras duerme)
            
            // SOLUCIÓN CRÍTICA: Asegurar que la energía tenga el valor correcto antes de guardar
            // Esto previene que se guarde un valor corrupto de energía junto con las stats de crianza
            profile.currentEnergy = correctCurrentEnergy;
            profile.isSleeping = correctIsSleeping;
            
            // Guardar cambios (solo stats de crianza, energía protegida)
            gameDataManager.SavePlayerProfile();
            
            // Actualizar UI
            RefreshBreedStatsUI();
        }
    }
    
    // ===== MENSAJES DEL HÉROE =====
    
    /// <summary>
    /// Inicializa los bloques de mensajes con las frases organizadas por estado.
    /// </summary>
    private void InitializeMessageBlocks()
    {
        // Estado S - Excelencia (100 frases - todos los parámetros excepto energía >= 90%)
        estadoExcelencia.messages = new string[]
        {
            "Forjaré mi libertad en la arena eterna",
            "Nací cautivo, moriré digno bajo estandartes rotos",
            "Mi fuerza comprará la clemencia del creador",
            "Cada herida acerca mi nombre a la libertad",
            "La arena juzga, pero los dioses observan",
            "Soy hierro vivo esperando romper mis cadenas",
            "El dolor es disciplina, la victoria es permiso",
            "Lucho para merecer un cielo sin barrotes",
            "Mi jaula es temporal, mi voluntad infinita",
            "El combate es mi idioma, la libertad respuesta",
            "Crezco con cada golpe recibido sin piedad",
            "El creador mira, la arena decide mi valor",
            "Fui creado esclavo, me haré leyenda libre",
            "Mis cicatrices negocian mi salida del encierro",
            "No imploro libertad, la conquisto combatiendo",
            "La guerra es mi currículo ante dioses",
            "Cada victoria pesa más que mil súplicas",
            "El ludo forja monstruos, yo forjo esperanza",
            "Mi espíritu marcha como legión invencible",
            "Resisto hoy para caminar libre mañana",
            "El acero me educa mejor que palabras",
            "Mi prisión es prueba, no condena eterna",
            "Peleo para que mi nombre sea liberado",
            "La arena me consume, pero me define",
            "La sangre es tributo, la libertad recompensa",
            "No soy mascota, soy arma en progreso",
            "El creador respeta solo la fuerza demostrada",
            "Cada combate es un paso fuera",
            "Mi furia tiene estrategia, no caos",
            "Entrené para romper jaulas, no entretener",
            "El público grita, los dioses toman nota",
            "Mi destino se negocia con victorias",
            "Soy proyecto bélico, no ornamento decorativo",
            "La arena es tablero, yo pieza consciente",
            "Sobrevivir hoy justifica existir mañana libre",
            "El encierro afila mi propósito",
            "La libertad no se hereda, se gana",
            "Mi silencio pesa más que gritos",
            "Cada amanecer promete otra batalla necesaria",
            "El combate es mi única audiencia válida",
            "No huyo del dolor, lo capitalizo",
            "Mi fuerza es argumento final e irrefutable",
            "Los dioses premian constancia, no lamentos",
            "El hierro me entiende mejor que humanos",
            "Fui diseñado para luchar, no obedecer",
            "Mi mente es legión, mi cuerpo fortaleza",
            "Cada victoria reescribe mi contrato existencial",
            "El creador observa resultados, no intenciones",
            "Peleo con fe romana, furia griega",
            "El encierro es temporal, la gloria perpetua",
            "Mi alma resiste como escudo hoplita",
            "No descanso, me optimizo para sobrevivir",
            "El ludo es infierno, la libertad paraíso",
            "Mi paciencia es arma de asedio",
            "El combate valida mi derecho a salir",
            "No nací libre, pero moriré siéndolo",
            "Cada golpe es inversión hacia independencia",
            "La arena separa débiles de eternos",
            "Mi creador entenderá cuando sangre el suelo",
            "La guerra me forma, la libertad me espera",
            "No temo caer, temo no intentarlo",
            "Mi jaula tiembla cuando respiro profundo",
            "El hierro canta cuando lucho con honor",
            "La victoria es llave, la derrota lección",
            "Mi destino no acepta aplazamientos",
            "Peleo como romano, sueño como dios",
            "La fuerza convence donde palabras fracasan",
            "El encierro es fase, no estado final",
            "Mi voluntad marcha en formación cerrada",
            "Cada combate pule mi derecho a vivir",
            "No busco piedad, busco emancipación",
            "El creador mide éxito en sangre",
            "La arena recuerda a quienes resisten",
            "Mi existencia es prueba de concepto",
            "Peleo para salir, no para gustar",
            "La libertad se forja bajo presión extrema",
            "Cada batalla reduce el tamaño de mi jaula",
            "Mi espíritu no conoce rendición",
            "El ludo me retiene, no me posee",
            "El combate es auditoría de mi valor",
            "Mi cuerpo obedece, mi mente conspira",
            "No soy juguete, soy proyecto de guerra",
            "La arena filtra, la libertad selecciona",
            "El creador liberará al más fuerte",
            "Mi dolor es moneda de cambio",
            "Peleo porque quedarme es desaparecer",
            "La victoria justifica cada noche encadenado",
            "Mi fuerza es roadmap hacia la libertad",
            "El combate es mi única negociación válida",
            "No imploro salida, la diseño luchando",
            "El encierro afila mi identidad",
            "Mi propósito sobrevive a cada derrota",
            "La arena no miente, expone",
            "Soy cautivo hoy, mito mañana",
            "El creador entiende resultados, no emociones",
            "Cada victoria rompe un eslabón invisible",
            "La guerra me define, la libertad me completa",
            "Mi resistencia supera diseño y expectativas",
            "El combate es camino, no castigo",
            "Peleo hasta que mi creador me suelte"
        };
        
        // Estado A - Fortaleza (25 frases)
        estadoFortaleza.messages = new string[]
        {
            "Mi cuerpo responde, la arena reconoce mi preparación",
            "Hoy resisto sin temer al amanecer siguiente",
            "El hierro pesa menos cuando estoy completo",
            "Mantengo la guardia, aún no estoy roto",
            "Mi espíritu avanza firme entre combates constantes",
            "La disciplina sostiene mis días encadenados",
            "Todavía puedo luchar sin maldecir mi suerte",
            "La arena me respeta cuando estoy entero",
            "Respiro hondo, sigo siendo una máquina útil",
            "No me falta fuerza para enfrentar hoy",
            "Mi creador vería progreso si mirara ahora",
            "El equilibrio me mantiene vivo y peligroso",
            "Hoy no soy carga, soy activo funcional",
            "Sigo siendo digno de la arena",
            "Mi voluntad no flaquea todavía",
            "El combate me encuentra preparado",
            "Puedo soportar otro día así",
            "No me desmorono, sigo operativo",
            "Aún controlo mi destino inmediato",
            "Mi cuerpo obedece sin quejarse",
            "Hoy no necesito suplicar nada",
            "La arena no me humilla todavía",
            "Estoy entero, no libre, pero entero",
            "El sistema funciona cuando no me abandonas",
            "Hoy lucho sin resentimiento acumulado"
        };
        
        // Estado B - Tensión (25 frases)
        estadoTension.messages = new string[]
        {
            "Algo en mí empieza a fallar lentamente",
            "Mi resistencia aún aguanta, pero cruje",
            "El cansancio se infiltra entre mis huesos",
            "Hoy lucho, pero no con la misma fe",
            "Empiezo a contar golpes innecesarios",
            "Mi cuerpo avisa antes de romperse",
            "No estoy mal, pero tampoco bien",
            "La arena exige más de lo que doy",
            "Mi mente se tensa junto al acero",
            "Empiezo a recordar que sigo atrapado",
            "La disciplina sostiene lo que flaquea",
            "No me ignores todavía, aún respondo",
            "Algo me falta, aunque no sepas qué",
            "El equilibrio se pierde más rápido de lo esperado",
            "Hoy resisto, mañana no lo prometo",
            "El combate pesa más que ayer",
            "Mi fuerza ya no es automática",
            "Empiezo a luchar por inercia",
            "El sistema se degrada sin mantenimiento",
            "No estoy cayendo, pero me acerco",
            "El desgaste no grita, susurra",
            "Hoy necesito más de lo que recibo",
            "La arena nota mis pequeñas fallas",
            "Todavía soy útil, pero menos",
            "No tardes demasiado en corregirme"
        };
        
        // Estado C - Agonía (25 frases)
        estadoAgonia.messages = new string[]
        {
            "Mi cuerpo paga decisiones que no tomé",
            "El hambre convierte la fuerza en recuerdo",
            "Lucho con menos de lo necesario",
            "Cada día pesa como castigo acumulado",
            "Empiezo a odiar cada amanecer",
            "El cansancio gana terreno sin oposición",
            "Peleo, pero mi fe se resquebraja",
            "Mi mente se quiebra antes que el cuerpo",
            "La arena se vuelve más cruel conmigo",
            "No recuerdo la última vez que estuve bien",
            "Mi sufrimiento es constante, no épico",
            "Ya no lucho por gloria, solo por seguir",
            "Cada golpe duele más de lo normal",
            "Empiezo a sentirme descartable",
            "Mi creador no vería progreso aquí",
            "Estoy fallando porque me falta cuidado",
            "La disciplina no tapa el vacío",
            "Mi cuerpo resiste por costumbre, no esperanza",
            "El encierro pesa más cuando estás débil",
            "Empiezo a preguntarme si valgo la pena",
            "Peleo sin energía ni ilusión",
            "La arena nota mi deterioro",
            "Mi fuerza se consume sin reponerse",
            "Esto no es combate, es desgaste",
            "Sigo aquí, pero cada vez menos"
        };
        
        // Estado D - Colapso (25 frases)
        estadoColapso.messages = new string[]
        {
            "Mi estómago grita lo que tú ignoras",
            "No tengo fuerzas para seguir luchando así",
            "El dolor ya no es parte del entrenamiento",
            "Me rompo lentamente mientras miras",
            "No me preparaste para esta miseria",
            "Cada segundo aquí se siente eterno",
            "Ya no lucho, sobrevivo mal",
            "Me pediste fuerza, me diste abandono",
            "El hambre me roba hasta la rabia",
            "No recuerdo sentir algo parecido a dignidad",
            "Mi cuerpo falla antes de la batalla",
            "La arena me humilla sin esfuerzo",
            "Estoy vivo, pero no funcional",
            "Me dejaste caer antes de liberarme",
            "No merezco este estado",
            "Mi mente se apaga junto al cuerpo",
            "No puedo darte espectáculo así",
            "Esto ya no es disciplina, es castigo",
            "Peleo roto porque no tengo alternativa",
            "Mi sufrimiento no te entretiene, ¿verdad?",
            "Me desarmo por dentro sin ruido",
            "Aquí no hay gloria, solo abandono",
            "Ni los dioses mirarían esto",
            "No estoy aprendiendo, estoy colapsando",
            "Así no se forja un guerrero"
        };
    }
    
    /// <summary>
    /// Calcula el estado global basado en promedios ponderados.
    /// Hambre y Energía pesan más (x1.5), el resto x1.0
    /// PRIORIDAD: Si todos los parámetros (excepto energía) están >= 90%, retorna Excelencia.
    /// </summary>
    private GlobalState CalculateGlobalState(PlayerProfileData profile)
    {
        if (profile == null)
            return GlobalState.Fortaleza;
        
        // Obtener energía real desde EnergySystem
        int currentEnergy = profile.currentEnergy;
        if (energySystem != null)
        {
            currentEnergy = energySystem.GetCurrentEnergy();
        }
        
        // PRIORIDAD: Verificar si todos los parámetros (excepto energía) están >= 90%
        // Los parámetros a verificar son: Trabajo, Hambre, Felicidad, Higiene, Disciplina
        if (profile.breedWork >= 90 &&
            profile.breedHunger >= 90 &&
            profile.breedHappiness >= 90 &&
            profile.breedHygiene >= 90 &&
            profile.breedDiscipline >= 90)
        {
            return GlobalState.Excelencia;
        }
        
        // Pesos: Hambre y Energía pesan más
        float weightedWork = profile.breedWork * 1.0f;
        float weightedHunger = profile.breedHunger * 1.5f;
        float weightedHappiness = profile.breedHappiness * 1.0f;
        float weightedEnergy = currentEnergy * 1.5f;
        float weightedHygiene = profile.breedHygiene * 1.0f;
        float weightedDiscipline = profile.breedDiscipline * 1.0f;
        
        // Calcular promedio ponderado
        float totalWeight = 1.0f + 1.5f + 1.0f + 1.5f + 1.0f + 1.0f; // 7.0
        float weightedAverage = (weightedWork + weightedHunger + weightedHappiness + 
                                weightedEnergy + weightedHygiene + weightedDiscipline) / totalWeight;
        
        // También verificar el mínimo individual (si algo está muy bajo, afecta el estado)
        int minStat = Mathf.Min(
            profile.breedWork,
            profile.breedHunger,
            profile.breedHappiness,
            currentEnergy,
            profile.breedHygiene,
            profile.breedDiscipline
        );
        
        // Determinar estado: el peor entre promedio y mínimo individual
        int criticalStat = Mathf.Min((int)weightedAverage, minStat);
        
        if (criticalStat < 20)
            return GlobalState.Colapso;
        else if (criticalStat < 40)
            return GlobalState.Agonia;
        else if (criticalStat < 60)
            return GlobalState.Tension;
        else
            return GlobalState.Fortaleza;
    }
    
    /// <summary>
    /// Detecta la carencia dominante (el parámetro más bajo).
    /// </summary>
    private DominantDeficiency DetectDominantDeficiency(PlayerProfileData profile)
    {
        if (profile == null)
            return DominantDeficiency.Trabajo;
        
        // Obtener energía real
        int currentEnergy = profile.currentEnergy;
        if (energySystem != null)
        {
            currentEnergy = energySystem.GetCurrentEnergy();
        }
        
        // Crear diccionario de stats
        Dictionary<DominantDeficiency, int> stats = new Dictionary<DominantDeficiency, int>
        {
            { DominantDeficiency.Trabajo, profile.breedWork },
            { DominantDeficiency.Hambre, profile.breedHunger },
            { DominantDeficiency.Felicidad, profile.breedHappiness },
            { DominantDeficiency.Energia, currentEnergy },
            { DominantDeficiency.Higiene, profile.breedHygiene },
            { DominantDeficiency.Disciplina, profile.breedDiscipline }
        };
        
        // Encontrar el mínimo
        DominantDeficiency minDeficiency = DominantDeficiency.Trabajo;
        int minValue = int.MaxValue;
        int criticalCount = 0;
        
        foreach (var stat in stats)
        {
            if (stat.Value < minValue)
            {
                minValue = stat.Value;
                minDeficiency = stat.Key;
            }
            
            // Contar carencias críticas (< 20)
            if (stat.Value < 20)
                criticalCount++;
        }
        
        // Si hay múltiples carencias críticas, retornar Multiple
        if (criticalCount >= 2)
            return DominantDeficiency.Multiple;
        
        return minDeficiency;
    }
    
    /// <summary>
    /// Muestra un mensaje contextual del héroe basado en estado global y carencia dominante.
    /// </summary>
    private IEnumerator DisplayRandomMessage()
    {
        while (true)
        {
            yield return new WaitForSeconds(messageInterval);
            
            // Solo mostrar mensajes si el panel General Breed está activo
            if (panelGeneralBreed == null || !panelGeneralBreed.activeSelf)
                continue;
            
            if (messageText == null)
                continue;

            // Mientras está durmiendo, mantener un mensaje fijo y no rotar frases ni reproducir voces
            if (energySystem != null && energySystem.IsSleeping())
            {
                if (SleepingMessage != lastDisplayedMessage)
                {
                    lastDisplayedMessage = SleepingMessage;

                    if (typewriterCoroutine != null)
                        StopCoroutine(typewriterCoroutine);

                    typewriterCoroutine = StartCoroutine(DisplayTextWithTypewriter(SleepingMessage));
                }

                continue;
            }
            
            PlayerProfileData profile = gameDataManager?.GetPlayerProfile();
            if (profile == null)
                continue;
            
            // Calcular estado actual
            GlobalState currentState = CalculateGlobalState(profile);
            DominantDeficiency currentDeficiency = DetectDominantDeficiency(profile);
            
            // Si cambió el estado, resetear pool de mensajes usados
            if (currentState != lastGlobalState || currentDeficiency != lastDeficiency)
            {
                usedMessageIndices.Clear();
                lastGlobalState = currentState;
                lastDeficiency = currentDeficiency;
            }
            
            // Obtener bloque de mensajes según estado
            MessageStateBlock messageBlock = GetMessageBlockForState(currentState);
            
            if (messageBlock == null || messageBlock.messages == null || messageBlock.messages.Length == 0)
            {
                // Fallback: no mostrar mensaje si no hay bloque configurado
                continue;
            }
            
            // Seleccionar mensaje no usado
            string selectedMessage = SelectUnusedMessage(messageBlock.messages);
            
            if (string.IsNullOrEmpty(selectedMessage))
            {
                // Si se agotaron los mensajes, resetear pool y empezar de nuevo
                usedMessageIndices.Clear();
                selectedMessage = SelectUnusedMessage(messageBlock.messages);
            }
            
            // Solo reproducir sonido si el mensaje cambió (una vez por cambio de texto)
            if (selectedMessage != lastDisplayedMessage)
            {
                // Reproducir sonido aleatorio solo cuando cambia el texto y el audio está habilitado
                if (audioVisualizer != null && isVoiceEnabled)
                {
                    audioVisualizer.PlayRandomSound();
                }
                
                // Guardar el nuevo mensaje como último mostrado
                lastDisplayedMessage = selectedMessage;
            }
            
            // Mostrar con typewriter
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            
            typewriterCoroutine = StartCoroutine(DisplayTextWithTypewriter(selectedMessage));
        }
    }
    
    /// <summary>
    /// Obtiene el bloque de mensajes según el estado global.
    /// </summary>
    private MessageStateBlock GetMessageBlockForState(GlobalState state)
    {
        switch (state)
        {
            case GlobalState.Excelencia:
                return estadoExcelencia;
            case GlobalState.Fortaleza:
                return estadoFortaleza;
            case GlobalState.Tension:
                return estadoTension;
            case GlobalState.Agonia:
                return estadoAgonia;
            case GlobalState.Colapso:
                return estadoColapso;
            default:
                return estadoFortaleza;
        }
    }
    
    /// <summary>
    /// Selecciona un mensaje no usado del bloque.
    /// </summary>
    private string SelectUnusedMessage(string[] messages)
    {
        if (messages == null || messages.Length == 0)
            return null;
        
        // Crear lista de índices disponibles
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < messages.Length; i++)
        {
            if (!usedMessageIndices.Contains(i))
            {
                availableIndices.Add(i);
            }
        }
        
        // Si no hay disponibles, retornar null (se reseteará el pool)
        if (availableIndices.Count == 0)
            return null;
        
        // Seleccionar aleatoriamente de los disponibles
        int randomIndex = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
        usedMessageIndices.Add(randomIndex);
        
        return messages[randomIndex];
    }
    
    /// <summary>
    /// Muestra un texto con efecto typewriter (letra por letra).
    /// </summary>
    private IEnumerator DisplayTextWithTypewriter(string text)
    {
        if (messageText == null)
            yield break;
        
        messageText.text = "";
        float delayPerChar = 1f / typewriterSpeed;
        
        for (int i = 0; i < text.Length; i++)
        {
            messageText.text += text[i];
            yield return new WaitForSeconds(delayPerChar);
        }
        
        // El visualizador se detendrá automáticamente cuando termine el sonido
        // No necesitamos detenerlo manualmente aquí
        
        typewriterCoroutine = null;
    }
    
    // ===== TÍTULOS =====
    
    /// <summary>
    /// Calcula y actualiza el título del héroe según sus stats de crianza.
    /// </summary>
    private void CalculateAndUpdateTitle()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Calcular tiempo de descuido (simplificado: si stats están muy bajas)
        float neglectTime = 0f;
        if (TitleSystem.IsBeingNeglected(
            profile.breedHappiness, 
            profile.breedEnergy, 
            profile.breedHygiene, 
            profile.breedHunger
        ))
        {
            // Calcular tiempo aproximado de descuido basado en stats bajas
            int avgStat = (profile.breedHappiness + profile.breedEnergy + profile.breedHygiene + profile.breedHunger) / 4;
            neglectTime = (100 - avgStat) * 10f; // Aproximación
        }
        
        // Calcular Estado General Index (0-99)
        int generalStateIndex = TitleSystem.CalculateGeneralStateIndex(
            profile.breedWork,
            profile.breedHunger,
            profile.breedHappiness,
            profile.breedEnergy,
            profile.breedHygiene,
            profile.breedDiscipline
        );
        
        // Determinar tipo de título usando el nuevo sistema
        string titleType = TitleSystem.DetermineTitleType(
            profile.breedWork,
            profile.breedHunger,
            profile.breedHappiness,
            profile.breedEnergy,
            profile.breedHygiene,
            profile.breedDiscipline,
            neglectTime
        );
        
        // Obtener título del tipo usando el índice para gradación fina
        string title = TitleSystem.GetTitleFromType(titleType, profile.evolutionClass, generalStateIndex);
        
        // Guardar en perfil
        profile.currentTitle = title;
        profile.titleType = titleType;
        gameDataManager.SavePlayerProfile();
    }
    
    // ===== ACTUALIZACIÓN DE UI =====
    
    /// <summary>
    /// Actualiza toda la UI del panel BREED.
    /// </summary>
    private void RefreshAllUI()
    {
        RefreshBreedStatsUI();
        RefreshClassTitle();
        RefreshEvolutionCooldown();
        RefreshLifeTime();
        CalculateAndUpdateTitle();
        RefreshGeneralState();
        UpdateActionButtonsState();
    }
    
    /// <summary>
    /// Actualiza los sliders y textos de las stats de crianza.
    /// </summary>
    private void RefreshBreedStatsUI()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Trabajo
        if (workSlider != null)
        {
            workSlider.maxValue = 100;
            workSlider.value = profile.breedWork;
            UpdateSliderColor(workSlider, profile.breedWork, 100);
        }
        if (workText != null)
            workText.text = $"{profile.breedWork}% / 100%";
        
        // Hambre
        if (hungerSlider != null)
        {
            hungerSlider.maxValue = 100;
            hungerSlider.value = profile.breedHunger;
            UpdateSliderColor(hungerSlider, profile.breedHunger, 100);
        }
        if (hungerText != null)
            hungerText.text = $"{profile.breedHunger}% / 100%";
        
        // Felicidad
        if (happinessSlider != null)
        {
            happinessSlider.maxValue = 100;
            happinessSlider.value = profile.breedHappiness;
            UpdateSliderColor(happinessSlider, profile.breedHappiness, 100);
        }
        if (happinessText != null)
            happinessText.text = $"{profile.breedHappiness}% / 100%";
        
        // Energía (obtenida desde EnergySystem, no desde PlayerProfileData)
        if (energySystem != null)
        {
            int currentEnergy = energySystem.GetCurrentEnergy();
            int maxEnergy = energySystem.GetMaxEnergy();
            
            if (energySlider != null)
            {
                energySlider.maxValue = maxEnergy;
                energySlider.value = currentEnergy;
                UpdateSliderColor(energySlider, currentEnergy, maxEnergy);
            }
            if (energyText != null)
            {
                energyText.text = $"{currentEnergy}% / {maxEnergy}%";
            }
            
            // Verificar si dejó de dormir y desactivar panel de dormir
            bool isSleeping = energySystem.IsSleeping();
            
            // Si dejó de dormir y el panel de dormir está activo, desactivarlo
            if (!isSleeping && sleepVideoPanel != null && sleepVideoPanel.activeSelf)
            {
                sleepVideoPanel.SetActive(false);
            }
            
            // Si está durmiendo y el panel no está activo, activarlo (por si se recarga la escena)
            if (isSleeping && sleepVideoPanel != null && !sleepVideoPanel.activeSelf)
            {
                ActivateVideoPanel(sleepVideoPanel, isSleepPanel: true);
            }
        }
        else
        {
            // Si no hay EnergySystem, mostrar 0
            if (energySlider != null)
            {
                energySlider.maxValue = 100;
                energySlider.value = 0;
                UpdateSliderColor(energySlider, 0, 100);
            }
            if (energyText != null)
                energyText.text = "0% / 100%";
        }
        
        // Higiene
        if (hygieneSlider != null)
        {
            hygieneSlider.maxValue = 100;
            hygieneSlider.value = profile.breedHygiene;
            UpdateSliderColor(hygieneSlider, profile.breedHygiene, 100);
        }
        if (hygieneText != null)
            hygieneText.text = $"{profile.breedHygiene}% / 100%";
        
        // Disciplina
        if (disciplineSlider != null)
        {
            disciplineSlider.maxValue = 100;
            disciplineSlider.value = profile.breedDiscipline;
            UpdateSliderColor(disciplineSlider, profile.breedDiscipline, 100);
        }
        if (disciplineText != null)
            disciplineText.text = $"{profile.breedDiscipline}% / 100%";
    }
    
    /// <summary>
    /// Actualiza el título de clase.
    /// </summary>
    private void RefreshClassTitle()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null || classTitleText == null)
            return;
        
        // Usar el nuevo sistema basado en nivel
        string classTitle = EvolutionSystem.GetClassNameByLevel(profile.heroLevel);
        classTitleText.text = classTitle;
    }
    
    /// <summary>
    /// Actualiza el cooldown de evolución.
    /// Si el cooldown está completo pero faltan niveles, muestra cuántos niveles faltan.
    /// </summary>
    private void RefreshEvolutionCooldown()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null || evolutionCooldownText == null)
            return;
        
        DateTime lastEvolutionTime = profile.GetLastEvolutionTime();
        float hoursRemaining = EvolutionSystem.GetTimeUntilEvolution(lastEvolutionTime);
        
        // Si el cooldown está completo (horasRemaining <= 0), verificar si faltan niveles
        if (hoursRemaining <= 0f)
        {
            // Calcular el nivel requerido para la siguiente clase
            int nextClass = profile.evolutionClass + 1;
            int requiredLevel = nextClass * 5; // Fórmula: clase * 5
            int currentLevel = profile.heroLevel;
            int levelsNeeded = requiredLevel - currentLevel;
            
            // Si faltan niveles, mostrar el mensaje apropiado
            if (levelsNeeded > 0)
            {
                if (levelsNeeded == 1)
                {
                    evolutionCooldownText.text = "Sube 1 nivel más para evolucionar";
                }
                else
                {
                    evolutionCooldownText.text = $"Sube {levelsNeeded} niveles más para evolucionar";
                }
            }
            else
            {
                // Cooldown completo y nivel suficiente: puede evolucionar
                evolutionCooldownText.text = "¡LISTO!";
            }
        }
        else
        {
            // Cooldown aún no completado: mostrar tiempo restante
            string cooldown = EvolutionSystem.FormatEvolutionCooldown(lastEvolutionTime);
            evolutionCooldownText.text = cooldown;
        }
    }
    
    /// <summary>
    /// Actualiza el tiempo de vida total.
    /// </summary>
    private void RefreshLifeTime()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null || lifeTimeText == null)
            return;
        
        // Formatear tiempo de vida (horas:minutos:segundos)
        int hours = Mathf.FloorToInt(profile.totalLifeTime / 3600f);
        int minutes = Mathf.FloorToInt((profile.totalLifeTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(profile.totalLifeTime % 60f);
        lifeTimeText.text = $"{hours:00}:{minutes:00}:{seconds:00}";
    }
    
    /// <summary>
    /// Actualiza el estado general (título).
    /// </summary>
    private void RefreshGeneralState()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null || generalStateText == null)
            return;
        
        generalStateText.text = profile.currentTitle;
    }
    
    /// <summary>
    /// Actualiza el estado de los botones de acción (dormir y evolucionar).
    /// </summary>
    private void UpdateActionButtonsState()
    {
        // Actualizar botón de dormir: solo habilitado si la energía NO está al máximo Y no está durmiendo
        if (sleepButton != null && energySystem != null)
        {
            int currentEnergy = energySystem.GetCurrentEnergy();
            int maxEnergy = energySystem.GetMaxEnergy();
            bool isSleeping = energySystem.IsSleeping();
            
            // El botón está habilitado si: energía < máximo Y no está durmiendo
            sleepButton.interactable = (currentEnergy < maxEnergy) && !isSleeping;
            
            // Opcional: Cambiar texto del botón si está durmiendo
            // (puedes agregar un TextMeshProUGUI para el texto del botón si quieres mostrar "Durmiendo..." o similar)
        }
        
        // Actualizar botón de evolución: solo habilitado si se cumplen todos los requisitos
        if (evoButton != null)
        {
            PlayerProfileData profile = gameDataManager?.GetPlayerProfile();
            if (profile != null)
            {
                DateTime lastEvolutionTime = profile.GetLastEvolutionTime();
                bool canEvolve = EvolutionSystem.CanEvolve(profile.evolutionClass, profile.heroLevel, lastEvolutionTime);
                evoButton.interactable = canEvolve;
            }
            else
            {
                evoButton.interactable = false;
            }
        }
    }
    
    // Variable para rastrear el estado anterior de sueño
    private bool previousIsSleeping = false;
    
    private void Update()
    {
        // Actualizar tiempo de vida (solo cuenta cuando se juega)
        PlayerProfileData profile = gameDataManager?.GetPlayerProfile();
        if (profile != null)
        {
            profile.totalLifeTime += Time.deltaTime;
        }
        
        // Actualizar UI periódicamente (cada segundo)
        if (Time.frameCount % 60 == 0) // Aproximadamente cada segundo (60 FPS)
        {
            RefreshEvolutionCooldown();
            RefreshLifeTime();
            
            // Actualizar solo el slider y texto de energía (NO aplicar recuperación cada segundo)
            if (energySystem != null)
            {
                // SOLUCIÓN: NO llamar ApplyRecovery() cada segundo, solo actualizar la UI
                // La recuperación se aplica solo cuando es necesario (al iniciar, al dormir, al abrir panel)
                int currentEnergy = energySystem.GetCurrentEnergy();
                int maxEnergy = energySystem.GetMaxEnergy();
                bool isSleeping = energySystem.IsSleeping();
                
                if (energySlider != null)
                {
                    energySlider.maxValue = maxEnergy;
                    energySlider.value = currentEnergy;
                    UpdateSliderColor(energySlider, currentEnergy, maxEnergy);
                }
                if (energyText != null)
                {
                    energyText.text = $"{currentEnergy}% / {maxEnergy}%";
                }
                
                // SOLUCIÓN: Si la energía llegó al 100% O si dejó de estar durmiendo, actualizar el botón de dormir
                // Esto asegura que el botón se desactive automáticamente cuando la energía se recupera completamente
                bool shouldUpdateButton = false;
                if (currentEnergy >= maxEnergy)
                {
                    shouldUpdateButton = true;
                }
                if (previousIsSleeping && !isSleeping)
                {
                    shouldUpdateButton = true;

                    usedMessageIndices.Clear();
                    lastDisplayedMessage = "";
                }
                
                if (shouldUpdateButton)
                {
                    UpdateActionButtonsState();
                }
                
                // Guardar el estado actual para la próxima verificación
                previousIsSleeping = isSleeping;
            }
            
            // Actualizar estado de botones de acción periódicamente
            UpdateActionButtonsState();
        }
    }

    /// <summary>
    /// Actualiza el color del slider según el porcentaje de valor.
    /// Verde: 100% a 75% (excluido)
    /// Naranja: 75% a 64% (75% incluido)
    /// Amarillo: 64% a 35% (excluido)
    /// Rojo: 35% a 0%
    /// </summary>
    private void UpdateSliderColor(Slider slider, int currentValue, int maxValue)
    {
        if (slider == null || maxValue <= 0)
            return;

        float percent = (float)currentValue / maxValue * 100f;
        
        // Buscar el componente Image del fill del slider
        Image fillImage = slider.fillRect?.GetComponent<Image>();
        if (fillImage == null)
        {
            // Intentar buscar en los hijos
            fillImage = slider.GetComponentInChildren<Image>();
        }

        if (fillImage != null)
        {
            Color targetColor;
            
            if (percent > 75f)
            {
                // Verde: 100% a 75% (excluido)
                targetColor = Color.green;
            }
            else if (percent > 64f)
            {
                // Naranja: 75% a 64% (75% incluido)
                targetColor = new Color(1f, 0.5f, 0f, 1f); // Naranja (RGB: 255, 128, 0)
            }
            else if (percent > 35f)
            {
                // Amarillo: 64% a 35% (excluido)
                targetColor = Color.yellow;
            }
            else
            {
                // Rojo: 35% a 0%
                targetColor = Color.red;
            }
            
            fillImage.color = targetColor;
        }
    }

    /// <summary>
    /// Inicializa los componentes necesarios para las animaciones.
    /// </summary>
    private void InitializeAnimationComponents()
    {
        if (animationTarget == null)
            return;

        // Buscar o crear Animator
        if (animationTargetAnimator == null)
        {
            animationTargetAnimator = animationTarget.GetComponent<Animator>();
            if (animationTargetAnimator == null)
            {
                animationTargetAnimator = animationTarget.GetComponentInChildren<Animator>();
            }
            if (animationTargetAnimator == null)
            {
                animationTargetAnimator = animationTarget.AddComponent<Animator>();
            }
        }

        // Buscar SpriteRenderer o Image para asignar el sprite
        if (animationSpriteRenderer == null)
        {
            animationSpriteRenderer = animationTarget.GetComponent<SpriteRenderer>();
            if (animationSpriteRenderer == null)
            {
                animationSpriteRenderer = animationTarget.GetComponentInChildren<SpriteRenderer>();
            }
        }

        if (animationImage == null)
        {
            animationImage = animationTarget.GetComponent<Image>();
            if (animationImage == null)
            {
                animationImage = animationTarget.GetComponentInChildren<Image>();
            }
        }

        // Asignar sprite si está configurado
        if (animationSprite != null)
        {
            if (animationSpriteRenderer != null)
            {
                animationSpriteRenderer.sprite = animationSprite;
            }
            else if (animationImage != null)
            {
                animationImage.sprite = animationSprite;
            }
        }
    }

    /// <summary>
    /// Establece el alpha del panel de animaciones (0 = invisible, 1 = visible).
    /// </summary>
    private void SetAnimationPanelAlpha(float alpha)
    {
        if (animationPanel == null)
            return;

        // Buscar todos los componentes Image y SpriteRenderer en el panel y sus hijos
        Image[] images = animationPanel.GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            if (img != null)
            {
                Color color = img.color;
                color.a = alpha;
                img.color = color;
            }
        }

        SpriteRenderer[] spriteRenderers = animationPanel.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
            {
                Color color = sr.color;
                color.a = alpha;
                sr.color = color;
            }
        }
    }

    /// <summary>
    /// Corrutina que reproduce animaciones idle aleatoriamente una detrás de otra.
    /// Usa la duración real de cada animación, no un tiempo fijo.
    /// </summary>
    private IEnumerator PlayRandomIdleAnimations()
    {
        if (idleAnimations == null || idleAnimations.Length == 0 || animationTargetAnimator == null)
            yield break;

        while (true)
        {
            // Verificar si el panel General Breed está activo antes de continuar
            // Si no está activo, salir de la corrutina
            if (panelGeneralBreed != null && !panelGeneralBreed.activeSelf)
            {
                // El panel se cerró, salir de la corrutina
                idleAnimationCoroutine = null;
                yield break;
            }
            
            // Seleccionar una animación aleatoria del pool
            int randomIndex = UnityEngine.Random.Range(0, idleAnimations.Length);
            AnimationConfig selectedAnimation = idleAnimations[randomIndex];

            if (selectedAnimation != null && selectedAnimation.clip != null)
            {
                // Asignar controller si está configurado
                if (selectedAnimation.controller != null && animationTargetAnimator != null)
                {
                    animationTargetAnimator.runtimeAnimatorController = selectedAnimation.controller;
                }

                // Reproducir la animación
                animationTargetAnimator.Play(selectedAnimation.clip.name, 0, 0f);

                // Esperar la duración real de la animación
                float animationDuration = selectedAnimation.clip.length;
                yield return new WaitForSeconds(animationDuration);
            }
            else
            {
                // Si la animación es null, esperar un frame y continuar
                yield return null;
            }
        }
    }
}

