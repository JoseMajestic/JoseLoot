using UnityEngine;

/// <summary>
/// Ajusta la intensidad de una luz 3D cada vez que el héroe sube de nivel.
/// Permite configurar el incremento por nivel desde el inspector.
/// </summary>
public class LightManager3D : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Luz que se ajustará. Si se deja vacío, tomará la luz del mismo GameObject.")]
    [SerializeField] private Light targetLight;

    [Header("Configuración")]
    [Tooltip("Incremento de intensidad aplicado cada vez que el héroe sube un nivel.")]
    [SerializeField] private float intensityPerLevel = 2f;

    [Tooltip("Nivel de referencia a partir del cual se empieza a sumar intensidad.")]
    [SerializeField] private int referenceLevel = 1;

    [Tooltip("Aplicar automáticamente la intensidad correspondiente al nivel actual al habilitar el objeto.")]
    [SerializeField] private bool applyInitialSync = true;

    [Tooltip("Limitar la intensidad final dentro de un rango.")]
    [SerializeField] private bool clampIntensity = false;

    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 5000f;

    private GameDataManager gameDataManager;
    private float baseIntensity;
    private bool initialized;

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        if (targetLight != null)
        {
            baseIntensity = targetLight.intensity;
            initialized = true;
        }
        else
        {
            Debug.LogWarning($"{nameof(LightManager3D)}: No se asignó ninguna luz.");
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
        if (applyInitialSync)
        {
            ApplyCurrentLevelIntensity();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (gameDataManager == null)
        {
            gameDataManager = GameDataManager.Instance;
        }

        if (gameDataManager == null)
            return;

        GameDataManager.HeroLeveledUp += HandleHeroLeveledUp;
    }

    private void UnsubscribeFromEvents()
    {
        GameDataManager.HeroLeveledUp -= HandleHeroLeveledUp;
    }

    private void HandleHeroLeveledUp()
    {
        ApplyCurrentLevelIntensity();
    }

    private void ApplyCurrentLevelIntensity()
    {
        if (!initialized || targetLight == null)
            return;

        if (gameDataManager == null)
        {
            gameDataManager = GameDataManager.Instance;
        }

        PlayerProfileData profile = gameDataManager != null ? gameDataManager.GetPlayerProfile() : null;
        int heroLevel = profile != null ? Mathf.Max(1, profile.heroLevel) : referenceLevel;

        float levelsAboveReference = Mathf.Max(0, heroLevel - referenceLevel);
        float targetIntensity = baseIntensity + (levelsAboveReference * intensityPerLevel);

        if (clampIntensity)
        {
            targetIntensity = Mathf.Clamp(targetIntensity, minIntensity, maxIntensity);
        }

        targetLight.intensity = targetIntensity;
    }
}
