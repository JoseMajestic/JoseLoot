using UnityEngine;

[CreateAssetMenu(menuName = "StoryPanel/Nodes/Intro")]
public class StoryPanelIntroNode : StoryPanelNode
{
    [Header("Texto del botón")]
    public string acceptText = "Continuar";

    [Header("Siguiente nodo")]
    public StoryPanelNode nextNode;

    public override void Enter(StoryPanelManager manager)
    {
        manager.ShowIntroNode(this);
    }
}
