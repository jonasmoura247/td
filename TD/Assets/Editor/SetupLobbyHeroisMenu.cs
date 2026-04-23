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
/// TD → Setup Herois no Lobby Aberto
/// Adiciona HeroisManager, SistemaTendasManager e PainelHerois ao lobby ativo.
/// Conecta o BotaoHerois ja existente na cena ao PainelHeroisController.
/// Seguro para rodar multiplas vezes — reaproveita objetos existentes.
/// </summary>
public static class SetupLobbyHeroisMenu
{
    private const string PathHeroiDatabase   = "Assets/Data/Herois/HeroiDatabase.asset";
    private const string PathHeroiItemPrefab = "Assets/Prefabs/UI/Herois/HeroiItem.prefab";

    [MenuItem("TD/Setup Herois no Lobby Aberto")]
    public static void Executar()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[SetupLobbyHerois] Nenhuma cena aberta.");
            return;
        }

        if (Application.isPlaying)
        {
            Debug.LogWarning("[SetupLobbyHerois] Saia do Play Mode antes de rodar o setup.");
            return;
        }

        var heroiDatabase = AssetDatabase.LoadAssetAtPath<HeroiDatabase>(PathHeroiDatabase);
        if (heroiDatabase == null)
        {
            Debug.LogError($"[SetupLobbyHerois] HeroiDatabase nao encontrado em {PathHeroiDatabase}. Rode primeiro: TD → Setup Sistemas (Planos 1-7).");
            return;
        }

        var heroiItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PathHeroiItemPrefab);
        if (heroiItemPrefab == null)
            Debug.LogWarning($"[SetupLobbyHerois] HeroiItem.prefab nao encontrado em {PathHeroiItemPrefab}.");

        GarantirEventSystem();
        AdicionarHeroisManager(scene, heroiDatabase);
        AdicionarSistemaTendasManager(scene);

        Canvas canvas = EncontrarOuCriarCanvas();
        var controller = CriarOuAtualizarPainelHerois(canvas, heroiItemPrefab);
        ConectarBotaoHeroisExistente(canvas, controller);

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[SetupLobbyHerois] Setup concluido na cena '{scene.name}'.");
    }

    // ── Managers ──────────────────────────────────────────────────────────

    private static void AdicionarHeroisManager(Scene scene, HeroiDatabase db)
    {
        var existente = Object.FindFirstObjectByType<HeroisManager>(FindObjectsInactive.Include);
        if (existente != null)
        {
            AtribuirCampo(existente, "heroiDatabase", db);
            Debug.Log("[SetupLobbyHerois] HeroisManager ja existe — banco de dados atualizado.");
            return;
        }

        var go = new GameObject("HeroisManager");
        SceneManager.MoveGameObjectToScene(go, scene);
        var manager = go.AddComponent<HeroisManager>();
        AtribuirCampo(manager, "heroiDatabase", db);
        Undo.RegisterCreatedObjectUndo(go, "Criar HeroisManager");
        Debug.Log("[SetupLobbyHerois] HeroisManager criado.");
    }

    private static void AdicionarSistemaTendasManager(Scene scene)
    {
        var existente = Object.FindFirstObjectByType<SistemaTendasManager>(FindObjectsInactive.Include);
        if (existente != null)
        {
            Debug.Log("[SetupLobbyHerois] SistemaTendasManager ja existe.");
            return;
        }

        var go = new GameObject("SistemaTendasManager");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<SistemaTendasManager>();
        Undo.RegisterCreatedObjectUndo(go, "Criar SistemaTendasManager");
        Debug.Log("[SetupLobbyHerois] SistemaTendasManager criado.");
    }

    // ── UI: painel ────────────────────────────────────────────────────────

    private static PainelHeroisController CriarOuAtualizarPainelHerois(Canvas canvas, GameObject heroiItemPrefab)
    {
        var scene = SceneManager.GetActiveScene();
        var controllerGo = EncontrarOuCriarRaiz(scene, "PainelHeroisController");
        var controller = controllerGo.GetComponent<PainelHeroisController>()
                         ?? controllerGo.AddComponent<PainelHeroisController>();

        // Overlay escuro (tela cheia)
        var painelGo = EncontrarOuCriarFilho(canvas.transform, "PainelHerois",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        AnchorarTelaCheia(painelGo.GetComponent<RectTransform>());
        painelGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        // Janela central
        var janela = EncontrarOuCriarFilho(painelGo.transform, "JanelaHerois",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
            typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var janelaRect = janela.GetComponent<RectTransform>();
        janelaRect.anchorMin        = new Vector2(0.5f, 0.5f);
        janelaRect.anchorMax        = new Vector2(0.5f, 0.5f);
        janelaRect.pivot            = new Vector2(0.5f, 0.5f);
        janelaRect.sizeDelta        = new Vector2(560f, 440f);
        janelaRect.anchoredPosition = Vector2.zero;
        janela.GetComponent<Image>().color = new Color(0.1f, 0.13f, 0.19f, 0.97f);

        var vlg = janela.GetComponent<VerticalLayoutGroup>();
        vlg.padding               = new RectOffset(20, 20, 20, 20);
        vlg.spacing               = 10f;
        vlg.childControlWidth     = true;
        vlg.childControlHeight    = false;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;

        var fitter = janela.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit   = ContentSizeFitter.FitMode.Unconstrained;

        // Título
        var titulo = EncontrarOuCriarFilho(janela.transform, "Titulo",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        titulo.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 40f);
        titulo.GetComponent<LayoutElement>().preferredHeight = 40f;
        var tituloTmp = titulo.GetComponent<TextMeshProUGUI>();
        tituloTmp.font      = ObterFontePadraoTMP();
        tituloTmp.text      = "Herois Contratados";
        tituloTmp.fontSize  = 30;
        tituloTmp.alignment = TextAlignmentOptions.Center;
        tituloTmp.color     = Color.white;

        // Container dos itens
        var container = EncontrarOuCriarFilho(janela.transform, "ContainerHerois",
            typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        container.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 280f);
        container.GetComponent<LayoutElement>().preferredHeight = 280f;
        var containerVlg = container.GetComponent<VerticalLayoutGroup>();
        containerVlg.spacing               = 8f;
        containerVlg.childControlWidth     = true;
        containerVlg.childControlHeight    = false;
        containerVlg.childForceExpandWidth  = true;
        containerVlg.childForceExpandHeight = false;
        var containerFitter = container.GetComponent<ContentSizeFitter>();
        containerFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        containerFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        // Botão fechar dentro da janela
        var fecharGo = EncontrarOuCriarFilho(janela.transform, "BotaoFechar",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        fecharGo.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 44f);
        fecharGo.GetComponent<Image>().color = new Color(0.6f, 0.15f, 0.15f, 1f);
        fecharGo.GetComponent<LayoutElement>().preferredHeight = 44f;
        CriarOuAtualizarTextoFilho(fecharGo.transform, "Texto", "Fechar");
        var botaoFechar = fecharGo.GetComponent<Button>();
        botaoFechar.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(botaoFechar.onClick, controller.FecharPainel);
        EditorUtility.SetDirty(botaoFechar);

        // Wiring do controller
        AtribuirCampos(controller,
            ("painel",             (Object)painelGo),
            ("areaConteudoPainel", (Object)janelaRect),
            ("containerItens",     (Object)container.transform),
            ("prefabItemHeroi",    (Object)heroiItemPrefab));

        painelGo.SetActive(false);
        return controller;
    }

    // ── UI: botao existente ───────────────────────────────────────────────

    private static void ConectarBotaoHeroisExistente(Canvas canvas, PainelHeroisController controller)
    {
        var botaoTransform = BuscarRecursivo(canvas.transform, "BotaoHerois");
        if (botaoTransform == null)
        {
            Debug.LogWarning("[SetupLobbyHerois] 'BotaoHerois' nao encontrado no Canvas. Conecte manualmente o botao ao PainelHeroisController.AbrirPainel().");
            return;
        }

        var button = botaoTransform.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("[SetupLobbyHerois] 'BotaoHerois' encontrado mas nao tem componente Button.");
            return;
        }

        // Remove listeners persistentes antigos que apontem para AbrirPainel para evitar duplicata
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            if (button.onClick.GetPersistentMethodName(i) == "AbrirPainel")
                UnityEventTools.RemovePersistentListener(button.onClick, i);
        }

        UnityEventTools.AddPersistentListener(button.onClick, controller.AbrirPainel);
        EditorUtility.SetDirty(button);
        Debug.Log($"[SetupLobbyHerois] BotaoHerois '{botaoTransform.name}' conectado ao PainelHeroisController.AbrirPainel().");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Transform BuscarRecursivo(Transform parent, string nome)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var filho = parent.GetChild(i);
            if (filho.name == nome) return filho;
            var resultado = BuscarRecursivo(filho, nome);
            if (resultado != null) return resultado;
        }
        return null;
    }

    private static void GarantirEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(go, "Criar EventSystem");
    }

    private static Canvas EncontrarOuCriarCanvas()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas != null) return canvas;

        var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        return canvas;
    }

    private static GameObject EncontrarOuCriarRaiz(Scene scene, string nome)
    {
        foreach (var r in scene.GetRootGameObjects())
            if (r.name == nome) return r;
        var go = new GameObject(nome);
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    private static GameObject EncontrarOuCriarFilho(Transform parent, string nome, params System.Type[] tipos)
    {
        var filho = parent.Find(nome);
        GameObject go;
        if (filho != null)
        {
            go = filho.gameObject;
        }
        else
        {
            go = new GameObject(nome, tipos);
            go.transform.SetParent(parent, false);
        }
        foreach (var t in tipos)
            if (go.GetComponent(t) == null) go.AddComponent(t);
        return go;
    }

    private static void AnchorarTelaCheia(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void CriarOuAtualizarTextoFilho(Transform parent, string nome, string texto)
    {
        var go = EncontrarOuCriarFilho(parent, nome,
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.font      = ObterFontePadraoTMP();
        tmp.text      = texto;
        tmp.fontSize  = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
    }

    private static void AtribuirCampo(Object alvo, string campo, Object valor)
    {
        var so   = new SerializedObject(alvo);
        var prop = so.FindProperty(campo);
        if (prop != null) prop.objectReferenceValue = valor;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(alvo);
    }

    private static void AtribuirCampos(Object alvo, params (string campo, Object valor)[] pares)
    {
        var so = new SerializedObject(alvo);
        foreach (var (campo, valor) in pares)
        {
            var prop = so.FindProperty(campo);
            if (prop != null) prop.objectReferenceValue = valor;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(alvo);
    }

    private static TMP_FontAsset ObterFontePadraoTMP()
    {
        if (TMP_Settings.defaultFontAsset != null) return TMP_Settings.defaultFontAsset;
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        foreach (var guid in guids)
        {
            var fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (fonte != null) return fonte;
        }
        return null;
    }
}
#endif
