using UnityEngine;

[CreateAssetMenu(menuName = "StoryPanel/Nodes/Chapter End")]
public class StoryPanelChapterEndNode : StoryPanelNode
{
    [Header("Texto del botón")]
    public string acceptText = "Aceptar";

    public override void Enter(StoryPanelManager manager)
    {
        manager.ShowChapterEndNode(this);
    }
}
