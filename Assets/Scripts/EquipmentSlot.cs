using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa un slot individual de equipo en la UI.
/// Gestiona la visualización y la interacción con un slot de equipo.
/// </summary>
public class EquipmentSlot : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button slotButton;
    [SerializeField] private GameObject emptySlotIndicator;
    [SerializeField] private Text slotLabel; // Opcional: muestra el nombre del slot

    [Header("Configuración")]
    [SerializeField] private EquipmentManager.EquipmentSlotType slotType;
    [SerializeField] private Color emptySlotColor = Color.gray;
    [SerializeField] private Color occupiedSlotColor = Color.white;
    [SerializeField] private Color selectedSlotColor = Color.yellow;

    private ItemInstance currentItem;
    private EquipmentManager equipmentManager;
    private bool isSelected = false;

    // Eventos
    public System.Action<EquipmentSlot> OnSlotClicked;
    public System.Action<EquipmentSlot, ItemInstance> OnItemChanged;

    private void Awake()
    {
        if (slotButton == null)
            slotButton = GetComponent<Button>();

        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClick);

        // Actualizar etiqueta del slot si existe
        if (slotLabel != null)
        {
            slotLabel.text = GetSlotTypeName();
        }
    }

    /// <summary>
    /// Inicializa el slot con su tipo y referencia al EquipmentManager.
    /// </summary>
    public void Initialize(EquipmentManager.EquipmentSlotType type, EquipmentManager manager)
    {
        slotType = type;
        equipmentManager = manager;
        UpdateVisuals();
    }

    /// <summary>
    /// Establece el item que se muestra en este slot.
    /// </summary>
    public void SetItem(ItemInstance itemInstance)
    {
        currentItem = itemInstance;
        UpdateVisuals();
        OnItemChanged?.Invoke(this, itemInstance);
    }

    /// <summary>
    /// Obtiene el ItemInstance actual en este slot.
    /// </summary>
    public ItemInstance GetItem()
    {
        return currentItem;
    }

    /// <summary>
    /// Obtiene el ItemData base del item en este slot (para compatibilidad).
    /// </summary>
    public ItemData GetBaseItem()
    {
        return currentItem != null && currentItem.IsValid() ? currentItem.baseItem : null;
    }

    /// <summary>
    /// Obtiene el tipo de slot de equipo.
    /// </summary>
    public EquipmentManager.EquipmentSlotType GetSlotType()
    {
        return slotType;
    }

    /// <summary>
    /// Selecciona o deselecciona este slot.
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
    }

    /// <summary>
    /// Verifica si este slot está seleccionado.
    /// </summary>
    public bool IsSelected()
    {
        return isSelected;
    }

    /// <summary>
    /// Actualiza la visualización del slot.
    /// </summary>
    private void UpdateVisuals()
    {
        if (currentItem == null || !currentItem.IsValid())
        {
            // Slot vacío
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }

            if (emptySlotIndicator != null)
                emptySlotIndicator.SetActive(true);

            if (backgroundImage != null)
                backgroundImage.color = isSelected ? selectedSlotColor : emptySlotColor;
        }
        else
        {
            // Slot ocupado
            if (itemIcon != null)
            {
                itemIcon.sprite = currentItem.GetItemSprite();
                itemIcon.enabled = true;
            }

            if (emptySlotIndicator != null)
                emptySlotIndicator.SetActive(false);

            if (backgroundImage != null)
                backgroundImage.color = isSelected ? selectedSlotColor : occupiedSlotColor;
        }
    }

    /// <summary>
    /// Maneja el clic en el slot.
    /// </summary>
    private void OnSlotClick()
    {
        OnSlotClicked?.Invoke(this);
    }

    /// <summary>
    /// Obtiene el nombre del tipo de slot para mostrar en la UI.
    /// Orden: Montura, Casco, Collar, Arma, Armadura, Escudo, Guantes, Cinturon, Anillo, Botas
    /// </summary>
    private string GetSlotTypeName()
    {
        switch (slotType)
        {
            case EquipmentManager.EquipmentSlotType.Montura:
                return "Montura";
            case EquipmentManager.EquipmentSlotType.Casco:
                return "Casco";
            case EquipmentManager.EquipmentSlotType.Collar:
                return "Collar";
            case EquipmentManager.EquipmentSlotType.Arma:
                return "Arma";
            case EquipmentManager.EquipmentSlotType.Armadura:
                return "Armadura";
            case EquipmentManager.EquipmentSlotType.Escudo:
                return "Escudo";
            case EquipmentManager.EquipmentSlotType.Guantes:
                return "Guantes";
            case EquipmentManager.EquipmentSlotType.Cinturon:
                return "Cinturón";
            case EquipmentManager.EquipmentSlotType.Anillo:
                return "Anillo";
            case EquipmentManager.EquipmentSlotType.Botas:
                return "Botas";
            default:
                return "Equipo";
        }
    }

    /// <summary>
    /// Muestra información del item en un tooltip (si está implementado).
    /// </summary>
    public void ShowTooltip()
    {
        if (currentItem != null && currentItem.IsValid())
        {
            ItemData baseItem = currentItem.baseItem;
            ItemStats stats = currentItem.GetFinalStats();
            // TODO: Implementar sistema de tooltip
            Debug.Log($"Item equipado: {currentItem.GetItemName()}\nNivel: {currentItem.currentLevel}\nDescripción: {baseItem.description}\nSlot: {GetSlotTypeName()}\nStats: Ataque={stats.ataque}, Defensa={stats.defensa}, HP={stats.hp}");
        }
    }
}

