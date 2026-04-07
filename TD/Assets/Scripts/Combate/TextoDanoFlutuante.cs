using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Número de dano que sobe e desaparece; cria Canvas+TMP se o prefab não tiver.</summary>
public class TextoDanoFlutuante : MonoBehaviour
{
    private const string CaminhoFontePadraoTmp = "Fonts & Materials/LiberationSans SDF";

    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private float tamanhoFonte = 56f;
    [SerializeField] private float duracaoSegundos = 0.85f;
    [SerializeField] private float deslocamentoMundo = 0.45f;

    private float _duracaoAnim;
    private float _deslocamentoAnim;

    /// <summary>Instância sem prefab (posição em espaço mundo).</summary>
    public static TextoDanoFlutuante Criar(Vector3 posicaoMundo)
    {
        var go = new GameObject("TextoDanoFlutuante");
        go.transform.position = posicaoMundo;
        return go.AddComponent<TextoDanoFlutuante>();
    }

    private void Awake()
    {
        if (texto == null)
            texto = GetComponentInChildren<TextMeshProUGUI>(true);
        if (texto == null)
            ConstruirUiBasica();
        else
            SincronizarVisualDoTexto();
    }

    private void ConstruirUiBasica()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 2000;
        }

        var rtRaiz = GetComponent<RectTransform>();
        if (rtRaiz != null)
            rtRaiz.sizeDelta = new Vector2(220f, 80f);

        if (GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        }

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        var goTexto = new GameObject("Texto");
        goTexto.transform.SetParent(transform, false);
        texto = goTexto.AddComponent<TextMeshProUGUI>();

        texto.alignment = TextAlignmentOptions.Center;
        texto.color = Color.white;
        texto.raycastTarget = false;
        texto.textWrappingMode = TextWrappingModes.NoWrap;
        texto.overflowMode = TextOverflowModes.Overflow;

        var rtTxt = texto.rectTransform;
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = Vector2.zero;
        rtTxt.offsetMax = Vector2.zero;

        SincronizarVisualDoTexto();
    }

    /// <summary>Aplica fonte, tamanho, escala do canvas e sorting conforme <see cref="ConfiguracaoTextoDanoFlutuanteMapa"/> ou valores locais.</summary>
    private void SincronizarVisualDoTexto()
    {
        if (texto == null)
            return;

        var cfg = ConfiguracaoTextoDanoFlutuanteMapa.Instancia;

        TMP_FontAsset fonteAsset;
        if (cfg != null && cfg.Fonte != null)
            fonteAsset = cfg.Fonte;
        else if (texto.font != null)
            fonteAsset = texto.font;
        else
            fonteAsset = Resources.Load<TMP_FontAsset>(CaminhoFontePadraoTmp);
        if (fonteAsset != null)
            texto.font = fonteAsset;

        texto.fontSize = ObterTamanhoFonteEfetivo();

        var canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.sortingOrder = cfg != null ? cfg.SortingOrderCanvas : 2000;

        var rtRaiz = GetComponent<RectTransform>();
        if (rtRaiz != null)
        {
            float s = cfg != null
                ? Mathf.Max(0.0001f, cfg.EscalaCanvasUniforme)
                : 0.012f;
            rtRaiz.localScale = new Vector3(s, s, s);
        }
    }

    private float ObterTamanhoFonteEfetivo()
    {
        var cfg = ConfiguracaoTextoDanoFlutuanteMapa.Instancia;
        return cfg != null ? cfg.TamanhoFonte : tamanhoFonte;
    }

    private float ObterDuracaoEfetiva()
    {
        var cfg = ConfiguracaoTextoDanoFlutuanteMapa.Instancia;
        return cfg != null ? cfg.DuracaoSegundos : duracaoSegundos;
    }

    private float ObterDeslocamentoEfetivo()
    {
        var cfg = ConfiguracaoTextoDanoFlutuanteMapa.Instancia;
        return cfg != null ? cfg.DeslocamentoMundo : deslocamentoMundo;
    }

    private static Color ResolverCorLegado(Color corDefinicao)
    {
        var cfg = ConfiguracaoTextoDanoFlutuanteMapa.Instancia;
        if (cfg != null && cfg.ModoCorTexto == ConfiguracaoTextoDanoFlutuanteMapa.ModoCor.ForcarCorDoMapa)
            return cfg.CorMapa;
        return corDefinicao;
    }

    /// <summary>Exibe dano com estilo por tipo (biblioteca), suporte a crítico e overrides.</summary>
    public void Inicializar(PedidoDanoFlutuante pedido, BibliotecaEstilosTipoDanoSO biblioteca)
    {
        if (texto == null)
        {
            Destroy(gameObject);
            return;
        }

        var cfg = ConfiguracaoTextoDanoFlutuanteMapa.Instancia;
        BibliotecaEstilosTipoDanoSO bib = biblioteca != null
            ? biblioteca
            : (cfg != null ? cfg.BibliotecaEstilosTipoDano : null);

        EntradaEstiloTipoDano est = bib != null ? bib.Resolver(pedido.Tipo) : null;

        float tamBase = est != null ? est.tamanhoFonteBase : ObterTamanhoFonteEfetivo();
        float tamCrit = est != null ? est.tamanhoFonteCritico : tamBase * 1.28f;
        float tamFonte = pedido.Critico ? tamCrit : tamBase;

        string prefixo = est != null ? est.prefixoOpcional : string.Empty;
        string sufixoCrit = string.Empty;
        if (pedido.Critico)
            sufixoCrit = est != null ? est.sufixoSeCritico : "!";

        string conteudo = $"{prefixo}{pedido.ValorExibicao}{sufixoCrit}";

        Color cor;
        if (cfg != null && cfg.ModoCorTexto == ConfiguracaoTextoDanoFlutuanteMapa.ModoCor.ForcarCorDoMapa)
            cor = cfg.CorMapa;
        else if (pedido.UsarCorOverride)
            cor = pedido.CorOverride;
        else if (est != null)
            cor = est.corTexto;
        else
            cor = Color.white;

        SincronizarVisualDoTexto();
        texto.fontSize = tamFonte;
        texto.text = conteudo;
        texto.color = cor;
        _duracaoAnim = ObterDuracaoEfetiva();
        _deslocamentoAnim = ObterDeslocamentoEfetivo();
        StopAllCoroutines();
        StartCoroutine(AnimarCoroutine());
    }

    public void Inicializar(int valorDano, Color cor)
    {
        if (texto == null)
        {
            Destroy(gameObject);
            return;
        }

        SincronizarVisualDoTexto();
        texto.text = valorDano.ToString();
        texto.color = ResolverCorLegado(cor);
        _duracaoAnim = ObterDuracaoEfetiva();
        _deslocamentoAnim = ObterDeslocamentoEfetivo();
        StopAllCoroutines();
        StartCoroutine(AnimarCoroutine());
    }

    private IEnumerator AnimarCoroutine()
    {
        Transform alvo = transform;
        Vector3 posInicial = alvo.position;
        Color cor = texto.color;
        float t = 0f;
        float dur = Mathf.Max(0.01f, _duracaoAnim);

        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);
            alvo.position = posInicial + Vector3.up * (_deslocamentoAnim * u);
            cor.a = 1f - u;
            texto.color = cor;
            yield return null;
        }

        Destroy(gameObject);
    }
}
