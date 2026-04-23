/// <summary>
/// Estados da máquina de estados da IA de unidade.
/// </summary>
public enum EstadoIA
{
    /// <summary>Sem alvo. Unidade está parada ou retomando rota de waypoints.</summary>
    Ocioso,

    /// <summary>
    /// Unidade aliada patrulhando entre PontoA e PontoB via <see cref="PatrulhaUnidade"/>.
    /// Interrompida automaticamente ao detectar inimigo; retomada após eliminar ameaças.
    /// </summary>
    Patrulhando,

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

    /// <summary>
    /// Unidade ranged recuando estrategicamente de um inimigo muito próximo.
    /// Move-se para longe do alvo enquanto ataca, mantendo-se dentro dos limites de câmera.
    /// Retorna a Atacando quando a distância for segura, ou a Perseguindo se o inimigo afastou.
    /// </summary>
    Kiting,

    /// <summary>Unidade morta. Todos os sistemas de IA pararam.</summary>
    Morto,
}
