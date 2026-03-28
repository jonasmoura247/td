using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentoTooltipUI : MonoBehaviour
{
    [SerializeField] private RectTransform painelRaiz;
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private Vector2 deslocamentoMouse = new Vector2(14f, -14f);
    [SerializeField] private float margemBordaCanvas = 12f;
    [SerializeField] private Canvas canvasRaiz;

    [SerializeField] private TMP_FontAsset fonteTexto;
    [SerializeField] private float tamanhoFonte = 15f;
    [SerializeField] private Color corTexto = Color.white;
    [SerializeField] private Sprite spritePainelFundo;
    [SerializeField] private Color corPainelFundo = new Color(0.06f, 0.07f, 0.1f, 0.96f);
    [SerializeField] private Image.Type tipoImagemFundo = Image.Type.Sliced;

    private Image imagemFundoCache;

    public static TalentoTooltipUI GarantirNaCanvas(Canvas canvas)
    {
        if (canvas == null)
            return null;
        var existente = canvas.GetComponentInChildren<TalentoTooltipUI>(true);
        if (existente != null)
            return existente;
        return CriarTooltipFilhoDoCanvas(canvas);
    }

    public static TalentoTooltipUI CriarTooltipFilhoDoCanvas(Canvas canvas)
    {
        if (canvas == null)
            return null;

        GameObject root = new GameObject("TooltipTalentos", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        root.transform.SetAsLastSibling();
        RectTransform rtRoot = root.GetComponent<RectTransform>();
        rtRoot.sizeDelta = new Vector2(360f, 240f);

        Image fundo = root.AddComponent<Image>();
        fundo.color = new Color(0.06f, 0.07f, 0.1f, 0.96f);
        fundo.raycastTarget = false;

        Sprite spr = Resources.Load<Sprite>("Talentos/TooltipFundo");
        if (spr != null)
        {
            fundo.sprite = spr;
            fundo.type = Image.Type.Sliced;
        }

        GameObject textoGo = new GameObject("Texto");
        textoGo.transform.SetParent(root.transform, false);
        RectTransform rtTxt = textoGo.AddComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = new Vector2(12f, 10f);
        rtTxt.offsetMax = new Vector2(-12f, -10f);

        TextMeshProUGUI tmp = textoGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = 15f;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.richText = true;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;

        TalentoTooltipUI tt = root.AddComponent<TalentoTooltipUI>();
        tt.Configurar(rtRoot, tmp, canvas);
        root.SetActive(false);
        return tt;
    }

    public void Configurar(RectTransform painel, TextMeshProUGUI txt, Canvas cv)
    {
        painelRaiz = painel;
        texto = txt;
        canvasRaiz = cv;
        imagemFundoCache = painelRaiz != null ? painelRaiz.GetComponent<Image>() : null;
        AplicarEstiloVisual();
        if (painelRaiz != null)
            painelRaiz.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (painelRaiz != null && texto != null)
            AplicarEstiloVisual();
    }

    public void AplicarEstiloVisual()
    {
        if (texto != null)
        {
            if (fonteTexto != null)
                texto.font = fonteTexto;
            texto.fontSize = tamanhoFonte;
            texto.color = corTexto;
        }
        if (imagemFundoCache == null && painelRaiz != null)
            imagemFundoCache = painelRaiz.GetComponent<Image>();
        if (imagemFundoCache != null)
        {
            if (spritePainelFundo != null)
            {
                imagemFundoCache.sprite = spritePainelFundo;
                imagemFundoCache.type = tipoImagemFundo;
            }
            imagemFundoCache.color = corPainelFundo;
        }
    }

    public void Mostrar(string conteudo, Vector2 posicaoTela)
    {
        if (texto != null)
            texto.text = conteudo;
        if (painelRaiz == null)
            return;
        painelRaiz.gameObject.SetActive(true);
        painelRaiz.SetAsLastSibling();
        Posicionar(posicaoTela);
    }

    public void Ocultar()
    {
        if (painelRaiz != null)
            painelRaiz.gameObject.SetActive(false);
    }

    private void Posicionar(Vector2 posicaoTela)
    {
        if (canvasRaiz == null || painelRaiz == null)
            return;

        Canvas canvasRef = canvasRaiz.rootCanvas != null ? canvasRaiz.rootCanvas : canvasRaiz;
        RectTransform canvasRect = canvasRef.transform as RectTransform;
        if (canvasRect == null)
            return;

        Camera cam = null;
        if (canvasRef.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvasRef.worldCamera != null ? canvasRef.worldCamera : Camera.main;

        RectTransform parentRt = painelRaiz.parent as RectTransform;
        if (parentRt == null)
            parentRt = canvasRect;

        painelRaiz.anchorMin = new Vector2(0.5f, 0.5f);
        painelRaiz.anchorMax = new Vector2(0.5f, 0.5f);
        painelRaiz.pivot = new Vector2(0f, 1f);

        Vector2 screenComAjuste = posicaoTela + deslocamentoMouse;
        Vector2 localNoPai;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt,
                screenComAjuste,
                cam,
                out localNoPai))
        {
            localNoPai = Vector2.zero;
        }

        painelRaiz.anchoredPosition = localNoPai;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(painelRaiz);

        AjustarDentroDoCanvas(parentRt, cam);
    }

    private void AjustarDentroDoCanvas(RectTransform parentRt, Camera cam)
    {
        Rect area = parentRt.rect;
        float pad = margemBordaCanvas;
        float limMinX = area.xMin + pad;
        float limMaxX = area.xMax - pad;
        float limMinY = area.yMin + pad;
        float limMaxY = area.yMax - pad;

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
        if (min.x < limMinX) delta.x += limMinX - min.x;
        if (max.x > limMaxX) delta.x += limMaxX - max.x;
        if (min.y < limMinY) delta.y += limMinY - min.y;
        if (max.y > limMaxY) delta.y += limMaxY - max.y;

        painelRaiz.anchoredPosition += delta;
    }
}
