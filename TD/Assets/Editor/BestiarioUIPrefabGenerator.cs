#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class BestiarioUIPrefabGenerator
{
    private const string DirPrefabs = "Assets/Prefabs/UI/Bestiario";
    private const string DirResources = "Assets/Resources/Bestiario";
    private const string PathLayout = DirResources + "/BestiarioUILayout.asset";
    private const string PathSlot = DirPrefabs + "/BestiarioSlotCriatura.prefab";
    private const string PathLinha = DirPrefabs + "/BestiarioRunaLinha.prefab";
    private const string PathCatalogo = DirPrefabs + "/BestiarioCatalogoRunaItem.prefab";

    [MenuItem("TD/Bestiario/Gerar prefabs UI e layout (Resources)")]
    public static void GerarTudo()
    {
        GarantirPastas();

        GameObject slot = ConstruirSlotCriatura();
        GameObject linha = ConstruirLinhaRuna();
        GameObject catalogo = ConstruirItemCatalogo();

        try
        {
            PrefabUtility.SaveAsPrefabAsset(slot, PathSlot);
            PrefabUtility.SaveAsPrefabAsset(linha, PathLinha);
            PrefabUtility.SaveAsPrefabAsset(catalogo, PathCatalogo);
        }
        finally
        {
            Object.DestroyImmediate(slot);
            Object.DestroyImmediate(linha);
            Object.DestroyImmediate(catalogo);
        }

        var layout = AssetDatabase.LoadAssetAtPath<BestiarioUILayout>(PathLayout);
        if (layout == null)
        {
            layout = ScriptableObject.CreateInstance<BestiarioUILayout>();
            AssetDatabase.CreateAsset(layout, PathLayout);
        }

        layout.prefabSlotCriatura = AssetDatabase.LoadAssetAtPath<GameObject>(PathSlot);
        layout.prefabLinhaRuna = AssetDatabase.LoadAssetAtPath<GameObject>(PathLinha);
        layout.prefabItemCatalogoRuna = AssetDatabase.LoadAssetAtPath<GameObject>(PathCatalogo);
        layout.colunasGrelha = 3;
        EditorUtility.SetDirty(layout);
        AssetDatabase.SaveAssets();

        EditorGUIUtility.PingObject(layout);
        Debug.Log("[Bestiario] Prefabs e BestiarioUILayout atualizados em " + DirPrefabs + " / " + PathLayout);
    }

    private static void GarantirPastas()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        if (!AssetDatabase.IsValidFolder(DirPrefabs))
            AssetDatabase.CreateFolder("Assets/Prefabs/UI", "Bestiario");
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(DirResources))
            AssetDatabase.CreateFolder("Assets/Resources", "Bestiario");
    }

    private static TMP_FontAsset FontePadrao()
    {
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
    }

    private static void AplicarFonte(TextMeshProUGUI tmp)
    {
        var f = FontePadrao();
        if (f != null)
            tmp.font = f;
        tmp.color = Color.white;
    }

    private static GameObject ConstruirSlotCriatura()
    {
        var fonte = FontePadrao();
        GameObject raiz = new GameObject("BestiarioSlotCriatura", typeof(RectTransform));
        var rt = raiz.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100f, 100f);

        var le = raiz.AddComponent<LayoutElement>();
        le.preferredWidth = 100f;
        le.preferredHeight = 100f;
        le.minWidth = 100f;
        le.minHeight = 100f;

        var borda = raiz.AddComponent<Image>();
        borda.color = new Color(0.35f, 0.28f, 0.15f, 1f);
        borda.raycastTarget = true;

        var btn = raiz.AddComponent<Button>();
        btn.targetGraphic = borda;

        GameObject interno = new GameObject("Interno", typeof(RectTransform));
        interno.transform.SetParent(raiz.transform, false);
        var rtIn = interno.GetComponent<RectTransform>();
        rtIn.anchorMin = Vector2.zero;
        rtIn.anchorMax = Vector2.one;
        rtIn.offsetMin = new Vector2(3f, 3f);
        rtIn.offsetMax = new Vector2(-3f, -3f);
        var fundoInterno = interno.AddComponent<Image>();
        fundoInterno.color = new Color(0.12f, 0.12f, 0.14f, 1f);
        fundoInterno.raycastTarget = false;

        GameObject iconeGo = new GameObject("Icone", typeof(RectTransform));
        iconeGo.transform.SetParent(interno.transform, false);
        var rtIc = iconeGo.GetComponent<RectTransform>();
        rtIc.anchorMin = new Vector2(0.1f, 0.28f);
        rtIc.anchorMax = new Vector2(0.9f, 0.95f);
        rtIc.offsetMin = Vector2.zero;
        rtIc.offsetMax = Vector2.zero;
        var imgIcone = iconeGo.AddComponent<Image>();
        imgIcone.raycastTarget = false;

        GameObject nomeGo = new GameObject("Nome", typeof(RectTransform));
        nomeGo.transform.SetParent(interno.transform, false);
        var rtNm = nomeGo.GetComponent<RectTransform>();
        rtNm.anchorMin = new Vector2(0.05f, 0.72f);
        rtNm.anchorMax = new Vector2(0.95f, 0.98f);
        rtNm.offsetMin = Vector2.zero;
        rtNm.offsetMax = Vector2.zero;
        var tmpNome = nomeGo.AddComponent<TextMeshProUGUI>();
        if (fonte != null)
            tmpNome.font = fonte;
        tmpNome.fontSize = 12f;
        tmpNome.alignment = TextAlignmentOptions.Top;
        tmpNome.color = Color.white;
        tmpNome.raycastTarget = false;

        GameObject marcosGo = new GameObject("MarcosEstagios", typeof(RectTransform));
        marcosGo.transform.SetParent(interno.transform, false);
        var rtMc = marcosGo.GetComponent<RectTransform>();
        rtMc.anchorMin = new Vector2(0f, 0f);
        rtMc.anchorMax = new Vector2(1f, 0.16f);
        rtMc.offsetMin = new Vector2(2f, 0f);
        rtMc.offsetMax = new Vector2(-2f, 0f);
        var tmpMarcos = marcosGo.AddComponent<TextMeshProUGUI>();
        if (fonte != null)
            tmpMarcos.font = fonte;
        tmpMarcos.fontSize = 10f;
        tmpMarcos.alignment = TextAlignmentOptions.Center;
        tmpMarcos.color = new Color(0.85f, 0.88f, 0.95f, 1f);
        tmpMarcos.textWrappingMode = TextWrappingModes.NoWrap;
        tmpMarcos.overflowMode = TextOverflowModes.Overflow;
        tmpMarcos.raycastTarget = false;

        GameObject rodapeGo = new GameObject("Rodape", typeof(RectTransform));
        rodapeGo.transform.SetParent(interno.transform, false);
        var rtRd = rodapeGo.GetComponent<RectTransform>();
        rtRd.anchorMin = new Vector2(0f, 0.16f);
        rtRd.anchorMax = new Vector2(1f, 0.28f);
        rtRd.offsetMin = new Vector2(2f, 1f);
        rtRd.offsetMax = new Vector2(-2f, -1f);
        var tmpRodape = rodapeGo.AddComponent<TextMeshProUGUI>();
        if (fonte != null)
            tmpRodape.font = fonte;
        tmpRodape.fontSize = 12f;
        tmpRodape.alignment = TextAlignmentOptions.BottomRight;
        tmpRodape.color = new Color(1f, 0.92f, 0.4f, 1f);

        var item = raiz.AddComponent<BestiarioSlotCriaturaItem>();
        var so = new SerializedObject(item);
        so.FindProperty("imagemIcone").objectReferenceValue = imgIcone;
        so.FindProperty("textoRodape").objectReferenceValue = tmpRodape;
        so.FindProperty("textoNome").objectReferenceValue = tmpNome;
        so.FindProperty("botao").objectReferenceValue = btn;
        so.FindProperty("imagemMolduraRaiz").objectReferenceValue = borda;
        so.FindProperty("textoMarcosEstagio").objectReferenceValue = tmpMarcos;
        so.ApplyModifiedPropertiesWithoutUndo();

        return raiz;
    }

    private static GameObject ConstruirLinhaRuna()
    {
        var fonte = FontePadrao();
        GameObject raiz = new GameObject("BestiarioRunaLinha", typeof(RectTransform));
        var h = raiz.AddComponent<HorizontalLayoutGroup>();
        h.childAlignment = TextAnchor.MiddleLeft;
        h.spacing = 12f;
        h.childControlWidth = false;
        h.childForceExpandWidth = false;
        h.padding = new RectOffset(4, 4, 4, 4);

        var leRoot = raiz.AddComponent<LayoutElement>();
        leRoot.minHeight = 40f;
        leRoot.preferredHeight = 40f;
        leRoot.flexibleWidth = 1f;

        GameObject iconeGo = new GameObject("IconeRuna", typeof(RectTransform));
        iconeGo.transform.SetParent(raiz.transform, false);
        var imgI = iconeGo.AddComponent<Image>();
        imgI.color = Color.white;
        var leI = iconeGo.AddComponent<LayoutElement>();
        leI.preferredWidth = 40f;
        leI.preferredHeight = 40f;

        GameObject txtGo = new GameObject("Texto", typeof(RectTransform));
        txtGo.transform.SetParent(raiz.transform, false);
        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        AplicarFonte(tmp);
        tmp.fontSize = 12f;
        tmp.text = "Runa";
        var leT = txtGo.AddComponent<LayoutElement>();
        leT.flexibleWidth = 1f;
        leT.minWidth = 120f;

        GameObject btnGo = new GameObject("BotaoAcao", typeof(RectTransform));
        btnGo.transform.SetParent(raiz.transform, false);
        var imgB = btnGo.AddComponent<Image>();
        imgB.color = new Color(0.2f, 0.35f, 0.5f, 1f);
        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = imgB;
        var leB = btnGo.AddComponent<LayoutElement>();
        leB.preferredWidth = 110f;
        leB.preferredHeight = 30f;

        GameObject lblGo = new GameObject("Label", typeof(RectTransform));
        lblGo.transform.SetParent(btnGo.transform, false);
        var rtl = lblGo.GetComponent<RectTransform>();
        rtl.anchorMin = Vector2.zero;
        rtl.anchorMax = Vector2.one;
        rtl.offsetMin = Vector2.zero;
        rtl.offsetMax = Vector2.zero;
        var tmpB = lblGo.AddComponent<TextMeshProUGUI>();
        if (fonte != null)
            tmpB.font = fonte;
        tmpB.alignment = TextAlignmentOptions.Midline;
        tmpB.fontSize = 10f;
        tmpB.text = "Equipar";

        var linha = raiz.AddComponent<BestiarioRunaLinhaItem>();
        var so = new SerializedObject(linha);
        so.FindProperty("imagemIcone").objectReferenceValue = imgI;
        so.FindProperty("textoLinha").objectReferenceValue = tmp;
        so.FindProperty("botaoAcao").objectReferenceValue = btn;
        so.FindProperty("textoBotao").objectReferenceValue = tmpB;
        so.FindProperty("imagemBotao").objectReferenceValue = imgB;
        so.ApplyModifiedPropertiesWithoutUndo();

        return raiz;
    }

    private static GameObject ConstruirItemCatalogo()
    {
        var fonte = FontePadrao();
        GameObject raiz = new GameObject("BestiarioCatalogoRunaItem", typeof(RectTransform));
        var rtRaiz = raiz.GetComponent<RectTransform>();
        rtRaiz.sizeDelta = new Vector2(504f, 195f);

        var cg = raiz.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        var imgBg = raiz.AddComponent<Image>();
        imgBg.color = new Color(0.14f, 0.15f, 0.18f, 1f);
        var btn = raiz.AddComponent<Button>();
        btn.targetGraphic = imgBg;

        var leR = raiz.AddComponent<LayoutElement>();
        leR.minHeight = 110f;
        leR.preferredWidth = 504f;
        leR.preferredHeight = 195f;
        leR.flexibleWidth = 0f;

        GameObject iconeGo = new GameObject("Icone", typeof(RectTransform));
        iconeGo.transform.SetParent(raiz.transform, false);
        var rtIcone = iconeGo.GetComponent<RectTransform>();
        rtIcone.anchorMin = rtIcone.anchorMax = new Vector2(0f, 0.5f);
        rtIcone.pivot = new Vector2(0.5f, 0.5f);
        rtIcone.anchoredPosition = new Vector2(38f, 0f);
        rtIcone.sizeDelta = new Vector2(56f, 56f);
        var imgI = iconeGo.AddComponent<Image>();

        GameObject col = new GameObject("ColunaTexto", typeof(RectTransform));
        col.transform.SetParent(raiz.transform, false);
        var rtCol = col.GetComponent<RectTransform>();
        rtCol.anchorMin = Vector2.zero;
        rtCol.anchorMax = Vector2.one;
        rtCol.pivot = new Vector2(0.5f, 0.5f);
        rtCol.offsetMin = new Vector2(78f, 8f);
        rtCol.offsetMax = new Vector2(-10f, -8f);

        GameObject goTit = new GameObject("Titulo", typeof(RectTransform));
        goTit.transform.SetParent(col.transform, false);
        var rtTit = goTit.GetComponent<RectTransform>();
        rtTit.anchorMin = rtTit.anchorMax = new Vector2(0f, 1f);
        rtTit.pivot = new Vector2(0f, 1f);
        rtTit.anchoredPosition = new Vector2(0f, -4f);
        rtTit.sizeDelta = new Vector2(400f, 28f);
        var tmpTit = goTit.AddComponent<TextMeshProUGUI>();
        AplicarFonte(tmpTit);
        tmpTit.fontSize = 14f;
        tmpTit.fontStyle = FontStyles.Bold;
        tmpTit.text = "Titulo runa";

        GameObject goDesc = new GameObject("DescricaoCatalogo", typeof(RectTransform));
        goDesc.transform.SetParent(col.transform, false);
        var rtDesc = goDesc.GetComponent<RectTransform>();
        rtDesc.anchorMin = rtDesc.anchorMax = new Vector2(0f, 1f);
        rtDesc.pivot = new Vector2(0f, 1f);
        rtDesc.anchoredPosition = new Vector2(0f, -36f);
        rtDesc.sizeDelta = new Vector2(400f, 56f);
        var tmpDesc = goDesc.AddComponent<TextMeshProUGUI>();
        AplicarFonte(tmpDesc);
        tmpDesc.fontSize = 10f;
        tmpDesc.color = new Color(0.78f, 0.8f, 0.82f, 1f);
        tmpDesc.text = "Descricao visivel no catalogo (opcional no database).";
        tmpDesc.textWrappingMode = TextWrappingModes.Normal;

        GameObject goReq = new GameObject("Requisitos", typeof(RectTransform));
        goReq.transform.SetParent(col.transform, false);
        var rtReq = goReq.GetComponent<RectTransform>();
        rtReq.anchorMin = rtReq.anchorMax = new Vector2(0f, 1f);
        rtReq.pivot = new Vector2(0f, 1f);
        rtReq.anchoredPosition = new Vector2(0f, -96f);
        rtReq.sizeDelta = new Vector2(400f, 88f);
        var tmpReq = goReq.AddComponent<TextMeshProUGUI>();
        AplicarFonte(tmpReq);
        tmpReq.fontSize = 11f;
        tmpReq.text = "Requisitos";

        GameObject goEf = new GameObject("Efeito", typeof(RectTransform));
        goEf.transform.SetParent(col.transform, false);
        var rtEf = goEf.GetComponent<RectTransform>();
        rtEf.anchorMin = rtEf.anchorMax = new Vector2(0f, 1f);
        rtEf.pivot = new Vector2(0f, 1f);
        rtEf.anchoredPosition = new Vector2(0f, -188f);
        rtEf.sizeDelta = new Vector2(400f, 36f);
        var tmpEf = goEf.AddComponent<TextMeshProUGUI>();
        AplicarFonte(tmpEf);
        tmpEf.fontSize = 10f;
        tmpEf.color = new Color(0.85f, 0.9f, 1f, 1f);
        tmpEf.text = "Efeito";

        var item = raiz.AddComponent<BestiarioCatalogoRunaItem>();
        var so = new SerializedObject(item);
        so.FindProperty("canvasGroupRaiz").objectReferenceValue = cg;
        so.FindProperty("imagemIcone").objectReferenceValue = imgI;
        so.FindProperty("textoTitulo").objectReferenceValue = tmpTit;
        so.FindProperty("textoDescricaoCatalogo").objectReferenceValue = tmpDesc;
        so.FindProperty("textoRequisitos").objectReferenceValue = tmpReq;
        so.FindProperty("textoEfeito").objectReferenceValue = tmpEf;
        so.FindProperty("botaoRaiz").objectReferenceValue = btn;
        so.ApplyModifiedPropertiesWithoutUndo();

        return raiz;
    }
}
#endif
