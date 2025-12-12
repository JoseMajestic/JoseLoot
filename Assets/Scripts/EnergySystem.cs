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
                Debug.LogError($"[ENERGY DEBUG] ⚠️⚠️⚠️ VALOR SOSPECHOSO {energy} DETECTADO EN GetCurrentEnergy ⚠️⚠️⚠️");
                Debug.LogError($"[ENERGY DEBUG] profile.currentEnergy = {energy}, isSleeping = {profile.isSleeping}");
                Debug.LogError($"[ENERGY DEBUG] Corrigiendo a 0 (valor por defecto cuando no está durmiendo y hay corrupción)");
                Debug.LogError($"[ENERGY DEBUG] StackTrace: {System.Environment.StackTrace}");
                
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
                Debug.LogWarning($"[ENERGY DEBUG] Valor sospechoso {energy} detectado mientras duerme. Podría ser válido o corrupto.");
            }
        }
        
        // Validar rango (0-100 es válido, no corregir valores válidos)
        if (energy < 0 || energy > MAX_ENERGY)
        {
            Debug.LogError($"[ENERGY DEBUG] Valor de energía fuera de rango: {energy}. Corrigiendo a 0.");
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
