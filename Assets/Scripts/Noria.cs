using UnityEngine;
using System.Collections;

/// <summary>
/// Rota un cilindro continuamente en el eje Y para crear un efecto de paisaje giratorio.
/// Gestiona el fondo mostrado según el nivel del jugador o el panel activo.
/// </summary>
public class Noria : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [Tooltip("Velocidad de rotación en grados por segundo")]
    [SerializeField] private float rotationSpeed = 10f;
    
    [Tooltip("Si está activado, la rotación se invierte (gira en sentido contrario)")]
    [SerializeField] private bool reverseRotation = false;
    
    [Tooltip("Si está activado, la rotación se pausa")]
    [SerializeField] private bool pauseRotation = false;

    [Header("Configuración de Textura")]
    [Tooltip("Material del cilindro que contiene la textura a cambiar")]
    [SerializeField] private Material cylinderMaterial;
    
    [Header("Fondos por Nivel")]
    [Tooltip("Sprites ordenados según la progresión de nivel. Se usarán hasta 100 entradas.")]
    [SerializeField] private Sprite[] levelBackgrounds;

    [Tooltip("Sprite que se mostrará si no hay uno configurado para el nivel actual.")]
    [SerializeField] private Sprite defaultBackground;

    [Header("Fondos por Panel")]
    [Tooltip("Sprite específico al entrar en el panel general de expediciones.")]
    [SerializeField] private Sprite expeditionBackground;

    [Tooltip("Sprite específico al entrar en el panel general de combate/batalla.")]
    [SerializeField] private Sprite combatBackground;

    [Header("Seguimiento de Paneles")]
    [Tooltip("PanelNavigationManager que dispara eventos al abrir/cerrar paneles.")]
    [SerializeField] private PanelNavigationManager panelNavigationManager;

    [Tooltip("Panel(es) que deben activar el fondo de expedición.")]
    [SerializeField] private GameObject[] expeditionPanels;

    [Tooltip("Panel(es) que deben activar el fondo de combate/batalla.")]
    [SerializeField] private GameObject[] combatPanels;

    private Renderer cylinderRenderer;
    private Sprite lastAppliedSprite;
    private int currentLevel = 1;
    private PanelContext currentPanelContext = PanelContext.LevelDriven;
    private GameDataManager gameDataManager;
    private bool levelEventsSubscribed;
    private bool panelEventsSubscribed;
    private bool expeditionPanelsPreviouslyActive;
    private bool combatPanelsPreviouslyActive;

    private enum PanelContext
    {
        LevelDriven,
        Expedition,
        Combat
    }

    private void Awake()
    {
        // Intentar obtener el Renderer si no se asignó el Material directamente
        if (cylinderMaterial == null)
        {
            cylinderRenderer = GetComponent<Renderer>();
            if (cylinderRenderer != null)
            {
                cylinderMaterial = cylinderRenderer.material;
            }
        }
    }

    private void OnEnable()
    {
        SubscribeToLevelEvents();
        SubscribeToPanelEvents();
        InitializePanelStateTracking();
        UpdateBackgroundSprite();
    }

    private void OnDisable()
    {
        UnsubscribeFromLevelEvents();
        UnsubscribeFromPanelEvents();
    }

    private void Update()
    {
        if (!pauseRotation)
        {
            // Calcular la rotación para este frame
            float rotationAmount = rotationSpeed * Time.deltaTime;

            // Invertir si es necesario
            if (reverseRotation)
                rotationAmount = -rotationAmount;

            // Rotar en el eje Y (vertical)
            transform.Rotate(0f, rotationAmount, 0f, Space.Self);
        }

        MonitorAssignedPanels();
    }

    /// <summary>
    /// Actualiza la textura del cilindro según el contexto actual.
    /// </summary>
    private void UpdateBackgroundSprite()
    {
        if (cylinderMaterial == null)
            return;

        Sprite targetSprite = currentPanelContext switch
        {
            PanelContext.Expedition => expeditionBackground,
            PanelContext.Combat => combatBackground,
            _ => ResolveLevelSprite(currentLevel)
        };

        if (targetSprite == null)
            targetSprite = lastAppliedSprite != null ? lastAppliedSprite : defaultBackground;

        ApplySpriteToMaterial(targetSprite);
    }

    /// <summary>
    /// Obtiene el sprite adecuado para un nivel concreto siguiendo las reglas solicitadas.
    /// </summary>
    private Sprite ResolveLevelSprite(int level)
    {
        if (levelBackgrounds == null || levelBackgrounds.Length == 0)
            return defaultBackground;

        int index = GetLevelBackgroundIndex(level);
        index = Mathf.Clamp(index, 0, levelBackgrounds.Length - 1);
        return levelBackgrounds[index];
    }

    /// <summary>
    /// Calcula el índice del sprite para un nivel: 
    /// niveles 1-10 cambian cada nivel, 11-50 cada 5 niveles, a partir de 51 vuelve a cambiar cada nivel.
    /// </summary>
    private int GetLevelBackgroundIndex(int level)
    {
        if (level <= 1)
            return 0;

        if (level <= 10)
            return level - 1;

        if (level <= 50)
        {
            int offset = level - 11;
            return 10 + (offset / 5);
        }

        // A partir del 51 vuelve a cambiar de uno en uno.
        int index = 18 + (level - 51);

        // Evitar overflow si el nivel supera las imágenes disponibles: usar última disponible.
        if (index >= levelBackgrounds.Length)
        {
            index = levelBackgrounds.Length - 1;
        }

        return index;
    }

    /// <summary>
    /// Aplica un sprite al material manteniendo la última textura válida si falta el sprite.
    /// </summary>
    private void ApplySpriteToMaterial(Sprite sprite)
    {
        if (sprite == null || cylinderMaterial == null)
            return;

        Texture2D texture = SpriteToTexture2D(sprite);
        if (texture == null)
            return;

        cylinderMaterial.mainTexture = texture;
        lastAppliedSprite = sprite;
    }

    /// <summary>
    /// Establece el nivel actual del jugador para actualizar el fondo.
    /// </summary>
    public void SetPlayerLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);
        if (currentPanelContext == PanelContext.LevelDriven)
        {
            UpdateBackgroundSprite();
        }
    }

    /// <summary>
    /// Activa el fondo especial al entrar en un panel específico.
    /// </summary>
    public void SetPanelContextToExpedition()
    {
        currentPanelContext = PanelContext.Expedition;
        UpdateBackgroundSprite();
    }

    /// <summary>
    /// Activa el fondo especial para combate/batalla.
    /// </summary>
    public void SetPanelContextToCombat()
    {
        currentPanelContext = PanelContext.Combat;
        UpdateBackgroundSprite();
    }

    /// <summary>
    /// Vuelve al modo controlado por nivel (por ejemplo, al salir de paneles especiales).
    /// </summary>
    public void ClearPanelContextOverride()
    {
        currentPanelContext = PanelContext.LevelDriven;
        UpdateBackgroundSprite();
    }

    /// <summary>
    /// Convierte un Sprite a Texture2D usando la textura directamente.
    /// </summary>
    private Texture2D SpriteToTexture2D(Sprite sprite)
    {
        if (sprite == null)
            return null;

        // Usar directamente la textura del sprite (no requiere Read/Write Enabled)
        // Si el sprite es parte de un atlas, esto devolverá la textura completa del atlas
        return sprite.texture;
    }

    private void SubscribeToLevelEvents()
    {
        if (levelEventsSubscribed)
            return;

        gameDataManager = GameDataManager.Instance;
        if (gameDataManager == null)
        {
            Debug.LogWarning("Noria: GameDataManager no disponible; no se actualizará el fondo por nivel.");
            return;
        }

        GameDataManager.HeroLeveledUp += HandleHeroLeveledUp;
        levelEventsSubscribed = true;

        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile != null)
        {
            SetPlayerLevel(profile.heroLevel);
        }
    }

    private void UnsubscribeFromLevelEvents()
    {
        if (!levelEventsSubscribed)
            return;

        GameDataManager.HeroLeveledUp -= HandleHeroLeveledUp;
        levelEventsSubscribed = false;
    }

    private void HandleHeroLeveledUp()
    {
        if (gameDataManager == null)
            gameDataManager = GameDataManager.Instance;

        int heroLevel = 1;
        PlayerProfileData profile = gameDataManager?.GetPlayerProfile();
        if (profile != null)
        {
            heroLevel = profile.heroLevel;
        }

        SetPlayerLevel(heroLevel);
    }

    private void SubscribeToPanelEvents()
    {
        if (panelEventsSubscribed)
            return;

        if (panelNavigationManager == null)
        {
            if (gameDataManager == null)
            {
                gameDataManager = GameDataManager.Instance;
            }
            panelNavigationManager = gameDataManager != null
                ? gameDataManager.PanelNavigationManager
                : FindFirstObjectByType<PanelNavigationManager>();
        }

        if (panelNavigationManager == null)
            return;

        panelNavigationManager.OnPanelOpened += HandlePanelOpened;
        panelNavigationManager.OnPanelClosed += HandlePanelClosed;
        panelEventsSubscribed = true;
    }

    private void UnsubscribeFromPanelEvents()
    {
        if (!panelEventsSubscribed || panelNavigationManager == null)
            return;

        panelNavigationManager.OnPanelOpened -= HandlePanelOpened;
        panelNavigationManager.OnPanelClosed -= HandlePanelClosed;
        panelEventsSubscribed = false;
    }

    private void HandlePanelOpened(GameObject openedPanel)
    {
        if (openedPanel == null)
            return;

        if (IsPanelInList(openedPanel, expeditionPanels))
        {
            SetPanelContextToExpedition();
            return;
        }

        if (IsPanelInList(openedPanel, combatPanels))
        {
            SetPanelContextToCombat();
        }
    }

    private void HandlePanelClosed(GameObject closedPanel)
    {
        if (closedPanel == null)
            return;

        bool isExpeditionPanel = IsPanelInList(closedPanel, expeditionPanels);
        bool isCombatPanel = IsPanelInList(closedPanel, combatPanels);

        if ((currentPanelContext == PanelContext.Expedition && isExpeditionPanel) ||
            (currentPanelContext == PanelContext.Combat && isCombatPanel))
        {
            ClearPanelContextOverride();
        }
    }

    private static bool IsPanelInList(GameObject panel, GameObject[] list)
    {
        if (panel == null || list == null)
            return false;

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == panel)
                return true;
        }
        return false;
    }

    private void InitializePanelStateTracking()
    {
        expeditionPanelsPreviouslyActive = AreAnyPanelsActive(expeditionPanels);
        combatPanelsPreviouslyActive = AreAnyPanelsActive(combatPanels);

        // Si algún panel ya está abierto al habilitar Noria, respetar su contexto.
        if (combatPanelsPreviouslyActive)
        {
            SetPanelContextToCombat();
        }
        else if (expeditionPanelsPreviouslyActive)
        {
            SetPanelContextToExpedition();
        }
    }

    private void MonitorAssignedPanels()
    {
        bool expeditionActive = AreAnyPanelsActive(expeditionPanels);
        bool combatActive = AreAnyPanelsActive(combatPanels);

        // Prioridad: combate sobre expedición cuando ambos activos.
        if (!combatPanelsPreviouslyActive && combatActive)
        {
            SetPanelContextToCombat();
        }
        else if (combatPanelsPreviouslyActive && !combatActive && currentPanelContext == PanelContext.Combat)
        {
            // Si aún hay paneles de expedición activos, volver a ellos; si no, limpiar.
            if (expeditionActive)
            {
                SetPanelContextToExpedition();
            }
            else
            {
                ClearPanelContextOverride();
            }
        }

        if (!expeditionPanelsPreviouslyActive && expeditionActive && currentPanelContext != PanelContext.Combat)
        {
            SetPanelContextToExpedition();
        }
        else if (expeditionPanelsPreviouslyActive && !expeditionActive && currentPanelContext == PanelContext.Expedition)
        {
            ClearPanelContextOverride();
        }

        combatPanelsPreviouslyActive = combatActive;
        expeditionPanelsPreviouslyActive = expeditionActive;
    }

    private static bool AreAnyPanelsActive(GameObject[] panels)
    {
        if (panels == null || panels.Length == 0)
            return false;

        for (int i = 0; i < panels.Length; i++)
        {
            GameObject panel = panels[i];
            if (panel != null && panel.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Establece la velocidad de rotación.
    /// </summary>
    /// <param name="speed">Velocidad en grados por segundo</param>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    /// <summary>
    /// Obtiene la velocidad de rotación actual.
    /// </summary>
    public float GetRotationSpeed()
    {
        return rotationSpeed;
    }

    /// <summary>
    /// Invierte la dirección de rotación.
    /// </summary>
    public void ToggleReverseRotation()
    {
        reverseRotation = !reverseRotation;
    }

    /// <summary>
    /// Establece el estado de pausa de la rotación.
    /// </summary>
    /// <param name="value">true para pausar, false para reanudar</param>
    public void SetPauseRotation(bool value)
    {
        pauseRotation = value;
    }
}

