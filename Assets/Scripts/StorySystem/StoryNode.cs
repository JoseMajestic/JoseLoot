using UnityEngine;

/// <summary>
/// Clase base abstracta para todos los nodos de historia
/// Cada pantalla = un nodo
/// </summary>
public abstract class StoryNode : ScriptableObject
{
    [Header("Contenido Visual")]
    public Sprite image;
    [TextArea(3, 5)] public string text;
    
    /// <summary>
    /// Método que se ejecuta al entrar en este nodo
    /// </summary>
    /// <param name="manager">Referencia al StoryManager</param>
    public abstract void Enter(StoryManager manager);
}

/// <summary>
/// Nodo de introducción (1 botón de continuar)
/// </summary>
[CreateAssetMenu(menuName = "Story/Nodes/Intro")]
public class IntroNode : StoryNode
{
    [Header("Siguiente Nodo")]
    public StoryNode nextNode;
    
    public override void Enter(StoryManager manager)
    {
        manager.storyUIPanel.Show(
            image,
            text,
            new StoryButton("Continuar", () => manager.GoToNode(nextNode))
        );
    }
}

/// <summary>
/// Nodo de decisión con 2 opciones (A/B)
/// </summary>
[CreateAssetMenu(menuName = "Story/Nodes/Decision")]
public class DecisionNode : StoryNode
{
    [Header("Opciones")]
    public string optionAText = "Opción A";
    public string optionBText = "Opción B";
    
    [Header("Nodos Destino")]
    public StoryNode optionANode;
    public StoryNode optionBNode;
    
    public override void Enter(StoryManager manager)
    {
        manager.storyUIPanel.Show(
            image,
            text,
            new StoryButton(optionAText, () => manager.GoToNode(optionANode)),
            new StoryButton(optionBText, () => manager.GoToNode(optionBNode))
        );
    }
}

/// <summary>
/// Nodo de combate - puente con el sistema de combate existente
/// </summary>
[CreateAssetMenu(menuName = "Story/Nodes/Combat")]
public class CombatNode : StoryNode
{
    [Header("Configuración de Combate")]
    public EnemyData enemy;
    
    [Header("Nodos de Transición")]
    public StoryNode onVictory;
    public StoryNode onDefeat;
    
    public override void Enter(StoryManager manager)
    {
        manager.StartCombat(enemy, this);
    }
}

/// <summary>
/// Nodo de fin de capítulo
/// </summary>
[CreateAssetMenu(menuName = "Story/Nodes/EndChapter")]
public class EndChapterNode : StoryNode
{
    public override void Enter(StoryManager manager)
    {
        manager.storyUIPanel.Show(
            image,
            text,
            new StoryButton("Cerrar capítulo", manager.EndChapter)
        );
    }
}

/// <summary>
/// Contenedor de un capítulo completo de historia
/// </summary>
[CreateAssetMenu(menuName = "Story/Chapter")]
public class StoryChapter : ScriptableObject
{
    [Header("Información del Capítulo")]
    public string chapterName;
    [TextArea(2, 3)] public string chapterDescription;
    
    [Header("Nodo Inicial")]
    public StoryNode entryNode;
}
