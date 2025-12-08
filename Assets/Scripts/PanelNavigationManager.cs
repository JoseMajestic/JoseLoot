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

    private GameObject currentActivePanel;

    // Eventos
    public System.Action<GameObject> OnPanelOpened;
    public System.Action<GameObject> OnPanelClosed;

    private void Start()
    {
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
    /// </summary>
    public void OpenPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogWarning("Intentando abrir un panel nulo.");
            return;
        }

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

