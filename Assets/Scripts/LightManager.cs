using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Gestiona un efecto de luz y texto cuando el héroe sube de nivel.
/// </summary>
public class LightManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Light levelUpLight;
    [SerializeField] private TextMeshPro levelUpText;

    [Header("Configuración de Glow")]
    [SerializeField, Tooltip("Duración total del glow (subida + bajada)")]
    private float glowDurationSeconds = 2f;
    [SerializeField] private float minIntensity = 2f;
    [SerializeField] private float maxIntensity = 2000f;

    [Header("Texto")]
    [SerializeField, TextArea] private string defaultLevelUpMessage = "¡Has subido de nivel!";

    private Coroutine glowRoutine;

    private void OnEnable()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.HeroLeveledUp += OnHeroLeveledUp;
        }
    }

    private void OnDisable()
    {
        GameDataManager.HeroLeveledUp -= OnHeroLeveledUp;
        StopGlowRoutine(resetLight: true);
    }

    private void OnHeroLeveledUp()
    {
        TriggerLevelUpGlow(defaultLevelUpMessage);
    }

    /// <summary>
    /// Activa el texto de nivel y reproduce el efecto de luz configurado.
    /// </summary>
    /// <param name="message">Texto opcional que se mostrará en el TMP asignado.</param>
    public void TriggerLevelUpGlow(string message = null)
    {
        if (levelUpText != null)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                levelUpText.text = message;
            }
            levelUpText.gameObject.SetActive(true);
        }

        StopGlowRoutine();

        if (levelUpLight == null)
        {
            return;
        }

        glowRoutine = StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        float duration = Mathf.Max(0.01f, glowDurationSeconds);
        float halfDuration = duration * 0.5f;

        if (!levelUpLight.gameObject.activeInHierarchy)
        {
            levelUpLight.gameObject.SetActive(true);
        }

        levelUpLight.intensity = minIntensity;

        yield return AnimateIntensity(minIntensity, maxIntensity, halfDuration);
        yield return AnimateIntensity(maxIntensity, minIntensity, halfDuration);

        levelUpLight.intensity = minIntensity;

        if (levelUpText != null)
        {
            levelUpText.gameObject.SetActive(false);
        }

        glowRoutine = null;
    }

    private IEnumerator AnimateIntensity(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            levelUpLight.intensity = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            levelUpLight.intensity = Mathf.Lerp(from, to, t);
            yield return null;
        }
    }

    private void StopGlowRoutine(bool resetLight = false)
    {
        if (glowRoutine != null)
        {
            StopCoroutine(glowRoutine);
            glowRoutine = null;
        }

        if (resetLight && levelUpLight != null)
        {
            levelUpLight.intensity = minIntensity;
        }

        if (resetLight && levelUpText != null)
        {
            levelUpText.gameObject.SetActive(false);
        }
    }
}
