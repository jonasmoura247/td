using UnityEngine;

/// <summary>
/// Lógica pura das tendas: probabilidade de aparecer e tipo sorteado.
/// Separado do MonoBehaviour para ser testável sem Unity runtime.
/// </summary>
public class SistemaTendasLogica
{
    public const float ChanceBase = 0.30f;
    public const int PityFasesSemTenda = 3;

    // Limiares acumulados para tipo de tenda
    private const float LimiarMercador = 0.40f;
    private const float LimiarHerois = 0.70f;
    private const float LimiarMista = 0.90f;

    /// <summary>
    /// Decide se haverá tenda nesta fase.
    /// Após <see cref="PityFasesSemTenda"/> fases seguidas sem tenda → garantida.
    /// </summary>
    public bool DecidarTemTenda(RNGDeterministico rng, int fasesSemTenda)
    {
        if (fasesSemTenda >= PityFasesSemTenda)
        {
            Debug.LogFormat("[TENDA] [PITY ATIVADO] Garantida após {0} fases sem tenda.", PityFasesSemTenda);
            return true;
        }

        bool tem = rng.NextFloat() < ChanceBase;
        Debug.LogFormat("[TENDA] {0}", tem ? "Tenda aparece nesta fase." : "Sem tenda nesta fase.");
        return tem;
    }

    /// <summary>
    /// Sorteia o tipo de tenda com as probabilidades configuradas:
    /// Mercador 40% | Heróis 30% | Mista 20% | EventoEspecial 10%.
    /// </summary>
    public TipoTenda DecidarTipoTenda(RNGDeterministico rng)
    {
        float val = rng.NextFloat();
        TipoTenda tipo;
        if (val < LimiarMercador)       tipo = TipoTenda.Mercador;
        else if (val < LimiarHerois)    tipo = TipoTenda.Herois;
        else if (val < LimiarMista)     tipo = TipoTenda.Mista;
        else                            tipo = TipoTenda.EventoEspecial;

        Debug.LogFormat("[TENDA] [TIPO TENDA] {0}", tipo);
        return tipo;
    }
}
