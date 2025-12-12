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
    private const float RECOVERY_TIME_HOURS = 4f; // 4 horas para recuperar completamente (de 0% a 100%)
    private const float RECOVERY_RATE_PER_SECOND = MAX_ENERGY / (RECOVERY_TIME_HOURS * 3600f); // Energía recuperada por segundo
    
    private GameDataManager gameDataManager;
    
    private void Start()
    {
        gameDataManager = GameDataManager.Instance;
        
        if (gameDataManager == null)
        {
            Debug.LogError("EnergySystem: GameDataManager no encontrado.");
            return;
        }
        
        // NOTA: Sistema offline desactivado por ahora para evitar problemas
        // CalculateOfflineRecovery();
    }
    
    private void Update()
    {
        if (gameDataManager == null)
            return;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return;
        
        // Si está durmiendo, recuperar energía automáticamente
        if (profile.isSleeping)
        {
            // Calcular energía a recuperar en este frame
            float energyToRecover = RECOVERY_RATE_PER_SECOND * Time.deltaTime;
            
            // Aplicar recuperación con redondeo
            float newEnergy = profile.currentEnergy + energyToRecover;
            
            // Redondear: si es >= 0.5, redondear hacia arriba; si es < 0.5, redondear hacia abajo
            int energyRounded = Mathf.RoundToInt(newEnergy);
            profile.currentEnergy = Mathf.Min(MAX_ENERGY, energyRounded);
            
            // Si llegó al 100%, despertar automáticamente
            if (profile.currentEnergy >= MAX_ENERGY)
            {
                profile.currentEnergy = MAX_ENERGY;
                profile.isSleeping = false;
                gameDataManager.SavePlayerProfile();
                Debug.Log("[ENERGY DEBUG] EnergySystem - Energía recuperada al 100%, héroe despertó automáticamente.");
            }
            else
            {
                // Guardar cambios periódicamente mientras duerme (cada segundo aproximadamente)
                if (Time.frameCount % 60 == 0)
                {
                    gameDataManager.SavePlayerProfile();
                }
            }
        }
    }
    
    /// <summary>
    /// Obtiene la energía actual del héroe.
    /// </summary>
    public int GetCurrentEnergy()
    {
        if (gameDataManager == null)
            return MAX_ENERGY;
        
        PlayerProfileData profile = gameDataManager.GetPlayerProfile();
        if (profile == null)
            return MAX_ENERGY;
        
        int energy = profile.currentEnergy;
        
        // DEBUG: Detectar específicamente el valor 49
        if (energy == 49)
        {
            Debug.LogError($"[ENERGY DEBUG] ⚠️⚠️⚠️ VALOR 49 DETECTADO EN GetCurrentEnergy ⚠️⚠️⚠️");
            Debug.LogError($"[ENERGY DEBUG] profile.currentEnergy = {energy}");
            Debug.LogError($"[ENERGY DEBUG] profile.isSleeping = {profile.isSleeping}");
            Debug.LogError($"[ENERGY DEBUG] profile.lastSleepTimeString = '{profile.lastSleepTimeString}'");
            Debug.LogError($"[ENERGY DEBUG] StackTrace: {System.Environment.StackTrace}");
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
            Debug.Log("[ENERGY DEBUG] EnergySystem.SpendEnergy - Héroe despertó porque se gastó energía.");
        }
        
        if (profile.currentEnergy < amount)
        {
            Debug.LogWarning($"EnergySystem: No hay suficiente energía. Actual: {profile.currentEnergy}, Requerida: {amount}");
            return false;
        }
        
        int energyBefore = profile.currentEnergy;
        profile.currentEnergy -= amount;
        profile.currentEnergy = Mathf.Clamp(profile.currentEnergy, 0, MAX_ENERGY);
        int energyAfter = profile.currentEnergy;
        
        Debug.Log($"[ENERGY DEBUG] SpendEnergy - ANTES: {energyBefore}, gastando: {amount}, DESPUÉS: {energyAfter}");
        
        // Guardar cambios
        gameDataManager.SavePlayerProfile();
        
        Debug.Log($"[ENERGY DEBUG] SpendEnergy - FINAL guardado: {profile.currentEnergy}");
        
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
        
        // Guardar fecha/hora actual (para futuro sistema offline)
        profile.SaveLastSleepTime();
        
        // Guardar cambios
        gameDataManager.SavePlayerProfile();
        
        Debug.Log($"[ENERGY DEBUG] EnergySystem.StartSleeping - Héroe comenzó a dormir. Energía actual: {profile.currentEnergy}%, se recuperará al 100% en 4 horas.");
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
            gameDataManager.SavePlayerProfile();
            Debug.Log($"[ENERGY DEBUG] EnergySystem.WakeUp - Héroe despertó manualmente. Energía actual: {profile.currentEnergy}%");
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
    
    // ===== MÉTODOS DESACTIVADOS (sistema offline desactivado por ahora) =====
    
    /// <summary>
    /// DESACTIVADO: Calcula la recuperación de energía basada en el tiempo offline.
    /// </summary>
    private void CalculateOfflineRecovery()
    {
        // Sistema offline desactivado por ahora
        return;
    }
    
    /// <summary>
    /// DESACTIVADO: Calcula cuánta energía se recuperará basada en el tiempo transcurrido desde que durmió.
    /// </summary>
    public int CalculateRecoveryFromSleep()
    {
        // Sistema offline desactivado por ahora
        return 0;
    }
    
    /// <summary>
    /// DESACTIVADO: Aplica la recuperación de energía calculada.
    /// </summary>
    public void ApplyRecovery()
    {
        // Sistema offline desactivado por ahora
        return;
    }
}
