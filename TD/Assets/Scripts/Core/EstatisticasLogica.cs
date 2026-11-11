using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logica pura de estatisticas — sem MonoBehaviour, testavel isoladamente.
/// EstatisticasManager delega para esta classe.
/// </summary>
public class EstatisticasLogica
{
    private DadosEstatisticas _dados;

    public DadosEstatisticas Dados => _dados;

    public EstatisticasLogica(DadosEstatisticas dados)
    {
        _dados = dados ?? new DadosEstatisticas();
        _dados.Normalizar();
    }

    // ---- Combate ----

    public void RegistrarKill(string nomePrefab, TipoRaca? racaInimiga)
    {
        var g = _dados.globais;
        g.totalMonstrosMortos++;

        if (string.IsNullOrEmpty(g.primeiroMonstroMorto))
            g.primeiroMonstroMorto = nomePrefab;
        g.ultimoMonstroMorto = nomePrefab;

        // Kill por tipo de unidade
        if (!string.IsNullOrEmpty(nomePrefab))
        {
            bool encontrado = false;
            foreach (var r in g.killsPorTipoUnidade)
            {
                if (r != null && r.tipoUnidade == nomePrefab)
                {
                    r.quantidade++;
                    encontrado = true;
                    break;
                }
            }
            if (!encontrado)
                g.killsPorTipoUnidade.Add(new RegistroKillUnidade(nomePrefab, 1));
        }

        // Kill por raca
        if (racaInimiga.HasValue)
        {
            switch (racaInimiga.Value)
            {
                case TipoRaca.Goblin: g.goblinsMortos++;  break;
                case TipoRaca.Humano: g.humanosMortos++;  break;
                case TipoRaca.Elfo:   g.elfosMortos++;    break;
                case TipoRaca.Anao:   g.anoesMortos++;    break;
            }
        }
    }

    // ---- Ouro ----

    public void RegistrarOuroAdicionado(int delta, int totalAtual)
    {
        if (delta <= 0) return;
        var g = _dados.globais;
        g.totalOuroAcumuladoLifetime += delta;
        if (totalAtual > g.maiorOuroSimultaneo)
            g.maiorOuroSimultaneo = totalAtual;
    }

    public void RegistrarOuroGasto(int delta)
    {
        if (delta <= 0) return;
        _dados.globais.totalOuroGasto += delta;
    }

    // ---- Waves ----

    public void RegistrarWaveCompletada(int numeroWave)
    {
        var g = _dados.globais;
        g.wavesCompletadas++;
        if (numeroWave > g.maiorWaveAtingida)
            g.maiorWaveAtingida = numeroWave;
    }

    // ---- Torres ----

    public void RegistrarTorreConstruida()    => _dados.globais.torresConstruidas++;
    public void RegistrarTorreDestruida()     => _dados.globais.torresDestruidas++;

    // ---- Partida ----

    /// <summary>
    /// Chame ao fim de uma partida.
    /// </summary>
    /// <param name="vitoria">Se o jogador venceu.</param>
    /// <param name="duracao">Duracao em segundos.</param>
    /// <param name="xpGanho">XP ganho nesta partida.</param>
    /// <param name="racaJogador">Raca do perfil ativo.</param>
    /// <param name="killsNestaPartida">Kills feitos nesta partida especificamente.</param>
    /// <param name="ouroAcumuladoNestaPartida">Ouro acumulado nesta partida.</param>
    public void FinalizarPartida(bool vitoria, float duracao, int xpGanho, TipoRaca racaJogador,
        int killsNestaPartida, long ouroAcumuladoNestaPartida)
    {
        var g = _dados.globais;

        // Partida
        g.totalPartidas++;
        if (vitoria)
        {
            g.totalVitorias++;
            g.sequenciaVitorias++;
            if (g.sequenciaVitorias > g.melhorSequenciaVitorias)
                g.melhorSequenciaVitorias = g.sequenciaVitorias;
        }
        else
        {
            g.totalDerrotas++;
            g.sequenciaVitorias = 0;
        }

        // Tempo
        g.tempoTotalPartidas += duracao;
        if (duracao > g.tempoMaiorPartida)
            g.tempoMaiorPartida = duracao;
        if (g.tempoMenorPartida < 0f || duracao < g.tempoMenorPartida)
            g.tempoMenorPartida = duracao;

        // Ultima partida
        if (g.ultimaPartida == null) g.ultimaPartida = new UltimaPartidaInfo();
        g.ultimaPartida.vitoria  = vitoria;
        g.ultimaPartida.duracao  = duracao;
        g.ultimaPartida.dataHora = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        // XP
        if (xpGanho > 0)
        {
            g.totalXPGanho += xpGanho;
            if (xpGanho > g.maiorXPPartida)
                g.maiorXPPartida = xpGanho;
        }

        // Por raca
        var porRaca = _dados.ObterOuCriarEstatisticasRaca(racaJogador);
        porRaca.partidasJogadas++;
        if (vitoria)   porRaca.vitorias++;
        else           porRaca.derrotas++;
        porRaca.killsComRaca          += killsNestaPartida;
        porRaca.ouroAcumuladoComRaca  += ouroAcumuladoNestaPartida;
    }

    // ---- Helpers ----

    public static TipoRaca? DetectarRacaPorNomePrefab(string nomePrefab)
    {
        if (string.IsNullOrEmpty(nomePrefab)) return null;
        string lower = nomePrefab.ToLowerInvariant();
        if (lower.Contains("goblin")) return TipoRaca.Goblin;
        if (lower.Contains("humano")) return TipoRaca.Humano;
        if (lower.Contains("elfo"))   return TipoRaca.Elfo;
        if (lower.Contains("anao") || lower.Contains("anoes") || lower.Contains("dwar")) return TipoRaca.Anao;
        return null;
    }
}
