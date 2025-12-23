using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona las recompensas de combate incluyendo monedas y objetos.
/// Se integra con CombatManager para añadir recompensas de objetos al sistema de tiers.
/// </summary>
public class CombatRewardManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Base de datos de tiers para recompensas")]
    [SerializeField] private RewardTierDatabase rewardTierDatabase;
    
    [Tooltip("Referencia al InventoryManager para añadir objetos")]
    [SerializeField] private InventoryManager inventoryManager;
    
    [Tooltip("Referencia al GameDataManager para guardar progreso")]
    [SerializeField] private GameDataManager gameDataManager;
    
    [Header("UI de Recompensas")]
    [Tooltip("Panel que muestra los objetos obtenidos (opcional)")]
    [SerializeField] private GameObject rewardPanel;
    
    [Tooltip("Contenedor para los slots de objetos obtenidos")]
    [SerializeField] private Transform rewardSlotsContainer;
    
    [Tooltip("Prefab para mostrar un objeto obtenido")]
    [SerializeField] private GameObject rewardSlotPrefab;
    
    [Header("Configuración")]
    [Tooltip("Si es true, muestra los objetos obtenidos en un panel antes de añadirlos al inventario")]
    [SerializeField] private bool showRewardPanel = true;
    
    [Tooltip("Tiempo que se muestra el panel de recompensas (segundos)")]
    [SerializeField] private float rewardPanelDisplayTime = 3f;
    
    // Eventos
    public System.Action<List<ItemInstance>> OnRewardsGenerated;
    public System.Action OnRewardsClaimed;
    
    /// <summary>
    /// Procesa las recompensas de combate para un enemigo vencido.
    /// </summary>
    /// <param name="enemy">Enemigo vencido</param>
    /// <param name="coinsToAward">Monedas a añadir (ya procesadas por CombatManager)</param>
    public void ProcessCombatRewards(EnemyData enemy, int coinsToAward)
    {
        if (enemy == null)
        {
            Debug.LogWarning("CombatRewardManager: Enemigo nulo");
            return;
        }
        
        // Generar recompensas de objetos
        List<ItemData> itemRewards = GenerateItemRewards(enemy);
        
        // Convertir a ItemInstance
        List<ItemInstance> itemInstances = new List<ItemInstance>();
        foreach (var itemData in itemRewards)
        {
            itemInstances.Add(new ItemInstance(itemData));
        }
        
        // Disparar evento
        OnRewardsGenerated?.Invoke(itemInstances);
        
        // Mostrar panel o añadir directamente
        Debug.Log($"CombatRewardManager: showRewardPanel = {showRewardPanel}, itemInstances.Count = {itemInstances.Count}");
        
        if (showRewardPanel && itemInstances.Count > 0)
        {
            Debug.Log("CombatRewardManager: Iniciando ShowRewardPanelCoroutine");
            StartCoroutine(ShowRewardPanelCoroutine(itemInstances));
        }
        else
        {
            Debug.Log("CombatRewardManager: Añadiendo directamente al inventario");
            AddRewardsToInventory(itemInstances);
        }
    }
    
    /// <summary>
    /// Genera las recompensas de objetos basadas en la configuración del enemigo.
    /// </summary>
    private List<ItemData> GenerateItemRewards(EnemyData enemy)
    {
        List<ItemData> rewards = new List<ItemData>();
        
        if (rewardTierDatabase == null)
        {
            Debug.LogWarning("CombatRewardManager: RewardTierDatabase no asignado");
            return rewards;
        }
        
        if (enemy.rewardConfig == null)
        {
            Debug.LogWarning($"CombatRewardManager: El enemigo {enemy.enemyName} no tiene configuración de recompensas");
            return rewards;
        }
        
        // Validar configuración
        if (!enemy.rewardConfig.Validate())
        {
            return rewards;
        }
        
        // Generar recompensas desde la base de datos de tiers
        rewards = rewardTierDatabase.GenerateRewards(enemy.rewardConfig);
        
        // Añadir recompensas forzadas si hay
        if (enemy.rewardConfig.forcedRewards != null)
        {
            foreach (var forcedReward in enemy.rewardConfig.forcedRewards)
            {
                if (forcedReward != null)
                    rewards.Add(forcedReward);
            }
        }
        
        Debug.Log($"Generadas {rewards.Count} recompensas para {enemy.enemyName}: {enemy.rewardConfig.GetDebugInfo()}");
        
        return rewards;
    }
    
    /// <summary>
    /// Muestra el panel de recompensas y luego añade los objetos al inventario.
    /// </summary>
    private System.Collections.IEnumerator ShowRewardPanelCoroutine(List<ItemInstance> rewards)
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
            
            // Limpiar slots anteriores
            Debug.Log($"CombatRewardManager: Limpiando y creando slots, rewards.Count = {rewards.Count}");
            
            if (rewardSlotsContainer != null)
            {
                foreach (Transform child in rewardSlotsContainer)
                {
                    Destroy(child.gameObject);
                }
                
                // Esperar un frame para asegurar que el panel esté estable
                yield return null;
                
                // Crear slots para los nuevos objetos
                foreach (var reward in rewards)
                {
                    Debug.Log($"CombatRewardManager: Creando slot para reward: {reward.GetItemName()}");
                    
                    if (rewardSlotPrefab != null)
                    {
                        GameObject slotObj = Instantiate(rewardSlotPrefab, rewardSlotsContainer);
                        
                        // Esperar un frame antes de configurar
                        yield return null;
                        
                        // Configurar el slot (necesitarías un RewardSlot component)
                        RewardSlot slot = slotObj.GetComponent<RewardSlot>();
                        if (slot != null)
                        {
                            Debug.Log("CombatRewardManager: Llamando a slot.Setup()");
                            slot.Setup(reward);
                        }
                        else
                        {
                            Debug.LogError("CombatRewardManager: No se encontró componente RewardSlot en el prefab");
                        }
                    }
                    else
                    {
                        Debug.LogError("CombatRewardManager: rewardSlotPrefab es null");
                    }
                }
            }
            else
            {
                Debug.LogError("CombatRewardManager: rewardSlotsContainer es null");
            }
            
            // Esperar el tiempo configurado
            yield return new WaitForSeconds(rewardPanelDisplayTime);
            
            // Ocultar panel
            rewardPanel.SetActive(false);
        }
        
        // Añadir objetos al inventario
        AddRewardsToInventory(rewards);
    }
    
    /// <summary>
    /// Añade las recompensas directamente al inventario.
    /// </summary>
    private void AddRewardsToInventory(List<ItemInstance> rewards)
    {
        if (inventoryManager == null)
        {
            Debug.LogError("CombatRewardManager: InventoryManager no asignado");
            return;
        }
        
        int addedCount = 0;
        List<ItemInstance> failedToAdd = new List<ItemInstance>();
        
        foreach (var reward in rewards)
        {
            if (reward == null || !reward.IsValid())
                continue;
                
            int slotIndex = inventoryManager.AddItem(reward);
            if (slotIndex >= 0)
            {
                addedCount++;
                Debug.Log($"Objeto añadido al inventario: {reward.GetItemName()}");
            }
            else
            {
                failedToAdd.Add(reward);
                Debug.LogWarning($"No se pudo añadir objeto al inventario: {reward.GetItemName()}");
            }
        }
        
        // Guardar progreso si se añadieron objetos
        if (addedCount > 0 && gameDataManager != null)
        {
            gameDataManager.SavePlayerProfile();
        }
        
        // Mostrar mensaje sobre objetos que no se pudieron añadir
        if (failedToAdd.Count > 0)
        {
            Debug.LogWarning($"{failedToAdd.Count} objetos no se pudieron añadir por falta de espacio en el inventario");
        }
        
        OnRewardsClaimed?.Invoke();
    }
    
    /// <summary>
    /// Valida que todas las referencias estén configuradas.
    /// </summary>
    public bool ValidateSetup()
    {
        bool isValid = true;
        
        if (rewardTierDatabase == null)
        {
            Debug.LogError("CombatRewardManager: RewardTierDatabase no asignado");
            isValid = false;
        }
        
        if (inventoryManager == null)
        {
            Debug.LogError("CombatRewardManager: InventoryManager no asignado");
            isValid = false;
        }
        
        if (showRewardPanel && rewardPanel == null)
        {
            Debug.LogWarning("CombatRewardManager: showRewardPanel está activado pero rewardPanel no asignado");
        }
        
        if (showRewardPanel && rewardSlotsContainer == null)
        {
            Debug.LogWarning("CombatRewardManager: showRewardPanel está activado pero rewardSlotsContainer no asignado");
        }
        
        if (showRewardPanel && rewardSlotPrefab == null)
        {
            Debug.LogWarning("CombatRewardManager: showRewardPanel está activado pero rewardSlotPrefab no asignado");
        }
        
        return isValid;
    }
    
    private void Start()
    {
        ValidateSetup();
    }
}
