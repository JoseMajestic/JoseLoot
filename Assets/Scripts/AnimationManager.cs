using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona las animaciones del jugador y enemigo durante el combate.
/// Maneja los estados: Idle, Attack, Defense, KO.
/// </summary>
public class AnimationManager : MonoBehaviour
{
    [System.Serializable]
    public class AnimationStateConfig
    {
        [Tooltip("Clip de animación para este estado")]
        public AnimationClip clip;
        
        [Tooltip("Controller de animación para este estado")]
        public RuntimeAnimatorController controller;
        
        [Tooltip("Objeto con SpriteRenderer donde se mostrará la animación")]
        public GameObject spriteRendererObject;
    }

    [System.Serializable]
    public class EnemyAnimationSet
    {
        [Tooltip("Configuración de animación Idle")]
        public AnimationStateConfig idle;
        
        [Tooltip("Configuración de animación Attack")]
        public AnimationStateConfig attack;
        
        [Tooltip("Configuración de animación Defense")]
        public AnimationStateConfig defense;
        
        [Tooltip("Configuración de animación KO")]
        public AnimationStateConfig ko;
        
        // NOTA: El sprite del enemigo ahora se toma directamente de EnemyData.enemySprite
        // Ya no es necesario configurarlo aquí
    }

    public enum AnimationState
    {
        Idle,
        Attack,
        Defense,
        KO
    }

    [Header("Animaciones del Jugador")]
    [Tooltip("Configuración de animación Idle del jugador")]
    [SerializeField] private AnimationStateConfig playerIdle;
    
    [Tooltip("Configuración de animación Attack del jugador")]
    [SerializeField] private AnimationStateConfig playerAttack;
    
    [Tooltip("Configuración de animación Defense del jugador")]
    [SerializeField] private AnimationStateConfig playerDefense;
    
    [Tooltip("Configuración de animación KO del jugador")]
    [SerializeField] private AnimationStateConfig playerKO;
    
    [Tooltip("Sprite fijo del jugador (asignado una vez al inicio del combate)")]
    [SerializeField] private Sprite playerSprite;

    [Header("Animaciones de Enemigos")]
    [Tooltip("Array de configuraciones de animación para cada enemigo (alineado con el array de enemigos del BattleManager)")]
    [SerializeField] private EnemyAnimationSet[] enemyAnimationSets;

    [Header("Configuración")]
    [Tooltip("Duración fija de todas las animaciones en segundos")]
    [SerializeField] private float animationDuration = 2f;

    // Estado actual
    private int currentEnemyIndex = -1;
    private EnemyData currentEnemyData = null; // Referencia al EnemyData actual para obtener el sprite
    private Animator playerAnimator;
    private Animator enemyAnimator;
    private SpriteRenderer playerSpriteRenderer;
    private SpriteRenderer enemySpriteRenderer;

    /// <summary>
    /// Se ejecuta al inicializar el componente.
    /// Oculta todas las animaciones por defecto hasta que el panel de combate se abra.
    /// </summary>
    private void Awake()
    {
        // Ocultar todas las animaciones al inicio
        HideAllAnimations();
    }

    /// <summary>
    /// Inicializa las animaciones para un combate específico.
    /// </summary>
    /// <param name="enemyIndex">Índice del enemigo en el array de enemyAnimationSets</param>
    /// <param name="enemyData">Referencia al EnemyData para obtener el sprite del enemigo</param>
    public void InitializeForCombat(int enemyIndex, EnemyData enemyData)
    {
        currentEnemyIndex = enemyIndex;
        currentEnemyData = enemyData;

        // Inicializar jugador
        InitializePlayer();

        // Inicializar enemigo
        InitializeEnemy(enemyIndex);

        // Establecer estado inicial Idle
        SetPlayerIdle();
        SetEnemyIdle();
    }

    /// <summary>
    /// Inicializa los componentes del jugador.
    /// </summary>
    private void InitializePlayer()
    {
        // Ocultar todos los paneles del jugador primero
        HideAllPlayerPanels();

        // Buscar o crear Animator del jugador
        if (playerIdle != null && playerIdle.spriteRendererObject != null)
        {
            playerAnimator = playerIdle.spriteRendererObject.GetComponent<Animator>();
            if (playerAnimator == null)
            {
                playerAnimator = playerIdle.spriteRendererObject.AddComponent<Animator>();
            }

            // Buscar SpriteRenderer
            playerSpriteRenderer = playerIdle.spriteRendererObject.GetComponent<SpriteRenderer>();
            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer = playerIdle.spriteRendererObject.GetComponentInChildren<SpriteRenderer>();
            }

            // Asignar sprite del jugador si está configurado
            if (playerSprite != null && playerSpriteRenderer != null)
            {
                playerSpriteRenderer.sprite = playerSprite;
            }
        }
    }

    /// <summary>
    /// Inicializa los componentes del enemigo.
    /// </summary>
    private void InitializeEnemy(int enemyIndex)
    {
        if (enemyIndex < 0 || enemyIndex >= enemyAnimationSets.Length)
        {
            Debug.LogWarning($"AnimationManager: Índice de enemigo inválido: {enemyIndex}");
            return;
        }

        EnemyAnimationSet enemySet = enemyAnimationSets[enemyIndex];
        if (enemySet == null)
        {
            Debug.LogWarning($"AnimationManager: No hay configuración de animación para el enemigo en el índice {enemyIndex}");
            return;
        }

        // Ocultar todos los paneles del enemigo primero
        HideAllEnemyPanels(enemySet);

        // Buscar o crear Animator del enemigo
        if (enemySet.idle != null && enemySet.idle.spriteRendererObject != null)
        {
            enemyAnimator = enemySet.idle.spriteRendererObject.GetComponent<Animator>();
            if (enemyAnimator == null)
            {
                enemyAnimator = enemySet.idle.spriteRendererObject.AddComponent<Animator>();
            }

            // Buscar SpriteRenderer
            enemySpriteRenderer = enemySet.idle.spriteRendererObject.GetComponent<SpriteRenderer>();
            if (enemySpriteRenderer == null)
            {
                enemySpriteRenderer = enemySet.idle.spriteRendererObject.GetComponentInChildren<SpriteRenderer>();
            }

            // Asignar sprite del enemigo desde EnemyData
            if (currentEnemyData != null && currentEnemyData.enemySprite != null && enemySpriteRenderer != null)
            {
                enemySpriteRenderer.sprite = currentEnemyData.enemySprite;
            }
        }
    }

    /// <summary>
    /// Reproduce una animación del jugador y espera a que termine.
    /// </summary>
    public IEnumerator PlayPlayerAnimation(AnimationState state)
    {
        AnimationStateConfig config = GetPlayerConfig(state);
        if (config == null)
        {
            Debug.LogWarning($"AnimationManager: No hay configuración para el estado {state} del jugador");
            yield return new WaitForSeconds(animationDuration);
            yield break;
        }

        // Aplicar configuración
        ApplyPlayerConfig(config);

        // Esperar duración fija
        yield return new WaitForSeconds(animationDuration);

        // NO volver automáticamente a Idle para animaciones de ataque
        // El CombatManager se encargará de volver a Idle explícitamente después de la animación de ataque
        // Solo volver a Idle automáticamente para animaciones de defensa
        if (state != AnimationState.KO && state != AnimationState.Attack)
        {
            SetPlayerIdle();
        }
    }

    /// <summary>
    /// Reproduce una animación del enemigo y espera a que termine.
    /// </summary>
    public IEnumerator PlayEnemyAnimation(AnimationState state)
    {
        if (currentEnemyIndex < 0 || currentEnemyIndex >= enemyAnimationSets.Length)
        {
            Debug.LogWarning($"AnimationManager: No hay enemigo inicializado o índice inválido: {currentEnemyIndex}");
            yield return new WaitForSeconds(animationDuration);
            yield break;
        }

        EnemyAnimationSet enemySet = enemyAnimationSets[currentEnemyIndex];
        if (enemySet == null)
        {
            Debug.LogWarning($"AnimationManager: No hay configuración de animación para el enemigo en el índice {currentEnemyIndex}");
            yield return new WaitForSeconds(animationDuration);
            yield break;
        }

        AnimationStateConfig config = GetEnemyConfig(enemySet, state);
        if (config == null)
        {
            Debug.LogWarning($"AnimationManager: No hay configuración para el estado {state} del enemigo");
            yield return new WaitForSeconds(animationDuration);
            yield break;
        }

        // Aplicar configuración
        ApplyEnemyConfig(config);

        // Esperar duración fija
        yield return new WaitForSeconds(animationDuration);

        // NO volver automáticamente a Idle para animaciones de ataque
        // El CombatManager se encargará de volver a Idle explícitamente después de la animación de ataque
        // Solo volver a Idle automáticamente para animaciones de defensa
        if (state != AnimationState.KO && state != AnimationState.Attack)
        {
            SetEnemyIdle();
        }
    }

    /// <summary>
    /// Establece el estado Idle del jugador.
    /// </summary>
    public void SetPlayerIdle()
    {
        AnimationStateConfig config = GetPlayerConfig(AnimationState.Idle);
        if (config != null)
        {
            ApplyPlayerConfig(config);
        }
    }

    /// <summary>
    /// Establece el estado Idle del enemigo.
    /// </summary>
    public void SetEnemyIdle()
    {
        if (currentEnemyIndex < 0 || currentEnemyIndex >= enemyAnimationSets.Length)
            return;

        EnemyAnimationSet enemySet = enemyAnimationSets[currentEnemyIndex];
        if (enemySet == null)
            return;

        AnimationStateConfig config = GetEnemyConfig(enemySet, AnimationState.Idle);
        if (config != null)
        {
            ApplyEnemyConfig(config);
        }
    }

    /// <summary>
    /// Obtiene la configuración del jugador para un estado específico.
    /// </summary>
    private AnimationStateConfig GetPlayerConfig(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.Idle:
                return playerIdle;
            case AnimationState.Attack:
                return playerAttack;
            case AnimationState.Defense:
                return playerDefense;
            case AnimationState.KO:
                return playerKO;
            default:
                return null;
        }
    }

    /// <summary>
    /// Obtiene la configuración del enemigo para un estado específico.
    /// </summary>
    private AnimationStateConfig GetEnemyConfig(EnemyAnimationSet enemySet, AnimationState state)
    {
        switch (state)
        {
            case AnimationState.Idle:
                return enemySet.idle;
            case AnimationState.Attack:
                return enemySet.attack;
            case AnimationState.Defense:
                return enemySet.defense;
            case AnimationState.KO:
                return enemySet.ko;
            default:
                return null;
        }
    }

    /// <summary>
    /// Aplica la configuración de animación al jugador.
    /// </summary>
    private void ApplyPlayerConfig(AnimationStateConfig config)
    {
        if (config == null || config.spriteRendererObject == null)
            return;

        // Ocultar todos los paneles del jugador EXCEPTO el actual
        // Esto evita ocultar el panel activo durante la transición
        HideAllPlayerPanelsExcept(config.spriteRendererObject);

        // Mostrar el panel del estado actual
        ShowPanel(config.spriteRendererObject);

        // Asegurar que tenemos el Animator
        if (playerAnimator == null)
        {
            playerAnimator = config.spriteRendererObject.GetComponent<Animator>();
            if (playerAnimator == null)
            {
                playerAnimator = config.spriteRendererObject.AddComponent<Animator>();
            }
        }

        // Asegurar que el Animator esté habilitado
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
        }

        // IMPORTANTE: Asignar sprite ANTES de reproducir la animación
        // para evitar que sobrescriba los cambios de la animación
        if (playerSprite != null)
        {
            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer = config.spriteRendererObject.GetComponent<SpriteRenderer>();
                if (playerSpriteRenderer == null)
                {
                    playerSpriteRenderer = config.spriteRendererObject.GetComponentInChildren<SpriteRenderer>();
                }
            }

            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.sprite = playerSprite;
            }
        }

        // Asignar controller si está configurado
        if (config.controller != null && playerAnimator != null)
        {
            playerAnimator.runtimeAnimatorController = config.controller;
        }

        // Reproducir clip si está configurado
        if (config.clip != null && playerAnimator != null)
        {
            // Si hay controller, intentar usar el nombre del clip como nombre de estado
            // Si no hay controller, el Play() funcionará directamente con el clip
            // Usar el nombre del clip como nombre de estado (debe coincidir con el estado en el controller)
            // Parámetros: (nombre del estado/clip, capa, tiempo normalizado para reiniciar desde el inicio)
            playerAnimator.Play(config.clip.name, 0, 0f);
        }
    }

    /// <summary>
    /// Aplica la configuración de animación al enemigo.
    /// </summary>
    private void ApplyEnemyConfig(AnimationStateConfig config)
    {
        if (config == null || config.spriteRendererObject == null)
            return;

        if (currentEnemyIndex < 0 || currentEnemyIndex >= enemyAnimationSets.Length)
            return;

        EnemyAnimationSet enemySet = enemyAnimationSets[currentEnemyIndex];
        if (enemySet == null)
            return;

        // Ocultar todos los paneles del enemigo EXCEPTO el actual
        // Esto evita ocultar el panel activo durante la transición
        HideAllEnemyPanelsExcept(enemySet, config.spriteRendererObject);

        // Mostrar el panel del estado actual
        ShowPanel(config.spriteRendererObject);

        // Asegurar que tenemos el Animator
        if (enemyAnimator == null)
        {
            enemyAnimator = config.spriteRendererObject.GetComponent<Animator>();
            if (enemyAnimator == null)
            {
                enemyAnimator = config.spriteRendererObject.AddComponent<Animator>();
            }
        }

        // Asegurar que el Animator esté habilitado
        if (enemyAnimator != null)
        {
            enemyAnimator.enabled = true;
        }

        // IMPORTANTE: Asignar sprite ANTES de reproducir la animación
        // para evitar que sobrescriba los cambios de la animación
        // El sprite se toma directamente de EnemyData
        // CRÍTICO: Buscar el SpriteRenderer específico del spriteRendererObject actual
        // NO reutilizar enemySpriteRenderer global, cada estado tiene su propio SpriteRenderer
        if (currentEnemyData != null && currentEnemyData.enemySprite != null)
        {
            SpriteRenderer currentStateSpriteRenderer = config.spriteRendererObject.GetComponent<SpriteRenderer>();
            if (currentStateSpriteRenderer == null)
            {
                currentStateSpriteRenderer = config.spriteRendererObject.GetComponentInChildren<SpriteRenderer>();
            }

            if (currentStateSpriteRenderer != null)
            {
                currentStateSpriteRenderer.sprite = currentEnemyData.enemySprite;
            }
        }

        // Asignar controller si está configurado
        if (config.controller != null && enemyAnimator != null)
        {
            enemyAnimator.runtimeAnimatorController = config.controller;
        }

        // Reproducir clip si está configurado
        if (config.clip != null && enemyAnimator != null)
        {
            // Si hay controller, intentar usar el nombre del clip como nombre de estado
            // Si no hay controller, el Play() funcionará directamente con el clip
            // Usar el nombre del clip como nombre de estado (debe coincidir con el estado en el controller)
            // Parámetros: (nombre del estado/clip, capa, tiempo normalizado para reiniciar desde el inicio)
            enemyAnimator.Play(config.clip.name, 0, 0f);
        }
    }

    /// <summary>
    /// Oculta todos los paneles del jugador (Alpha = 0).
    /// </summary>
    private void HideAllPlayerPanels()
    {
        if (playerIdle != null && playerIdle.spriteRendererObject != null)
            HidePanel(playerIdle.spriteRendererObject);
        
        if (playerAttack != null && playerAttack.spriteRendererObject != null)
            HidePanel(playerAttack.spriteRendererObject);
        
        if (playerDefense != null && playerDefense.spriteRendererObject != null)
            HidePanel(playerDefense.spriteRendererObject);
        
        if (playerKO != null && playerKO.spriteRendererObject != null)
            HidePanel(playerKO.spriteRendererObject);
    }

    /// <summary>
    /// Oculta todos los paneles del jugador EXCEPTO el especificado (Alpha = 0).
    /// Esto evita ocultar el panel activo durante las transiciones.
    /// </summary>
    private void HideAllPlayerPanelsExcept(GameObject exceptPanel)
    {
        if (playerIdle != null && playerIdle.spriteRendererObject != null && playerIdle.spriteRendererObject != exceptPanel)
            HidePanel(playerIdle.spriteRendererObject);
        
        if (playerAttack != null && playerAttack.spriteRendererObject != null && playerAttack.spriteRendererObject != exceptPanel)
            HidePanel(playerAttack.spriteRendererObject);
        
        if (playerDefense != null && playerDefense.spriteRendererObject != null && playerDefense.spriteRendererObject != exceptPanel)
            HidePanel(playerDefense.spriteRendererObject);
        
        if (playerKO != null && playerKO.spriteRendererObject != null && playerKO.spriteRendererObject != exceptPanel)
            HidePanel(playerKO.spriteRendererObject);
    }

    /// <summary>
    /// Oculta todos los paneles del enemigo (Alpha = 0).
    /// </summary>
    private void HideAllEnemyPanels(EnemyAnimationSet enemySet)
    {
        if (enemySet == null)
            return;

        if (enemySet.idle != null && enemySet.idle.spriteRendererObject != null)
            HidePanel(enemySet.idle.spriteRendererObject);
        
        if (enemySet.attack != null && enemySet.attack.spriteRendererObject != null)
            HidePanel(enemySet.attack.spriteRendererObject);
        
        if (enemySet.defense != null && enemySet.defense.spriteRendererObject != null)
            HidePanel(enemySet.defense.spriteRendererObject);
        
        if (enemySet.ko != null && enemySet.ko.spriteRendererObject != null)
            HidePanel(enemySet.ko.spriteRendererObject);
    }

    /// <summary>
    /// Oculta todos los paneles del enemigo EXCEPTO el especificado (Alpha = 0).
    /// Esto evita ocultar el panel activo durante las transiciones.
    /// </summary>
    private void HideAllEnemyPanelsExcept(EnemyAnimationSet enemySet, GameObject exceptPanel)
    {
        if (enemySet == null)
            return;

        if (enemySet.idle != null && enemySet.idle.spriteRendererObject != null && enemySet.idle.spriteRendererObject != exceptPanel)
            HidePanel(enemySet.idle.spriteRendererObject);
        
        if (enemySet.attack != null && enemySet.attack.spriteRendererObject != null && enemySet.attack.spriteRendererObject != exceptPanel)
            HidePanel(enemySet.attack.spriteRendererObject);
        
        if (enemySet.defense != null && enemySet.defense.spriteRendererObject != null && enemySet.defense.spriteRendererObject != exceptPanel)
            HidePanel(enemySet.defense.spriteRendererObject);
        
        if (enemySet.ko != null && enemySet.ko.spriteRendererObject != null && enemySet.ko.spriteRendererObject != exceptPanel)
            HidePanel(enemySet.ko.spriteRendererObject);
    }

    /// <summary>
    /// Oculta todas las animaciones (jugador y enemigos).
    /// Se llama cuando se cierra el panel de combate.
    /// </summary>
    public void HideAllAnimations()
    {
        // Ocultar todas las animaciones del jugador
        HideAllPlayerPanels();

        // Ocultar todas las animaciones de todos los enemigos
        if (enemyAnimationSets != null)
        {
            foreach (EnemyAnimationSet enemySet in enemyAnimationSets)
            {
                if (enemySet != null)
                {
                    HideAllEnemyPanels(enemySet);
                }
            }
        }
    }

    /// <summary>
    /// Muestra un panel (Alpha = 255/1).
    /// Funciona tanto con SpriteRenderer como con Image (UI).
    /// Asegura que el GameObject esté activo para que las animaciones se reproduzcan.
    /// </summary>
    private void ShowPanel(GameObject panelObject)
    {
        if (panelObject == null)
            return;

        // Asegurar que el GameObject esté activo
        if (!panelObject.activeSelf)
        {
            panelObject.SetActive(true);
        }

        SetPanelAlpha(panelObject, 1f);
    }

    /// <summary>
    /// Oculta un panel (Alpha = 0).
    /// Funciona tanto con SpriteRenderer como con Image (UI).
    /// </summary>
    private void HidePanel(GameObject panelObject)
    {
        if (panelObject == null)
            return;

        SetPanelAlpha(panelObject, 0f);
    }

    /// <summary>
    /// Establece el Alpha de un panel (0 = invisible, 1 = visible).
    /// Funciona tanto con SpriteRenderer como con Image (UI).
    /// </summary>
    private void SetPanelAlpha(GameObject panelObject, float alpha)
    {
        if (panelObject == null)
            return;

        // Intentar con SpriteRenderer primero
        SpriteRenderer spriteRenderer = panelObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = panelObject.GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
            return;
        }

        // Si no hay SpriteRenderer, intentar con Image (UI)
        Image image = panelObject.GetComponent<Image>();
        if (image == null)
        {
            image = panelObject.GetComponentInChildren<Image>();
        }

        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
            return;
        }

        // Si no se encuentra ningún componente, buscar en todos los hijos
        SpriteRenderer[] spriteRenderers = panelObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
        }

        Image[] images = panelObject.GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            Color color = img.color;
            color.a = alpha;
            img.color = color;
        }
    }
}

