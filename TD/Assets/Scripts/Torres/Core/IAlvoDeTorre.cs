using UnityEngine;

/// <summary>
/// Contrato opcional para alvos especiais (ex.: inimigos sem MovimentoUnidade).
/// O alvejamento padrão usa UnidadeController + MovimentoUnidade quando este interface não está presente.
/// </summary>
public interface IAlvoDeTorre
{
    Transform ObterTransformAcerto();
    bool PodeSerAlvejado();
    /// <summary>Distância restante ao objetivo; menor = mais avançado (maior prioridade).</summary>
    float ObterDistanciaRestanteTrajeto();
}
