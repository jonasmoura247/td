using System;
using UnityEngine;

/// <summary>
/// Singleton Unity que expõe ExpPartidaLogica ao runtime do jogo.
/// Toda regra de negócio vive em <see cref="ExpPartidaLogica"/> — testável sem MonoBehaviour.
/// </summary>
public class RunExpPartida : MonoBehaviour
{
    public static event Action OnExpTemporariaAlterada;

    /// <summary>
    /// Disparado ao fim de cada partida (vitoria ou derrota).
    /// Parametros: (vitoria, duracao_segundos, xp_ganho).
    /// </summary>
    public static event Action<bool, float, int> OnPartidaFinalizada;

    /// <summary>
    /// Disparado uma vez por relíquia equipada ao vencer a partida.
    /// Parâmetro: idReliquia. Use para achievements e tracking externo.
    /// </summary>
    public static event Action<string> OnVitoriaComReliquia;

    public static RunExpPartida Instance { get; private set; }

    [SerializeField] private bool concederExpSemVitoriaParaTestes;

    /// <summary>Lógica pura de XP de partida. Acessível para testes de integração.</summary>
    public ExpPartidaLogica Logica { get; private set; } = new ExpPartidaLogica();

    public int ExpTemporariaRun                => Logica.ExpTemporariaRun;
    public bool ConcederExpSemVitoriaParaTestes => concederExpSemVitoriaParaTestes;
    public bool RunResolvida                   => Logica.RunResolvida;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Logica.OnExpTemporariaAlterada += () => OnExpTemporariaAlterada?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegistrarMorteInimigo(bool ehBoss) => Logica.RegistrarMorteInimigo(ehBoss);

    public void ConcluirComVitoria()
    {
        int exp = Logica.ConcluirComVitoria();
        AplicarExpAoPerfilPermanente(exp);
        RegistrarVitoriaFaseAtual();
        HeroisManager.Instance?.AvançarFase();
        DispararHooksVitoria();
        OnPartidaFinalizada?.Invoke(true, 0f, exp);
    }

    private void RegistrarVitoriaFaseAtual()
    {
        string faseId = FaseIdentificacao.FaseAtualId;
        if (string.IsNullOrEmpty(faseId)) return;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null) return;

        SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
        {
            if (d == null) return;
            if (d.progressaoFases == null) d.progressaoFases = new ProgressaoFases();
            d.progressaoFases.RegistrarVitoria(faseId);
        });
    }

    public void ConcluirComDerrota()
    {
        int exp = Logica.ConcluirComDerrota(concederExpSemVitoriaParaTestes);
        if (concederExpSemVitoriaParaTestes)
            AplicarExpAoPerfilPermanente(exp);
        OnPartidaFinalizada?.Invoke(false, 0f, exp);
    }

    private void DispararHooksVitoria()
    {
        if (ReliquiasManager.Instance == null) return;

        var equipadas = ReliquiasManager.Instance.GetReliquiasEquipadas();
        if (equipadas.Count == 0) return;

        if (PerfilJogadorAtivo.TemPerfilAtivo && SaveSlotManager.Instance != null)
        {
            SaveSlotManager.Instance.AtualizarSlotAtivo(dados =>
            {
                if (dados == null) return;
                foreach (var id in equipadas)
                    dados.DesbloquearDescricao(id);
            });
        }

        foreach (var id in equipadas)
            OnVitoriaComReliquia?.Invoke(id);
    }

    private void AplicarExpAoPerfilPermanente(int delta)
    {
        if (delta <= 0) return;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return;
        TipoRaca raca = PerfilJogadorAtivo.Raca;
        SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
        {
            if (d != null)
                d.AdicionarExpPermanenteRaca(raca, delta);
        });
    }
}
