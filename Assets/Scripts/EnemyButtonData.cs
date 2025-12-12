using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase serializable auxiliar para asociar botones con instancias de EnemyData.
/// Permite que en el Inspector de BattleManager se pueda arrastrar tanto el botón como la instancia EnemyData en cada posición del array.
/// </summary>
[System.Serializable]
public class EnemyButtonData
{
    [Tooltip("Botón que activa este enemigo")]
    public Button button;
    
    [Tooltip("Instancia EnemyData asociada a este botón")]
    public EnemyData enemyData;
}





