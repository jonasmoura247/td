#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Monta automaticamente os objetos da tenda na cena de mapa mundo aberta.
/// Ha duas opcoes de menu: setup normal e setup de teste com tenda forcada.
/// </summary>
public static class SetupTendaMapaTesteMenu
{
    private const string PathHeroiDatabase = "Assets/Data/Herois/HeroiDatabase.asset";
    private const string PathHeroiPirata = "Assets/Data/Herois/HeroiDef_Pirata.asset";
    private const string PathHeroiPirataPrefab = "Assets/Prefabs/Herois/HeroiPirata.prefab";
    private const string PathReliquiaDatabase = "Assets/Prefabs/Reliquias/ReliquiaDatabase.asset";
    private const string PathBotaoOpcaoPrefab = "Assets/Prefabs/UI/Tenda/TendaOpcaoBotao.prefab";
    private const string PathMarcadorPrefab = "Assets/Prefabs/Tendas/Square.prefab";
    private const string PathSpriteTenda = "Assets/Sprites/Tenda/Tenda.png";

    [MenuItem("TD/Setup Tenda na Cena Aberta")]
    public static void ExecutarNormal()
    {
        ExecutarInterno(true);
    }

    [MenuItem("TD/Setup Tenda de Teste na Cena Aberta")]
    public static void ExecutarTeste()
    {
        ExecutarInterno(true);
    }

    private static void ExecutarInterno(bool forcarTendaEmTeste)
    {
        GarantirPastas();

        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[SetupTendaTeste] Nenhuma cena aberta.");
            return;
        }

        if (Application.isPlaying)
        {
            Debug.LogWarning("[SetupTendaTeste] Saia do Play Mode antes de rodar o setup.");
            return;
        }

        GameObject marcadorPrefab = ConfigurarPrefabMarcador();
        GameObject botaoOpcaoPrefab = CriarOuAtualizarPrefabBotaoOpcao();
        HeroiDatabase heroiDatabase = AssetDatabase.LoadAssetAtPath<HeroiDatabase>(PathHeroiDatabase);
        ReliquiaDatabase reliquiaDatabase = AssetDatabase.LoadAssetAtPath<ReliquiaDatabase>(PathReliquiaDatabase);

        if (marcadorPrefab == null || botaoOpcaoPrefab == null)
        {
            Debug.LogError("[SetupTendaTeste] Setup abortado por falta de prefab obrigatorio.");
            return;
        }

        ValidarHeroiPirata();
        GarantirEventSystem();

        Canvas canvas = EncontrarOuCriarCanvas();
        var painel = EncontrarOuCriarPainelTenda(canvas.transform, out RectTransform janela);

        TMP_Text titulo = EncontrarOuCriarTexto(janela, "Titulo", "Tenda");
        TMP_Text descricao = EncontrarOuCriarTexto(janela, "Descricao", "Escolha suas recompensas.");

        GameObject secaoReliquias = EncontrarOuCriarSecao(janela, "SecaoReliquias", "Reliquias", out RectTransform contentReliquias);
        GameObject secaoHerois = EncontrarOuCriarSecao(janela, "SecaoHerois", "Herois", out RectTransform contentHerois);
        Button botaoFechar = EncontrarOuCriarBotao(janela, "Fechar", "Fechar");

        var controllerGo = EncontrarOuCriarObjeto(scene, "PainelTendaController");
        var controller = controllerGo.GetComponent<PainelTendaController>();
        if (controller == null)
            controller = controllerGo.AddComponent<PainelTendaController>();

        ConfigurarPainelController(controller, painel, titulo, descricao, secaoReliquias, contentReliquias, secaoHerois, contentHerois, botaoOpcaoPrefab, reliquiaDatabase, heroiDatabase);
        ConfigurarBotaoFechar(botaoFechar, controller);

        var bootstrapGo = EncontrarOuCriarObjeto(scene, "TendaMapaBootstrap");
        var bootstrap = bootstrapGo.GetComponent<TendaMapaBootstrap>();
        if (bootstrap == null)
            bootstrap = bootstrapGo.AddComponent<TendaMapaBootstrap>();

        ConfigurarBootstrap(bootstrap, controller, reliquiaDatabase, heroiDatabase, marcadorPrefab, forcarTendaEmTeste);

        painel.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene);

        Debug.Log(forcarTendaEmTeste
            ? "[SetupTendaTeste] Setup de teste concluido na cena aberta."
            : "[SetupTendaTeste] Setup normal concluido na cena aberta.");
        Selection.activeObject = bootstrapGo;
    }

    private static void GarantirPastas()
    {
        GarantirPasta("Assets/Prefabs", "UI");
        GarantirPasta("Assets/Prefabs/UI", "Tenda");
        GarantirPasta("Assets/Prefabs", "Tendas");
    }

    private static void GarantirPasta(string pai, string filho)
    {
        if (!AssetDatabase.IsValidFolder($"{pai}/{filho}"))
            AssetDatabase.CreateFolder(pai, filho);
    }

    private static GameObject ConfigurarPrefabMarcador()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PathMarcadorPrefab);
        if (prefab == null)
        {
            Debug.LogError($"[SetupTendaTeste] Prefab do marcador nao encontrado: {PathMarcadorPrefab}");
            return null;
        }

        Sprite spriteTenda = CarregarSpriteTenda();

        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        root.name = "TendaMarcadorTeste";

        if (root.GetComponent<TendaMarcadorMapa>() == null)
            root.AddComponent<TendaMarcadorMapa>();
        if (root.GetComponent<Collider2D>() == null)
            root.AddComponent<BoxCollider2D>();

        var sr = root.GetComponent<SpriteRenderer>() ?? root.AddComponent<SpriteRenderer>();
        if (spriteTenda != null)
        {
            sr.sprite = spriteTenda;
            sr.color = Color.white;
            sr.sortingOrder = 1;

            var col = root.GetComponent<BoxCollider2D>() ?? root.AddComponent<BoxCollider2D>();
            var bounds = spriteTenda.bounds;
            col.size = new Vector2(bounds.size.x, bounds.size.y);
            col.offset = new Vector2(bounds.center.x, bounds.center.y);
        }
        else
        {
            sr.color = new Color(1f, 0.55f, 0.1f, 1f);
            Debug.LogWarning($"[SetupTendaTeste] Sprite da tenda nao encontrada em: {PathSpriteTenda}");
        }

        root.transform.localScale = new Vector3(0.381f, 0.381f, 1f);
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
        return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }

    private static Sprite CarregarSpriteTenda()
    {
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(PathSpriteTenda);
        for (int i = 0; i < allAssets.Length; i++)
        {
            if (allAssets[i] is Sprite s)
                return s;
        }
        return null;
    }

    private static GameObject CriarOuAtualizarPrefabBotaoOpcao()
    {
        TMP_FontAsset fonte = ObterFontePadraoTMP();
        GameObject existente = AssetDatabase.LoadAssetAtPath<GameObject>(PathBotaoOpcaoPrefab);
        bool prefabExistente = existente != null;
        GameObject root = prefabExistente
            ? PrefabUtility.LoadPrefabContents(PathBotaoOpcaoPrefab)
            : CriarEstruturaBotaoOpcao(fonte);

        root.name = "TendaOpcaoBotao";
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 44f);

        if (root.GetComponent<CanvasRenderer>() == null)
            root.AddComponent<CanvasRenderer>();

        var image = root.GetComponent<Image>() ?? root.AddComponent<Image>();
        image.color = new Color(0.2f, 0.28f, 0.38f, 0.95f);

        var button = root.GetComponent<Button>() ?? root.AddComponent<Button>();
        var botaoCtrl = root.GetComponent<TendaOpcaoBotaoController>() ?? root.AddComponent<TendaOpcaoBotaoController>();

        Transform textoTransform = root.transform.Find("Texto");
        if (textoTransform == null)
        {
            var textoGo = new GameObject("Texto", typeof(RectTransform));
            textoGo.transform.SetParent(root.transform, false);
            textoTransform = textoGo.transform;
        }

        var textoRect = (RectTransform)textoTransform;
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = new Vector2(12f, 6f);
        textoRect.offsetMax = new Vector2(-12f, -6f);

        if (textoTransform.GetComponent<CanvasRenderer>() == null)
            textoTransform.gameObject.AddComponent<CanvasRenderer>();

        var textoTmp = textoTransform.GetComponent<TextMeshProUGUI>() ?? textoTransform.gameObject.AddComponent<TextMeshProUGUI>();
        textoTmp.font = fonte;
        textoTmp.fontSize = 24;
        textoTmp.alignment = TextAlignmentOptions.Center;
        textoTmp.text = "Opcao";
        textoTmp.color = Color.white;

        if (textoTransform.GetComponent<LayoutElement>() == null)
            textoTransform.gameObject.AddComponent<LayoutElement>();

        ConfigurarCamposSerializados(botaoCtrl, ("botao", button), ("textoTmp", textoTmp));

        PrefabUtility.SaveAsPrefabAsset(root, PathBotaoOpcaoPrefab);

        if (prefabExistente)
            PrefabUtility.UnloadPrefabContents(root);
        else
            Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        return AssetDatabase.LoadAssetAtPath<GameObject>(PathBotaoOpcaoPrefab);
    }

    private static GameObject CriarEstruturaBotaoOpcao(TMP_FontAsset fonte)
    {
        var root = new GameObject("TendaOpcaoBotao", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(TendaOpcaoBotaoController));
        var textoGo = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textoGo.transform.SetParent(root.transform, false);
        var texto = textoGo.GetComponent<TextMeshProUGUI>();
        texto.font = fonte;
        return root;
    }

    private static void ValidarHeroiPirata()
    {
        var def = AssetDatabase.LoadAssetAtPath<HeroiDef>(PathHeroiPirata);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PathHeroiPirataPrefab);
        if (def == null || prefab == null)
            return;

        if (def.prefab != prefab)
        {
            def.prefab = prefab;
            EditorUtility.SetDirty(def);
            AssetDatabase.SaveAssets();
        }
    }

    private static void GarantirEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
            return;

        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(go, "Criar EventSystem");
    }

    private static Canvas EncontrarOuCriarCanvas()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas != null)
            return canvas;

        var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static GameObject EncontrarOuCriarPainelTenda(Transform canvasTransform, out RectTransform janela)
    {
        var painel = EncontrarOuCriarFilho(canvasTransform, "PainelTenda", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var painelRect = painel.GetComponent<RectTransform>();
        painelRect.anchorMin = Vector2.zero;
        painelRect.anchorMax = Vector2.one;
        painelRect.offsetMin = Vector2.zero;
        painelRect.offsetMax = Vector2.zero;

        var painelImage = painel.GetComponent<Image>();
        painelImage.color = new Color(0f, 0f, 0f, 0.5f);
        painelImage.raycastTarget = true;

        var janelaGo = EncontrarOuCriarFilho(painel.transform, "JanelaTenda", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        janela = janelaGo.GetComponent<RectTransform>();
        janela.anchorMin = new Vector2(0.5f, 0.5f);
        janela.anchorMax = new Vector2(0.5f, 0.5f);
        janela.pivot = new Vector2(0.5f, 0.5f);
        janela.sizeDelta = new Vector2(720f, 560f);
        janela.anchoredPosition = Vector2.zero;

        var janelaImage = janelaGo.GetComponent<Image>();
        janelaImage.color = new Color(0.1f, 0.14f, 0.2f, 0.97f);

        var layout = janelaGo.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 12f;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        var fitter = janelaGo.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        return painel;
    }

    private static TMP_Text EncontrarOuCriarTexto(RectTransform parent, string nome, string texto)
    {
        var go = EncontrarOuCriarFilho(parent, nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, nome == "Descricao" ? 64f : 42f);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.font = ObterFontePadraoTMP();
        tmp.text = texto;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.alignment = nome == "Titulo" ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft;
        tmp.fontSize = nome == "Titulo" ? 34 : 24;

        var layout = go.GetComponent<LayoutElement>();
        layout.preferredHeight = rect.sizeDelta.y;
        return tmp;
    }

    private static GameObject EncontrarOuCriarSecao(RectTransform parent, string nome, string titulo, out RectTransform content)
    {
        var secao = EncontrarOuCriarFilho(parent, nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
        var secaoRect = secao.GetComponent<RectTransform>();
        secaoRect.sizeDelta = new Vector2(0f, 180f);

        var image = secao.GetComponent<Image>();
        image.color = new Color(0.16f, 0.2f, 0.28f, 0.9f);

        var layout = secao.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 8f;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        var tituloTmp = EncontrarOuCriarTexto(secaoRect, "TituloSecao", titulo);
        tituloTmp.alignment = TextAlignmentOptions.Left;
        tituloTmp.fontSize = 26;

        var contentGo = EncontrarOuCriarFilho(secao.transform, nome == "SecaoReliquias" ? "ContentReliquias" : "ContentHerois",
            typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        content = contentGo.GetComponent<RectTransform>();
        content.sizeDelta = new Vector2(0f, 120f);

        var contentLayout = contentGo.GetComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 6f;
        contentLayout.childControlHeight = false;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;

        var fitter = contentGo.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var layoutElement = contentGo.GetComponent<LayoutElement>();
        layoutElement.minHeight = 80f;

        return secao;
    }

    private static Button EncontrarOuCriarBotao(RectTransform parent, string nome, string texto)
    {
        var go = EncontrarOuCriarFilho(parent, nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220f, 48f);

        var image = go.GetComponent<Image>();
        image.color = new Color(0.3f, 0.42f, 0.2f, 1f);

        var button = go.GetComponent<Button>();
        var textTransform = go.transform.Find("Texto");
        if (textTransform == null)
        {
            textTransform = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)).transform;
            textTransform.SetParent(go.transform, false);
        }

        var txtRect = (RectTransform)textTransform;
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        var tmp = textTransform.GetComponent<TextMeshProUGUI>() ?? textTransform.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = ObterFontePadraoTMP();
        tmp.text = texto;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        var layout = go.GetComponent<LayoutElement>();
        layout.preferredHeight = 48f;

        return button;
    }

    private static void ConfigurarPainelController(
        PainelTendaController controller,
        GameObject painel,
        TMP_Text titulo,
        TMP_Text descricao,
        GameObject secaoReliquias,
        RectTransform contentReliquias,
        GameObject secaoHerois,
        RectTransform contentHerois,
        GameObject prefabBotaoOpcao,
        ReliquiaDatabase reliquiaDatabase,
        HeroiDatabase heroiDatabase)
    {
        ConfigurarCamposSerializados(
            controller,
            ("painel", painel),
            ("textoTitulo", titulo),
            ("textoDescricao", descricao),
            ("secaoReliquias", secaoReliquias),
            ("containerReliquias", contentReliquias),
            ("prefabBotaoOpcao", prefabBotaoOpcao),
            ("secaoHerois", secaoHerois),
            ("containerHerois", contentHerois),
            ("reliquiaDatabase", reliquiaDatabase),
            ("heroiDatabase", heroiDatabase)
        );
    }

    private static void ConfigurarBotaoFechar(Button botaoFechar, PainelTendaController controller)
    {
        botaoFechar.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(botaoFechar.onClick, controller.FecharTenda);
        EditorUtility.SetDirty(botaoFechar);
    }

    private static void ConfigurarBootstrap(TendaMapaBootstrap bootstrap, PainelTendaController controller, ReliquiaDatabase reliquiaDatabase, HeroiDatabase heroiDatabase, GameObject marcadorPrefab, bool forcarTendaEmTeste)
    {
        var marcadorComp = marcadorPrefab != null ? marcadorPrefab.GetComponent<TendaMarcadorMapa>() : null;
        ConfigurarCamposSerializados(
            bootstrap,
            ("painelTenda", controller),
            ("reliquiaDatabase", reliquiaDatabase),
            ("heroiDatabase", heroiDatabase),
            ("prefabMarcadorTenda", marcadorComp)
        );

        var so = new SerializedObject(bootstrap);
        var offsetProp = so.FindProperty("offsetMarcador");
        var forcarProp = so.FindProperty("forcarTendaEmTeste");
        var tipoProp = so.FindProperty("tipoTendaForcada");
        if (offsetProp != null)
            offsetProp.vector3Value = new Vector3(1.5f, 0.75f, 0f);
        if (forcarProp != null)
            forcarProp.boolValue = forcarTendaEmTeste;
        if (tipoProp != null)
            tipoProp.enumValueIndex = (int)TipoTenda.Mista;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(bootstrap);
    }

    private static void ConfigurarCamposSerializados(Object alvo, params (string nome, Object valor)[] refs)
    {
        var so = new SerializedObject(alvo);
        foreach (var (nome, valor) in refs)
        {
            var prop = so.FindProperty(nome);
            if (prop != null)
                prop.objectReferenceValue = valor;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(alvo);
    }

    private static GameObject EncontrarOuCriarObjeto(Scene scene, string nome)
    {
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == nome)
                return roots[i];
        }

        var go = new GameObject(nome);
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    private static GameObject EncontrarOuCriarFilho(Transform parent, string nome, params System.Type[] componentTypes)
    {
        Transform child = parent.Find(nome);
        GameObject go;
        if (child != null)
        {
            go = child.gameObject;
        }
        else
        {
            go = new GameObject(nome, componentTypes);
            go.transform.SetParent(parent, false);
        }

        for (int i = 0; i < componentTypes.Length; i++)
        {
            if (go.GetComponent(componentTypes[i]) == null)
                go.AddComponent(componentTypes[i]);
        }

        return go;
    }

    private static TMP_FontAsset ObterFontePadraoTMP()
    {
        if (TMP_Settings.defaultFontAsset != null)
            return TMP_Settings.defaultFontAsset;

        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fonte != null)
                return fonte;
        }

        return null;
    }
}
#endif
