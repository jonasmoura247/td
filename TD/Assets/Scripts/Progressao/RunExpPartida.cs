using System;
using UnityEngine;

/// <summary>
/// Singleton Unity que expõe ExpPartidaLogica ao runtime do jogo.
/// Toda regra de negócio vive em <see cref="ExpPartidaLogica"/> — testável sem MonoBehaviour.
/// </summary>
public class RunExpPartida : MonoBehaviour
{
    public static event Action OnExpTemporariaAlterada;

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
    }

    public void ConcluirComDerrota()
    {
        int exp = Logica.ConcluirComDerrota(concederExpSemVitoriaParaTestes);
        if (concederExpSemVitoriaParaTestes)
            AplicarExpAoPerfilPermanente(exp);
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
