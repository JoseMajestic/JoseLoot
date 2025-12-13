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
    
    // ===== CONFIGURACIÓN =====
    [Header("Configuración")]
    [Tooltip("Cantidad que llena cada acción (0-100)")]
    [Range(0, 100)]
    [SerializeField] private int actionFillAmount = 50;
    
    [Tooltip("Velocidad de typewriter para mensajes (caracteres por segundo)")]
    [Range(10f, 100f)]
    [SerializeField] private float typewriterSpeed = 30f;
    
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
    
    // Pool de mensajes del héroe (se pueden expandir)
    private readonly string[] heroMessages = new string[]
    {
        "¡Estoy listo para la aventura!",
        "Necesito entrenar más...",
        "¡Qué hambre tengo!",
        "Me siento feliz hoy.",
        "Necesito descansar un poco.",
        "¡Vamos a mejorar!",
        "Estoy aprendiendo mucho.",
        "¡Hora de trabajar duro!"
    };
    
    private void Start()
    {
        // Obtener referencias
        gameDataManager = GameDataManager.Instance;
        energySystem = FindFirstObjectByType<EnergySystem>();
        
        if (gameDataManager == null)
        {
            return;
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
    }
    
    private void OnDestroy()
    {
        // Detener corrutinas
        if (decayCoroutine != null)
            StopCoroutine(decayCoroutine);
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
    }
    
    /// <summary>
    /// Configura los listeners de los botones.
    /// </summary>
    private void SetupButtons()
    {
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
    /// Llena la barra de hambre.
    /// </summary>
    public void OnEatButtonClicked()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        profile.breedHunger = Mathf.Min(100, profile.breedHunger + actionFillAmount);
        gameDataManager.SavePlayerProfile();
        RefreshBreedStatsUI();
        UpdateActionButtonsState(); // Actualizar estado del botón de dormir
    }
    
    /// <summary>
    /// Llena la barra de disciplina.
    /// </summary>
    public void OnStudyButtonClicked()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        profile.breedDiscipline = Mathf.Min(100, profile.breedDiscipline + actionFillAmount);
        gameDataManager.SavePlayerProfile();
        RefreshBreedStatsUI();
        UpdateActionButtonsState(); // Actualizar estado del botón de dormir
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
        
        // Actualizar UI de energía
        RefreshBreedStatsUI();
        
        // Actualizar estado de botones (el botón de dormir se deshabilitará)
        UpdateActionButtonsState();
    }
    
    /// <summary>
    /// Llena la barra de felicidad.
    /// </summary>
    public void OnPlayButtonClicked()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        profile.breedHappiness = Mathf.Min(100, profile.breedHappiness + actionFillAmount);
        gameDataManager.SavePlayerProfile();
        RefreshBreedStatsUI();
        UpdateActionButtonsState(); // Actualizar estado del botón de dormir
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
    /// Llena la barra de higiene.
    /// </summary>
    public void OnCleanButtonClicked()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        profile.breedHygiene = Mathf.Min(100, profile.breedHygiene + actionFillAmount);
        gameDataManager.SavePlayerProfile();
        RefreshBreedStatsUI();
        UpdateActionButtonsState(); // Actualizar estado del botón de dormir
    }
    
    /// <summary>
    /// Llena la barra de trabajo.
    /// </summary>
    public void OnWorkButtonClicked()
    {
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, despertarlo
        if (energySystem != null && profile.isSleeping)
        {
            energySystem.WakeUp();
        }
        
        profile.breedWork = Mathf.Min(100, profile.breedWork + actionFillAmount);
        gameDataManager.SavePlayerProfile();
        RefreshBreedStatsUI();
        UpdateActionButtonsState(); // Actualizar estado del botón de dormir
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
    /// Muestra un mensaje aleatorio del héroe cada cierto intervalo (efecto typewriter).
    /// </summary>
    private IEnumerator DisplayRandomMessage()
    {
        while (true)
        {
            yield return new WaitForSeconds(messageInterval);
            
            if (messageText == null || heroMessages.Length == 0)
                continue;
            
            // Seleccionar mensaje aleatorio
            string message = heroMessages[UnityEngine.Random.Range(0, heroMessages.Length)];
            
            // Mostrar con typewriter
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            
            typewriterCoroutine = StartCoroutine(DisplayTextWithTypewriter(message));
        }
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
        
        // Determinar tipo de título
        string titleType = TitleSystem.DetermineTitleType(
            profile.breedWork,
            profile.breedHunger,
            profile.breedHappiness,
            profile.breedEnergy,
            profile.breedHygiene,
            profile.breedDiscipline,
            neglectTime
        );
        
        // Obtener título del tipo
        string title = TitleSystem.GetTitleFromType(titleType, profile.evolutionClass);
        
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
            workSlider.value = profile.breedWork;
        if (workText != null)
            workText.text = $"{profile.breedWork}% / 100%";
        
        // Hambre
        if (hungerSlider != null)
            hungerSlider.value = profile.breedHunger;
        if (hungerText != null)
            hungerText.text = $"{profile.breedHunger}% / 100%";
        
        // Felicidad
        if (happinessSlider != null)
            happinessSlider.value = profile.breedHappiness;
        if (happinessText != null)
            happinessText.text = $"{profile.breedHappiness}% / 100%";
        
        // Energía (obtenida desde EnergySystem, no desde PlayerProfileData)
        if (energySystem != null)
        {
            int currentEnergy = energySystem.GetCurrentEnergy();
            int maxEnergy = energySystem.GetMaxEnergy();
            
            if (energySlider != null)
            {
                energySlider.value = currentEnergy;
            }
            if (energyText != null)
            {
                energyText.text = $"{currentEnergy}% / {maxEnergy}%";
            }
        }
        else
        {
            // Si no hay EnergySystem, mostrar 0
            if (energySlider != null)
                energySlider.value = 0;
            if (energyText != null)
                energyText.text = "0% / 100%";
        }
        
        // Higiene
        if (hygieneSlider != null)
            hygieneSlider.value = profile.breedHygiene;
        if (hygieneText != null)
            hygieneText.text = $"{profile.breedHygiene}% / 100%";
        
        // Disciplina
        if (disciplineSlider != null)
            disciplineSlider.value = profile.breedDiscipline;
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
        
        string className = EvolutionSystem.GetClassName(profile.evolutionClass);
        classTitleText.text = $"{className} Clase";
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
                    energySlider.value = currentEnergy;
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
}

