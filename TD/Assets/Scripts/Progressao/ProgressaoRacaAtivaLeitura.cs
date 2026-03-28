public static class ProgressaoRacaAtivaLeitura
{
    public static ProgressaoRacaSnapshot ObterSnapshotRacaAtiva()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || PerfilJogadorAtivo.Dados == null)
            return ProgressaoRacaSnapshot.Vazio();

        DadosSlotSave dados = PerfilJogadorAtivo.Dados;
        TipoRaca raca = dados.raca;
        int expPermanente = dados.ObterExpPermanenteRaca(raca);
        int expTempRun = 0;
        if (RunExpPartida.Instance != null && !RunExpPartida.Instance.RunResolvida)
            expTempRun = RunExpPartida.Instance.ExpTemporariaRun;
        int expTotal = expPermanente + expTempRun;
        int nivel = NivelRacaCalculadora.CalcularNivelPorExpTotal(expTotal);
        int expNo = NivelRacaCalculadora.ObterExpAcumuladaNoNivelAtual(expTotal);
        int span = NivelRacaCalculadora.ObterExpTotalParaCompletarSegmentoDoNivelAtual(expTotal);
        int restante = NivelRacaCalculadora.ObterExpRestanteParaProximoNivel(expTotal);
        float p01 = NivelRacaCalculadora.CalcularProgressoDentroDoNivel01(expTotal);
        string nome = ObterNomeExibicaoRaca(raca);

        return new ProgressaoRacaSnapshot(
            true,
            raca,
            nome,
            nivel,
            expTotal,
            expNo,
            span,
            restante,
            p01,
            nivel + 1);
    }

    public static string ObterNomeExibicaoRaca(TipoRaca raca)
    {
        return raca switch
        {
            TipoRaca.Goblin => "Goblin",
            TipoRaca.Elfo => "Elfo",
            TipoRaca.Anao => "Anão",
            TipoRaca.Humano => "Humano",
            _ => raca.ToString()
        };
    }
}
