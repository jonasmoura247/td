using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tooltip de gameplay reutilizável para itens, relíquias e torres.
/// Segue o mesmo padrão de <see cref="TalentoTooltipUI"/>:
///   - Singleton por Canvas
///   - Posicionamento automático junto ao cursor
///   - Conteúdo formatado com rich text
///
/// Configuração:
///   Adicione ao Canvas (Screen Space Overlay) via prefab ou use
///   <see cref="GarantirNoCanvas"/> para criação automática em runtime.
/// </summary>
public class TooltipGameplayUI : MonoBehaviour
{
    public static TooltipGameplayUI Instance { get; private set; }

    [SerializeField] private RectTransform painelRaiz;
    [SerializeField] private TextMeshProUGUI textoNome;
    [SerializeField] private TextMeshProUGUI textoDescricao;
    [SerializeField] private TextMeshProUGUI textoDetalhes;
    [SerializeField] private Canvas canvasRaiz;

    [Header("Posicionamento")]
    [SerializeField] private Vector2 deslocamentoMouse = new Vector2(14f, -14f);
    [SerializeField] private float margemBorda = 12f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (painelRaiz != null)
            painelRaiz.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ─── API pública ─────────────────────────────────────────────────────────

    /// <summary>
    /// Exibe o tooltip com os dados fornecidos.
    /// Campos vazios ou zero são omitidos automaticamente.
    /// </summary>
    public void Mostrar(DadosTooltip dados, Vector2 posicaoTela)
    {
        if (painelRaiz == null) return;

        if (textoNome != null)
            textoNome.text = dados.nome ?? "";

        if (textoDescricao != null)
            textoDescricao.text = dados.descricao ?? "";

        if (textoDetalhes != null)
            textoDetalhes.text = MontarDetalhes(dados);

        painelRaiz.gameObject.SetActive(true);
        painelRaiz.SetAsLastSibling();
        Posicionar(posicaoTela);
    }

    public void Ocultar()
    {
        if (painelRaiz != null)
            painelRaiz.gameObject.SetActive(false);
    }

    // ─── Singleton por Canvas (criação automática) ────────────────────────

    /// <summary>
    /// Garante que existe exatamente um <see cref="TooltipGameplayUI"/> no canvas.
    /// Cria automaticamente se ainda não existir.
    /// </summary>
    public static TooltipGameplayUI GarantirNoCanvas(Canvas canvas)
    {
        if (canvas == null) return null;
        var existente = canvas.GetComponentInChildren<TooltipGameplayUI>(true);
        if (existente != null) return existente;
        return CriarNoCanvas(canvas);
    }

    private static TooltipGameplayUI CriarNoCanvas(Canvas canvas)
    {
        GameObject root = new GameObject("TooltipGameplay", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        root.transform.SetAsLastSibling();

        RectTransform rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280f, 180f);

        Image fundo = root.AddComponent<Image>();
        fundo.color = new Color(0.05f, 0.06f, 0.09f, 0.95f);
        fundo.raycastTarget = false;

        // Nome
        var goNome = CriarFilhoTexto(root.transform, "Nome",
            new Vector2(8f, -8f), new Vector2(-8f, -36f), 16f, Color.white, true);

        // Descrição
        var goDesc = CriarFilhoTexto(root.transform, "Descricao",
            new Vector2(8f, -42f), new Vector2(-8f, -100f), 13f, new Color(0.8f, 0.8f, 0.8f), false);

        // Detalhes (valor, custo, tipo)
        var goDet = CriarFilhoTexto(root.transform, "Detalhes",
            new Vector2(8f, -104f), new Vector2(-8f, -8f), 12f, new Color(0.7f, 0.9f, 0.7f), false);

        TooltipGameplayUI tt = root.AddComponent<TooltipGameplayUI>();
        tt.painelRaiz = rt;
        tt.canvasRaiz = canvas;
        tt.textoNome = goNome;
        tt.textoDescricao = goDesc;
        tt.textoDetalhes = goDet;
        root.SetActive(false);
        return tt;
    }

    private static TextMeshProUGUI CriarFilhoTexto(
        Transform pai, string nome,
        Vector2 offsetMin, Vector2 offsetMax,
        float tamanho, Color cor, bool negrito)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = tamanho;
        tmp.color = cor;
        tmp.fontStyle = negrito ? FontStyles.Bold : FontStyles.Normal;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.richText = true;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        return tmp;
    }

    // ─── Internos ────────────────────────────────────────────────────────────

    private static string MontarDetalhes(DadosTooltip d)
    {
        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(d.tipo))
            sb.AppendLine($"<color=#AAD4FF>Tipo:</color> {d.tipo}");
        if (d.valor > 0)
            sb.AppendLine($"<color=#FFD700>Valor:</color> {d.valor}");
        if (d.custo > 0)
            sb.AppendLine($"<color=#FF9966>Custo:</color> {d.custo}");
        return sb.ToString().TrimEnd();
    }

    private void Posicionar(Vector2 posicaoTela)
    {
        if (canvasRaiz == null || painelRaiz == null) return;

        Canvas canvasRef = canvasRaiz.rootCanvas ?? canvasRaiz;
        RectTransform canvasRect = canvasRef.transform as RectTransform;
        if (canvasRect == null) return;

        Camera cam = canvasRef.renderMode != RenderMode.ScreenSpaceOverlay
            ? (canvasRef.worldCamera ?? Camera.main)
            : null;

        RectTransform parentRt = painelRaiz.parent as RectTransform ?? canvasRect;

        painelRaiz.anchorMin = new Vector2(0.5f, 0.5f);
        painelRaiz.anchorMax = new Vector2(0.5f, 0.5f);
        painelRaiz.pivot = new Vector2(0f, 1f);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt, posicaoTela + deslocamentoMouse, cam, out Vector2 local))
            painelRaiz.anchoredPosition = local;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(painelRaiz);
        AjustarDentroDoCanvas(parentRt);
    }

    private void AjustarDentroDoCanvas(RectTransform parentRt)
    {
        float pad = margemBorda;
        Rect area = parentRt.rect;

        Vector3[] corners = new Vector3[4];
        painelRaiz.GetWorldCorners(corners);
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        for (int i = 0; i < 4; i++)
        {
            Vector2 lp = parentRt.InverseTransformPoint(corners[i]);
            min = Vector2.Min(min, lp);
            max = Vector2.Max(max, lp);
        }

        Vector2 delta = Vector2.zero;
        if (min.x < area.xMin + pad) delta.x += area.xMin + pad - min.x;
        if (max.x > area.xMax - pad) delta.x += area.xMax - pad - max.x;
        if (min.y < area.yMin + pad) delta.y += area.yMin + pad - min.y;
        if (max.y > area.yMax - pad) delta.y += area.yMax - pad - max.y;

        painelRaiz.anchoredPosition += delta;
    }
}

/// <summary>
/// Dados a exibir no tooltip. Campos zerados/vazios são omitidos.
/// </summary>
[System.Serializable]
public struct DadosTooltip
{
    public string nome;
    public string descricao;
    public string tipo;
    public int valor;
    public int custo;

    public static DadosTooltip Criar(string nome, string descricao = "",
        string tipo = "", int valor = 0, int custo = 0)
        => new DadosTooltip
        {
            nome = nome,
            descricao = descricao,
            tipo = tipo,
            valor = valor,
            custo = custo,
        };
}
