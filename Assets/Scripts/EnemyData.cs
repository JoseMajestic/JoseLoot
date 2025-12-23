using UnityEngine;

/// <summary>
/// ScriptableObject que define las estadísticas de un enemigo.
/// Similar a ItemData pero para enemigos.
/// </summary>
[CreateAssetMenu(fileName = "New Enemy", menuName = "Combate/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Informacion Basica")]
    [Tooltip("Nombre del enemigo")]
    public string enemyName = "Nuevo Enemigo";
    
    [TextArea(2, 4)]
    [Tooltip("Descripción del enemigo")]
    public string description = "";
    
    [Tooltip("Imagen del enemigo")]
    public Sprite enemySprite;
    
    [Header("=== ESTADISTICAS ===")]
    [Tooltip("Puntos de vida")]
    public int hp = 100;
    
    [Tooltip("Poder de ataque")]
    public int ataque = 10;
    
    [Tooltip("Poder de defensa")]
    public int defensa = 5;
    
    [Tooltip("Velocidad de ataque")]
    public int velocidadAtaque = 10;
    
    [Tooltip("Probabilidad de golpe critico (1 punto = 1%)")]
    public int ataqueCritico = 0;
    
    [Tooltip("Multiplicador de dano critico")]
    public int danoCritico = 0;
    
    [Tooltip("Suerte del enemigo")]
    public int suerte = 0;
    
    [Tooltip("Destreza / Habilidad")]
    public int destreza = 0;
    
    [Header("=== RECOMPENSAS ===")]
    [Tooltip("Monedas que otorga al vencer")]
    public int rewardCoins = 50;
    
    [Tooltip("Experiencia que otorga al vencer")]
    public int experienceReward = 10;
    
    [Header("=== RECOMPENSAS DE OBJETOS ===")]
    [Tooltip("Configuración de recompensas de objetos para este enemigo")]
    public EnemyRewardConfig rewardConfig = new EnemyRewardConfig();
    
    [Header("=== PROGRESO ===")]
    [Tooltip("Nivel requerido para desbloquear (0 = siempre disponible)")]
    public int requiredLevel = 0;
    
    [Tooltip("Nivel del enemigo (opcional)")]
    public int level = 1;
    
    [Header("=== ATAQUES ===")]
    [Tooltip("Array de ataques disponibles para este enemigo. Se selecciona uno aleatoriamente cada turno. Si está vacío, usa ataque básico.")]
    public AttackData[] availableAttacks = new AttackData[0];
}



