using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente para los botones de slots dentro de HeroProfile.
/// Detecta presiones prolongadas (pointer down/up) y notifica al HeroProfileManager.
/// </summary>
public class HeroProfileEquipmentSlotButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Tooltip("Slot de equipo asociado a este botón")]
    [SerializeField] private EquipmentManager.EquipmentSlotType slotType;

    private HeroProfileManager heroProfileManager;

    public EquipmentManager.EquipmentSlotType SlotType => slotType;
    
    public void SetHeroProfileManager(HeroProfileManager manager)
    {
        heroProfileManager = manager;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (heroProfileManager == null)
            return;

        heroProfileManager.OnEquipmentSlotPointerDown(slotType, this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        NotifyPointerUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        NotifyPointerUp();
    }

    private void NotifyPointerUp()
    {
        if (heroProfileManager == null)
            return;

        heroProfileManager.OnEquipmentSlotPointerUp(this);
    }
}
