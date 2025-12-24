using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Orquestador principal del sistema de historia
/// Controla capítulos, navegación y estados
/// </summary>
public class StoryManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] public UIStoryPanel storyUIPanel;
    [SerializeField] private GameObject storyPanel; // Panel principal como Gym
    
    [Header("Referencias de Combate")]
    [SerializeField] private CombatManager combatManager;
    
    [Header("Capítulos")]
    [SerializeField] private List<StoryChapter> chapters;
    
    [Header("Estado Actual")]
    [SerializeField] private StoryChapter currentChapter;
    [SerializeField] private StoryNode currentNode;
    
    // Variables internas
    private CombatNode currentCombatNode;
    
    /// <summary>
    /// Inicia un capítulo específico
    /// </summary>
    public void StartChapter(StoryChapter chapter)
    {
        if (chapter == null)
        {
            Debug.LogError("StoryManager: Capítulo nulo");
            return;
        }
        
        currentChapter = chapter;
        
        if (chapter.entryNode != null)
        {
            GoToNode(chapter.entryNode);
        }
        else
        {
            Debug.LogError($"StoryManager: El capítulo {chapter.chapterName} no tiene nodo inicial");
        }
    }
    
    /// <summary>
    /// Navega a un nodo específico
    /// </summary>
    public void GoToNode(StoryNode node)
    {
        if (node == null)
        {
            Debug.LogError("StoryManager: Nodo nulo");
            return;
        }
        
        currentNode = node;
        node.Enter(this);
    }
    
    /// <summary>
    /// Inicia un combate usando el sistema existente
    /// </summary>
    public void StartCombat(EnemyData enemy, CombatNode combatNode)
    {
        if (combatManager == null)
        {
            Debug.LogError("StoryManager: CombatManager no asignado");
            return;
        }
        
        if (enemy == null)
        {
            Debug.LogError("StoryManager: EnemyData nulo");
            return;
        }
        
        currentCombatNode = combatNode;
        
        // Ocultar panel de historia durante el combate
        storyUIPanel.Hide();
        
        // Iniciar combate con callbacks
        combatManager.StartCombat(
            enemy,
            onVictory: OnCombatVictory,
            onDefeat: OnCombatDefeat
        );
    }
    
    /// <summary>
    /// Callback cuando el jugador gana el combate
    /// </summary>
    private void OnCombatVictory()
    {
        if (currentCombatNode != null && currentCombatNode.onVictory != null)
        {
            GoToNode(currentCombatNode.onVictory);
        }
        else
        {
            Debug.LogWarning("StoryManager: No hay nodo de victoria configurado");
            EndChapter();
        }
        
        currentCombatNode = null;
    }
    
    /// <summary>
    /// Callback cuando el jugador pierde el combate
    /// </summary>
    private void OnCombatDefeat()
    {
        if (currentCombatNode != null && currentCombatNode.onDefeat != null)
        {
            GoToNode(currentCombatNode.onDefeat);
        }
        else
        {
            Debug.LogWarning("StoryManager: No hay nodo de derrota configurado");
            EndChapter();
        }
        
        currentCombatNode = null;
    }
    
    /// <summary>
    /// Finaliza el capítulo actual
    /// </summary>
    public void EndChapter()
    {
        storyUIPanel.Hide();
        
        // Aquí puedes añadir lógica para:
        // - Guardar progreso
        // - Desbloquear siguiente capítulo
        // - Volver al menú principal
        
        Debug.Log($"StoryManager: Capítulo '{currentChapter?.chapterName}' finalizado");
        
        currentChapter = null;
        currentNode = null;
        
        // Opcional: volver al menú principal
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Muestra el panel de historia
    /// </summary>
    public void ShowStoryPanel()
    {
        if (storyPanel != null)
        {
            storyPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Oculta el panel de historia
    /// </summary>
    public void HideStoryPanel()
    {
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Obtiene el capítulo actual
    /// </summary>
    public StoryChapter GetCurrentChapter()
    {
        return currentChapter;
    }
    
    /// <summary>
    /// Obtiene el nodo actual
    /// </summary>
    public StoryNode GetCurrentNode()
    {
        return currentNode;
    }
    
    private void Start()
    {
        // Validar referencias
        if (storyUIPanel == null)
            Debug.LogError("StoryManager: storyUIPanel no asignado");
        
        if (combatManager == null)
            Debug.LogError("StoryManager: combatManager no asignado");
        
        // Empezar con el panel oculto
        HideStoryPanel();
    }
}
