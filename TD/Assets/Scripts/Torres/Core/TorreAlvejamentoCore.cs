using UnityEngine;

/// <summary>
/// Escolhe inimigo no alcance: menor distância restante na rota; empate = mais próximo da torre.
/// </summary>
public class TorreAlvejamentoCore
{
    private Transform _origemTorre;
    private TorreAtributosCore _atributos;
    private Transform _fallbackObjetivo;
    private readonly Collider2D[] _bufferOverlap = new Collider2D[64];

    public Transform AlvoAtual { get; private set; }

    public void Configurar(Transform origemTorre, TorreAtributosCore atributos, Transform fallbackObjetivoFimTrajeto)
    {
        _origemTorre = origemTorre;
        _atributos = atributos;
        _fallbackObjetivo = fallbackObjetivoFimTrajeto;
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

        int count = Physics2D.OverlapCircleNonAlloc(_origemTorre.position, alcance, _bufferOverlap, mask);

        float melhorDistRestante = float.MaxValue;
        float melhorDistTorre = float.MaxValue;
        Transform escolhido = null;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = _bufferOverlap[i];
            if (col == null) continue;

            UnidadeController unidade = col.GetComponentInParent<UnidadeController>();
            if (unidade == null || unidade.faccao != Faccao.Inimigo || !unidade.EstaVivo)
                continue;

            IAlvoDeTorre especial = unidade.GetComponentInParent<IAlvoDeTorre>();
            float distRestante;
            Transform pontoAcerto = unidade.transform;

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
                if (mov != null)
                    distRestante = mov.CalcularDistanciaRestanteNaRota();
                else
                    distRestante = ObterDistanciaFallback(unidade.transform.position);

                pontoAcerto = unidade.transform;
            }

            float distTorre = Vector2.SqrMagnitude((Vector2)(pontoAcerto.position - _origemTorre.position));

            if (distRestante < melhorDistRestante - 0.001f ||
                (Mathf.Approximately(distRestante, melhorDistRestante) && distTorre < melhorDistTorre))
            {
                melhorDistRestante = distRestante;
                melhorDistTorre = distTorre;
                escolhido = pontoAcerto;
            }
        }

        AlvoAtual = escolhido;
    }

    private float ObterDistanciaFallback(Vector2 posicaoUnidade)
    {
        if (_fallbackObjetivo == null)
            return float.MaxValue;
        return Vector2.Distance(posicaoUnidade, _fallbackObjetivo.position);
    }
}
