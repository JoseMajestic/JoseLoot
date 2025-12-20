using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Visualizador de espectro de audio tipo ecualizador.
/// Muestra barras que suben y bajan según el audio que se reproduce.
/// </summary>
public class AudioSpectrumVisualizer : MonoBehaviour
{
    [Header("Configuración de Audio")]
    [Tooltip("AudioSource que reproducirá los sonidos")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Mixer Group de salida para las voces (opcional)")]
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup voiceMixerGroup;
    
    [Tooltip("Pool de sonidos que se pueden reproducir")]
    [SerializeField] private AudioClip[] audioClips;
    
    [Header("Barras del Visualizador")]
    [Tooltip("Array de Sliders que representan las barras del ecualizador")]
    [SerializeField] private Slider[] spectrumBars;
    
    [Tooltip("Número de muestras del espectro (debe ser potencia de 2: 64, 128, 256, 512, 1024)")]
    [SerializeField] private int spectrumSize = 64;
    
    [Header("Configuración Visual")]
    [Tooltip("Multiplicador de altura de las barras (ajusta la sensibilidad)")]
    [SerializeField] private float barHeightMultiplier = 50f;
    
    [Tooltip("Suavizado de las barras (0 = sin suavizado, 1 = máximo suavizado)")]
    [Range(0f, 1f)]
    [SerializeField] private float smoothing = 0.5f;
    
    [Tooltip("Velocidad de actualización del visualizador (frames por segundo)")]
    [SerializeField] private int updateRate = 30;
    
    [Header("Panel")]
    [Tooltip("Panel que contiene el visualizador (siempre visible)")]
    [SerializeField] private GameObject visualizerPanel;
    
    [Header("Vibración en Reposo")]
    [Tooltip("Rango mínimo de vibración cuando no hay sonido (0.01 = 1%)")]
    [SerializeField] private float idleVibrationMin = 0.01f;
    
    [Tooltip("Rango máximo de vibración cuando no hay sonido (0.05 = 5%)")]
    [SerializeField] private float idleVibrationMax = 0.05f;
    
    [Tooltip("Velocidad de la vibración en reposo")]
    [SerializeField] private float idleVibrationSpeed = 2f;
    
    // Datos del espectro
    private float[] spectrumData;
    private float[] smoothedSpectrumData;
    
    // Control de actualización
    private float updateInterval;
    private float lastUpdateTime;
    
    // Estado
    private bool isVisualizing = false;
    private bool isPlayingSound = false;
    private Coroutine visualizationCoroutine;
    private Coroutine idleVibrationCoroutine;
    
    // Valores de vibración guardados
    private float[] idleBarValues; // Guarda los valores de vibración actuales de cada barra
    private bool hasIdleValues = false; // Indica si tenemos valores de vibración guardados
    
    // Estado de activación
    private bool isEnabled = true; // Controla si el visualizador está habilitado
    
    private void Start()
    {
        // Inicializar arrays
        spectrumData = new float[spectrumSize];
        smoothedSpectrumData = new float[spectrumSize];
        
        // Calcular intervalo de actualización
        updateInterval = 1f / updateRate;
        
        // Crear AudioSource si no existe
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Configuración recomendada para que las voces no corten la música por límite de voces
        // (en Unity, menor número = más prioridad)
        audioSource.priority = 200;
        audioSource.spatialBlend = 0f;

        if (voiceMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = voiceMixerGroup;
        }
        
        // Inicializar barras
        InitializeBars();
        
        // El panel se activará/desactivará según el estado del panel Breed
        // No iniciar vibración aquí, se iniciará cuando se habilite el visualizador
        if (visualizerPanel != null)
        {
            visualizerPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Inicializa las barras del visualizador.
    /// </summary>
    private void InitializeBars()
    {
        if (spectrumBars == null || spectrumBars.Length == 0)
            return;
        
        foreach (Slider bar in spectrumBars)
        {
            if (bar != null)
            {
                bar.minValue = 0f;
                bar.maxValue = 1f;
                bar.value = 0f;
            }
        }
    }
    
    /// <summary>
    /// Reproduce un sonido aleatorio del pool y activa el visualizador.
    /// </summary>
    public void PlayRandomSound()
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("AudioSpectrumVisualizer: No hay sonidos en el pool.");
            return;
        }
        
        // Seleccionar sonido aleatorio
        AudioClip clip = audioClips[Random.Range(0, audioClips.Length)];
        
        if (clip == null)
        {
            Debug.LogWarning("AudioSpectrumVisualizer: El clip seleccionado es null.");
            return;
        }
        
        PlaySound(clip);
    }
    
    /// <summary>
    /// Reproduce un sonido específico y activa el visualizador.
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;
        
        // Asegurar que existe AudioSource
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Detener cualquier sonido anterior que esté reproduciéndose
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Guardar los valores actuales de las barras antes de empezar el sonido
        SaveCurrentBarValues();
        
        // Detener vibración en reposo
        StopIdleVibration();
        
        // Marcar que estamos reproduciendo sonido
        isPlayingSound = true;
        
        // Reproducir sonido
        audioSource.clip = clip;
        audioSource.Play();
        
        // Iniciar visualización con espectro real
        StartVisualization();
    }
    
    /// <summary>
    /// Detiene el sonido y vuelve a la vibración en reposo.
    /// </summary>
    public void StopSound()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        // Guardar los valores actuales antes de volver a la vibración
        SaveCurrentBarValues();
        
        // Marcar que ya no estamos reproduciendo sonido
        isPlayingSound = false;
        
        // Detener visualización con espectro real
        StopVisualization();
        
        // Volver a la vibración en reposo
        StartIdleVibration();
    }
    
    /// <summary>
    /// Guarda los valores actuales de las barras para transición suave.
    /// </summary>
    private void SaveCurrentBarValues()
    {
        if (spectrumBars == null || spectrumBars.Length == 0)
            return;
        
        // Inicializar array si no existe
        if (idleBarValues == null || idleBarValues.Length != spectrumBars.Length)
        {
            idleBarValues = new float[spectrumBars.Length];
        }
        
        // Guardar valores actuales de cada barra
        for (int i = 0; i < spectrumBars.Length; i++)
        {
            if (spectrumBars[i] != null)
            {
                idleBarValues[i] = spectrumBars[i].value;
            }
        }
        
        hasIdleValues = true;
    }
    
    /// <summary>
    /// Inicia la visualización del espectro.
    /// </summary>
    private void StartVisualization()
    {
        if (isVisualizing)
            return;
        
        isVisualizing = true;
        visualizationCoroutine = StartCoroutine(UpdateSpectrumVisualization());
    }
    
    /// <summary>
    /// Detiene la visualización del espectro.
    /// </summary>
    private void StopVisualization()
    {
        isVisualizing = false;
        
        if (visualizationCoroutine != null)
        {
            StopCoroutine(visualizationCoroutine);
            visualizationCoroutine = null;
        }
    }
    
    /// <summary>
    /// Corrutina que actualiza el visualizador en tiempo real con espectro real.
    /// </summary>
    private IEnumerator UpdateSpectrumVisualization()
    {
        while (isVisualizing && isPlayingSound)
        {
            // Esperar intervalo de actualización
            yield return new WaitForSeconds(updateInterval);
            
            // Solo actualizar si el audio está reproduciéndose
            if (audioSource != null && audioSource.isPlaying)
            {
                UpdateSpectrum();
                UpdateBars();
            }
            else
            {
                // Si el audio terminó, volver a vibración en reposo
                StopSound();
                break;
            }
        }
    }
    
    /// <summary>
    /// Inicia la vibración en reposo (barras vibrando entre 1-5%).
    /// </summary>
    private void StartIdleVibration()
    {
        if (idleVibrationCoroutine != null)
            return;
        
        idleVibrationCoroutine = StartCoroutine(IdleVibration());
    }
    
    /// <summary>
    /// Detiene la vibración en reposo.
    /// </summary>
    private void StopIdleVibration()
    {
        if (idleVibrationCoroutine != null)
        {
            StopCoroutine(idleVibrationCoroutine);
            idleVibrationCoroutine = null;
        }
    }
    
    /// <summary>
    /// Corrutina que hace vibrar las barras entre 1-5% cuando no hay sonido.
    /// </summary>
    private IEnumerator IdleVibration()
    {
        // Si tenemos valores guardados, usarlos como punto de partida
        // Si no, inicializar valores aleatorios
        float[] baseValues = new float[spectrumBars != null ? spectrumBars.Length : 0];
        float[] targetValues = new float[spectrumBars != null ? spectrumBars.Length : 0];
        
        // Inicializar valores: usar valores guardados si existen, sino aleatorios
        for (int i = 0; i < baseValues.Length; i++)
        {
            if (hasIdleValues && idleBarValues != null && i < idleBarValues.Length)
            {
                // Usar el valor guardado como punto de partida
                baseValues[i] = idleBarValues[i];
            }
            else if (spectrumBars != null && i < spectrumBars.Length && spectrumBars[i] != null)
            {
                // Usar el valor actual de la barra
                baseValues[i] = spectrumBars[i].value;
            }
            else
            {
                // Inicializar aleatoriamente
                baseValues[i] = Random.Range(idleVibrationMin, idleVibrationMax);
            }
            
            targetValues[i] = Random.Range(idleVibrationMin, idleVibrationMax);
        }
        
        while (!isPlayingSound)
        {
            yield return new WaitForSeconds(updateInterval);
            
            if (spectrumBars == null || spectrumBars.Length == 0)
                continue;
            
            // Actualizar cada barra
            for (int i = 0; i < spectrumBars.Length && i < baseValues.Length; i++)
            {
                if (spectrumBars[i] == null)
                    continue;
                
                // Interpolar hacia el valor objetivo
                baseValues[i] = Mathf.Lerp(baseValues[i], targetValues[i], Time.deltaTime * idleVibrationSpeed);
                
                // Asegurar que el valor esté en el rango de vibración
                baseValues[i] = Mathf.Clamp(baseValues[i], idleVibrationMin, idleVibrationMax);
                
                // Actualizar barra
                spectrumBars[i].value = baseValues[i];
                
                // Guardar el valor actualizado para la próxima vez
                if (idleBarValues != null && i < idleBarValues.Length)
                {
                    idleBarValues[i] = baseValues[i];
                }
                
                // Si llegamos al objetivo, generar nuevo objetivo
                if (Mathf.Abs(baseValues[i] - targetValues[i]) < 0.001f)
                {
                    targetValues[i] = Random.Range(idleVibrationMin, idleVibrationMax);
                }
            }
        }
        
        idleVibrationCoroutine = null;
    }
    
    /// <summary>
    /// Obtiene los datos del espectro de audio.
    /// </summary>
    private void UpdateSpectrum()
    {
        if (audioSource == null)
            return;
        
        // Obtener datos del espectro (FFT)
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        
        // Aplicar suavizado
        for (int i = 0; i < spectrumSize; i++)
        {
            smoothedSpectrumData[i] = Mathf.Lerp(smoothedSpectrumData[i], spectrumData[i], 1f - smoothing);
        }
    }
    
    /// <summary>
    /// Actualiza las barras del visualizador según el espectro.
    /// </summary>
    private void UpdateBars()
    {
        if (spectrumBars == null || spectrumBars.Length == 0)
            return;
        
        int barsCount = spectrumBars.Length;
        
        // Distribuir los datos del espectro entre las barras
        for (int i = 0; i < barsCount; i++)
        {
            if (spectrumBars[i] == null)
                continue;
            
            // Calcular índice en el espectro para esta barra
            int spectrumIndex = Mathf.FloorToInt((float)i / barsCount * spectrumSize);
            spectrumIndex = Mathf.Clamp(spectrumIndex, 0, spectrumSize - 1);
            
            // Obtener valor del espectro y aplicar multiplicador
            float spectrumValue = smoothedSpectrumData[spectrumIndex] * barHeightMultiplier;
            
            // Normalizar a rango 0-1
            float normalizedSpectrumValue = Mathf.Clamp01(spectrumValue);
            
            // Obtener valor base de vibración (donde estaba la barra antes del sonido)
            float baseVibrationValue = 0f;
            if (hasIdleValues && idleBarValues != null && i < idleBarValues.Length)
            {
                baseVibrationValue = idleBarValues[i];
            }
            
            // Combinar: usar el máximo entre la vibración base y el espectro
            // Esto hace que las barras suban desde donde estaban, no desde 0
            float finalValue = Mathf.Max(baseVibrationValue, normalizedSpectrumValue);
            
            // Actualizar barra
            spectrumBars[i].value = finalValue;
        }
    }
    
    /// <summary>
    /// Resetea todas las barras a 0.
    /// </summary>
    private void ResetBars()
    {
        if (spectrumBars == null)
            return;
        
        foreach (Slider bar in spectrumBars)
        {
            if (bar != null)
            {
                bar.value = 0f;
            }
        }
    }
    
    /// <summary>
    /// Verifica si el visualizador está activo.
    /// </summary>
    public bool IsVisualizing()
    {
        return isVisualizing && audioSource != null && audioSource.isPlaying;
    }
    
    /// <summary>
    /// Habilita el visualizador (activa el panel y la vibración en reposo).
    /// </summary>
    public void EnableVisualizer()
    {
        isEnabled = true;
        
        // Activar panel
        if (visualizerPanel != null)
        {
            visualizerPanel.SetActive(true);
        }
        
        // Iniciar vibración en reposo si no hay sonido reproduciéndose
        if (!isPlayingSound)
        {
            StartIdleVibration();
        }
    }
    
    /// <summary>
    /// Deshabilita el visualizador (desactiva el panel, detiene sonidos y vibración).
    /// </summary>
    public void DisableVisualizer()
    {
        isEnabled = false;
        
        // Detener cualquier sonido que esté reproduciéndose
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Detener visualización
        StopVisualization();
        
        // Detener vibración en reposo
        StopIdleVibration();
        
        // Desactivar panel
        if (visualizerPanel != null)
        {
            visualizerPanel.SetActive(false);
        }
    }
}

