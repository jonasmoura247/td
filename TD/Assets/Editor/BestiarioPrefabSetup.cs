#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class BestiarioPrefabSetup
{
    /// <summary>IDs iguais aos do <see cref="BestiarioDatabase"/> — mesmos prefabs referenciados no Mapa_1.</summary>
    private static readonly (string assetPath, string criaturaId)[] PrefabsMapa1 =
    {
        ("Assets/Prefabs/Characters/Goblins/GoblinGuerreiro_Prefab.prefab", "gob_guerreiro"),
        ("Assets/Prefabs/Characters/Goblins/GoblinLanceiro.prefab", "gob_lanceiro"),
        ("Assets/Prefabs/Characters/Goblins/GoblinGuerreiroN.prefab", "gob_guerreiro_n"),
        ("Assets/Prefabs/Characters/Humanos/HumanoGuerreiro.prefab", "hum_guerreiro"),
        ("Assets/Prefabs/Characters/Elfos/ElfoAssassino.prefab", "elf_assassino"),
        ("Assets/Prefabs/Characters/Elfos/ElfoMago.prefab", "elf_mago"),
        ("Assets/Prefabs/Characters/Anoes/AnaoGuerreiro.prefab", "ana_guerreiro"),
    };

    [MenuItem("TD/Bestiario/Adicionar identificadores nos prefabs do Mapa_1 (todos)")]
    public static void ConfigurarPrefabsMapa1()
    {
        for (int i = 0; i < PrefabsMapa1.Length; i++)
            ConfigurarPrefab(PrefabsMapa1[i].assetPath, PrefabsMapa1[i].criaturaId);
        AssetDatabase.SaveAssets();
        Debug.Log("[Bestiario] Prefabs do Mapa_1 atualizados (IdentificadorBestiarioInimigo + RegistoMorteBestiario).");
    }

    [MenuItem("TD/Bestiario/Adicionar identificadores nos prefabs Goblin (inimigos)")]
    public static void ConfigurarPrefabs()
    {
        ConfigurarPrefab(
            "Assets/Prefabs/Characters/Goblins/GoblinGuerreiro_Prefab.prefab",
            "gob_guerreiro");
        ConfigurarPrefab(
            "Assets/Prefabs/Characters/Goblins/GoblinLanceiro.prefab",
            "gob_lanceiro");
        AssetDatabase.SaveAssets();
        Debug.Log("[Bestiario] Prefabs Goblin atualizados com IdentificadorBestiarioInimigo e RegistoMorteBestiario.");
    }

    private static void ConfigurarPrefab(string assetPath, string id)
    {
        if (!System.IO.File.Exists(assetPath))
        {
            Debug.LogWarning("[Bestiario] Prefab nao encontrado: " + assetPath);
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(assetPath);
        try
        {
            var ident = root.GetComponent<IdentificadorBestiarioInimigo>();
            if (ident == null)
                ident = root.AddComponent<IdentificadorBestiarioInimigo>();
            SerializedObject soId = new SerializedObject(ident);
            soId.FindProperty("criaturaId").stringValue = id;
            soId.ApplyModifiedPropertiesWithoutUndo();

            if (root.GetComponent<RegistoMorteBestiario>() == null)
                root.AddComponent<RegistoMorteBestiario>();

            PrefabUtility.SaveAsPrefabAsset(root, assetPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
#endif
