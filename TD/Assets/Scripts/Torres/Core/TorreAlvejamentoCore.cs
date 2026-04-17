using UnityEngine;

/// <summary>
/// Detecta e escolhe o alvo no alcance da torre usando Strategy Pattern.
/// A prioridade é configurável por torre (Inspector) e alterável em runtime.
/// </summary>
public class TorreAlvejamentoCore
{
    private Transform _origemTorre;
    private TorreAtributosCore _atributos;
    private Transform _fallbackObjetivo;
    private readonly Collider2D[] _bufferOverlap = new Collider2D[64];
    private readonly CandidatoAlvo[] _bufferCandidatos = new CandidatoAlvo[64];

    private IEstrategiaSelecionarAlvo _estrategia = new EstrategiaAlvoPrimeiro();

    public Transform AlvoAtual { get; private set; }

    public void Configurar(
        Transform origemTorre,
        TorreAtributosCore atributos,
        Transform fallbackObjetivoFimTrajeto,
        PrioridadeAlvoTorre prioridade = PrioridadeAlvoTorre.Primeiro)
    {
        _origemTorre = origemTorre;
        _atributos = atributos;
        _fallbackObjetivo = fallbackObjetivoFimTrajeto;
        _estrategia = EstrategiaAlvoFactory.Criar(prioridade);
    }

    /// <summary>Altera a prioridade de alvo em runtime (ex.: via HUD futuro).</summary>
    public void DefinirPrioridade(PrioridadeAlvoTorre prioridade)
    {
        _estrategia = EstrategiaAlvoFactory.Criar(prioridade);
    }

    public void AtualizarAlvo()
    {
        AlvoAtual = null;
        if (_origemTorre == null || _atributos?.Dados == null)
            return;

        float alcance = _atributos.ObterAlcanceEfetivo();
        if (alcance <= 0f)
            return;

        LayerMask mask = _atributos.Dados.layerMaskAlvos;
        if (mask.value == 0)
            mask = LayerMask.GetMask("Inimigo");

        var filtroTorre = new ContactFilter2D();
        filtroTorre.SetLayerMask(mask);
        int count = Physics2D.OverlapCircle(_origemTorre.position, alcance, filtroTorre, _bufferOverlap);

        int totalCandidatos = 0;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = _bufferOverlap[i];
            if (col == null) continue;

            UnidadeController unidade = col.GetComponentInParent<UnidadeController>();
            if (unidade == null || unidade.faccao != Faccao.Inimigo || !unidade.EstaVivo)
                continue;

            IAlvoDeTorre especial = unidade.GetComponentInParent<IAlvoDeTorre>();
            float distRestante;
            Transform pontoAcerto;

            if (especial != null)
            {
                if (!especial.PodeSerAlvejado())
                    continue;
                distRestante = especial.ObterDistanciaRestanteTrajeto();
                pontoAcerto = especial.ObterTransformAcerto();
            }
            else
            {
                MovimentoUnidade mov = unidade.GetComponentInParent<MovimentoUnidade>();
                distRestante = mov != null
                    ? mov.CalcularDistanciaRestanteNaRota()
                    : ObterDistanciaFallback(unidade.transform.position);
                pontoAcerto = unidade.transform;
            }

            float distTorreSqr = Vector2.SqrMagnitude((Vector2)(pontoAcerto.position - _origemTorre.position));

            _bufferCandidatos[totalCandidatos++] = new CandidatoAlvo
            {
                pontoAcerto = pontoAcerto,
                distanciaRestanteTrajeto = distRestante,
                distanciaTorreSqr = distTorreSqr,
                vidaAtual = unidade.GetVidaAtual(),
            };

            if (totalCandidatos >= _bufferCandidatos.Length)
                break;
        }

        if (totalCandidatos == 0)
            return;

        AlvoAtual = _estrategia.SelecionarAlvo(_origemTorre, _bufferCandidatos, totalCandidatos);
    }

    private float ObterDistanciaFallback(Vector2 posicaoUnidade)
    {
        if (_fallbackObjetivo == null)
            return float.MaxValue;
        return Vector2.Distance(posicaoUnidade, _fallbackObjetivo.position);
    }
}
