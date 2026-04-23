#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menu TD > Reliquias > Criar Bussola do Verme
/// Gera automaticamente: prefab do efeito, prefab do indicador de UI,
/// ReliquiaDef ScriptableObject e adiciona ao ReliquiaDatabase.
/// </summary>
public static class BussolaVermeSetup
{
    private const string DirPrefabsReliquias  = "Assets/Prefabs/Reliquias";
    private const string DirDataReliquias     = "Assets/Data/Reliquias";
    private const string PathDatabase         = "Assets/Prefabs/Reliquias/ReliquiaDatabase.asset";
    private const string PathDefBussola       = DirDataReliquias + "/ReliquiaDef_BussolaVerme.asset";
    private const string PathPrefabIndicador  = DirPrefabsReliquias + "/PrefabIndicadorDirecaoWave.prefab";
    private const string PathPrefabEfeito     = DirPrefabsReliquias + "/PrefabsEfeito_BussolaVerme.prefab";

    [MenuItem("TD/Reliquias/Criar Bussola do Verme")]
    public static void CriarTudo()
    {
        GarantirPastas();

        Sprite iconeQEDVSV = CarregarSprite("QEDVSV");

        GameObject prefabIndicador = CriarESalvarPrefabIndicador(iconeQEDVSV);
        GameObject prefabEfeito    = CriarESalvarPrefabEfeito(prefabIndicador);
        ReliquiaDef def            = CriarESalvarReliquiaDef(iconeQEDVSV, prefabEfeito);

        AdicionarAoDatabase(def);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(def);
        Selection.activeObject = def;

        Debug.Log("[BussolaVerme] Setup concluido! Prefab indicador, prefab efeito e ReliquiaDef criados e registrados no ReliquiaDatabase.");
    }

    // ── PASTAS ────────────────────────────────────────────────────────────────

    private static void GarantirPastas()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");

        if (!AssetDatabase.IsValidFolder(DirDataReliquias))
            AssetDatabase.CreateFolder("Assets/Data", "Reliquias");

        if (!AssetDatabase.IsValidFolder(DirPrefabsReliquias))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Reliquias");
    }

    // ── PREFAB INDICADOR ─────────────────────────────────────────────────────

    private static GameObject CriarESalvarPrefabIndicador(Sprite icone)
    {
        // Root
        var root = new GameObject("PrefabIndicadorDirecaoWave", typeof(RectTransform));
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(200f, 80f);
        var indicadorComp = root.AddComponent<IndicadorDirecaoWaveUI>();

        // Filho: imagem da seta (usa o ícone da relíquia como seta rotacionável)
        var setaGo = new GameObject("Seta", typeof(RectTransform));
        setaGo.transform.SetParent(root.transform, false);
        var setaRect = setaGo.GetComponent<RectTransform>();
        setaRect.anchoredPosition = new Vector2(0f, 20f);
        setaRect.sizeDelta = new Vector2(60f, 60f);
        var img = setaGo.AddComponent<Image>();
        if (icone != null)
            img.sprite = icone;
        img.preserveAspect = true;

        // Filho: texto do lado
        var textoGo = new GameObject("TextoLado", typeof(RectTransform));
        textoGo.transform.SetParent(root.transform, false);
        var textoRect = textoGo.GetComponent<RectTransform>();
        textoRect.anchoredPosition = new Vector2(0f, -25f);
        textoRect.sizeDelta = new Vector2(200f, 30f);
        var tmp = textoGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "ESQUERDA";
        tmp.fontSize = 14f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Injetar referências no componente via SerializedObject
        var tempPrefabPath = "Assets/_TempIndicadorBussola.prefab";
        var tempPrefab = PrefabUtility.SaveAsPrefabAsset(root, tempPrefabPath);

        // Agora usa SerializedObject para atribuir os campos SerializeField
        var so = new SerializedObject(tempPrefab.GetComponent<IndicadorDirecaoWaveUI>());
        so.FindProperty("imagemSeta").objectReferenceValue = tempPrefab.transform.Find("Seta").GetComponent<Image>();
        so.FindProperty("textoLado").objectReferenceValue  = tempPrefab.transform.Find("TextoLado").GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();

        // Move para o destino final
        AssetDatabase.CopyAsset(tempPrefabPath, PathPrefabIndicador);
        AssetDatabase.DeleteAsset(tempPrefabPath);
        Object.DestroyImmediate(root);

        Debug.Log("[BussolaVerme] Prefab indicador criado em " + PathPrefabIndicador);
        return AssetDatabase.LoadAssetAtPath<GameObject>(PathPrefabIndicador);
    }

    // ── PREFAB EFEITO ─────────────────────────────────────────────────────────

    private static GameObject CriarESalvarPrefabEfeito(GameObject prefabIndicador)
    {
        var root = new GameObject("PrefabsEfeito_BussolaVerme");
        root.AddComponent<EfeitoBussolaVerme>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PathPrefabEfeito);
        Object.DestroyImmediate(root);

        // Atribui o prefabIndicador via SerializedObject
        if (prefabIndicador != null)
        {
            var so = new SerializedObject(prefab.GetComponent<EfeitoBussolaVerme>());
            so.FindProperty("prefabIndicador").objectReferenceValue = prefabIndicador.GetComponent<IndicadorDirecaoWaveUI>();
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SavePrefabAsset(prefab);
        }

        Debug.Log("[BussolaVerme] Prefab efeito criado em " + PathPrefabEfeito);
        return prefab;
    }

    // ── RELIQUIA DEF ──────────────────────────────────────────────────────────

    private static ReliquiaDef CriarESalvarReliquiaDef(Sprite icone, GameObject prefabEfeito)
    {
        // Verifica se já existe e remove para recriar limpo
        if (AssetDatabase.LoadAssetAtPath<ReliquiaDef>(PathDefBussola) != null)
            AssetDatabase.DeleteAsset(PathDefBussola);

        var def = ScriptableObject.CreateInstance<ReliquiaDef>();
        def.idReliquia          = ReliquiaIds.BussolaVerme;
        def.nomeExibicao        = "Que espécie de verme são vocês?";
        def.descricao           = "Revela de qual direção virá a próxima wave de inimigos, durante o tempo de espera entre ondas.";
        def.icone               = icone;
        def.prefabEfeitoPartida = prefabEfeito;

        AssetDatabase.CreateAsset(def, PathDefBussola);
        Debug.Log("[BussolaVerme] ReliquiaDef criado em " + PathDefBussola);
        return def;
    }

    // ── RELIQUIA DATABASE ─────────────────────────────────────────────────────

    private static void AdicionarAoDatabase(ReliquiaDef novaDef)
    {
        var db = AssetDatabase.LoadAssetAtPath<ReliquiaDatabase>(PathDatabase);
        if (db == null)
        {
            Debug.LogError("[BussolaVerme] ReliquiaDatabase nao encontrado em " + PathDatabase + ". Adicione manualmente.");
            return;
        }

        var so = new SerializedObject(db);
        var entradas = so.FindProperty("entradas");

        // Verifica se já existe entrada com o mesmo ID
        for (int i = 0; i < entradas.arraySize; i++)
        {
            var entrada = entradas.GetArrayElementAtIndex(i).objectReferenceValue as ReliquiaDef;
            if (entrada != null && entrada.idReliquia == ReliquiaIds.BussolaVerme)
            {
                Debug.Log("[BussolaVerme] ReliquiaDatabase ja contem BussolaVerme. Atualizando referencia.");
                entradas.GetArrayElementAtIndex(i).objectReferenceValue = novaDef;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(db);
                return;
            }
        }

        // Adiciona nova entrada
        entradas.arraySize++;
        entradas.GetArrayElementAtIndex(entradas.arraySize - 1).objectReferenceValue = novaDef;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(db);
        Debug.Log("[BussolaVerme] ReliquiaDef adicionado ao ReliquiaDatabase (total: " + entradas.arraySize + " entradas).");
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────

    private static Sprite CarregarSprite(string nomeSemExtensao)
    {
        string[] guids = AssetDatabase.FindAssets(nomeSemExtensao + " t:Sprite");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path)
                    .Equals(nomeSemExtensao, System.StringComparison.OrdinalIgnoreCase))
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    Debug.Log("[BussolaVerme] Sprite '" + nomeSemExtensao + "' carregado de: " + path);
                    return sprite;
                }
            }
        }
        Debug.LogWarning("[BussolaVerme] Sprite '" + nomeSemExtensao + "' nao encontrado. Importe o arquivo e recrie via menu.");
        return null;
    }
}
#endif
