using UnityEngine;

public static class BestiarioAtaqueInimigo
{
    /// <summary>Dano base já inclui talentos (GetDanoAtaque). Aplica rolagem de crítico.</summary>
    public static int CalcularDanoAoCastelo(UnidadeController atacante, int danoBase, BestiarioDatabase database)
    {
        return CalcularDanoAoCastelo(atacante, danoBase, database, out _);
    }

    /// <summary>Como <see cref="CalcularDanoAoCastelo(UnidadeController, int, BestiarioDatabase)"/>; expõe se o golpe foi crítico (texto flutuante).</summary>
    public static int CalcularDanoAoCastelo(
        UnidadeController atacante,
        int danoBase,
        BestiarioDatabase database,
        out bool foiCritico)
    {
        foiCritico = false;
        if (atacante == null || danoBase <= 0)
            return danoBase;

        // Base: campo por unidade (Inspector). Runa de bestiário pode sobrescrever.
        float chance = atacante.chanceCritico;
        float multUsado = atacante.multiplicadorCritico;

        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        float multBase = database != null ? database.multiplicadorCriticoBase : 1.2f;

        var st = atacante.GetComponent<RunaEstadoInimigo>();
        if (st != null)
        {
            chance = st.chanceCritico;
            multBase = st.multiplicadorCriticoBase;
            multUsado = multBase;
            if (st.temCriticoAprimorado)
                multUsado = st.multiplicadorCriticoComRuna;
        }

        if (Random.value < chance)
        {
            foiCritico = true;
            return Mathf.Max(1, Mathf.RoundToInt(danoBase * multUsado));
        }

        return danoBase;
    }
}
