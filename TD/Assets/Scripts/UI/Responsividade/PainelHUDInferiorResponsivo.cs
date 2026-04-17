using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ajusta dinamicamente o tamanho do painel HUD inferior (torres e unidades)
/// e o tamanho dos slots filhos para diferentes resolucoes.
///
/// Como usar:
///   Adicione este script ao GameObject raiz do HUDConstrucaoTorres ou HUDColocacaoUnidades.
///   Configure os valores de referencia conforme o design na resolucao 1920x1080.
///
/// O script:
///   - Redimensiona o painel para ser uma porcentagem da largura da tela.
///   - Ajusta a altura do painel proporcional a altura da tela.
///   - Recalcula o tamanho de cada slot filho com LayoutElement.
///   - Atualiza o spacing e padding do HorizontalLayoutGroup.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PainelHUDInferiorResponsivo : MonoBehaviour
{
    [Header("Resolucao de referencia")]
    [SerializeField] private Vector2 resolucaoReferencia = new Vector2(1920f, 1080f);

    [Header("Painel — tamanho na resolucao de referencia (px)")]
    [SerializeField] private float larguraPainelReferencia = 440f;
    [SerializeField] private float alturaPainelReferencia  = 96f;

    [Header("Slots — tamanho na resolucao de referencia (px)")]
    [SerializeField] private float tamanhoSlotReferencia   = 58f;

    [Header("Layout — valores na resolucao de referencia (px)")]
    [SerializeField] private float spacingReferencia = 8f;
    [SerializeField] private RectOffset paddingReferencia = new RectOffset(6, 6, 4, 4);

    [Header("Limites do painel")]
    [SerializeField] private float larguraMinima   = 200f;
    [SerializeField] private float larguraMaxima   = 900f;
    [SerializeField] private float alturaMinima    = 48f;
    [SerializeField] private float alturaMaxima    = 200f;

    [Header("Limites dos slots")]
    [SerializeField] private float slotMinimo = 36f;
    [SerializeField] private float slotMaximo = 120f;

    [Header("Referencias (auto-detectadas se nao preenchidas)")]
    [SerializeField] private HorizontalLayoutGroup layoutGroup;

    private RectTransform _rt;

    // ─── Ciclo de vida ───────────────────────────────────────────────────────
    private void Awake()
    {
        _rt = GetComponent<RectTransform>();

        if (layoutGroup == null)
            layoutGroup = GetComponentInChildren<HorizontalLayoutGroup>();
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

    // ─── Logica ──────────────────────────────────────────────────────────────

    public void Atualizar()
    {
        if (_rt == null) return;

        float fatorH = Screen.height / resolucaoReferencia.y;
        float fatorW = Screen.width  / resolucaoReferencia.x;
        // Usa o menor fator para evitar que o painel fique maior que a tela
        float fator  = Mathf.Min(fatorW, fatorH);

        AjustarPainel(fatorW, fatorH);
        AjustarSlots(fator);
        AjustarLayout(fator);
    }

    private void AjustarPainel(float fatorW, float fatorH)
    {
        float novaLargura = Mathf.Clamp(larguraPainelReferencia * fatorW, larguraMinima, larguraMaxima);
        float novaAltura  = Mathf.Clamp(alturaPainelReferencia  * fatorH, alturaMinima,  alturaMaxima);

        Vector2 delta = _rt.sizeDelta;
        delta.x = novaLargura;
        delta.y = novaAltura;
        _rt.sizeDelta = delta;
    }

    private void AjustarSlots(float fator)
    {
        float tamanhoSlot = Mathf.Clamp(tamanhoSlotReferencia * fator, slotMinimo, slotMaximo);

        // Percorre filhos diretos que tenham LayoutElement e os redimensiona
        foreach (Transform filho in transform)
        {
            LayoutElement le = filho.GetComponent<LayoutElement>();
            if (le == null)
                le = filho.gameObject.AddComponent<LayoutElement>();

            le.preferredWidth  = tamanhoSlot;
            le.preferredHeight = tamanhoSlot;
            le.minWidth        = slotMinimo;
            le.minHeight       = slotMinimo;

            // Atualiza o RectTransform tambem (para elementos fora de LayoutGroup)
            RectTransform rtFilho = filho as RectTransform;
            if (rtFilho != null)
            {
                Vector2 deltaFilho = rtFilho.sizeDelta;
                deltaFilho.x = tamanhoSlot;
                deltaFilho.y = tamanhoSlot;
                rtFilho.sizeDelta = deltaFilho;
            }
        }
    }

    private void AjustarLayout(float fator)
    {
        if (layoutGroup == null) return;

        layoutGroup.spacing = spacingReferencia * fator;
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(paddingReferencia.left   * fator),
            Mathf.RoundToInt(paddingReferencia.right  * fator),
            Mathf.RoundToInt(paddingReferencia.top    * fator),
            Mathf.RoundToInt(paddingReferencia.bottom * fator)
        );

        LayoutRebuilder.MarkLayoutForRebuild(_rt);
    }
}
