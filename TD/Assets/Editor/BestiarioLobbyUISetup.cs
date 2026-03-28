#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class BestiarioLobbyUISetup
{
    private const string PathDatabase = "Assets/Resources/Bestiario/BestiarioDatabase.asset";

    private static readonly string[] CenasLobby =
    {
        "Assets/Scenes/Lobby/LobbyGoblins/LobbyGoblin.unity",
        "Assets/Scenes/Lobby/LobbyHumanos/LobbyHumano.unity",
        "Assets/Scenes/Lobby/LobbyElfos/LobbyElfo.unity",
        "Assets/Scenes/Lobby/LobbyAnoes/LobbyAnoes.unity"
    };

    [MenuItem("TD/Bestiario/Configurar UI em TODOS os lobbies (4 cenas)")]
    public static void ConfigurarTodosLobbies()
    {
        if (!EditorUtility.DisplayDialog("Bestiario",
                "Abre e altera as 4 cenas de lobby (PainelBestiario + ControleBestiario). Continuar?",
                "Sim", "Nao"))
            return;

        string cenaAtual = EditorSceneManager.GetActiveScene().path;
        try
        {
            for (int i = 0; i < CenasLobby.Length; i++)
            {
                if (!System.IO.File.Exists(CenasLobby[i]))
                {
                    Debug.LogWarning("[Bestiario] Cena nao encontrada: " + CenasLobby[i]);
                    continue;
                }
                EditorSceneManager.OpenScene(CenasLobby[i]);
                ConfigurarInterno(salvarCena: true);
            }
        }
        finally
        {
            if (!string.IsNullOrEmpty(cenaAtual) && System.IO.File.Exists(cenaAtual))
                EditorSceneManager.OpenScene(cenaAtual);
        }
        Debug.Log("[Bestiario] Todas as cenas de lobby processadas.");
    }

    [MenuItem("TD/Bestiario/Configurar UI no lobby (cena atual)")]
    public static void Configurar()
    {
        ConfigurarInterno(salvarCena: false);
    }

    private static void ConfigurarInterno(bool salvarCena)
    {
        GameObject botaoGo = EncontrarPorNome("BotaoBestiario");
        if (botaoGo == null)
        {
            EditorUtility.DisplayDialog("Bestiario", "Nao encontrei GameObject chamado BotaoBestiario na cena (inclui inativos).", "OK");
            return;
        }

        Canvas canvas = botaoGo.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Bestiario", "BotaoBestiario nao esta sob um Canvas.", "OK");
            return;
        }

        var db = AssetDatabase.LoadAssetAtPath<BestiarioDatabase>(PathDatabase);
        if (db == null)
        {
            EditorUtility.DisplayDialog("Bestiario", "Falta o asset em " + PathDatabase + ". Use o menu Criar database em Resources.", "OK");
            return;
        }

        Transform existentePainel = canvas.transform.Find("PainelBestiario");
        if (existentePainel != null)
        {
            if (!EditorUtility.DisplayDialog("Bestiario", "Ja existe PainelBestiario. Remover e recriar?", "Sim", "Nao"))
                return;
            Undo.DestroyObjectImmediate(existentePainel.gameObject);
        }

        Transform existenteCtrl = canvas.transform.Find("ControleBestiario");
        if (existenteCtrl != null)
            Undo.DestroyObjectImmediate(existenteCtrl.gameObject);

        Undo.SetCurrentGroupName("Bestiario UI Lobby");
        int grupo = Undo.GetCurrentGroup();

        GameObject root = new GameObject("PainelBestiario");
        Undo.RegisterCreatedObjectUndo(root, "Painel Bestiario");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rtRoot = root.AddComponent<RectTransform>();
        rtRoot.anchorMin = Vector2.zero;
        rtRoot.anchorMax = Vector2.one;
        rtRoot.offsetMin = Vector2.zero;
        rtRoot.offsetMax = Vector2.zero;

        Image imgRoot = root.AddComponent<Image>();
        imgRoot.color = new Color(0.06f, 0.07f, 0.1f, 0.96f);
        imgRoot.raycastTarget = true;

        PainelBestiario painel = root.AddComponent<PainelBestiario>();

        CriarBarraSuperior(root.transform, out TextMeshProUGUI tmpPontos, out Button btnFechar);

        GameObject main = new GameObject("Main");
        Undo.RegisterCreatedObjectUndo(main, "Main");
        main.transform.SetParent(root.transform, false);
        RectTransform rtMain = main.AddComponent<RectTransform>();
        rtMain.anchorMin = Vector2.zero;
        rtMain.anchorMax = Vector2.one;
        rtMain.offsetMin = new Vector2(16, 16);
        rtMain.offsetMax = new Vector2(-16, -56);

        HorizontalLayoutGroup hor = main.AddComponent<HorizontalLayoutGroup>();
        hor.spacing = 16f;
        hor.childAlignment = TextAnchor.UpperCenter;
        hor.childControlWidth = true;
        hor.childControlHeight = true;
        hor.childForceExpandWidth = true;
        hor.childForceExpandHeight = true;

        GameObject scrollGo = CriarScroll(main.transform, out RectTransform contentGrid);
        LayoutElement leScroll = scrollGo.AddComponent<LayoutElement>();
        leScroll.flexibleWidth = 2f;
        leScroll.minWidth = 320f;

        GameObject detalheGo = CriarDetalhe(main.transform, out TextMeshProUGUI tmpTitulo, out TextMeshProUGUI tmpInfo, out RectTransform areaRunas);
        LayoutElement leDet = detalheGo.AddComponent<LayoutElement>();
        leDet.flexibleWidth = 1f;
        leDet.minWidth = 300f;

        SerializedObject soPainel = new SerializedObject(painel);
        soPainel.FindProperty("database").objectReferenceValue = db;
        soPainel.FindProperty("textoPontos").objectReferenceValue = tmpPontos;
        soPainel.FindProperty("textoTituloDetalhe").objectReferenceValue = tmpTitulo;
        soPainel.FindProperty("textoInfoCriatura").objectReferenceValue = tmpInfo;
        soPainel.FindProperty("areaListaCriaturas").objectReferenceValue = contentGrid;
        soPainel.FindProperty("areaBotoesRunas").objectReferenceValue = areaRunas;
        soPainel.ApplyModifiedPropertiesWithoutUndo();

        root.SetActive(false);

        GameObject ctrlGo = new GameObject("ControleBestiario");
        Undo.RegisterCreatedObjectUndo(ctrlGo, "Controle Bestiario");
        ctrlGo.transform.SetParent(canvas.transform, false);
        ctrlGo.AddComponent<RectTransform>();
        LobbyBestiarioController ctrl = ctrlGo.AddComponent<LobbyBestiarioController>();

        Button btnAbrir = botaoGo.GetComponent<Button>();
        SerializedObject soBtnLimpar = new SerializedObject(btnAbrir);
        SerializedProperty callsAbrir = soBtnLimpar.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        if (callsAbrir != null && callsAbrir.isArray)
            callsAbrir.ClearArray();
        soBtnLimpar.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject soCtrl = new SerializedObject(ctrl);
        soCtrl.FindProperty("painelRaiz").objectReferenceValue = root;
        soCtrl.FindProperty("painelBestiario").objectReferenceValue = painel;
        soCtrl.FindProperty("botaoAbrir").objectReferenceValue = btnAbrir;
        soCtrl.FindProperty("botaoFechar").objectReferenceValue = btnFechar;
        soCtrl.ApplyModifiedPropertiesWithoutUndo();

        Undo.RecordObject(btnAbrir, "Bestiario OnClick");
        UnityEventTools.AddVoidPersistentListener(btnAbrir.onClick, ctrl.AbrirPainel);
        Undo.RecordObject(btnFechar, "Bestiario Fechar");
        UnityEventTools.AddVoidPersistentListener(btnFechar.onClick, ctrl.FecharPainel);

        Undo.CollapseUndoOperations(grupo);
        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(ctrlGo);
        EditorUtility.SetDirty(btnAbrir.gameObject);
        Selection.activeGameObject = root;
        if (salvarCena)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[Bestiario] Painel criado e botoes ligados: " + EditorSceneManager.GetActiveScene().path);
    }

    private static GameObject EncontrarPorNome(string nome)
    {
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name == nome)
                return t.gameObject;
        }
        return null;
    }

    private static void CriarBarraSuperior(Transform parent, out TextMeshProUGUI pontos, out Button btnFechar)
    {
        GameObject bar = new GameObject("BarraSuperior");
        Undo.RegisterCreatedObjectUndo(bar, "Barra");
        bar.transform.SetParent(parent, false);
        RectTransform rt = bar.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 44);
        rt.anchoredPosition = new Vector2(0, 0);

        HorizontalLayoutGroup h = bar.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(12, 12, 6, 6);
        h.childAlignment = TextAnchor.MiddleLeft;
        h.spacing = 12f;
        h.childControlWidth = false;
        h.childControlHeight = true;

        GameObject goTxt = new GameObject("TextoPontos");
        goTxt.transform.SetParent(bar.transform, false);
        pontos = goTxt.AddComponent<TextMeshProUGUI>();
        AplicarFonteTmp(pontos);
        pontos.text = "Pontos bestiario: 0";
        pontos.fontSize = 20f;
        LayoutElement leP = goTxt.AddComponent<LayoutElement>();
        leP.flexibleWidth = 1f;
        leP.minWidth = 200f;

        GameObject goBtn = new GameObject("BotaoFecharBestiario");
        goBtn.transform.SetParent(bar.transform, false);
        Image imgB = goBtn.AddComponent<Image>();
        imgB.color = new Color(0.45f, 0.15f, 0.15f, 1f);
        btnFechar = goBtn.AddComponent<Button>();
        btnFechar.targetGraphic = imgB;
        LayoutElement leB = goBtn.AddComponent<LayoutElement>();
        leB.preferredWidth = 100f;
        leB.preferredHeight = 32f;

        GameObject goLbl = new GameObject("Texto");
        goLbl.transform.SetParent(goBtn.transform, false);
        RectTransform rtl = goLbl.AddComponent<RectTransform>();
        rtl.anchorMin = Vector2.zero;
        rtl.anchorMax = Vector2.one;
        rtl.offsetMin = Vector2.zero;
        rtl.offsetMax = Vector2.zero;
        TextMeshProUGUI tmpB = goLbl.AddComponent<TextMeshProUGUI>();
        AplicarFonteTmp(tmpB);
        tmpB.text = "Fechar";
        tmpB.alignment = TextAlignmentOptions.Midline;
        tmpB.fontSize = 16f;
    }

    private static GameObject CriarScroll(Transform parent, out RectTransform contentOut)
    {
        GameObject go = new GameObject("ScrollListaCriaturas");
        Undo.RegisterCreatedObjectUndo(go, "Scroll");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.12f, 0.12f, 0.14f, 1f);

        ScrollRect sr = go.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(go.transform, false);
        RectTransform vrt = viewport.AddComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = new Vector2(6, 6);
        vrt.offsetMax = new Vector2(-6, -6);
        viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0.02f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject content = new GameObject("ContentListaCriaturas");
        content.transform.SetParent(viewport.transform, false);
        RectTransform crt = content.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 1);
        crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(0.5f, 1);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0, 600);

        sr.viewport = vrt;
        sr.content = crt;
        contentOut = crt;
        return go;
    }

    private static GameObject CriarDetalhe(Transform parent, out TextMeshProUGUI titulo, out TextMeshProUGUI info, out RectTransform areaRunas)
    {
        GameObject go = new GameObject("DetalheRunas");
        Undo.RegisterCreatedObjectUndo(go, "Detalhe");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.1f, 0.11f, 0.14f, 1f);

        VerticalLayoutGroup vl = go.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(12, 12, 12, 12);
        vl.spacing = 10f;
        vl.childControlWidth = true;
        vl.childControlHeight = true;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        GameObject goT = new GameObject("TituloRunas");
        goT.transform.SetParent(go.transform, false);
        titulo = goT.AddComponent<TextMeshProUGUI>();
        AplicarFonteTmp(titulo);
        titulo.text = "Runas";
        titulo.fontSize = 22f;
        titulo.fontStyle = FontStyles.Bold;
        LayoutElement leT = goT.AddComponent<LayoutElement>();
        leT.preferredHeight = 32f;

        GameObject goI = new GameObject("InfoCriatura");
        goI.transform.SetParent(go.transform, false);
        info = goI.AddComponent<TextMeshProUGUI>();
        AplicarFonteTmp(info);
        info.text = "Selecione uma criatura.";
        info.fontSize = 15f;
        info.alignment = TextAlignmentOptions.TopLeft;
        LayoutElement leI = goI.AddComponent<LayoutElement>();
        leI.preferredHeight = 120f;
        leI.flexibleWidth = 1f;

        GameObject goA = new GameObject("AreaBotoesRunas");
        goA.transform.SetParent(go.transform, false);
        areaRunas = goA.AddComponent<RectTransform>();
        LayoutElement leA = goA.AddComponent<LayoutElement>();
        leA.flexibleHeight = 1f;
        leA.minHeight = 200f;

        return go;
    }

    private static void AplicarFonteTmp(TextMeshProUGUI tmp)
    {
        TMP_FontAsset fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (fonte != null)
            tmp.font = fonte;
        tmp.color = Color.white;
    }
}
#endif
