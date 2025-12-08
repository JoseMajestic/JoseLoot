
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GenerateEquipmentItems : ScriptableObject
{
    private static readonly string[] Rarezas = {
        "Común", "Raro", "Mágico", "Etéreo", "Excelente",
        "Legendario", "Épico", "Celestial", "Extremo", "Demoníaco"
    };

    private static readonly string[] TiposEquipo = {
        "Arma", "Escudo", "Armadura", "Casco", "Guantes", "Botas", "Cinturón"
    };

    [MenuItem("Tools/Inventario/Generar 100 Equipos (Nivel 1)")]
    public static void Generar100Equipos()
    {
        string folderPath = "Assets/Items/Equipo";
        if (!AssetDatabase.IsValidFolder("Assets/Items"))
            AssetDatabase.CreateFolder("Assets", "Items");
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets/Items", "Equipo");

        var items = new List<ItemData>();
        int contador = 0;

        // 3 objetos por tipo y rareza → 7 x 10 x 3 = 210 → tomamos solo 100
        foreach (string rareza in Rarezas)
        {
            int rarezaIndex = System.Array.IndexOf(Rarezas, rareza);
            float multiplicador = 1f + rarezaIndex * 0.8f + (rarezaIndex >= 7 ? rarezaIndex * 0.6f : 0);

            foreach (string tipoStr in TiposEquipo)
            {
                ItemType tipo = TipoToItemType(tipoStr);

                // 3 variantes por tipo/rareza
                for (int variante = 0; variante < 3 && contador < 100; variante++)
                {
                    string nombre = GenerarNombreEquipo(tipoStr, rareza, variante, rarezaIndex);
                    ItemData item = CrearEquipo(nombre, tipo, rareza, multiplicador, variante);
                    items.Add(item);
                    contador++;
                }
            }
        }

        // Guardar en Assets
        foreach (var item in items)
        {
            string safeName = SanitizeFileName(item.itemName);
            string path = $"{folderPath}/{safeName}.asset";
            int copy = 1;
            while (AssetDatabase.LoadAssetAtPath<ItemData>(path) != null)
            {
                path = $"{folderPath}/{safeName} ({copy}).asset";
                copy++;
            }
            AssetDatabase.CreateAsset(item, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"¡100 equipos de nivel 1 generados en {folderPath}!");
    }

    private static ItemType TipoToItemType(string tipo)
    {
        return tipo switch
        {
            "Arma" => ItemType.Weapon,
            "Escudo" => ItemType.Armor,
            "Armadura" => ItemType.Armor,
            "Casco" => ItemType.Armor,
            "Guantes" => ItemType.Armor,
            "Botas" => ItemType.Armor,
            "Cinturón" => ItemType.Armor,
            _ => ItemType.Armor
        };
    }

    private static string GenerarNombreEquipo(string tipo, string rareza, int variante, int rarezaIndex)
    {
        string[] prefijos = { "Oscuro", "Luminoso", "Antiguo", "Sagrado", "Maldito", "Divino", "Roto", "Perfecto", "Olvidado", "Eterno" };
        string[] sufijos = { "de la Tormenta", "del Dragón", "de las Sombras", "del Vacío", "de la Luz", "del Caos", "del Abismo", "del Alba", "del Ocaso", "de los Dioses" };
        string[] variantesNombre = { "I", "II", "III", "Alpha", "Beta", "Omega", "Prime", "Nova", "Zeta", "Sigma" };

        string baseName = tipo;
        string varName = variante < variantesNombre.Length ? $" {variantesNombre[variante]}" : "";

        if (rarezaIndex >= 7)
            return $"{rareza} {baseName}{varName} {sufijos[Random.Range(0, sufijos.Length)]}";

        if (rarezaIndex >= 4)
            return $"{prefijos[Random.Range(0, prefijos.Length)]} {baseName}{varName}";

        return $"{baseName} {rareza}{varName}";
    }

    private static ItemData CrearEquipo(string nombre, ItemType tipo, string rareza, float mult, int variante)
    {
        ItemData item = ScriptableObject.CreateInstance<ItemData>();
        item.itemName = nombre;
        item.itemType = tipo;
        item.rareza = rareza;
        item.nivel = 1; // Todos nivel 1
        item.itemSprite = null;

        int baseStat = 3 + variante;
        int precioBase = tipo == ItemType.Weapon ? 150 : 120;
        item.price = Mathf.RoundToInt(precioBase * mult * (1f + variante * 0.2f));

        // Estadísticas base según tipo
        switch (tipo)
        {
            case ItemType.Weapon:
                item.ataque = Mathf.RoundToInt(baseStat * 2.5f * mult);
                item.velocidadAtaque = variante == 0 ? 1 : variante == 1 ? 0 : -1;
                if (mult > 3) item.ataqueCritico = Mathf.Min(3 + (int)mult, 20);
                if (mult > 5) item.danoCritico = 120 + (int)(mult * 15);
                break;

            case ItemType.Armor:
                if (nombre.Contains("Escudo"))
                {
                    item.defensa = Mathf.RoundToInt(baseStat * 3f * mult);
                    item.hp = Mathf.RoundToInt(15 * mult);
                }
                else if (nombre.Contains("Armadura"))
                {
                    item.defensa = Mathf.RoundToInt(baseStat * 2.8f * mult);
                    item.hp = Mathf.RoundToInt(25 * mult);
                }
                else if (nombre.Contains("Casco"))
                {
                    item.defensa = Mathf.RoundToInt(baseStat * 1.8f * mult);
                    item.hp = Mathf.RoundToInt(12 * mult);
                }
                else if (nombre.Contains("Guantes"))
                {
                    item.ataque = Mathf.RoundToInt(baseStat * 1.2f * mult);
                    item.destreza = Mathf.RoundToInt(2 * mult);
                }
                else if (nombre.Contains("Botas"))
                {
                    item.velocidadAtaque = 1 + (variante > 0 ? 1 : 0);
                    item.suerte = Mathf.RoundToInt(1.5f * mult);
                }
                else if (nombre.Contains("Cinturón"))
                {
                    item.hp = Mathf.RoundToInt(18 * mult);
                    item.mana = Mathf.RoundToInt(15 * mult);
                }
                break;
        }

        // Descripción
        item.description = GenerarDescripcion(tipo, rareza, nombre.Contains("Escudo") ? "Escudo" : 
                                              nombre.Contains("Armadura") ? "Pechera" :
                                              nombre.Contains("Casco") ? "Casco" :
                                              nombre.Contains("Guantes") ? "Guantes" :
                                              nombre.Contains("Botas") ? "Botas" :
                                              nombre.Contains("Cinturón") ? "Cinturón" : "Arma");

        return item;
    }

    private static string GenerarDescripcion(ItemType tipo, string rareza, string subtipo)
    {
        string[] descs = {
            $"Equipo de rareza {rareza}. Ideal para guerreros.",
            $"Forjado con materiales {rareza.ToLower()}.",
            $"Un {subtipo} legendario entre aventureros.",
            $"Brilla con el poder de la rareza {rareza}.",
            $"Usado por héroes de antaño.",
            $"Protege con fuerza {rareza}."
        };
        return descs[Random.Range(0, descs.Length)];
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}