using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Gestiona el panel de historia con persistencia de datos.
/// Sistema independiente (sin combate) con capítulos configurables y nodos ScriptableObject.
/// Usa botones preexistentes (sin instanciar prefabs) para evitar problemas de anclaje/layout.
/// </summary>
public class StoryPanelManager : MonoBehaviour
{
    [System.Serializable]
    public class ChapterConfig
    {
        public string chapterName;
        public StoryPanelNode entryNode;
        public int rewardCoins;
        public ItemData[] rewardItems = new ItemData[0];
        public Button openChapterButton;
    }

    [Header("Capítulos")]
    [SerializeField] private ChapterConfig[] chapters = new ChapterConfig[0];

    [Header("Estado Actual")]
    [SerializeField] private ChapterConfig currentChapter;
    [SerializeField] private StoryPanelNode currentNode;

    [Header("UI - Contenido (por subpanel)")]
    [SerializeField] private Image introImage;
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private Image transitionAImage;
    [SerializeField] private TextMeshProUGUI transitionAText;
    [SerializeField] private Image transitionBImage;
    [SerializeField] private TextMeshProUGUI transitionBText;
    [SerializeField] private Image chapterEndImage;
    [SerializeField] private TextMeshProUGUI chapterEndText;

    [Header("UI - Recompensas")]
    [Tooltip("Texto donde se listan monedas e items obtenidos al finalizar el capítulo")]
    [SerializeField] private TextMeshProUGUI rewardsText;
    [Tooltip("Contenedor para instanciar slots de objetos de recompensa (opcional)")]
    [SerializeField] private Transform rewardSlotsContainer;
    [Tooltip("Prefab para mostrar cada objeto de recompensa (debe tener RewardSlot)")]
    [SerializeField] private GameObject rewardSlotPrefab;

    [Header("UI - Panel Contenedor")]
    [Tooltip("Panel que contiene la UI del capítulo (intro/transiciones/fin). Se activa al pulsar un botón de capítulo.")]
    [SerializeField] private GameObject chapterContentPanel;

    [Header("UI - Subpaneles (opcional)")]
    [Tooltip("Opcional: Panel intro (se activa al entrar en nodos Intro)")]
    [SerializeField] private GameObject introPanel;
    [Tooltip("Opcional: Panel transición A (1 opción)")]
    [SerializeField] private GameObject transitionAPanel;
    [Tooltip("Opcional: Panel transición B (2 opciones)")]
    [SerializeField] private GameObject transitionBPanel;
    [Tooltip("Opcional: Panel fin de capítulo")]
    [SerializeField] private GameObject chapterEndPanel;

    [Header("UI - Botones (preexistentes)")]
    [SerializeField] private Button introAcceptButton;
    [SerializeField] private Button transitionAButton;
    [SerializeField] private Button transitionBButton;
    [SerializeField] private Button transitionBSecondButton;
    [SerializeField] private Button chapterEndAcceptButton;
    [SerializeField] private Button exitButton;

    [Header("UI - Textos de Botones")]
    [SerializeField] private TextMeshProUGUI introAcceptButtonText;
    [SerializeField] private TextMeshProUGUI transitionAButtonText;
    [SerializeField] private TextMeshProUGUI transitionBButtonText;
    [SerializeField] private TextMeshProUGUI transitionBSecondButtonText;
    [SerializeField] private TextMeshProUGUI chapterEndAcceptButtonText;

    [Header("Navegación")]
    [Tooltip("Panel que se abrirá al pulsar Salir (por ejemplo: Panel General Breed)")]
    [SerializeField] private GameObject panelToOpenOnExit;

    private GameDataManager gameDataManager;

    private void Awake()
    {
        gameDataManager = GameDataManager.Instance;
        WireChapterButtons();
        WireExitButton();
        HideAllActionButtons();

        if (chapterContentPanel != null)
        {
            chapterContentPanel.SetActive(false);
        }

        HideAllStorySubPanels();
    }
    
    /// <summary>
    /// Se llama cuando el panel se activa.
    /// </summary>
    private void OnEnable()
    {
        HideAllActionButtons();
        HideAllStorySubPanels();
    }
    
    /// <summary>
    /// Se llama cuando el panel se desactiva.
    /// Guarda el estado actual.
    /// </summary>
    private void OnDisable()
    {
        SaveStoryState();
    }

    public void StartChapterByIndex(int chapterIndex)
    {
        if (chapters == null || chapterIndex < 0 || chapterIndex >= chapters.Length)
            return;

        StartChapter(chapters[chapterIndex]);
    }

    public void StartChapter(ChapterConfig chapter)
    {
        if (chapter == null || chapter.entryNode == null)
            return;

        currentChapter = chapter;

        if (chapterContentPanel != null)
        {
            chapterContentPanel.SetActive(true);
        }

        GoToNode(chapter.entryNode);
    }

    public void GoToNode(StoryPanelNode node)
    {
        if (node == null)
            return;

        currentNode = node;
        node.Enter(this);
        SaveStoryState();
    }

    public void ShowIntroNode(StoryPanelIntroNode node)
    {
        if (node == null)
            return;

        SetContent(introImage, introText, node.image, node.text);
        SetRewardsText(null);
        ClearRewardSlots();
        HideAllActionButtons();
        ActivateSubPanel(introPanel);

        if (introAcceptButton != null)
        {
            introAcceptButton.gameObject.SetActive(true);
            introAcceptButton.onClick.RemoveAllListeners();
            if (introAcceptButtonText != null)
                introAcceptButtonText.text = string.IsNullOrEmpty(node.acceptText) ? "Continuar" : node.acceptText;
            introAcceptButton.onClick.AddListener(() => GoToNode(node.nextNode));
        }
    }

    public void ShowTransitionNode(StoryPanelTransitionNode node)
    {
        if (node == null)
            return;

        if (node.hasOptionB)
            SetContent(transitionBImage, transitionBText, node.image, node.text);
        else
            SetContent(transitionAImage, transitionAText, node.image, node.text);

        SetRewardsText(null);
        HideAllActionButtons();

        // Subpanel: 1 opción => A, 2 opciones => B
        if (node.hasOptionB)
            ActivateSubPanel(transitionBPanel);
        else
            ActivateSubPanel(transitionAPanel);

        // Caso 1: 1 opción (Transition A)
        if (!node.hasOptionB)
        {
            if (transitionAButton != null)
            {
                transitionAButton.gameObject.SetActive(true);
                transitionAButton.onClick.RemoveAllListeners();
                if (transitionAButtonText != null)
                    transitionAButtonText.text = node.optionAText;
                transitionAButton.onClick.AddListener(() => GoToNode(node.optionANode));
            }

            // Asegurar que no se vean botones de transición B
            if (transitionBButton != null) transitionBButton.gameObject.SetActive(false);
            if (transitionBSecondButton != null) transitionBSecondButton.gameObject.SetActive(false);
        }
        // Caso 2: 2 opciones (Transition B)
        else
        {
            // En Transition B usamos dos botones (A y B). No mostramos el botón de Transition A.
            if (transitionAButton != null) transitionAButton.gameObject.SetActive(false);

            if (transitionBButton != null)
            {
                transitionBButton.gameObject.SetActive(true);
                transitionBButton.onClick.RemoveAllListeners();
                if (transitionBButtonText != null)
                    transitionBButtonText.text = node.optionAText;
                transitionBButton.onClick.AddListener(() => GoToNode(node.optionANode));
            }

            if (transitionBSecondButton != null)
            {
                transitionBSecondButton.gameObject.SetActive(true);
                transitionBSecondButton.onClick.RemoveAllListeners();
                if (transitionBSecondButtonText != null)
                    transitionBSecondButtonText.text = node.optionBText;
                transitionBSecondButton.onClick.AddListener(() => GoToNode(node.optionBNode));
            }
        }
    }

    public void ShowChapterEndNode(StoryPanelChapterEndNode node)
    {
        if (node == null)
            return;

        SetContent(chapterEndImage, chapterEndText, node.image, node.text);
        UpdateRewardsPreviewText();
        HideAllActionButtons();
        ActivateSubPanel(chapterEndPanel);

        if (chapterEndAcceptButton != null)
        {
            chapterEndAcceptButton.gameObject.SetActive(true);
            chapterEndAcceptButton.onClick.RemoveAllListeners();
            if (chapterEndAcceptButtonText != null)
                chapterEndAcceptButtonText.text = string.IsNullOrEmpty(node.acceptText) ? "Aceptar" : node.acceptText;
            chapterEndAcceptButton.onClick.AddListener(ApplyChapterRewardsAndExit);
        }
    }

    private void ApplyChapterRewardsAndExit()
    {
        if (currentChapter == null)
        {
            ExitTo(panelToOpenOnExit);
            return;
        }

        if (gameDataManager != null)
        {
            if (gameDataManager.PlayerMoney != null)
            {
                gameDataManager.PlayerMoney.AddMoney(currentChapter.rewardCoins);
            }
            else
            {
                PlayerProfileData profile = gameDataManager.GetPlayerProfile();
                if (profile != null)
                {
                    profile.playerMoney += currentChapter.rewardCoins;
                }
            }

            if (gameDataManager.InventoryManager != null && currentChapter.rewardItems != null)
            {
                foreach (var item in currentChapter.rewardItems)
                {
                    if (item != null)
                    {
                        gameDataManager.InventoryManager.AddItem(item);
                    }
                }
            }

            gameDataManager.SavePlayerProfile();
        }

        ExitTo(panelToOpenOnExit);
    }

    private void SetContent(Image targetImage, TextMeshProUGUI targetText, Sprite sprite, string text)
    {
        if (targetImage != null)
        {
            targetImage.sprite = sprite;
            targetImage.enabled = sprite != null;
        }

        if (targetText != null)
        {
            targetText.text = text ?? "";
        }
    }

    private void SetRewardsText(string value)
    {
        if (rewardsText == null)
            return;

        rewardsText.text = value ?? "";
    }

    private void UpdateRewardsPreviewText()
    {
        if (currentChapter == null)
        {
            SetRewardsText("");
            ClearRewardSlots();
            return;
        }

        bool canShowSlots = rewardSlotsContainer != null && rewardSlotPrefab != null;
        string itemsLine = BuildItemsLine(currentChapter.rewardItems);

        if (!string.IsNullOrEmpty(itemsLine) && !canShowSlots)
        {
            SetRewardsText($"Monedas: {currentChapter.rewardCoins}\nObjetos: {itemsLine}");
        }
        else
        {
            SetRewardsText($"Monedas: {currentChapter.rewardCoins}");
        }

        if (canShowSlots)
        {
            PopulateRewardSlots(currentChapter.rewardItems);
        }
        else
        {
            ClearRewardSlots();
        }
    }

    private string BuildItemsLine(ItemData[] rewardItems)
    {
        if (rewardItems == null || rewardItems.Length == 0)
            return "";

        List<string> names = new List<string>();
        foreach (var item in rewardItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemName))
            {
                names.Add(item.itemName);
            }
        }

        return names.Count > 0 ? string.Join(", ", names) : "";
    }

    private void PopulateRewardSlots(ItemData[] rewardItems)
    {
        ClearRewardSlots();

        if (rewardItems == null || rewardItems.Length == 0 || rewardSlotsContainer == null || rewardSlotPrefab == null)
            return;

        foreach (var itemData in rewardItems)
        {
            if (itemData == null)
                continue;

            GameObject slotObj = Instantiate(rewardSlotPrefab, rewardSlotsContainer);
            if (slotObj == null)
                continue;

            RewardSlot slot = slotObj.GetComponent<RewardSlot>();
            if (slot == null)
            {
                slot = slotObj.GetComponentInChildren<RewardSlot>(true);
            }

            if (slot == null)
                continue;

            ItemInstance tempInstance = new ItemInstance(itemData);
            slot.Setup(tempInstance);
        }
    }

    private void ClearRewardSlots()
    {
        if (rewardSlotsContainer == null)
            return;

        for (int i = rewardSlotsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(rewardSlotsContainer.GetChild(i).gameObject);
        }
    }

    private void HideAllActionButtons()
    {
        if (introAcceptButton != null) introAcceptButton.gameObject.SetActive(false);
        if (transitionAButton != null) transitionAButton.gameObject.SetActive(false);
        if (transitionBButton != null) transitionBButton.gameObject.SetActive(false);
        if (transitionBSecondButton != null) transitionBSecondButton.gameObject.SetActive(false);
        if (chapterEndAcceptButton != null) chapterEndAcceptButton.gameObject.SetActive(false);
    }

    private void HideAllStorySubPanels()
    {
        if (introPanel != null) introPanel.SetActive(false);
        if (transitionAPanel != null) transitionAPanel.SetActive(false);
        if (transitionBPanel != null) transitionBPanel.SetActive(false);
        if (chapterEndPanel != null) chapterEndPanel.SetActive(false);
    }

    private void ActivateSubPanel(GameObject panel)
    {
        HideAllStorySubPanels();

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void WireChapterButtons()
    {
        if (chapters == null)
            return;

        for (int i = 0; i < chapters.Length; i++)
        {
            int index = i;
            if (chapters[i] != null && chapters[i].openChapterButton != null)
            {
                chapters[i].openChapterButton.onClick.RemoveAllListeners();
                chapters[i].openChapterButton.onClick.AddListener(() => StartChapterByIndex(index));
            }
        }
    }

    private void WireExitButton()
    {
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => ExitTo(panelToOpenOnExit));
        }
    }

    private void ExitTo(GameObject panelToOpen)
    {
        HideAllActionButtons();

        if (chapterContentPanel != null)
        {
            chapterContentPanel.SetActive(false);
        }

        gameObject.SetActive(false);

        if (panelToOpen == null)
            return;

        if (gameDataManager != null && gameDataManager.PanelNavigationManager != null)
        {
            gameDataManager.PanelNavigationManager.OpenPanel(panelToOpen);
        }
        else
        {
            panelToOpen.SetActive(true);
        }
    }

    private void SaveStoryState()
    {
        if (gameDataManager == null)
            return;

        gameDataManager.SavePlayerProfile();
    }
}
