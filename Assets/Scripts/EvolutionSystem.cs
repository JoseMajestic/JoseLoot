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
    /// Obtiene el título de clase basado en el nivel del héroe.
    /// Sistema épico con 12 fases progresivas (120 títulos totales).
    /// </summary>
    public static string GetClassNameByLevel(int heroLevel)
    {
        // Determinar fase según nivel
        int phase = GetPhaseByLevel(heroLevel);
        
        // Obtener array de títulos de la fase
        string[] phaseTitles = GetPhaseTitles(phase);
        
        if (phaseTitles == null || phaseTitles.Length == 0)
            return "Esclavo de la Arena";
        
        // Calcular índice dentro de la fase para progresión determinista
        // Cada fase tiene 10 títulos, distribuimos según el nivel dentro del rango
        int minLevel = GetPhaseMinLevel(phase);
        int maxLevel = GetPhaseMaxLevel(phase);
        int levelRange = maxLevel - minLevel + 1;
        int levelInPhase = heroLevel - minLevel;
        
        // Calcular índice (0-9) basado en la posición dentro de la fase
        int titleIndex = Mathf.Clamp(
            Mathf.FloorToInt((float)levelInPhase / (levelRange / 10f)), 
            0, 
            phaseTitles.Length - 1
        );
        
        return phaseTitles[titleIndex];
    }
    
    /// <summary>
    /// Determina la fase según el nivel del héroe.
    /// </summary>
    private static int GetPhaseByLevel(int heroLevel)
    {
        if (heroLevel >= 901) return 12; // FASE XII
        if (heroLevel >= 801) return 11; // FASE XI
        if (heroLevel >= 701) return 10; // FASE X
        if (heroLevel >= 601) return 9;  // FASE IX
        if (heroLevel >= 501) return 8;  // FASE VIII
        if (heroLevel >= 401) return 7;  // FASE VII
        if (heroLevel >= 301) return 6;  // FASE VI
        if (heroLevel >= 201) return 5;  // FASE V
        if (heroLevel >= 151) return 4;  // FASE IV
        if (heroLevel >= 101) return 3;  // FASE III
        if (heroLevel >= 51) return 2;   // FASE II
        return 1; // FASE I
    }
    
    /// <summary>
    /// Obtiene el nivel mínimo de una fase.
    /// </summary>
    private static int GetPhaseMinLevel(int phase)
    {
        switch (phase)
        {
            case 1: return 1;
            case 2: return 51;
            case 3: return 101;
            case 4: return 151;
            case 5: return 201;
            case 6: return 301;
            case 7: return 401;
            case 8: return 501;
            case 9: return 601;
            case 10: return 701;
            case 11: return 801;
            case 12: return 901;
            default: return 1;
        }
    }
    
    /// <summary>
    /// Obtiene el nivel máximo de una fase.
    /// </summary>
    private static int GetPhaseMaxLevel(int phase)
    {
        switch (phase)
        {
            case 1: return 50;
            case 2: return 100;
            case 3: return 150;
            case 4: return 200;
            case 5: return 300;
            case 6: return 400;
            case 7: return 500;
            case 8: return 600;
            case 9: return 700;
            case 10: return 800;
            case 11: return 900;
            case 12: return 999;
            default: return 50;
        }
    }
    
    /// <summary>
    /// Obtiene los títulos de una fase específica.
    /// </summary>
    private static string[] GetPhaseTitles(int phase)
    {
        switch (phase)
        {
            case 1: // FASE I · INICIACIÓN (1-50)
                return new string[]
                {
                    "Esclavo de la Arena",
                    "Recluta del Polvo",
                    "Portador de Cadenas",
                    "Luchador Anónimo",
                    "Carne para el Combate",
                    "Aspirante del Ludo",
                    "Guerrero Sin Nombre",
                    "Forjado en Sangre Ajena",
                    "Superviviente del Primer Día",
                    "Soldado de Nadie"
                };
            
            case 2: // FASE II · FORJA (51-100)
                return new string[]
                {
                    "Combatiente Reconocido",
                    "Veterano del Ludo",
                    "Portador del Acero",
                    "Guerrero Persistente",
                    "Afilado por la Arena",
                    "Sangre Consagrada",
                    "Dominador del Combate",
                    "Luchador Implacable",
                    "Azote de Novatos",
                    "Acero con Voluntad"
                };
            
            case 3: // FASE III · DISCIPLINA (101-150)
                return new string[]
                {
                    "Centinela del Ludo",
                    "Ejecutor de la Arena",
                    "Discípulo del Hierro",
                    "Estratega del Combate",
                    "Guardián del Orden Sangriento",
                    "Legionario del Dolor",
                    "Portador de Disciplina",
                    "Veterano Indomable",
                    "Columna de la Arena",
                    "Guerrero de Hierro Frío"
                };
            
            case 4: // FASE IV · AUTORIDAD (151-200)
                return new string[]
                {
                    "Decano del Combate",
                    "Instructor de Muerte",
                    "Voz de la Arena",
                    "Mano Ejecutora",
                    "Centurión del Ludo",
                    "Comandante del Polvo",
                    "Arquitecto de Victorias",
                    "Supervisor de Masacres",
                    "Señor del Ritmo Bélico",
                    "Estratega Consagrado"
                };
            
            case 5: // FASE V · GUERRA ABIERTA (201-300)
                return new string[]
                {
                    "Cazador Infernal",
                    "Verdugo de Demonios",
                    "Rompelegiones",
                    "Destructor del Abismo",
                    "Azote del Infierno",
                    "Matador de Engendros",
                    "Purificador de Carne Negra",
                    "Guerrero Antiinfernal",
                    "Ejecutor del Caos",
                    "Martillo del Averno"
                };
            
            case 6: // FASE VI · LEYENDA (301-400)
                return new string[]
                {
                    "Campeón de la Arena",
                    "Campeón del Fuego",
                    "Terror del Abismo",
                    "Espada Viviente",
                    "Mito en Progreso",
                    "Leyenda Encadenada",
                    "Guerrero Irreductible",
                    "Matalegiones",
                    "Señor de las Cicatrices",
                    "Portador de Historias Sangrientas"
                };
            
            case 7: // FASE VII · DOMINIO (401-500)
                return new string[]
                {
                    "Dominador del Combate",
                    "Maestro de la Guerra",
                    "General del Ludo",
                    "Arquitecto de Masacres",
                    "Soberano de la Arena",
                    "Señor del Acero",
                    "Portador del Miedo",
                    "Voluntad Encarnada",
                    "Emperador Sin Trono",
                    "Verdad del Combate"
                };
            
            case 8: // FASE VIII · SUPREMACÍA (501-600)
                return new string[]
                {
                    "Flagelo Infernal",
                    "Destructor de Hordas",
                    "Azote del Vacío",
                    "Aniquilador de Legiones",
                    "Muerte Organizada",
                    "Criterio del Combate",
                    "Juicio de Sangre",
                    "Espada del Fin",
                    "Castigo Viviente",
                    "Condena del Averno"
                };
            
            case 9: // FASE IX · MITO ABSOLUTO (601-700)
                return new string[]
                {
                    "Mito Inmortal",
                    "Ley Viviente",
                    "Símbolo de Guerra",
                    "Apocalipsis Controlado",
                    "Portador del Final",
                    "Terror Sistemático",
                    "Verbo del Combate",
                    "Último Argumento",
                    "Ruina Andante",
                    "Dominio Hecho Carne"
                };
            
            case 10: // FASE X · TRANSCENDENCIA (701-800)
                return new string[]
                {
                    "Voluntad Inhumana",
                    "Acero Consciente",
                    "Concepto de Guerra",
                    "Encarnación del Conflicto",
                    "Mente Bélica Absoluta",
                    "Forma Final del Combate",
                    "Idea de la Muerte",
                    "Arquetipo del Guerrero",
                    "Instrumento del Fin",
                    "Principio de la Destrucción"
                };
            
            case 11: // FASE XI · APOCALIPSIS (801-900)
                return new string[]
                {
                    "Presagio de Ruina",
                    "Cataclismo Dirigido",
                    "Colapso Personificado",
                    "Destructor de Eras",
                    "Fin de las Legiones",
                    "Silencio Tras la Batalla",
                    "Último General",
                    "Ancla del Apocalipsis",
                    "Guerra Permanente",
                    "Punto Sin Retorno"
                };
            
            case 12: // FASE XII · FINAL (901-999)
                return new string[]
                {
                    "Señor del Fin",
                    "Veredicto Final",
                    "Muerte Organizada",
                    "Emperador del Apocalipsis",
                    "Último Gladiador",
                    "Final de la Arena",
                    "Dominio Absoluto",
                    "Guerra Hecha Voluntad",
                    "Fin de Todo Conflicto",
                    "Liberado por la Sangre"
                };
            
            default:
                return new string[] { "Esclavo de la Arena" };
        }
    }
    
    /// <summary>
    /// Obtiene el nombre de la clase de evolución (método legacy, mantiene compatibilidad).
    /// Ahora usa el nivel del héroe en lugar de evolutionClass.
    /// </summary>
    public static string GetClassName(int evolutionClass)
    {
        // NOTA: Este método ahora está deprecado pero se mantiene por compatibilidad.
        // El nuevo sistema usa GetClassNameByLevel() basado en el nivel del héroe.
        // Por ahora, retornamos un título por defecto si se llama con evolutionClass.
        // Si se necesita usar el nivel, se debe llamar a GetClassNameByLevel() directamente.
        return "Esclavo de la Arena";
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

