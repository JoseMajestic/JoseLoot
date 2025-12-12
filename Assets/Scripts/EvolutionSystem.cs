using System;
using UnityEngine;

/// <summary>
/// Sistema de evolución del héroe.
/// Requisitos: cooldown de evolución (tiempo real) + nivel de jugador mínimo.
/// Al evolucionar: cambia el título (clase: Primera, Segunda, Tercera, etc.).
/// </summary>
public static class EvolutionSystem
{
    // Nombres de las clases de evolución
    private static readonly string[] ClassNames = new string[]
    {
        "Primera",
        "Segunda",
        "Tercera",
        "Cuarta",
        "Quinta",
        "Sexta",
        "Séptima",
        "Octava",
        "Novena",
        "Décima"
    };
    
    // Cooldown de evolución en horas (tiempo real)
    private const float EVOLUTION_COOLDOWN_HOURS = 24f; // 24 horas por defecto
    
    // Nivel mínimo del héroe requerido para cada clase
    // Clase 0 (Primera) no requiere nivel, Clase 1 requiere nivel 5, etc.
    private static int GetRequiredHeroLevel(int evolutionClass)
    {
        return evolutionClass * 5; // Clase 1 = nivel 5, Clase 2 = nivel 10, etc.
    }
    
    /// <summary>
    /// Obtiene el nombre de la clase de evolución.
    /// </summary>
    public static string GetClassName(int evolutionClass)
    {
        if (evolutionClass < 0 || evolutionClass >= ClassNames.Length)
            return $"Clase {evolutionClass + 1}";
        
        return ClassNames[evolutionClass];
    }
    
    /// <summary>
    /// Verifica si el héroe puede evolucionar.
    /// Requisitos: cooldown completado + nivel de héroe suficiente.
    /// </summary>
    public static bool CanEvolve(int currentEvolutionClass, int heroLevel, DateTime lastEvolutionTime)
    {
        // Verificar nivel del héroe
        int requiredLevel = GetRequiredHeroLevel(currentEvolutionClass + 1);
        if (heroLevel < requiredLevel)
        {
            return false;
        }
        
        // Verificar cooldown (tiempo real)
        if (lastEvolutionTime == DateTime.MinValue)
        {
            // Si nunca evolucionó, puede evolucionar a la primera clase
            return currentEvolutionClass == 0;
        }
        
        TimeSpan timeSinceEvolution = DateTime.Now - lastEvolutionTime;
        return timeSinceEvolution.TotalHours >= EVOLUTION_COOLDOWN_HOURS;
    }
    
    /// <summary>
    /// Obtiene el tiempo restante hasta la próxima evolución (en horas).
    /// </summary>
    public static float GetTimeUntilEvolution(DateTime lastEvolutionTime)
    {
        if (lastEvolutionTime == DateTime.MinValue)
            return 0f; // Puede evolucionar inmediatamente
        
        TimeSpan timeSinceEvolution = DateTime.Now - lastEvolutionTime;
        float hoursRemaining = EVOLUTION_COOLDOWN_HOURS - (float)timeSinceEvolution.TotalHours;
        
        return Mathf.Max(0f, hoursRemaining);
    }
    
    /// <summary>
    /// Formatea el tiempo restante hasta la evolución.
    /// </summary>
    public static string FormatEvolutionCooldown(DateTime lastEvolutionTime)
    {
        float hoursRemaining = GetTimeUntilEvolution(lastEvolutionTime);
        
        if (hoursRemaining <= 0f)
        {
            return "¡LISTO!";
        }
        
        int days = Mathf.FloorToInt(hoursRemaining / 24f);
        int hours = Mathf.FloorToInt(hoursRemaining % 24f);
        int minutes = Mathf.FloorToInt((hoursRemaining % 1f) * 60f);
        int seconds = Mathf.FloorToInt(((hoursRemaining % 1f) * 60f % 1f) * 60f);
        
        if (days > 0)
        {
            return $"{days:00}:{hours:00}:{minutes:00}:{seconds:00}";
        }
        else if (hours > 0)
        {
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
        else
        {
            return $"{minutes:00}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// Obtiene el mensaje de requisitos para evolucionar.
    /// </summary>
    public static string GetEvolutionRequirements(int currentEvolutionClass, int heroLevel, DateTime lastEvolutionTime)
    {
        int nextClass = currentEvolutionClass + 1;
        int requiredLevel = GetRequiredHeroLevel(nextClass);
        
        if (heroLevel < requiredLevel)
        {
            return $"Nivel {requiredLevel} requerido (actual: {heroLevel})";
        }
        
        float hoursRemaining = GetTimeUntilEvolution(lastEvolutionTime);
        if (hoursRemaining > 0f)
        {
            return $"Espera {hoursRemaining:F1} horas más";
        }
        
        return "¡Puedes evolucionar!";
    }
}

