using System;
using UnityEngine;

/// <summary>
/// Sistema de energía del héroe.
/// La energía NO decae automáticamente, solo se descarga manualmente (gimnasio o combate).
/// Se recupera durmiendo (4 horas tiempo real para llegar de 0% a 100%).
/// Mientras está durmiendo, la energía se recupera automáticamente.
/// Si se gasta energía (combate o mejora) mientras duerme, se despierta automáticamente.
/// </summary>
public class EnergySystem : MonoBehaviour
{
    private const int MAX_ENERGY = 100;
    private const float RECOVERY_TIME_HOURS = 04f; // 4 horas para recuperar completamente (de 0% a 100%)
    private const float RECOVERY_RATE_PER_SECOND = MAX_ENERGY / (RECOVERY_TIME_HOURS * 3600f); // Energía recuperada por segundo
    private const double MAX_OFFLINE_RECOVERY_SECONDS = 60d * 60d * 24d; // Limitar a 24h de progreso por vez
    private const float ENERGY_SAVE_INTERVAL = 1f;
    
    public event Action<int, int, bool> OnEnergyChanged;
    
    private GameDataManager gameDataManager;
    private float saveTimer = 0f;
    
    private void Start()
    {
        gameDataManager = GameDataManager.Instance;
        
        if (gameDataManager == null)
        {
            Debug.LogError("EnergySystem: GameDataManager no encontrado.");
            return;
        }
        
        ApplyRecoveryInternal(resetTimer: true);
    }
    
    private void Update()
    {
        if (gameDataManager == null)
            return;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        if (profile.isSleeping)
        {
            saveTimer += Time.deltaTime;
        }
        else
        {
            saveTimer = 0f;
        }
        
        ApplyRecoveryInternal(resetTimer: false, profile);
    }
    
    /// <summary>
    /// Obtiene la energía actual del héroe.
    /// Valida y corrige valores sospechosos (48, 49) que pueden ser corruptos.
    /// NO corrige valores válidos como 0 (después de reset) o valores normales.
    /// </summary>
    public int GetCurrentEnergy()
    {
        if (gameDataManager == null)
            return 0; // Cambiar de MAX_ENERGY a 0 como valor por defecto
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return 0; // Cambiar de MAX_ENERGY a 0 como valor por defecto
        
        int energy = profile.currentEnergy;
        
        // SOLUCIÓN CRÍTICA: Validar y corregir valores sospechosos (48, 49)
        // Estos valores son típicamente corruptos y no tienen sentido lógico
        // NO corregir valores válidos como 0 (después de reset)
        if (energy == 48 || energy == 49)
        {
            // Si no está durmiendo, estos valores son definitivamente corruptos
            if (!profile.isSleeping)
            {
                // Corregir el valor corrupto a 0 (no a 100)
                profile.currentEnergy = 0;
                profile.isSleeping = false;
                
                // Guardar el valor corregido
                gameDataManager.SavePlayerProfile();
                
                return 0;
            }
            // Si está durmiendo, 48 o 49 podrían ser válidos (recuperación parcial)
            // Pero es sospechoso, así que lo registramos
            else
            {
            }
        }
        
        // Validar rango (0-100 es válido, no corregir valores válidos)
        if (energy < 0 || energy > MAX_ENERGY)
        {
            profile.currentEnergy = 0; // Cambiar de MAX_ENERGY a 0
            gameDataManager.SavePlayerProfile();
            return 0;
        }
        
        return energy;
    }
    
    /// <summary>
    /// Obtiene la energía máxima del héroe.
    /// </summary>
    public int GetMaxEnergy()
    {
        return MAX_ENERGY;
    }
    
    /// <summary>
    /// Verifica si hay suficiente energía para un coste.
    /// </summary>
    public bool CanAfford(int cost)
    {
        return GetCurrentEnergy() >= cost;
    }
    
    /// <summary>
    /// Gasta energía (gimnasio o combate).
    /// Si está durmiendo, lo despierta automáticamente.
    /// </summary>
    public bool SpendEnergy(int amount)
    {
        if (gameDataManager == null)
            return false;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return false;
        
        // Si está durmiendo, despertar automáticamente
        if (profile.isSleeping)
        {
            profile.isSleeping = false;
            profile.SaveEnergyTimestamp(DateTime.UtcNow);
        }
        
        if (profile.currentEnergy < amount)
        {
            Debug.LogWarning($"EnergySystem: No hay suficiente energía. Actual: {profile.currentEnergy}, Requerida: {amount}");
            return false;
        }
        
        profile.currentEnergy -= amount;
        profile.currentEnergy = Mathf.Clamp(profile.currentEnergy, 0, MAX_ENERGY);
        
        // Guardar cambios
        gameDataManager.SavePlayerProfile();
        
        return true;
    }
    
    /// <summary>
    /// Inicia el sueño (el héroe comienza a recuperar energía automáticamente).
    /// La energía se recuperará completamente en 4 horas (de 0% a 100%).
    /// </summary>
    public void StartSleeping()
    {
        if (gameDataManager == null)
            return;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Activar estado de sueño
        profile.isSleeping = true;
        
        // Guardar fecha/hora actual (para recuperación offline)
        DateTime now = DateTime.UtcNow;
        profile.SaveLastSleepTime(now);
        profile.SaveEnergyTimestamp(now);
        
        // Guardar cambios
        gameDataManager.SavePlayerProfile();
    }
    
    /// <summary>
    /// Despierta al héroe manualmente (si está durmiendo).
    /// </summary>
    public void WakeUp()
    {
        if (gameDataManager == null)
            return;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        if (profile.isSleeping)
        {
            profile.isSleeping = false;
            profile.SaveEnergyTimestamp(DateTime.UtcNow);
            gameDataManager.SavePlayerProfile();
        }
    }
    
    /// <summary>
    /// Verifica si el héroe está durmiendo.
    /// </summary>
    public bool IsSleeping()
    {
        if (gameDataManager == null)
            return false;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return false;
        
        return profile.isSleeping;
    }
    
    /// <summary>
    /// Aplica la recuperación de energía usando el reloj del sistema.
    /// </summary>
    public void ApplyRecovery(bool resetTimer = false)
    {
        ApplyRecoveryInternal(resetTimer, null);
    }
    
    private void ApplyRecoveryInternal(bool resetTimer, PlayerProfileData cachedProfile = null)
    {
        if (gameDataManager == null)
            return;
        
        PlayerProfileData profile = cachedProfile ?? gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        DateTime now = DateTime.UtcNow;
        DateTime lastUpdate = profile.GetLastEnergyUpdate();
        
        if (lastUpdate == DateTime.MinValue || resetTimer)
        {
            profile.SaveEnergyTimestamp(now);
            lastUpdate = now;
        }
        
        if (!profile.isSleeping)
        {
            profile.SaveEnergyTimestamp(now);
            if (resetTimer)
            {
                gameDataManager.SavePlayerProfile();
            }
            return;
        }
        
        double elapsedSeconds = Math.Max(0d, (now - lastUpdate).TotalSeconds);
        if (elapsedSeconds <= 0d)
            return;
        
        double secondsToProcess = Math.Min(elapsedSeconds, MAX_OFFLINE_RECOVERY_SECONDS);
        float energyGain = (float)(secondsToProcess * RECOVERY_RATE_PER_SECOND);
        
        float newEnergyValue = profile.currentEnergy + energyGain;
        int roundedEnergy = Mathf.Clamp(Mathf.RoundToInt(newEnergyValue), 0, MAX_ENERGY);
        bool energyChanged = roundedEnergy != profile.currentEnergy;
        profile.currentEnergy = roundedEnergy;
        
        if (profile.currentEnergy >= MAX_ENERGY)
        {
            profile.currentEnergy = MAX_ENERGY;
            profile.isSleeping = false;
        }
        
        profile.SaveEnergyTimestamp(now);
        
        if (resetTimer || energyChanged || profile.currentEnergy >= MAX_ENERGY || saveTimer >= ENERGY_SAVE_INTERVAL)
        {
            gameDataManager.SavePlayerProfile();
            saveTimer = 0f;
        }
    }
}
