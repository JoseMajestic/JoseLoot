using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Script Editor que genera automáticamente 21 ataques predefinidos desde el menú Tools.
/// </summary>
public class AttackGenerator : EditorWindow
{
    [MenuItem("Tools/Combate/Generar 21 Ataques Predefinidos")]
    public static void GenerateAttacks()
    {
        // Crear carpeta si no existe
        string folderPath = "Assets/Attacks";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parentFolder = "Assets";
            string folderName = "Attacks";
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        int createdCount = 0;

        // 1. Basic Hit
        CreateAttack("Basic Hit", "Ataque básico sin efectos especiales.", 
                     AttackEffectType.Normal, 0, 0);
        createdCount++;

        // 2-5. Healing (4 ataques)
        CreateAttack("Healing 1", "Cura 25% del HP máximo.", 
                     AttackEffectType.Heal, 25, 0);
        CreateAttack("Healing 2", "Cura 50% del HP máximo.", 
                     AttackEffectType.Heal, 50, 0);
        CreateAttack("Healing 3", "Cura 75% del HP máximo.", 
                     AttackEffectType.Heal, 75, 0);
        CreateAttack("Healing 4", "Cura 100% del HP máximo.", 
                     AttackEffectType.Heal, 100, 0);
        createdCount += 4;

        // 6-8. Poison (3 ataques)
        CreateAttack("Poison 1", "Aplica veneno que causa 10% del HP máximo al final de cada ronda.", 
                     AttackEffectType.Poison, 10, 0);
        CreateAttack("Poison 2", "Aplica veneno que causa 15% del HP máximo al final de cada ronda.", 
                     AttackEffectType.Poison, 15, 0);
        CreateAttack("Poison 3", "Aplica veneno que causa 20% del HP máximo al final de cada ronda.", 
                     AttackEffectType.Poison, 20, 0);
        createdCount += 3;

        // 9-11. Multiple Attack (3 ataques)
        CreateAttack("Multiple Attack 1", "Ataca 2 veces en esta ronda (como 2 golpes básicos).", 
                     AttackEffectType.MultipleAttack, 2, 0);
        CreateAttack("Multiple Attack 2", "Ataca 3 veces en esta ronda (como 3 golpes básicos).", 
                     AttackEffectType.MultipleAttack, 3, 0);
        CreateAttack("Multiple Attack 3", "Ataca 4 veces en esta ronda (como 4 golpes básicos).", 
                     AttackEffectType.MultipleAttack, 4, 0);
        createdCount += 3;

        // 12. Stun (1 ataque)
        CreateAttack("Stun 1", "Aturde al oponente con 50% de probabilidad + 1% por cada punto de suerte.", 
                     AttackEffectType.Stun, 0, 0);
        createdCount++;

        // 13-15. Strong Blow (3 ataques)
        CreateAttack("Strong Blow 1", "Golpe fuerte que hace 2x el daño del ataque.", 
                     AttackEffectType.StrongBlow, 2, 0);
        CreateAttack("Strong Blow 2", "Golpe fuerte que hace 3x el daño del ataque.", 
                     AttackEffectType.StrongBlow, 3, 0);
        CreateAttack("Strong Blow 3", "Golpe fuerte que hace 4x el daño del ataque.", 
                     AttackEffectType.StrongBlow, 4, 0);
        createdCount += 3;

        // 16-18. Attack Buff (3 ataques)
        CreateAttack("Attack Buff 1", "Aumenta el ataque 10% por 3 rondas.", 
                     AttackEffectType.AttackBuff, 10, 3);
        CreateAttack("Attack Buff 2", "Aumenta el ataque 15% por 4 rondas.", 
                     AttackEffectType.AttackBuff, 15, 4);
        CreateAttack("Attack Buff 3", "Aumenta el ataque 30% por 5 rondas.", 
                     AttackEffectType.AttackBuff, 30, 5);
        createdCount += 3;

        // 19-21. Defense Buff (3 ataques)
        CreateAttack("Defense Buff 1", "Aumenta la defensa 10% por 3 rondas.", 
                     AttackEffectType.DefenseBuff, 10, 3);
        CreateAttack("Defense Buff 2", "Aumenta la defensa 15% por 4 rondas.", 
                     AttackEffectType.DefenseBuff, 15, 4);
        CreateAttack("Defense Buff 3", "Aumenta la defensa 20% por 5 rondas.", 
                     AttackEffectType.DefenseBuff, 20, 5);
        createdCount += 3;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✓ Se generaron {createdCount} ataques en {folderPath}");
        EditorUtility.DisplayDialog("Ataques Generados", 
            $"Se crearon {createdCount} ataques exitosamente en:\n{folderPath}", "OK");
    }

    /// <summary>
    /// Crea un ScriptableObject AttackData con los parámetros especificados.
    /// </summary>
    private static void CreateAttack(string attackName, string description, 
                                    AttackEffectType effectType, int effectValue, int duration)
    {
        AttackData attack = ScriptableObject.CreateInstance<AttackData>();
        attack.attackName = attackName;
        attack.description = description;
        attack.effectType = effectType;
        attack.effectValue = effectValue;
        attack.duration = duration;
        
        // Valores por defecto
        attack.baseDamage = 10;
        attack.skillBonus = 0;

        // Nombre del archivo (sin espacios)
        string fileName = attackName.Replace(" ", "_");
        string path = $"Assets/Attacks/{fileName}.asset";
        
        // Si el archivo ya existe, agregar número
        int counter = 1;
        while (File.Exists(path))
        {
            path = $"Assets/Attacks/{fileName}_{counter}.asset";
            counter++;
        }

        AssetDatabase.CreateAsset(attack, path);
    }
}

