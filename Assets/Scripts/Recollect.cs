using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sistema de recolección automática de monedas y objetos.
/// Calcula progreso usando DateTime.UtcNow para soportar avance offline y sincroniza con la UI.
/// </summary>
public class Recollect : MonoBehaviour
{
    private const string STATUS_STOPPED = "Detenido";
    private const string STATUS_RUNNING = "En curso";
    private const string STATUS_COMPLETED = "Expedición completada";
    private const float STATE_SAVE_INTERVAL = 2f;
    private const double ONE_SECOND_MS = 1000d;

    [Header("Referencias principales")]
    [SerializeField] private EnergySystem energySystem;
    [SerializeField] private GameDataManager gameDataManager;
    [SerializeField] private PlayerMoney playerMoney;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private RewardTierDatabase rewardTierDatabase;

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button collectAllButton;
    [SerializeField] private Button historyOpenButton;
    [SerializeField] private Button historyCloseButton;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI itemsText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI rewardsSummaryText;
    [SerializeField] private GameObject animationPanel;
    [SerializeField] private GameObject historyPanel;
    [SerializeField] private Transform historyEntriesRoot;
    [SerializeField] private GameObject historyEntryPrefab;

    [Header("Configuración")]
    [SerializeField, Tooltip("Coste de energía por iniciar una expedición")]
    private int energyCost = 10;
    [SerializeField, Tooltip("Duración máxima de la expedición (ms)")]
    private double maxSessionMilliseconds = 86_400_000d; // 24h
    [SerializeField, Tooltip("Milisegundos necesarios para obtener una moneda")]
    private double coinIntervalMilliseconds = 1000d;
    [SerializeField, Tooltip("Milisegundos necesarios para obtener un objeto")]
    private double itemIntervalMilliseconds = 60_000d;
    [SerializeField, Tooltip("Segundos entre mensajes del historial")]
    private float historyEntryIntervalSeconds = 30f;
    [SerializeField, Tooltip("Entradas máximas del historial (FIFO)")]
    private int maxHistoryEntries = 10;

    private PlayerProfileData profile;
    private bool expeditionRunning;
    private bool expeditionCompleted;
    private double accumulatedCoins;
    private double elapsedMilliseconds;
    private double coinTimerMs;
    private double itemTimerMs;
    private double historyCoins;
    private readonly List<ItemInstance> pendingItems = new();
    private readonly List<PlayerProfileData.RecollectItemSnapshot> pendingSnapshots = new();
    private readonly List<ItemInstance> historyItems = new();
    private readonly List<string> historyCache = new();
    private readonly List<GameObject> historySpawned = new();
    private DateTime expeditionStartUtc = DateTime.MinValue;
    private DateTime lastUpdateUtc = DateTime.MinValue;
    private float historyTimer;
    private float saveCooldown;
    private bool stateDirty;

    private void Awake()
    {
        gameDataManager ??= GameDataManager.Instance;
        if (gameDataManager != null)
        {
            playerMoney ??= gameDataManager.PlayerMoney;
            inventoryManager ??= gameDataManager.InventoryManager;
            itemDatabase ??= gameDataManager.ItemDatabase;
        }

        if (energySystem == null)
        {
            energySystem = FindFirstObjectByType<EnergySystem>(FindObjectsInactive.Include);
        }
    }

    private void OnEnable()
    {
        RegisterCallbacks();
        if (energySystem != null)
        {
            energySystem.OnEnergyChanged += HandleEnergyChanged;
        }

        InitializeState();
        UpdateEnergyUI();
        UpdateUI();
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
        if (energySystem != null)
        {
            energySystem.OnEnergyChanged -= HandleEnergyChanged;
        }

        PersistState(forceImmediate: true);
    }

    private void Update()
    {
        if (!expeditionRunning)
        {
            if (expeditionCompleted)
            {
                UpdateUI();
            }
            return;
        }

        if (profile == null)
            return;

        DateTime now = DateTime.UtcNow;
        if (lastUpdateUtc == DateTime.MinValue)
        {
            lastUpdateUtc = now;
        }

        double deltaMs = Math.Max(0d, (now - lastUpdateUtc).TotalMilliseconds);
        lastUpdateUtc = now;

        ProcessProgress(deltaMs);
        historyTimer += Time.deltaTime;
        if (historyTimer >= historyEntryIntervalSeconds && historyEntryIntervalSeconds > 0f)
        {
            historyTimer = 0f;
            FlushHistoryInterval();
        }

        if (elapsedMilliseconds >= maxSessionMilliseconds)
        {
            CompleteExpedition(autoClaim: true);
        }

        UpdateUI();
        PersistState();
    }

    #region Inicialización / Persistencia

    private void InitializeState()
    {
        if (!EnsureDependencies())
            return;

        expeditionRunning = profile.recollectIsRunning;
        expeditionCompleted = profile.recollectCompleted;
        accumulatedCoins = profile.recollectPendingCoins;
        elapsedMilliseconds = profile.recollectElapsedMilliseconds;
        coinTimerMs = profile.recollectCoinTimerMs;
        itemTimerMs = profile.recollectItemTimerMs;
        expeditionStartUtc = ParseUtc(profile.recollectStartUtcString);
        lastUpdateUtc = ParseUtc(profile.recollectLastUpdateUtcString);

        pendingSnapshots.Clear();
        pendingSnapshots.AddRange(profile.LoadRecollectItems());
        RebuildItemsFromSnapshots();

        historyCache.Clear();
        historyCache.AddRange(profile.LoadRecollectHistory());
        RebuildHistoryUI();

        if (expeditionRunning && elapsedMilliseconds >= maxSessionMilliseconds)
        {
            CompleteExpedition(autoClaim: true);
        }

        SetAnimationPanel(expeditionRunning);
        UpdateButtonStates();
        UpdateStatusText();
    }

    private void PersistState(bool forceImmediate = false)
    {
        if (profile == null || gameDataManager == null)
            return;

        if (!forceImmediate)
        {
            saveCooldown -= Time.deltaTime;
            if (!stateDirty && saveCooldown > 0f)
                return;
        }

        profile.SaveRecollectState(expeditionRunning, expeditionCompleted, elapsedMilliseconds, accumulatedCoins, coinTimerMs, itemTimerMs, expeditionStartUtc, lastUpdateUtc);
        profile.SaveRecollectItems(pendingSnapshots);
        profile.SaveRecollectHistory(historyCache);
        gameDataManager.SavePlayerProfile();

        stateDirty = false;
        saveCooldown = STATE_SAVE_INTERVAL;
    }

    private void ClearPersistentState()
    {
        if (profile == null || gameDataManager == null)
            return;

        profile.ClearRecollectState();
        gameDataManager.SavePlayerProfile();
    }

    #endregion

    #region Botones / Acciones

    private void RegisterCallbacks()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (collectAllButton != null)
            collectAllButton.onClick.AddListener(OnCollectClicked);
        if (historyOpenButton != null)
            historyOpenButton.onClick.AddListener(() => SetHistoryPanelVisible(true));
        if (historyCloseButton != null)
            historyCloseButton.onClick.AddListener(() => SetHistoryPanelVisible(false));
    }

    private void UnregisterCallbacks()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartClicked);
        if (collectAllButton != null)
            collectAllButton.onClick.RemoveListener(OnCollectClicked);
        if (historyOpenButton != null)
            historyOpenButton.onClick.RemoveAllListeners();
        if (historyCloseButton != null)
            historyCloseButton.onClick.RemoveAllListeners();
    }

    private void OnStartClicked()
    {
        if (expeditionRunning || expeditionCompleted)
            return;

        if (!EnsureDependencies() || energySystem == null)
            return;

        if (!energySystem.CanAfford(energyCost))
        {
            Debug.LogWarning("Recollect: energía insuficiente.");
            return;
        }

        if (!energySystem.SpendEnergy(energyCost))
        {
            Debug.LogWarning("Recollect: no se pudo gastar energía.");
            return;
        }

        BeginExpedition();
        UpdateEnergyUI();
    }

    private void OnCollectClicked()
    {
        if (!expeditionRunning && !expeditionCompleted)
            return;

        ClaimRewards(autoClaim: false);
        ResetRuntimeState();
        ClearPersistentState();
        UpdateUI();
    }

    private void BeginExpedition()
    {
        expeditionRunning = true;
        expeditionCompleted = false;
        accumulatedCoins = 0d;
        elapsedMilliseconds = 0d;
        coinTimerMs = 0d;
        itemTimerMs = 0d;
        expeditionStartUtc = DateTime.UtcNow;
        lastUpdateUtc = expeditionStartUtc;
        rewardsSummaryText?.SetText(string.Empty);
        pendingItems.Clear();
        pendingSnapshots.Clear();
        ClearHistoryUI();
        historyCache.Clear();
        historyTimer = 0f;
        historyCoins = 0d;
        historyItems.Clear();

        SetAnimationPanel(true);
        UpdateButtonStates();
        UpdateStatusText();
        stateDirty = true;
        PersistState(forceImmediate: true);
    }

    private void CompleteExpedition(bool autoClaim)
    {
        if (!expeditionRunning)
            return;

        expeditionRunning = false;
        expeditionCompleted = true;
        SetAnimationPanel(false);
        FlushHistoryInterval(forceMessage: true);
        ClaimRewards(autoClaim);
        UpdateButtonStates();
        UpdateStatusText();
        stateDirty = true;
        PersistState(forceImmediate: true);
    }

    private void ClaimRewards(bool autoClaim)
    {
        if (!EnsureDependencies())
            return;

        int coinsAward = Mathf.FloorToInt((float)accumulatedCoins);
        int coinsFromLiquidation = 0;
        int itemsAdded = 0;
        int itemsDestroyed = 0;

        if (pendingItems.Count > 0)
        {
            foreach (var item in pendingItems)
            {
                if (item == null || !item.IsValid())
                    continue;

                int slotIndex = inventoryManager.AddItem(item);
                if (slotIndex >= 0)
                {
                    itemsAdded++;
                }
                else
                {
                    itemsDestroyed++;
                    coinsFromLiquidation += GetSellPrice(item);
                }
            }
        }

        int totalCoins = coinsAward + coinsFromLiquidation;
        if (totalCoins > 0 && playerMoney != null)
        {
            playerMoney.AddMoney(totalCoins);
        }

        string summary = BuildRewardsSummary(totalCoins, coinsAward, coinsFromLiquidation, itemsDestroyed);
        rewardsSummaryText?.SetText(summary);

        accumulatedCoins = 0d;
        pendingItems.Clear();
        pendingSnapshots.Clear();
        stateDirty = true;
    }

    private string BuildRewardsSummary(int totalCoins, int baseCoins, int compensationCoins, int itemsDestroyed)
    {
        List<string> lines = new()
        {
            "Has obtenido:",
            $"Monedas: +{totalCoins}"
        };

        if (compensationCoins > 0)
        {
            lines.Add($"(Incluye +{compensationCoins} por objetos destruidos)");
        }

        lines.Add("Objetos:");

        if (pendingSnapshots.Count == 0)
        {
            lines.Add("- Ningún objeto");
        }
        else
        {
            const int maxVisibleItems = 4;
            var ordered = pendingSnapshots
                .OrderBy(s => GetRarityRank(s.rarityId))
                .ThenBy(s => s.itemName)
                .ToList();

            for (int i = 0; i < ordered.Count && i < maxVisibleItems; i++)
            {
                var snapshot = ordered[i];
                string rarityLabel = RarityColorProvider.GetDisplayName(snapshot.rarityId);
                string coloredLabel = ColorizeByRarity(snapshot.rarityId, $"[{rarityLabel}] {snapshot.itemName}");
                lines.Add($"- {coloredLabel}");
            }

            if (ordered.Count > maxVisibleItems)
            {
                lines.Add("...");
            }
        }

        if (itemsDestroyed > 0)
        {
            lines.Add($"{itemsDestroyed} objetos fueron destruidos por falta de espacio. Roma te paga +{compensationCoins} monedas.");
        }

        return string.Join("\n", lines);
    }

    private void ResetRuntimeState()
    {
        expeditionRunning = false;
        expeditionCompleted = false;
        accumulatedCoins = 0d;
        elapsedMilliseconds = 0d;
        coinTimerMs = 0d;
        itemTimerMs = 0d;
        historyCoins = 0d;
        pendingItems.Clear();
        pendingSnapshots.Clear();
        historyItems.Clear();
        historyCache.Clear();
        ClearHistoryUI();
        SetAnimationPanel(false);
        UpdateButtonStates();
        UpdateStatusText();
    }

    #endregion

    #region Lógica de progreso

    private void ProcessProgress(double deltaMs)
    {
        elapsedMilliseconds += deltaMs;
        coinTimerMs += deltaMs;
        itemTimerMs += deltaMs;

        bool gainedCoins = false;

        if (coinIntervalMilliseconds > 0d && coinTimerMs >= coinIntervalMilliseconds)
        {
            double coinsEarned = Math.Floor(coinTimerMs / coinIntervalMilliseconds);
            if (coinsEarned > 0d)
            {
                accumulatedCoins += coinsEarned;
                historyCoins += coinsEarned;
                coinTimerMs -= coinsEarned * coinIntervalMilliseconds;
                gainedCoins = true;
                stateDirty = true;
            }
        }

        if (itemIntervalMilliseconds > 0d && itemTimerMs >= itemIntervalMilliseconds)
        {
            int itemsEarned = Mathf.FloorToInt((float)(itemTimerMs / itemIntervalMilliseconds));
            if (itemsEarned > 0)
            {
                for (int i = 0; i < itemsEarned; i++)
                {
                    if (TryGenerateItem(out ItemInstance instance, out PlayerProfileData.RecollectItemSnapshot snapshot))
                    {
                        pendingItems.Add(instance);
                        pendingSnapshots.Add(snapshot);
                        historyItems.Add(instance);
                        stateDirty = true;
                    }
                }
                itemTimerMs -= itemsEarned * itemIntervalMilliseconds;
            }
        }

        if (gainedCoins)
        {
            stateDirty = true;
        }
    }

    private bool TryGenerateItem(out ItemInstance instance, out PlayerProfileData.RecollectItemSnapshot snapshot)
    {
        instance = null;
        snapshot = null;

        if (rewardTierDatabase == null || profile == null)
            return false;

        int tier = GetTierForHeroLevel(profile.heroLevel);
        ItemData data = rewardTierDatabase.GetRandomItemFromTierWeightedByType(tier);
        data ??= rewardTierDatabase.GetRandomItemFromTier(tier);

        if (data == null)
            return false;

        instance = new ItemInstance(data);
        snapshot = new PlayerProfileData.RecollectItemSnapshot
        {
            itemId = data.name,
            itemName = data.itemName,
            level = instance.currentLevel,
            rarityId = data.rareza,
            sellPrice = data.price
        };

        return true;
    }

    #endregion

    #region Historial

    private void FlushHistoryInterval(bool forceMessage = false)
    {
        bool hasCoins = historyCoins > 0d;
        bool hasItems = historyItems.Count > 0;

        if (!forceMessage && !hasCoins && !hasItems)
            return;

        string message = BuildHistoryMessage(hasCoins, hasItems);
        AddHistoryEntry(message);

        historyCoins = 0d;
        historyItems.Clear();
    }

    private string BuildHistoryMessage(bool hasCoins, bool hasItems)
    {
        if (!hasCoins && !hasItems)
        {
            return "El héroe no ha encontrado enemigos y no ha ganado nada.";
        }

        if (hasCoins && hasItems)
        {
            ItemInstance item = historyItems[0];
            string rarity = RarityColorProvider.GetDisplayName(item?.GetRarity());
            string coloredName = ColorizeByRarity(item?.GetRarity(), item?.GetItemName());
            string coloredRarity = ColorizeByRarity(item?.GetRarity(), rarity);
            return $"El héroe ha encontrado un enemigo y ha ganado +{Mathf.FloorToInt((float)historyCoins)} monedas por una gran victoria.\nAdemás obtuvo {coloredName} ({coloredRarity}) por una victoria fascinante.";
        }

        if (hasCoins)
        {
            return $"El héroe ha encontrado un enemigo y ha ganado +{Mathf.FloorToInt((float)historyCoins)} monedas por una gran victoria.";
        }

        ItemInstance earnedItem = historyItems[0];
        string earnedName = ColorizeByRarity(earnedItem?.GetRarity(), earnedItem?.GetItemName());
        string earnedRarity = ColorizeByRarity(earnedItem?.GetRarity(), RarityColorProvider.GetDisplayName(earnedItem?.GetRarity()));
        return $"El héroe ha ganado {earnedName} ({earnedRarity}) por una victoria fascinante.";
    }

    private void AddHistoryEntry(string message)
    {
        historyCache.Add(message);
        while (historyCache.Count > maxHistoryEntries)
        {
            historyCache.RemoveAt(0);
        }

        if (historyEntryPrefab == null || historyEntriesRoot == null)
            return;

        GameObject entry = Instantiate(historyEntryPrefab, historyEntriesRoot);
        var text = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = message;
        }
        historySpawned.Add(entry);

        while (historySpawned.Count > maxHistoryEntries)
        {
            GameObject oldest = historySpawned[0];
            historySpawned.RemoveAt(0);
            Destroy(oldest);
        }

        stateDirty = true;
    }

    private void RebuildHistoryUI()
    {
        ClearHistoryUI();
        if (historyEntryPrefab == null || historyEntriesRoot == null)
            return;

        foreach (var entry in historyCache)
        {
            GameObject instance = Instantiate(historyEntryPrefab, historyEntriesRoot);
            var text = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = entry;
            }
            historySpawned.Add(instance);
        }
    }

    private void ClearHistoryUI()
    {
        foreach (var entry in historySpawned)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        historySpawned.Clear();
    }

    private void SetHistoryPanelVisible(bool visible)
    {
        if (historyPanel != null)
        {
            historyPanel.SetActive(visible);
        }
    }

    #endregion

    #region UI Helpers

    private void UpdateUI()
    {
        double clamped = Math.Min(elapsedMilliseconds, maxSessionMilliseconds);
        TimeSpan span = TimeSpan.FromMilliseconds(clamped);
        timerText?.SetText($"{(int)span.TotalHours:00}:{span.Minutes:00}:{span.Seconds:00}");
        coinsText?.SetText($"Monedas acumuladas: {Mathf.FloorToInt((float)accumulatedCoins)}");
        itemsText?.SetText($"Objetos acumulados: {pendingSnapshots.Count}");
        UpdateStatusText();
        UpdateButtonStates();
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
            return;

        statusText.SetText(expeditionRunning ? STATUS_RUNNING : expeditionCompleted ? STATUS_COMPLETED : STATUS_STOPPED);
    }

    private void UpdateButtonStates()
    {
        if (startButton != null)
            startButton.interactable = !expeditionRunning && !expeditionCompleted;
        if (collectAllButton != null)
            collectAllButton.interactable = expeditionRunning || expeditionCompleted;
    }

    private void SetAnimationPanel(bool visible)
    {
        if (animationPanel != null)
        {
            animationPanel.SetActive(visible);
        }
    }

    private void HandleEnergyChanged(int current, int max, bool sleeping)
    {
        UpdateEnergyUI(current, max);
    }

    private void UpdateEnergyUI()
    {
        if (energySystem == null)
            return;
        UpdateEnergyUI(energySystem.GetCurrentEnergy(), energySystem.GetMaxEnergy());
    }

    private void UpdateEnergyUI(int current, int max)
    {
        energyText?.SetText($"Energía: {current}/{max}");
    }

    #endregion

    #region Utilidades

    private void RebuildItemsFromSnapshots()
    {
        pendingItems.Clear();
        if (itemDatabase == null)
            return;

        foreach (var snapshot in pendingSnapshots)
        {
            ItemInstance instance = SnapshotToInstance(snapshot);
            if (instance != null)
            {
                pendingItems.Add(instance);
            }
        }
    }

    private ItemInstance SnapshotToInstance(PlayerProfileData.RecollectItemSnapshot snapshot)
    {
        if (snapshot == null || itemDatabase == null)
            return null;

        ItemData data = itemDatabase.GetItemByName(snapshot.itemId);
        data ??= itemDatabase.GetItemByName(snapshot.itemName);

        if (data == null)
            return null;

        return new ItemInstance(data, Mathf.Max(1, snapshot.level));
    }

    private DateTime ParseUtc(string isoString)
    {
        if (string.IsNullOrEmpty(isoString))
            return DateTime.MinValue;

        if (DateTime.TryParse(isoString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed))
        {
            if (parsed.Kind == DateTimeKind.Unspecified)
            {
                parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }
            return parsed.ToUniversalTime();
        }

        return DateTime.MinValue;
    }

    private bool EnsureDependencies()
    {
        if (gameDataManager == null)
        {
            gameDataManager = GameDataManager.Instance;
        }

        if (gameDataManager == null)
        {
            Debug.LogError("Recollect: GameDataManager no encontrado.");
            return false;
        }

        playerMoney ??= gameDataManager.PlayerMoney;
        inventoryManager ??= gameDataManager.InventoryManager;
        itemDatabase ??= gameDataManager.ItemDatabase;
        profile ??= gameDataManager.GetPlayerProfile();

        if (playerMoney == null || inventoryManager == null || itemDatabase == null || profile == null)
        {
            Debug.LogError("Recollect: faltan dependencias críticas (dinero, inventario o perfil).");
            return false;
        }

        return true;
    }

    private int GetTierForHeroLevel(int heroLevel)
    {
        if (heroLevel >= 500) return 5;
        if (heroLevel >= 400) return 4;
        if (heroLevel >= 300) return 3;
        if (heroLevel >= 200) return 2;
        return 1;
    }

    private int GetSellPrice(ItemInstance instance)
    {
        if (instance?.baseItem == null)
            return 0;
        return Mathf.Max(0, instance.baseItem.price);
    }

    private int GetRarityRank(string rarity)
    {
        string[] order =
        {
            "Plebeius",
            "Auxiliaris",
            "Legionarius",
            "Veteranus",
            "Centurio",
            "Tribunus",
            "Praetorianus",
            "Imperialis",
            "Augustus",
            "Divinus"
        };

        for (int i = 0; i < order.Length; i++)
        {
            if (string.Equals(order[i], rarity, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return int.MaxValue;
    }

    private string ColorizeByRarity(string rarityId, string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        string hex = RarityColorProvider.GetColorHex(rarityId);
        if (string.IsNullOrEmpty(hex))
            return text;

        return $"<color=#{hex}>{text}</color>";
    }

    #endregion
}