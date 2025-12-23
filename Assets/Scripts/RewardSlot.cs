using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Componente UI para mostrar un objeto obtenido como recompensa.
/// Se usa en el panel de recompensas de combate.
/// </summary>
public class RewardSlot : MonoBehaviour
{
    [Header("Componentes UI")]
    [Tooltip("Imagen que muestra el sprite del objeto")]
    [SerializeField] private Image itemImage;
    
    [Tooltip("Texto que muestra el nombre del objeto")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    
    [Tooltip("Texto que muestra el nivel/rareza del objeto")]
    [SerializeField] private TextMeshProUGUI itemInfoText;
    
    [Tooltip("Imagen de fondo del slot")]
    [SerializeField] private Image backgroundImage;
    
    // Referencia al objeto mostrado
    private ItemInstance currentItem;
    
    /// <summary>
    /// Configura el slot con un objeto específico.
    /// </summary>
    /// <param name="itemInstance">Instancia del objeto a mostrar</param>
    public void Setup(ItemInstance itemInstance)
    {
        Debug.Log("=== REWARD SLOT SETUP INICIADO ===");
        Debug.Log($"ItemInstance null: {itemInstance == null}");
        
        if (itemInstance == null || !itemInstance.IsValid())
        {
            Debug.LogWarning("RewardSlot: ItemInstance inválido");
            ClearSlot();
            return;
        }
        
        Debug.Log($"ItemInstance.IsValid(): {itemInstance.IsValid()}");
        Debug.Log($"BaseItem null: {itemInstance.baseItem == null}");
        
        if (itemInstance.baseItem == null)
        {
            Debug.LogError("RewardSlot: BaseItem es NULL");
            ClearSlot();
            return;
        }
        
        Debug.Log($"BaseItem.itemName: {itemInstance.baseItem.itemName}");
        Debug.Log($"BaseItem.itemSprite null: {itemInstance.baseItem.itemSprite == null}");
        
        currentItem = itemInstance;
        
        // Configurar imagen del objeto
        Debug.Log($"ItemImage component null: {itemImage == null}");
        if (itemImage != null)
        {
            Sprite itemSprite = itemInstance.GetItemSprite();
            Debug.Log($"Sprite obtenido: {itemSprite != null}");
            
            if (itemSprite != null)
            {
                itemImage.sprite = itemSprite;
                itemImage.enabled = true;
                Debug.Log($"Sprite asignado: {itemSprite.name}");
            }
            else
            {
                itemImage.enabled = false;
                Debug.LogWarning("RewardSlot: Sprite es null, Image desactivada");
            }
        }
        
        // Configurar texto del nombre
        Debug.Log($"ItemNameText component null: {itemNameText == null}");
        if (itemNameText != null)
        {
            string itemName = itemInstance.GetItemName();
            itemNameText.text = itemName ?? "Objeto sin nombre";
            Debug.Log($"Nombre asignado: {itemName}");
        }
        
        // Configurar texto de información (nivel y rareza)
        Debug.Log($"ItemInfoText component null: {itemInfoText == null}");
        if (itemInfoText != null)
        {
            try
            {
                string rarity = itemInstance.GetRarity();
                if (!string.IsNullOrEmpty(rarity))
                {
                    itemInfoText.text = $"Nivel {itemInstance.currentLevel} - {rarity}";
                }
                else
                {
                    itemInfoText.text = $"Nivel {itemInstance.currentLevel}";
                }
                Debug.Log($"Info asignada: Nivel {itemInstance.currentLevel} - {rarity}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"RewardSlot: Error al obtener información del objeto: {e.Message}");
                itemInfoText.text = $"Nivel {itemInstance.currentLevel}";
            }
        }
        
        // Configurar color de fondo (blanco por defecto, sin colores de rareza)
        Debug.Log($"BackgroundImage component null: {backgroundImage == null}");
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
            Debug.Log("BackgroundImage color asignado a blanco");
        }
        
        Debug.Log("=== REWARD SLOT SETUP COMPLETADO ===");
    }
    
    /// <summary>
    /// Limpia el slot (lo deja vacío).
    /// </summary>
    public void ClearSlot()
    {
        currentItem = null;
        
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }
        
        if (itemNameText != null)
        {
            itemNameText.text = "";
        }
        
        if (itemInfoText != null)
        {
            itemInfoText.text = "";
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
        }
    }
    
    /// <summary>
    /// Obtiene el objeto actualmente mostrado en este slot.
    /// </summary>
    /// <returns>ItemInstance actual, o null si está vacío</returns>
    public ItemInstance GetCurrentItem()
    {
        return currentItem;
    }
    
    /// <summary>
    /// Verifica si el slot está ocupado.
    /// </summary>
    /// <returns>True si hay un objeto en el slot</returns>
    public bool IsOccupied()
    {
        return currentItem != null && currentItem.IsValid();
    }
    
    /// <summary>
    /// Añade un efecto de animación cuando se muestra el objeto.
    /// </summary>
    public void PlayAppearAnimation()
    {
        // Animación simple de escala sin LeanTween
        if (transform != null)
        {
            // Escalar de 0 a 1 con animación básica de Unity
            StartCoroutine(ScaleAnimation(Vector3.zero, Vector3.one, 0.5f));
        }
    }
    
    /// <summary>
    /// Añade un efecto de brillo cuando se muestra el objeto.
    /// </summary>
    public void PlayGlowEffect()
    {
        // Efecto de brillo simple sin LeanTween
        if (backgroundImage != null)
        {
            StartCoroutine(GlowAnimation());
        }
    }
    
    /// <summary>
    /// Corrutina para animación de escala.
    /// </summary>
    private IEnumerator ScaleAnimation(Vector3 fromScale, Vector3 toScale, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Usar curva ease-out-back simple
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            
            if (transform != null)
            {
                transform.localScale = Vector3.Lerp(fromScale, toScale, easedProgress);
            }
            
            yield return null;
        }
        
        if (transform != null)
        {
            transform.localScale = toScale;
        }
    }
    
    /// <summary>
    /// Corrutina para efecto de brillo.
    /// </summary>
    private IEnumerator GlowAnimation()
    {
        if (backgroundImage == null)
        {
            Debug.LogWarning("RewardSlot: backgroundImage es null, no se puede ejecutar GlowAnimation");
            yield break;
        }
            
        Color originalColor = backgroundImage.color;
        Color glowColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
        
        float duration = 1f;
        float elapsed = 0f;
        
        while (true)
        {
            elapsed += Time.deltaTime;
            float progress = (elapsed % (duration * 2f)) / duration;
            
            if (progress > 1f)
                progress = 2f - progress; // Ping-pong effect
            
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.Lerp(originalColor, glowColor, progress);
            }
            yield return null;
        }
    }
    
    private void Start()
    {
        // Asegurar que el slot esté limpio al iniciar
        ClearSlot();
    }
}
