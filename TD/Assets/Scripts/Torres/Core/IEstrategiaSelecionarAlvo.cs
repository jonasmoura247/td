using UnityEngine;

/// <summary>
/// Contrato do Strategy Pattern para seleção de alvo nas torres.
/// Cada implementação define um critério de escolha independente.
/// </summary>
public interface IEstrategiaSelecionarAlvo
{
    /// <summary>
    /// Seleciona o melhor alvo dentre os <paramref name="candidatos"/> detectados no alcance da torre.
    /// </summary>
    /// <param name="origemTorre">Posição da torre (para cálculo de distância).</param>
    /// <param name="candidatos">Buffer de candidatos válidos já filtrados (inimigos vivos).</param>
    /// <param name="contagem">Quantidade de candidatos válidos no buffer.</param>
    /// <returns>Transform do alvo escolhido, ou <c>null</c> se não houver candidatos.</returns>
    Transform SelecionarAlvo(Transform origemTorre, CandidatoAlvo[] candidatos, int contagem);
}

/// <summary>Dados de um candidato a alvo, calculados uma única vez por frame pelo alvejamento.</summary>
public struct CandidatoAlvo
{
    public Transform pontoAcerto;
    public float distanciaRestanteTrajeto;
    public float distanciaTorreSqr;
    public int vidaAtual;
}
