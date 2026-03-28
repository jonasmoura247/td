using System;
using UnityEngine;

public class RunExpPartida : MonoBehaviour
{
    public static event Action OnExpTemporariaAlterada;
    public const int ExpPorInimigoComum = 50;
    public const int ExpPorBoss = 150;

    public static RunExpPartida Instance { get; private set; }

    [SerializeField] private bool concederExpSemVitoriaParaTestes;

    private int expTemporariaRun;
    private bool runResolvida;

    public int ExpTemporariaRun => expTemporariaRun;
    public bool ConcederExpSemVitoriaParaTestes => concederExpSemVitoriaParaTestes;
    public bool RunResolvida => runResolvida;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegistrarMorteInimigo(bool ehBoss)
    {
        if (runResolvida)
            return;
        expTemporariaRun += ehBoss ? ExpPorBoss : ExpPorInimigoComum;
        OnExpTemporariaAlterada?.Invoke();
    }

    public void ConcluirComVitoria()
    {
        if (runResolvida)
            return;
        runResolvida = true;
        AplicarExpTemporariaAoPerfilPermanente();
        expTemporariaRun = 0;
        OnExpTemporariaAlterada?.Invoke();
    }

    public void ConcluirComDerrota()
    {
        if (runResolvida)
            return;
        runResolvida = true;
        if (concederExpSemVitoriaParaTestes)
            AplicarExpTemporariaAoPerfilPermanente();
        expTemporariaRun = 0;
        OnExpTemporariaAlterada?.Invoke();
    }

    private void AplicarExpTemporariaAoPerfilPermanente()
    {
        if (expTemporariaRun <= 0)
            return;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return;
        int delta = expTemporariaRun;
        TipoRaca raca = PerfilJogadorAtivo.Raca;
        SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
        {
            if (d != null)
                d.AdicionarExpPermanenteRaca(raca, delta);
        });
    }
}
