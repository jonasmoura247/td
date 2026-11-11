using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RegistroKillUnidade
{
    public string tipoUnidade;
    public int quantidade;

    public RegistroKillUnidade() { }
    public RegistroKillUnidade(string tipo, int qtd) { tipoUnidade = tipo; quantidade = qtd; }
}

[Serializable]
public class UltimaPartidaInfo
{
    public bool vitoria;
    public float duracao;
    public string dataHora;
}

[Serializable]
public class EstatisticasGlobais
{
    // Tempo
    public float tempoTotalPartidas;
    public float tempoMaiorPartida;
    /// <summary>-1 = ainda nao jogou nenhuma partida completa.</summary>
    public float tempoMenorPartida = -1f;

    // Partidas
    public int totalPartidas;
    public int totalVitorias;
    public int totalDerrotas;
    public int sequenciaVitorias;
    public int melhorSequenciaVitorias;
    public UltimaPartidaInfo ultimaPartida = new UltimaPartidaInfo();

    // Combate
    public int totalMonstrosMortos;
    public string primeiroMonstroMorto;
    public string ultimoMonstroMorto;
    public List<RegistroKillUnidade> killsPorTipoUnidade = new List<RegistroKillUnidade>();

    // Kills por raca inimiga
    public int goblinsMortos;
    public int humanosMortos;
    public int elfosMortos;
    public int anoesMortos;

    // Waves
    public int wavesCompletadas;
    public int maiorWaveAtingida;

    // Torres
    public int torresConstruidas;
    public int torresDestruidas;

    // Economia
    public long totalOuroAcumuladoLifetime;
    public long totalOuroGasto;
    public int maiorOuroSimultaneo;

    // Progressao XP
    public long totalXPGanho;
    public int maiorXPPartida;
}

[Serializable]
public class EstatisticasPorRaca
{
    public TipoRaca raca;
    public int partidasJogadas;
    public int vitorias;
    public int derrotas;
    public int killsComRaca;
    public long ouroAcumuladoComRaca;
}

[Serializable]
public class DadosEstatisticas
{
    public EstatisticasGlobais globais = new EstatisticasGlobais();
    public List<EstatisticasPorRaca> estatisticasPorRaca = new List<EstatisticasPorRaca>();

    // ---- Propriedades derivadas (nao serializadas) ----

    public float WinRate => globais.totalPartidas > 0
        ? (float)globais.totalVitorias / globais.totalPartidas * 100f
        : 0f;

    public float TempoMedioPartida => globais.totalPartidas > 0
        ? globais.tempoTotalPartidas / globais.totalPartidas
        : 0f;

    public string UnidadeMaisMorta
    {
        get
        {
            if (globais.killsPorTipoUnidade == null || globais.killsPorTipoUnidade.Count == 0)
                return null;
            RegistroKillUnidade max = null;
            foreach (var r in globais.killsPorTipoUnidade)
            {
                if (r == null) continue;
                if (max == null || r.quantidade > max.quantidade) max = r;
            }
            return max?.tipoUnidade;
        }
    }

    public float KillsPorMinuto
    {
        get
        {
            if (globais.tempoTotalPartidas <= 0f) return 0f;
            return globais.totalMonstrosMortos / (globais.tempoTotalPartidas / 60f);
        }
    }

    public EstatisticasPorRaca ObterOuCriarEstatisticasRaca(TipoRaca raca)
    {
        if (estatisticasPorRaca == null) estatisticasPorRaca = new List<EstatisticasPorRaca>();
        foreach (var e in estatisticasPorRaca)
            if (e != null && e.raca == raca) return e;
        var novo = new EstatisticasPorRaca { raca = raca };
        estatisticasPorRaca.Add(novo);
        return novo;
    }

    public EstatisticasPorRaca ObterEstatisticasRaca(TipoRaca raca)
    {
        if (estatisticasPorRaca == null) return null;
        foreach (var e in estatisticasPorRaca)
            if (e != null && e.raca == raca) return e;
        return null;
    }

    public float WinRatePorRaca(TipoRaca raca)
    {
        var e = ObterEstatisticasRaca(raca);
        if (e == null || e.partidasJogadas == 0) return 0f;
        return (float)e.vitorias / e.partidasJogadas * 100f;
    }

    public void Normalizar()
    {
        if (globais == null) globais = new EstatisticasGlobais();
        if (globais.killsPorTipoUnidade == null) globais.killsPorTipoUnidade = new List<RegistroKillUnidade>();
        if (globais.ultimaPartida == null) globais.ultimaPartida = new UltimaPartidaInfo();
        if (estatisticasPorRaca == null) estatisticasPorRaca = new List<EstatisticasPorRaca>();

        var g = globais;
        g.totalMonstrosMortos     = Mathf.Max(0, g.totalMonstrosMortos);
        g.totalVitorias           = Mathf.Max(0, g.totalVitorias);
        g.totalDerrotas           = Mathf.Max(0, g.totalDerrotas);
        g.totalPartidas           = Mathf.Max(0, g.totalPartidas);
        g.sequenciaVitorias       = Mathf.Max(0, g.sequenciaVitorias);
        g.melhorSequenciaVitorias = Mathf.Max(0, g.melhorSequenciaVitorias);
        g.maiorOuroSimultaneo     = Mathf.Max(0, g.maiorOuroSimultaneo);
        g.tempoTotalPartidas      = Mathf.Max(0f, g.tempoTotalPartidas);
        g.tempoMaiorPartida       = Mathf.Max(0f, g.tempoMaiorPartida);
        g.goblinsMortos           = Mathf.Max(0, g.goblinsMortos);
        g.humanosMortos           = Mathf.Max(0, g.humanosMortos);
        g.elfosMortos             = Mathf.Max(0, g.elfosMortos);
        g.anoesMortos             = Mathf.Max(0, g.anoesMortos);
        g.wavesCompletadas        = Mathf.Max(0, g.wavesCompletadas);
        g.maiorWaveAtingida       = Mathf.Max(0, g.maiorWaveAtingida);
        g.torresConstruidas       = Mathf.Max(0, g.torresConstruidas);
        g.torresDestruidas        = Mathf.Max(0, g.torresDestruidas);
        g.maiorXPPartida          = Mathf.Max(0, g.maiorXPPartida);

        if (g.totalOuroAcumuladoLifetime < 0) g.totalOuroAcumuladoLifetime = 0;
        if (g.totalOuroGasto < 0)             g.totalOuroGasto = 0;
        if (g.totalXPGanho < 0)               g.totalXPGanho = 0;

        foreach (var r in g.killsPorTipoUnidade)
            if (r != null) r.quantidade = Mathf.Max(0, r.quantidade);

        foreach (var e in estatisticasPorRaca)
        {
            if (e == null) continue;
            e.partidasJogadas = Mathf.Max(0, e.partidasJogadas);
            e.vitorias        = Mathf.Max(0, e.vitorias);
            e.derrotas        = Mathf.Max(0, e.derrotas);
            e.killsComRaca    = Mathf.Max(0, e.killsComRaca);
            if (e.ouroAcumuladoComRaca < 0) e.ouroAcumuladoComRaca = 0;
        }
    }
}
