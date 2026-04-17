using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente de gatilho do tooltip de gameplay.
/// Anexe a qualquer objeto de UI (item, relíquia, slot de torre, etc.)
/// e preencha os dados no Inspector.
///
/// O <see cref="TooltipGameplayUI"/> é buscado no Canvas raiz automaticamente.
/// </summary>
public class TooltipGatilho : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerMoveHandler
{
    [Header("Dados do Tooltip")]
    [SerializeField] private string nomeExibicao = "";
    [SerializeField] [TextArea(2, 5)] private string descricao = "";
    [SerializeField] private string tipo = "";
    [SerializeField] private int valor = 0;
    [SerializeField] private int custo = 0;

    private Canvas _canvas;

    private void Start()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null)
            TooltipGameplayUI.GarantirNoCanvas(_canvas.rootCanvas ?? _canvas);
    }

    // ─── Configuração em runtime (para itens carregados dinamicamente) ──────

    /// <summary>Redefine os dados do tooltip em runtime (para slots populados por código).</summary>
    public void DefinirDados(DadosTooltip dados)
    {
        nomeExibicao = dados.nome;
        descricao = dados.descricao;
        tipo = dados.tipo;
        valor = dados.valor;
        custo = dados.custo;
    }

    /// <summary>Atalho para definir apenas nome e descrição.</summary>
    public void DefinirDados(string nome, string desc, string tipoStr = "", int val = 0, int cus = 0)
    {
        nomeExibicao = nome;
        descricao = desc;
        tipo = tipoStr;
        valor = val;
        custo = cus;
    }

    // ─── Eventos de pointer ──────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        Exibir(eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        Exibir(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipGameplayUI.Instance?.Ocultar();
    }

    private void OnDisable()
    {
        TooltipGameplayUI.Instance?.Ocultar();
    }

    private void Exibir(Vector2 posicaoTela)
    {
        if (TooltipGameplayUI.Instance == null) return;

        var dados = DadosTooltip.Criar(nomeExibicao, descricao, tipo, valor, custo);
        TooltipGameplayUI.Instance.Mostrar(dados, posicaoTela);
    }
}
