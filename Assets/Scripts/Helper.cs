using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Helper script que pulsa automáticamente una secuencia de botones en frames específicos.
/// Útil para inicializar paneles en el orden correcto sin que el jugador lo note.
/// </summary>
public class Helper : MonoBehaviour
{
    [Header("Configuración de Botones")]
    [Tooltip("Array de botones que se pulsarán en secuencia")]
    [SerializeField] private Button[] buttonsToClick;

    [Header("Configuración de Timing")]
    [Tooltip("Número de frames a esperar entre cada clic de botón")]
    [SerializeField] private int framesBetweenClicks = 1;

    [Tooltip("Número de frames a esperar antes de empezar a pulsar botones (útil para pantalla de carga)")]
    [SerializeField] private int initialDelayFrames = 0;

    [Tooltip("Si está marcado, los botones se pulsarán automáticamente al iniciar")]
    [SerializeField] private bool autoClickOnStart = true;

    [Header("Clic Automático en Inventario")]
    [Tooltip("Slot del inventario al que se hará clic automáticamente después de refrescar todo")]
    [SerializeField] private InventorySlot inventorySlotToClick;

    [Tooltip("Número de frames a esperar después de pulsar todos los botones antes de hacer clic en el slot del inventario")]
    [SerializeField] private int framesBeforeInventoryClick = 5;

    private void Start()
    {
        if (autoClickOnStart)
        {
            StartCoroutine(ClickButtonsSequence());
        }
    }

    /// <summary>
    /// Inicia la secuencia de clics en los botones.
    /// Se puede llamar manualmente desde otros scripts si autoClickOnStart está desactivado.
    /// </summary>
    public void StartButtonSequence()
    {
        StartCoroutine(ClickButtonsSequence());
    }

    /// <summary>
    /// Corrutina que pulsa los botones en secuencia con el delay configurado.
    /// </summary>
    private IEnumerator ClickButtonsSequence()
    {
        // Esperar el delay inicial (útil para pantalla de carga)
        for (int i = 0; i < initialDelayFrames; i++)
        {
            yield return null;
        }

        // Pulsar cada botón en secuencia
        if (buttonsToClick != null && buttonsToClick.Length > 0)
        {
            for (int i = 0; i < buttonsToClick.Length; i++)
            {
                Button button = buttonsToClick[i];
                
                if (button != null && button.gameObject.activeInHierarchy)
                {
                    // Pulsar el botón
                    if (button.onClick != null)
                    {
                        button.onClick.Invoke();
                    }
                    
                    // Esperar el número de frames configurado antes del siguiente botón
                    for (int j = 0; j < framesBetweenClicks; j++)
                    {
                        yield return null;
                    }
                }
                else
                {
                    // Si el botón es null o está inactivo, solo esperar un frame y continuar
                    yield return null;
                }
            }
        }

        Debug.Log($"Helper: Secuencia de {buttonsToClick?.Length ?? 0} botones completada.");

        // Esperar frames adicionales para que el inventario se refresque completamente
        for (int i = 0; i < framesBeforeInventoryClick; i++)
        {
            yield return null;
        }

        // Hacer clic automático en el slot del inventario si está configurado
        if (inventorySlotToClick != null)
        {
            // Esperar un frame adicional para asegurar que el slot esté completamente inicializado
            yield return null;
            
            // Verificar que el slot esté activo
            if (inventorySlotToClick.gameObject.activeInHierarchy)
            {
                // Obtener el Button del slot
                Button slotButton = inventorySlotToClick.GetComponent<Button>();
                if (slotButton == null)
                {
                    slotButton = inventorySlotToClick.GetComponentInChildren<Button>();
                }
                
                if (slotButton != null)
                {
                    // Asegurar que el botón esté interactuable
                    if (!slotButton.interactable)
                    {
                        slotButton.interactable = true;
                    }
                    
                    // Intentar invocar el evento OnSlotClicked del InventorySlot primero
                    if (inventorySlotToClick.OnSlotClicked != null)
                    {
                        inventorySlotToClick.OnSlotClicked.Invoke(inventorySlotToClick);
                        Debug.Log("Helper: Clic automático realizado en slot del inventario (OnSlotClicked).");
                    }
                    else if (slotButton.onClick != null)
                    {
                        // Si no hay evento OnSlotClicked, hacer clic en el botón directamente
                        slotButton.onClick.Invoke();
                        Debug.Log("Helper: Clic automático realizado en botón del slot del inventario (onClick).");
                    }
                    else
                    {
                        Debug.LogWarning("Helper: No se pudo hacer clic en el slot - no hay eventos disponibles.");
                    }
                }
                else
                {
                    Debug.LogWarning("Helper: No se encontró Button en el InventorySlot.");
                }
            }
            else
            {
                Debug.LogWarning("Helper: El InventorySlot no está activo en la jerarquía.");
            }
        }
    }

    /// <summary>
    /// Establece el array de botones programáticamente (útil para configuración dinámica).
    /// </summary>
    public void SetButtons(Button[] buttons)
    {
        buttonsToClick = buttons;
    }

    /// <summary>
    /// Establece el número de frames entre clics programáticamente.
    /// </summary>
    public void SetFramesBetweenClicks(int frames)
    {
        framesBetweenClicks = Mathf.Max(0, frames);
    }

    /// <summary>
    /// Establece el delay inicial programáticamente.
    /// </summary>
    public void SetInitialDelay(int frames)
    {
        initialDelayFrames = Mathf.Max(0, frames);
    }
}

