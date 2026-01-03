using UnityEngine;

/// <summary>
/// Gestor central de datos del juego.
/// Singleton que nunca se desactiva y mantiene referencias a todos los managers.
/// Asegura persistencia de datos aunque los paneles UI se desactiven.
/// </summary>
public class GameDataManager : MonoBehaviour
{
    private static GameDataManager instance;

    [Header("Referencias a Managers")]
    [Tooltip("Referencia al InventoryManager")]
    [SerializeField] private InventoryManager inventoryManager;

    [Tooltip("Referencia al EquipmentManager")]
    [SerializeField] private EquipmentManager equipmentManager;

    [Tooltip("Referencia al ItemImprovementSystem")]
    [SerializeField] private ItemImprovementSystem itemImprovementSystem;

    [Tooltip("Referencia al PlayerMoney")]
    [SerializeField] private PlayerMoney playerMoney;

    [Tooltip("Referencia al ShopService")]
    [SerializeField] private ShopService shopService;

    [Tooltip("Referencia al ItemDatabase")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Tooltip("Referencia al PanelNavigationManager")]
    [SerializeField] private PanelNavigationManager panelNavigationManager;

    [Tooltip("Referencia al ChestManager")]
    [SerializeField] private ChestManager chestManager;

    [Tooltip("Referencia al InventoryAutoOrganizer")]
    [SerializeField] private InventoryAutoOrganizer inventoryAutoOrganizer;
    
    [Header("Servicios de Progresión")]
    [Tooltip("Servicio que aplica las bonificaciones irregulares por nivel del héroe")]
    [SerializeField] private HeroProgressionService heroProgressionService;

    [Header("Sistema de Guardado")]
    [Tooltip("Datos del perfil del jugador")]
    private PlayerProfileData playerProfile = new PlayerProfileData();

    private const string PLAYER_PROFILE_KEY = "PlayerProfileData";

    // Propiedades públicas para acceso desde otros scripts
    public static GameDataManager Instance 
    { 
        get 
        {
            // Si instance es null, intentar encontrarlo (útil si se accede antes de Awake)
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameDataManager>();
                if (instance == null)
                {
                    Debug.LogError("GameDataManager: No se encontró instancia en la escena. Asegúrate de que existe un GameObject con GameDataManager.");
                }
            }
            return instance;
        }
    }
    public InventoryManager InventoryManager => inventoryManager;
    public EquipmentManager EquipmentManager => equipmentManager;
    public ItemImprovementSystem ItemImprovementSystem => itemImprovementSystem;
    public PlayerMoney PlayerMoney => playerMoney;
    public ShopService ShopService => shopService;
    public ItemDatabase ItemDatabase => itemDatabase;
    public PanelNavigationManager PanelNavigationManager => panelNavigationManager;
    public ChestManager ChestManager => chestManager;
    public InventoryAutoOrganizer InventoryAutoOrganizer => inventoryAutoOrganizer;

    private void Awake()
    {
        // Implementar patrón Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Mantener el objeto entre escenas
        }
        else if (instance != this)
        {
            Debug.LogWarning("Ya existe una instancia de GameDataManager. Destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }

        // Validar referencias críticas
        ValidateReferences();
        
        // SOLUCIÓN: Cargar perfil del jugador al iniciar en modo silencioso
        // Esto evita que se disparen eventos antes de que todo esté inicializado
        LoadPlayerProfile(silent: true);
    }

    /// <summary>
    /// Valida que las referencias críticas estén asignadas.
    /// Muestra errores para referencias críticas y warnings para opcionales.
    /// </summary>
    private void ValidateReferences()
    {
        bool hasCriticalErrors = false;
        
        // Referencias CRÍTICAS (el juego no funcionará sin ellas)
        if (inventoryManager == null)
        {
            Debug.LogError("❌ CRÍTICO: GameDataManager - InventoryManager no está asignado. El inventario no funcionará. Asigna la referencia en el Inspector.");
            hasCriticalErrors = true;
        }
        
        if (playerMoney == null)
        {
            Debug.LogError("❌ CRÍTICO: GameDataManager - PlayerMoney no está asignado. El sistema de dinero no funcionará. Asigna la referencia en el Inspector.");
            hasCriticalErrors = true;
        }
        
        // Referencias IMPORTANTES (pueden causar problemas pero no bloquean todo)
        if (equipmentManager == null)
        {
            Debug.LogWarning("⚠️ GameDataManager - EquipmentManager no está asignado. El sistema de equipo puede no funcionar correctamente.");
        }
        
        if (itemDatabase == null)
        {
            Debug.LogWarning("⚠️ GameDataManager - ItemDatabase no está asignado. Puede haber problemas al cargar items desde la base de datos.");
        }
        
        // Mostrar resumen si hay errores críticos
        if (hasCriticalErrors)
        {
            Debug.LogError("🚨 GameDataManager tiene referencias críticas faltantes. El juego puede no funcionar correctamente. Revisa el Inspector y asigna las referencias faltantes.");
        }
    }

    /// <summary>
    /// Inicializa todas las referencias si no están asignadas (búsqueda automática).
    /// </summary>
    [ContextMenu("Auto-Asignar Referencias")]
    public void AutoAssignReferences()
    {
        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();
        
        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>();
        
        if (itemImprovementSystem == null)
            itemImprovementSystem = FindFirstObjectByType<ItemImprovementSystem>();
        
        if (playerMoney == null)
            playerMoney = FindFirstObjectByType<PlayerMoney>();
        
        if (shopService == null)
            shopService = FindFirstObjectByType<ShopService>();
        
        if (panelNavigationManager == null)
            panelNavigationManager = FindFirstObjectByType<PanelNavigationManager>();
        
        if (chestManager == null)
            chestManager = FindFirstObjectByType<ChestManager>();
        
        if (inventoryAutoOrganizer == null)
            inventoryAutoOrganizer = FindFirstObjectByType<InventoryAutoOrganizer>();
        
        if (heroProgressionService == null)
            heroProgressionService = FindFirstObjectByType<HeroProgressionService>();

        Debug.Log("GameDataManager: Referencias auto-asignadas.");
    }

    /// <summary>
    /// Guarda el perfil del jugador (items equipados con nivel + inventario completo).
    /// </summary>
    public void SavePlayerProfile()
    {
        // Guardar equipo
        if (equipmentManager != null)
        {
            playerProfile.SaveEquipmentState(equipmentManager);
        }
        
        // SOLUCIÓN: Guardar también el inventario completo
        if (inventoryManager != null)
        {
            playerProfile.SaveInventoryState(inventoryManager);
        }
        
        // SOLUCIÓN: Guardar las monedas del jugador
        if (playerMoney != null)
        {
            playerProfile.playerMoney = playerMoney.GetMoney();
        }
        
        // SOLUCIÓN CRÍTICA: Validar que currentEnergy no tenga un valor corrupto antes de guardar
        // Si tiene un valor sospechoso (48, 49) y no está durmiendo, obtener el valor correcto desde EnergySystem
        int energyBeforeValidation = playerProfile.currentEnergy;
        if (playerProfile.currentEnergy == 48 || playerProfile.currentEnergy == 49)
        {
            if (!playerProfile.isSleeping)
            {
                // Si no está durmiendo y tiene un valor sospechoso, intentar obtener el valor correcto
                EnergySystem energySystem = FindFirstObjectByType<EnergySystem>();
                if (energySystem != null)
                {
                    int correctEnergy = energySystem.GetCurrentEnergy();
                    
                    if (correctEnergy != 48 && correctEnergy != 49)
                    {
                        playerProfile.currentEnergy = correctEnergy;
                    }
                    else
                    {
                        playerProfile.currentEnergy = 0; // Cambiar de 100 a 0
                        playerProfile.isSleeping = false;
                    }
                }
                else
                {
                    playerProfile.currentEnergy = 0; // Cambiar de 100 a 0
                    playerProfile.isSleeping = false;
                }
            }
        }
        
        string json = JsonUtility.ToJson(playerProfile);
        
        PlayerPrefs.SetString(PLAYER_PROFILE_KEY, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Carga el perfil del jugador (items equipados + inventario completo).
    /// </summary>
    /// <param name="silent">Si es true, no dispara eventos de inventario (útil para carga inicial)</param>
    public void LoadPlayerProfile(bool silent = false)
    {
        if (PlayerPrefs.HasKey(PLAYER_PROFILE_KEY))
        {
            string json = PlayerPrefs.GetString(PLAYER_PROFILE_KEY);
            
            playerProfile = JsonUtility.FromJson<PlayerProfileData>(json);
            
            // SOLUCIÓN: Validar y corregir energía si tiene un valor inválido
            // Esto corrige valores como 48, 49 que pueden haber sido guardados por bugs anteriores
            // NO corregir valores válidos como 0 (después de reset)
            if (playerProfile.currentEnergy < 0 || playerProfile.currentEnergy > 100)
            {
                playerProfile.currentEnergy = 0; // Cambiar de 100 a 0
                playerProfile.isSleeping = false;
                SavePlayerProfile(); // Guardar el valor corregido inmediatamente
            }
            else if ((playerProfile.currentEnergy == 48 || playerProfile.currentEnergy == 49) && !playerProfile.isSleeping)
            {
                // Caso específico: valor 48 o 49 sin estar durmiendo (posible bug anterior)
                playerProfile.currentEnergy = 0; // Cambiar de 100 a 0
                playerProfile.isSleeping = false;
                SavePlayerProfile(); // Guardar el valor corregido inmediatamente
            }
            
            // SOLUCIÓN: Cargar también el inventario completo desde el perfil guardado
            // Usar modo silencioso en la carga inicial para evitar eventos prematuros
            if (inventoryManager != null)
            {
                playerProfile.LoadInventoryState(inventoryManager, silent: silent);
                // AUN EN MODO SILENCIOSO: notificar a los suscriptores para que UI/forja puedan rellenar
                // sin depender de eventos disparados durante la deserialización.
                inventoryManager.NotifyInventoryChanged();
            }
            
            // SOLUCIÓN CRÍTICA: Cargar también el equipo desde el perfil guardado
            // Esto asegura que los items equipados se restauren correctamente al reanudar la partida
            // Usar modo silencioso para evitar eventos prematuros durante la carga inicial
            if (equipmentManager != null && inventoryManager != null && itemDatabase != null)
            {
                playerProfile.LoadEquipmentState(equipmentManager, inventoryManager, itemDatabase, silent: silent);
            }
            
            // SOLUCIÓN: Cargar las monedas guardadas del jugador
            if (playerMoney != null)
            {
                playerMoney.SetMoney(playerProfile.playerMoney);
                playerMoney.MarkMoneyLoadedFromProfile();
            }

            // Aplicar cualquier bonificación de progresión pendiente y guardar si hubo cambios
            if (ApplyHeroProgressionToCurrentLevel())
            {
                SavePlayerProfile();
            }
        }
        else
        {
            playerProfile = new PlayerProfileData();

            // Perfil nuevo: limpiar el estado en memoria para evitar equipo/objetos fantasma.
            if (equipmentManager != null)
            {
                equipmentManager.ClearAllEquippedItems();
            }

            if (inventoryManager != null)
            {
                inventoryManager.ClearInventory();
                inventoryManager.NotifyInventoryChanged();
            }

            // Si es un perfil nuevo, usar las monedas iniciales del Inspector
            if (playerMoney != null)
            {
                // Las monedas iniciales se establecerán en PlayerMoney.Start()
            }
        }
    }

    /// <summary>
    /// Obtiene el nivel guardado de un item equipado por su GUID.
    /// </summary>
    public int GetEquippedItemLevelFromProfile(string instanceId)
    {
        return playerProfile.GetEquippedItemLevel(instanceId);
    }

    /// <summary>
    /// Sincroniza los niveles del inventario desde el perfil guardado sin recargar todo.
    /// Compara los niveles guardados con los actuales y actualiza solo si hay diferencias.
    /// </summary>
    public void SyncInventoryLevelsFromProfile()
    {
        if (inventoryManager == null || playerProfile == null)
            return;

        // Recargar el perfil desde PlayerPrefs para tener los datos más recientes
        if (PlayerPrefs.HasKey(PLAYER_PROFILE_KEY))
        {
            string json = PlayerPrefs.GetString(PLAYER_PROFILE_KEY);
            playerProfile = JsonUtility.FromJson<PlayerProfileData>(json);
        }

        // Si no hay datos de inventario guardados, no hacer nada
        if (playerProfile.inventoryData == null || playerProfile.inventoryData.Length != InventoryManager.INVENTORY_SIZE)
            return;

        // SOLUCIÓN: Sincronizar niveles item por item sin recargar todo el inventario
        // Esto preserva las referencias en memoria pero actualiza los niveles desde el perfil guardado
        bool anyLevelChanged = false;
        for (int i = 0; i < InventoryManager.INVENTORY_SIZE; i++)
        {
            ItemInstance currentItem = inventoryManager.GetItem(i);
            
            // Si hay un item en memoria y hay datos guardados para este slot
            if (currentItem != null && currentItem.IsValid() && 
                !string.IsNullOrEmpty(playerProfile.inventoryData[i]))
            {
                // Deserializar el item guardado para obtener su nivel
                ItemInstance savedItem = new ItemInstance();
                if (savedItem.Deserialize(playerProfile.inventoryData[i], itemDatabase))
                {
                    // Si es la misma instancia (mismo GUID) pero el nivel es diferente, actualizar
                    if (currentItem.IsSameInstance(savedItem) && 
                        currentItem.currentLevel != savedItem.currentLevel)
                    {
                        // Actualizar el nivel del item en memoria desde el perfil guardado
                        currentItem.SetLevel(savedItem.currentLevel);
                        anyLevelChanged = true;
                    }
                }
            }
        }

        if (anyLevelChanged)
        {
            Debug.Log("Niveles del inventario sincronizados desde el perfil guardado.");
        }
    }

    /// <summary>
    /// Sincroniza los niveles de los items equipados desde el perfil guardado sin recargar todo.
    /// Compara los niveles guardados con los actuales y actualiza solo si hay diferencias.
    /// </summary>
    public void SyncEquippedItemLevelsFromProfile()
    {
        if (equipmentManager == null || playerProfile == null)
            return;

        // Recargar el perfil desde PlayerPrefs para tener los datos más recientes
        if (PlayerPrefs.HasKey(PLAYER_PROFILE_KEY))
        {
            string json = PlayerPrefs.GetString(PLAYER_PROFILE_KEY);
            playerProfile = JsonUtility.FromJson<PlayerProfileData>(json);
        }

        // Si no hay datos de equipo guardados, no hacer nada
        if (playerProfile.equippedItems == null || playerProfile.equippedItems.Count == 0)
            return;

        // SOLUCIÓN: Sincronizar niveles de items equipados slot por slot
        // Esto preserva las referencias en memoria pero actualiza los niveles desde el perfil guardado
        bool anyLevelChanged = false;
        foreach (var slotType in System.Enum.GetValues(typeof(EquipmentManager.EquipmentSlotType)))
        {
            EquipmentManager.EquipmentSlotType slot = (EquipmentManager.EquipmentSlotType)slotType;
            ItemInstance currentItem = equipmentManager.GetEquippedItem(slot);
            
            // Si hay un item equipado, buscar su nivel guardado en el perfil
            if (currentItem != null && currentItem.IsValid())
            {
                string instanceId = currentItem.GetInstanceId();
                int savedLevel = playerProfile.GetEquippedItemLevel(instanceId);
                
                // Si hay un nivel guardado y es diferente del actual, actualizar
                if (savedLevel > 0 && savedLevel != currentItem.currentLevel)
                {
                    // Actualizar el nivel del item en memoria desde el perfil guardado
                    currentItem.SetLevel(savedLevel);
                    anyLevelChanged = true;
                }
            }
        }

        if (anyLevelChanged)
        {
            Debug.Log("Niveles de items equipados sincronizados desde el perfil guardado.");
        }
    }

    /// <summary>
    /// Obtiene el perfil del jugador.
    /// </summary>
    public PlayerProfileData GetPlayerProfile()
    {
        // Asegurar que el perfil esté cargado
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        return playerProfile;
    }

    /// <summary>
    /// Desbloquea un nivel de enemigo y guarda el progreso.
    /// </summary>
    public void UnlockEnemyLevel(int level)
    {
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        
        playerProfile.UnlockEnemyLevel(level);
        SavePlayerProfile();
    }

    /// <summary>
    /// Verifica si un nivel de enemigo está desbloqueado.
    /// </summary>
    public bool IsEnemyLevelUnlocked(int level)
    {
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        
        return playerProfile.IsEnemyLevelUnlocked(level);
    }

    /// <summary>
    /// Marca un enemigo como derrotado.
    /// </summary>
    /// <param name="enemyName">Nombre del enemigo derrotado</param>
    public void MarkEnemyDefeated(string enemyName)
    {
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        
        if (playerProfile != null)
        {
            playerProfile.MarkEnemyDefeated(enemyName);
            SavePlayerProfile();
        }
    }

    /// <summary>
    /// Incrementa el contador de enfrentamientos.
    /// </summary>
    public void IncrementTotalClashes()
    {
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        
        if (playerProfile != null)
        {
            playerProfile.totalClashes++;
            SavePlayerProfile();
            RefreshHeroProfileStatistics();
        }
    }

    /// <summary>
    /// Incrementa el contador de cofres abiertos.
    /// </summary>
    public void IncrementTotalOpenChests()
    {
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        
        if (playerProfile != null)
        {
            playerProfile.totalOpenChests++;
            SavePlayerProfile();
            RefreshHeroProfileStatistics();
        }
    }

    /// <summary>
    /// Incrementa el contador de peleas ganadas.
    /// </summary>
    public void IncrementTotalWonFights()
    {
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        
        if (playerProfile != null)
        {
            playerProfile.totalWonFights++;
            SavePlayerProfile();
            RefreshHeroProfileStatistics();
        }
    }

    /// <summary>
    /// Incrementa el contador de peleas perdidas.
    /// </summary>
    public void IncrementTotalLostFights()
    {
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        
        if (playerProfile != null)
        {
            playerProfile.totalLostFights++;
            SavePlayerProfile();
            RefreshHeroProfileStatistics();
        }
    }

    /// <summary>
    /// Refresca las estadísticas en HeroProfileManager si está disponible.
    /// </summary>
    private void RefreshHeroProfileStatistics()
    {
        HeroProfileManager heroProfileManager = FindFirstObjectByType<HeroProfileManager>();
        if (heroProfileManager != null)
        {
            heroProfileManager.RefreshStatistics();
            heroProfileManager.RefreshHeroExperience();
        }

        // Refrescar sección de experiencia en BreedManager si está disponible
        BreedManager breedManager = FindFirstObjectByType<BreedManager>();
        if (breedManager != null)
        {
            breedManager.RefreshHeroExperienceSection();
        }
    }

    /// <summary>
    /// Agrega experiencia al héroe y actualiza la UI.
    /// </summary>
    public void AddHeroExperience(int experience)
    {
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        
        if (playerProfile != null)
        {
            int oldLevel = playerProfile.heroLevel;
            playerProfile.AddHeroExperience(experience);
            bool leveledUp = playerProfile.heroLevel > oldLevel;
            ApplyHeroProgressionToCurrentLevel();
            SavePlayerProfile();

            RefreshHeroProfileStatistics();

            // Si subió de nivel, actualizar botones de ataque
            if (leveledUp)
            {
                // Notificar a CombatManager si está activo
                CombatManager combatManager = FindFirstObjectByType<CombatManager>();
                if (combatManager != null)
                {
                    combatManager.OnHeroLevelUp();
                }
            }
        }
    }

    /// <summary>
    /// Desbloquea un ataque en la biblioteca.
    /// </summary>
    /// <param name="attackName">Nombre del ataque a desbloquear</param>
    public void UnlockAttack(string attackName)
    {
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        
        if (playerProfile != null)
        {
            playerProfile.UnlockAttack(attackName);
            SavePlayerProfile();
        }
    }

    /// <summary>
    /// Verifica si un ataque está desbloqueado.
    /// </summary>
    /// <param name="attackName">Nombre del ataque a verificar</param>
    /// <returns>True si el ataque está desbloqueado, false en caso contrario</returns>
    public bool IsAttackUnlocked(string attackName)
    {
        if (playerProfile == null)
        {
            LoadPlayerProfile();
        }
        
        if (playerProfile != null)
        {
            return playerProfile.IsAttackUnlocked(attackName);
        }
        
        return false;
    }

    /// <summary>
    /// Aplica las bonificaciones de progresión del héroe hasta el nivel actual.
    /// </summary>
    private bool ApplyHeroProgressionToCurrentLevel()
    {
        if (playerProfile == null)
            return false;

        if (heroProgressionService == null)
        {
            heroProgressionService = FindFirstObjectByType<HeroProgressionService>();
        }

        if (heroProgressionService == null)
        {
            Debug.LogWarning("GameDataManager: HeroProgressionService no está disponible; no se aplicarán bonificaciones de nivel.");
            return false;
        }

        int previousLevelApplied = playerProfile.heroProgressionLevelApplied;
        heroProgressionService.ApplyLevelBonuses(playerProfile, playerProfile.heroLevel);
        return playerProfile.heroProgressionLevelApplied != previousLevelApplied;
    }
}

