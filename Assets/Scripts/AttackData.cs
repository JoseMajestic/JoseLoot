using UnityEngine;

/// <summary>
/// ScriptableObject que define un tipo de ataque con sus propiedades y efectos especiales.
/// </summary>
[CreateAssetMenu(fileName = "New Attack", menuName = "Combate/Attack")]
public class AttackData : ScriptableObject
{
    [Header("Informacion Basica")]
    [Tooltip("Nombre del ataque")]
    public string attackName = "Nuevo Ataque";
    
    [TextArea(2, 4)]
    [Tooltip("Descripción del ataque")]
    public string description = "";
    
    [Header("=== ESTADISTICAS ===")]
    [Tooltip("Daño base del ataque")]
    public int baseDamage = 10;
    
    [Tooltip("Bonificación de habilidad")]
    public int skillBonus = 0;
    
    [Header("=== EFECTOS ESPECIALES ===")]
    [Tooltip("Tipo de efecto especial")]
    public AttackEffectType effectType = AttackEffectType.Normal;
    
    [Tooltip("Valor del efecto (porcentaje para Heal/Poison/Buff, multiplicador para StrongBlow, número de golpes para MultipleAttack)")]
    public int effectValue = 0;
    
    [Tooltip("Duración en rondas (solo para Buffs: 3, 4, 5 rondas)")]
    public int duration = 0;
    
    [Header("=== PROGRESO ===")]
    [Tooltip("Nivel del héroe requerido para desbloquear este ataque (0 = siempre disponible)")]
    public int requiredHeroLevel = 0;
    
    [Tooltip("Precio en monedas para comprar/desbloquear este ataque en la biblioteca")]
    public int unlockPrice = 100;
}

/// <summary>
/// Enum que define los tipos de efectos especiales que puede tener un ataque.
/// </summary>
public enum AttackEffectType
{
    Normal,         // Sin efecto especial (solo este aplica suerte)
    Heal,           // Cura HP (effectValue = porcentaje: 25, 50, 75, 100)
    Poison,         // Veneno (effectValue = porcentaje: 10, 15, 20)
    Stun,           // Aturde (50% + suerte% probabilidad)
    MultipleAttack, // Ataque múltiple (effectValue = número de golpes: 2, 3, 4)
    StrongBlow,     // Golpe fuerte (effectValue = multiplicador: 2, 3, 4)
    AttackBuff,     // Buff de ataque (effectValue = porcentaje, duration = rondas)
    DefenseBuff     // Buff de defensa (effectValue = porcentaje, duration = rondas)
}



