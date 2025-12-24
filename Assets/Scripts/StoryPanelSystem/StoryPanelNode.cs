using UnityEngine;

public abstract class StoryPanelNode : ScriptableObject
{
    [Header("Contenido Visual")]
    public Sprite image;

    [TextArea(3, 8)]
    public string text;

    public abstract void Enter(StoryPanelManager manager);
}
