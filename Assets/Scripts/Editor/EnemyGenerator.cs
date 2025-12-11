using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Script Editor que genera automáticamente 18 enemigos predefinidos desde el menú Tools.
/// Los enemigos están balanceados por nivel de dificultad.
/// </summary>
public class EnemyGenerator : EditorWindow
{
    [MenuItem("Tools/Combate/Generar 18 Enemigos Predefinidos")]
    public static void GenerateEnemies()
    {
        // Crear carpeta si no existe
        string folderPath = "Assets/Enemies";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parentFolder = "Assets";
            string folderName = "Enemies";
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        int createdCount = 0;

        // NIVEL 1 - FÁCIL (Enemigos 1-3)
        CreateEnemy("Goblin Débil", "Un goblin pequeño y débil, fácil de derrotar. Perfecto para principiantes.", 
                     1, 0, 80, 8, 3, 8, 2, 3, 1, 2, 30, 10);
        CreateEnemy("Esqueleto Novato", "Un esqueleto recién reanimado. Sus huesos crujen con cada movimiento.", 
                     1, 0, 70, 10, 4, 7, 1, 2, 0, 1, 35, 12);
        CreateEnemy("Lobo Pequeño", "Un lobo joven pero ágil. Ataca con rapidez pero tiene poca resistencia.", 
                     1, 0, 90, 9, 2, 12, 3, 4, 2, 3, 40, 15);
        createdCount += 3;

        // NIVEL 2 - FÁCIL-MEDIO (Enemigos 4-6)
        CreateEnemy("Orco Guerrero", "Un orco entrenado en combate. Más fuerte que los goblins pero aún manejable.", 
                     2, 1, 120, 15, 6, 9, 3, 4, 1, 2, 60, 20);
        CreateEnemy("Araña Venenosa", "Una araña gigante con veneno letal. Sus mordeduras pueden ser peligrosas.", 
                     2, 1, 100, 12, 5, 11, 4, 5, 2, 3, 55, 18);
        CreateEnemy("Bandido", "Un bandido humano armado. Conoce algunas técnicas de combate básicas.", 
                     2, 1, 110, 14, 4, 10, 2, 3, 1, 2, 65, 22);
        createdCount += 3;

        // NIVEL 3 - MEDIO (Enemigos 7-9)
        CreateEnemy("Troll", "Un troll grande y resistente. Su piel gruesa lo hace difícil de dañar.", 
                     3, 2, 180, 20, 10, 7, 5, 6, 2, 3, 90, 30);
        CreateEnemy("Guerrero Esqueleto", "Un esqueleto con armadura y experiencia en combate. Más peligroso que sus parientes novatos.", 
                     3, 2, 160, 22, 8, 9, 4, 5, 1, 2, 85, 28);
        CreateEnemy("Ogro", "Un ogro masivo con fuerza bruta. Sus golpes pueden ser devastadores.", 
                     3, 2, 200, 18, 12, 6, 6, 7, 2, 3, 95, 32);
        createdCount += 3;

        // NIVEL 4 - MEDIO-ALTO (Enemigos 10-12)
        CreateEnemy("Demonio Menor", "Un demonio del inframundo. Sus ataques están imbuidos de energía oscura.", 
                     4, 3, 250, 28, 10, 11, 7, 8, 3, 4, 120, 40);
        CreateEnemy("Caballero Oscuro", "Un caballero caído que sirve a las fuerzas oscuras. Combina fuerza y defensa.", 
                     4, 3, 220, 30, 12, 10, 6, 7, 2, 3, 130, 42);
        CreateEnemy("Dragón Pequeño", "Un dragón joven pero ya formidable. Sus escamas protegen y sus garras desgarran.", 
                     4, 3, 280, 25, 9, 13, 8, 9, 4, 5, 140, 45);
        createdCount += 3;

        // NIVEL 5 - ALTO (Enemigos 13-15)
        CreateEnemy("Lich", "Un hechicero inmortal que domina la magia oscura. Extremadamente peligroso.", 
                     5, 4, 350, 35, 15, 12, 10, 11, 5, 6, 180, 50);
        CreateEnemy("Gigante de Hielo", "Un gigante ancestral del norte. Su cuerpo helado es casi indestructible.", 
                     5, 4, 400, 32, 18, 8, 12, 13, 3, 4, 200, 55);
        CreateEnemy("Vampiro", "Un vampiro anciano con siglos de experiencia. Rápido, fuerte y astuto.", 
                     5, 4, 320, 38, 14, 15, 11, 12, 6, 7, 190, 52);
        createdCount += 3;

        // NIVEL 6 - MUY ALTO (Enemigos 16-18)
        CreateEnemy("Dragón Anciano", "Un dragón milenario con poder inmenso. Uno de los enemigos más temibles.", 
                     6, 5, 500, 45, 20, 14, 15, 16, 8, 9, 250, 60);
        CreateEnemy("Señor de las Sombras", "El gobernante de las tinieblas. Su poder oscuro es casi ilimitado.", 
                     6, 5, 450, 48, 18, 16, 14, 15, 7, 8, 280, 65);
        CreateEnemy("Boss Final", "El enemigo definitivo. Combina todas las habilidades y es el más poderoso de todos.", 
                     6, 5, 600, 50, 25, 10, 18, 20, 10, 12, 300, 70);
        createdCount += 3;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✓ Se generaron {createdCount} enemigos en {folderPath}");
        EditorUtility.DisplayDialog("Enemigos Generados", 
            $"Se crearon {createdCount} enemigos exitosamente en:\n{folderPath}\n\nLos enemigos están organizados por nivel de dificultad (1-6).", "OK");
    }

    /// <summary>
    /// Crea un ScriptableObject EnemyData con los parámetros especificados.
    /// </summary>
    private static void CreateEnemy(string enemyName, string description, int level, int requiredLevel,
                                   int hp, int ataque, int defensa, int velocidadAtaque,
                                   int ataqueCritico, int danoCritico, int suerte, int destreza,
                                   int rewardCoins, int experienceReward)
    {
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = description;
        enemy.level = level;
        enemy.requiredLevel = requiredLevel;
        enemy.hp = hp;
        enemy.ataque = ataque;
        enemy.defensa = defensa;
        enemy.velocidadAtaque = velocidadAtaque;
        enemy.ataqueCritico = ataqueCritico;
        enemy.danoCritico = danoCritico;
        enemy.suerte = suerte;
        enemy.destreza = destreza;
        enemy.rewardCoins = rewardCoins;
        enemy.experienceReward = experienceReward;
        
        // Array de ataques vacío (se puede asignar después desde el Inspector)
        enemy.availableAttacks = new AttackData[0];

        // Nombre del archivo (sin espacios y caracteres especiales)
        string fileName = enemyName.Replace(" ", "_").Replace("é", "e").Replace("ó", "o");
        string path = $"Assets/Enemies/{fileName}.asset";
        
        // Si el archivo ya existe, agregar número
        int counter = 1;
        while (File.Exists(path))
        {
            path = $"Assets/Enemies/{fileName}_{counter}.asset";
            counter++;
        }

        AssetDatabase.CreateAsset(enemy, path);
    }
}

