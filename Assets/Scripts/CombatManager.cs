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
    
    [Tooltip("Textos configurables del combate")]
    [SerializeField] private CombatTexts combatTexts;
    
    // Estado del combate
    private EnemyData currentEnemy = null;
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
    private bool playerStunned = false;
    
    // Efectos activos del enemigo
    private int enemyAttackBuffPercent = 0;
    private int enemyAttackBuffRounds = 0;
    private int enemyDefenseBuffPercent = 0;
    private int enemyDefenseBuffRounds = 0;
    private int enemyPoisonPercent = 0;
    private bool enemyStunned = false;

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
        
        // Resetear efectos activos
        ResetActiveEffects();
        
        UpdateUI();
        
        // Actualizar disponibilidad de botones de ataque según nivel del héroe
        UpdateAttackButtonsAvailability();
        
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
        playerStunned = false;
        
        // Efectos del enemigo
        enemyAttackBuffPercent = 0;
        enemyAttackBuffRounds = 0;
        enemyDefenseBuffPercent = 0;
        enemyDefenseBuffRounds = 0;
        enemyPoisonPercent = 0;
        enemyStunned = false;
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
    /// Aplica efectos de veneno al inicio de la ronda.
    /// </summary>
    private IEnumerator ApplyPoisonEffects()
    {
        // Veneno del jugador (aplicado al enemigo)
        if (enemyPoisonPercent > 0)
        {
            int poisonDamage = Mathf.RoundToInt(enemyMaxHp * enemyPoisonPercent / 100f);
            enemyCurrentHp = Mathf.Max(0, enemyCurrentHp - poisonDamage);
            UpdateEnemyHP();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.poisonEnemy, currentEnemy.enemyName, poisonDamage);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            yield return new WaitForSeconds(1f);
            
            // Verificar si el enemigo murió
            if (enemyCurrentHp <= 0)
            {
                yield break;
            }
        }
        
        // Veneno del enemigo (aplicado al jugador)
        if (playerPoisonPercent > 0)
        {
            int poisonDamage = Mathf.RoundToInt(playerMaxHp * playerPoisonPercent / 100f);
            playerCurrentHp = Mathf.Max(0, playerCurrentHp - poisonDamage);
            UpdatePlayerHP();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.poisonPlayer, poisonDamage);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            
            yield return new WaitForSeconds(1f);
            
            // Verificar si el jugador murió
            if (playerCurrentHp <= 0)
            {
                yield break;
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
        
        if (selectedAttack == null)
        {
            Debug.LogWarning("CombatManager: El ataque seleccionado es nulo.");
            return;
        }

        // Mostrar detalles del ataque
        if (roundDetailsText != null && combatTexts != null)
        {
            string text = FormatText(combatTexts.attackSelected, selectedAttack.attackName, selectedAttack.description);
            StartCoroutine(DisplayTextWithTypewriter(text));
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

        // Aplicar efectos de veneno al inicio de la ronda
        yield return StartCoroutine(ApplyPoisonEffects());
        
        // Verificar si alguien murió por veneno
        if (enemyCurrentHp <= 0)
        {
            OnPlayerVictory();
            yield break;
        }
        if (playerCurrentHp <= 0)
        {
            OnPlayerDefeat();
            yield break;
        }
        
        // Actualizar duración de buffs
        UpdateBuffs();
        
        // Resetear stun al inicio de la ronda
        playerStunned = false;
        enemyStunned = false;

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
            // Ataque básico
            yield return StartCoroutine(ProcessBasicAttack(isPlayer));
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
    /// Procesa un ataque básico (sin AttackData).
    /// </summary>
    private IEnumerator ProcessBasicAttack(bool isPlayer)
    {
        if (isPlayer)
        {
            int damage = CalculateDamage(GetPlayerEffectiveAttack(), playerDestreza, playerAtaqueCritico, playerDanoCritico, 
                                       GetEnemyEffectiveDefense(), true);
            enemyCurrentHp = Mathf.Max(0, enemyCurrentHp - damage);
            UpdateEnemyHP();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.playerBasicAttack, damage);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            int damage = CalculateDamage(GetEnemyEffectiveAttack(), enemyDestreza, enemyAtaqueCritico, enemyDanoCritico, 
                                       GetPlayerEffectiveDefense(), false);
            playerCurrentHp = Mathf.Max(0, playerCurrentHp - damage);
            UpdatePlayerHP();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.enemyBasicAttack, currentEnemy.enemyName, damage);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Procesa un ataque normal (solo daño).
    /// </summary>
    private IEnumerator ProcessNormalAttack(AttackData attack, bool isPlayer)
    {
        if (isPlayer)
        {
            int damage = CalculateDamage(GetPlayerEffectiveAttack(), playerDestreza, playerAtaqueCritico, playerDanoCritico, 
                                       GetEnemyEffectiveDefense(), true);
            enemyCurrentHp = Mathf.Max(0, enemyCurrentHp - damage);
            UpdateEnemyHP();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.playerAttack, attack.attackName, damage);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            int damage = CalculateDamage(GetEnemyEffectiveAttack(), enemyDestreza, enemyAtaqueCritico, enemyDanoCritico, 
                                       GetPlayerEffectiveDefense(), false);
            playerCurrentHp = Mathf.Max(0, playerCurrentHp - damage);
            UpdatePlayerHP();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.enemyAttack, currentEnemy.enemyName, attack.attackName, damage);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Procesa un ataque de curación.
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
            playerCurrentHp = newHp;
            UpdatePlayerHP();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.playerHeal, attack.attackName, actualHeal);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            enemyCurrentHp = newHp;
            UpdateEnemyHP();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.enemyHeal, currentEnemy.enemyName, attack.attackName, actualHeal);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Procesa un ataque de veneno.
    /// </summary>
    private IEnumerator ProcessPoisonAttack(AttackData attack, bool isPlayer)
    {
        // Primero aplica el daño normal
        yield return StartCoroutine(ProcessNormalAttack(attack, isPlayer));
        
        // Luego aplica el veneno (se aplicará al inicio de la siguiente ronda)
        int poisonPercent = attack.effectValue; // 10, 15, 20
        
        if (isPlayer)
        {
            enemyPoisonPercent = poisonPercent;
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
                string text = FormatText(combatTexts.stunFailed, attack.attackName, target);
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
    /// </summary>
    private IEnumerator ProcessMultipleAttack(AttackData attack, bool isPlayer)
    {
        int numHits = attack.effectValue; // 2, 3, 4
        
        for (int i = 0; i < numHits; i++)
        {
            yield return StartCoroutine(ProcessBasicAttack(isPlayer));
            
            // Verificar si el objetivo murió
            if (isPlayer && enemyCurrentHp <= 0)
                yield break;
            if (!isPlayer && playerCurrentHp <= 0)
                yield break;
        }
        
        if (roundDetailsText != null && combatTexts != null)
        {
            string attacker = isPlayer ? "Jugador" : currentEnemy.enemyName;
            string text = FormatText(combatTexts.multipleAttack, attacker, numHits, attack.attackName);
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
            int baseDamage = CalculateDamage(GetPlayerEffectiveAttack(), playerDestreza, playerAtaqueCritico, playerDanoCritico, 
                                            GetEnemyEffectiveDefense(), true);
            int totalDamage = baseDamage * multiplier;
            enemyCurrentHp = Mathf.Max(0, enemyCurrentHp - totalDamage);
            UpdateEnemyHP();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.strongBlow, "Jugador", attack.attackName, multiplier, totalDamage);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            int baseDamage = CalculateDamage(GetEnemyEffectiveAttack(), enemyDestreza, enemyAtaqueCritico, enemyDanoCritico, 
                                            GetPlayerEffectiveDefense(), false);
            int totalDamage = baseDamage * multiplier;
            playerCurrentHp = Mathf.Max(0, playerCurrentHp - totalDamage);
            UpdatePlayerHP();
            
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.strongBlow, currentEnemy.enemyName, attack.attackName, multiplier, totalDamage);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
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
            playerAttackBuffPercent = buffPercent;
            playerAttackBuffRounds = duration;
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.attackBuff, "Jugador", attack.attackName, buffPercent, duration);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            enemyAttackBuffPercent = buffPercent;
            enemyAttackBuffRounds = duration;
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.attackBuff, currentEnemy.enemyName, attack.attackName, buffPercent, duration);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
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
            playerDefenseBuffPercent = buffPercent;
            playerDefenseBuffRounds = duration;
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.defenseBuff, "Jugador", attack.attackName, buffPercent, duration);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            enemyDefenseBuffPercent = buffPercent;
            enemyDefenseBuffRounds = duration;
            if (roundDetailsText != null && combatTexts != null)
            {
                string text = FormatText(combatTexts.defenseBuff, currentEnemy.enemyName, attack.attackName, buffPercent, duration);
                yield return StartCoroutine(DisplayTextWithDelay(text));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Ejecuta el turno del jugador.
    /// </summary>
    private IEnumerator ExecutePlayerTurn()
    {
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
            yield break;
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

                    // Atacar de nuevo (ataque básico)
                    yield return StartCoroutine(ProcessBasicAttack(true));

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
            yield break;
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

                    // Atacar de nuevo (ataque básico)
                    yield return StartCoroutine(ProcessBasicAttack(false));

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
    /// </summary>
    private void OnPlayerDefeat()
    {
        combatInProgress = false;

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

    /// <summary>
    /// Actualiza el estado de los botones de ataque según el nivel del héroe.
    /// </summary>
    private void UpdateAttackButtonsAvailability()
    {
        if (attackButtons == null || GameDataManager.Instance == null)
            return;

        PlayerProfileData profile = GameDataManager.Instance.GetPlayerProfile();
        if (profile == null)
            return;

        int heroLevel = profile.heroLevel;

        foreach (var attackButton in attackButtons)
        {
            if (attackButton.button != null && attackButton.attackData != null)
            {
                // Deshabilitar si el nivel requerido es mayor al nivel del héroe
                bool isUnlocked = attackButton.attackData.requiredHeroLevel == 0 || 
                                 heroLevel >= attackButton.attackData.requiredHeroLevel;
                
                attackButton.button.interactable = isUnlocked;
            }
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

