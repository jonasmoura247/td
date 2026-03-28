public enum TipoEfeitoRuna
{
    Nenhum = 0,
    /// <summary>Ao receber dano, chance de sofrer dano extra = ceil(vidaMax * fração).</summary>
    DanoEcoAoReceberDano = 1,
    /// <summary>Dano desta unidade ao castelo: crítico usa multiplicador elevado.</summary>
    CriticoMultiplicadorAtaque = 2
}
