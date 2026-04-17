using System;

/// <summary>
/// Lógica pura de acumulação de XP durante uma partida — sem MonoBehaviour, sem Unity runtime.
/// RunExpPartida delega para esta classe, permitindo testes unitários isolados.
///
/// ConcluirComVitoria/Derrota retornam o delta de XP ganho.
/// O chamador (RunExpPartida) é responsável por persistir esse valor no save.
/// </summary>
public class ExpPartidaLogica
{
    public const int ExpPorInimigoComum = 50;
    public const int ExpPorBoss = 150;

    private int _expTemporariaRun;
    private bool _runResolvida;

    public event Action OnExpTemporariaAlterada;

    public int ExpTemporariaRun => _expTemporariaRun;
    public bool RunResolvida => _runResolvida;

    public void RegistrarMorteInimigo(bool ehBoss)
    {
        if (_runResolvida) return;
        _expTemporariaRun += ehBoss ? ExpPorBoss : ExpPorInimigoComum;
        OnExpTemporariaAlterada?.Invoke();
    }

    /// <summary>
    /// Conclui com vitória. Retorna a XP acumulada que deve ser salva no perfil permanente.
    /// Chamadas subsequentes retornam 0 (run já resolvida).
    /// </summary>
    public int ConcluirComVitoria()
    {
        if (_runResolvida) return 0;
        _runResolvida = true;
        int expGanha = _expTemporariaRun;
        _expTemporariaRun = 0;
        OnExpTemporariaAlterada?.Invoke();
        return expGanha;
    }

    /// <summary>
    /// Conclui com derrota. Retorna a XP a ser salva:
    /// o valor acumulado se <paramref name="concederExpSemVitoria"/> for true, senão 0.
    /// </summary>
    public int ConcluirComDerrota(bool concederExpSemVitoria)
    {
        if (_runResolvida) return 0;
        _runResolvida = true;
        int expGanha = concederExpSemVitoria ? _expTemporariaRun : 0;
        _expTemporariaRun = 0;
        OnExpTemporariaAlterada?.Invoke();
        return expGanha;
    }
}
