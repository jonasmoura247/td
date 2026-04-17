/// <summary>
/// Estados da máquina de estados da IA de unidade.
/// </summary>
public enum EstadoIA
{
    /// <summary>Sem alvo. Unidade está parada ou retomando rota de waypoints.</summary>
    Ocioso,

    /// <summary>Tem alvo fora do alcance de ataque. Movendo-se em direção ao alvo.</summary>
    Perseguindo,

    /// <summary>Tem alvo dentro do alcance de ataque (por distância). Atacando em intervalo.</summary>
    Atacando,

    /// <summary>
    /// Colidiu fisicamente com unidade inimiga. Movimento zerado, ataque direto.
    /// Nenhum empurrão — velocidade forçada a zero todo FixedUpdate.
    /// Ao matar alvo retorna para Ocioso (retoma waypoints → castelo).
    /// </summary>
    Combate,

    /// <summary>Unidade morta. Todos os sistemas de IA pararam.</summary>
    Morto,
}
