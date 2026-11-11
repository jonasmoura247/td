using System;
using UnityEngine;

/// <summary>
/// Singleton que escuta eventos estaticos do jogo e acumula estatisticas no perfil ativo.
/// Persiste entre cenas. Salva apenas ao fim de cada partida.
/// </summary>
public class EstatisticasManager : MonoBehaviour
{
    public static EstatisticasManager Instance { get; private set; }

    private EstatisticasLogica _logica;

    // Contadores por partida (resetados ao iniciar uma nova)
    private int   _killsNestaPartida;
    private long  _ouroAcumuladoNestaPartida;
    private float _inicioPartida = -1f;
    private bool  _partidaAtiva;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        UnidadeController.OnUnidadeMorreu    += AoUnidadeMorrer;
        RunExpPartida.OnPartidaFinalizada    += AoPartidaFinalizar;
        OuroManager.OnOuroAdicionadoStatic   += AoOuroAdicionado;
        OuroManager.OnOuroGastoStatic        += AoOuroGasto;
        WaveManager.OnWaveCompletada         += AoWaveCompletada;
        TorresManager.OnTorreConstruida      += AoTorreConstruida;
        TorresManager.OnTorreDestruida       += AoTorreDestruida;
    }

    private void OnDisable()
    {
        UnidadeController.OnUnidadeMorreu    -= AoUnidadeMorrer;
        RunExpPartida.OnPartidaFinalizada    -= AoPartidaFinalizar;
        OuroManager.OnOuroAdicionadoStatic   -= AoOuroAdicionado;
        OuroManager.OnOuroGastoStatic        -= AoOuroGasto;
        WaveManager.OnWaveCompletada         -= AoWaveCompletada;
        TorresManager.OnTorreConstruida      -= AoTorreConstruida;
        TorresManager.OnTorreDestruida       -= AoTorreDestruida;
    }

    private void IniciarSessaoDePartida()
    {
        if (_partidaAtiva) return;
        _partidaAtiva              = true;
        _killsNestaPartida         = 0;
        _ouroAcumuladoNestaPartida = 0;
        _inicioPartida             = Time.realtimeSinceStartup;
    }

    private void AoUnidadeMorrer(UnidadeController unidade)
    {
        if (unidade == null || unidade.faccao != Faccao.Inimigo) return;
        IniciarSessaoDePartida();
        GarantirLogica();
        TipoRaca? raca = EstatisticasLogica.DetectarRacaPorNomePrefab(unidade.NomePrefab);
        _logica.RegistrarKill(unidade.NomePrefab, raca);
        _killsNestaPartida++;
    }

    private void AoOuroAdicionado(int delta, int total)
    {
        IniciarSessaoDePartida();
        GarantirLogica();
        _logica.RegistrarOuroAdicionado(delta, total);
        _ouroAcumuladoNestaPartida += delta;
    }

    private void AoOuroGasto(int delta)
    {
        IniciarSessaoDePartida();
        GarantirLogica();
        _logica.RegistrarOuroGasto(delta);
    }

    private void AoWaveCompletada(int numeroWave)
    {
        IniciarSessaoDePartida();
        GarantirLogica();
        _logica.RegistrarWaveCompletada(numeroWave);
    }

    private void AoTorreConstruida()
    {
        IniciarSessaoDePartida();
        GarantirLogica();
        _logica.RegistrarTorreConstruida();
    }

    private void AoTorreDestruida()
    {
        IniciarSessaoDePartida();
        GarantirLogica();
        _logica.RegistrarTorreDestruida();
    }

    private void AoPartidaFinalizar(bool vitoria, float duracaoFornecida, int xpGanho)
    {
        GarantirLogica();

        float duracao = _inicioPartida >= 0f
            ? Time.realtimeSinceStartup - _inicioPartida
            : duracaoFornecida;

        TipoRaca raca = PerfilJogadorAtivo.TemPerfilAtivo ? PerfilJogadorAtivo.Raca : TipoRaca.Goblin;

        _logica.FinalizarPartida(vitoria, duracao, xpGanho, raca, _killsNestaPartida, _ouroAcumuladoNestaPartida);

        SalvarEstatisticas();

        _partidaAtiva              = false;
        _killsNestaPartida         = 0;
        _ouroAcumuladoNestaPartida = 0;
        _inicioPartida             = -1f;
    }

    private void GarantirLogica()
    {
        if (_logica != null) return;

        DadosEstatisticas dados = null;
        if (PerfilJogadorAtivo.TemPerfilAtivo && PerfilJogadorAtivo.Dados != null)
            dados = PerfilJogadorAtivo.Dados.estatisticas;

        _logica = new EstatisticasLogica(dados ?? new DadosEstatisticas());

        // Reassociar referencia caso tenha sido null
        if (PerfilJogadorAtivo.TemPerfilAtivo && PerfilJogadorAtivo.Dados != null
            && PerfilJogadorAtivo.Dados.estatisticas == null)
        {
            PerfilJogadorAtivo.Dados.estatisticas = _logica.Dados;
        }
    }

    /// <summary>Invalida a logica cached (ex: ao trocar de perfil).</summary>
    public void ResetarLogicaCached()
    {
        _logica                    = null;
        _partidaAtiva              = false;
        _killsNestaPartida         = 0;
        _ouroAcumuladoNestaPartida = 0;
        _inicioPartida             = -1f;
    }

    private void SalvarEstatisticas()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null) return;
        var dados = _logica.Dados;
        SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
        {
            if (d != null)
                d.estatisticas = dados;
        });
    }

    /// <summary>Retorna os dados de estatisticas do perfil ativo (ou cria um novo se nao houver).</summary>
    public DadosEstatisticas ObterDados()
    {
        GarantirLogica();
        return _logica.Dados;
    }
}
