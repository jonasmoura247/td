using UnityEngine;

/// <summary>
/// Implementações concretas das estratégias de seleção de alvo.
/// Escolha via <see cref="PrioridadeAlvoTorre"/> no Inspector da torre.
/// </summary>

// ─── Primeiro (mais avançado no caminho) ────────────────────────────────────
public sealed class EstrategiaAlvoPrimeiro : IEstrategiaSelecionarAlvo
{
    public Transform SelecionarAlvo(Transform origemTorre, CandidatoAlvo[] candidatos, int contagem)
    {
        Transform escolhido = null;
        float melhorDist = float.MaxValue;
        float melhorDistTorre = float.MaxValue;

        for (int i = 0; i < contagem; i++)
        {
            ref CandidatoAlvo c = ref candidatos[i];
            if (c.distanciaRestanteTrajeto < melhorDist - 0.001f ||
                (Mathf.Approximately(c.distanciaRestanteTrajeto, melhorDist) && c.distanciaTorreSqr < melhorDistTorre))
            {
                melhorDist = c.distanciaRestanteTrajeto;
                melhorDistTorre = c.distanciaTorreSqr;
                escolhido = c.pontoAcerto;
            }
        }
        return escolhido;
    }
}

// ─── Último (mais atrasado no caminho) ─────────────────────────────────────
public sealed class EstrategiaAlvoUltimo : IEstrategiaSelecionarAlvo
{
    public Transform SelecionarAlvo(Transform origemTorre, CandidatoAlvo[] candidatos, int contagem)
    {
        Transform escolhido = null;
        float piorDist = float.MinValue;
        float melhorDistTorre = float.MaxValue;

        for (int i = 0; i < contagem; i++)
        {
            ref CandidatoAlvo c = ref candidatos[i];
            if (c.distanciaRestanteTrajeto > piorDist + 0.001f ||
                (Mathf.Approximately(c.distanciaRestanteTrajeto, piorDist) && c.distanciaTorreSqr < melhorDistTorre))
            {
                piorDist = c.distanciaRestanteTrajeto;
                melhorDistTorre = c.distanciaTorreSqr;
                escolhido = c.pontoAcerto;
            }
        }
        return escolhido;
    }
}

// ─── Mais forte (maior vida atual) ─────────────────────────────────────────
public sealed class EstrategiaAlvoMaisForte : IEstrategiaSelecionarAlvo
{
    public Transform SelecionarAlvo(Transform origemTorre, CandidatoAlvo[] candidatos, int contagem)
    {
        Transform escolhido = null;
        int melhorVida = int.MinValue;
        float melhorDistTorre = float.MaxValue;

        for (int i = 0; i < contagem; i++)
        {
            ref CandidatoAlvo c = ref candidatos[i];
            if (c.vidaAtual > melhorVida ||
                (c.vidaAtual == melhorVida && c.distanciaTorreSqr < melhorDistTorre))
            {
                melhorVida = c.vidaAtual;
                melhorDistTorre = c.distanciaTorreSqr;
                escolhido = c.pontoAcerto;
            }
        }
        return escolhido;
    }
}

// ─── Mais fraco (menor vida atual) ─────────────────────────────────────────
public sealed class EstrategiaAlvoMaisFraco : IEstrategiaSelecionarAlvo
{
    public Transform SelecionarAlvo(Transform origemTorre, CandidatoAlvo[] candidatos, int contagem)
    {
        Transform escolhido = null;
        int piorVida = int.MaxValue;
        float melhorDistTorre = float.MaxValue;

        for (int i = 0; i < contagem; i++)
        {
            ref CandidatoAlvo c = ref candidatos[i];
            if (c.vidaAtual < piorVida ||
                (c.vidaAtual == piorVida && c.distanciaTorreSqr < melhorDistTorre))
            {
                piorVida = c.vidaAtual;
                melhorDistTorre = c.distanciaTorreSqr;
                escolhido = c.pontoAcerto;
            }
        }
        return escolhido;
    }
}

/// <summary>Factory que retorna a implementação correta dado o enum.</summary>
public static class EstrategiaAlvoFactory
{
    public static IEstrategiaSelecionarAlvo Criar(PrioridadeAlvoTorre prioridade)
    {
        return prioridade switch
        {
            PrioridadeAlvoTorre.Ultimo     => new EstrategiaAlvoUltimo(),
            PrioridadeAlvoTorre.MaisForte  => new EstrategiaAlvoMaisForte(),
            PrioridadeAlvoTorre.MaisFraco  => new EstrategiaAlvoMaisFraco(),
            _                              => new EstrategiaAlvoPrimeiro(),
        };
    }
}
