using UnityEngine;

/// <summary>
/// Sistema de títulos basado en stats de crianza.
/// Similar al HTML, determina el título según cómo se cría al héroe.
/// </summary>
public static class TitleSystem
{
    // Títulos por tipo de crianza (similar al HTML)
    private static readonly string[] BalancedTitles = new string[]
    {
        "Ciudadano Ejemplar",
        "Ser Equilibrado",
        "Alma Armónica",
        "Ente Completo",
        "Espíritu Sereno"
    };
    
    private static readonly string[] HappyTitles = new string[]
    {
        "Alma Radiante",
        "Ser Luminoso",
        "Espíritu Alegre",
        "Luz Digital",
        "Monarca Feliz"
    };
    
    private static readonly string[] WorkerTitles = new string[]
    {
        "Monarca Digital",
        "Código Supremo",
        "Arquitecto Binario",
        "Mente Productiva",
        "Genio Laboral"
    };
    
    private static readonly string[] DisciplinedTitles = new string[]
    {
        "Guardián Eterno",
        "Centinela Digital",
        "Orden Absoluto",
        "Voluntad Férrea",
        "Maestro Estoico"
    };
    
    private static readonly string[] AthleteTitles = new string[]
    {
        "Titán Digital",
        "Coloso Binario",
        "Fuerza Infinita",
        "Guerrero Pixel",
        "Campeón Eterno"
    };
    
    private static readonly string[] ScholarTitles = new string[]
    {
        "Sabio Ancestral",
        "Oráculo Digital",
        "Mente Infinita",
        "Conocedor Eterno",
        "Erudito Supremo"
    };
    
    private static readonly string[] NeutralTitles = new string[]
    {
        "Viajero Digital",
        "Ente Errante",
        "Pixel Solitario",
        "Código Flotante",
        "Existencia Pasiva"
    };
    
    // Títulos perturbadores (descuido prolongado)
    private static readonly string[] NeglectedTitles = new string[]
    {
        "Eco Vacío",
        "Sombra Olvidada",
        "Código Corrupto",
        "Error Fatal",
        "Entidad Rota"
    };
    
    private static readonly string[] AbandonedTitles = new string[]
    {
        "Alma Fragmentada",
        "Consciencia Perdida",
        "Grito Silencioso",
        "Vacío Eterno",
        "Olvido Digital"
    };
    
    private static readonly string[] CriticalTitles = new string[]
    {
        "Última Señal",
        "Núcleo Dañado",
        "Fin Inminente",
        "Existencia Frágil",
        "Susurro Final"
    };
    
    // Umbrales de descuido (en segundos)
    private const float NEGLECT_THRESHOLD = 300f;  // 5 minutos
    private const float ABANDON_THRESHOLD = 900f;   // 15 minutos
    private const float CRITICAL_THRESHOLD = 1800f; // 30 minutos
    
    /// <summary>
    /// Determina el tipo de título según los stats de crianza.
    /// </summary>
    public static string DetermineTitleType(int work, int hunger, int happiness, int energy, int hygiene, int discipline, float neglectTime)
    {
        // Verificar descuido primero (títulos perturbadores)
        if (neglectTime >= CRITICAL_THRESHOLD)
        {
            return "critical";
        }
        else if (neglectTime >= ABANDON_THRESHOLD)
        {
            return "abandoned";
        }
        else if (neglectTime >= NEGLECT_THRESHOLD)
        {
            return "neglected";
        }
        
        // Calcular promedios
        float avgPositive = (happiness + energy + hygiene + hunger) / 4f;
        
        // Determinar tipo según stats dominantes
        if (happiness >= 80 && avgPositive >= 70)
        {
            return "happy";
        }
        else if (work >= 80 && discipline >= 60)
        {
            return "worker";
        }
        else if (discipline >= 80)
        {
            return "disciplined";
        }
        else if (work >= 70 && energy >= 70)
        {
            return "athlete";
        }
        else if (work >= 60 && discipline >= 50 && happiness >= 50)
        {
            return "scholar";
        }
        else if (avgPositive >= 60)
        {
            return "balanced";
        }
        else
        {
            return "neutral";
        }
    }
    
    /// <summary>
    /// Obtiene un título aleatorio del tipo especificado.
    /// </summary>
    public static string GetTitleFromType(string titleType, int evolutionClass)
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
            return "Viajero Digital";
        
        // Usar la clase de evolución para seleccionar títulos más "avanzados"
        int index = Mathf.Min(Mathf.FloorToInt(evolutionClass / 5f), titles.Length - 1);
        return titles[index];
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

