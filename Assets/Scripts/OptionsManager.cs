using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Gestiona las opciones del juego, incluyendo la música y efectos de sonido.
/// Se integra con el PanelNavigationManager para controlar la reproducción de música.
/// </summary>
public class OptionsManager : MonoBehaviour
{
    [Header("Configuración de Audio")]
    [Tooltip("Fuente de audio para la música")]
    [SerializeField] private AudioSource musicSource;

    [Tooltip("Mixer Group de salida para la música (opcional)")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    
    [Tooltip("Lista de pistas de música para reproducir aleatoriamente")]
    [SerializeField] private List<AudioClip> musicTracks = new List<AudioClip>();
    
    [Header("UI Elements")]
    [Tooltip("Botón para alternar la música")]
    [SerializeField] private Button musicToggleButton;
    
    [Tooltip("Imagen del botón de música cuando está activada")]
    [SerializeField] private Sprite musicOnSprite;
    
    [Tooltip("Imagen del botón de música cuando está desactivada")]
    [SerializeField] private Sprite musicOffSprite;
    
    [Header("Referencias")]
    [Tooltip("Referencia al PanelNavigationManager")]
    [SerializeField] private PanelNavigationManager panelNavigationManager;
    
    [Header("UI - Panel de Confirmación de Reinicio")]
    [Tooltip("Panel de confirmación de reinicio")]
    [SerializeField] private GameObject resetConfirmationPanel;
    
    [Tooltip("Botón que abre el panel de confirmación de reinicio")]
    [SerializeField] private Button resetButton;
    
    [Tooltip("Botón de confirmar reinicio (dentro del panel)")]
    [SerializeField] private Button confirmResetButton;
    
    [Tooltip("Botón para cancelar el reinicio (dentro del panel)")]
    [SerializeField] private Button cancelResetButton;

    private const string PlayerProfileKey = "PlayerProfileData";
    
    // Estado de la música
    private bool isMusicEnabled = true;
    private int currentTrackIndex = -1;
    
    // Singleton
    public static OptionsManager Instance { get; private set; }
    
    private void Awake()
    {
        // Implementar patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Inicializar la fuente de audio si no está asignada
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = false;
                musicSource.playOnAwake = false;
            }

            // Asegurar que la música tenga prioridad alta para que no sea “culled” por límite de voces
            // (en Unity, menor número = más prioridad)
            musicSource.priority = 0;
            musicSource.spatialBlend = 0f;

            if (musicMixerGroup != null)
            {
                musicSource.outputAudioMixerGroup = musicMixerGroup;
            }
            
            // Cargar preferencias guardadas
            LoadPreferences();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Configurar el botón de música
        if (musicToggleButton != null)
        {
            musicToggleButton.onClick.AddListener(ToggleMusic);
            UpdateMusicButtonVisuals();
        }
        
        // Configurar el botón de reinicio
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ShowFullResetConfirmation);
        }
        
        // Configurar botones del panel de confirmación
        if (confirmResetButton != null)
        {
            confirmResetButton.onClick.AddListener(ConfirmFullReset);
        }
        
        if (cancelResetButton != null)
        {
            cancelResetButton.onClick.AddListener(HideFullResetConfirmation);
        }
        
        // Asegurarse de que el panel de confirmación esté desactivado al inicio
        if (resetConfirmationPanel != null)
        {
            resetConfirmationPanel.SetActive(false);
        }
        
        // Iniciar la reproducción de música si está habilitada
        if (isMusicEnabled && musicTracks.Count > 0)
        {
            PlayRandomTrack();
        }
        
        // Suscribirse a los eventos del PanelNavigationManager
        if (panelNavigationManager != null)
        {
            panelNavigationManager.OnPanelOpened += OnPanelOpened;
            panelNavigationManager.OnPanelClosed += OnPanelClosed;
        }
        else
        {
            Debug.LogWarning("PanelNavigationManager no asignado en OptionsManager");
        }
    }
    
    private void OnDestroy()
    {
        // Desuscribirse de los eventos al destruir el objeto
        if (panelNavigationManager != null)
        {
            panelNavigationManager.OnPanelOpened -= OnPanelOpened;
            panelNavigationManager.OnPanelClosed -= OnPanelClosed;
        }
    }
    
    private void Update()
    {
        // Reproducir la siguiente canción si la actual terminó
        if (isMusicEnabled && !musicSource.isPlaying && musicTracks.Count > 0)
        {
            PlayNextTrack();
        }
    }
    
    /// <summary>
    /// Alterna el estado de la música (activar/desactivar)
    /// </summary>
    public void ToggleMusic()
    {
        isMusicEnabled = !isMusicEnabled;
        
        if (isMusicEnabled)
        {
            // Si se está activando la música, reproducir una pista
            if (musicSource != null && !musicSource.isPlaying && musicTracks.Count > 0)
            {
                PlayRandomTrack();
            }
        }
        else
        {
            // Si se está desactivando la música, detener la reproducción
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }
        
        // Actualizar la interfaz de usuario
        UpdateMusicButtonVisuals();
        
        // Guardar preferencias
        SavePreferences();
    }
    
    /// <summary>
    /// Reproduce una pista de música aleatoria
    /// </summary>
    private void PlayRandomTrack()
    {
        if (musicTracks.Count == 0 || !isMusicEnabled) return;
        
        int newIndex;
        do
        {
            newIndex = Random.Range(0, musicTracks.Count);
        } 
        while (musicTracks.Count > 1 && newIndex == currentTrackIndex);
        
        currentTrackIndex = newIndex;
        PlayTrack(currentTrackIndex);
    }
    
    /// <summary>
    /// Reproduce la siguiente pista en la lista
    /// </summary>
    private void PlayNextTrack()
    {
        if (musicTracks.Count == 0 || !isMusicEnabled) return;
        
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Count;
        PlayTrack(currentTrackIndex);
    }
    
    /// <summary>
    /// Reproduce una pista específica por su índice
    /// </summary>
    private void PlayTrack(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= musicTracks.Count || musicTracks[trackIndex] == null) 
            return;
            
        musicSource.clip = musicTracks[trackIndex];
        musicSource.Play();
    }
    
    /// <summary>
    /// Actualiza los elementos visuales del botón de música
    /// </summary>
    private void UpdateMusicButtonVisuals()
    {
        if (musicToggleButton != null && musicToggleButton.image != null)
        {
            musicToggleButton.image.sprite = isMusicEnabled ? musicOnSprite : musicOffSprite;
        }
    }
    
    /// <summary>
    /// Se llama cuando se abre un panel
    /// </summary>
    private void OnPanelOpened(GameObject panel)
    {
        // Si se abre un panel y la música está habilitada pero no se está reproduciendo, iniciar reproducción
        if (isMusicEnabled && (musicSource == null || !musicSource.isPlaying) && musicTracks.Count > 0)
        {
            PlayRandomTrack();
        }
    }
    
    /// <summary>
    /// Se llama cuando se cierra un panel
    /// </summary>
    private void OnPanelClosed(GameObject panel)
    {
        // No necesitamos hacer nada especial al cerrar un panel
        // La música seguirá reproduciéndose a menos que se desactive explícitamente
    }
    
    /// <summary>
    /// Carga las preferencias guardadas del jugador
    /// </summary>
    private void LoadPreferences()
    {
        isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
    }
    
    /// <summary>
    /// Guarda las preferencias del jugador
    /// </summary>
    private void SavePreferences()
    {
        PlayerPrefs.SetInt("MusicEnabled", isMusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Agrega una pista de música a la lista de reproducción
    /// </summary>
    public void AddMusicTrack(AudioClip track)
    {
        if (track != null && !musicTracks.Contains(track))
        {
            musicTracks.Add(track);
            
            // Si no hay música reproduciéndose y está habilitada, comenzar a reproducir
            if (isMusicEnabled && (musicSource == null || !musicSource.isPlaying))
            {
                PlayRandomTrack();
            }
        }
    }
    
    /// <summary>
    /// Establece el volumen de la música
    /// </summary>
    /// <param name="volume">Nivel de volumen (0-1)</param>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }
    
    /// <summary>
    /// Obtiene el estado actual de la música
    /// </summary>
    public bool IsMusicEnabled()
    {
        return isMusicEnabled;
    }

    private void ShowFullResetConfirmation()
    {
        if (resetConfirmationPanel != null)
        {
            resetConfirmationPanel.SetActive(true);
        }
    }

    private void HideFullResetConfirmation()
    {
        if (resetConfirmationPanel != null)
        {
            resetConfirmationPanel.SetActive(false);
        }
    }

    private void ConfirmFullReset()
    {
        try
        {
            PlayerPrefs.DeleteKey(PlayerProfileKey);
            PlayerPrefs.Save();

            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.LoadPlayerProfile(silent: false);

                // Reset completo de objetos: inventario + equipo.
                // La forja se alimenta del inventario/equipo, así que quedará vacía al refrescar.
                InventoryManager inventoryManager = GameDataManager.Instance.InventoryManager;
                if (inventoryManager != null)
                {
                    inventoryManager.ClearInventory();
                    inventoryManager.NotifyInventoryChanged();
                }

                EquipmentManager equipmentManager = GameDataManager.Instance.EquipmentManager;
                if (equipmentManager != null)
                {
                    equipmentManager.ClearAllEquippedItems();
                }

                // Persistir el estado vacío.
                GameDataManager.Instance.SavePlayerProfile();
            }
        }
        finally
        {
            HideFullResetConfirmation();
        }
    }
}
