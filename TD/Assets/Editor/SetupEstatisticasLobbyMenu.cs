#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menu TD/Estatisticas: cria/recria o painel de estatisticas em todos os lobbies com um clique.
/// Regras criticas:
///   - Sem emoji nem caracteres acentuados em strings TMP runtime.
///   - Nao chamar AddComponent RectTransform em GOs ja criados via CriarGOUI().
///   - Guard contra Play Mode.
/// </summary>
public static class SetupEstatisticasLobbyMenu
{
    private static readonly string[] CenasLobby =
    {
        "Assets/Scenes/Lobby/LobbyGoblins/LobbyGoblin.unity",
        "Assets/Scenes/Lobby/LobbyHumanos/LobbyHumano.unity",
        "Assets/Scenes/Lobby/LobbyElfos/LobbyElfo.unity",
        "Assets/Scenes/Lobby/LobbyAnoes/LobbyAnoes.unity"
    };

    [MenuItem("TD/Estatisticas/Configurar em TODOS os lobbies")]
    public static void ConfigurarTodosLobbies()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Estatisticas", "Saia do Play Mode antes de executar este menu.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Estatisticas",
                "Abre e altera as 4 cenas de lobby (PainelEstatisticasRoot + ControleEstatisticas). Continuar?",
                "Sim", "Nao"))
            return;

        string cenaAtual = EditorSceneManager.GetActiveScene().path;
        try
        {
            foreach (var caminho in CenasLobby)
            {
                if (!System.IO.File.Exists(caminho))
                {
                    Debug.LogWarning("[Estatisticas] Cena nao encontrada: " + caminho);
                    continue;
                }
                EditorSceneManager.OpenScene(caminho);
                ConfigurarInterno(salvarCena: true);
            }
        }
        finally
        {
            if (!string.IsNullOrEmpty(cenaAtual) && System.IO.File.Exists(cenaAtual))
                EditorSceneManager.OpenScene(cenaAtual);
        }
        Debug.Log("[Estatisticas] Todas as cenas de lobby processadas.");
    }

    [MenuItem("TD/Estatisticas/Configurar na cena atual")]
    public static void ConfigurarCenaAtual()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Estatisticas", "Saia do Play Mode antes de executar este menu.", "OK");
            return;
        }
        ConfigurarInterno(salvarCena: false);
    }

    // -----------------------------------------------------------------------

    private static void ConfigurarInterno(bool salvarCena)
    {
        // Encontrar BotaoEstatisticas
        GameObject botaoGo = EncontrarPorNome("BotaoEstatisticas");
        if (botaoGo == null)
        {
            Debug.LogWarning("[Estatisticas] BotaoEstatisticas nao encontrado na cena.");
            return;
        }

        Canvas canvas = botaoGo.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[Estatisticas] BotaoEstatisticas nao esta sob um Canvas.");
            return;
        }

        // Limpeza de versoes anteriores (busca recursiva por componente)
        foreach (var antigo in canvas.GetComponentsInChildren<PainelEstatisticas>(true))
            Undo.DestroyObjectImmediate(antigo.gameObject);
        foreach (var antigo in canvas.GetComponentsInChildren<LobbyEstatisticasController>(true))
            Undo.DestroyObjectImmediate(antigo.gameObject);

        // Tambem remover pelos nomes canonicos (caso existam sem os componentes)
        RemoverSeExistir(canvas.transform, "PainelEstatisticasRoot");
        RemoverSeExistir(canvas.transform, "ControleEstatisticas");

        Undo.SetCurrentGroupName("Estatisticas UI Lobby");

        // ---- Painel principal ----
        GameObject raiz = CriarGOUI("PainelEstatisticasRoot", canvas.transform);
        var rtRaiz = raiz.GetComponent<RectTransform>();
        rtRaiz.anchorMin = Vector2.zero;
        rtRaiz.anchorMax = Vector2.one;
        rtRaiz.offsetMin = Vector2.zero;
        rtRaiz.offsetMax = Vector2.zero;

        var imgRaiz = raiz.AddComponent<Image>();
        imgRaiz.color = new Color(0.05f, 0.06f, 0.09f, 0.97f);
        imgRaiz.raycastTarget = true;

        var painelComp = raiz.AddComponent<PainelEstatisticas>();

        // ---- Barra de topo ----
        GameObject topBar = CriarGOUI("TopBar", raiz.transform);
        var rtTop = topBar.GetComponent<RectTransform>();
        rtTop.anchorMin = new Vector2(0, 1);
        rtTop.anchorMax = new Vector2(1, 1);
        rtTop.pivot     = new Vector2(0.5f, 1f);
        rtTop.sizeDelta = new Vector2(0, 48f);
        rtTop.anchoredPosition = Vector2.zero;
        var imgTop = topBar.AddComponent<Image>();
        imgTop.color = new Color(0.1f, 0.1f, 0.15f, 1f);

        // Titulo
        GameObject tituloGo = CriarGOUI("Titulo", topBar.transform);
        var rtTitulo = tituloGo.GetComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0, 0);
        rtTitulo.anchorMax = new Vector2(0.8f, 1);
        rtTitulo.offsetMin = new Vector2(12, 0);
        rtTitulo.offsetMax = Vector2.zero;
        var tmpTitulo = tituloGo.AddComponent<TextMeshProUGUI>();
        tmpTitulo.text      = "Estatisticas do Jogador";
        tmpTitulo.fontSize  = 20f;
        tmpTitulo.fontStyle = FontStyles.Bold;
        tmpTitulo.color     = Color.white;
        tmpTitulo.alignment = TextAlignmentOptions.MidlineLeft;

        // Botao Fechar
        GameObject btnFecharGo = CriarGOUI("BotaoFechar", topBar.transform);
        var rtFechar = btnFecharGo.GetComponent<RectTransform>();
        rtFechar.anchorMin      = new Vector2(1, 0.1f);
        rtFechar.anchorMax      = new Vector2(1, 0.9f);
        rtFechar.pivot          = new Vector2(1, 0.5f);
        rtFechar.sizeDelta      = new Vector2(80, 0);
        rtFechar.anchoredPosition = new Vector2(-8, 0);
        var imgBtnFechar = btnFecharGo.AddComponent<Image>();
        imgBtnFechar.color = new Color(0.7f, 0.15f, 0.1f, 1f);
        var btnFechar = btnFecharGo.AddComponent<Button>();
        GameObject txtFecharGo = CriarGOUI("Text", btnFecharGo.transform);
        StretchFull(txtFecharGo.GetComponent<RectTransform>());
        var tmpFechar = txtFecharGo.AddComponent<TextMeshProUGUI>();
        tmpFechar.text      = "Fechar";
        tmpFechar.fontSize  = 16f;
        tmpFechar.color     = Color.white;
        tmpFechar.alignment = TextAlignmentOptions.Center;

        // ---- Barra de abas ----
        GameObject abasBar = CriarGOUI("AbasBar", raiz.transform);
        var rtAbas = abasBar.GetComponent<RectTransform>();
        rtAbas.anchorMin      = new Vector2(0, 1);
        rtAbas.anchorMax      = new Vector2(1, 1);
        rtAbas.pivot          = new Vector2(0.5f, 1f);
        rtAbas.sizeDelta      = new Vector2(0, 40f);
        rtAbas.anchoredPosition = new Vector2(0, -48f);
        var imgAbas = abasBar.AddComponent<Image>();
        imgAbas.color = new Color(0.08f, 0.09f, 0.12f, 1f);
        var hlAbas = abasBar.AddComponent<HorizontalLayoutGroup>();
        hlAbas.childAlignment       = TextAnchor.MiddleLeft;
        hlAbas.childControlWidth    = true;
        hlAbas.childControlHeight   = true;
        hlAbas.childForceExpandWidth  = true;
        hlAbas.childForceExpandHeight = true;
        hlAbas.spacing = 2f;
        hlAbas.padding = new RectOffset(4, 4, 4, 4);

        Button btnGeral    = CriarBotaoAba("BotaoAba_Geral",    abasBar.transform, "Geral");
        Button btnCombate  = CriarBotaoAba("BotaoAba_Combate",  abasBar.transform, "Combate");
        Button btnRecursos = CriarBotaoAba("BotaoAba_Recursos", abasBar.transform, "Recursos");
        Button btnPorRaca  = CriarBotaoAba("BotaoAba_PorRaca",  abasBar.transform, "Por Raca");

        // ---- Container dos paineis ----
        GameObject container = CriarGOUI("ContainerPaineis", raiz.transform);
        var rtContainer = container.GetComponent<RectTransform>();
        rtContainer.anchorMin = Vector2.zero;
        rtContainer.anchorMax = Vector2.one;
        rtContainer.offsetMin = new Vector2(0, 0);
        rtContainer.offsetMax = new Vector2(0, -88f);  // top bar 48 + abas 40

        // ---- 4 Abas ----
        GameObject painelGeral    = CriarPainelAba("PainelGeral",    container.transform, ativo: true);
        GameObject painelCombate  = CriarPainelAba("PainelCombate",  container.transform, ativo: false);
        GameObject painelRecursos = CriarPainelAba("PainelRecursos", container.transform, ativo: false);
        GameObject painelPorRaca  = CriarPainelAba("PainelPorRaca",  container.transform, ativo: false);

        // ---- Conteudo Geral ----
        Transform geralContent = ObterScrollContent(painelGeral);

        TextMeshProUGUI tmpTotalPartidas   = CriarLabel(geralContent, "LabelTotalPartidas",   "Partidas: 0");
        TextMeshProUGUI tmpTotalVitorias   = CriarLabel(geralContent, "LabelTotalVitorias",   "Vitorias: 0");
        TextMeshProUGUI tmpTotalDerrotas   = CriarLabel(geralContent, "LabelTotalDerrotas",   "Derrotas: 0");
        TextMeshProUGUI tmpWinRate         = CriarLabel(geralContent, "LabelWinRate",         "Win Rate: 0%");
        TextMeshProUGUI tmpSequencia       = CriarLabel(geralContent, "LabelSequencia",       "Sequencia atual: 0");
        TextMeshProUGUI tmpMelhorSequencia = CriarLabel(geralContent, "LabelMelhorSequencia", "Melhor sequencia: 0");
        TextMeshProUGUI tmpUltimaPartida   = CriarLabel(geralContent, "LabelUltimaPartida",   "Ultima partida: --");
        CriarSeparador(geralContent);
        TextMeshProUGUI tmpTempoTotal  = CriarLabel(geralContent, "LabelTempoTotal",  "Tempo total: 0s");
        TextMeshProUGUI tmpTempoMedio  = CriarLabel(geralContent, "LabelTempoMedio",  "Tempo medio: 0s");
        TextMeshProUGUI tmpTempoMaior  = CriarLabel(geralContent, "LabelTempoMaior",  "Partida mais longa: 0s");
        TextMeshProUGUI tmpTempoMenor  = CriarLabel(geralContent, "LabelTempoMenor",  "Partida mais curta: --");

        // ---- Conteudo Combate ----
        Transform combateContent = ObterScrollContent(painelCombate);

        TextMeshProUGUI tmpTotalMonstros    = CriarLabel(combateContent, "LabelTotalMonstros",    "Monstros mortos: 0");
        TextMeshProUGUI tmpKillsMin         = CriarLabel(combateContent, "LabelKillsMin",         "Kills/min: 0.0");
        CriarSeparador(combateContent);
        TextMeshProUGUI tmpGoblins  = CriarLabel(combateContent, "LabelGoblins",  "Goblins mortos: 0");
        TextMeshProUGUI tmpHumanos  = CriarLabel(combateContent, "LabelHumanos",  "Humanos mortos: 0");
        TextMeshProUGUI tmpElfos    = CriarLabel(combateContent, "LabelElfos",    "Elfos mortos: 0");
        TextMeshProUGUI tmpAnoes    = CriarLabel(combateContent, "LabelAnoes",    "Anoes mortos: 0");
        CriarSeparador(combateContent);
        TextMeshProUGUI tmpMaisMorta      = CriarLabel(combateContent, "LabelMaisMorta",      "Unidade mais morta: --");
        TextMeshProUGUI tmpWaves          = CriarLabel(combateContent, "LabelWaves",          "Waves completadas: 0");
        TextMeshProUGUI tmpMaiorWave      = CriarLabel(combateContent, "LabelMaiorWave",      "Maior wave atingida: 0");
        TextMeshProUGUI tmpTorresConst    = CriarLabel(combateContent, "LabelTorresConst",    "Torres construidas: 0");
        TextMeshProUGUI tmpTorresDestr    = CriarLabel(combateContent, "LabelTorresDestr",    "Torres destruidas: 0");

        // ---- Conteudo Recursos ----
        Transform recursosContent = ObterScrollContent(painelRecursos);

        TextMeshProUGUI tmpOuroTotal       = CriarLabel(recursosContent, "LabelOuroTotal",       "Ouro acumulado total: 0");
        TextMeshProUGUI tmpOuroGasto       = CriarLabel(recursosContent, "LabelOuroGasto",       "Ouro gasto total: 0");
        TextMeshProUGUI tmpMaiorOuro       = CriarLabel(recursosContent, "LabelMaiorOuro",       "Maior ouro simultaneo: 0");
        CriarSeparador(recursosContent);
        TextMeshProUGUI tmpXPTotal         = CriarLabel(recursosContent, "LabelXPTotal",         "XP total ganho: 0");
        TextMeshProUGUI tmpMaiorXP         = CriarLabel(recursosContent, "LabelMaiorXP",         "Maior XP em uma partida: 0");
        CriarSeparador(recursosContent);
        TextMeshProUGUI tmpPrimeiro        = CriarLabel(recursosContent, "LabelPrimeiro",        "Primeiro monstro morto: --");
        TextMeshProUGUI tmpUltimo          = CriarLabel(recursosContent, "LabelUltimo",          "Ultimo monstro morto: --");

        // ---- Conteudo Por Raca ----
        Transform porRacaContent = ObterScrollContent(painelPorRaca);

        var dadosRacas = new (TipoRaca raca, string titulo)[]
        {
            (TipoRaca.Goblin, "-- Goblin --"),
            (TipoRaca.Humano, "-- Humano --"),
            (TipoRaca.Elfo,   "-- Elfo --"),
            (TipoRaca.Anao,   "-- Anao --"),
        };

        var itens = new (TipoRaca raca, TextMeshProUGUI partidas, TextMeshProUGUI vitorias, TextMeshProUGUI derrotas,
            TextMeshProUGUI winRate, TextMeshProUGUI kills, TextMeshProUGUI ouro)[4];

        for (int i = 0; i < 4; i++)
        {
            var (raca, titulo) = dadosRacas[i];
            CriarTitulo(porRacaContent, "TituloRaca_" + raca, titulo);
            var partidas = CriarLabel(porRacaContent, "Label_" + raca + "_Partidas", "Partidas: 0");
            var vit      = CriarLabel(porRacaContent, "Label_" + raca + "_Vitorias", "Vitorias: 0");
            var der      = CriarLabel(porRacaContent, "Label_" + raca + "_Derrotas", "Derrotas: 0");
            var wr       = CriarLabel(porRacaContent, "Label_" + raca + "_WinRate",  "Win Rate: 0%");
            var kills    = CriarLabel(porRacaContent, "Label_" + raca + "_Kills",    "Kills: 0");
            var ouro     = CriarLabel(porRacaContent, "Label_" + raca + "_Ouro",     "Ouro: 0");
            if (i < 3) CriarSeparador(porRacaContent);
            itens[i] = (raca, partidas, vit, der, wr, kills, ouro);
        }

        // ---- Serializar PainelEstatisticas ----
        SerializedObject so = new SerializedObject(painelComp);

        // Abas - botoes
        SetObjProp(so, "botaoAbaGeral",    btnGeral);
        SetObjProp(so, "botaoAbaCombate",  btnCombate);
        SetObjProp(so, "botaoAbaRecursos", btnRecursos);
        SetObjProp(so, "botaoAbaPorRaca",  btnPorRaca);

        // Abas - paineis
        SetObjProp(so, "painelGeral",    painelGeral);
        SetObjProp(so, "painelCombate",  painelCombate);
        SetObjProp(so, "painelRecursos", painelRecursos);
        SetObjProp(so, "painelPorRaca",  painelPorRaca);

        // Geral
        SetObjProp(so, "textoTotalPartidas",   tmpTotalPartidas);
        SetObjProp(so, "textoTotalVitorias",   tmpTotalVitorias);
        SetObjProp(so, "textoTotalDerrotas",   tmpTotalDerrotas);
        SetObjProp(so, "textoWinRate",         tmpWinRate);
        SetObjProp(so, "textoSequencia",       tmpSequencia);
        SetObjProp(so, "textoMelhorSequencia", tmpMelhorSequencia);
        SetObjProp(so, "textoUltimaPartida",   tmpUltimaPartida);
        SetObjProp(so, "textoTempoTotal",      tmpTempoTotal);
        SetObjProp(so, "textoTempoMedio",      tmpTempoMedio);
        SetObjProp(so, "textoTempoMaior",      tmpTempoMaior);
        SetObjProp(so, "textoTempoMenor",      tmpTempoMenor);

        // Combate
        SetObjProp(so, "textoTotalMonstros",    tmpTotalMonstros);
        SetObjProp(so, "textoKillsPorMinuto",   tmpKillsMin);
        SetObjProp(so, "textoGoblinsMortos",    tmpGoblins);
        SetObjProp(so, "textoHumanosMortos",    tmpHumanos);
        SetObjProp(so, "textoElfosMortos",      tmpElfos);
        SetObjProp(so, "textoAnoesMortos",      tmpAnoes);
        SetObjProp(so, "textoUnidadeMaisMorta", tmpMaisMorta);
        SetObjProp(so, "textoWavesCompletadas", tmpWaves);
        SetObjProp(so, "textoMaiorWave",        tmpMaiorWave);
        SetObjProp(so, "textoTorresConstruidas",tmpTorresConst);
        SetObjProp(so, "textoTorresDestruidas", tmpTorresDestr);

        // Recursos
        SetObjProp(so, "textoOuroTotal",      tmpOuroTotal);
        SetObjProp(so, "textoOuroGasto",      tmpOuroGasto);
        SetObjProp(so, "textoMaiorOuro",      tmpMaiorOuro);
        SetObjProp(so, "textoTotalXP",        tmpXPTotal);
        SetObjProp(so, "textoMaiorXPPartida", tmpMaiorXP);
        SetObjProp(so, "textoPrimeiroMonstro",tmpPrimeiro);
        SetObjProp(so, "textoUltimoMonstro",  tmpUltimo);

        // itensRaca
        var propItens = so.FindProperty("itensRaca");
        propItens.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            var elem = propItens.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("raca").enumValueIndex = (int)itens[i].raca;
            elem.FindPropertyRelative("textoPartidas").objectReferenceValue = itens[i].partidas;
            elem.FindPropertyRelative("textoVitorias").objectReferenceValue = itens[i].vitorias;
            elem.FindPropertyRelative("textoDerrotas").objectReferenceValue = itens[i].derrotas;
            elem.FindPropertyRelative("textoWinRate").objectReferenceValue  = itens[i].winRate;
            elem.FindPropertyRelative("textoKills").objectReferenceValue    = itens[i].kills;
            elem.FindPropertyRelative("textoOuro").objectReferenceValue     = itens[i].ouro;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        // ---- ControleEstatisticas ----
        GameObject ctrlGo = CriarGOUI("ControleEstatisticas", canvas.transform);
        var ctrlComp = ctrlGo.AddComponent<LobbyEstatisticasController>();

        SerializedObject soCtrl = new SerializedObject(ctrlComp);
        SetObjProp(soCtrl, "painelRaiz",          raiz);
        SetObjProp(soCtrl, "painelEstatisticas",   painelComp);
        SetObjProp(soCtrl, "botaoAbrir",           botaoGo.GetComponent<Button>());
        SetObjProp(soCtrl, "botaoFechar",          btnFechar);
        soCtrl.ApplyModifiedPropertiesWithoutUndo();

        raiz.SetActive(false);

        if (salvarCena)
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        Debug.Log("[Estatisticas] Painel criado: " + EditorSceneManager.GetActiveScene().name);
    }

    // -----------------------------------------------------------------------
    // Helpers de criacao de UI

    /// <summary>Cria um GameObject com RectTransform e o aninha sob parent.</summary>
    private static GameObject CriarGOUI(string nome, Transform parent)
    {
        var go = new GameObject(nome);
        Undo.RegisterCreatedObjectUndo(go, nome);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Button CriarBotaoAba(string nome, Transform parent, string label)
    {
        GameObject go = CriarGOUI(nome, parent);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.18f, 0.19f, 0.24f, 1f);
        var btn = go.AddComponent<Button>();
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 32f;

        GameObject txtGo = CriarGOUI("Text", go.transform);
        StretchFull(txtGo.GetComponent<RectTransform>());
        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 15f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return btn;
    }

    private static GameObject CriarPainelAba(string nome, Transform parent, bool ativo)
    {
        GameObject go = CriarGOUI(nome, parent);
        var rt = go.GetComponent<RectTransform>();
        StretchFull(rt);
        go.SetActive(ativo);
        return go;
    }

    /// <summary>Cria um ScrollView simples dentro do painel e retorna o Content (Transform).</summary>
    private static Transform ObterScrollContent(GameObject painel)
    {
        GameObject scrollGo = CriarGOUI("ScrollView", painel.transform);
        var rtScroll = scrollGo.GetComponent<RectTransform>();
        StretchFull(rtScroll);

        var scroll = scrollGo.AddComponent<ScrollRect>();
        var imgScroll = scrollGo.AddComponent<Image>();
        imgScroll.color = new Color(0, 0, 0, 0.01f);

        // Viewport
        GameObject viewportGo = CriarGOUI("Viewport", scrollGo.transform);
        StretchFull(viewportGo.GetComponent<RectTransform>());
        var mask = viewportGo.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        viewportGo.AddComponent<Image>();

        // Content
        GameObject contentGo = CriarGOUI("Content", viewportGo.transform);
        var rtContent = contentGo.GetComponent<RectTransform>();
        rtContent.anchorMin = new Vector2(0, 1);
        rtContent.anchorMax = new Vector2(1, 1);
        rtContent.pivot     = new Vector2(0.5f, 1f);
        rtContent.sizeDelta = new Vector2(0, 0);

        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.padding               = new RectOffset(16, 16, 12, 12);
        vlg.spacing               = 8f;
        vlg.childAlignment        = TextAnchor.UpperLeft;
        vlg.childControlWidth     = true;
        vlg.childControlHeight    = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var csf = contentGo.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content   = rtContent;
        scroll.viewport  = viewportGo.GetComponent<RectTransform>();
        scroll.horizontal = false;
        scroll.vertical   = true;
        scroll.scrollSensitivity = 30f;

        return contentGo.transform;
    }

    private static TextMeshProUGUI CriarLabel(Transform parent, string nome, string textoInicial)
    {
        GameObject go = CriarGOUI(nome, parent);
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 24f;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = textoInicial;
        tmp.fontSize  = 16f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        return tmp;
    }

    private static void CriarTitulo(Transform parent, string nome, string texto)
    {
        GameObject go = CriarGOUI(nome, parent);
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 28f;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = texto;
        tmp.fontSize  = 17f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = new Color(0.9f, 0.75f, 0.2f, 1f);
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private static void CriarSeparador(Transform parent)
    {
        GameObject go = CriarGOUI("Separador", parent);
        var le = go.AddComponent<LayoutElement>();
        le.minHeight     = 2f;
        le.preferredHeight = 2f;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.35f, 1f);
    }

    // -----------------------------------------------------------------------
    // Helpers de pesquisa / serializacao

    private static GameObject EncontrarPorNome(string nome)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.name == nome && go.scene.isLoaded) return go;
        return null;
    }

    private static void RemoverSeExistir(Transform parent, string nome)
    {
        var t = parent.Find(nome);
        if (t != null) Undo.DestroyObjectImmediate(t.gameObject);
    }

    private static void SetObjProp(SerializedObject so, string prop, UnityEngine.Object val)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = val;
        else Debug.LogWarning("[Estatisticas] Propriedade nao encontrada: " + prop);
    }
}
#endif
