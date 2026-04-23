#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menu TD > Patrulha > Configurar UI de Patrulha na Cena
///
/// Cria automaticamente na cena aberta:
///   • GameObject "ServicoPatrulhaUI" com o componente <see cref="ServicoPatrulhaUI"/>
///   • Painel flutuante "PainelPatrulha" no Canvas principal com botão "Patrulha"
///   • Liga as referências via SerializedObject (sem arrastar nada no Inspector)
///   • Auto-detecta a layer "Jogador" para layerUnidadesJogador
///
/// Idempotente: re-executar substitui sem duplicar.
/// </summary>
public static class PatrulhaSetup
{
    private const string NomeServico      = "ServicoPatrulhaUI";
    private const string NomePainel       = "PainelPatrulha";
    private const string NomeBotao        = "BotaoPatrulha";

    [MenuItem("TD/Patrulha/Configurar UI de Patrulha na Cena")]
    public static void ConfigurarNaCena()
    {
        Canvas canvas = EncontrarCanvasPrincipal();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog(
                "Patrulha Setup",
                "Nenhum Canvas encontrado na cena.\n\nAbra a cena de gameplay antes de executar este setup.",
                "OK");
            return;
        }

        // Remove instâncias antigas para garantir idempotência
        RemoverSeExistir(NomeServico);
        RemoverFilhoSeExistir(canvas.transform, NomePainel);

        // 1. Cria o painel flutuante com o botão
        CriarPainelComBotao(canvas, out GameObject painel, out Button botao);

        // 2. Cria o GameObject do serviço e liga as referências
        ServicoPatrulhaUI servico = CriarServicoPatrulhaUI(painel, botao);

        // Marca a cena como modificada
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorGUIUtility.PingObject(servico);
        Selection.activeGameObject = servico.gameObject;

        Debug.Log("[PatrulhaSetup] UI de patrulha configurada com sucesso! " +
                  "Salve a cena (Ctrl+S) para persistir as mudanças.");
    }

    // ── CANVAS ────────────────────────────────────────────────────────────────

    private static Canvas EncontrarCanvasPrincipal()
    {
        // Prefere Canvas com "HUD" ou "Overlay" no nome
        Canvas[] todos = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Canvas melhor = null;
        int melhorPrioridade = -1;

        foreach (Canvas c in todos)
        {
            int prioridade = 0;
            string nome = c.gameObject.name.ToLower();

            if (nome.Contains("hud"))     prioridade = 3;
            else if (nome.Contains("overlay")) prioridade = 2;
            else if (nome.Contains("ui"))      prioridade = 1;

            if (c.renderMode == RenderMode.ScreenSpaceOverlay) prioridade++;

            if (prioridade > melhorPrioridade)
            {
                melhorPrioridade = prioridade;
                melhor = c;
            }
        }

        return melhor ?? (todos.Length > 0 ? todos[0] : null);
    }

    // ── PAINEL E BOTÃO ────────────────────────────────────────────────────────

    private static void CriarPainelComBotao(Canvas canvas, out GameObject painelGo, out Button btn)
    {
        // ── Painel raiz ──────────────────────────────────────────────────
        painelGo = new GameObject(NomePainel, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(painelGo, "Criar PainelPatrulha");
        painelGo.transform.SetParent(canvas.transform, false);

        var painelRect = painelGo.GetComponent<RectTransform>();
        // Ancora no centro-inferior da tela, acima do HUD de unidades
        painelRect.anchorMin = new Vector2(0.5f, 0f);
        painelRect.anchorMax = new Vector2(0.5f, 0f);
        painelRect.pivot     = new Vector2(0.5f, 0f);
        painelRect.sizeDelta = new Vector2(160f, 60f);
        painelRect.anchoredPosition = new Vector2(0f, 180f); // 180px acima da base

        // Fundo semitransparente
        var painelImg = painelGo.AddComponent<Image>();
        painelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);
        painelImg.raycastTarget = false;

        // Começa escondido (o ServicoPatrulhaUI ativa quando necessário)
        painelGo.SetActive(false);

        // ── Botão ────────────────────────────────────────────────────────
        var botaoGo = new GameObject(NomeBotao, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(botaoGo, "Criar BotaoPatrulha");
        botaoGo.transform.SetParent(painelGo.transform, false);

        var botaoRect = botaoGo.GetComponent<RectTransform>();
        botaoRect.anchorMin = Vector2.zero;
        botaoRect.anchorMax = Vector2.one;
        botaoRect.offsetMin = new Vector2(8f, 8f);
        botaoRect.offsetMax = new Vector2(-8f, -8f);

        // Fundo do botão
        var botaoImg = botaoGo.AddComponent<Image>();
        botaoImg.color = new Color(0.22f, 0.48f, 0.78f, 1f); // azul

        // ColorBlock com estados hover/pressed
        btn = botaoGo.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor      = new Color(0.22f, 0.48f, 0.78f, 1f);
        cb.highlightedColor = new Color(0.30f, 0.58f, 0.90f, 1f);
        cb.pressedColor     = new Color(0.15f, 0.35f, 0.60f, 1f);
        cb.selectedColor    = cb.normalColor;
        cb.fadeDuration     = 0.1f;
        btn.colors = cb;

        // Texto TMP
        var textoGo = new GameObject("Texto", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(textoGo, "Criar TextoBotaoPatrulha");
        textoGo.transform.SetParent(botaoGo.transform, false);

        var textoRect = textoGo.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = textoRect.offsetMax = Vector2.zero;

        var tmp = textoGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = "Patrulha";
        tmp.fontSize  = 18f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.raycastTarget = false;

        Debug.Log($"[PatrulhaSetup] Painel '{NomePainel}' e botão '{NomeBotao}' criados no Canvas '{canvas.name}'.");
    }

    // ── SERVIÇO ───────────────────────────────────────────────────────────────

    private static ServicoPatrulhaUI CriarServicoPatrulhaUI(GameObject painel, Button botao)
    {
        var go = new GameObject(NomeServico);
        Undo.RegisterCreatedObjectUndo(go, "Criar ServicoPatrulhaUI");

        var servico = go.AddComponent<ServicoPatrulhaUI>();

        // Liga referências via SerializedObject
        var so = new SerializedObject(servico);

        so.FindProperty("painelPatrulha").objectReferenceValue = painel;
        so.FindProperty("botaoPatrulha").objectReferenceValue  = botao;

        // Auto-detecta layer "Jogador" (LayerMask serializa o valor via m_Bits)
        int layerIdx = LayerMask.NameToLayer("Jogador");
        if (layerIdx >= 0)
        {
            var layerProp = so.FindProperty("layerUnidadesJogador");
            // SerializedProperty de LayerMask tem um filho "m_Bits" com o valor inteiro
            var bitsProp = layerProp?.FindPropertyRelative("m_Bits");
            if (bitsProp != null)
                bitsProp.intValue = 1 << layerIdx;
            else if (layerProp != null)
                layerProp.intValue = 1 << layerIdx; // fallback direto
        }
        else
            Debug.LogWarning("[PatrulhaSetup] Layer 'Jogador' não encontrada. " +
                             "Configure 'layerUnidadesJogador' no Inspector do ServicoPatrulhaUI manualmente.");

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(servico);

        Debug.Log($"[PatrulhaSetup] ServicoPatrulhaUI criado e configurado." +
                  (layerIdx >= 0 ? " Layer 'Jogador' detectada automaticamente." : ""));

        return servico;
    }

    // ── UTILITÁRIOS ───────────────────────────────────────────────────────────

    private static void RemoverSeExistir(string nomeGO)
    {
        var existente = GameObject.Find(nomeGO);
        if (existente != null)
        {
            Undo.DestroyObjectImmediate(existente);
            Debug.Log($"[PatrulhaSetup] Removido '{nomeGO}' existente para recriar limpo.");
        }
    }

    private static void RemoverFilhoSeExistir(Transform pai, string nomeFilho)
    {
        Transform filho = pai.Find(nomeFilho);
        if (filho != null)
        {
            Undo.DestroyObjectImmediate(filho.gameObject);
            Debug.Log($"[PatrulhaSetup] Removido filho '{nomeFilho}' existente para recriar limpo.");
        }
    }
}
#endif
