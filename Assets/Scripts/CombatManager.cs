using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona el panel de combate por turnos con lógica completa de batalla.
/// Maneja el sistema de turnos, críticos, suerte y paneles de victoria/derrota.
/// </summary>
public class CombatManager : MonoBehaviour
{
    [Header("Textos de Ronda")]
    [Tooltip("Texto que muestra la ronda actual")]
    [SerializeField] private TextMeshProUGUI roundText;
    
    [Header("HP del Jugador")]
    [Tooltip("Slider de HP del jugador")]
    [SerializeField] private Slider playerHpSlider;
    
    [Tooltip("Texto de HP del jugador mostrando 'HP actual / HP total'")]
    [SerializeField] private TextMeshProUGUI playerHpText;
    
    [Tooltip("Texto de HP del jugador en porcentaje")]
    [SerializeField] private TextMeshProUGUI playerHpPercentText;
    
    [Header("HP del Enemigo")]
    [Tooltip("Slider de HP del enemigo")]
    [SerializeField] private Slider enemyHpSlider;
    
    [Tooltip("Texto de HP del enemigo")]
    [SerializeField] private TextMeshProUGUI enemyHpText;
    
    [Tooltip("Texto de HP del enemigo en porcentaje")]
    [SerializeField] private TextMeshProUGUI enemyHpPercentText;
    
    [Header("Paneles de Imagen")]
    [Tooltip("Panel donde se muestra la imagen del jugador")]
    [SerializeField] private GameObject playerImagePanel;
    
    [Tooltip("Panel donde se muestra la imagen del enemigo")]
    [SerializeField] private GameObject enemyImagePanel;
    
    [Header("Detalles de Ronda")]
    [Tooltip("Texto con detalles de la ronda (solo información durante el combate)")]
    [SerializeField] private TextMeshProUGUI roundDetailsText;
    
    [Header("Detalles de Ataque Seleccionado")]
    [Tooltip("Texto que muestra los detalles del ataque seleccionado antes de iniciar la ronda")]
    [SerializeField] private TextMeshProUGUI selectedAttackDetailsText;
    
    [Header("Panel de Combate")]
    [Tooltip("Panel principal de combate (se cierra al aceptar victoria/derrota)")]
    [SerializeField] private GameObject combatPanel;
    
    [Tooltip("Panel General Gym que se abre cuando se cierra el panel de combate (opcional)")]
    [SerializeField] private GameObject panelGeneralGym;
    
    [Header("Ataques")]
    [Tooltip("Array de botones de habilidades/ataques (cada posición contiene el botón y la instancia AttackData asociada)")]
    [SerializeField] private AttackButtonData[] attackButtons;
    
    [Tooltip("AttackData del ataque básico (opcional, para usar sprites de Damage y Effects). Si no está asignado, el ataque básico no usará sprites personalizados.")]
    [SerializeField] private AttackData basicAttackData;
    
    [Tooltip("Botón para ejecutar el ataque")]
    [SerializeField] private Button combatButton;
    
    [Tooltip("Botón que cierra el panel de combate")]
    [SerializeField] private Button closeCombatButton;

    [System.Serializable]
    public class PanelButtonData
    {
        [Tooltip("Botón que abre este panel o ejecuta un ataque")]
        public Button button;
        
        [Tooltip("Panel que se abre al presionar el botón (null si es un ataque directo)")]
        public GameObject panel;
        
        [Tooltip("Si es true, este botón ejecuta un ataque directamente (no abre panel)")]
        public bool isDirectAttack = false;
        
        [Tooltip("Índice del ataque en attackButtons si es un ataque directo (-1 si no es ataque)")]
        public int attackButtonIndex = -1;
    }

    [Header("Navegación de Paneles de Ataques")]
    [Tooltip("Botón de Ataque Básico (ataque directo, no abre panel)")]
    [SerializeField] private Button basicAttackButton;
    
    [Tooltip("Botón de Especiales (abre panel de especiales)")]
    [SerializeField] private Button specialsButton;
    
    [Tooltip("Botón de Magia (abre panel de magia)")]
    [SerializeField] private Button magicButton;

    [Header("Panel de Especiales")]
    [Tooltip("Panel que contiene los botones de especiales")]
    [SerializeField] private GameObject specialsPanel;
    
    [Tooltip("Botones dentro del panel de especiales y sus sub-paneles")]
    [SerializeField] private PanelButtonData[] specialsButtons;

    [Header("Panel de Magia")]
    [Tooltip("Panel que contiene los botones de magia")]
    [SerializeField] private GameObject magicPanel;
    
    [Tooltip("Botones dentro del panel de magia y sus sub-paneles")]
    [SerializeField] private PanelButtonData[] magicButtons;
    
    [Header("Panel de Victoria")]
    [Tooltip("Panel de victoria")]
    [SerializeField] private GameObject victoryPanel;
    
    [Tooltip("Imagen del panel de victoria")]
    [SerializeField] private Image victoryImage;
    
    [Tooltip("Texto del panel de victoria")]
    [SerializeField] private TextMeshProUGUI victoryText;
    
    [Tooltip("Botón de aceptar victoria que cierra el panel")]
    [SerializeField] private Button victoryAcceptButton;
    
    [Header("Panel de Derrota")]
    [Tooltip("Panel de derrota")]
    [SerializeField] private GameObject defeatPanel;
    
    [Tooltip("Imagen del panel de derrota")]
    [SerializeField] private Image defeatImage;
    
    [Tooltip("Texto del panel de derrota")]
    [SerializeField] private TextMeshProUGUI defeatText;
    
    [Tooltip("Botón de aceptar derrota que cierra el panel")]
    [SerializeField] private Button defeatAcceptButton;
    
    [Header("Referencias")]
    [Tooltip("Referencia a la base de datos de ataques")]
    [SerializeField] private AttackDatabase attackDatabase;
    
    [Tooltip("Para obtener stats del jugador")]
    [SerializeField] private EquipmentManager equipmentManager;
    
    [Tooltip("Para agregar monedas al ganar")]
    [SerializeField] private PlayerMoney playerMoney;
    
    [Tooltip("Textos configurables del combate")]
    [SerializeField] private CombatTexts combatTexts;
    
    [Tooltip("Referencia al AnimationManager para gestionar animaciones")]
    [SerializeField] private AnimationManager animationManager;
    
    [Header("Paneles de Estados Alterados - Jugador")]
    [Tooltip("Panel que se muestra cuando el jugador tiene buff de ataque activo")]
    [SerializeField] private GameObject playerAttackBuffPanel;
    
    [Tooltip("Panel que se muestra cuando el jugador tiene buff de defensa activo")]
    [SerializeField] private GameObject playerDefenseBuffPanel;
    
    [Tooltip("Panel que se muestra cuando el jugador está envenenado")]
    [SerializeField] private GameObject playerPoisonPanel;
    
    [Tooltip("Panel que se muestra cuando el jugador está aturdido")]
    [SerializeField] private GameObject playerStunPanel;
    
    [Header("Paneles de Estados Alterados - Enemigo")]
    [Tooltip("Panel que se muestra cuando el enemigo tiene buff de ataque activo")]
    [SerializeField] private GameObject enemyAttackBuffPanel;
    
    [Tooltip("Panel que se muestra cuando el enemigo tiene buff de defensa activo")]
    [SerializeField] private GameObject enemyDefenseBuffPanel;
    
    [Tooltip("Panel que se muestra cuando el enemigo está envenenado")]
    [SerializeField] private GameObject enemyPoisonPanel;
    
    [Tooltip("Panel que se muestra cuando el enemigo está aturdido")]
    [SerializeField] private GameObject enemyStunPanel;
    
    // Estado del combate
    private EnemyData currentEnemy = null;
    private int currentEnemyIndex = -1;
    private AttackData selectedAttack = null;  // Ataque del jugador
    private AttackData enemySelectedAttack = null;  // Ataque del enemigo (seleccionado aleatoriamente)
    private int currentRound = 0;
    
    // Stats del jugador
    private int playerMaxHp = 0;
    private int playerCurrentHp = 0;
    private int playerAtaque = 0;
    private int playerDefensa = 0;
    private int playerVelocidadAtaque = 0;
    private int playerAtaqueCritico = 0;
    private int playerDanoCritico = 0;
    private int playerSuerte = 0;
    private int playerDestreza = 0;
    
    // Stats del enemigo
    private int enemyMaxHp = 0;
    private int enemyCurrentHp = 0;
    private int enemyAtaque = 0;
    private int enemyDefensa = 0;
    private int enemyVelocidadAtaque = 0;
    private int enemyAtaqueCritico = 0;
    private int enemyDanoCritico = 0;
    private int enemySuerte = 0;
    private int enemyDestreza = 0;
    
    // Control de suerte (solo una vez por ronda)
    private bool playerLuckUsedThisRound = false;
    private bool enemyLuckUsedThisRound = false;
    
    // Control de combate
    private bool combatInProgress = false;
    private int rewardCoins = 0;
    
    // Control de typewriter
    private Coroutine currentTypewriterCoroutine = null;
    private bool isDisplayingText = false;
    
    // Efectos activos del jugador
    private int playerAttackBuffPercent = 0;
    private int playerAttackBuffRounds = 0;
    private int playerDefenseBuffPercent = 0;
    private int playerDefenseBuffRounds = 0;
    private int playerPoisonPercent = 0;
    private int playerPoisonRounds = 0; // SOLUCIÓN: Contador de rondas de veneno del jugador
    private bool playerStunned = false;
    
    // Efectos activos del enemigo
    private int enemyAttackBuffPercent = 0;
    private int enemyAttackBuffRounds = 0;
    private int enemyDefenseBuffPercent = 0;
    private int enemyDefenseBuffRounds = 0;
    private int enemyPoisonPercent = 0;
    private int enemyPoisonRounds = 0; // SOLUCIÓN: Contador de rondas de veneno del enemigo
    private bool enemyStunned = false;

    private void Start()
    {
        InitializeButtons();
        InitializePanels();
        InitializePanelNavigation();
    }

    /// <summary>
    /// Inicializa los paneles al inicio.
    /// </summary>
    private void InitializePanels()
    {
        // Asegurar que los paneles de victoria/derrota estén desactivados al inicio
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
        
        // Ocultar todos los paneles de estados alterados al inicio
        HideAllStatusPanels();
        
        // Resetear navegación de paneles al estado inicial
        ResetPanelNavigation();
    }

    /// <summary>
    /// Inicializa los listeners de los botones.
    /// </summary>
    private void InitializeButtons()
    {
        if (combatButton != null)
        {
            combatButton.onClick.AddListener(OnCombatButtonClicked);
        }

        if (closeCombatButton != null)
        {
            closeCombatButton.onClick.AddListener(OnCloseCombatButtonClicked);
        }

        if (victoryAcceptButton != null)
        {
            victoryAcceptButton.onClick.AddListener(OnVictoryAcceptClicked);
        }

        if (defeatAcceptButton != null)
        {
            defeatAcceptButton.onClick.AddListener(OnDefeatAcceptClicked);
        }

        // Inicializar listeners de botones de ataques
        if (attackButtons != null)
        {
            for (int i = 0; i < attackButtons.Length; i++)
            {
                int index = i; // Capturar índice para el closure
                if (attackButtons[i].button != null)
                {
                    attackButtons[i].button.onClick.AddListener(() => OnAttackButtonClicked(index));
                }
            }
        }
    }

    /// <summary>
    /// Inicializa la navegación de paneles jerárquicos.
    /// </summary>
    private void InitializePanelNavigation()
    {
        // Botón de ataque básico (ataque directo)
        if (basicAttackButton != null)
        {
            basicAttackButton.onClick.AddListener(() => OnBasicAttackButtonClicked());
        }

        // Botón de especiales
        if (specialsButton != null)
        {
            specialsButton.onClick.AddListener(() => OnCategoryButtonClicked(specialsPanel, specialsButtons));
        }

        // Botón de magia
        if (magicButton != null)
        {
            magicButton.onClick.AddListener(() => OnCategoryButtonClicked(magicPanel, magicButtons));
        }

        // Inicializar botones de especiales
        if (specialsButtons != null)
        {
            foreach (var buttonData in specialsButtons)
            {
                if (buttonData.button != null)
                {
                    if (buttonData.isDirectAttack && buttonData.attackButtonIndex >= 0)
                    {
                        // Es un ataque directo
                        int attackIndex = buttonData.attackButtonIndex;
                        buttonData.button.onClick.AddListener(() => OnDirectAttackButtonClicked(attackIndex));
                    }
                    else if (buttonData.panel != null)
                    {
                        // Abre un sub-panel
                        buttonData.button.onClick.AddListener(() => OnSubPanelButtonClicked(buttonData.panel, buttonData, specialsButtons));
                    }
                }
            }
        }

        // Inicializar botones de magia
        if (magicButtons != null)
        {
            foreach (var buttonData in magicButtons)
            {
                if (buttonData.button != null)
                {
                    if (buttonData.isDirectAttack && buttonData.attackButtonIndex >= 0)
                    {
                        // Es un ataque directo
                        int attackIndex = buttonData.attackButtonIndex;
                        buttonData.button.onClick.AddListener(() => OnDirectAttackButtonClicked(attackIndex));
                    }
                    else if (buttonData.panel != null)
                    {
                        // Abre un sub-panel
                        buttonData.button.onClick.AddListener(() => OnSubPanelButtonClicked(buttonData.panel, buttonData, magicButtons));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Se llama cuando se presiona el botón de ataque básico.
    /// </summary>
    private void OnBasicAttackButtonClicked()
    {
        // El ataque básico es null (se procesa como ataque básico)
        selectedAttack = null;
        
        // Mostrar detalles del ataque básico
        if (selectedAttackDetailsText != null && combatTexts != null)
        {
            selectedAttackDetailsText.text = "Ataque básico: Un golpe simple sin efectos especiales.";
        }
        else if (selectedAttackDetailsText != null)
        {
            selectedAttackDetailsText.text = "Ataque básico";
        }

        // Habilitar botón de combate
        if (combatButton != null)
        {
            combatButton.interactable = true;
        }
    }

    /// <summary>
    /// Se llama cuando se presiona un botón de categoría (Especiales o Magia).
    /// </summary>
    private void OnCategoryButtonClicked(GameObject categoryPanel, PanelButtonData[] categoryButtons)
    {
        // Cerrar todos los paneles abiertos excepto los principales
        CloseAllSubPanels();
        
        // Abrir el panel de categoría
        if (categoryPanel != null)
        {
            categoryPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Se llama cuando se presiona un botón que abre un sub-panel.
    /// </summary>
    private void OnSubPanelButtonClicked(GameObject subPanel, PanelButtonData clickedButton, PanelButtonData[] categoryButtons)
    {
        // Cerrar otros sub-paneles del mismo nivel
        if (categoryButtons != null)
        {
            foreach (var buttonData in categoryButtons)
            {
                if (buttonData.panel != null && buttonData.panel != subPanel)
                {
                    buttonData.panel.SetActive(false);
                }
            }
        }
        
        // Abrir el sub-panel
        if (subPanel != null)
        {
            subPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Se llama cuando se presiona un botón de ataque directo desde un sub-panel.
    /// </summary>
    private void OnDirectAttackButtonClicked(int attackButtonIndex)
    {
        // Seleccionar el ataque usando el índice
        OnAttackButtonClicked(attackButtonIndex);
    }

    /// <summary>
    /// Cierra todos los sub-paneles (excepto los botones principales).
    /// </summary>
    private void CloseAllSubPanels()
    {
        // Cerrar panel de especiales
        if (specialsPanel != null)
        {
            specialsPanel.SetActive(false);
        }

        // Cerrar panel de magia
        if (magicPanel != null)
        {
            magicPanel.SetActive(false);
        }

        // Cerrar todos los sub-paneles de especiales
        if (specialsButtons != null)
        {
            foreach (var buttonData in specialsButtons)
            {
                if (buttonData.panel != null)
                {
                    buttonData.panel.SetActive(false);
                }
            }
        }

        // Cerrar todos los sub-paneles de magia
        if (magicButtons != null)
        {
            foreach (var buttonData in magicButtons)
            {
                if (buttonData.panel != null)
                {
                    buttonData.panel.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Resetea la navegación al estado inicial (solo botones principales visibles).
    /// Se llama cuando se ejecuta un ataque o al iniciar el combate.
    /// </summary>
    private void ResetPanelNavigation()
    {
        CloseAllSubPanels();
    }

    /// <summary>
    /// Inicia el combate con el enemigo especificado.
    /// </summary>
    /// <param name="enemy">Datos del enemigo</param>
    /// <param name="enemyIndex">Índice del enemigo en el array del BattleManager</param>
    public void StartCombat(EnemyData enemy, int enemyIndex = -1)
    {
        if (enemy == null)
        {
            Debug.LogError("CombatManager: No se puede iniciar combate sin enemigo.");
            return;
        }

        currentEnemy = enemy;
        currentEnemyIndex = enemyIndex;
        rewardCoins = enemy.rewardCoins;
        
        // Inicializar animaciones (pasar el EnemyData para obtener el sprite)
        if (animationManager != null)
        {
            animationManager.InitializeForCombat(enemyIndex, enemy);
        }
        
        // Calcular stats del jugador desde el equipo
        CalculatePlayerStats();
        
        // Inicializar stats del enemigo
        enemyMaxHp = enemy.hp;
        enemyCurrentHp = enemy.hp;
        enemyAtaque = enemy.ataque;
        enemyDefensa = enemy.defensa;
        enemyVelocidadAtaque = enemy.velocidadAtaque;
        enemyAtaqueCritico = enemy.ataqueCritico;
        enemyDanoCritico = enemy.danoCritico;
        enemySuerte = enemy.suerte;
        enemyDestreza = enemy.destreza;
        
        // Mostrar imagen del enemigo
        if (enemyImagePanel != null)
        {
            // Buscar la imagen primero en el componente directo, luego en los hijos
            Image enemyImage = enemyImagePanel.GetComponent<Image>();
            if (enemyImage == null)
            {
                enemyImage = enemyImagePanel.GetComponentInChildren<Image>();
            }
            
            if (enemyImage != null)
            {
                if (enemy.enemySprite != null)
                {
                    enemyImage.sprite = enemy.enemySprite;
                    enemyImage.enabled = true; // Asegurar que esté habilitada
                }
                else
                {
                    Debug.LogWarning($"CombatManager: El enemigo {enemy.enemyName} no tiene sprite asignado.");
                }
            }
            else
            {
                Debug.LogWarning("CombatManager: No se encontró componente Image en enemyImagePanel.");
            }
        }
        
        // Asegurar que el panel de combate esté activo
        if (combatPanel != null)
        {
            combatPanel.SetActive(true);
        }
        
        // Asegurar que los paneles de victoria/derrota estén desactivados
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
        
        // Inicializar UI
        currentRound = 0;
        combatInProgress = true;
        playerLuckUsedThisRound = false;
        enemyLuckUsedThisRound = false;
        selectedAttack = null;
        enemySelectedAttack = null;
        
        // SOLUCIÓN: Limpiar texto del ataque seleccionado al iniciar combate
        if (selectedAttackDetailsText != null)
        {
            selectedAttackDetailsText.text = "";
        }
        
        // Limpiar detalles de ronda
        if (roundDetailsText != null)
        {
            roundDetailsText.text = "";
        }
        
        // Resetear efectos activos
        ResetActiveEffects();
        
        // Ocultar todos los paneles de estados alterados al inicio del combate
        HideAllStatusPanels();
        
        UpdateUI();
        
        // Actualizar disponibilidad de botones de ataque según nivel del héroe
        UpdateAttackButtonsAvailability();
        
        // SOLUCIÓN: Asegurar explícitamente que el botón básico esté habilitado
        if (basicAttackButton != null)
        {
            basicAttackButton.interactable = true;
        }
        
        // Deshabilitar botón de combate hasta que se seleccione un ataque
        if (combatButton != null)
        {
            combatButton.interactable = false;
        }
        
        // Resetear navegación de paneles al estado inicial
        ResetPanelNavigation();
    }

    /// <summary>
    /// Calcula las estadísticas del jugador desde el equipo equipado.
    /// </summary>
    private void CalculatePlayerStats()
    {
        if (equipmentManager == null)
        {
            Debug.LogError("CombatManager: EquipmentManager no está asignado.");
            return;
        }

        EquipmentStats stats = equipmentManager.GetTotalEquipmentStats();
        
        // El HP del jugador se calcula desde el equipo
        // Por ahora, usamos un HP base + HP del equipo
        int baseHp = 100; // HP base del jugador
        playerMaxHp = baseHp + stats.hp;
        playerCurrentHp = playerMaxHp;
        playerAtaque = stats.ataque;
        playerDefensa = stats.defensa;
        playerVelocidadAtaque = stats.velocidadAtaque;
        playerAtaqueCritico = stats.ataqueCritico;
        playerDanoCritico = stats.danoCritico;
        playerSuerte = stats.suerte;
        playerDestreza = stats.destreza;
    }

    /// <summary>
    /// Resetea todos los efectos activos al inicio del combate.
    /// </summary>
    private void ResetActiveEffects()
    {
        // Efectos del jugador
        playerAttackBuffPercent = 0;
        playerAttackBuffRounds = 0;
        playerDefenseBuffPercent = 0;
        playerDefenseBuffRounds = 0;
        playerPoisonPercent = 0;
        playerPoisonRounds = 0; // SOLUCIÓN: Resetear rondas de veneno
        playerStunned = false;
        
        // Efectos del enemigo
        enemyAttackBuffPercent = 0;
        enemyAttackBuffRounds = 0;
        enemyDefenseBuffPercent = 0;
        enemyDefenseBuffRounds = 0;
        enemyPoisonPercent = 0;
        enemyPoisonRounds = 0; // SOLUCIÓN: Resetear rondas de veneno
        enemyStunned = false;
        
        // Ocultar todos los paneles de estados alterados
        HideAllStatusPanels();
    }
    
    /// <summary>
    /// Actualiza la visibilidad de todos los paneles de estados alterados según los estados activos.
    /// </summary>
    private void UpdateStatusPanels()
    {
        // Paneles del jugador
        if (playerAttackBuffPanel != null)
        {
            playerAttackBuffPanel.SetActive(playerAttackBuffRounds > 0);
        }
        
        if (playerDefenseBuffPanel != null)
        {
            playerDefenseBuffPanel.SetActive(playerDefenseBuffRounds > 0);
        }
        
        if (playerPoisonPanel != null)
        {
            playerPoisonPanel.SetActive(playerPoisonRounds > 0);
        }
        
        if (playerStunPanel != null)
        {
            playerStunPanel.SetActive(playerStunned);
        }
        
        // Paneles del enemigo
        if (enemyAttackBuffPanel != null)
        {
            enemyAttackBuffPanel.SetActive(enemyAttackBuffRounds > 0);
        }
        
        if (enemyDefenseBuffPanel != null)
        {
            enemyDefenseBuffPanel.SetActive(enemyDefenseBuffRounds > 0);
        }
        
        if (enemyPoisonPanel != null)
        {
            enemyPoisonPanel.SetActive(enemyPoisonRounds > 0);
        }
        
        if (enemyStunPanel != null)
        {
            enemyStunPanel.SetActive(enemyStunned);
        }
    }
    
    /// <summary>
    /// Oculta todos los paneles de estados alterados.
    /// </summary>
    private void HideAllStatusPanels()
    {
        if (playerAttackBuffPanel != null) playerAttackBuffPanel.SetActive(false);
        if (playerDefenseBuffPanel != null) playerDefenseBuffPanel.SetActive(false);
        if (playerPoisonPanel != null) playerPoisonPanel.SetActive(false);
        if (playerStunPanel != null) playerStunPanel.SetActive(false);
        
        if (enemyAttackBuffPanel != null) enemyAttackBuffPanel.SetActive(false);
        if (enemyDefenseBuffPanel != null) enemyDefenseBuffPanel.SetActive(false);
        if (enemyPoisonPanel != null) enemyPoisonPanel.SetActive(false);
        if (enemyStunPanel != null) enemyStunPanel.SetActive(false);
    }

    /// <summary>
    /// Muestra un texto con efecto typewriter (letra por letra).
    /// </summary>
    private IEnumerator DisplayTextWithTypewriter(string text)
    {
        if (roundDetailsText == null)
            yield break;

        // Detener cualquier typewriter anterior
        if (currentTypewriterCoroutine != null)
        {
            StopCoroutine(currentTypewriterCoroutine);
        }

        isDisplayingText = true;
        roundDetailsText.text = "";

        // Obtener velocidad de escritura desde CombatTexts o usar valor por defecto
        float speed = combatTexts != null ? combatTexts.typewriterSpeed : 30f;
        float delayPerChar = 1f / speed;

        // Mostrar texto letra por letra
        for (int i = 0; i < text.Length; i++)
        {
            roundDetailsText.text += text[i];
            yield return new WaitForSeconds(delayPerChar);
        }

        isDisplayingText = false;
        currentTypewriterCoroutine = null;
    }

    /// <summary>
    /// Muestra un texto con efecto typewriter y espera el delay configurado.
    /// </summary>
    private IEnumerator DisplayTextWithDelay(string text)
    {
        yield return StartCoroutine(DisplayTextWithTypewriter(text));
        
        // Obtener delay desde CombatTexts o usar valor por defecto
        float delay = combatTexts != null ? combatTexts.delayBetweenMessages : 1.5f;
        yield return new WaitForSeconds(delay);
    }

    /// <summary>
    /// Formatea un texto usando string.Format con los parámetros dados.
    /// </summary>
    private string FormatText(string format, params object[] args)
    {
        if (string.IsNullOrEmpty(format))
            return "";
        
        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format; // Si falla el formato, devolver el texto original
        }
    }

    /// <summary>
    /// Obtiene el nombre del ataque con color según su tipo de efecto.
    /// Verde para curación, morado para veneno, rojo para buff de ataque, azul para buff de defensa.
    /// </summary>
    private string GetColoredAttackName(AttackData attack)
    {
        if (attack == null || string.IsNullOrEmpty(attack.attackName))
            return "";

        string colorTag = "";
        
        switch (attack.effectType)
        {
            case AttackEffectType.Heal:
                // Verde para curación
                colorTag = "<color=#00FF00>"; // Verde
                break;
                
            case AttackEffectType.Poison:
                // Morado para veneno
                colorTag = "<color=#800080>"; // Morado
                break;
                
            case AttackEffectType.AttackBuff:
                // Rojo para buff de ataque
                colorTag = "<color=red>"; // Rojo
                break;
                
            case AttackEffectType.DefenseBuff:
                // Azul para buff de defensa
                colorTag = "<color=blue>"; // Azul
                break;
                
            default:
                // Sin color para otros tipos
                return attack.attackName;
        }
        
        return $"{colorTag}{attack.attackName}</color>";
    }

    /// <summary>
    /// SOLUCIÓN: Muestra los estados activos al inicio de la ronda (ANTES de aplicar efectos y actualizar buffs).
    /// Muestra veneno, stun y buffs con sus rondas restantes.
    /// </summary>
    private IEnumerator DisplayActiveStatesAtRoundStart()
    {
        if (roundDetailsText == null || combatTexts == null || currentEnemy == null)
            yield break;

        List<string> stateMessages = new List<string>();

        // Veneno del jugador
        if (playerPoisonPercent > 0 && playerPoisonRounds > 0)
        {
            string text = FormatText(combatTexts.playerStillPoisoned, playerPoisonPercent, playerPoisonRounds);
            stateMessages.Add(text);
        }

        // Veneno del enemigo
        if (enemyPoisonPercent > 0 && enemyPoisonRounds > 0)
        {
            string text = FormatText(combatTexts.enemyStillPoisoned, currentEnemy.enemyName, enemyPoisonPercent, enemyPoisonRounds);
            stateMessages.Add(text);
        }

        // Stun del jugador (si todavía está activo desde la ronda anterior)
        if (playerStunned)
        {
            stateMessages.Add(combatTexts.playerStillStunned);
        }

        // Stun del enemigo (si todavía está activo desde la ronda anterior)
        if (enemyStunned)
        {
            string text = FormatText(combatTexts.enemyStillStunned, currentEnemy.enemyName);
            stateMessages.Add(text);
        }

        // Buff de ataque del jugador (mostrar rondas ANTES de decrementar)
        if (playerAttackBuffRounds > 0 && playerAttackBuffPercent > 0)
        {
            string text = FormatText(combatTexts.playerAttackBuffActive, playerAttackBuffPercent, playerAttackBuffRounds);
            stateMessages.Add(text);
        }

        // Buff de ataque del enemigo
        if (enemyAttackBuffRounds > 0 && enemyAttackBuffPercent > 0)
        {
            string text = FormatText(combatTexts.enemyAttackBuffActive, currentEnemy.enemyName, enemyAttackBuffPercent, enemyAttackBuffRounds);
            stateMessages.Add(text);
        }

        // Buff de defensa del jugador
        if (playerDefenseBuffRounds > 0 && playerDefenseBuffPercent > 0)
        {
            string text = FormatText(combatTexts.playerDefenseBuffActive, playerDefenseBuffPercent, playerDefenseBuffRounds);
            stateMessages.Add(text);
        }

        // Buff de defensa del enemigo
        if (enemyDefenseBuffRounds > 0 && enemyDefenseBuffPercent > 0)
        {
            string text = FormatText(combatTexts.enemyDefenseBuffActive, currentEnemy.enemyName, enemyDefenseBuffPercent, enemyDefenseBuffRounds);
            stateMessages.Add(text);
        }

        // Mostrar todos los mensajes de estado
        foreach (string message in stateMessages)
        {
            yield return StartCoroutine(DisplayTextWithDelay(message));
        }
    }

    /// <summary>
    /// SOLUCIÓN: Muestra los estados restantes al final de la ronda (DESPUÉS de UpdateBuffs).
    /// Muestra cuántas rondas quedan de buffs activos.
    /// </summary>
    private IEnumerator DisplayActiveStatesAtRoundEnd(bool hadPlayerAttackBuff, bool hadPlayerDefenseBuff, 
                                                       bool hadEnemyAttackBuff, bool hadEnemyDefenseBuff)
    {
        if (roundDetailsText == null || combatTexts == null || currentEnemy == null)
            yield break;

        List<string> stateMessages = new List<string>();

        // Buff de ataque del jugador (mostrar rondas DESPUÉS de decrementar)
        if (playerAttackBuffRounds > 0)
        {
            string text = FormatText(combatTexts.playerAttackBuffRemaining, playerAttackBuffRounds);
            stateMessages.Add(text);
        }
        else if (hadPlayerAttackBuff && playerAttackBuffRounds == 0)
        {
            // El buff acaba de terminar en esta ronda
            string text = FormatText(combatTexts.playerAttackBuffRemaining, 0);
            stateMessages.Add(text);
        }

        // Buff de ataque del enemigo
        if (enemyAttackBuffRounds > 0)
        {
            string text = FormatText(combatTexts.enemyAttackBuffRemaining, currentEnemy.enemyName, enemyAttackBuffRounds);
            stateMessages.Add(text);
        }
        else if (hadEnemyAttackBuff && enemyAttackBuffRounds == 0)
        {
            // El buff acaba de terminar en esta ronda
            string text = FormatText(combatTexts.enemyAttackBuffRemaining, currentEnemy.enemyName, 0);
            stateMessages.Add(text);
        }

        // Buff de defensa del jugador
        if (playerDefenseBuffRounds > 0)
        {
            string text = FormatText(combatTexts.playerDefenseBuffRemaining, playerDefenseBuffRounds);
            stateMessages.Add(text);
        }
        else if (hadPlayerDefenseBuff && playerDefenseBuffRounds == 0)
        {
            // El buff acaba de terminar en esta ronda
            string text = FormatText(combatTexts.playerDefenseBuffRemaining, 0);
            stateMessages.Add(text);
        }

        // Buff de defensa del enemigo
        if (enemyDefenseBuffRounds > 0)
        {
            string text = FormatText(combatTexts.enemyDefenseBuffRemaining, currentEnemy.enemyName, enemyDefenseBuffRounds);
            stateMessages.Add(text);
        }
        else if (hadEnemyDefenseBuff && enemyDefenseBuffRounds == 0)
        {
            // El buff acaba de terminar en esta ronda
            string text = FormatText(combatTexts.enemyDefenseBuffRemaining, currentEnemy.enemyName, 0);
            stateMessages.Add(text);
        }

        // Mostrar todos los mensajes de estado
        foreach (string message in stateMessages)
        {
            yield return StartCoroutine(DisplayTextWithDelay(message));
        }
    }

    /// <summary>
    /// Muestra los mensajes de estado de veneno al inicio de la ronda (sin aplicar daño).
    /// </summary>
    private IEnumerator DisplayPoisonStatusMessages()
    {
        // Veneno del enemigo (aplicado por el jugador)
        if (enemyPoisonPercent > 0 && enemyPoisonRounds > 0)
        {
            // Verificar si el enemigo ya está muerto
            if (enemyCurrentHp <= 0)
            {
                // El enemigo ya está muerto, limpiar el veneno y salir
                enemyPoisonPercent = 0;
                enemyPoisonRounds = 0;
            }
            else if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
            {
                string text = FormatText(combatTexts.enemyStillPoisoned, currentEnemy.enemyName, enemyPoisonPercent, enemyPoisonRounds);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
        }
        
        // Veneno del jugador (aplicado por el enemigo)
        if (playerPoisonPercent > 0 && playerPoisonRounds > 0)
        {
            // Verificar si el jugador ya está muerto
            if (playerCurrentHp <= 0)
            {
                // El jugador ya está muerto, limpiar el veneno y salir
                playerPoisonPercent = 0;
                playerPoisonRounds = 0;
            }
            else if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.playerStillPoisoned, playerPoisonPercent, playerPoisonRounds);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
        }
    }

    /// <summary>
    /// Calcula el daño básico promedio (sin crítico) para un atacante.
    /// </summary>
    private int CalculateBasicAttackDamage(bool isPlayer)
    {
        if (isPlayer)
        {
            // Calcular daño básico del jugador (sin crítico)
            int ataque = GetPlayerEffectiveAttack();
            int destreza = playerDestreza;
            int defensaOponente = GetEnemyEffectiveDefense();
            
            // Calcular daño total: (ataque + destreza / 2) * 1 (sin crítico)
            float totalDamage = ataque + destreza / 2f;
            
            // Calcular daño efectivo: daño_total - defensa_oponente + destreza_atacante / 2
            float effectiveDamage = totalDamage - defensaOponente + (destreza / 2f);
            
            // Mínimo 1 de daño
            return Mathf.Max(1, Mathf.RoundToInt(effectiveDamage));
        }
        else
        {
            // Calcular daño básico del enemigo (sin crítico)
            int ataque = GetEnemyEffectiveAttack();
            int destreza = enemyDestreza;
            int defensaOponente = GetPlayerEffectiveDefense();
            
            // Calcular daño total: (ataque + destreza / 2) * 1 (sin crítico)
            float totalDamage = ataque + destreza / 2f;
            
            // Calcular daño efectivo: daño_total - defensa_oponente + destreza_atacante / 2
            float effectiveDamage = totalDamage - defensaOponente + (destreza / 2f);
            
            // Mínimo 1 de daño
            return Mathf.Max(1, Mathf.RoundToInt(effectiveDamage));
        }
    }

    /// <summary>
    /// Aplica el daño de veneno al final de la ronda.
    /// SOLUCIÓN: El veneno quita un 35% del daño de un ataque básico.
    /// SOLUCIÓN: Verifica si el objetivo está muerto ANTES de aplicar el veneno.
    /// </summary>
    private IEnumerator ApplyPoisonDamage()
    {
        // Veneno del enemigo (aplicado por el jugador)
        if (enemyPoisonPercent > 0 && enemyPoisonRounds > 0)
        {
            // SOLUCIÓN: Verificar si el enemigo ya está muerto ANTES de aplicar el veneno
            if (enemyCurrentHp <= 0)
            {
                // El enemigo ya está muerto, limpiar el veneno y salir
                enemyPoisonPercent = 0;
                enemyPoisonRounds = 0;
            }
            else
            {
                // Calcular daño básico del jugador (quien aplicó el veneno)
                int basicDamage = CalculateBasicAttackDamage(true);
                
                // El veneno quita un 35% del daño básico
                int poisonDamage = Mathf.RoundToInt(basicDamage * 0.35f);
                int targetEnemyHp = Mathf.Max(0, enemyCurrentHp - poisonDamage);
                
                if (roundDetailsText != null && combatTexts != null)
                {
                    string text = FormatText(combatTexts.poisonEnemy, currentEnemy.enemyName, poisonDamage);
                    yield return StartCoroutine(DisplayTextWithDelay(text));
                }
                
                // Reducir HP del enemigo gradualmente
                yield return StartCoroutine(ReduceEnemyHPGradually(targetEnemyHp));
                
                // SOLUCIÓN: Decrementar rondas de veneno después de aplicar el daño
                enemyPoisonRounds--;
                if (enemyPoisonRounds <= 0)
                {
                    // El veneno terminó, limpiar
                    enemyPoisonPercent = 0;
                    enemyPoisonRounds = 0;
                }
                
                // Actualizar paneles de estados alterados después de aplicar veneno
                UpdateStatusPanels();
                
                yield return new WaitForSeconds(1f);
            }
        }
        
        // Veneno del jugador (aplicado por el enemigo)
        if (playerPoisonPercent > 0 && playerPoisonRounds > 0)
        {
            // SOLUCIÓN: Verificar si el jugador ya está muerto ANTES de aplicar el veneno
            if (playerCurrentHp <= 0)
            {
                // El jugador ya está muerto, limpiar el veneno y salir
                playerPoisonPercent = 0;
                playerPoisonRounds = 0;
                
                // Actualizar paneles de estados alterados
                UpdateStatusPanels();
            }
            else
            {
                // Calcular daño básico del enemigo (quien aplicó el veneno)
                int basicDamage = CalculateBasicAttackDamage(false);
                
                // El veneno quita un 35% del daño básico
                int poisonDamage = Mathf.RoundToInt(basicDamage * 0.35f);
                int targetPlayerHp = Mathf.Max(0, playerCurrentHp - poisonDamage);
                
                if (roundDetailsText != null && combatTexts != null)
                {
                    string text = FormatText(combatTexts.poisonPlayer, poisonDamage);
                    yield return StartCoroutine(DisplayTextWithDelay(text));
                }
                
                // Reducir HP del jugador gradualmente
                yield return StartCoroutine(ReducePlayerHPGradually(targetPlayerHp));
                
                // SOLUCIÓN: Decrementar rondas de veneno después de aplicar el daño
                playerPoisonRounds--;
                if (playerPoisonRounds <= 0)
                {
                    // El veneno terminó, limpiar
                    playerPoisonPercent = 0;
                    playerPoisonRounds = 0;
                }
                
                // Actualizar paneles de estados alterados después de aplicar veneno
                UpdateStatusPanels();
                
                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Actualiza la duración de los buffs activos.
    /// </summary>
    private void UpdateBuffs()
    {
        // Buffs del jugador
        if (playerAttackBuffRounds > 0)
        {
            playerAttackBuffRounds--;
            if (playerAttackBuffRounds <= 0)
            {
                playerAttackBuffPercent = 0;
            }
        }
        
        if (playerDefenseBuffRounds > 0)
        {
            playerDefenseBuffRounds--;
            if (playerDefenseBuffRounds <= 0)
            {
                playerDefenseBuffPercent = 0;
            }
        }
        
        // Buffs del enemigo
        if (enemyAttackBuffRounds > 0)
        {
            enemyAttackBuffRounds--;
            if (enemyAttackBuffRounds <= 0)
            {
                enemyAttackBuffPercent = 0;
            }
        }
        
        if (enemyDefenseBuffRounds > 0)
        {
            enemyDefenseBuffRounds--;
            if (enemyDefenseBuffRounds <= 0)
            {
                enemyDefenseBuffPercent = 0;
            }
        }
        
        // Actualizar paneles de estados alterados después de actualizar buffs
        UpdateStatusPanels();
    }

    /// <summary>
    /// Obtiene el ataque efectivo del jugador (con buffs aplicados).
    /// </summary>
    private int GetPlayerEffectiveAttack()
    {
        float multiplier = 1f + (playerAttackBuffPercent / 100f);
        return Mathf.RoundToInt(playerAtaque * multiplier);
    }

    /// <summary>
    /// Obtiene la defensa efectiva del jugador (con buffs aplicados).
    /// </summary>
    private int GetPlayerEffectiveDefense()
    {
        float multiplier = 1f + (playerDefenseBuffPercent / 100f);
        return Mathf.RoundToInt(playerDefensa * multiplier);
    }

    /// <summary>
    /// Obtiene el ataque efectivo del enemigo (con buffs aplicados).
    /// </summary>
    private int GetEnemyEffectiveAttack()
    {
        float multiplier = 1f + (enemyAttackBuffPercent / 100f);
        return Mathf.RoundToInt(enemyAtaque * multiplier);
    }

    /// <summary>
    /// Obtiene la defensa efectiva del enemigo (con buffs aplicados).
    /// </summary>
    private int GetEnemyEffectiveDefense()
    {
        float multiplier = 1f + (enemyDefenseBuffPercent / 100f);
        return Mathf.RoundToInt(enemyDefensa * multiplier);
    }

    /// <summary>
    /// Se llama cuando se hace clic en un botón de ataque.
    /// </summary>
    private void OnAttackButtonClicked(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= attackButtons.Length)
        {
            Debug.LogWarning($"CombatManager: Índice de botón inválido: {buttonIndex}");
            return;
        }

        selectedAttack = attackButtons[buttonIndex].attackData;
        
        // SOLUCIÓN: Permitir que attackData sea null (ataque básico)
        // Si es null, tratarlo como ataque básico
        if (selectedAttack == null)
        {
            // Es el ataque básico, establecer null explícitamente y mostrar detalles
            selectedAttack = null;
            
            // Mostrar detalles del ataque básico
            if (selectedAttackDetailsText != null && combatTexts != null)
            {
                selectedAttackDetailsText.text = "Ataque básico: Un golpe simple sin efectos especiales.";
            }
            else if (selectedAttackDetailsText != null)
            {
                selectedAttackDetailsText.text = "Ataque básico";
            }
            
            // Habilitar botón de combate
            if (combatButton != null)
            {
                combatButton.interactable = true;
            }
            
            return; // Salir aquí, ya que es ataque básico
        }

        // Si llegamos aquí, es un ataque especial (no básico)
        // SOLUCIÓN: Mostrar detalles del ataque en el texto específico para ataques seleccionados
        // NO usar roundDetailsText, que es solo para información de la ronda durante el combate
        if (selectedAttackDetailsText != null && combatTexts != null)
        {
            string text = FormatText(combatTexts.attackSelected, selectedAttack.attackName, selectedAttack.description);
            selectedAttackDetailsText.text = text;
        }
        else if (selectedAttackDetailsText != null)
        {
            // Si no hay combatTexts, mostrar información básica
            selectedAttackDetailsText.text = $"{selectedAttack.attackName}: {selectedAttack.description}";
        }

        // Habilitar botón de combate
        if (combatButton != null)
        {
            combatButton.interactable = true;
        }
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón de combate.
    /// </summary>
    private void OnCombatButtonClicked()
    {
        // NOTA: No verificamos si selectedAttack es null porque null es válido para el ataque básico.
        // El botón solo se habilita cuando hay un ataque seleccionado (básico o especial).

        if (!combatInProgress)
        {
            Debug.LogWarning("CombatManager: El combate no está en progreso.");
            return;
        }

        // Iniciar nueva ronda
        currentRound++;
        playerLuckUsedThisRound = false;
        enemyLuckUsedThisRound = false;

        // SOLUCIÓN: Limpiar texto del ataque seleccionado al iniciar la ronda
        // Los detalles del ataque ya no son relevantes, ahora se mostrará información de la ronda
        if (selectedAttackDetailsText != null)
        {
            selectedAttackDetailsText.text = "";
        }
        
        // Cerrar todos los paneles de navegación cuando se ejecuta el ataque
        ResetPanelNavigation();

        // Actualizar texto de ronda
        if (roundText != null)
        {
            roundText.text = $"Ronda {currentRound}";
        }

        // Ejecutar turno del jugador
        StartCoroutine(ExecuteCombatRound());
    }

    /// <summary>
    /// Ejecuta una ronda completa de combate.
    /// </summary>
    private IEnumerator ExecuteCombatRound()
    {
        // Deshabilitar botones durante el combate
        if (combatButton != null)
        {
            combatButton.interactable = false;
        }

        // SOLUCIÓN: Mostrar estados activos al inicio de la ronda (ANTES de aplicar efectos y actualizar buffs)
        yield return StartCoroutine(DisplayActiveStatesAtRoundStart());
        
        // Actualizar paneles de estados alterados al inicio de la ronda
        UpdateStatusPanels();
        
        // SOLUCIÓN: Mostrar mensajes de estado de veneno al inicio (sin aplicar daño)
        yield return StartCoroutine(DisplayPoisonStatusMessages());
        
        // SOLUCIÓN: Guardar estado de buffs antes de actualizar para detectar cuáles terminaron
        bool hadPlayerAttackBuff = playerAttackBuffRounds > 0;
        bool hadPlayerDefenseBuff = playerDefenseBuffRounds > 0;
        bool hadEnemyAttackBuff = enemyAttackBuffRounds > 0;
        bool hadEnemyDefenseBuff = enemyDefenseBuffRounds > 0;
        
        // Actualizar duración de buffs
        UpdateBuffs();
        
        // Actualizar paneles de estados alterados después de actualizar buffs
        UpdateStatusPanels();
        
        // Resetear stun al inicio de la ronda
        playerStunned = false;
        enemyStunned = false;
        
        // Actualizar paneles de estados alterados después de resetear stun
        UpdateStatusPanels();

        // Determinar orden de ataque
        bool playerAttacksFirst = DetermineAttackOrder();

        // SOLUCIÓN: Mostrar quién ataca primero antes de comenzar la ronda
        if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
        {
            string firstAttackerText;
            if (playerAttacksFirst)
            {
                // El jugador ataca primero
                firstAttackerText = FormatText(combatTexts.playerAttacksFirst, "Héroe");
            }
            else
            {
                // El enemigo ataca primero
                firstAttackerText = FormatText(combatTexts.enemyAttacksFirst, currentEnemy.enemyName);
            }
            yield return StartCoroutine(DisplayTextWithDelay(firstAttackerText));
        }

        if (playerAttacksFirst)
        {
            // Turno del jugador
            yield return StartCoroutine(ExecutePlayerTurn());
            
            // Verificar si el enemigo murió
            if (enemyCurrentHp <= 0)
            {
                yield return StartCoroutine(OnPlayerVictory());
                yield break;
            }

            // Turno del enemigo
            yield return StartCoroutine(ExecuteEnemyTurn());
            
            // Verificar si el jugador murió
            if (playerCurrentHp <= 0)
            {
                yield return StartCoroutine(OnPlayerDefeat());
                yield break;
            }
        }
        else
        {
            // Turno del enemigo primero
            yield return StartCoroutine(ExecuteEnemyTurn());
            
            // Verificar si el jugador murió
            if (playerCurrentHp <= 0)
            {
                yield return StartCoroutine(OnPlayerDefeat());
                yield break;
            }

            // Turno del jugador
            yield return StartCoroutine(ExecutePlayerTurn());
            
            // Verificar si el enemigo murió
            if (enemyCurrentHp <= 0)
            {
                yield return StartCoroutine(OnPlayerVictory());
                yield break;
            }
        }

        // SOLUCIÓN: Mostrar estados restantes al final de la ronda (DESPUÉS de UpdateBuffs)
        yield return StartCoroutine(DisplayActiveStatesAtRoundEnd(hadPlayerAttackBuff, hadPlayerDefenseBuff, hadEnemyAttackBuff, hadEnemyDefenseBuff));
        
        // SOLUCIÓN: Aplicar daño de veneno al final de la ronda (después de todos los ataques)
        yield return StartCoroutine(ApplyPoisonDamage());
        
        // Verificar si alguien murió por veneno
        if (enemyCurrentHp <= 0)
        {
            yield return StartCoroutine(OnPlayerVictory());
            yield break;
        }
        if (playerCurrentHp <= 0)
        {
            yield return StartCoroutine(OnPlayerDefeat());
            yield break;
        }
        
        // Si ambos siguen vivos, habilitar botón para siguiente ronda
        if (combatButton != null)
        {
            combatButton.interactable = true;
        }
        
        // Mostrar texto de siguiente ronda
        if (roundDetailsText != null && combatTexts != null)
        {
            yield return StartCoroutine(DisplayTextWithTypewriter(combatTexts.nextRound));
        }
    }

    /// <summary>
    /// Determina el orden de ataque basado en la velocidad.
    /// </summary>
    private bool DetermineAttackOrder()
    {
        if (playerVelocidadAtaque > enemyVelocidadAtaque)
        {
            return true; // Jugador ataca primero
        }
        else if (enemyVelocidadAtaque > playerVelocidadAtaque)
        {
            return false; // Enemigo ataca primero
        }
        else
        {
            // Empate: aleatorio
            return Random.Range(0, 2) == 0;
        }
    }

    /// <summary>
    /// Procesa un ataque y aplica sus efectos especiales.
    /// </summary>
    private IEnumerator ProcessAttack(AttackData attack, bool isPlayer)
    {
        if (attack == null)
        {
            // Ataque básico: usar basicAttackData si está disponible para los sprites
            yield return StartCoroutine(ProcessBasicAttack(isPlayer, basicAttackData));
            yield break;
        }

        switch (attack.effectType)
        {
            case AttackEffectType.Normal:
                // Ataque normal (solo daño)
                yield return StartCoroutine(ProcessNormalAttack(attack, isPlayer));
                break;
                
            case AttackEffectType.Heal:
                // Curación
                yield return StartCoroutine(ProcessHealAttack(attack, isPlayer));
                break;
                
            case AttackEffectType.Poison:
                // Veneno
                yield return StartCoroutine(ProcessPoisonAttack(attack, isPlayer));
                break;
                
            case AttackEffectType.Stun:
                // Aturdimiento
                yield return StartCoroutine(ProcessStunAttack(attack, isPlayer));
                break;
                
            case AttackEffectType.MultipleAttack:
                // Ataque múltiple
                yield return StartCoroutine(ProcessMultipleAttack(attack, isPlayer));
                break;
                
            case AttackEffectType.StrongBlow:
                // Golpe fuerte
                yield return StartCoroutine(ProcessStrongBlowAttack(attack, isPlayer));
                break;
                
            case AttackEffectType.AttackBuff:
                // Buff de ataque
                yield return StartCoroutine(ProcessAttackBuff(attack, isPlayer));
                break;
                
            case AttackEffectType.DefenseBuff:
                // Buff de defensa
                yield return StartCoroutine(ProcessDefenseBuff(attack, isPlayer));
                break;
        }
    }

    /// <summary>
    /// Procesa un ataque básico (con AttackData opcional para sprites).
    /// </summary>
    private IEnumerator ProcessBasicAttack(bool isPlayer, AttackData attack = null)
    {
        if (isPlayer)
        {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.playerBasicAttack);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Reproducir animación de ataque del jugador (una vez, 2 segundos)
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Attack));
            }
            
            // 2.5. Reproducir animación de damage del jugador (después del ataque)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Damage, damageSprite));
            }
            
            // 2.6. Reproducir animación de effects del jugador (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
            }
            
            // 3. Calcular daño
            int damage = CalculateDamage(GetPlayerEffectiveAttack(), playerDestreza, playerAtaqueCritico, playerDanoCritico, 
                                       GetEnemyEffectiveDefense(), true);
            int targetEnemyHp = Mathf.Max(0, enemyCurrentHp - damage);
            
            // 4. Mostrar texto de daño recibido por el enemigo
            if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
            {
                string text = FormatText(combatTexts.enemyReceivesDamage, currentEnemy.enemyName, damage);
                yield return StartCoroutine(DisplayTextWithTypewriter(text));
            }
            
            // 6. Reproducir animación de defensa del enemigo (una vez, 2 segundos) - ANTES de reducir HP
            if (animationManager != null)
            {
                
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Defense));
            }
            
            // 7. Enemigo vuelve a Idle y reducir HP gradualmente (mientras está en Idle)
            if (animationManager != null)
            {
                animationManager.SetEnemyIdle();
            }
            yield return StartCoroutine(ReduceEnemyHPGradually(targetEnemyHp));
        }
        else
        {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
            {
                string text = FormatText(combatTexts.enemyBasicAttack, currentEnemy.enemyName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Reproducir animación de ataque del enemigo (una vez, 2 segundos)
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Attack));
            }
            
            // 2.5. Reproducir animación de damage del enemigo (después del ataque)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Damage, damageSprite));
            }
            
            // 2.6. Reproducir animación de effects del enemigo (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
            }
            
            // 3. Calcular daño
            int damage = CalculateDamage(GetEnemyEffectiveAttack(), enemyDestreza, enemyAtaqueCritico, enemyDanoCritico, 
                                       GetPlayerEffectiveDefense(), false);
            int targetPlayerHp = Mathf.Max(0, playerCurrentHp - damage);
            
            // 4. Mostrar texto de daño recibido por el jugador
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.playerReceivesDamage, damage);
                yield return StartCoroutine(DisplayTextWithTypewriter(text));
            }
            
            // 5. Reproducir animación de defensa del jugador (una vez, 2 segundos) - ANTES de reducir HP
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Defense));
            }
            
            // 6. Jugador vuelve a Idle (loop) y reducir HP gradualmente (mientras está en Idle)
            if (animationManager != null)
            {
                animationManager.SetPlayerIdle();
            }
            yield return StartCoroutine(ReducePlayerHPGradually(targetPlayerHp));
        }
    }

    /// <summary>
    /// Procesa un ataque normal (solo daño).
    /// </summary>
    private IEnumerator ProcessNormalAttack(AttackData attack, bool isPlayer)
    {
        if (isPlayer)
        {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.playerAttack, coloredName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Reproducir animación de ataque del jugador (una vez, 2 segundos)
            // NOTA: PlayPlayerAnimation ya vuelve a Idle automáticamente al final
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Attack));
            }
            
            // 2.5. Reproducir animación de damage del jugador (después del ataque)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Damage, damageSprite));
            }
            
            // 2.6. Reproducir animación de effects del jugador (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
            }
            
            // 3. Calcular daño
            int damage = CalculateDamage(GetPlayerEffectiveAttack(), playerDestreza, playerAtaqueCritico, playerDanoCritico, 
                                       GetEnemyEffectiveDefense(), true);
            int targetEnemyHp = Mathf.Max(0, enemyCurrentHp - damage);
            
            // 4. Mostrar texto de daño recibido por el enemigo
            if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
            {
                string text = FormatText(combatTexts.enemyReceivesDamage, currentEnemy.enemyName, damage);
                yield return StartCoroutine(DisplayTextWithTypewriter(text));
            }
            
            // 5. Reproducir animación de defensa del enemigo (una vez, 2 segundos) - ANTES de reducir HP
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Defense));
            }
            
            // 7. Enemigo vuelve a Idle y reducir HP gradualmente (mientras está en Idle)
            if (animationManager != null)
            {
                animationManager.SetEnemyIdle();
            }
            yield return StartCoroutine(ReduceEnemyHPGradually(targetEnemyHp));
        }
        else
        {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.enemyAttack, currentEnemy.enemyName, coloredName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Reproducir animación de ataque del enemigo (una vez, 2 segundos)
            // NOTA: PlayEnemyAnimation ya vuelve a Idle automáticamente al final
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Attack));
            }
            
            // 2.5. Reproducir animación de damage del enemigo (después del ataque)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Damage, damageSprite));
            }
            
            // 2.6. Reproducir animación de effects del enemigo (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
            }
            
            // 3. Calcular daño
            int damage = CalculateDamage(GetEnemyEffectiveAttack(), enemyDestreza, enemyAtaqueCritico, enemyDanoCritico, 
                                       GetPlayerEffectiveDefense(), false);
            int targetPlayerHp = Mathf.Max(0, playerCurrentHp - damage);
            
            // 4. Mostrar texto de daño recibido por el jugador
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.playerReceivesDamage, damage);
                yield return StartCoroutine(DisplayTextWithTypewriter(text));
            }
            
            // 5. Reproducir animación de defensa del jugador (una vez, 2 segundos) - ANTES de reducir HP
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Defense));
            }
            
            // 6. Jugador vuelve a Idle (loop) y reducir HP gradualmente (mientras está en Idle)
            if (animationManager != null)
            {
                animationManager.SetPlayerIdle();
            }
            yield return StartCoroutine(ReducePlayerHPGradually(targetPlayerHp));
        }
    }

    /// <summary>
    /// Procesa un ataque de curación.
    /// SOLUCIÓN: Aumenta el HP gradualmente en lugar de actualizarlo directamente.
    /// </summary>
    private IEnumerator ProcessHealAttack(AttackData attack, bool isPlayer)
    {
        int healPercent = attack.effectValue; // 25, 50, 75, 100
        int maxHp = isPlayer ? playerMaxHp : enemyMaxHp;
        int currentHp = isPlayer ? playerCurrentHp : enemyCurrentHp;
        
        int healAmount = Mathf.RoundToInt(maxHp * healPercent / 100f);
        int newHp = Mathf.Min(maxHp, currentHp + healAmount);
        int actualHeal = newHp - currentHp;
        
        if (isPlayer)
        {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.playerAttack, coloredName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Asegurar que el jugador esté en Idle antes de mostrar Damage/Effects
            if (animationManager != null)
            {
                animationManager.SetPlayerIdle();
            }
            
            // 3. Reproducir animación de damage del jugador (sin animación de ataque, es un efecto de soporte)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Damage, damageSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 4. Reproducir animación de effects del jugador (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
        }
            
            // 5. Mostrar texto de curación
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.playerHeal, coloredName, actualHeal);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 4. Aumentar HP gradualmente
            yield return StartCoroutine(IncreasePlayerHPGradually(newHp));
            }
            else
            {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.enemyAttack, currentEnemy.enemyName, coloredName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Asegurar que el enemigo esté en Idle antes de mostrar Damage/Effects
            if (animationManager != null)
            {
                animationManager.SetEnemyIdle();
            }
            
            // 3. Reproducir animación de damage del enemigo (sin animación de ataque, es un efecto de soporte)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Damage, damageSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 4. Reproducir animación de effects del enemigo (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 5. Mostrar texto de curación
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.enemyHeal, currentEnemy.enemyName, coloredName, actualHeal);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 4. Aumentar HP gradualmente
            yield return StartCoroutine(IncreaseEnemyHPGradually(newHp));
        }
    }

    /// <summary>
    /// Procesa un ataque de veneno.
    /// SOLUCIÓN: Establece las rondas de veneno cuando se aplica (usa duration del AttackData o 3 rondas por defecto).
    /// NOTA: El veneno se aplica al final de cada ronda, no al inicio. El daño es 35% del daño de un ataque básico.
    /// </summary>
    private IEnumerator ProcessPoisonAttack(AttackData attack, bool isPlayer)
    {
        // Primero aplica el daño normal
        yield return StartCoroutine(ProcessNormalAttack(attack, isPlayer));
        
        // Luego aplica el veneno (se aplicará al final de cada ronda)
        int poisonPercent = attack.effectValue; // 10, 15, 20 (este valor ya no se usa para el daño, solo para el mensaje)
        
        // SOLUCIÓN: Determinar duración del veneno (usar duration del AttackData si está disponible, o 3 rondas por defecto)
        int poisonDuration = attack.duration > 0 ? attack.duration : 3; // Por defecto 3 rondas
        
        if (isPlayer)
        {
            enemyPoisonPercent = poisonPercent;
            enemyPoisonRounds = poisonDuration; // SOLUCIÓN: Establecer rondas de veneno
            
            // Actualizar paneles de estados alterados
            UpdateStatusPanels();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.poisonAppliedEnemy, currentEnemy.enemyName, poisonPercent);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            playerPoisonPercent = poisonPercent;
            playerPoisonRounds = poisonDuration; // SOLUCIÓN: Establecer rondas de veneno
            
            // Actualizar paneles de estados alterados
            UpdateStatusPanels();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.poisonAppliedPlayer, poisonPercent);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Procesa un ataque de aturdimiento.
    /// </summary>
    private IEnumerator ProcessStunAttack(AttackData attack, bool isPlayer)
    {
        // Primero aplica el daño normal
        yield return StartCoroutine(ProcessNormalAttack(attack, isPlayer));
        
        // Calcular probabilidad de stun: 50% + suerte%
        int suerte = isPlayer ? playerSuerte : enemySuerte;
        float stunChance = 50f + suerte;
        stunChance = Mathf.Clamp(stunChance, 0f, 100f);
        
        bool stunSuccess = Random.Range(0f, 100f) < stunChance;
        
        if (stunSuccess)
        {
            if (isPlayer)
            {
                enemyStunned = true;
                
                // Actualizar paneles de estados alterados
                UpdateStatusPanels();
                
                if (roundDetailsText != null && combatTexts != null)
                {
                    string text = FormatText(combatTexts.stunEnemy, currentEnemy.enemyName);
                    yield return StartCoroutine(DisplayTextWithDelay(text));
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                playerStunned = true;
                
                // Actualizar paneles de estados alterados
                UpdateStatusPanels();
                
                if (roundDetailsText != null && combatTexts != null)
                {
                    string text = FormatText(combatTexts.stunPlayer);
                    yield return StartCoroutine(DisplayTextWithDelay(text));
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
        }
        else
        {
            if (roundDetailsText != null && combatTexts != null)
            {
                string target = isPlayer ? currentEnemy.enemyName : "el jugador";
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.stunFailed, coloredName, target);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Procesa un ataque múltiple.
    /// SOLUCIÓN: Verifica si el objetivo está muerto antes de cada golpe para evitar ataques innecesarios.
    /// </summary>
    private IEnumerator ProcessMultipleAttack(AttackData attack, bool isPlayer)
    {
        int numHits = attack.effectValue; // 2, 3, 4
        
        for (int i = 0; i < numHits; i++)
        {
            // SOLUCIÓN: Verificar si el objetivo ya está muerto ANTES de ejecutar el golpe
            if (isPlayer && enemyCurrentHp <= 0)
                yield break;
            if (!isPlayer && playerCurrentHp <= 0)
                yield break;
            
            // Pasar el AttackData para usar sus sprites en cada golpe
            yield return StartCoroutine(ProcessBasicAttack(isPlayer, attack));
            
            // Verificar si el objetivo murió después del golpe
            if (isPlayer && enemyCurrentHp <= 0)
                yield break;
            if (!isPlayer && playerCurrentHp <= 0)
                yield break;
        }
        
        if (roundDetailsText != null && combatTexts != null)
        {
            string attacker = isPlayer ? "Jugador" : currentEnemy.enemyName;
            string coloredName = GetColoredAttackName(attack);
            string text = FormatText(combatTexts.multipleAttack, attacker, numHits, coloredName);
            yield return StartCoroutine(DisplayTextWithDelay(text));
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Procesa un golpe fuerte.
    /// </summary>
    private IEnumerator ProcessStrongBlowAttack(AttackData attack, bool isPlayer)
    {
        int multiplier = attack.effectValue; // 2, 3, 4
        
        if (isPlayer)
        {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.playerAttack, coloredName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Reproducir animación de ataque del jugador (una vez, 2 segundos)
            // NOTA: PlayPlayerAnimation ya vuelve a Idle automáticamente al final
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Attack));
            }
            
            // 2.5. Reproducir animación de damage del jugador (después del ataque)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Damage, damageSprite));
            }
            
            // 2.6. Reproducir animación de effects del jugador (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
            }
            
            // 3. Calcular daño
            int baseDamage = CalculateDamage(GetPlayerEffectiveAttack(), playerDestreza, playerAtaqueCritico, playerDanoCritico, 
                                            GetEnemyEffectiveDefense(), true);
            int totalDamage = baseDamage * multiplier;
            int targetEnemyHp = Mathf.Max(0, enemyCurrentHp - totalDamage);
            
            // 4. Mostrar texto de daño recibido por el enemigo
            if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
            {
                string text = FormatText(combatTexts.enemyReceivesDamage, currentEnemy.enemyName, totalDamage);
                yield return StartCoroutine(DisplayTextWithTypewriter(text));
            }
            
            // 5. Reproducir animación de defensa del enemigo (una vez, 2 segundos) - ANTES de reducir HP
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Defense));
            }
            
            // 6. Enemigo vuelve a Idle y reducir HP gradualmente (mientras está en Idle)
            if (animationManager != null)
            {
                animationManager.SetEnemyIdle();
            }
            yield return StartCoroutine(ReduceEnemyHPGradually(targetEnemyHp));
        }
        else
        {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.enemyAttack, currentEnemy.enemyName, coloredName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Reproducir animación de ataque del enemigo (una vez, 2 segundos)
            // NOTA: PlayEnemyAnimation ya vuelve a Idle automáticamente al final
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Attack));
            }
            
            // 2.5. Reproducir animación de damage del enemigo (después del ataque)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Damage, damageSprite));
            }
            
            // 2.6. Reproducir animación de effects del enemigo (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
            }
            
            // 3. Calcular daño
            int baseDamage = CalculateDamage(GetEnemyEffectiveAttack(), enemyDestreza, enemyAtaqueCritico, enemyDanoCritico, 
                                            GetPlayerEffectiveDefense(), false);
            int totalDamage = baseDamage * multiplier;
            int targetPlayerHp = Mathf.Max(0, playerCurrentHp - totalDamage);
            
            // 4. Mostrar texto de daño recibido por el jugador
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.playerReceivesDamage, totalDamage);
                yield return StartCoroutine(DisplayTextWithTypewriter(text));
            }
            
            // 5. Reproducir animación de defensa del jugador (una vez, 2 segundos) - ANTES de reducir HP
            if (animationManager != null)
            {
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Defense));
            }
            
            // 6. Jugador vuelve a Idle (loop) y reducir HP gradualmente (mientras está en Idle)
            if (animationManager != null)
            {
                animationManager.SetPlayerIdle();
            }
            yield return StartCoroutine(ReducePlayerHPGradually(targetPlayerHp));
        }
    }

    /// <summary>
    /// Procesa un buff de ataque.
    /// </summary>
    private IEnumerator ProcessAttackBuff(AttackData attack, bool isPlayer)
    {
        int buffPercent = attack.effectValue; // 10, 15, 30
        int duration = attack.duration; // 3, 4, 5
        
        if (isPlayer)
        {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.playerAttack, coloredName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Asegurar que el jugador esté en Idle antes de mostrar Damage/Effects
            if (animationManager != null)
            {
                animationManager.SetPlayerIdle();
            }
            
            // 3. Reproducir animación de damage del jugador (sin animación de ataque, es un efecto de soporte)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Damage, damageSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 4. Reproducir animación de effects del jugador (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 5. Aplicar buff
            playerAttackBuffPercent = buffPercent;
            playerAttackBuffRounds = duration;
            
            // Actualizar paneles de estados alterados
            UpdateStatusPanels();
            
            // 6. Mostrar texto del buff
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.attackBuff, "Jugador", coloredName, buffPercent, duration);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            }
            else
            {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.enemyAttack, currentEnemy.enemyName, coloredName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Asegurar que el enemigo esté en Idle antes de mostrar Damage/Effects
            if (animationManager != null)
            {
                animationManager.SetEnemyIdle();
            }
            
            // 3. Reproducir animación de damage del enemigo (sin animación de ataque, es un efecto de soporte)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Damage, damageSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 4. Reproducir animación de effects del enemigo (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 5. Aplicar buff
            enemyAttackBuffPercent = buffPercent;
            enemyAttackBuffRounds = duration;
            
            // Actualizar paneles de estados alterados
            UpdateStatusPanels();
            
            // 6. Mostrar texto del buff
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.attackBuff, currentEnemy.enemyName, coloredName, buffPercent, duration);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
        }
    }

    /// <summary>
    /// Procesa un buff de defensa.
    /// </summary>
    private IEnumerator ProcessDefenseBuff(AttackData attack, bool isPlayer)
    {
        int buffPercent = attack.effectValue; // 10, 15, 20
        int duration = attack.duration; // 3, 4, 5
        
        if (isPlayer)
        {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.playerAttack, coloredName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Asegurar que el jugador esté en Idle antes de mostrar Damage/Effects
            if (animationManager != null)
            {
                animationManager.SetPlayerIdle();
            }
            
            // 3. Reproducir animación de damage del jugador (sin animación de ataque, es un efecto de soporte)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Damage, damageSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 4. Reproducir animación de effects del jugador (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 5. Aplicar buff
            playerDefenseBuffPercent = buffPercent;
            playerDefenseBuffRounds = duration;
            
            // Actualizar paneles de estados alterados
            UpdateStatusPanels();
            
            // 6. Mostrar texto del buff
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.defenseBuff, "Jugador", coloredName, buffPercent, duration);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            }
            else
            {
            // 1. Mostrar texto del ataque (solo nombre)
            if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.enemyAttack, currentEnemy.enemyName, coloredName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            // 2. Asegurar que el enemigo esté en Idle antes de mostrar Damage/Effects
            if (animationManager != null)
            {
                animationManager.SetEnemyIdle();
            }
            
            // 3. Reproducir animación de damage del enemigo (sin animación de ataque, es un efecto de soporte)
            // Usar sprite de damage del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite damageSprite = attack != null ? attack.damageSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Damage, damageSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 4. Reproducir animación de effects del enemigo (después del damage)
            // Usar sprite de effects del AttackData si está disponible
            if (animationManager != null)
            {
                Sprite effectsSprite = attack != null ? attack.effectsSprite : null;
                yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.Effects, effectsSprite));
                // Pequeño delay para asegurar que la animación se complete
                yield return new WaitForSeconds(0.1f);
            }
            
            // 5. Aplicar buff
            enemyDefenseBuffPercent = buffPercent;
            enemyDefenseBuffRounds = duration;
            
            // Actualizar paneles de estados alterados
            UpdateStatusPanels();
            
            // 6. Mostrar texto del buff
            if (roundDetailsText != null && combatTexts != null)
            {
                string coloredName = GetColoredAttackName(attack);
                string text = FormatText(combatTexts.defenseBuff, currentEnemy.enemyName, coloredName, buffPercent, duration);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
        }
    }

    /// <summary>
    /// Ejecuta el turno del jugador.
    /// </summary>
    private IEnumerator ExecutePlayerTurn()
    {
        // Verificar si el jugador está muerto antes de ejecutar el turno
        if (playerCurrentHp <= 0)
        {
            yield break; // No ejecutar turno si está muerto
        }
        
        // Verificar si el jugador está aturdido
        if (playerStunned)
        {
            if (roundDetailsText != null && combatTexts != null)
            {
                yield return StartCoroutine(DisplayTextWithDelay(combatTexts.playerStunned));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
            playerStunned = false; // Resetear stun después de perder el turno
            
            // Actualizar paneles de estados alterados después de resetear stun
            UpdateStatusPanels();
            
            yield break;
        }

        // SOLUCIÓN: Mostrar mensaje de que ahora ataca el jugador
        if (roundDetailsText != null && combatTexts != null)
        {
            string text = FormatText(combatTexts.nowPlayerAttacks, "Héroe");
            yield return StartCoroutine(DisplayTextWithDelay(text));
        }

        // Procesar el ataque seleccionado
        yield return StartCoroutine(ProcessAttack(selectedAttack, true));

        // Verificar si el enemigo murió
        if (enemyCurrentHp <= 0)
        {
            yield break;
        }

        // Calcular suerte (solo para ataques básicos/normales, una vez por ronda)
        if (!playerLuckUsedThisRound)
        {
            // Si es ataque básico (null) o normal, aplicar suerte
            if (selectedAttack == null || selectedAttack.effectType == AttackEffectType.Normal)
            {
                bool luckActivated = CalculateLuck(playerSuerte);
                if (luckActivated)
                {
                    playerLuckUsedThisRound = true;
                    
                    if (roundDetailsText != null && combatTexts != null)
                    {
                        yield return StartCoroutine(DisplayTextWithDelay(combatTexts.playerLuck));
                    }
                    else
                    {
                        yield return new WaitForSeconds(1f);
                    }

                    // Asegurar que ambos estén en Idle antes del segundo ataque
                    if (animationManager != null)
                    {
                        animationManager.SetPlayerIdle();
                        animationManager.SetEnemyIdle();
                    }

                    // Atacar de nuevo (ataque básico): usar basicAttackData si está disponible
                    yield return StartCoroutine(ProcessBasicAttack(true, basicAttackData));

                    // Verificar si el enemigo murió
                    if (enemyCurrentHp <= 0)
                    {
                        yield break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Selecciona un ataque aleatorio del enemigo de su lista de ataques disponibles.
    /// </summary>
    private void SelectRandomEnemyAttack()
    {
        if (currentEnemy == null || currentEnemy.availableAttacks == null || currentEnemy.availableAttacks.Length == 0)
        {
            enemySelectedAttack = null; // Usa ataque básico
            return;
        }
        
        // Filtrar ataques no nulos
        List<AttackData> validAttacks = new List<AttackData>();
        foreach (var attack in currentEnemy.availableAttacks)
        {
            if (attack != null)
                validAttacks.Add(attack);
        }
        
        if (validAttacks.Count == 0)
        {
            enemySelectedAttack = null; // Usa ataque básico
            return;
        }
        
        // Seleccionar aleatoriamente
        int randomIndex = Random.Range(0, validAttacks.Count);
        enemySelectedAttack = validAttacks[randomIndex];
    }

    /// <summary>
    /// Ejecuta el turno del enemigo.
    /// </summary>
    private IEnumerator ExecuteEnemyTurn()
    {
        // Verificar si el enemigo está muerto antes de ejecutar el turno
        if (enemyCurrentHp <= 0)
        {
            yield break; // No ejecutar turno si está muerto
        }
        
        // Verificar si el enemigo está aturdido
        if (enemyStunned)
        {
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.enemyStunned, currentEnemy.enemyName);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
            enemyStunned = false; // Resetear stun después de perder el turno
            
            // Actualizar paneles de estados alterados después de resetear stun
            UpdateStatusPanels();
            
            yield break;
        }

        // SOLUCIÓN: Mostrar mensaje de que ahora ataca el enemigo
        if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
        {
            string text = FormatText(combatTexts.nowEnemyAttacks, currentEnemy.enemyName);
            yield return StartCoroutine(DisplayTextWithDelay(text));
        }

        // Seleccionar ataque aleatorio del enemigo
        SelectRandomEnemyAttack();
        
        // Procesar el ataque seleccionado
        yield return StartCoroutine(ProcessAttack(enemySelectedAttack, false));

        // Verificar si el jugador murió
        if (playerCurrentHp <= 0)
        {
            yield break;
        }

        // Calcular suerte (solo para ataques básicos/normales, una vez por ronda)
        if (!enemyLuckUsedThisRound)
        {
            // Si es ataque básico (null) o normal, aplicar suerte
            if (enemySelectedAttack == null || enemySelectedAttack.effectType == AttackEffectType.Normal)
            {
                bool luckActivated = CalculateLuck(enemySuerte);
                if (luckActivated)
                {
                    enemyLuckUsedThisRound = true;
                    
                    if (roundDetailsText != null && combatTexts != null)
                    {
                        string text = FormatText(combatTexts.enemyLuck, currentEnemy.enemyName);
                        yield return StartCoroutine(DisplayTextWithDelay(text));
                    }
                    else
                    {
                        yield return new WaitForSeconds(1f);
                    }

                    // Asegurar que ambos estén en Idle antes del segundo ataque
                    if (animationManager != null)
                    {
                        animationManager.SetPlayerIdle();
                        animationManager.SetEnemyIdle();
                    }

                    // Atacar de nuevo (ataque básico): usar basicAttackData si está disponible
                    yield return StartCoroutine(ProcessBasicAttack(false, basicAttackData));

                    // Verificar si el jugador murió
                    if (playerCurrentHp <= 0)
                    {
                        yield break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calcula el daño de un ataque.
    /// </summary>
    private int CalculateDamage(int ataque, int destreza, int ataqueCritico, int danoCritico, 
                                int defensaOponente, bool isPlayer)
    {
        // Determinar si es crítico
        bool isCritical = Random.Range(0, 100) < ataqueCritico;
        
        // Calcular multiplicador de crítico
        float critMultiplier = isCritical ? (1f + (danoCritico / 10f) + 0.1f) : 1f;
        
        // Calcular daño total: (ataque + destreza / 2) * multiplicador_critico
        float totalDamage = (ataque + destreza / 2f) * critMultiplier;
        
        // Calcular daño efectivo: daño_total - defensa_oponente + destreza_atacante / 2
        float effectiveDamage = totalDamage - defensaOponente + (destreza / 2f);
        
        // Mínimo 1 de daño
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(effectiveDamage));
        
        return finalDamage;
    }

    /// <summary>
    /// Calcula si la suerte se activa.
    /// </summary>
    private bool CalculateLuck(int suerte)
    {
        // Probabilidad: 35% base + 1% por cada punto de suerte (máximo 100%)
        float luckChance = 35f + suerte;
        luckChance = Mathf.Clamp(luckChance, 0f, 100f);
        
        return Random.Range(0f, 100f) < luckChance;
    }

    /// <summary>
    /// Actualiza la UI del HP del jugador.
    /// </summary>
    private void UpdatePlayerHP()
    {
        if (playerHpSlider != null)
        {
            playerHpSlider.maxValue = playerMaxHp;
            playerHpSlider.value = playerCurrentHp;
            
            // Actualizar color del slider según porcentaje
            UpdateSliderColor(playerHpSlider, playerCurrentHp, playerMaxHp);
        }

        if (playerHpText != null)
        {
            playerHpText.text = $"{playerCurrentHp} / {playerMaxHp}";
        }

        if (playerHpPercentText != null)
        {
            float percent = (float)playerCurrentHp / playerMaxHp * 100f;
            playerHpPercentText.text = $"{percent:F0}%";
        }
    }

    /// <summary>
    /// Reduce el HP del jugador gradualmente de uno en uno hasta llegar al HP objetivo.
    /// </summary>
    private IEnumerator ReducePlayerHPGradually(int targetHp)
    {
        targetHp = Mathf.Max(0, targetHp);
        
        while (playerCurrentHp > targetHp)
        {
            playerCurrentHp--;
            UpdatePlayerHP();
            yield return null; // Esperar un frame antes de reducir el siguiente punto
        }
        
        // Asegurar que llegamos exactamente al target
        playerCurrentHp = targetHp;
        UpdatePlayerHP();
    }

    /// <summary>
    /// Reduce el HP del enemigo gradualmente de uno en uno hasta llegar al HP objetivo.
    /// </summary>
    private IEnumerator ReduceEnemyHPGradually(int targetHp)
    {
        targetHp = Mathf.Max(0, targetHp);
        
        while (enemyCurrentHp > targetHp)
        {
            enemyCurrentHp--;
            UpdateEnemyHP();
            yield return null; // Esperar un frame antes de reducir el siguiente punto
        }
        
        // Asegurar que llegamos exactamente al target
        enemyCurrentHp = targetHp;
        UpdateEnemyHP();
    }

    /// <summary>
    /// Aumenta el HP del jugador gradualmente de uno en uno hasta llegar al HP objetivo.
    /// </summary>
    private IEnumerator IncreasePlayerHPGradually(int targetHp)
    {
        targetHp = Mathf.Min(playerMaxHp, targetHp);
        
        while (playerCurrentHp < targetHp)
        {
            playerCurrentHp++;
            UpdatePlayerHP();
            yield return null; // Esperar un frame antes de aumentar el siguiente punto
        }
        
        // Asegurar que llegamos exactamente al target
        playerCurrentHp = targetHp;
        UpdatePlayerHP();
    }

    /// <summary>
    /// Aumenta el HP del enemigo gradualmente de uno en uno hasta llegar al HP objetivo.
    /// </summary>
    private IEnumerator IncreaseEnemyHPGradually(int targetHp)
    {
        targetHp = Mathf.Min(enemyMaxHp, targetHp);
        
        while (enemyCurrentHp < targetHp)
        {
            enemyCurrentHp++;
            UpdateEnemyHP();
            yield return null; // Esperar un frame antes de aumentar el siguiente punto
        }
        
        // Asegurar que llegamos exactamente al target
        enemyCurrentHp = targetHp;
        UpdateEnemyHP();
    }

    /// <summary>
    /// Actualiza la UI del HP del enemigo.
    /// </summary>
    private void UpdateEnemyHP()
    {
        if (enemyHpSlider != null)
        {
            enemyHpSlider.maxValue = enemyMaxHp;
            enemyHpSlider.value = enemyCurrentHp;
            
            // Actualizar color del slider según porcentaje
            UpdateSliderColor(enemyHpSlider, enemyCurrentHp, enemyMaxHp);
        }

        if (enemyHpText != null)
        {
            enemyHpText.text = $"{enemyCurrentHp} / {enemyMaxHp}";
        }

        if (enemyHpPercentText != null)
        {
            float percent = (float)enemyCurrentHp / enemyMaxHp * 100f;
            enemyHpPercentText.text = $"{percent:F0}%";
        }
    }

    /// <summary>
    /// Actualiza toda la UI.
    /// </summary>
    private void UpdateUI()
    {
        UpdatePlayerHP();
        UpdateEnemyHP();

        if (roundText != null)
        {
            roundText.text = $"Ronda {currentRound}";
        }
    }

    /// <summary>
    /// Actualiza el color del slider según el porcentaje de HP.
    /// Verde: 100% a 65%
    /// Amarillo: 65% a 35%
    /// Rojo: 35% a 0%
    /// </summary>
    private void UpdateSliderColor(Slider slider, int currentHp, int maxHp)
    {
        if (slider == null || maxHp <= 0)
            return;

        float percent = (float)currentHp / maxHp * 100f;
        
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
            
            if (percent > 65f)
            {
                // Verde: 100% a 65%
                targetColor = Color.green;
            }
            else if (percent > 35f)
            {
                // Amarillo: 65% a 35%
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
    /// Se llama cuando el jugador gana.
    /// SOLUCIÓN: Ahora es una corrutina que muestra el texto de debilitación antes del panel de victoria.
    /// </summary>
    private IEnumerator OnPlayerVictory()
    {
        combatInProgress = false;

        // Reproducir animación KO del enemigo
        if (animationManager != null)
        {
            yield return StartCoroutine(animationManager.PlayEnemyAnimation(AnimationManager.AnimationState.KO));
            // NO volver a Idle: la animación KO es la última y debe permanecer en KO hasta que se presione "Aceptar"
        }

        // SOLUCIÓN: Mostrar texto de debilitación del enemigo ANTES del panel de victoria
        if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
        {
            string defeatText = FormatText(combatTexts.enemyDefeated, currentEnemy.enemyName);
            yield return StartCoroutine(DisplayTextWithDelay(defeatText));
        }

        // Mostrar texto de victoria del jugador
        if (roundDetailsText != null && combatTexts != null)
        {
            yield return StartCoroutine(DisplayTextWithDelay(combatTexts.playerWins));
        }

        // Agregar monedas
        if (playerMoney != null)
        {
            playerMoney.AddMoney(rewardCoins);
        }

        // Marcar enemigo como derrotado (para desbloqueo secuencial)
        if (currentEnemy != null && GameDataManager.Instance != null)
        {
            GameDataManager.Instance.MarkEnemyDefeated(currentEnemy.enemyName);
        }

        // Otorgar experiencia al derrotar al enemigo
        if (currentEnemy != null && GameDataManager.Instance != null)
        {
            int experienceReward = currentEnemy.experienceReward;
            if (experienceReward > 0)
            {
                GameDataManager.Instance.AddHeroExperience(experienceReward);
            }
        }

        // Incrementar estadísticas de pelea ganada y enfrentamiento
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.IncrementTotalClashes();
            GameDataManager.Instance.IncrementTotalWonFights();
        }

        // Desbloquear siguiente nivel (sistema antiguo, mantener por compatibilidad)
        if (currentEnemy != null && currentEnemy.requiredLevel > 0)
        {
            UnlockNextEnemyLevel(currentEnemy.requiredLevel);
        }

        // Guardar progreso
        SaveProgress();

        // Mostrar panel de victoria
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (victoryText != null)
        {
            victoryText.text = $"¡Victoria!\nHas ganado {rewardCoins} monedas.";
        }
    }

    /// <summary>
    /// Se llama cuando el jugador pierde.
    /// SOLUCIÓN: Ahora es una corrutina que muestra el texto de debilitación antes del panel de derrota.
    /// </summary>
    private IEnumerator OnPlayerDefeat()
    {
        combatInProgress = false;

        // Reproducir animación KO del jugador
        if (animationManager != null)
        {
            yield return StartCoroutine(animationManager.PlayPlayerAnimation(AnimationManager.AnimationState.KO));
            // NO volver a Idle: la animación KO es la última y debe permanecer en KO hasta que se presione "Aceptar"
        }

        // SOLUCIÓN: Mostrar texto de debilitación del jugador ANTES del panel de derrota
        if (roundDetailsText != null && combatTexts != null)
        {
            yield return StartCoroutine(DisplayTextWithDelay(combatTexts.playerDefeated));
        }

        // Mostrar texto de victoria del enemigo
        if (roundDetailsText != null && combatTexts != null && currentEnemy != null)
        {
            string winText = FormatText(combatTexts.enemyWins, currentEnemy.enemyName);
            yield return StartCoroutine(DisplayTextWithDelay(winText));
        }

        // Incrementar estadísticas de pelea perdida y enfrentamiento
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.IncrementTotalClashes();
            GameDataManager.Instance.IncrementTotalLostFights();
        }

        // Mostrar panel de derrota
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        if (defeatText != null)
        {
            defeatText.text = "Has sido derrotado...";
        }
    }

    /// <summary>
    /// Desbloquea el siguiente nivel de enemigo.
    /// </summary>
    private void UnlockNextEnemyLevel(int currentLevel)
    {
        // Desbloquear el nivel siguiente (currentLevel + 1)
        int nextLevel = currentLevel + 1;
        
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.UnlockEnemyLevel(nextLevel);
            Debug.Log($"Nivel de enemigo {nextLevel} desbloqueado.");
        }
        else
        {
            Debug.LogWarning("CombatManager: GameDataManager no está disponible. No se puede desbloquear nivel.");
        }
    }

    /// <summary>
    /// Guarda el progreso del jugador.
    /// </summary>
    private void SaveProgress()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SavePlayerProfile();
        }
    }

    /// <summary>
    /// Se llama cuando se acepta la victoria.
    /// Las monedas ya fueron agregadas en OnPlayerVictory().
    /// </summary>
    private void OnVictoryAcceptClicked()
    {
        // Resetear todos los efectos activos (buffs, veneno, stun) al aceptar victoria
        ResetActiveEffects();
        
        // Resetear animaciones a Idle
        if (animationManager != null)
        {
            animationManager.SetPlayerIdle();
            animationManager.SetEnemyIdle();
        }

        // Cerrar panel de victoria
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        // Cerrar panel de combate
        CloseCombatPanel();
    }

    /// <summary>
    /// Se llama cuando se acepta la derrota.
    /// NO se agregan monedas.
    /// </summary>
    private void OnDefeatAcceptClicked()
    {
        // Resetear todos los efectos activos (buffs, veneno, stun) al aceptar derrota
        ResetActiveEffects();
        
        // Resetear animaciones a Idle
        if (animationManager != null)
        {
            animationManager.SetPlayerIdle();
            animationManager.SetEnemyIdle();
        }

        // Cerrar panel de derrota
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }

        // Cerrar panel de combate
        CloseCombatPanel();
    }

    /// <summary>
    /// Se llama cuando se hace clic en el botón de cerrar el panel de combate.
    /// </summary>
    private void OnCloseCombatButtonClicked()
    {
        CloseCombatPanel();
    }

    /// <summary>
    /// Cierra el panel de combate y abre el Panel General Gym si está asignado.
    /// </summary>
    private void CloseCombatPanel()
    {
        // Ocultar todas las animaciones antes de cerrar el panel
        if (animationManager != null)
        {
            animationManager.HideAllAnimations();
        }

        if (combatPanel != null)
        {
            combatPanel.SetActive(false);
        }
        else
        {
            // Fallback: desactivar el GameObject del CombatManager si no hay panel asignado
            gameObject.SetActive(false);
        }
        
        // SOLUCIÓN: Abrir el Panel General Gym cuando se cierra el combate
        // Esto permite volver a la selección de enemigos después de cerrar el combate
        
        // Si la referencia se perdió, intentar encontrarla desde BattleManager
        GameObject panelToActivate = panelGeneralGym;
        
        if (panelToActivate == null)
        {
            // Buscar BattleManager y obtener el panel desde ahí
            BattleManager battleManager = FindFirstObjectByType<BattleManager>();
            if (battleManager != null)
            {
                // El BattleManager debería tener la referencia, intentar obtenerla
                // Buscar el panel por el padre del BattleManager
                Transform battleManagerParent = battleManager.transform.parent;
                if (battleManagerParent != null)
                {
                    panelToActivate = battleManagerParent.gameObject;
                }
            }
        }
        
        if (panelToActivate != null)
        {
            // Usar PanelNavigationManager si está disponible para que el sistema de navegación sepa que el panel está abierto
            if (GameDataManager.Instance != null && GameDataManager.Instance.PanelNavigationManager != null)
            {
                GameDataManager.Instance.PanelNavigationManager.OpenPanel(panelToActivate);
            }
            else
            {
                // Fallback: activar directamente si no hay PanelNavigationManager
                panelToActivate.SetActive(true);
            }
        }
    }

    private void OnDestroy()
    {
        // Limpiar listeners
        if (combatButton != null)
        {
            combatButton.onClick.RemoveAllListeners();
        }

        if (closeCombatButton != null)
        {
            closeCombatButton.onClick.RemoveAllListeners();
        }

        if (victoryAcceptButton != null)
        {
            victoryAcceptButton.onClick.RemoveAllListeners();
        }

        if (defeatAcceptButton != null)
        {
            defeatAcceptButton.onClick.RemoveAllListeners();
        }

        if (attackButtons != null)
        {
            foreach (var attackButton in attackButtons)
            {
                if (attackButton.button != null)
                {
                    attackButton.button.onClick.RemoveAllListeners();
                }
            }
        }
    }

    /// <summary>
    /// Actualiza el estado de los botones de ataque según el nivel del héroe y si están desbloqueados.
    /// </summary>
    private void UpdateAttackButtonsAvailability()
    {
        // SOLUCIÓN: El botón de ataque básico siempre está habilitado (no requiere desbloqueo)
        // Asegurar que se habilite al inicio y al final del método para evitar que se deshabilite después
        if (basicAttackButton != null)
        {
            basicAttackButton.interactable = true;
        }

        if (attackButtons == null || GameDataManager.Instance == null)
        {
            // Asegurar que el botón básico esté habilitado incluso si hay errores
            if (basicAttackButton != null)
            {
                basicAttackButton.interactable = true;
            }
            return;
        }

        PlayerProfileData profile = GameDataManager.Instance.GetPlayerProfile();
        if (profile == null)
        {
            // Asegurar que el botón básico esté habilitado incluso si no hay perfil
            if (basicAttackButton != null)
            {
                basicAttackButton.interactable = true;
            }
            return;
        }

        int heroLevel = profile.heroLevel;
        GameDataManager gameDataManager = GameDataManager.Instance;

        foreach (var attackButton in attackButtons)
        {
            if (attackButton.button != null && attackButton.attackData != null)
            {
                // Verificar nivel requerido
                bool hasRequiredLevel = attackButton.attackData.requiredHeroLevel == 0 || 
                                       heroLevel >= attackButton.attackData.requiredHeroLevel;
                
                // Verificar si el ataque está desbloqueado en la biblioteca
                // NOTA: El ataque básico (null) siempre está disponible
                bool isUnlocked = attackButton.attackData == null || 
                                 gameDataManager.IsAttackUnlocked(attackButton.attackData.attackName);
                
                // El botón está habilitado solo si cumple ambos requisitos
                attackButton.button.interactable = hasRequiredLevel && isUnlocked;
            }
        }
        
        // SOLUCIÓN: Asegurar que el botón básico esté habilitado al final del método
        // Esto previene que cualquier lógica adicional lo deshabilite
        if (basicAttackButton != null)
        {
            basicAttackButton.interactable = true;
        }
    }

    /// <summary>
    /// Se llama cuando el héroe sube de nivel durante el combate.
    /// </summary>
    public void OnHeroLevelUp()
    {
        UpdateAttackButtonsAvailability();
    }
}


