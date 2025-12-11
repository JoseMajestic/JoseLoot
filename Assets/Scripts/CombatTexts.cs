using UnityEngine;

/// <summary>
/// ScriptableObject que contiene todos los textos configurables del sistema de combate.
/// Permite editar todos los mensajes desde el Inspector de Unity.
/// </summary>
[CreateAssetMenu(fileName = "Combat Texts", menuName = "Combate/Combat Texts")]
public class CombatTexts : ScriptableObject
{
    [Header("=== SELECCIÓN DE ATAQUE ===")]
    [TextArea(2, 3)]
    [Tooltip("Texto cuando el jugador debe seleccionar un ataque")]
    public string selectAttack = "Selecciona un ataque";
    
    [TextArea(2, 3)]
    [Tooltip("Texto cuando se selecciona un ataque. {0} = nombre del ataque, {1} = descripción")]
    public string attackSelected = "Ataque seleccionado: {0}\n{1}";
    
    [Header("=== ATAQUES DEL JUGADOR ===")]
    [TextArea(2, 3)]
    [Tooltip("Texto del ataque básico del jugador. {0} = daño")]
    public string playerBasicAttack = "Jugador usa ataque básico y causa {0} de daño.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto del ataque del jugador. {0} = nombre del ataque, {1} = daño")]
    public string playerAttack = "Jugador usa {0} y causa {1} de daño.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de curación del jugador. {0} = nombre del ataque, {1} = HP curado")]
    public string playerHeal = "Jugador usa {0} y se cura {1} de HP.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto cuando se activa la suerte del jugador")]
    public string playerLuck = "¡Suerte activada! El jugador ataca de nuevo.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto del ataque de suerte del jugador. {0} = daño")]
    public string playerLuckAttack = "Jugador ataca de nuevo y causa {0} de daño.";
    
    [Header("=== ATAQUES DEL ENEMIGO ===")]
    [TextArea(2, 3)]
    [Tooltip("Texto del ataque básico del enemigo. {0} = nombre del enemigo, {1} = daño")]
    public string enemyBasicAttack = "{0} usa ataque básico y causa {1} de daño.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto del ataque del enemigo. {0} = nombre del enemigo, {1} = nombre del ataque, {2} = daño")]
    public string enemyAttack = "{0} usa {1} y causa {2} de daño.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de curación del enemigo. {0} = nombre del enemigo, {1} = nombre del ataque, {2} = HP curado")]
    public string enemyHeal = "{0} usa {1} y se cura {2} de HP.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto cuando se activa la suerte del enemigo. {0} = nombre del enemigo")]
    public string enemyLuck = "¡Suerte activada! {0} ataca de nuevo.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto del ataque de suerte del enemigo. {0} = nombre del enemigo, {1} = nombre del ataque, {2} = daño")]
    public string enemyLuckAttack = "{0} usa {1} de nuevo y causa {2} de daño.";
    
    [Header("=== EFECTOS ESPECIALES ===")]
    [TextArea(2, 3)]
    [Tooltip("Texto de veneno en el enemigo. {0} = nombre del enemigo, {1} = daño de veneno")]
    public string poisonEnemy = "¡Veneno! {0} pierde {1} de HP.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de veneno en el jugador. {0} = daño de veneno")]
    public string poisonPlayer = "¡Veneno! El jugador pierde {0} de HP.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de veneno aplicado al enemigo. {0} = nombre del enemigo, {1} = porcentaje")]
    public string poisonAppliedEnemy = "{0} está envenenado ({1}% de HP por ronda).";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de veneno aplicado al jugador. {0} = porcentaje")]
    public string poisonAppliedPlayer = "El jugador está envenenado ({0}% de HP por ronda).";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de stun exitoso en el enemigo. {0} = nombre del enemigo")]
    public string stunEnemy = "{0} está aturdido y perderá su siguiente turno.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de stun exitoso en el jugador")]
    public string stunPlayer = "El jugador está aturdido y perderá su siguiente turno.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto cuando el stun falla. {0} = nombre del ataque, {1} = objetivo")]
    public string stunFailed = "{0} falló. {1} resistió el aturdimiento.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de ataque múltiple. {0} = atacante, {1} = número de golpes, {2} = nombre del ataque")]
    public string multipleAttack = "{0} ejecutó {1} golpes con {2}.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de golpe fuerte. {0} = atacante, {1} = nombre del ataque, {2} = multiplicador, {3} = daño")]
    public string strongBlow = "{0} usa {1} ({2}x) y causa {3} de daño.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de buff de ataque. {0} = atacante, {1} = nombre del ataque, {2} = porcentaje, {3} = rondas")]
    public string attackBuff = "{0} usa {1}. Ataque aumentado {2}% por {3} rondas.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de buff de defensa. {0} = atacante, {1} = nombre del ataque, {2} = porcentaje, {3} = rondas")]
    public string defenseBuff = "{0} usa {1}. Defensa aumentada {2}% por {3} rondas.";
    
    [Header("=== ESTADOS ===")]
    [TextArea(2, 3)]
    [Tooltip("Texto cuando el jugador está aturdido")]
    public string playerStunned = "El jugador está aturdido y no puede atacar.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto cuando el enemigo está aturdido. {0} = nombre del enemigo")]
    public string enemyStunned = "{0} está aturdido y no puede atacar.";
    
    [TextArea(2, 3)]
    [Tooltip("Texto de siguiente ronda")]
    public string nextRound = "Siguiente ronda";
    
    [Header("=== CONFIGURACIÓN ===")]
    [Tooltip("Velocidad de escritura (caracteres por segundo)")]
    [Range(10f, 100f)]
    public float typewriterSpeed = 30f;
    
    [Tooltip("Delay entre frases (segundos)")]
    [Range(0.5f, 5f)]
    public float delayBetweenMessages = 1.5f;
}

