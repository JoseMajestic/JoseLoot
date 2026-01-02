using UnityEngine;

[CreateAssetMenu(menuName = "StoryPanel/Nodes/Intro")]
public class StoryPanelIntroNode : StoryPanelNode
{
    [Header("Título del nodo")]
    [Tooltip("Texto que se mostrará como título en la diapositiva de intro.")]
    public string title;

    [Header("Texto del botón")]
    public string acceptText = "Continuar";

    [Header("Siguiente nodo")]
    public StoryPanelNode nextNode;

    public override void Enter(StoryPanelManager manager)
    {
        manager.ShowIntroNode(this);
    }
}
