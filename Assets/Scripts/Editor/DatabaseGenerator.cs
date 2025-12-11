using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// Script Editor que genera automáticamente las instancias de EnemyDatabase y AttackDatabase.
/// También puede llenarlas automáticamente con todos los ataques y enemigos encontrados.
/// </summary>
public class DatabaseGenerator : EditorWindow
{
    [MenuItem("Tools/Combate/Generar Bases de Datos (EnemyDatabase y AttackDatabase)")]
    public static void GenerateDatabases()
    {
        // Crear carpeta Databases si no existe
        string folderPath = "Assets/Databases";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parentFolder = "Assets";
            string folderName = "Databases";
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        bool attackDbCreated = false;
        bool enemyDbCreated = false;

        // 1. Crear AttackDatabase
        string attackDbPath = "Assets/Databases/Attack Database.asset";
        AttackDatabase attackDatabase = null;

        if (File.Exists(attackDbPath))
        {
            attackDatabase = AssetDatabase.LoadAssetAtPath<AttackDatabase>(attackDbPath);
            Debug.Log("Attack Database ya existe. Se actualizará automáticamente.");
        }
        else
        {
            attackDatabase = ScriptableObject.CreateInstance<AttackDatabase>();
            AssetDatabase.CreateAsset(attackDatabase, attackDbPath);
            attackDbCreated = true;
        }

        // 2. Crear EnemyDatabase
        string enemyDbPath = "Assets/Databases/Enemy Database.asset";
        EnemyDatabase enemyDatabase = null;

        if (File.Exists(enemyDbPath))
        {
            enemyDatabase = AssetDatabase.LoadAssetAtPath<EnemyDatabase>(enemyDbPath);
            Debug.Log("Enemy Database ya existe. Se actualizará automáticamente.");
        }
        else
        {
            enemyDatabase = ScriptableObject.CreateInstance<EnemyDatabase>();
            AssetDatabase.CreateAsset(enemyDatabase, enemyDbPath);
            enemyDbCreated = true;
        }

        // 3. Llenar AttackDatabase con todos los ataques encontrados
        string[] attackGuids = AssetDatabase.FindAssets("t:AttackData", new[] { "Assets/Attacks" });
        if (attackGuids.Length > 0)
        {
            AttackData[] allAttacks = new AttackData[attackGuids.Length];
            for (int i = 0; i < attackGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(attackGuids[i]);
                allAttacks[i] = AssetDatabase.LoadAssetAtPath<AttackData>(path);
            }

            // Usar SerializedObject para modificar el array privado
            SerializedObject so = new SerializedObject(attackDatabase);
            SerializedProperty attacksProperty = so.FindProperty("attacks");
            attacksProperty.arraySize = allAttacks.Length;
            for (int i = 0; i < allAttacks.Length; i++)
            {
                attacksProperty.GetArrayElementAtIndex(i).objectReferenceValue = allAttacks[i];
            }
            so.ApplyModifiedProperties();

            Debug.Log($"✓ Attack Database llenada con {allAttacks.Length} ataques.");
        }
        else
        {
            Debug.LogWarning("No se encontraron ataques en Assets/Attacks. Ejecuta primero 'Generar 21 Ataques Predefinidos'.");
        }

        // 4. Llenar EnemyDatabase con todos los enemigos encontrados
        string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyData", new[] { "Assets/Enemies" });
        if (enemyGuids.Length > 0)
        {
            EnemyData[] allEnemies = new EnemyData[enemyGuids.Length];
            for (int i = 0; i < enemyGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(enemyGuids[i]);
                allEnemies[i] = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            }

            // Usar SerializedObject para modificar el array privado
            SerializedObject so = new SerializedObject(enemyDatabase);
            SerializedProperty enemiesProperty = so.FindProperty("enemies");
            enemiesProperty.arraySize = allEnemies.Length;
            for (int i = 0; i < allEnemies.Length; i++)
            {
                enemiesProperty.GetArrayElementAtIndex(i).objectReferenceValue = allEnemies[i];
            }
            so.ApplyModifiedProperties();

            Debug.Log($"✓ Enemy Database llenada con {allEnemies.Length} enemigos.");
        }
        else
        {
            Debug.LogWarning("No se encontraron enemigos en Assets/Enemies. Ejecuta primero 'Generar 18 Enemigos Predefinidos'.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string message = "Bases de datos generadas:\n\n";
        message += $"✓ Attack Database: {attackDbPath}\n";
        message += $"✓ Enemy Database: {enemyDbPath}\n\n";
        
        if (attackDbCreated)
            message += "Attack Database creada.\n";
        if (enemyDbCreated)
            message += "Enemy Database creada.\n";
        
        message += "\nAhora puedes arrastrar estos assets a:\n";
        message += "- CombatManager > attackDatabase\n";
        message += "- BattleManager > enemyDatabase";

        EditorUtility.DisplayDialog("Bases de Datos Generadas", message, "OK");
    }

    [MenuItem("Tools/Combate/Actualizar Bases de Datos (Llenar con ataques/enemigos existentes)")]
    public static void UpdateDatabases()
    {
        // Buscar las bases de datos existentes
        string[] attackDbGuids = AssetDatabase.FindAssets("t:AttackDatabase");
        string[] enemyDbGuids = AssetDatabase.FindAssets("t:EnemyDatabase");

        if (attackDbGuids.Length == 0 && enemyDbGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", 
                "No se encontraron bases de datos. Ejecuta primero 'Generar Bases de Datos'.", "OK");
            return;
        }

        // Actualizar AttackDatabase
        if (attackDbGuids.Length > 0)
        {
            string attackDbPath = AssetDatabase.GUIDToAssetPath(attackDbGuids[0]);
            AttackDatabase attackDatabase = AssetDatabase.LoadAssetAtPath<AttackDatabase>(attackDbPath);

            string[] attackGuids = AssetDatabase.FindAssets("t:AttackData", new[] { "Assets/Attacks" });
            if (attackGuids.Length > 0)
            {
                AttackData[] allAttacks = new AttackData[attackGuids.Length];
                for (int i = 0; i < attackGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(attackGuids[i]);
                    allAttacks[i] = AssetDatabase.LoadAssetAtPath<AttackData>(path);
                }

                SerializedObject so = new SerializedObject(attackDatabase);
                SerializedProperty attacksProperty = so.FindProperty("attacks");
                attacksProperty.arraySize = allAttacks.Length;
                for (int i = 0; i < allAttacks.Length; i++)
                {
                    attacksProperty.GetArrayElementAtIndex(i).objectReferenceValue = allAttacks[i];
                }
                so.ApplyModifiedProperties();

                Debug.Log($"✓ Attack Database actualizada con {allAttacks.Length} ataques.");
            }
        }

        // Actualizar EnemyDatabase
        if (enemyDbGuids.Length > 0)
        {
            string enemyDbPath = AssetDatabase.GUIDToAssetPath(enemyDbGuids[0]);
            EnemyDatabase enemyDatabase = AssetDatabase.LoadAssetAtPath<EnemyDatabase>(enemyDbPath);

            string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyData", new[] { "Assets/Enemies" });
            if (enemyGuids.Length > 0)
            {
                EnemyData[] allEnemies = new EnemyData[enemyGuids.Length];
                for (int i = 0; i < enemyGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(enemyGuids[i]);
                    allEnemies[i] = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                }

                SerializedObject so = new SerializedObject(enemyDatabase);
                SerializedProperty enemiesProperty = so.FindProperty("enemies");
                enemiesProperty.arraySize = allEnemies.Length;
                for (int i = 0; i < allEnemies.Length; i++)
                {
                    enemiesProperty.GetArrayElementAtIndex(i).objectReferenceValue = allEnemies[i];
                }
                so.ApplyModifiedProperties();

                Debug.Log($"✓ Enemy Database actualizada con {allEnemies.Length} enemigos.");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Bases de Datos Actualizadas", 
            "Las bases de datos se han actualizado con todos los ataques y enemigos encontrados.", "OK");
    }
}

