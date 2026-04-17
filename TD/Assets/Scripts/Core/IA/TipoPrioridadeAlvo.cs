/// <summary>
/// Critério de seleção de alvo para o <see cref="SistemaIAUnidade"/>.
/// Configurável por unidade no Inspector.
/// </summary>
public enum TipoPrioridadeAlvo
{
    /// <summary>Inimigo mais próximo desta unidade (padrão — mais previsível).</summary>
    MaisProximo = 0,

    /// <summary>Inimigo com menor vida atual (fácil de eliminar — foco de fire).</summary>
    MenorVida = 1,

    /// <summary>Inimigo com maior danoBase (maior ameaça — eliminar primeiro).</summary>
    MaiorAmeaca = 2,

    /// <summary>
    /// Inimigo mais avançado no caminho (menor distância restante até o castelo).
    /// Equivale ao modo "Primeiro" das torres — mais urgente.
    /// </summary>
    TipoAlvo = 3,
}
