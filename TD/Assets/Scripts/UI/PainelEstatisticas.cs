using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Painel de estatisticas do jogador com 4 abas (Geral, Combate, Recursos, Por Raca).
/// Ligado ao LobbyEstatisticasController que controla visibilidade do painel raiz.
/// </summary>
public class PainelEstatisticas : MonoBehaviour
{
    // ---- Abas ----
    [Header("Abas")]
    [SerializeField] private Button botaoAbaGeral;
    [SerializeField] private Button botaoAbaCombate;
    [SerializeField] private Button botaoAbaRecursos;
    [SerializeField] private Button botaoAbaPorRaca;

    [SerializeField] private GameObject painelGeral;
    [SerializeField] private GameObject painelCombate;
    [SerializeField] private GameObject painelRecursos;
    [SerializeField] private GameObject painelPorRaca;

    // ---- Geral ----
    [Header("Geral")]
    [SerializeField] private TextMeshProUGUI textoTotalPartidas;
    [SerializeField] private TextMeshProUGUI textoTotalVitorias;
    [SerializeField] private TextMeshProUGUI textoTotalDerrotas;
    [SerializeField] private TextMeshProUGUI textoWinRate;
    [SerializeField] private TextMeshProUGUI textoSequencia;
    [SerializeField] private TextMeshProUGUI textoMelhorSequencia;
    [SerializeField] private TextMeshProUGUI textoUltimaPartida;

    [Header("Geral - Tempo")]
    [SerializeField] private TextMeshProUGUI textoTempoTotal;
    [SerializeField] private TextMeshProUGUI textoTempoMedio;
    [SerializeField] private TextMeshProUGUI textoTempoMaior;
    [SerializeField] private TextMeshProUGUI textoTempoMenor;

    // ---- Combate ----
    [Header("Combate")]
    [SerializeField] private TextMeshProUGUI textoTotalMonstros;
    [SerializeField] private TextMeshProUGUI textoKillsPorMinuto;
    [SerializeField] private TextMeshProUGUI textoGoblinsMortos;
    [SerializeField] private TextMeshProUGUI textoHumanosMortos;
    [SerializeField] private TextMeshProUGUI textoElfosMortos;
    [SerializeField] private TextMeshProUGUI textoAnoesMortos;
    [SerializeField] private TextMeshProUGUI textoUnidadeMaisMorta;
    [SerializeField] private TextMeshProUGUI textoWavesCompletadas;
    [SerializeField] private TextMeshProUGUI textoMaiorWave;
    [SerializeField] private TextMeshProUGUI textoTorresConstruidas;
    [SerializeField] private TextMeshProUGUI textoTorresDestruidas;

    // ---- Recursos ----
    [Header("Recursos")]
    [SerializeField] private TextMeshProUGUI textoOuroTotal;
    [SerializeField] private TextMeshProUGUI textoOuroGasto;
    [SerializeField] private TextMeshProUGUI textoMaiorOuro;
    [SerializeField] private TextMeshProUGUI textoTotalXP;
    [SerializeField] private TextMeshProUGUI textoMaiorXPPartida;
    [SerializeField] private TextMeshProUGUI textoPrimeiroMonstro;
    [SerializeField] private TextMeshProUGUI textoUltimoMonstro;

    // ---- Por Raca ----
    [Header("Por Raca")]
    [SerializeField] private UIItemEstatisticasRaca[] itensRaca;

    private void Awake()
    {
        botaoAbaGeral?.onClick.AddListener(() => MostrarAba(0));
        botaoAbaCombate?.onClick.AddListener(() => MostrarAba(1));
        botaoAbaRecursos?.onClick.AddListener(() => MostrarAba(2));
        botaoAbaPorRaca?.onClick.AddListener(() => MostrarAba(3));
    }

    public void AoAbrirPainel()
    {
        MostrarAba(0);
        AtualizarUI();
    }

    private void MostrarAba(int indice)
    {
        if (painelGeral != null)    painelGeral.SetActive(indice == 0);
        if (painelCombate != null)  painelCombate.SetActive(indice == 1);
        if (painelRecursos != null) painelRecursos.SetActive(indice == 2);
        if (painelPorRaca != null)  painelPorRaca.SetActive(indice == 3);
    }

    private void AtualizarUI()
    {
        DadosEstatisticas dados = null;

        if (EstatisticasManager.Instance != null)
            dados = EstatisticasManager.Instance.ObterDados();

        if (dados == null)
            dados = new DadosEstatisticas();

        var g = dados.globais;

        // -- Geral --
        Set(textoTotalPartidas,  "Partidas: " + g.totalPartidas);
        Set(textoTotalVitorias,  "Vitorias: " + g.totalVitorias);
        Set(textoTotalDerrotas,  "Derrotas: " + g.totalDerrotas);
        Set(textoWinRate,        "Win Rate: " + dados.WinRate.ToString("F1") + "%");
        Set(textoSequencia,      "Sequencia atual: " + g.sequenciaVitorias);
        Set(textoMelhorSequencia,"Melhor sequencia: " + g.melhorSequenciaVitorias);

        if (g.ultimaPartida != null && !string.IsNullOrEmpty(g.ultimaPartida.dataHora))
        {
            string res = g.ultimaPartida.vitoria ? "Vitoria" : "Derrota";
            Set(textoUltimaPartida, "Ultima partida: " + res + " (" + g.ultimaPartida.dataHora + ")");
        }
        else
        {
            Set(textoUltimaPartida, "Ultima partida: --");
        }

        Set(textoTempoTotal,  "Tempo total: " + FormatarTempo(g.tempoTotalPartidas));
        Set(textoTempoMedio,  "Tempo medio: " + FormatarTempo(dados.TempoMedioPartida));
        Set(textoTempoMaior,  "Partida mais longa: " + FormatarTempo(g.tempoMaiorPartida));
        Set(textoTempoMenor,  "Partida mais curta: " + (g.tempoMenorPartida >= 0f ? FormatarTempo(g.tempoMenorPartida) : "--"));

        // -- Combate --
        Set(textoTotalMonstros,    "Monstros mortos: " + g.totalMonstrosMortos);
        Set(textoKillsPorMinuto,   "Kills/min: " + dados.KillsPorMinuto.ToString("F1"));
        Set(textoGoblinsMortos,    "Goblins mortos: " + g.goblinsMortos);
        Set(textoHumanosMortos,    "Humanos mortos: " + g.humanosMortos);
        Set(textoElfosMortos,      "Elfos mortos: " + g.elfosMortos);
        Set(textoAnoesMortos,      "Anoes mortos: " + g.anoesMortos);
        Set(textoUnidadeMaisMorta, "Unidade mais morta: " + (dados.UnidadeMaisMorta ?? "--"));
        Set(textoWavesCompletadas, "Waves completadas: " + g.wavesCompletadas);
        Set(textoMaiorWave,        "Maior wave atingida: " + g.maiorWaveAtingida);
        Set(textoTorresConstruidas,"Torres construidas: " + g.torresConstruidas);
        Set(textoTorresDestruidas, "Torres destruidas: " + g.torresDestruidas);

        // -- Recursos --
        Set(textoOuroTotal,       "Ouro acumulado total: " + g.totalOuroAcumuladoLifetime);
        Set(textoOuroGasto,       "Ouro gasto total: " + g.totalOuroGasto);
        Set(textoMaiorOuro,       "Maior ouro simultaneo: " + g.maiorOuroSimultaneo);
        Set(textoTotalXP,         "XP total ganho: " + g.totalXPGanho);
        Set(textoMaiorXPPartida,  "Maior XP em uma partida: " + g.maiorXPPartida);
        Set(textoPrimeiroMonstro, "Primeiro monstro morto: " + (string.IsNullOrEmpty(g.primeiroMonstroMorto) ? "--" : g.primeiroMonstroMorto));
        Set(textoUltimoMonstro,   "Ultimo monstro morto: " + (string.IsNullOrEmpty(g.ultimoMonstroMorto) ? "--" : g.ultimoMonstroMorto));

        // -- Por Raca --
        if (itensRaca != null)
        {
            foreach (var item in itensRaca)
            {
                if (item == null) continue;
                var e = dados.ObterEstatisticasRaca(item.raca);
                int partidas  = e != null ? e.partidasJogadas : 0;
                int vitorias  = e != null ? e.vitorias : 0;
                int derrotas  = e != null ? e.derrotas : 0;
                float wr      = dados.WinRatePorRaca(item.raca);
                int kills     = e != null ? e.killsComRaca : 0;
                long ouro     = e != null ? e.ouroAcumuladoComRaca : 0;

                Set(item.textoPartidas, "Partidas: " + partidas);
                Set(item.textoVitorias, "Vitorias: " + vitorias);
                Set(item.textoDerrotas, "Derrotas: " + derrotas);
                Set(item.textoWinRate,  "Win Rate: " + wr.ToString("F1") + "%");
                Set(item.textoKills,    "Kills: " + kills);
                Set(item.textoOuro,     "Ouro: " + ouro);
            }
        }
    }

    private static void Set(TextMeshProUGUI tmp, string texto)
    {
        if (tmp != null) tmp.text = texto;
    }

    private static string FormatarTempo(float segundos)
    {
        if (segundos <= 0f) return "0s";
        int total = Mathf.FloorToInt(segundos);
        int h     = total / 3600;
        int m     = (total % 3600) / 60;
        int s     = total % 60;
        if (h > 0) return h + "h " + m + "m " + s + "s";
        if (m > 0) return m + "m " + s + "s";
        return s + "s";
    }
}
