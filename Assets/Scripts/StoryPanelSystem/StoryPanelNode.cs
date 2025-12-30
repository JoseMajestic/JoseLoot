using UnityEngine;

public abstract class StoryPanelNode : ScriptableObject
{
    [Header("Contenido Visual")]
    public Sprite image;

    [TextArea(3, 8)]
    public string text;

    [Header("Recompensas opcionales al entrar en este nodo")]
    [Tooltip("Monedas añadidas a la recompensa final al pasar por este nodo")]
    public int nodeRewardCoins = 0;

    [Tooltip("Objetos añadidos a la recompensa final al pasar por este nodo")]
    public ItemData[] nodeRewardItems = new ItemData[0];

    public abstract void Enter(StoryPanelManager manager);
}
