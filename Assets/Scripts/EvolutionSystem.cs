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
    /// </summary>
    /// <summary>
    /// Obtiene el nombre de clase basado en el nivel del héroe.
    /// Usa los 200 títulos proporcionados por el usuario (cada 5 niveles).
    /// </summary>
    public static string GetClassNameByLevel(int heroLevel)
    {
        // 200 títulos para niveles 0-999 (cada 5 niveles)
        string[] allTitles = new string[]
        {
            "Novato", "Aprendiz", "Recluta", "Miliciano", "Discípulo",
            "Aspirante", "Auxiliar", "Vigía", "Explorador", "Custodio",
            "Soldado", "Infante", "Hastatus", "Princeps", "Triarius",
            "Legionario", "Lancero", "Escudero", "Ballestero", "Veterano",
            "Decanus", "Optio", "Signifer", "Cornicen", "Tesserarius",
            "Vexillarius", "Imaginifer", "Aquilifer", "Beneficiarius", "Evocatus",
            "Centinela", "Guardián", "Protector", "Defensor", "Combatiente",
            "Gladiador", "Duelista", "Campeador", "Paladín", "Adalid",
            "Decurión", "Suboficial", "Instructor", "Armero", "Estratega",
            "Capitán", "Comandante", "Maestre", "Custos", "Praesidium",
            "Centurión", "Centurión Mayor", "Primus Ordo", "Primus Pilus", "Tribuno Menor",
            "Tribuno Militar", "Tribuno Mayor", "Prefecto", "Prefecto Castrense", "Legado",
            "Legatus Legionis", "Dux", "Comes", "Gobernador", "Procónsul",
            "Magistrado", "Censor", "Edil", "Cuestor", "Senador",
            "Senador Mayor", "Patricio", "Noble", "Aristócrata", "Estratego",
            "Gran Capitán", "General", "General Supremo", "Alto Mando", "Comandante Supremo",
            "Magister Militum", "Magister Peditum", "Magister Equitum", "Vicario", "Corrector",
            "Consular", "Proconsular", "Regente", "Protector Imperii", "Voz del Senado",
            "Mano del Imperio", "Heraldo Imperial", "Campeón Imperial", "Custodio Imperial", "Alto Prelado",
            "Pontífice", "Pontífice Máximo", "Dictator", "Caesar", "Imperator",
            "Imperator Primus", "Imperator Magnus", "Imperator Invictus", "Dominus", "Dominus Belli",
            "Dominus Imperii", "Señor de la Guerra", "Señor del Imperio", "Regente Supremo", "Emperador",
            "Emperador Mayor", "Emperador Supremo", "Emperador Eterno", "Soberano", "Soberano Absoluto",
            "Padre del Imperio", "Voz de los Dioses", "Mano de Roma", "Trono Viviente", "Ley del Imperio",
            "Fundador", "Restaurador", "Unificador", "Pacificador", "Conquistador",
            "Legislador", "Arquitecto", "Estratega Supremo", "Protector del Orden", "Guardián del Imperio",
            "Custodio del Senado", "Custodio del Pueblo", "Custodio de Roma", "Señor de las Legiones", "Amo del Estandarte",
            "Amo del Trono", "Regente del Mundo", "Regente de Roma", "Voz del Imperator", "Mano del Trono",
            "Heredero", "Sucesor", "Primogénito", "Elegido", "Consagrado",
            "Ascendente", "Magistratus Maximus", "Imperium Viviente", "Pilar del Imperio", "Símbolo de Roma",
            "Ley Viviente", "Voluntad del Imperio", "Voluntad del Senado", "Voluntad de Roma", "Voluntad Divina",
            "Campeón de los Dioses", "Elegido de Marte", "Elegido de Júpiter", "Hijo de Roma", "Padre de las Legiones",
            "Fundamento del Orden", "Guardián del Mundo", "Regente Eterno", "Custodio del Tiempo", "Portador del Imperium",
            "Portador del Trono", "Portador de la Ley", "Portador de Roma", "Voz del Mundo", "Voz de la Historia",
            "Eje del Imperio", "Corazón de Roma", "Alma del Imperio", "Destino Viviente", "Destino de Roma",
            "Destino del Mundo", "Testamento Imperial", "Crónica Viviente", "Legado Supremo", "Legado Eterno",
            "Mito", "Símbolo", "Pilar del Mundo", "Guardián del Destino", "Custodio de la Eternidad",
            "Voluntad Absoluta", "Ley Absoluta", "Imperium Absolutum", "Trono Absoluto", "Dominio Total",
            "Principio", "Autoridad Final", "Último Bastión", "Última Ley", "Último Emperador",
            "Emperador del Tiempo", "Emperador del Mundo", "Emperador de Roma", "Imperio Viviente", "Roma Aeterna"
        };
        
        // Calcular el índice del título (cada 5 niveles)
        int titleIndex = heroLevel / 5;
        
        // Asegurarse de que no exceda el array
        if (titleIndex >= allTitles.Length)
            titleIndex = allTitles.Length - 1;
        
        return allTitles[titleIndex];
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
                    "Novato",
                    "Aprendiz",
                    "Recluta",
                    "Miliciano",
                    "Discípulo",
                    "Aspirante",
                    "Auxiliar",
                    "Vigía",
                    "Explorador",
                    "Custodio"
                };
            
            case 2: // FASE II · FORJA (51-100)
                return new string[]
                {
                    "Soldado",
                    "Infante",
                    "Hastatus",
                    "Princeps",
                    "Triarius",
                    "Legionario",
                    "Lancero",
                    "Escudero",
                    "Ballestero",
                    "Veterano"
                };
            
            case 3: // FASE III · DISCIPLINA (101-150)
                return new string[]
                {
                    "Decanus",
                    "Optio",
                    "Signifer",
                    "Cornicen",
                    "Tesserarius",
                    "Vexillarius",
                    "Imaginifer",
                    "Aquilifer",
                    "Beneficiarius",
                    "Evocatus"
                };
            
            case 4: // FASE IV · AUTORIDAD (151-200)
                return new string[]
                {
                    "Centinela",
                    "Guardián",
                    "Protector",
                    "Defensor",
                    "Combatiente",
                    "Gladiador",
                    "Duelista",
                    "Campeador",
                    "Paladín",
                    "Adalid"
                };
            
            case 5: // FASE V · GUERRA ABIERTA (201-300)
                return new string[]
                {
                    "Decurión",
                    "Suboficial",
                    "Instructor",
                    "Armero",
                    "Estratega",
                    "Capitán",
                    "Comandante",
                    "Maestre",
                    "Custos",
                    "Praesidium"
                };
            
            case 6: // FASE VI · LEYENDA (301-400)
                return new string[]
                {
                    "Centurión",
                    "Centurión Mayor",
                    "Primus Ordo",
                    "Primus Pilus",
                    "Tribuno Menor",
                    "Tribuno Militar",
                    "Tribuno Mayor",
                    "Prefecto",
                    "Prefecto Castrense",
                    "Legado"
                };
            
            case 7: // FASE VII · DOMINIO (401-500)
                return new string[]
                {
                    "Legatus Legionis",
                    "Dux",
                    "Comes",
                    "Gobernador",
                    "Procónsul",
                    "Magistrado",
                    "Censor",
                    "Edil",
                    "Cuestor",
                    "Senador"
                };
            
            case 8: // FASE VIII · SUPREMACÍA (501-600)
                return new string[]
                {
                    "Senador Mayor",
                    "Patricio",
                    "Noble",
                    "Aristócrata",
                    "Estratego",
                    "Gran Capitán",
                    "General",
                    "General Supremo",
                    "Alto Mando",
                    "Comandante Supremo"
                };
            
            case 9: // FASE IX · MITO ABSOLUTO (601-700)
                return new string[]
                {
                    "Magister Militum",
                    "Magister Peditum",
                    "Magister Equitum",
                    "Vicario",
                    "Corrector",
                    "Consular",
                    "Proconsular",
                    "Regente",
                    "Protector Imperii",
                    "Voz del Senado"
                };
            
            case 10: // FASE X · TRANSCENDENCIA (701-800)
                return new string[]
                {
                    "Mano del Imperio",
                    "Heraldo Imperial",
                    "Campeón Imperial",
                    "Custodio Imperial",
                    "Alto Prelado",
                    "Pontífice",
                    "Pontífice Máximo",
                    "Dictator",
                    "Caesar",
                    "Imperator"
                };
            
            case 11: // FASE XI · APOCALIPSIS (801-900)
                return new string[]
                {
                    "Imperator Primus",
                    "Imperator Magnus",
                    "Imperator Invictus",
                    "Dominus",
                    "Dominus Belli",
                    "Dominus Imperii",
                    "Señor de la Guerra",
                    "Señor del Imperio",
                    "Regente Supremo",
                    "Emperador"
                };
            
            case 12: // FASE XII · FINAL (901-999)
                return new string[]
                {
                    "Emperador Mayor",
                    "Emperador Supremo",
                    "Emperador Eterno",
                    "Soberano",
                    "Soberano Absoluto",
                    "Padre del Imperio",
                    "Voz de los Dioses",
                    "Mano de Roma",
                    "Trono Viviente",
                    "Ley del Imperio"
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
            int levelsNeeded = requiredLevel - heroLevel;
            string nextClassTitle = GetClassNameByLevel(requiredLevel);
            
            if (levelsNeeded == 1)
            {
                return $"Sube 1 nivel más para evolucionar a {nextClassTitle}";
            }
            else
            {
                return $"Sube {levelsNeeded} niveles más para evolucionar a {nextClassTitle}";
            }
        }
        
        float hoursRemaining = GetTimeUntilEvolution(lastEvolutionTime);
        if (hoursRemaining > 0f)
        {
            return $"Espera {hoursRemaining:F1} horas más";
        }
        
        return "¡Puedes evolucionar!";
    }
}

