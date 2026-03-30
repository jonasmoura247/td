using System;

/// <summary>Alinha <see cref="TipoRaca"/> do save com <see cref="BestiarioCriaturaDef.racaId"/> no database.</summary>
public static class BestiarioRacaIdUtil
{
    public static string ParaId(TipoRaca raca)
    {
        return raca switch
        {
            TipoRaca.Goblin => "Goblin",
            TipoRaca.Humano => "Humano",
            TipoRaca.Elfo => "Elfo",
            TipoRaca.Anao => "Anao",
            _ => ""
        };
    }

    public static bool MesmaRacaQueJogador(string racaIdCriatura, TipoRaca racaJogador)
    {
        if (string.IsNullOrEmpty(racaIdCriatura))
            return false;
        return string.Equals(racaIdCriatura, ParaId(racaJogador), StringComparison.Ordinal);
    }
}
