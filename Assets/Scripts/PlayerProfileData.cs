using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Datos del perfil del jugador que se guardan persistentemente.
/// Incluye los items equipados con su GUID y nivel.
/// </summary>
[System.Serializable]
public class PlayerProfileData
{
    [System.Serializable]
    public class EquippedItemData
    {
        public string slotType; // "Arma", "ArmaSecundaria", etc.
        public string itemInstanceId; // GUID del ItemInstance
        public int level; // Nivel actual del item
        public string itemName; // Nombre del ItemData para referencia
    }

    public List<EquippedItemData> equippedItems = new List<EquippedItemData>();

    // SOLUCIÓN: Datos del inventario completo (serializado)
    // Array de strings, cada string es "nombreItemData|nivel|id|version" o null si el slot está vacío
    public string[] inventoryData = new string[InventoryManager.INVENTORY_SIZE];

    // Sistema de niveles desbloqueados de enemigos
    [Tooltip("Lista de niveles de enemigos desbloqueados")]
    public List<int> unlockedEnemyLevels = new List<int>();
    
    // Sistema de enemigos derrotados (para desbloqueo secuencial)
    [Tooltip("Lista de nombres de enemigos derrotados (para desbloqueo secuencial)")]
    public List<string> defeatedEnemies = new List<string>();

    // Estadísticas del jugador
    [Tooltip("Total de enfrentamientos realizados")]
    public int totalClashes = 0;

    [Tooltip("Total de cofres abiertos")]
    public int totalOpenChests = 0;

    [Tooltip("Total de peleas ganadas")]
    public int totalWonFights = 0;

    [Tooltip("Total de peleas perdidas")]
    public int totalLostFights = 0;

    // Sistema de experiencia del héroe
    [Tooltip("Experiencia actual del héroe")]
    public int heroExperience = 1;

    [Tooltip("Nivel actual del héroe")]
    public int heroLevel = 1;

    // ===== SISTEMA DE CRIANZA/BREED =====
    
    // Stats de crianza (0-100)
    // NOTA: breedEnergy NO decae automáticamente, solo se descarga manualmente
    [Tooltip("Trabajo (0-100)")]
    public int breedWork = 0;
    
    [Tooltip("Hambre (0-100)")]
    public int breedHunger = 0;
    
    [Tooltip("Felicidad (0-100)")]
    public int breedHappiness = 0;
    
    [Tooltip("Energía (0-100) - NO decae automáticamente, solo se descarga manualmente")]
    public int breedEnergy = 0;
    
    [Tooltip("Higiene (0-100)")]
    public int breedHygiene = 0;
    
    [Tooltip("Disciplina (0-100)")]
    public int breedDiscipline = 0;

    // Sistema de energía
    [Tooltip("Energía actual del héroe (0-100)")]
    public int currentEnergy = 0;
    
    [Tooltip("Energía máxima del héroe")]
    public int maxEnergy = 100;
    
    [Tooltip("Si el héroe está durmiendo (recuperando energía)")]
    public bool isSleeping = false;
    
    [Tooltip("Última vez que se durmió (para calcular recuperación offline - DESACTIVADO POR AHORA)")]
    public string lastSleepTimeString = ""; // Serializado como string porque DateTime no es serializable directamente

    // Sistema de evolución
    [Tooltip("Clase de evolución (0 = Primera, 1 = Segunda, etc.)")]
    public int evolutionClass = 0;
    
    [Tooltip("Última vez que evolucionó (para cooldown)")]
    public string lastEvolutionTimeString = ""; // Serializado como string
    
    [Tooltip("Tiempo total de vida en segundos (solo cuenta cuando se juega)")]
    public float totalLifeTime = 0f;

    // Mejoras de gimnasio (niveles de cada stat)
    [Tooltip("Nivel de mejora de HP (máximo 999)")]
    public int gymHPLevel = 0;
    
    [Tooltip("Nivel de mejora de Mana (máximo 999)")]
    public int gymManaLevel = 0;
    
    [Tooltip("Nivel de mejora de Ataque (máximo 999)")]
    public int gymAttackLevel = 0;
    
    [Tooltip("Nivel de mejora de Defensa (máximo 999)")]
    public int gymDefenseLevel = 0;
    
    [Tooltip("Nivel de mejora de Skill (máximo 999)")]
    public int gymSkillLevel = 0;
    
    [Tooltip("Nivel de mejora de Critical Attack (máximo 100)")]
    public int gymCriticalAttackLevel = 0;
    
    [Tooltip("Nivel de mejora de Critical Damage (máximo 100)")]
    public int gymCriticalDamageLevel = 0;
    
    [Tooltip("Nivel de mejora de Attack Speed (máximo 100)")]
    public int gymAttackSpeedLevel = 0;
    
    [Tooltip("Nivel de mejora de Luck (máximo 100)")]
    public int gymLuckLevel = 0;

    // Título actual
    [Tooltip("Título actual del héroe")]
    public string currentTitle = "Viajero Digital";
    
    [Tooltip("Tipo de título (balanced, happy, worker, disciplined, athlete, scholar, neutral, neglected, abandoned, critical)")]
    public string titleType = "neutral";
    
    // Bonus total acumulado de cada stat (para mostrar en Upgrade)
    // Se calcula sumando todas las mejoras desde nivel 1 hasta nivel actual
    [Tooltip("Bonus total acumulado de HP")]
    public int gymHPTotalBonus = 0;
    
    [Tooltip("Bonus total acumulado de Mana")]
    public int gymManaTotalBonus = 0;
    
    [Tooltip("Bonus total acumulado de Ataque")]
    public int gymAttackTotalBonus = 0;
    
    [Tooltip("Bonus total acumulado de Defensa")]
    public int gymDefenseTotalBonus = 0;
    
    [Tooltip("Bonus total acumulado de Skill")]
    public int gymSkillTotalBonus = 0;
    
    [Tooltip("Bonus total acumulado de Critical Attack")]
    public int gymCriticalAttackTotalBonus = 0;
    
    [Tooltip("Bonus total acumulado de Critical Damage")]
    public int gymCriticalDamageTotalBonus = 0;
    
    [Tooltip("Bonus total acumulado de Attack Speed")]
    public int gymAttackSpeedTotalBonus = 0;
    
    [Tooltip("Bonus total acumulado de Luck")]
    public int gymLuckTotalBonus = 0;

    /// <summary>
    /// Guarda el estado actual del equipo en el perfil.
    /// </summary>
    public void SaveEquipmentState(EquipmentManager equipmentManager)
    {
        if (equipmentManager == null)
            return;

        equippedItems.Clear();

        // Guardar cada slot de equipo
        foreach (EquipmentManager.EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
        {
            ItemInstance item = equipmentManager.GetEquippedItem(slotType);
            if (item != null && item.IsValid())
            {
                EquippedItemData data = new EquippedItemData
                {
                    slotType = slotType.ToString(),
                    itemInstanceId = item.GetInstanceId(),
                    level = item.currentLevel,
                    itemName = item.GetItemName()
                };
                equippedItems.Add(data);
            }
        }
    }

    /// <summary>
    /// Obtiene el nivel guardado de un item equipado por su GUID.
    /// </summary>
    public int GetEquippedItemLevel(string instanceId)
    {
        foreach (var itemData in equippedItems)
        {
            if (itemData.itemInstanceId == instanceId)
            {
                return itemData.level;
            }
        }
        return -1; // No encontrado
    }

    /// <summary>
    /// Carga el estado del equipo desde el perfil.
    /// Busca los items en el inventario por su GUID y los equipa en los slots correspondientes.
    /// </summary>
    /// <param name="equipmentManager">El EquipmentManager a actualizar</param>
    /// <param name="inventoryManager">El InventoryManager para buscar items por GUID</param>
    /// <param name="itemDatabase">El ItemDatabase para deserializar items si es necesario</param>
    /// <param name="silent">Si es true, no dispara eventos de inventario (útil para carga inicial)</param>
    public void LoadEquipmentState(EquipmentManager equipmentManager, InventoryManager inventoryManager, ItemDatabase itemDatabase, bool silent = false)
    {
        if (equipmentManager == null || inventoryManager == null || itemDatabase == null)
            return;

        if (equippedItems == null || equippedItems.Count == 0)
            return;

        // Para cada item equipado guardado, buscarlo en el inventario y equiparlo
        foreach (var equippedItemData in equippedItems)
        {
            // Parsear el tipo de slot
            if (!System.Enum.TryParse<EquipmentManager.EquipmentSlotType>(equippedItemData.slotType, out EquipmentManager.EquipmentSlotType slotType))
            {
                Debug.LogWarning($"No se pudo parsear el tipo de slot: {equippedItemData.slotType}");
                continue;
            }

            // Buscar el item en el inventario por su GUID
            ItemInstance itemToEquip = null;
            for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
            {
                ItemInstance item = inventoryManager.GetItem(i);
                if (item != null && item.IsValid() && item.GetInstanceId() == equippedItemData.itemInstanceId)
                {
                    // Encontrado el item en el inventario
                    // Asegurar que el nivel esté actualizado
                    if (item.currentLevel != equippedItemData.level)
                    {
                        item.SetLevel(equippedItemData.level);
                    }
                    itemToEquip = item;
                    break;
                }
            }

            // Si no se encontró en el inventario, intentar deserializarlo desde los datos guardados
            // (esto puede pasar si el item fue eliminado del inventario pero estaba equipado)
            if (itemToEquip == null)
            {
                // Buscar en el inventario por nombre y nivel (fallback)
                // Esto es menos seguro pero puede ayudar en casos de migración de datos
                Debug.LogWarning($"Item equipado con GUID {equippedItemData.itemInstanceId} no encontrado en inventario. Intentando buscar por nombre...");
                
                for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
                {
                    ItemInstance item = inventoryManager.GetItem(i);
                    if (item != null && item.IsValid() && 
                        item.GetItemName() == equippedItemData.itemName && 
                        item.currentLevel == equippedItemData.level)
                    {
                        itemToEquip = item;
                        // Actualizar el GUID del item para que coincida con el guardado
                        // (esto puede pasar si el item fue deserializado sin el GUID correcto)
                        break;
                    }
                }
            }

            // Si encontramos el item, equiparlo
            if (itemToEquip != null && itemToEquip.IsValid())
            {
                // SOLUCIÓN CRÍTICA: NO remover el item del inventario cuando se carga la partida
                // Los items equipados deben permanecer en el inventario con el panel de "equipado" visible
                // Esto permite que el grid de forja muestre todos los items (equipados y no equipados)
                // El item estará tanto en el inventario como equipado lógicamente
                
                // SOLUCIÓN: Equipar el item sin removerlo del inventario y sin disparar eventos si estamos en modo silencioso
                // Usar SetEquippedItem directamente en lugar de EquipItem para evitar eventos
                if (silent)
                {
                    // Equipar directamente sin disparar eventos
                    // NO remover del inventario - el item debe permanecer visible
                    ItemInstance previousItem = equipmentManager.GetEquippedItem(slotType);
                    equipmentManager.SetEquippedItemDirectly(slotType, itemToEquip);
                }
                else
                {
                    // Equipar normalmente (dispara eventos)
                    // En modo normal, EquipItem() tampoco remueve el item del inventario
                    // (según el código actual, el item se mantiene en el inventario)
                    equipmentManager.EquipItem(itemToEquip, slotType);
                }
            }
            else
            {
                Debug.LogWarning($"No se pudo encontrar el item equipado '{equippedItemData.itemName}' (GUID: {equippedItemData.itemInstanceId}) en el inventario. El slot {equippedItemData.slotType} quedará vacío.");
            }
        }
    }

    /// <summary>
    /// Guarda el estado actual del inventario en el perfil.
    /// </summary>
    public void SaveInventoryState(InventoryManager inventoryManager)
    {
        if (inventoryManager == null)
            return;

        // SOLUCIÓN: Serializar el inventario completo usando el método existente
        string[] serialized = inventoryManager.SerializeInventory();
        inventoryData = serialized;
    }

    /// <summary>
    /// Carga el estado del inventario desde el perfil.
    /// </summary>
    /// <param name="inventoryManager">El InventoryManager a actualizar</param>
    /// <param name="silent">Si es true, no dispara eventos (útil para carga inicial)</param>
    public void LoadInventoryState(InventoryManager inventoryManager, bool silent = false)
    {
        if (inventoryManager == null || inventoryData == null)
            return;

        // SOLUCIÓN: Deserializar el inventario completo usando el método existente
        // Solo cargar si hay datos guardados (no es un perfil nuevo)
        if (inventoryData.Length == InventoryManager.INVENTORY_SIZE)
        {
            inventoryManager.DeserializeInventory(inventoryData, silent: silent);
        }
    }

    /// <summary>
    /// Desbloquea un nivel de enemigo.
    /// </summary>
    /// <param name="level">Nivel a desbloquear</param>
    public void UnlockEnemyLevel(int level)
    {
        if (unlockedEnemyLevels == null)
        {
            unlockedEnemyLevels = new List<int>();
        }

        if (!unlockedEnemyLevels.Contains(level))
        {
            unlockedEnemyLevels.Add(level);
            Debug.Log($"Nivel de enemigo {level} desbloqueado.");
        }
    }

    /// <summary>
    /// Verifica si un nivel de enemigo está desbloqueado.
    /// </summary>
    /// <param name="level">Nivel a verificar</param>
    /// <returns>True si el nivel está desbloqueado, false en caso contrario</returns>
    public bool IsEnemyLevelUnlocked(int level)
    {
        if (unlockedEnemyLevels == null)
        {
            unlockedEnemyLevels = new List<int>();
            return false;
        }

        // El nivel 0 siempre está desbloqueado (siempre disponible)
        if (level == 0)
        {
            return true;
        }

        return unlockedEnemyLevels.Contains(level);
    }

    /// <summary>
    /// Marca un enemigo como derrotado.
    /// </summary>
    /// <param name="enemyName">Nombre del enemigo derrotado</param>
    public void MarkEnemyDefeated(string enemyName)
    {
        if (defeatedEnemies == null)
        {
            defeatedEnemies = new List<string>();
        }
        
        if (!string.IsNullOrEmpty(enemyName) && !defeatedEnemies.Contains(enemyName))
        {
            defeatedEnemies.Add(enemyName);
            Debug.Log($"Enemigo '{enemyName}' marcado como derrotado.");
        }
    }

    /// <summary>
    /// Verifica si un enemigo ha sido derrotado.
    /// </summary>
    /// <param name="enemyName">Nombre del enemigo a verificar</param>
    /// <returns>True si el enemigo fue derrotado, false en caso contrario</returns>
    public bool IsEnemyDefeated(string enemyName)
    {
        if (defeatedEnemies == null || string.IsNullOrEmpty(enemyName))
            return false;
        
        return defeatedEnemies.Contains(enemyName);
    }

    /// <summary>
    /// Calcula la experiencia necesaria para el siguiente nivel.
    /// Fórmula: experienciaNecesaria = nivelActual * 100
    /// </summary>
    public int GetExperienceNeededForNextLevel()
    {
        return heroLevel * 100;
    }

    /// <summary>
    /// Agrega experiencia al héroe y sube de nivel si es necesario.
    /// </summary>
    public void AddHeroExperience(int experience)
    {
        heroExperience += experience;
        
        // Subir de nivel mientras haya suficiente experiencia
        while (heroExperience >= GetExperienceNeededForNextLevel())
        {
            heroExperience -= GetExperienceNeededForNextLevel();
            heroLevel++;
            Debug.Log($"¡El héroe subió al nivel {heroLevel}!");
        }
    }

    // ===== MÉTODOS DE SERIALIZACIÓN DE DATETIME =====
    
    /// <summary>
    /// Guarda la fecha/hora actual como string serializable.
    /// </summary>
    public void SaveLastSleepTime()
    {
        lastSleepTimeString = DateTime.Now.ToString("O"); // ISO 8601 format
    }
    
    /// <summary>
    /// Obtiene la última vez que durmió como DateTime.
    /// </summary>
    public DateTime GetLastSleepTime()
    {
        if (string.IsNullOrEmpty(lastSleepTimeString))
            return DateTime.MinValue; // Si no hay fecha guardada, nunca durmió
        
        if (DateTime.TryParse(lastSleepTimeString, out DateTime result))
            return result;
        
        return DateTime.MinValue; // Fallback: nunca durmió
    }
    
    /// <summary>
    /// Guarda la fecha/hora de la última evolución como string serializable.
    /// </summary>
    public void SaveLastEvolutionTime()
    {
        lastEvolutionTimeString = DateTime.Now.ToString("O"); // ISO 8601 format
    }
    
    /// <summary>
    /// Obtiene la última vez que evolucionó como DateTime.
    /// </summary>
    public DateTime GetLastEvolutionTime()
    {
        if (string.IsNullOrEmpty(lastEvolutionTimeString))
            return DateTime.MinValue; // Si no hay fecha guardada, nunca evolucionó
        
        if (DateTime.TryParse(lastEvolutionTimeString, out DateTime result))
            return result;
        
        return DateTime.MinValue; // Fallback
    }

    // ===== MÉTODOS DE RESET =====
    
    /// <summary>
    /// Resetea todos los datos de crianza (stats, tiempo de vida, clase, mejoras de gimnasio).
    /// NO resetea: tiempo total jugado, inventario, nivel del héroe.
    /// </summary>
    public void ResetBreedData()
    {
        // Resetear stats de crianza
        breedWork = 0;
        breedHunger = 0;
        breedHappiness = 0;
        breedEnergy = 0;
        breedHygiene = 0;
        breedDiscipline = 0;
        
        // Resetear energía
        currentEnergy = 0; // Energía empieza en 0 después del reset
        isSleeping = false; // Despertar al resetear
        
        // Resetear tiempo de vida
        totalLifeTime = 0f;
        
        // Resetear clase de evolución
        evolutionClass = 0;
        lastEvolutionTimeString = "";
        
        // Resetear mejoras de gimnasio
        gymHPLevel = 0;
        gymManaLevel = 0;
        gymAttackLevel = 0;
        gymDefenseLevel = 0;
        gymSkillLevel = 0;
        gymCriticalAttackLevel = 0;
        gymCriticalDamageLevel = 0;
        gymAttackSpeedLevel = 0;
        gymLuckLevel = 0;
        
        // Resetear bonus totales
        gymHPTotalBonus = 0;
        gymManaTotalBonus = 0;
        gymAttackTotalBonus = 0;
        gymDefenseTotalBonus = 0;
        gymSkillTotalBonus = 0;
        gymCriticalAttackTotalBonus = 0;
        gymCriticalDamageTotalBonus = 0;
        gymAttackSpeedTotalBonus = 0;
        gymLuckTotalBonus = 0;
        
        // Resetear título
        currentTitle = "Viajero Digital";
        titleType = "neutral";
        
        Debug.Log("Datos de crianza reseteados completamente.");
    }
}

