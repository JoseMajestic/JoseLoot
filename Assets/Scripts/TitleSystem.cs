using UnityEngine;

/// <summary>
/// Sistema de títulos basado en stats de crianza.
/// Similar al HTML, determina el título según cómo se cría al héroe.
/// </summary>
public static class TitleSystem
{
    // Títulos por tipo de crianza (sistema épico de gladiador)
    private static readonly string[] BalancedTitles = new string[]
    {
        "Gladiador Íntegro",
        "Campeón en Equilibrio",
        "Guerrero Armónico",
        "Paladín Digital",
        "Custodio del Núcleo",
        "Centurión Estable",
        "Maestro del Balance",
        "Luchador Completo",
        "Titán Sereno",
        "Campeón Sin Fisuras"
    };
    
    private static readonly string[] HappyTitles = new string[]
    {
        "Gladiador Radiante",
        "Campeón de la Arena",
        "Guerrero Victorioso",
        "Paladín Eufórico",
        "Titán Triunfante",
        "Señor de la Gloria",
        "Luchador Exaltado",
        "Campeón Luminoso",
        "Emperador Jubiloso",
        "Héroe Celebrado"
    };
    
    private static readonly string[] WorkerTitles = new string[]
    {
        "Gladiador Incansable",
        "Obrero de la Arena",
        "Guerrero de Hierro",
        "Forjador Digital",
        "Centurión Productivo",
        "Titán del Esfuerzo",
        "Campeón del Deber",
        "Luchador Persistente",
        "Maestro del Ritmo",
        "Arquitecto de Victoria"
    };
    
    private static readonly string[] DisciplinedTitles = new string[]
    {
        "Gladiador Implacable",
        "Centinela de Acero",
        "Guerrero Estoico",
        "Paladín del Orden",
        "Titán de la Voluntad",
        "Campeón Inquebrantable",
        "Luchador Marcial",
        "Custodio de la Disciplina",
        "Maestro del Control",
        "Ejecutador Perfecto"
    };
    
    private static readonly string[] AthleteTitles = new string[]
    {
        "Gladiador Colosal",
        "Titán de la Arena",
        "Guerrero Bestial",
        "Campeón del Combate",
        "Coloso Digital",
        "Luchador Supremacista",
        "Paladín del Poder",
        "Destructor Binario",
        "Señor del Impacto",
        "Máquina de Batalla"
    };
    
    private static readonly string[] ScholarTitles = new string[]
    {
        "Gladiador Estratega",
        "Maestro de la Arena",
        "Oráculo del Combate",
        "Guerrero Táctico",
        "Paladín del Conocimiento",
        "Arquitecto de Guerra",
        "Titán Mental",
        "Campeón Analítico",
        "Señor del Cálculo",
        "General Digital"
    };
    
    private static readonly string[] NeutralTitles = new string[]
    {
        "Gladiador Errante",
        "Luchador Sin Nombre",
        "Guerrero en Tránsito",
        "Combatiente Gris",
        "Alma de la Arena",
        "Vagabundo Digital",
        "Espada sin Rumbo",
        "Campeón Anónimo",
        "Soldado Pasivo",
        "Existencia Marcial"
    };
    
    // Títulos perturbadores (descuido prolongado)
    private static readonly string[] NeglectedTitles = new string[]
    {
        "Gladiador Deteriorado",
        "Guerrero Desgastado",
        "Titán Agrietado",
        "Campeón en Declive",
        "Luchador Averiado",
        "Armadura Rota",
        "Sombra de la Arena",
        "Paladín Fatigado",
        "Núcleo Inestable",
        "Combatiente Corrupto"
    };
    
    private static readonly string[] AbandonedTitles = new string[]
    {
        "Gladiador Caído",
        "Guerrero Olvidado",
        "Campeón Destronado",
        "Titán Fragmentado",
        "Luchador Sin Honor",
        "Alma de Hierro Rota",
        "Espectro de la Arena",
        "Paladín Perdido",
        "Ruina Viviente",
        "Excombatiente"
    };
    
    private static readonly string[] CriticalTitles = new string[]
    {
        "Gladiador al Límite",
        "Guerrero Terminal",
        "Último Combatiente",
        "Núcleo Colapsado",
        "Campeón Moribundo",
        "Titán en Extinción",
        "Luchador Agónico",
        "Fin de la Arena",
        "Existencia Letal",
        "Última Batalla"
    };
    
    // Umbrales de descuido (en segundos)
    private const float NEGLECT_THRESHOLD = 300f;  // 5 minutos
    private const float ABANDON_THRESHOLD = 900f;   // 15 minutos
    private const float CRITICAL_THRESHOLD = 1800f; // 30 minutos
    
    /// <summary>
    /// Calcula el índice del Estado General (0-99) basado en las stats de crianza.
    /// </summary>
    public static int CalculateGeneralStateIndex(int work, int hunger, int happiness, int energy, int hygiene, int discipline)
    {
        // Calcular promedio ponderado (Hambre y Energía pesan más)
        float weightedWork = work * 1.0f;
        float weightedHunger = hunger * 1.5f;
        float weightedHappiness = happiness * 1.0f;
        float weightedEnergy = energy * 1.5f;
        float weightedHygiene = hygiene * 1.0f;
        float weightedDiscipline = discipline * 1.0f;
        
        float totalWeight = 7.0f; // 1.0 + 1.5 + 1.0 + 1.5 + 1.0 + 1.0
        float weightedAverage = (weightedWork + weightedHunger + weightedHappiness + 
                                weightedEnergy + weightedHygiene + weightedDiscipline) / totalWeight;
        
        // Convertir a índice 0-99
        int stateIndex = Mathf.Clamp(Mathf.RoundToInt(weightedAverage), 0, 99);
        return stateIndex;
    }
    
    /// <summary>
    /// Detecta el parámetro dominante (más alto).
    /// </summary>
    public static string DetectDominantParameter(int work, int hunger, int happiness, int energy, int hygiene, int discipline)
    {
        int maxValue = work;
        string dominant = "work";
        
        if (hunger > maxValue) { maxValue = hunger; dominant = "hunger"; }
        if (happiness > maxValue) { maxValue = happiness; dominant = "happiness"; }
        if (energy > maxValue) { maxValue = energy; dominant = "energy"; }
        if (hygiene > maxValue) { maxValue = hygiene; dominant = "hygiene"; }
        if (discipline > maxValue) { maxValue = discipline; dominant = "discipline"; }
        
        return dominant;
    }
    
    /// <summary>
    /// Detecta si hay carencias graves (algún parámetro < 20).
    /// </summary>
    public static bool HasCriticalDeficiencies(int work, int hunger, int happiness, int energy, int hygiene, int discipline)
    {
        return work < 20 || hunger < 20 || happiness < 20 || energy < 20 || hygiene < 20 || discipline < 20;
    }
    
    /// <summary>
    /// Determina el tipo de especialización según el parámetro dominante.
    /// </summary>
    private static string DetermineSpecializationType(string dominantParam, int work, int hunger, int happiness, int energy, int hygiene, int discipline)
    {
        switch (dominantParam)
        {
            case "happiness":
                return "happy";
            
            case "work":
                // Si trabajo es dominante, verificar si hay disciplina alta para "worker" o energía alta para "athlete"
                if (discipline >= 60)
                    return "worker";
                else if (energy >= 70)
                    return "athlete";
                else
                    return "worker"; // Por defecto worker
            
            case "discipline":
                return "disciplined";
            
            case "energy":
                // Si energía es dominante, verificar si hay trabajo alto para "athlete"
                if (work >= 70)
                    return "athlete";
                else
                    return "athlete"; // Por defecto athlete
            
            case "hygiene":
                // Higiene no tiene arquetipo específico, usar balanced o neutral
                return "balanced";
            
            case "hunger":
                // Hambre no tiene arquetipo específico, usar balanced o neutral
                return "balanced";
            
            default:
                // Si hay trabajo, disciplina y felicidad en niveles razonables, usar "scholar"
                if (work >= 60 && discipline >= 50 && happiness >= 50)
                    return "scholar";
                else
                    return "balanced";
        }
    }
    
    /// <summary>
    /// Determina el tipo de título según el nuevo sistema de Estado General Index.
    /// </summary>
    public static string DetermineTitleType(int work, int hunger, int happiness, int energy, int hygiene, int discipline, float neglectTime)
    {
        // Calcular Estado General Index (0-99)
        int stateIndex = CalculateGeneralStateIndex(work, hunger, happiness, energy, hygiene, discipline);
        
        // Detectar parámetros dominantes
        string dominantParam = DetectDominantParameter(work, hunger, happiness, energy, hygiene, discipline);
        bool hasCriticalDeficiencies = HasCriticalDeficiencies(work, hunger, happiness, energy, hygiene, discipline);
        
        // Determinar macroestado por rango
        if (stateIndex >= 0 && stateIndex <= 9)
        {
            // Crítico (0-9)
            return "critical";
        }
        else if (stateIndex >= 10 && stateIndex <= 19)
        {
            // Abandonado (10-19)
            return "abandoned";
        }
        else if (stateIndex >= 20 && stateIndex <= 29)
        {
            // Descuido (20-29)
            return "neglected";
        }
        else if (stateIndex >= 30 && stateIndex <= 44)
        {
            // Neutral (30-44)
            return "neutral";
        }
        else if (stateIndex >= 45 && stateIndex <= 59)
        {
            // Especialización (45-59) - elegir por parámetro dominante
            return DetermineSpecializationType(dominantParam, work, hunger, happiness, energy, hygiene, discipline);
        }
        else if (stateIndex >= 60 && stateIndex <= 74)
        {
            // Especialización Alta (60-74) - elegir por parámetro dominante
            return DetermineSpecializationType(dominantParam, work, hunger, happiness, energy, hygiene, discipline);
        }
        else if (stateIndex >= 75 && stateIndex <= 89)
        {
            // Balanceado (75-89) - solo si no hay carencias graves
            if (!hasCriticalDeficiencies)
            {
                return "balanced";
            }
            else
            {
                // Si hay carencias graves, usar especialización
                return DetermineSpecializationType(dominantParam, work, hunger, happiness, energy, hygiene, discipline);
            }
        }
        else // stateIndex >= 90 && stateIndex <= 99
        {
            // Óptimo (90-99) - solo si no hay carencias graves
            if (!hasCriticalDeficiencies)
            {
                return "balanced";
            }
            else
            {
                // Si hay carencias graves, usar especialización
                return DetermineSpecializationType(dominantParam, work, hunger, happiness, energy, hygiene, discipline);
            }
        }
    }
    
    /// <summary>
    /// Obtiene un título del tipo especificado.
    /// La selección ahora usa el Estado General Index para gradación fina.
    /// </summary>
    public static string GetTitleFromType(string titleType, int evolutionClass, int generalStateIndex = 50)
    {
        string[] titles = null;
        
        switch (titleType)
        {
            case "balanced":
                titles = BalancedTitles;
                break;
            case "happy":
                titles = HappyTitles;
                break;
            case "worker":
                titles = WorkerTitles;
                break;
            case "disciplined":
                titles = DisciplinedTitles;
                break;
            case "athlete":
                titles = AthleteTitles;
                break;
            case "scholar":
                titles = ScholarTitles;
                break;
            case "neglected":
                titles = NeglectedTitles;
                break;
            case "abandoned":
                titles = AbandonedTitles;
                break;
            case "critical":
                titles = CriticalTitles;
                break;
            default:
                titles = NeutralTitles;
                break;
        }
        
        if (titles == null || titles.Length == 0)
            return "Gladiador Errante";
        
        // Seleccionar título basado en el índice dentro del arquetipo
        // Usar el Estado General Index dentro del rango del macroestado para gradación fina
        // Distribuir los 10 títulos del arquetipo según el índice (0-99)
        // Cada título cubre aproximadamente 10 puntos del índice (0-9, 10-19, 20-29, etc.)
        int titleIndex = Mathf.Clamp(
            Mathf.FloorToInt((float)generalStateIndex / (100f / titles.Length)), 
            0, 
            titles.Length - 1
        );
        
        return titles[titleIndex];
    }
    
    /// <summary>
    /// Verifica si el héroe está siendo descuidado.
    /// Descuido: 2 o más stats en estado crítico (<= 20 para felicidad, <= 15 para otros).
    /// </summary>
    public static bool IsBeingNeglected(int happiness, int energy, int hygiene, int hunger)
    {
        int criticalCount = 0;
        
        if (happiness <= 20) criticalCount++;
        if (energy <= 15) criticalCount++;
        if (hygiene <= 15) criticalCount++;
        if (hunger <= 15) criticalCount++;
        
        return criticalCount >= 2;
    }
}

