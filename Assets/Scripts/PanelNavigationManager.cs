using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la navegación entre paneles.
/// Controla qué panel está activo y puede cerrar otros automáticamente.
/// </summary>
public class PanelNavigationManager : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Si es true, solo un panel puede estar abierto a la vez")]
    [SerializeField] private bool exclusiveMode = true;

    [Tooltip("Paneles que se gestionan (se pueden cerrar automáticamente)")]
    [SerializeField] private GameObject[] managedPanels = new GameObject[0];

    [Header("Panel Inicial")]
    [Tooltip("Panel que estará abierto al inicio (opcional)")]
    [SerializeField] private GameObject initialPanel;

    [Header("Efecto Fade entre Paneles")]
    [Tooltip("Image negro que cubre toda la pantalla durante las transiciones (debe estar en un Canvas con orden superior)")]
    [SerializeField] private UnityEngine.UI.Image fadeOverlay;

    [Tooltip("Duración del fade in/out en segundos")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Tooltip("Si es true, usa fade al cambiar entre paneles. Si es false, cambio directo sin fade")]
    [SerializeField] private bool useFadeTransition = true;

    private GameObject currentActivePanel;
    private bool isTransitioning = false; // Evitar múltiples transiciones simultáneas

    // Eventos
    public System.Action<GameObject> OnPanelOpened;
    public System.Action<GameObject> OnPanelClosed;

    private void Start()
    {
        // Inicializar fade overlay
        if (fadeOverlay != null)
        {
            // Configurar overlay como transparente inicialmente
            Color color = fadeOverlay.color;
            color.a = 0f;
            fadeOverlay.color = color;
            fadeOverlay.gameObject.SetActive(false);
        }

        // Abrir panel inicial si está configurado
        if (initialPanel != null)
        {
            OpenPanel(initialPanel);
        }
        else if (exclusiveMode && managedPanels != null && managedPanels.Length > 0)
        {
            // Si no hay panel inicial, cerrar todos los paneles gestionados
            CloseAllPanels();
        }
    }

    /// <summary>
    /// Abre un panel específico.
    /// Si está en modo exclusivo, cierra otros paneles automáticamente.
    /// Usa fade transition si está habilitado y hay un cambio de panel.
    /// </summary>
    public void OpenPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogWarning("Intentando abrir un panel nulo.");
            return;
        }

        // Si el panel ya está abierto, no hacer nada
        if (currentActivePanel == panel && panel.activeSelf)
        {
            return;
        }

        // Si hay una transición en curso, ignorar la nueva petición
        if (isTransitioning)
        {
            return;
        }

        // Si hay cambio de panel y fade está habilitado, usar fade
        bool shouldUseFade = useFadeTransition && 
                             fadeOverlay != null && 
                             exclusiveMode && 
                             currentActivePanel != null && 
                             currentActivePanel != panel;

        if (shouldUseFade)
        {
            StartCoroutine(OpenPanelWithFade(panel));
        }
        else
        {
            // Cambio directo sin fade
            OpenPanelDirect(panel);
        }
    }

    /// <summary>
    /// Abre un panel directamente sin fade (método interno).
    /// </summary>
    private void OpenPanelDirect(GameObject panel)
    {
        // Si está en modo exclusivo, cerrar el panel actual
        if (exclusiveMode && currentActivePanel != null && currentActivePanel != panel)
        {
            ClosePanel(currentActivePanel);
        }

        // Abrir el nuevo panel
        panel.SetActive(true);
        currentActivePanel = panel;
        OnPanelOpened?.Invoke(panel);
    }

    /// <summary>
    /// Abre un panel con efecto fade negro.
    /// </summary>
    private IEnumerator OpenPanelWithFade(GameObject panel)
    {
        isTransitioning = true;

        // Asegurar que el overlay esté activo
        if (!fadeOverlay.gameObject.activeSelf)
        {
            fadeOverlay.gameObject.SetActive(true);
        }

        // FADE IN: De transparente a negro
        yield return StartCoroutine(FadeImage(fadeOverlay, 0f, 1f, fadeDuration));

        // Cambiar paneles mientras está negro
        if (exclusiveMode && currentActivePanel != null && currentActivePanel != panel)
        {
            ClosePanel(currentActivePanel);
        }

        // Abrir el nuevo panel
        panel.SetActive(true);
        currentActivePanel = panel;

        // Pequeña pausa para que el cambio de paneles se complete
        yield return new WaitForSeconds(0.05f);

        // FADE OUT: De negro a transparente
        yield return StartCoroutine(FadeImage(fadeOverlay, 1f, 0f, fadeDuration));

        // Ocultar overlay después del fade out
        fadeOverlay.gameObject.SetActive(false);

        // Invocar evento
        OnPanelOpened?.Invoke(panel);

        isTransitioning = false;
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
    /// Cierra un panel específico.
    /// </summary>
    public void ClosePanel(GameObject panel)
    {
        if (panel == null)
            return;

        panel.SetActive(false);

        if (currentActivePanel == panel)
        {
            currentActivePanel = null;
        }

        OnPanelClosed?.Invoke(panel);
    }

    /// <summary>
    /// Cierra todos los paneles gestionados.
    /// </summary>
    public void CloseAllPanels()
    {
        if (managedPanels == null)
            return;

        foreach (GameObject panel in managedPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        currentActivePanel = null;
    }

    /// <summary>
    /// Obtiene el panel actualmente activo.
    /// </summary>
    public GameObject GetCurrentActivePanel()
    {
        return currentActivePanel;
    }

    /// <summary>
    /// Alterna un panel (si está abierto lo cierra, si está cerrado lo abre).
    /// Usa fade si está habilitado al abrir un panel diferente.
    /// </summary>
    public void TogglePanel(GameObject panel)
    {
        if (panel == null)
            return;

        if (panel.activeSelf)
        {
            ClosePanel(panel);
        }
        else
        {
            OpenPanel(panel);
        }
    }

    /// <summary>
    /// Añade un panel a la lista de paneles gestionados.
    /// </summary>
    public void AddManagedPanel(GameObject panel)
    {
        if (panel == null)
            return;

        List<GameObject> panelList = new List<GameObject>(managedPanels);
        if (!panelList.Contains(panel))
        {
            panelList.Add(panel);
            managedPanels = panelList.ToArray();
        }
    }

    /// <summary>
    /// Elimina un panel de la lista de paneles gestionados.
    /// </summary>
    public void RemoveManagedPanel(GameObject panel)
    {
        if (panel == null)
            return;

        List<GameObject> panelList = new List<GameObject>(managedPanels);
        if (panelList.Remove(panel))
        {
            managedPanels = panelList.ToArray();
        }
    }
}

