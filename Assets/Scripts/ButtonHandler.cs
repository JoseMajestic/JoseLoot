using UnityEngine;

/// <summary>
/// Handler simple para botones que abre/cierra paneles.
/// Se puede asignar directamente a botones en el Inspector.
/// </summary>
public class ButtonHandler : MonoBehaviour
{
    [Header("Paneles a Abrir")]
    [Tooltip("Paneles que se activarán al hacer clic en el botón")]
    [SerializeField] private GameObject[] panelsToOpen = new GameObject[0];

    [Header("Paneles a Cerrar")]
    [Tooltip("Paneles que se desactivarán al hacer clic en el botón")]
    [SerializeField] private GameObject[] panelsToClose = new GameObject[0];

    [Header("Configuración")]
    [Tooltip("Si es true, solo se puede abrir un panel a la vez (cierra otros automáticamente)")]
    [SerializeField] private bool exclusiveMode = false;

    [Tooltip("Referencia al PanelNavigationManager (opcional, para modo exclusivo)")]
    [SerializeField] private PanelNavigationManager panelNavigationManager;

    /// <summary>
    /// Método que se llama al hacer clic en el botón.
    /// Se puede asignar directamente al evento OnClick del botón.
    /// </summary>
    public void OnButtonClick()
    {
        // Si está en modo exclusivo y hay PanelNavigationManager, usarlo
        if (exclusiveMode && panelNavigationManager != null)
        {
            // Abrir el primer panel de la lista (si hay)
            if (panelsToOpen != null && panelsToOpen.Length > 0 && panelsToOpen[0] != null)
            {
                panelNavigationManager.OpenPanel(panelsToOpen[0]);
            }
        }
        else
        {
            // Modo normal: abrir y cerrar paneles según los arrays
            OpenPanels();
            ClosePanels();
        }
    }

    /// <summary>
    /// Abre los paneles configurados.
    /// </summary>
    private void OpenPanels()
    {
        if (panelsToOpen == null)
            return;

        foreach (GameObject panel in panelsToOpen)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Cierra los paneles configurados.
    /// </summary>
    private void ClosePanels()
    {
        if (panelsToClose == null)
            return;

        foreach (GameObject panel in panelsToClose)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Abre un panel específico (método público para uso desde código).
    /// </summary>
    public void OpenPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    /// <summary>
    /// Cierra un panel específico (método público para uso desde código).
    /// </summary>
    public void ClosePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}

