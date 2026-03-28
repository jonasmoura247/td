#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class BestiarioPrefabSetup
{
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
