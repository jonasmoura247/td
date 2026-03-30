/// <summary>Nível de dificuldade da criatura no bestiário (presets de marcos / pontos).</summary>
public enum NivelDificuldadeBestiario
{
    Facil = 0,
    Medio = 1,
    Dificil = 2,
    Expert = 3
}

public enum TipoEfeitoRuna
{
    Nenhum = 0,
    /// <summary>Ao receber dano, chance de sofrer dano extra = ceil(vidaMax * fração).</summary>
    DanoEcoAoReceberDano = 1,
    /// <summary>Dano desta unidade ao castelo: crítico usa multiplicador elevado.</summary>
    CriticoMultiplicadorAtaque = 2
}
