using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase serializable auxiliar para asociar botones con instancias de AttackData.
/// Permite que en el Inspector de CombatManager se pueda arrastrar tanto el botón como la instancia AttackData en cada posición del array.
/// </summary>
[System.Serializable]
public class AttackButtonData
{
    [Tooltip("Botón que activa este ataque")]
    public Button button;
    
    [Tooltip("Instancia AttackData asociada a este botón")]
    public AttackData attackData;
}



