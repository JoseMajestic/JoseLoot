using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventario/Item")]
public class ItemData : ScriptableObject
{
    [Header("Informacion Basica")]
    public string itemName = "Nuevo Item";
    public Sprite itemSprite;
    [TextArea(2, 4)]
    public string description = "";
    
    [Header("Precio")]
    [Tooltip("Precio de compra/venta del item")]
    public int price = 100;
    
    [Header("=== ESTADISTICAS ===")]
    [Tooltip("Puntos de vida que otorga")]
    public int hp = 0;
    
    [Tooltip("Puntos de mana que otorga")]
    public int mana = 0;
    
    [Tooltip("Poder de ataque")]
    public int ataque = 0;
    
    [Tooltip("Poder de defensa")]
    public int defensa = 0;
    
    [Tooltip("Velocidad de ataque")]
    public int velocidadAtaque = 0;
    
    [Tooltip("Probabilidad de golpe critico (%)")]
    public int ataqueCritico = 0;
    
    [Tooltip("Multiplicador de dano critico (%)")]
    public int danoCritico = 0;
    
    [Tooltip("Suerte para drops y eventos")]
    public int suerte = 0;
    
    [Tooltip("Destreza / Habilidad")]
    public int destreza = 0;
    
    [Tooltip("Nivel requerido o nivel del item")]
    public int nivel = 1;
    
    [Header("=== Rareza ===")]
    [Tooltip("Rareza del item (ej: Comun, Raro, Epico, Legendario)")]
    public string rareza = "Comun";
    
    [Header("=== Tipo de Item ===")]
    public ItemType itemType = ItemType.Arma;
}

public enum ItemType
{
    // Tipos específicos de equipo (todos equipables)
    Montura,
    Casco,
    Collar,
    Arma,
    Armadura,
    Escudo,
    Guantes,
    Cinturon,
    Anillo,
    Botas,
    // No equipables
    Otros
}
