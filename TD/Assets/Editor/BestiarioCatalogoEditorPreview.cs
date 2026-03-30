#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Permite ver o catálogo de runas no modo Edit (sem Play), usando linhas de exemplo.
/// O visual real vem do prefab — edita o prefab para tamanhos/fontes; o texto em jogo vem do código/database.
/// </summary>
public static class BestiarioCatalogoEditorPreview
{
    private const string PrefixoPreview = "EditorPreview_CatalogoRuna_";
    private const string PathLayout = "Assets/Resources/Bestiario/BestiarioUILayout.asset";

    [MenuItem("TD/Bestiario/Pré-visualizar catálogo (linhas de exemplo na cena)")]
    public static void ColocarPreview()
    {
        Transform content = EncontrarContentListaCatalogo();
        if (content == null)
        {
            EditorUtility.DisplayDialog("Bestiario",
                "Nao encontrei ContentListaCatalogoRunas na cena.\n" +
                "Abre um lobby com PainelBestiario (menu Configurar UI no lobby) e ativa o painel na Hierarchy se estiver oculto.",
                "OK");
            return;
        }

        var layout = AssetDatabase.LoadAssetAtPath<BestiarioUILayout>(PathLayout);
        if (layout == null || layout.prefabItemCatalogoRuna == null)
        {
            EditorUtility.DisplayDialog("Bestiario",
                "Falta BestiarioUILayout ou prefab do item de catalogo.\nUse: TD/Bestiario/Gerar prefabs UI e layout (Resources).",
                "OK");
            return;
        }

        RemoverPreviewInterno(content);

        var exemplos = new (string titulo, string req, string efeito, float alpha, string desc)[]
        {
            ("Eco de dor",
                "Liberar: 50 pts (uma vez)\nEquipar nesta criatura: min. 3 estagio(s) completo(s)",
                "Eco: 15% de 5% vida max.",
                1f,
                "Texto opcional visivel no cartao (campo descricaoCatalogo no database)."),
            ("Runa bloqueada (exemplo)",
                "Liberar: 60 pts (uma vez)\nEquipar nesta criatura: min. 3 estagio(s) completo(s)\nPontos insuficientes para liberar.",
                "Critico ao castelo: x1,5",
                layout.alphaRunaBloqueadaCatalogo,
                ""),
            ("Outra runa",
                "Liberar: 10 pts (uma vez)\nEquipar nesta criatura: min. 2 estagio(s) completo(s)",
                "Sem efeito configurado.",
                1f,
                "Sem descricao no cartao: deixe descricaoCatalogo vazio no database.")
        };

        Undo.SetCurrentGroupName("Preview catalogo runas");
        for (int i = 0; i < exemplos.Length; i++)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(layout.prefabItemCatalogoRuna, content);
            go.name = PrefixoPreview + i;
            Undo.RegisterCreatedObjectUndo(go, "Preview catalogo");

            var item = go.GetComponent<BestiarioCatalogoRunaItem>();
            if (item != null)
            {
                var e = exemplos[i];
                item.AplicarDadosPreviewEditor(e.titulo, e.req, e.efeito, e.alpha, e.desc);
            }
        }

        EditorSceneManager.MarkSceneDirty(content.gameObject.scene);
        Selection.activeGameObject = content.gameObject;
        Debug.Log("[Bestiario] Linhas de exemplo no catalogo. Remove com TD/Bestiario/Remover pre-visualizacao do catalogo.");
    }

    [MenuItem("TD/Bestiario/Remover pre-visualizacao do catalogo")]
    public static void RemoverPreviewMenu()
    {
        Transform content = EncontrarContentListaCatalogo();
        if (content == null)
        {
            EditorUtility.DisplayDialog("Bestiario", "Nao encontrei ContentListaCatalogoRunas.", "OK");
            return;
        }

        RemoverPreviewInterno(content);
        EditorSceneManager.MarkSceneDirty(content.gameObject.scene);
    }

    private static Transform EncontrarContentListaCatalogo()
    {
        foreach (Transform t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name == "ContentListaCatalogoRunas")
                return t;
        }
        return null;
    }

    private static void RemoverPreviewInterno(Transform content)
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Transform c = content.GetChild(i);
            if (c.name.StartsWith(PrefixoPreview, StringComparison.Ordinal))
                Undo.DestroyObjectImmediate(c.gameObject);
        }
    }
}
#endif
