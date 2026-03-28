public readonly struct ProgressaoRacaSnapshot
{
    public bool TemDadosValidos { get; }
    public TipoRaca Raca { get; }
    public string NomeExibicao { get; }
    public int Nivel { get; }
    public int ExpTotalPermanente { get; }
    public int ExpAcumuladaNoNivelAtual { get; }
    public int ExpTotalParaCompletarSegmentoDoNivel { get; }
    public int ExpRestanteParaProximoNivel { get; }
    public float ProgressoDentroDoNivel01 { get; }
    public int ProximoNivel { get; }

    public ProgressaoRacaSnapshot(
        bool temDadosValidos,
        TipoRaca raca,
        string nomeExibicao,
        int nivel,
        int expTotalPermanente,
        int expAcumuladaNoNivelAtual,
        int expTotalParaCompletarSegmentoDoNivel,
        int expRestanteParaProximoNivel,
        float progressoDentroDoNivel01,
        int proximoNivel)
    {
        TemDadosValidos = temDadosValidos;
        Raca = raca;
        NomeExibicao = nomeExibicao ?? "";
        Nivel = nivel;
        ExpTotalPermanente = expTotalPermanente;
        ExpAcumuladaNoNivelAtual = expAcumuladaNoNivelAtual;
        ExpTotalParaCompletarSegmentoDoNivel = expTotalParaCompletarSegmentoDoNivel;
        ExpRestanteParaProximoNivel = expRestanteParaProximoNivel;
        ProgressoDentroDoNivel01 = progressoDentroDoNivel01;
        ProximoNivel = proximoNivel;
    }

    public static ProgressaoRacaSnapshot Vazio()
    {
        return new ProgressaoRacaSnapshot(
            false,
            TipoRaca.Goblin,
            "",
            1,
            0,
            0,
            0,
            0,
            0f,
            2);
    }
}
