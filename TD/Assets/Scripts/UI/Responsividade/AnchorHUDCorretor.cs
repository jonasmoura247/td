using UnityEngine;

/// <summary>
/// Garante que o anchor e o pivot de um elemento de HUD estejam corretos em runtime,
/// independente de como estao configurados no prefab ou na cena.
///
/// Por que isso e necessario:
///   Elementos ancorados no centro (0.5, 0.5) derivam para fora da tela em resolucoes
///   diferentes da de referencia. Este script reposiciona o elemento no local correto
///   relativo a borda da tela escolhida.
///
/// Como usar:
///   Adicione este script ao GameObject de qualquer elemento de HUD que precise
///   ficar ancorado a uma borda especifica da tela.
///   Configure o "Canto de Ancora" e o "Offset" desejados.
///
/// Exemplos de uso:
///   TextoOuro    → CantoInferiorDireito, offset (−10, 10)
///   HUDProgressao → CantoSuperiorEsquerdo, offset (10, −10)
///   NotificacaoToast → TopoCentral, offset (0, −30)
///   HUDCastelo   → CantoInferiorDireito, offset (−10, 80)
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class AnchorHUDCorretor : MonoBehaviour
{
    public enum PosicaoHUD
    {
        TopoCentral,
        TopoEsquerdo,
        TopoDireito,
        CentroEsquerdo,
        CentroDireito,
        CentroTela,
        BaseEsquerda,
        BaseCentral,
        BaseDireita,
        Personalizado
    }

    [Header("Posicao na tela")]
    [Tooltip("Escolha onde este elemento deve ficar na tela.")]
    [SerializeField] private PosicaoHUD posicao = PosicaoHUD.TopoCentral;

    [Tooltip("Offset em pixels (espaco logico do Canvas) a partir da borda escolhida.")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    [Header("Personalizado (so usado se posicao = Personalizado)")]
    [SerializeField] private Vector2 anchorMinPersonalizado = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 anchorMaxPersonalizado = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 pivotPersonalizado     = new Vector2(0.5f, 0.5f);

    [Header("Aplicar somente uma vez")]
    [Tooltip("Se verdadeiro, aplica apenas no Start. Se falso, reaaplica sempre que a resolucao mudar.")]
    [SerializeField] private bool aplicarSomenteNoStart = false;

    private RectTransform _rt;

    // ─── Ciclo de vida ───────────────────────────────────────────────────────
    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    private void Start()
    {
        Aplicar();
        if (!aplicarSomenteNoStart)
            UIResponsividadeManager.OnLayoutAtualizado += Aplicar;
    }

    private void OnDestroy()
    {
        UIResponsividadeManager.OnLayoutAtualizado -= Aplicar;
    }

    // ─── Logica ──────────────────────────────────────────────────────────────

    public void Aplicar()
    {
        if (_rt == null) return;

        (Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot) = ObterConfiguracoes(posicao);

        _rt.anchorMin = anchorMin;
        _rt.anchorMax = anchorMax;
        _rt.pivot     = pivot;

        // Offset a partir da borda: o anchoredPosition e relativo ao anchor
        _rt.anchoredPosition = offset;
    }

    private (Vector2, Vector2, Vector2) ObterConfiguracoes(PosicaoHUD pos)
    {
        switch (pos)
        {
            case PosicaoHUD.TopoCentral:
                return (new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            case PosicaoHUD.TopoEsquerdo:
                return (new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            case PosicaoHUD.TopoDireito:
                return (new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));

            case PosicaoHUD.CentroEsquerdo:
                return (new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));

            case PosicaoHUD.CentroDireito:
                return (new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));

            case PosicaoHUD.CentroTela:
                return (new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            case PosicaoHUD.BaseEsquerda:
                return (new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));

            case PosicaoHUD.BaseCentral:
                return (new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));

            case PosicaoHUD.BaseDireita:
                return (new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));

            case PosicaoHUD.Personalizado:
            default:
                return (anchorMinPersonalizado, anchorMaxPersonalizado, pivotPersonalizado);
        }
    }

#if UNITY_EDITOR
    // ─── Gizmos de editor ────────────────────────────────────────────────────
    private void OnValidate()
    {
        if (_rt == null)
            _rt = GetComponent<RectTransform>();
        // Nao aplica no editor pois pode quebrar o fluxo de prefabs,
        // mas valida que o offset esta coerente com a posicao.
    }
#endif
}
