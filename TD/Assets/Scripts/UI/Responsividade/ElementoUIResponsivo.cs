using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ajusta dinamicamente o tamanho e a posicao de um RectTransform relativo a resolucao atual.
///
/// Funciona em dois modos:
///   - PercentualDaTela: o elemento ocupa X% da largura e Y% da altura da tela.
///   - EscalaRelativa:   o elemento mantem o tamanho configurado na resolucao de referencia
///                       e escala proporcionalmente com a resolucao atual.
///
/// Requer que o Canvas pai use "Scale With Screen Size" com resolucao 1920x1080.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ElementoUIResponsivo : MonoBehaviour
{
    public enum ModoEscala
    {
        /// <summary>Tamanho relativo (%) da largura e altura da tela.</summary>
        PercentualDaTela,
        /// <summary>Escala o tamanho a partir dos valores da resolucao de referencia.</summary>
        EscalaRelativa
    }

    [Header("Modo de escala")]
    [SerializeField] private ModoEscala modo = ModoEscala.EscalaRelativa;

    [Header("Modo: Percentual da Tela")]
    [Tooltip("Percentual da largura da tela (0 a 1). Use 0 para ignorar a largura.")]
    [Range(0f, 1f)]
    [SerializeField] private float percentualLargura = 0f;

    [Tooltip("Percentual da altura da tela (0 a 1). Use 0 para ignorar a altura.")]
    [Range(0f, 1f)]
    [SerializeField] private float percentualAltura = 0f;

    [Header("Modo: Escala Relativa")]
    [Tooltip("Tamanho do elemento na resolucao de referencia (1920x1080).")]
    [SerializeField] private Vector2 tamanhoNaReferencia = new Vector2(440f, 96f);

    [Tooltip("Resolucao de referencia usada para calcular o fator de escala.")]
    [SerializeField] private Vector2 resolucaoReferencia = new Vector2(1920f, 1080f);

    [Tooltip("Balanco entre escala por largura (0) e altura (1). Deve coincidir com o CanvasScaler.")]
    [Range(0f, 1f)]
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    [Header("O que atualizar")]
    [SerializeField] private bool atualizarLargura  = true;
    [SerializeField] private bool atualizarAltura   = true;

    [Header("Tamanho minimo e maximo (pixels na resolucao atual)")]
    [SerializeField] private Vector2 tamanhoMinimo = new Vector2(60f,  40f);
    [SerializeField] private Vector2 tamanhoMaximo = new Vector2(2000f, 800f);

    private RectTransform _rt;
    private LayoutElement _layoutElement;

    // ─── Ciclo de vida ───────────────────────────────────────────────────────
    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _layoutElement = GetComponent<LayoutElement>();
    }

    private void Start()
    {
        Atualizar();
        UIResponsividadeManager.OnLayoutAtualizado += Atualizar;
    }

    private void OnDestroy()
    {
        UIResponsividadeManager.OnLayoutAtualizado -= Atualizar;
    }

    // ─── Logica principal ────────────────────────────────────────────────────

    /// <summary>Recalcula e aplica o tamanho do elemento para a resolucao atual.</summary>
    public void Atualizar()
    {
        if (_rt == null) return;

        Vector2 novoTamanho = CalcularTamanho();
        AplicarTamanho(novoTamanho);
    }

    private Vector2 CalcularTamanho()
    {
        switch (modo)
        {
            case ModoEscala.PercentualDaTela:
                return new Vector2(
                    percentualLargura > 0f ? Screen.width  * percentualLargura : _rt.sizeDelta.x,
                    percentualAltura  > 0f ? Screen.height * percentualAltura  : _rt.sizeDelta.y
                );

            case ModoEscala.EscalaRelativa:
            default:
                float fatorW = Screen.width  / resolucaoReferencia.x;
                float fatorH = Screen.height / resolucaoReferencia.y;
                float fator  = Mathf.Lerp(fatorW, fatorH, matchWidthOrHeight);
                return tamanhoNaReferencia * fator;
        }
    }

    private void AplicarTamanho(Vector2 tamanho)
    {
        float novaLargura = Mathf.Clamp(tamanho.x, tamanhoMinimo.x, tamanhoMaximo.x);
        float novaAltura  = Mathf.Clamp(tamanho.y, tamanhoMinimo.y, tamanhoMaximo.y);

        Vector2 delta = _rt.sizeDelta;
        if (atualizarLargura) delta.x = novaLargura;
        if (atualizarAltura)  delta.y = novaAltura;
        _rt.sizeDelta = delta;

        // Sincroniza com LayoutElement se presente (para layouts automaticos)
        if (_layoutElement != null)
        {
            if (atualizarLargura && _layoutElement.preferredWidth  >= 0f) _layoutElement.preferredWidth  = novaLargura;
            if (atualizarAltura  && _layoutElement.preferredHeight >= 0f) _layoutElement.preferredHeight = novaAltura;
        }
    }
}
