using System.Collections;
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
    [Tooltip("Texto con detalles de la ronda")]
    [SerializeField] private TextMeshProUGUI roundDetailsText;
    
    [Header("Panel de Combate")]
    [Tooltip("Panel principal de combate (se cierra al aceptar victoria/derrota)")]
    [SerializeField] private GameObject combatPanel;
    
    [Header("Ataques")]
    [Tooltip("Array de botones de habilidades/ataques (cada posición contiene el botón y la instancia AttackData asociada)")]
    [SerializeField] private AttackButtonData[] attackButtons;
    
    [Tooltip("Botón para ejecutar el ataque")]
    [SerializeField] private Button combatButton;
    
    [Tooltip("Botón que cierra el panel de combate")]
    [SerializeField] private Button closeCombatButton;
    
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
    
    // Estado del combate
    private EnemyData currentEnemy = null;
    private AttackData selectedAttack = null;
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

    private void Start()
    {
        InitializeButtons();
        InitializePanels();
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
    /// Inicia el combate con el enemigo especificado.
    /// </summary>
    public void StartCombat(EnemyData enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("CombatManager: No se puede iniciar combate sin enemigo.");
            return;
        }

        currentEnemy = enemy;
        rewardCoins = enemy.rewardCoins;
        
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
            Image enemyImage = enemyImagePanel.GetComponentInChildren<Image>();
            if (enemyImage != null && enemy.enemySprite != null)
            {
                enemyImage.sprite = enemy.enemySprite;
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
        
        UpdateUI();
        
        // Deshabilitar botón de combate hasta que se seleccione un ataque
        if (combatButton != null)
        {
            combatButton.interactable = false;
        }
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
        
        if (selectedAttack == null)
        {
            Debug.LogWarning("CombatManager: El ataque seleccionado es nulo.");
            return;
        }

        // Mostrar detalles del ataque
        if (roundDetailsText != null)
        {
            roundDetailsText.text = $"Ataque seleccionado: {selectedAttack.attackName}\n{selectedAttack.description}";
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
        if (selectedAttack == null)
        {
            Debug.LogWarning("CombatManager: No hay ataque seleccionado.");
            return;
        }

        if (!combatInProgress)
        {
            Debug.LogWarning("CombatManager: El combate no está en progreso.");
            return;
        }

        // Iniciar nueva ronda
        currentRound++;
        playerLuckUsedThisRound = false;
        enemyLuckUsedThisRound = false;

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

        // Determinar orden de ataque
        bool playerAttacksFirst = DetermineAttackOrder();

        if (playerAttacksFirst)
        {
            // Turno del jugador
            yield return StartCoroutine(ExecutePlayerTurn());
            
            // Verificar si el enemigo murió
            if (enemyCurrentHp <= 0)
            {
                OnPlayerVictory();
                yield break;
            }

            // Turno del enemigo
            yield return StartCoroutine(ExecuteEnemyTurn());
            
            // Verificar si el jugador murió
            if (playerCurrentHp <= 0)
            {
                OnPlayerDefeat();
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
                OnPlayerDefeat();
                yield break;
            }

            // Turno del jugador
            yield return StartCoroutine(ExecutePlayerTurn());
            
            // Verificar si el enemigo murió
            if (enemyCurrentHp <= 0)
            {
                OnPlayerVictory();
                yield break;
            }
        }

        // Si ambos siguen vivos, habilitar botón para siguiente ronda
        if (combatButton != null)
        {
            combatButton.interactable = true;
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
    /// Ejecuta el turno del jugador.
    /// </summary>
    private IEnumerator ExecutePlayerTurn()
    {
        // Calcular daño
        int damage = CalculateDamage(playerAtaque, playerDestreza, playerAtaqueCritico, playerDanoCritico, 
                                     enemyDefensa, true);
        
        // Aplicar daño al enemigo
        enemyCurrentHp = Mathf.Max(0, enemyCurrentHp - damage);
        
        // Actualizar UI
        UpdateEnemyHP();
        
        // Mostrar detalles en el texto
        if (roundDetailsText != null)
        {
            roundDetailsText.text = $"Jugador ataca con {selectedAttack.attackName} y causa {damage} de daño.";
        }

        yield return new WaitForSeconds(1f);

        // Verificar si el enemigo murió
        if (enemyCurrentHp <= 0)
        {
            yield break;
        }

        // Calcular suerte (solo una vez por ronda)
        if (!playerLuckUsedThisRound)
        {
            bool luckActivated = CalculateLuck(playerSuerte);
            if (luckActivated)
            {
                playerLuckUsedThisRound = true;
                
                if (roundDetailsText != null)
                {
                    roundDetailsText.text = "¡Suerte activada! El jugador ataca de nuevo.";
                }

                yield return new WaitForSeconds(1f);

                // Atacar de nuevo
                int luckDamage = CalculateDamage(playerAtaque, playerDestreza, playerAtaqueCritico, playerDanoCritico, 
                                                 enemyDefensa, true);
                enemyCurrentHp = Mathf.Max(0, enemyCurrentHp - luckDamage);
                UpdateEnemyHP();

                if (roundDetailsText != null)
                {
                    roundDetailsText.text = $"Jugador ataca de nuevo y causa {luckDamage} de daño.";
                }

                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Ejecuta el turno del enemigo.
    /// </summary>
    private IEnumerator ExecuteEnemyTurn()
    {
        // El enemigo usa ataque básico (por ahora)
        int damage = CalculateDamage(enemyAtaque, enemyDestreza, enemyAtaqueCritico, enemyDanoCritico, 
                                     playerDefensa, false);
        
        // Aplicar daño al jugador
        playerCurrentHp = Mathf.Max(0, playerCurrentHp - damage);
        
        // Actualizar UI
        UpdatePlayerHP();
        
        // Mostrar detalles en el texto
        if (roundDetailsText != null)
        {
            roundDetailsText.text = $"{currentEnemy.enemyName} ataca y causa {damage} de daño.";
        }

        yield return new WaitForSeconds(1f);

        // Verificar si el jugador murió
        if (playerCurrentHp <= 0)
        {
            yield break;
        }

        // Calcular suerte del enemigo (solo una vez por ronda)
        if (!enemyLuckUsedThisRound)
        {
            bool luckActivated = CalculateLuck(enemySuerte);
            if (luckActivated)
            {
                enemyLuckUsedThisRound = true;
                
                if (roundDetailsText != null)
                {
                    roundDetailsText.text = $"¡Suerte activada! {currentEnemy.enemyName} ataca de nuevo.";
                }

                yield return new WaitForSeconds(1f);

                // Atacar de nuevo
                int luckDamage = CalculateDamage(enemyAtaque, enemyDestreza, enemyAtaqueCritico, enemyDanoCritico, 
                                                 playerDefensa, false);
                playerCurrentHp = Mathf.Max(0, playerCurrentHp - luckDamage);
                UpdatePlayerHP();

                if (roundDetailsText != null)
                {
                    roundDetailsText.text = $"{currentEnemy.enemyName} ataca de nuevo y causa {luckDamage} de daño.";
                }

                yield return new WaitForSeconds(1f);
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
    /// Actualiza la UI del HP del enemigo.
    /// </summary>
    private void UpdateEnemyHP()
    {
        if (enemyHpSlider != null)
        {
            enemyHpSlider.maxValue = enemyMaxHp;
            enemyHpSlider.value = enemyCurrentHp;
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
    /// Se llama cuando el jugador gana.
    /// </summary>
    private void OnPlayerVictory()
    {
        combatInProgress = false;

        // Agregar monedas
        if (playerMoney != null)
        {
            playerMoney.AddMoney(rewardCoins);
        }

        // Desbloquear siguiente nivel
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
    /// </summary>
    private void OnPlayerDefeat()
    {
        combatInProgress = false;

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
    /// Cierra el panel de combate.
    /// </summary>
    private void CloseCombatPanel()
    {
        if (combatPanel != null)
        {
            combatPanel.SetActive(false);
        }
        else
        {
            // Fallback: desactivar el GameObject del CombatManager si no hay panel asignado
            gameObject.SetActive(false);
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
}

