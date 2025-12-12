using UnityEngine;
using UnityEditor;

/// <summary>
/// Script Editor que genera automáticamente el asset CombatTexts con valores por defecto.
/// </summary>
public class CombatTextsGenerator : EditorWindow
{
    [MenuItem("Tools/Combate/Generar Combat Texts (Textos Configurables)")]
    public static void GenerateCombatTexts()
    {
        // Crear carpeta Databases si no existe
        string folderPath = "Assets/Databases";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parentFolder = "Assets";
            string folderName = "Databases";
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        string path = "Assets/Databases/Combat Texts.asset";
        
        // Verificar si ya existe
        CombatTexts existing = AssetDatabase.LoadAssetAtPath<CombatTexts>(path);
        if (existing != null)
        {
            bool overwrite = EditorUtility.DisplayDialog("Combat Texts ya existe", 
                "El archivo Combat Texts ya existe. ¿Deseas sobrescribirlo?\n\n" +
                "ADVERTENCIA: Esto reemplazará todos los textos personalizados que hayas editado.",
                "Sobrescribir", "Cancelar");
            
            if (!overwrite)
                return;
        }

        // Crear nuevo CombatTexts
        CombatTexts combatTexts = ScriptableObject.CreateInstance<CombatTexts>();
        
        // Los valores por defecto ya están en el ScriptableObject
        // No necesitamos establecerlos aquí porque ya tienen valores por defecto
        
        AssetDatabase.CreateAsset(combatTexts, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✓ Combat Texts creado en {path}");
        EditorUtility.DisplayDialog("Combat Texts Generado", 
            $"Se creó el archivo Combat Texts en:\n{path}\n\n" +
            "Ahora puedes:\n" +
            "1. Editar todos los textos desde el Inspector\n" +
            "2. Ajustar la velocidad de escritura (typewriterSpeed)\n" +
            "3. Ajustar el delay entre mensajes (delayBetweenMessages)\n" +
            "4. Arrastrar este asset a CombatManager > combatTexts", "OK");
    }
}



