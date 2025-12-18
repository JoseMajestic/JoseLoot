using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona las animaciones del jugador y enemigo durante el combate.
/// Maneja los estados: Idle, Attack, Defense, Damage, KO.
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
        
        [Tooltip("Configuración de animación Damage (para visualizar daño recibido)")]
        public AnimationStateConfig damage;
        
        [Tooltip("Configuración de animación Effects (para efectos visuales)")]
        public AnimationStateConfig effects;
        
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
        Damage,
        Effects,
        KO
    }

    [Header("Animaciones del Jugador")]
    [Tooltip("Configuración de animación Idle del jugador")]
    [SerializeField] private AnimationStateConfig playerIdle;
    
    [Tooltip("Configuración de animación Attack del jugador")]
    [SerializeField] private AnimationStateConfig playerAttack;
    
    [Tooltip("Configuración de animación Defense del jugador")]
    [SerializeField] private AnimationStateConfig playerDefense;
    
    [Tooltip("Configuración de animación Damage del jugador (para visualizar daño recibido)")]
    [SerializeField] private AnimationStateConfig playerDamage;
    
    [Tooltip("Configuración de animación Effects del jugador (para efectos visuales)")]
    [SerializeField] private AnimationStateConfig playerEffects;
    
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
    /// CRÍTICO: Deshabilita Animators problemáticos ANTES de ocultar paneles para evitar reproducción automática.
    /// </summary>
    private void Awake()
    {
        // PASO 1: Deshabilitar TODOS los Animators problemáticos PRIMERO
        // Esto previene que se reproduzcan automáticamente y muestren los paneles
        DisableAllProblematicAnimators();
        
        // PASO 2: Ocultar todas las animaciones (Alpha = 0)
        // Esto sobrescribe cualquier valor del Inspector
        HideAllAnimations();
    }

    /// <summary>
    /// Deshabilita TODOS los Animators de Defense, Damage, Effects y KO (jugador y todos los enemigos).
    /// Se usa en Awake() para prevenir reproducción automática al inicio del juego.
    /// </summary>
    private void DisableAllProblematicAnimators()
    {
        // Jugador: Defense, Damage, Effects, KO
        if (playerDefense != null && playerDefense.spriteRendererObject != null)
        {
            Animator animator = playerDefense.spriteRendererObject.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }
        if (playerDamage != null && playerDamage.spriteRendererObject != null)
        {
            Animator animator = playerDamage.spriteRendererObject.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }
        if (playerEffects != null && playerEffects.spriteRendererObject != null)
        {
            Animator animator = playerEffects.spriteRendererObject.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }
        if (playerKO != null && playerKO.spriteRendererObject != null)
        {
            Animator animator = playerKO.spriteRendererObject.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }

        // Enemigos: Defense, Damage, Effects, KO (para TODOS los enemigos, no solo el actual)
        if (enemyAnimationSets != null)
        {
            foreach (EnemyAnimationSet enemySet in enemyAnimationSets)
            {
                if (enemySet != null)
                {
                    if (enemySet.defense != null && enemySet.defense.spriteRendererObject != null)
                    {
                        Animator animator = enemySet.defense.spriteRendererObject.GetComponent<Animator>();
                        if (animator != null) animator.enabled = false;
                    }
                    if (enemySet.damage != null && enemySet.damage.spriteRendererObject != null)
                    {
                        Animator animator = enemySet.damage.spriteRendererObject.GetComponent<Animator>();
                        if (animator != null) animator.enabled = false;
                    }
                    if (enemySet.effects != null && enemySet.effects.spriteRendererObject != null)
                    {
                        Animator animator = enemySet.effects.spriteRendererObject.GetComponent<Animator>();
                        if (animator != null) animator.enabled = false;
                    }
                    if (enemySet.ko != null && enemySet.ko.spriteRendererObject != null)
                    {
                        Animator animator = enemySet.ko.spriteRendererObject.GetComponent<Animator>();
                        if (animator != null) animator.enabled = false;
                    }
                }
            }
        }
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

        // PASO 1: Ocultar TODOS los paneles primero (Alpha = 0)
        HideAllPlayerPanels();
        if (enemyIndex >= 0 && enemyIndex < enemyAnimationSets.Length)
        {
            EnemyAnimationSet enemySet = enemyAnimationSets[enemyIndex];
            if (enemySet != null)
            {
                HideAllEnemyPanels(enemySet);
            }
        }

        // PASO 2: Deshabilitar Animators de Defense, Damage y KO ANTES de cualquier configuración
        // Esto previene que se reproduzcan automáticamente
        DisableProblematicAnimators();

        // PASO 3: Inicializar componentes (sprite, animators básicos)
        InitializePlayer();
        InitializeEnemy(enemyIndex);

        // PASO 4: Mostrar solo Idle (Alpha = 1)
        SetPlayerIdle();
        SetEnemyIdle();
    }

    /// <summary>
    /// Deshabilita los Animators de Defense, Damage, Effects y KO para evitar reproducción automática.
    /// </summary>
    private void DisableProblematicAnimators()
    {
        // Jugador: Defense, Damage, Effects, KO
        if (playerDefense != null && playerDefense.spriteRendererObject != null)
        {
            Animator animator = playerDefense.spriteRendererObject.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }
        if (playerDamage != null && playerDamage.spriteRendererObject != null)
        {
            Animator animator = playerDamage.spriteRendererObject.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }
        if (playerEffects != null && playerEffects.spriteRendererObject != null)
        {
            Animator animator = playerEffects.spriteRendererObject.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }
        if (playerKO != null && playerKO.spriteRendererObject != null)
        {
            Animator animator = playerKO.spriteRendererObject.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
        }

        // Enemigo: Defense, Damage, Effects, KO
        if (currentEnemyIndex >= 0 && currentEnemyIndex < enemyAnimationSets.Length)
        {
            EnemyAnimationSet enemySet = enemyAnimationSets[currentEnemyIndex];
            if (enemySet != null)
            {
                if (enemySet.defense != null && enemySet.defense.spriteRendererObject != null)
                {
                    Animator animator = enemySet.defense.spriteRendererObject.GetComponent<Animator>();
                    if (animator != null) animator.enabled = false;
                }
                if (enemySet.damage != null && enemySet.damage.spriteRendererObject != null)
                {
                    Animator animator = enemySet.damage.spriteRendererObject.GetComponent<Animator>();
                    if (animator != null) animator.enabled = false;
                }
                if (enemySet.effects != null && enemySet.effects.spriteRendererObject != null)
                {
                    Animator animator = enemySet.effects.spriteRendererObject.GetComponent<Animator>();
                    if (animator != null) animator.enabled = false;
                }
                if (enemySet.ko != null && enemySet.ko.spriteRendererObject != null)
                {
                    Animator animator = enemySet.ko.spriteRendererObject.GetComponent<Animator>();
                    if (animator != null) animator.enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// Inicializa los componentes del jugador (sprite, animators básicos).
    /// NOTA: Los paneles ya están ocultos y los Animators problemáticos ya están deshabilitados.
    /// </summary>
    private void InitializePlayer()
    {
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
    /// Inicializa los componentes del enemigo (sprite, animators básicos).
    /// NOTA: Los paneles ya están ocultos y los Animators problemáticos ya están deshabilitados.
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
    /// Verifica que la animación haya terminado realmente antes de continuar.
    /// </summary>
    /// <param name="state">Estado de animación a reproducir</param>
    /// <param name="overrideSprite">Sprite opcional para sustituir el sprite por defecto (usado para Damage y Effects desde AttackData)</param>
    public IEnumerator PlayPlayerAnimation(AnimationState state, Sprite overrideSprite = null)
    {
        AnimationStateConfig config = GetPlayerConfig(state);
        if (config == null)
        {
            Debug.LogWarning($"AnimationManager: No hay configuración para el estado {state} del jugador");
            yield return new WaitForSeconds(animationDuration);
            yield break;
        }

        // 1. Ocultar Idle antes de mostrar la animación
        if (playerIdle != null && playerIdle.spriteRendererObject != null)
        {
            SetPanelAlpha(playerIdle.spriteRendererObject, 0f);
        }
        
        // 2. Aplicar configuración (esto inicia la animación y la muestra)
        yield return StartCoroutine(ApplyPlayerConfig(config, overrideSprite));

        // 3. Calcular duración a esperar (usando duración real del clip con límites)
        float durationToWait = GetAnimationDuration(config);
        
        // 4. Esperar la duración completa de la animación
        yield return new WaitForSeconds(durationToWait);
        
        // 5. Ocultar la animación actual y volver a Idle (excepto KO que se queda visible)
        if (state != AnimationState.KO)
        {
            // Ocultar la animación actual
            SetPanelAlpha(config.spriteRendererObject, 0f);
            
            // Si era Defense, deshabilitar el Animator para evitar reproducción automática
            if (state == AnimationState.Defense && playerDefense != null && playerDefense.spriteRendererObject != null)
            {
                Animator defenseAnimator = playerDefense.spriteRendererObject.GetComponent<Animator>();
                if (defenseAnimator != null)
                {
                    defenseAnimator.enabled = false;
                }
            }
            
            // Volver a mostrar Idle
            SetPlayerIdle();
        }
        // Para KO: el panel se queda visible, no se oculta ni se vuelve a Idle
    }

    /// <summary>
    /// Reproduce una animación del enemigo y espera a que termine.
    /// Verifica que la animación haya terminado realmente antes de continuar.
    /// </summary>
    /// <param name="state">Estado de animación a reproducir</param>
    /// <param name="overrideSprite">Sprite opcional para sustituir el sprite por defecto (usado para Damage y Effects desde AttackData)</param>
    public IEnumerator PlayEnemyAnimation(AnimationState state, Sprite overrideSprite = null)
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

        // 1. Ocultar Idle antes de mostrar la animación
        if (enemySet.idle != null && enemySet.idle.spriteRendererObject != null)
        {
            SetPanelAlpha(enemySet.idle.spriteRendererObject, 0f);
        }
        
        // 2. Aplicar configuración (esto inicia la animación y la muestra)
        yield return StartCoroutine(ApplyEnemyConfig(config, overrideSprite));

        // 3. Calcular duración a esperar (usando duración real del clip con límites)
        float durationToWait = GetAnimationDuration(config);
        
        // 4. Esperar la duración completa de la animación
        yield return new WaitForSeconds(durationToWait);
        
        // 5. Ocultar la animación actual y volver a Idle (excepto KO que se queda visible)
        if (state != AnimationState.KO)
        {
            // Ocultar la animación actual
            SetPanelAlpha(config.spriteRendererObject, 0f);
            
            // Si era Defense, deshabilitar el Animator para evitar reproducción automática
            if (state == AnimationState.Defense && enemySet.defense != null && enemySet.defense.spriteRendererObject != null)
            {
                Animator defenseAnimator = enemySet.defense.spriteRendererObject.GetComponent<Animator>();
                if (defenseAnimator != null)
                {
                    defenseAnimator.enabled = false;
                }
            }
            
            // Volver a mostrar Idle
            SetEnemyIdle();
        }
        // Para KO: el panel se queda visible, no se oculta ni se vuelve a Idle
    }

    /// <summary>
    /// Establece el estado Idle del jugador.
    /// Muestra Idle (Alpha = 1) y oculta todas las demás animaciones (Alpha = 0).
    /// NO reproduce la animación, solo muestra el panel.
    /// </summary>
    public void SetPlayerIdle()
    {
        AnimationStateConfig config = GetPlayerConfig(AnimationState.Idle);
        if (config == null || config.spriteRendererObject == null)
            return;
        
        // Ocultar todas las demás animaciones
        HideAllPlayerPanelsExcept(config.spriteRendererObject);
        
        // Mostrar Idle
        SetPanelAlpha(config.spriteRendererObject, 1f);
        
        // Configurar el Animator para Idle SIN reproducir la animación
        ShowPlayerPanelWithoutPlaying(config);
    }

    /// <summary>
    /// Establece el estado Idle del enemigo.
    /// Muestra Idle (Alpha = 1) y oculta todas las demás animaciones (Alpha = 0).
    /// NO reproduce la animación, solo muestra el panel.
    /// </summary>
    public void SetEnemyIdle()
    {
        if (currentEnemyIndex < 0 || currentEnemyIndex >= enemyAnimationSets.Length)
            return;

        EnemyAnimationSet enemySet = enemyAnimationSets[currentEnemyIndex];
        if (enemySet == null)
            return;

        AnimationStateConfig config = GetEnemyConfig(enemySet, AnimationState.Idle);
        if (config == null || config.spriteRendererObject == null)
            return;
        
        // Ocultar todas las demás animaciones
        HideAllEnemyPanelsExcept(enemySet, config.spriteRendererObject);
        
        // Mostrar Idle
        SetPanelAlpha(config.spriteRendererObject, 1f);
        
        // Configurar el Animator para Idle SIN reproducir la animación
        ShowEnemyPanelWithoutPlaying(config);
    }

    /// <summary>
    /// Calcula la duración de una animación basándose en el clip real con límites razonables.
    /// </summary>
    /// <param name="config">Configuración de la animación</param>
    /// <returns>Duración en segundos (limitada entre 0.5s y 3s, o el valor fijo si no hay clip)</returns>
    private float GetAnimationDuration(AnimationStateConfig config)
    {
        if (config == null || config.clip == null)
        {
            return animationDuration; // Fallback al valor fijo si no hay clip configurado
        }
        
        float clipLength = config.clip.length;
        
        // Si el clip es muy corto (menos de 0.5s), usar mínimo 0.5s para feedback visual adecuado
        if (clipLength < 0.5f)
        {
            return 0.5f;
        }
        
        // Si el clip es muy largo (más de 3s), limitar a 3s para mantener ritmo del combate
        if (clipLength > 3f)
        {
            return 3f;
        }
        
        // Usar duración real del clip
        return clipLength;
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
            case AnimationState.Damage:
                return playerDamage;
            case AnimationState.Effects:
                return playerEffects;
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
            case AnimationState.Damage:
                return enemySet.damage;
            case AnimationState.Effects:
                return enemySet.effects;
            case AnimationState.KO:
                return enemySet.ko;
            default:
                return null;
        }
    }

    /// <summary>
    /// Aplica la configuración de animación al jugador.
    /// </summary>
    /// <param name="config">Configuración de animación</param>
    /// <param name="overrideSprite">Sprite opcional para sustituir el sprite por defecto (usado para Damage y Effects desde AttackData)</param>
    private IEnumerator ApplyPlayerConfig(AnimationStateConfig config, Sprite overrideSprite = null)
    {
        if (config == null || config.spriteRendererObject == null)
            yield break;

        // CRÍTICO: Para Damage y Effects, asegurar que el Animator esté habilitado ANTES de cualquier otra operación
        // Esto es necesario porque Damage y Effects pueden haber sido deshabilitados en la inicialización
        if (config == playerDamage && playerDamage != null && playerDamage.spriteRendererObject != null)
        {
            Animator damageAnimator = playerDamage.spriteRendererObject.GetComponent<Animator>();
            if (damageAnimator != null)
            {
                damageAnimator.enabled = true;
            }
        }
        if (config == playerEffects && playerEffects != null && playerEffects.spriteRendererObject != null)
        {
            Animator effectsAnimator = playerEffects.spriteRendererObject.GetComponent<Animator>();
            if (effectsAnimator != null)
            {
                effectsAnimator.enabled = true;
            }
        }

        // Ocultar todos los paneles del jugador EXCEPTO el actual
        HideAllPlayerPanelsExcept(config.spriteRendererObject);

        // Mostrar el panel del estado actual (Alpha = 1)
        if (!config.spriteRendererObject.activeSelf)
        {
            config.spriteRendererObject.SetActive(true);
        }
        SetPanelAlpha(config.spriteRendererObject, 1f);

        // CRÍTICO: Obtener el Animator del GameObject del estado ACTUAL, no reutilizar uno global
        // Cada estado puede tener su propio Animator en su propio GameObject
        Animator currentAnimator = config.spriteRendererObject.GetComponent<Animator>();
        if (currentAnimator == null)
        {
            currentAnimator = config.spriteRendererObject.AddComponent<Animator>();
        }

        // CRÍTICO: Asegurar que el Animator esté habilitado (especialmente importante para Damage y Effects)
        // Damage y Effects pueden haber sido deshabilitados en la inicialización, así que forzar activación
        if (currentAnimator != null)
        {
            currentAnimator.enabled = true;
            // Forzar actualización inmediata para asegurar que el cambio se aplique
            if (!currentAnimator.enabled)
            {
                Debug.LogWarning($"AnimationManager: No se pudo habilitar el Animator para {config.spriteRendererObject.name}");
            }
        }

        // IMPORTANTE: Asignar sprite ANTES de reproducir la animación
        // para evitar que sobrescriba los cambios de la animación
        // Si hay overrideSprite (desde AttackData), usarlo; si no, usar playerSprite por defecto
        Sprite spriteToUse = overrideSprite != null ? overrideSprite : playerSprite;
        if (spriteToUse != null)
        {
            SpriteRenderer currentSpriteRenderer = config.spriteRendererObject.GetComponent<SpriteRenderer>();
            if (currentSpriteRenderer == null)
            {
                currentSpriteRenderer = config.spriteRendererObject.GetComponentInChildren<SpriteRenderer>();
            }

            if (currentSpriteRenderer != null)
            {
                currentSpriteRenderer.sprite = spriteToUse;
            }
        }

        // NOTA: El controller se asigna ahora justo antes de Play() para asegurar que esté configurado correctamente

        // Reproducir clip si está configurado
        // CRÍTICO: Siempre forzar la reproducción desde el inicio (0f) para asegurar que se reproduce
        // No verificar si ya está en el estado porque puede causar que no se reproduzca
        if (config.clip != null && currentAnimator != null)
        {
            // Asegurar que el Animator esté habilitado y listo
            if (!currentAnimator.enabled)
            {
                currentAnimator.enabled = true;
            }
            
            // Asignar controller si está configurado Y es diferente al actual
            // Esto debe hacerse ANTES de Play() para asegurar que el Animator tenga el controller correcto
            if (config.controller != null)
            {
                if (currentAnimator.runtimeAnimatorController != config.controller)
                {
                    // CRÍTICO: Deshabilitar el Animator antes de cambiar el controller para evitar reproducción automática
                    currentAnimator.enabled = false;
                    currentAnimator.runtimeAnimatorController = config.controller;
                    // CRÍTICO: Reactivar el Animator inmediatamente después de asignar el controller
                    // Esto es especialmente importante para Damage y Effects
                    currentAnimator.enabled = true;
                }
            }
            
            // CRÍTICO: Esperar un frame antes de Play() para asegurar que el controller esté completamente cargado
            yield return null;
            
            // CRÍTICO: Verificar nuevamente que el Animator esté habilitado justo antes de Play()
            // Esto es especialmente importante para Damage y Effects que pueden haber sido deshabilitados en la inicialización
            // Forzar activación explícita para Damage y Effects
            if (!currentAnimator.enabled)
            {
                currentAnimator.enabled = true;
            }
            
            // CRÍTICO ADICIONAL: Verificar una vez más que el Animator esté habilitado
            // Esto asegura que incluso si algo lo desactivó, se reactive antes de Play()
            // Especialmente importante para Damage y Effects
            if (currentAnimator != null && !currentAnimator.enabled)
            {
                currentAnimator.enabled = true;
            }
            
            // Forzar la reproducción desde el inicio (0f) para asegurar que se reproduce completamente
            // Parámetros: (nombre del estado/clip, capa, tiempo normalizado para reiniciar desde el inicio)
            currentAnimator.Play(config.clip.name, 0, 0f);
            
            // Forzar actualización inmediata del Animator para asegurar que la animación comience
            currentAnimator.Update(0f);
        }
    }

    /// <summary>
    /// Muestra el panel del jugador y configura el Animator SIN reproducir la animación.
    /// Usado para Idle cuando solo queremos mostrar el panel sin reproducir.
    /// </summary>
    private void ShowPlayerPanelWithoutPlaying(AnimationStateConfig config)
    {
        if (config == null || config.spriteRendererObject == null)
            return;

        // Obtener el Animator del GameObject del estado actual
        Animator currentAnimator = config.spriteRendererObject.GetComponent<Animator>();
        if (currentAnimator == null)
        {
            currentAnimator = config.spriteRendererObject.AddComponent<Animator>();
        }

        // Asegurar que el Animator esté habilitado
        if (currentAnimator != null)
        {
            currentAnimator.enabled = true;
        }

        // Asignar sprite si está configurado
        if (playerSprite != null)
        {
            SpriteRenderer currentSpriteRenderer = config.spriteRendererObject.GetComponent<SpriteRenderer>();
            if (currentSpriteRenderer == null)
            {
                currentSpriteRenderer = config.spriteRendererObject.GetComponentInChildren<SpriteRenderer>();
            }

            if (currentSpriteRenderer != null)
            {
                currentSpriteRenderer.sprite = playerSprite;
            }
        }

        // Asignar controller si está configurado (pero NO reproducir la animación)
        if (config.controller != null && currentAnimator != null)
        {
            if (currentAnimator.runtimeAnimatorController != config.controller)
            {
                currentAnimator.runtimeAnimatorController = config.controller;
            }
        }

        // NO reproducir el clip aquí - solo configurar el Animator
        // La animación de Idle debe estar en loop en el Animator Controller
    }

    /// <summary>
    /// Aplica la configuración de animación al enemigo.
    /// </summary>
    /// <param name="config">Configuración de animación</param>
    /// <param name="overrideSprite">Sprite opcional para sustituir el sprite por defecto (usado para Damage y Effects desde AttackData)</param>
    private IEnumerator ApplyEnemyConfig(AnimationStateConfig config, Sprite overrideSprite = null)
    {
        if (config == null || config.spriteRendererObject == null)
            yield break;

        if (currentEnemyIndex < 0 || currentEnemyIndex >= enemyAnimationSets.Length)
            yield break;

        EnemyAnimationSet enemySet = enemyAnimationSets[currentEnemyIndex];
        if (enemySet == null)
            yield break;

        // CRÍTICO: Para Damage y Effects, asegurar que el Animator esté habilitado ANTES de cualquier otra operación
        // Esto es necesario porque Damage y Effects pueden haber sido deshabilitados en la inicialización
        if (config == enemySet.damage && enemySet.damage != null && enemySet.damage.spriteRendererObject != null)
        {
            Animator damageAnimator = enemySet.damage.spriteRendererObject.GetComponent<Animator>();
            if (damageAnimator != null)
            {
                damageAnimator.enabled = true;
            }
        }
        if (config == enemySet.effects && enemySet.effects != null && enemySet.effects.spriteRendererObject != null)
        {
            Animator effectsAnimator = enemySet.effects.spriteRendererObject.GetComponent<Animator>();
            if (effectsAnimator != null)
            {
                effectsAnimator.enabled = true;
            }
        }

        // Ocultar todos los paneles del enemigo EXCEPTO el actual
        HideAllEnemyPanelsExcept(enemySet, config.spriteRendererObject);

        // Mostrar el panel del estado actual (Alpha = 1)
        if (!config.spriteRendererObject.activeSelf)
        {
            config.spriteRendererObject.SetActive(true);
        }
        SetPanelAlpha(config.spriteRendererObject, 1f);

        // CRÍTICO: Obtener el Animator del GameObject del estado ACTUAL, no reutilizar uno global
        // Cada estado puede tener su propio Animator en su propio GameObject
        Animator currentEnemyAnimator = config.spriteRendererObject.GetComponent<Animator>();
        if (currentEnemyAnimator == null)
        {
            currentEnemyAnimator = config.spriteRendererObject.AddComponent<Animator>();
        }

        // CRÍTICO: Asegurar que el Animator esté habilitado (especialmente importante para Damage y Effects)
        // Damage y Effects pueden haber sido deshabilitados en la inicialización, así que forzar activación
        if (currentEnemyAnimator != null)
        {
            currentEnemyAnimator.enabled = true;
            // Forzar actualización inmediata para asegurar que el cambio se aplique
            if (!currentEnemyAnimator.enabled)
            {
                Debug.LogWarning($"AnimationManager: No se pudo habilitar el Animator del enemigo para {config.spriteRendererObject.name}");
            }
        }

        // IMPORTANTE: Asignar sprite ANTES de reproducir la animación
        // para evitar que sobrescriba los cambios de la animación
        // Si hay overrideSprite (desde AttackData), usarlo; si no, usar enemySprite de EnemyData por defecto
        // CRÍTICO: Buscar el SpriteRenderer específico del spriteRendererObject actual
        // NO reutilizar enemySpriteRenderer global, cada estado tiene su propio SpriteRenderer
        Sprite spriteToUse = overrideSprite != null ? overrideSprite : (currentEnemyData != null ? currentEnemyData.enemySprite : null);
        if (spriteToUse != null)
        {
            SpriteRenderer currentStateSpriteRenderer = config.spriteRendererObject.GetComponent<SpriteRenderer>();
            if (currentStateSpriteRenderer == null)
            {
                currentStateSpriteRenderer = config.spriteRendererObject.GetComponentInChildren<SpriteRenderer>();
            }

            if (currentStateSpriteRenderer != null)
            {
                currentStateSpriteRenderer.sprite = spriteToUse;
            }
        }

        // Reproducir clip si está configurado
        // CRÍTICO: Siempre forzar la reproducción desde el inicio (0f) para asegurar que se reproduce
        // No verificar si ya está en el estado porque puede causar que no se reproduzca
        if (config.clip != null && currentEnemyAnimator != null)
        {
            // Asegurar que el Animator esté habilitado y listo
            if (!currentEnemyAnimator.enabled)
            {
                currentEnemyAnimator.enabled = true;
            }
            
            // Asignar controller si está configurado Y es diferente al actual
            // Esto debe hacerse ANTES de Play() para asegurar que el Animator tenga el controller correcto
            if (config.controller != null)
            {
                if (currentEnemyAnimator.runtimeAnimatorController != config.controller)
                {
                    // CRÍTICO: Deshabilitar el Animator antes de cambiar el controller para evitar reproducción automática
                    currentEnemyAnimator.enabled = false;
                    currentEnemyAnimator.runtimeAnimatorController = config.controller;
                    // CRÍTICO: Reactivar el Animator inmediatamente después de asignar el controller
                    // Esto es especialmente importante para Damage y Effects
                    currentEnemyAnimator.enabled = true;
                }
            }
            
            // CRÍTICO: Esperar un frame antes de Play() para asegurar que el controller esté completamente cargado
            yield return null;
            
            // CRÍTICO: Verificar nuevamente que el Animator esté habilitado justo antes de Play()
            // Esto es especialmente importante para Damage y Effects que pueden haber sido deshabilitados en la inicialización
            // Forzar activación explícita para Damage y Effects
            if (!currentEnemyAnimator.enabled)
            {
                currentEnemyAnimator.enabled = true;
            }
            
            // CRÍTICO ADICIONAL: Verificar una vez más que el Animator esté habilitado
            // Esto asegura que incluso si algo lo desactivó, se reactive antes de Play()
            // Especialmente importante para Damage y Effects
            if (currentEnemyAnimator != null && !currentEnemyAnimator.enabled)
            {
                currentEnemyAnimator.enabled = true;
            }
            
            // Forzar la reproducción desde el inicio (0f) para asegurar que se reproduce completamente
            // Parámetros: (nombre del estado/clip, capa, tiempo normalizado para reiniciar desde el inicio)
            currentEnemyAnimator.Play(config.clip.name, 0, 0f);
            
            // Forzar actualización inmediata del Animator para asegurar que la animación comience
            currentEnemyAnimator.Update(0f);
        }
    }

    /// <summary>
    /// Muestra el panel del enemigo y configura el Animator SIN reproducir la animación.
    /// Usado para Idle cuando solo queremos mostrar el panel sin reproducir.
    /// </summary>
    private void ShowEnemyPanelWithoutPlaying(AnimationStateConfig config)
    {
        if (config == null || config.spriteRendererObject == null)
            return;

        if (currentEnemyIndex < 0 || currentEnemyIndex >= enemyAnimationSets.Length)
            return;

        // Obtener el Animator del GameObject del estado actual
        Animator currentEnemyAnimator = config.spriteRendererObject.GetComponent<Animator>();
        if (currentEnemyAnimator == null)
        {
            currentEnemyAnimator = config.spriteRendererObject.AddComponent<Animator>();
        }

        // Asegurar que el Animator esté habilitado
        if (currentEnemyAnimator != null)
        {
            currentEnemyAnimator.enabled = true;
        }

        // Asignar sprite del enemigo desde EnemyData
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

        // Asignar controller si está configurado (pero NO reproducir la animación)
        if (config.controller != null && currentEnemyAnimator != null)
        {
            if (currentEnemyAnimator.runtimeAnimatorController != config.controller)
            {
                currentEnemyAnimator.runtimeAnimatorController = config.controller;
            }
        }

        // NO reproducir el clip aquí - solo configurar el Animator
        // La animación de Idle debe estar en loop en el Animator Controller
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
        
        if (playerDamage != null && playerDamage.spriteRendererObject != null)
            HidePanel(playerDamage.spriteRendererObject);
        
        if (playerEffects != null && playerEffects.spriteRendererObject != null)
            HidePanel(playerEffects.spriteRendererObject);
        
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
        
        if (playerDamage != null && playerDamage.spriteRendererObject != null && playerDamage.spriteRendererObject != exceptPanel)
            HidePanel(playerDamage.spriteRendererObject);
        
        if (playerEffects != null && playerEffects.spriteRendererObject != null && playerEffects.spriteRendererObject != exceptPanel)
            HidePanel(playerEffects.spriteRendererObject);
        
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
        
        if (enemySet.damage != null && enemySet.damage.spriteRendererObject != null)
            HidePanel(enemySet.damage.spriteRendererObject);
        
        if (enemySet.effects != null && enemySet.effects.spriteRendererObject != null)
            HidePanel(enemySet.effects.spriteRendererObject);
        
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
        
        if (enemySet.damage != null && enemySet.damage.spriteRendererObject != null && enemySet.damage.spriteRendererObject != exceptPanel)
            HidePanel(enemySet.damage.spriteRendererObject);
        
        if (enemySet.effects != null && enemySet.effects.spriteRendererObject != null && enemySet.effects.spriteRendererObject != exceptPanel)
            HidePanel(enemySet.effects.spriteRendererObject);
        
        if (enemySet.ko != null && enemySet.ko.spriteRendererObject != null && enemySet.ko.spriteRendererObject != exceptPanel)
            HidePanel(enemySet.ko.spriteRendererObject);
    }

    /// <summary>
    /// Oculta todas las animaciones (jugador y enemigos).
    /// Se llama cuando se cierra el panel de combate o al inicio del juego.
    /// CRÍTICO: Establece explícitamente Alpha = 0 para todos los paneles, sobrescribiendo valores del Inspector.
    /// </summary>
    public void HideAllAnimations()
    {
        // Ocultar todas las animaciones del jugador
        HideAllPlayerPanels();
        
        // CRÍTICO: Asegurar explícitamente que Defense, Damage, Effects y KO estén en Alpha = 0
        // Esto sobrescribe cualquier valor del Inspector
        if (playerDefense != null && playerDefense.spriteRendererObject != null)
        {
            SetPanelAlpha(playerDefense.spriteRendererObject, 0f);
        }
        if (playerDamage != null && playerDamage.spriteRendererObject != null)
        {
            SetPanelAlpha(playerDamage.spriteRendererObject, 0f);
        }
        if (playerEffects != null && playerEffects.spriteRendererObject != null)
        {
            SetPanelAlpha(playerEffects.spriteRendererObject, 0f);
        }
        if (playerKO != null && playerKO.spriteRendererObject != null)
        {
            SetPanelAlpha(playerKO.spriteRendererObject, 0f);
        }

        // Ocultar todas las animaciones de todos los enemigos
        if (enemyAnimationSets != null)
        {
            foreach (EnemyAnimationSet enemySet in enemyAnimationSets)
            {
                if (enemySet != null)
                {
                    HideAllEnemyPanels(enemySet);
                    
                    // CRÍTICO: Asegurar explícitamente que Defense, Damage, Effects y KO estén en Alpha = 0
                    if (enemySet.defense != null && enemySet.defense.spriteRendererObject != null)
                    {
                        SetPanelAlpha(enemySet.defense.spriteRendererObject, 0f);
                    }
                    if (enemySet.damage != null && enemySet.damage.spriteRendererObject != null)
                    {
                        SetPanelAlpha(enemySet.damage.spriteRendererObject, 0f);
                    }
                    if (enemySet.effects != null && enemySet.effects.spriteRendererObject != null)
                    {
                        SetPanelAlpha(enemySet.effects.spriteRendererObject, 0f);
                    }
                    if (enemySet.ko != null && enemySet.ko.spriteRendererObject != null)
                    {
                        SetPanelAlpha(enemySet.ko.spriteRendererObject, 0f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Muestra un panel (Alpha = 1).
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

        // Establecer alpha = 1
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
    /// CRÍTICO: Actualiza TODOS los componentes SpriteRenderer e Image, no solo el primero.
    /// </summary>
    private void SetPanelAlpha(GameObject panelObject, float alpha)
    {
        if (panelObject == null)
            return;

        // CRÍTICO: Actualizar TODOS los SpriteRenderer (incluido el del GameObject y todos los hijos)
        SpriteRenderer[] spriteRenderers = panelObject.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
            {
                Color color = sr.color;
                color.a = alpha;
                sr.color = color;
            }
        }

        // CRÍTICO: Actualizar TODAS las Image (incluida la del GameObject y todas las hijas)
        Image[] images = panelObject.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (img != null)
            {
                Color color = img.color;
                color.a = alpha;
                img.color = color;
            }
        }
    }
}

