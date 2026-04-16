/// <summary>
/// Modos de seleção de alvo disponíveis para as torres.
/// Configurável por torre no Inspector via <see cref="TorreDadosSO"/> ou em runtime.
/// </summary>
public enum PrioridadeAlvoTorre
{
    /// <summary>Inimigo mais avançado no caminho (menor distância restante até o castelo).</summary>
    Primeiro = 0,

    /// <summary>Inimigo mais atrasado no caminho (maior distância restante até o castelo).</summary>
    Ultimo = 1,

    /// <summary>Inimigo com maior vida atual.</summary>
    MaisForte = 2,

    /// <summary>Inimigo com menor vida atual.</summary>
    MaisFraco = 3,
}
