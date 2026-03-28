using UnityEngine;

public static class BestiarioRunasSpawnAplicador
{
    public static void Aplicar(GameObject instancia, UnidadeController uc, BestiarioDatabase database)
    {
        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (instancia == null || uc == null || database == null)
            return;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return;

        var ident = instancia.GetComponent<IdentificadorBestiarioInimigo>();
        if (ident == null || string.IsNullOrEmpty(ident.CriaturaId))
            return;

        PerfilJogadorAtivo.Dados.NormalizarBestiario();
        string runaId = BestiarioRegras.ObterRunaEquipadaNaCriatura(
            PerfilJogadorAtivo.Dados.bestiario, ident.CriaturaId);
        if (string.IsNullOrEmpty(runaId))
            return;

        var runa = database.ObterRuna(runaId);
        if (runa == null)
            return;

        var st = instancia.GetComponent<RunaEstadoInimigo>();
        if (st == null)
            st = instancia.AddComponent<RunaEstadoInimigo>();

        st.chanceCritico = database.chanceCriticoBase;
        st.multiplicadorCriticoBase = database.multiplicadorCriticoBase;
        st.temDanoEco = false;
        st.temCriticoAprimorado = false;

        switch (runa.tipoEfeito)
        {
            case TipoEfeitoRuna.DanoEcoAoReceberDano:
                st.temDanoEco = true;
                st.chanceDanoEco = Mathf.Clamp01(runa.parametroChance);
                st.fracaoVidaParaEco = Mathf.Max(0.0001f, runa.parametroValor);
                break;
            case TipoEfeitoRuna.CriticoMultiplicadorAtaque:
                st.temCriticoAprimorado = true;
                st.multiplicadorCriticoComRuna = Mathf.Max(1f, runa.parametroValor);
                break;
        }
    }
}
