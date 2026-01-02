using UnityEngine;

[CreateAssetMenu(menuName = "StoryPanel/Nodes/Transition")]
public class StoryPanelTransitionNode : StoryPanelNode
{
    [Header("Título del nodo")]
    [Tooltip("Título que se mostrará en las diapositivas de transición.")]
    public string title;

    [Header("Opciones")]
    public string optionAText = "Continuar";
    public StoryPanelNode optionANode;

    [Header("(Opcional) Segunda opción")]
    public bool hasOptionB = false;
    public string optionBText = "Opción B";
    public StoryPanelNode optionBNode;

    public override void Enter(StoryPanelManager manager)
    {
        manager.ShowTransitionNode(this);
    }
}
