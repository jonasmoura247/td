using NUnit.Framework;

/// <summary>
/// Testa todas as regras de negócio de XP de partida em isolamento total.
/// Sem MonoBehaviour, sem cena, sem singleton, sem SaveSlotManager.
/// </summary>
public class ExpPartidaLogicaTests
{
    // ── ESTADO INICIAL ─────────────────────────────────────────────────────

    [Test]
    public void Should_StartWithZeroExp_WhenCreated()
    {
        var logica = new ExpPartidaLogica();
        Assert.AreEqual(0, logica.ExpTemporariaRun);
    }

    [Test]
    public void Should_NotBeResolved_WhenCreated()
    {
        var logica = new ExpPartidaLogica();
        Assert.IsFalse(logica.RunResolvida);
    }

    // ── REGISTRO DE MORTES ─────────────────────────────────────────────────

    [Test]
    public void Should_AddComumExp_When_NormalEnemyDies()
    {
        var logica = new ExpPartidaLogica();
        logica.RegistrarMorteInimigo(ehBoss: false);
        Assert.AreEqual(ExpPartidaLogica.ExpPorInimigoComum, logica.ExpTemporariaRun);
    }

    [Test]
    public void Should_AddBossExp_When_BossEnemyDies()
    {
        var logica = new ExpPartidaLogica();
        logica.RegistrarMorteInimigo(ehBoss: true);
        Assert.AreEqual(ExpPartidaLogica.ExpPorBoss, logica.ExpTemporariaRun);
    }

    [Test]
    public void Should_BossExpBeGreaterThanComumExp()
    {
        Assert.Greater(ExpPartidaLogica.ExpPorBoss, ExpPartidaLogica.ExpPorInimigoComum);
    }

    [Test]
    public void Should_AccumulateExp_AcrossMultipleKills()
    {
        var logica = new ExpPartidaLogica();
        logica.RegistrarMorteInimigo(false);
        logica.RegistrarMorteInimigo(false);
        logica.RegistrarMorteInimigo(true);

        int esperado = (ExpPartidaLogica.ExpPorInimigoComum * 2) + ExpPartidaLogica.ExpPorBoss;
        Assert.AreEqual(esperado, logica.ExpTemporariaRun);
    }

    [Test]
    public void Should_FireOnExpAlterada_When_KillRegistered()
    {
        var logica = new ExpPartidaLogica();
        bool eventoDisparado = false;
        logica.OnExpTemporariaAlterada += () => eventoDisparado = true;

        logica.RegistrarMorteInimigo(false);

        Assert.IsTrue(eventoDisparado);
    }

    // ── CONCLUIR COM VITÓRIA ───────────────────────────────────────────────

    [Test]
    public void Should_ReturnAccumulatedExp_On_Victory()
    {
        var logica = new ExpPartidaLogica();
        logica.RegistrarMorteInimigo(false);
        logica.RegistrarMorteInimigo(false);

        int expRetornada = logica.ConcluirComVitoria();

        Assert.AreEqual(ExpPartidaLogica.ExpPorInimigoComum * 2, expRetornada);
    }

    [Test]
    public void Should_ResetExpToZero_After_Victory()
    {
        var logica = new ExpPartidaLogica();
        logica.RegistrarMorteInimigo(false);
        logica.ConcluirComVitoria();

        Assert.AreEqual(0, logica.ExpTemporariaRun);
    }

    [Test]
    public void Should_MarkRunAsResolved_After_Victory()
    {
        var logica = new ExpPartidaLogica();
        logica.ConcluirComVitoria();
        Assert.IsTrue(logica.RunResolvida);
    }

    [Test]
    public void Should_NotRegisterKills_After_Victory()
    {
        var logica = new ExpPartidaLogica();
        logica.ConcluirComVitoria();
        logica.RegistrarMorteInimigo(false);

        Assert.AreEqual(0, logica.ExpTemporariaRun);
    }

    [Test]
    public void Should_ReturnZero_On_DuplicateVictoryCall()
    {
        var logica = new ExpPartidaLogica();
        logica.RegistrarMorteInimigo(false);
        logica.ConcluirComVitoria();

        int segundaChamada = logica.ConcluirComVitoria();

        Assert.AreEqual(0, segundaChamada);
    }

    // ── CONCLUIR COM DERROTA ───────────────────────────────────────────────

    [Test]
    public void Should_ReturnZeroExp_On_Defeat_When_FlagIsFalse()
    {
        var logica = new ExpPartidaLogica();
        logica.RegistrarMorteInimigo(false);

        int expRetornada = logica.ConcluirComDerrota(concederExpSemVitoria: false);

        Assert.AreEqual(0, expRetornada);
    }

    [Test]
    public void Should_ReturnAccumulatedExp_On_Defeat_When_FlagIsTrue()
    {
        var logica = new ExpPartidaLogica();
        logica.RegistrarMorteInimigo(true); // boss

        int expRetornada = logica.ConcluirComDerrota(concederExpSemVitoria: true);

        Assert.AreEqual(ExpPartidaLogica.ExpPorBoss, expRetornada);
    }

    [Test]
    public void Should_ResetExpToZero_After_Defeat()
    {
        var logica = new ExpPartidaLogica();
        logica.RegistrarMorteInimigo(false);
        logica.ConcluirComDerrota(concederExpSemVitoria: true);

        Assert.AreEqual(0, logica.ExpTemporariaRun);
    }

    [Test]
    public void Should_MarkRunAsResolved_After_Defeat()
    {
        var logica = new ExpPartidaLogica();
        logica.ConcluirComDerrota(concederExpSemVitoria: false);
        Assert.IsTrue(logica.RunResolvida);
    }

    [Test]
    public void Should_ReturnZero_On_DuplicateDefeatCall()
    {
        var logica = new ExpPartidaLogica();
        logica.RegistrarMorteInimigo(false);
        logica.ConcluirComDerrota(false);

        int segundaChamada = logica.ConcluirComDerrota(false);

        Assert.AreEqual(0, segundaChamada);
    }

    [Test]
    public void Should_FireOnExpAlterada_When_ConcluirComVitoriaCalled()
    {
        var logica = new ExpPartidaLogica();
        bool eventoDisparado = false;
        logica.OnExpTemporariaAlterada += () => eventoDisparado = true;

        logica.ConcluirComVitoria();

        Assert.IsTrue(eventoDisparado);
    }

    [Test]
    public void Should_FireOnExpAlterada_When_ConcluirComDerrotaCalled()
    {
        var logica = new ExpPartidaLogica();
        bool eventoDisparado = false;
        logica.OnExpTemporariaAlterada += () => eventoDisparado = true;

        logica.ConcluirComDerrota(false);

        Assert.IsTrue(eventoDisparado);
    }
}
