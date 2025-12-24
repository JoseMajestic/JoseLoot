using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Panel visual reutilizable para el sistema de historia
/// NO DECIDE NADA, solo muestra contenido
/// </summary>
public class UIStoryPanel : MonoBehaviour
{
    [Header("Componentes UI")]
    [SerializeField] private Image storyImage;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private GameObject buttonPrefab;
    
    private List<GameObject> currentButtons = new List<GameObject>();
    
    /// <summary>
    /// Muestra el contenido del nodo con los botones especificados
    /// </summary>
    public void Show(Sprite image, string text, params StoryButton[] buttons)
    {
        // Limpiar botones anteriores
        ClearButtons();
        
        // Mostrar imagen
        if (storyImage != null)
        {
            storyImage.sprite = image;
            storyImage.gameObject.SetActive(image != null);
        }
        
        // Mostrar texto
        if (storyText != null)
        {
            storyText.text = text;
        }
        
        // Crear botones
        if (buttons != null && buttons.Length > 0)
        {
            foreach (var buttonData in buttons)
            {
                CreateButton(buttonData);
            }
        }
        
        // Activar panel
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Oculta el panel
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        ClearButtons();
    }
    
    private void CreateButton(StoryButton buttonData)
    {
        if (buttonPrefab == null || buttonsContainer == null)
        {
            Debug.LogError("UIStoryPanel: buttonPrefab o buttonsContainer no asignados");
            return;
        }
        
        GameObject buttonObj = Instantiate(buttonPrefab, buttonsContainer);
        Button buttonComponent = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(() => buttonData.onClick?.Invoke());
        }
        
        if (buttonText != null)
        {
            buttonText.text = buttonData.text;
        }
        
        currentButtons.Add(buttonObj);
    }
    
    private void ClearButtons()
    {
        foreach (var button in currentButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
        currentButtons.Clear();
    }
    
    private void Start()
    {
        Hide(); // Empezar oculto
    }
}
