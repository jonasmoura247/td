using UnityEngine;

public static class ModificadoresPartidaTalentos
{
    public static TalentTreeDatabase CarregarDatabaseOuNull(TalentTreeDatabase referencia)
    {
        if (referencia != null)
            return referencia;
        return Resources.Load<TalentTreeDatabase>("Talentos/TalentTreeDatabase");
    }

    public static void Aplicar(UnidadeController unidade, TalentTreeDatabase database)
    {
        database = CarregarDatabaseOuNull(database);
        if (unidade == null || database == null)
            return;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return;

        DadosSlotSave dados = PerfilJogadorAtivo.Dados;
        dados.NormalizarTalentos();

        var mods = unidade.GetComponent<UnidadeModificadores>();
        if (mods == null)
            mods = unidade.gameObject.AddComponent<UnidadeModificadores>();
        mods.Limpar();

        UnidadeIdentidadeResolver.ObterOuInferir(unidade, out TipoRaca racaUnidade, out UnidadeSubtipo subUnidade);

        var nosIds = dados.talentos.nosDesbloqueados;
        if (nosIds == null || nosIds.Count == 0)
            return;

        for (int i = 0; i < nosIds.Count; i++)
        {
            var no = database.ObterNo(dados.raca, nosIds[i]);
            if (no == null || no.efeitos == null)
                continue;
            for (int j = 0; j < no.efeitos.Length; j++)
            {
                var e = no.efeitos[j];
                if (e == null)
                    continue;
                if (!EfeitoSeAplica(e, unidade, dados, racaUnidade, subUnidade))
                    continue;
                mods.AcumularEfeito(e);
            }
        }

        mods.AplicarVidaMaxima(unidade);
    }

    private static bool EfeitoSeAplica(
        TalentEffectEntry e,
        UnidadeController u,
        DadosSlotSave dados,
        TipoRaca racaUnidade,
        UnidadeSubtipo subUnidade)
    {
        switch (e.alvo)
        {
            case AlvoEfeitoTalento.MesmaRacaPerfil:
                if (racaUnidade != dados.raca)
                    return false;
                if (e.subtipoFiltro != UnidadeSubtipo.Indefinido && subUnidade != e.subtipoFiltro)
                    return false;
                return true;
            case AlvoEfeitoTalento.InimigoDeRaca:
                if (u.faccao != Faccao.Inimigo)
                    return false;
                return racaUnidade == e.racaAlvoInimigo;
            default:
                return false;
        }
    }
}
