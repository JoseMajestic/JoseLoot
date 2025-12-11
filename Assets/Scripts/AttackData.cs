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
    
    [Tooltip("Valor del efecto (si aplica)")]
    public int effectValue = 0;
}

/// <summary>
/// Enum que define los tipos de efectos especiales que puede tener un ataque.
/// </summary>
public enum AttackEffectType
{
    Normal,  // Sin efecto especial
    Heal,    // Cura HP
    Poison,  // Veneno (daño por turno)
    Stun,    // Aturde (salta turno)
    Buff     // Mejora temporal
}

