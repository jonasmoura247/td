using NUnit.Framework;

/// <summary>
/// Testes unitarios para EstatisticasLogica.
/// Sem MonoBehaviour, sem cena, sem singleton.
/// </summary>
public class EstatisticasLogicaTests
{
    private EstatisticasLogica CriarLogica() => new EstatisticasLogica(null);

    // ---- Estado inicial ----

    [Test]
    public void Should_StartWithZeroKills_When_NewInstance()
    {
        var logica = CriarLogica();
        Assert.AreEqual(0, logica.Dados.globais.totalMonstrosMortos);
    }

    [Test]
    public void Should_StartWithZeroPartidas_When_NewInstance()
    {
        var logica = CriarLogica();
        Assert.AreEqual(0, logica.Dados.globais.totalPartidas);
    }

    // ---- Kills ----

    [Test]
    public void Should_IncrementTotalMonstros_When_KillRegistered()
    {
        var logica = CriarLogica();
        logica.RegistrarKill("Goblin", TipoRaca.Goblin);
        Assert.AreEqual(1, logica.Dados.globais.totalMonstrosMortos);
    }

    [Test]
    public void Should_IncrementGoblinsMortos_When_GoblinKilled()
    {
        var logica = CriarLogica();
        logica.RegistrarKill("Goblin", TipoRaca.Goblin);
        Assert.AreEqual(1, logica.Dados.globais.goblinsMortos);
    }

    [Test]
    public void Should_TrackKillByType_When_SameTypeKilledTwice()
    {
        var logica = CriarLogica();
        logica.RegistrarKill("GoblinWarrior", TipoRaca.Goblin);
        logica.RegistrarKill("GoblinWarrior", TipoRaca.Goblin);
        bool found = false;
        foreach (var r in logica.Dados.globais.killsPorTipoUnidade)
            if (r.tipoUnidade == "GoblinWarrior" && r.quantidade == 2) { found = true; break; }
        Assert.IsTrue(found);
    }

    [Test]
    public void Should_RecordPrimeiroMonstro_When_FirstKill()
    {
        var logica = CriarLogica();
        logica.RegistrarKill("BossGoblin", TipoRaca.Goblin);
        Assert.AreEqual("BossGoblin", logica.Dados.globais.primeiroMonstroMorto);
    }

    // ---- Ouro ----

    [Test]
    public void Should_AccumulateOuro_When_OuroAdded()
    {
        var logica = CriarLogica();
        logica.RegistrarOuroAdicionado(100, 100);
        logica.RegistrarOuroAdicionado(50, 150);
        Assert.AreEqual(150, logica.Dados.globais.totalOuroAcumuladoLifetime);
    }

    [Test]
    public void Should_TrackMaiorOuro_When_LargeAmount()
    {
        var logica = CriarLogica();
        logica.RegistrarOuroAdicionado(200, 200);
        logica.RegistrarOuroAdicionado(10, 100);
        Assert.AreEqual(200, logica.Dados.globais.maiorOuroSimultaneo);
    }

    [Test]
    public void Should_AccumulateOuroGasto_When_OuroSpent()
    {
        var logica = CriarLogica();
        logica.RegistrarOuroGasto(75);
        Assert.AreEqual(75, logica.Dados.globais.totalOuroGasto);
    }

    // ---- Partidas ----

    [Test]
    public void Should_IncrementVitorias_When_VitoriaRegistered()
    {
        var logica = CriarLogica();
        logica.FinalizarPartida(true, 120f, 50, TipoRaca.Goblin, 10, 200);
        Assert.AreEqual(1, logica.Dados.globais.totalVitorias);
        Assert.AreEqual(0, logica.Dados.globais.totalDerrotas);
    }

    [Test]
    public void Should_IncrementDerrotas_When_DerrotaRegistered()
    {
        var logica = CriarLogica();
        logica.FinalizarPartida(false, 90f, 0, TipoRaca.Goblin, 5, 100);
        Assert.AreEqual(1, logica.Dados.globais.totalDerrotas);
        Assert.AreEqual(0, logica.Dados.globais.totalVitorias);
    }

    [Test]
    public void Should_TrackTempoMaior_When_LongestMatch()
    {
        var logica = CriarLogica();
        logica.FinalizarPartida(true, 300f, 0, TipoRaca.Goblin, 0, 0);
        logica.FinalizarPartida(true, 150f, 0, TipoRaca.Goblin, 0, 0);
        Assert.AreEqual(300f, logica.Dados.globais.tempoMaiorPartida, 0.01f);
    }

    [Test]
    public void Should_TrackTempoMenor_When_ShortestMatch()
    {
        var logica = CriarLogica();
        logica.FinalizarPartida(true, 200f, 0, TipoRaca.Goblin, 0, 0);
        logica.FinalizarPartida(true, 80f, 0, TipoRaca.Goblin, 0, 0);
        Assert.AreEqual(80f, logica.Dados.globais.tempoMenorPartida, 0.01f);
    }

    [Test]
    public void Should_TrackSequenciaVitorias_When_MultipleWins()
    {
        var logica = CriarLogica();
        logica.FinalizarPartida(true, 100f, 0, TipoRaca.Goblin, 0, 0);
        logica.FinalizarPartida(true, 100f, 0, TipoRaca.Goblin, 0, 0);
        logica.FinalizarPartida(true, 100f, 0, TipoRaca.Goblin, 0, 0);
        Assert.AreEqual(3, logica.Dados.globais.sequenciaVitorias);
        Assert.AreEqual(3, logica.Dados.globais.melhorSequenciaVitorias);
    }

    [Test]
    public void Should_ResetSequencia_When_Derrota()
    {
        var logica = CriarLogica();
        logica.FinalizarPartida(true, 100f, 0, TipoRaca.Goblin, 0, 0);
        logica.FinalizarPartida(true, 100f, 0, TipoRaca.Goblin, 0, 0);
        logica.FinalizarPartida(false, 100f, 0, TipoRaca.Goblin, 0, 0);
        Assert.AreEqual(0, logica.Dados.globais.sequenciaVitorias);
        Assert.AreEqual(2, logica.Dados.globais.melhorSequenciaVitorias);
    }

    // ---- DetectarRaca ----

    [Test]
    public void Should_DetectGoblin_When_NameContainsGoblin()
    {
        var raca = EstatisticasLogica.DetectarRacaPorNomePrefab("GoblinWarrior");
        Assert.AreEqual(TipoRaca.Goblin, raca);
    }

    [Test]
    public void Should_ReturnNull_When_UnknownPrefabName()
    {
        var raca = EstatisticasLogica.DetectarRacaPorNomePrefab("UnknownUnit");
        Assert.IsNull(raca);
    }

    // ---- Normalizar ----

    [Test]
    public void Should_Normalizar_When_NegativeValues()
    {
        var dados = new DadosEstatisticas();
        dados.globais.totalMonstrosMortos = -5;
        dados.globais.totalXPGanho = -100;
        dados.Normalizar();
        Assert.AreEqual(0, dados.globais.totalMonstrosMortos);
        Assert.AreEqual(0, dados.globais.totalXPGanho);
    }
}
