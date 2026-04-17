using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Protege as regras do bestiário: registro de mortes, milestones, pontos,
/// desbloqueio de runas, equipamento exclusivo e custo de remoção.
/// BestiarioModoTeste.Ativo deve estar false (padrão) para estes testes.
/// </summary>
public class BestiarioRegrasTests
{
    [SetUp]
    public void Setup()
    {
        BestiarioModoTeste.Ativo = false;
    }

    [TearDown]
    public void TearDown()
    {
        BestiarioModoTeste.Ativo = false;
    }

    // ── HELPERS ────────────────────────────────────────────────────────────

    private static BestiarioDatabase CriarDatabase(
        BestiarioCriaturaDef[] criaturas = null,
        BestiarioRunaDef[] runas = null)
    {
        var db = ScriptableObject.CreateInstance<BestiarioDatabase>();
        db.criaturas = criaturas ?? System.Array.Empty<BestiarioCriaturaDef>();
        db.runas     = runas     ?? System.Array.Empty<BestiarioRunaDef>();
        return db;
    }

    private static BestiarioCriaturaDef CriarCriatura(
        string id, int[] marcos, int[] pontos, string racaId = "Humano")
    {
        return new BestiarioCriaturaDef
        {
            id = id, racaId = racaId, nomeExibicao = id,
            marcosMortes = marcos, pontosPorEstagio = pontos
        };
    }

    private static BestiarioRunaDef CriarRuna(
        string id, int custoPontos, int estagiosMinimos = 1)
    {
        return new BestiarioRunaDef
        {
            id = id, nomeExibicao = id,
            custoPontosLiberacao = custoPontos,
            estagiosMinimos = estagiosMinimos,
            tipoEfeito = TipoEfeitoRuna.Nenhum
        };
    }

    private static DadosSlotSave CriarSave()
    {
        var dados = new DadosSlotSave(0, TipoRaca.Goblin, GeneroChefe.Masculino);
        dados.NormalizarBestiario();
        return dados;
    }

    // ── REGISTRO DE MORTES ─────────────────────────────────────────────────

    [Test]
    public void Should_IncrementKillCount_When_EnemyDies()
    {
        var criatura = CriarCriatura("goblin_guerreiro", new[] { 100 }, new[] { 10 });
        var db = CriarDatabase(new[] { criatura });
        var dados = CriarSave();

        BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);

        Assert.AreEqual(1, BestiarioRegras.ObterMortes(dados, "goblin_guerreiro"));
    }

    [Test]
    public void Should_AccumulateKillCount_AcrossMultipleDeaths()
    {
        var criatura = CriarCriatura("goblin_guerreiro", new[] { 100 }, new[] { 10 });
        var db = CriarDatabase(new[] { criatura });
        var dados = CriarSave();

        for (int i = 0; i < 5; i++)
            BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);

        Assert.AreEqual(5, BestiarioRegras.ObterMortes(dados, "goblin_guerreiro"));
    }

    [Test]
    public void Should_IgnoreRegistrarMorte_When_CreatureNotInDatabase()
    {
        var db = CriarDatabase(); // banco vazio
        var dados = CriarSave();

        BestiarioRegras.RegistrarMorte("criatura_inexistente", db, dados);

        Assert.AreEqual(0, BestiarioRegras.ObterMortes(dados, "criatura_inexistente"));
    }

    [Test]
    public void Should_NeverHaveNegativeKillCount()
    {
        // Garante que mortes nunca ficam negativas por manipulação direta
        var dados = CriarSave();
        var prog = BestiarioRegras.ObterOuCriarProgresso(dados.bestiario, "teste");
        prog.mortes = -5;

        var criatura = CriarCriatura("teste", new[] { 100 }, new[] { 10 });
        var db = CriarDatabase(new[] { criatura });

        BestiarioRegras.RegistrarMorte("teste", db, dados);

        // Após RegistrarMorte com mortes=-5: Mathf.Max(0, -5+1) = 0
        Assert.GreaterOrEqual(BestiarioRegras.ObterMortes(dados, "teste"), 0);
    }

    // ── MILESTONES E PONTOS ────────────────────────────────────────────────

    [Test]
    public void Should_AwardPoints_When_KillMilestoneIsReached()
    {
        var criatura = CriarCriatura("goblin_guerreiro", new[] { 3 }, new[] { 10 });
        var db = CriarDatabase(new[] { criatura });
        var dados = CriarSave();

        BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);
        BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);
        BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados); // 3ª morte = milestone

        Assert.AreEqual(10, dados.bestiario.pontosBestiario);
    }

    [Test]
    public void Should_NotAwardPoints_Before_MilestoneReached()
    {
        var criatura = CriarCriatura("goblin_guerreiro", new[] { 5 }, new[] { 10 });
        var db = CriarDatabase(new[] { criatura });
        var dados = CriarSave();

        BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);
        BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);

        Assert.AreEqual(0, dados.bestiario.pontosBestiario);
    }

    [Test]
    public void Should_NotAwardPointsTwice_ForSameMilestone()
    {
        var criatura = CriarCriatura("goblin_guerreiro", new[] { 2 }, new[] { 10 });
        var db = CriarDatabase(new[] { criatura });
        var dados = CriarSave();

        // Atingir e ultrapassar o milestone
        for (int i = 0; i < 6; i++)
            BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);

        Assert.AreEqual(10, dados.bestiario.pontosBestiario,
            "Pontos do milestone devem ser concedidos exatamente uma vez");
    }

    [Test]
    public void Should_AwardPointsForEachMilestoneOnce_WhenMultipleMilestones()
    {
        var criatura = CriarCriatura("goblin_guerreiro",
            new[] { 2, 4 }, new[] { 10, 20 });
        var db = CriarDatabase(new[] { criatura });
        var dados = CriarSave();

        // 2 mortes → milestone 1 (10 pts)
        BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);
        BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);
        Assert.AreEqual(10, dados.bestiario.pontosBestiario);

        // 4 mortes → milestone 2 (+20 pts)
        BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);
        BestiarioRegras.RegistrarMorte("goblin_guerreiro", db, dados);
        Assert.AreEqual(30, dados.bestiario.pontosBestiario);
    }

    [Test]
    public void Should_ObterEstagiosCompletos_ReturnCorrectCountForAllBoundaries()
    {
        var criatura = CriarCriatura("id", new[] { 100, 300, 500 }, new[] { 10, 20, 30 });

        Assert.AreEqual(0, BestiarioRegras.ObterEstagiosCompletos(criatura, 0));
        Assert.AreEqual(0, BestiarioRegras.ObterEstagiosCompletos(criatura, 99));
        Assert.AreEqual(1, BestiarioRegras.ObterEstagiosCompletos(criatura, 100));
        Assert.AreEqual(1, BestiarioRegras.ObterEstagiosCompletos(criatura, 299));
        Assert.AreEqual(2, BestiarioRegras.ObterEstagiosCompletos(criatura, 300));
        Assert.AreEqual(3, BestiarioRegras.ObterEstagiosCompletos(criatura, 500));
        Assert.AreEqual(3, BestiarioRegras.ObterEstagiosCompletos(criatura, 9999));
    }

    // ── DESBLOQUEIO DE RUNAS ───────────────────────────────────────────────

    [Test]
    public void Should_NotUnlockRuna_When_PointsInsufficient()
    {
        var runa = CriarRuna("runa_eco", 50);
        var db = CriarDatabase(runas: new[] { runa });
        var dados = CriarSave();
        dados.bestiario.pontosBestiario = 30;

        bool resultado = BestiarioRegras.TentarLiberarRuna("runa_eco", db, dados);

        Assert.IsFalse(resultado);
        Assert.IsFalse(BestiarioRegras.RunaEstaLiberada(dados.bestiario, "runa_eco"));
    }

    [Test]
    public void Should_UnlockRuna_And_DeductPoints_When_PointsSufficient()
    {
        var runa = CriarRuna("runa_eco", 50);
        var db = CriarDatabase(runas: new[] { runa });
        var dados = CriarSave();
        dados.bestiario.pontosBestiario = 100;

        bool resultado = BestiarioRegras.TentarLiberarRuna("runa_eco", db, dados);

        Assert.IsTrue(resultado);
        Assert.IsTrue(BestiarioRegras.RunaEstaLiberada(dados.bestiario, "runa_eco"));
        Assert.AreEqual(50, dados.bestiario.pontosBestiario);
    }

    [Test]
    public void Should_NotUnlockSameRunaTwice()
    {
        var runa = CriarRuna("runa_eco", 10);
        var db = CriarDatabase(runas: new[] { runa });
        var dados = CriarSave();
        dados.bestiario.pontosBestiario = 100;

        BestiarioRegras.TentarLiberarRuna("runa_eco", db, dados);
        bool segundaTentativa = BestiarioRegras.TentarLiberarRuna("runa_eco", db, dados);

        Assert.IsFalse(segundaTentativa);
        Assert.AreEqual(90, dados.bestiario.pontosBestiario, "Pontos só devem ser deduzidos uma vez");
    }

    [Test]
    public void Should_NeverHaveNegativePointsBestiario_AfterUnlock()
    {
        var runa = CriarRuna("runa_eco", 10);
        var db = CriarDatabase(runas: new[] { runa });
        var dados = CriarSave();
        dados.bestiario.pontosBestiario = 10; // exatamente o suficiente

        BestiarioRegras.TentarLiberarRuna("runa_eco", db, dados);

        Assert.GreaterOrEqual(dados.bestiario.pontosBestiario, 0);
    }

    // ── EQUIPAMENTO DE RUNAS ───────────────────────────────────────────────

    [Test]
    public void Should_NotEquipRuna_When_NotUnlocked()
    {
        var criatura = CriarCriatura("goblin_archer", new[] { 1 }, new[] { 100 });
        var runa = CriarRuna("runa_crit", 10, estagiosMinimos: 1);
        var db = CriarDatabase(new[] { criatura }, new[] { runa });
        var dados = CriarSave();
        BestiarioRegras.RegistrarMorte("goblin_archer", db, dados);
        dados.bestiario.pontosBestiario = 100;
        // Intencionalmente não desbloqueia a runa

        bool resultado = BestiarioRegras.TentarEquiparRuna("goblin_archer", "runa_crit", db, dados);

        Assert.IsFalse(resultado);
    }

    [Test]
    public void Should_NotEquipRuna_When_StageRequirementNotMet()
    {
        var criatura = CriarCriatura("goblin_archer", new[] { 100 }, new[] { 50 });
        var runa = CriarRuna("runa_crit", 10, estagiosMinimos: 1); // exige estágio 1
        var db = CriarDatabase(new[] { criatura }, new[] { runa });
        var dados = CriarSave();
        // Sem mortes → estágio 0 < estagiosMinimos=1
        dados.bestiario.pontosBestiario = 100;
        BestiarioRegras.TentarLiberarRuna("runa_crit", db, dados);

        bool resultado = BestiarioRegras.TentarEquiparRuna("goblin_archer", "runa_crit", db, dados);

        Assert.IsFalse(resultado);
    }

    [Test]
    public void Should_EquipRuna_Successfully_When_AllRequirementsMet()
    {
        var criatura = CriarCriatura("goblin_archer", new[] { 1 }, new[] { 100 });
        var runa = CriarRuna("runa_crit", 10, estagiosMinimos: 1);
        var db = CriarDatabase(new[] { criatura }, new[] { runa });
        var dados = CriarSave();
        BestiarioRegras.RegistrarMorte("goblin_archer", db, dados);
        dados.bestiario.pontosBestiario = 100;
        BestiarioRegras.TentarLiberarRuna("runa_crit", db, dados);

        bool resultado = BestiarioRegras.TentarEquiparRuna("goblin_archer", "runa_crit", db, dados);

        Assert.IsTrue(resultado);
        Assert.AreEqual("runa_crit",
            BestiarioRegras.ObterRunaEquipadaNaCriatura(dados.bestiario, "goblin_archer"));
    }

    [Test]
    public void Should_NotEquipSameRuna_OnTwoDifferentCreatures()
    {
        var criatura1 = CriarCriatura("goblin_1", new[] { 1 }, new[] { 100 });
        var criatura2 = CriarCriatura("goblin_2", new[] { 1 }, new[] { 100 });
        var runa = CriarRuna("runa_crit", 10, estagiosMinimos: 1);
        var db = CriarDatabase(new[] { criatura1, criatura2 }, new[] { runa });
        var dados = CriarSave();

        BestiarioRegras.RegistrarMorte("goblin_1", db, dados);
        BestiarioRegras.RegistrarMorte("goblin_2", db, dados);
        dados.bestiario.pontosBestiario = 200;
        BestiarioRegras.TentarLiberarRuna("runa_crit", db, dados);

        BestiarioRegras.TentarEquiparRuna("goblin_1", "runa_crit", db, dados);
        bool segundoEquipamento = BestiarioRegras.TentarEquiparRuna("goblin_2", "runa_crit", db, dados);

        Assert.IsFalse(segundoEquipamento,
            "A mesma runa não pode ser equipada em duas criaturas ao mesmo tempo");
    }

    [Test]
    public void Should_NotEquipTwoDifferentRunas_OnSameCreature()
    {
        var criatura = CriarCriatura("goblin_warrior", new[] { 1 }, new[] { 100 });
        var runa1 = CriarRuna("runa_eco", 10, estagiosMinimos: 1);
        var runa2 = CriarRuna("runa_crit", 10, estagiosMinimos: 1);
        var db = CriarDatabase(new[] { criatura }, new[] { runa1, runa2 });
        var dados = CriarSave();

        BestiarioRegras.RegistrarMorte("goblin_warrior", db, dados);
        dados.bestiario.pontosBestiario = 200;
        BestiarioRegras.TentarLiberarRuna("runa_eco", db, dados);
        BestiarioRegras.TentarLiberarRuna("runa_crit", db, dados);
        BestiarioRegras.TentarEquiparRuna("goblin_warrior", "runa_eco", db, dados);

        bool segundoEquipamento = BestiarioRegras.TentarEquiparRuna("goblin_warrior", "runa_crit", db, dados);

        Assert.IsFalse(segundoEquipamento,
            "Uma criatura só pode ter uma runa equipada por vez");
        Assert.AreEqual("runa_eco",
            BestiarioRegras.ObterRunaEquipadaNaCriatura(dados.bestiario, "goblin_warrior"),
            "A runa original deve permanecer equipada");
    }

    // ── CUSTO DE REMOÇÃO ───────────────────────────────────────────────────

    [Test]
    public void Should_CalculateRunaRemovalCost_BasedOnPlayerLevel()
    {
        var dados = CriarSave();
        dados.AdicionarExpPermanenteRaca(TipoRaca.Goblin, 400); // nível 2

        int custo = BestiarioRegras.CalcularCustoOuroRemocaoRuna(dados);
        int nivelAtual = dados.ObterNivelRaca(TipoRaca.Goblin);

        Assert.AreEqual(BestiarioRegras.OuroRemocaoRunaPorNivelJogador * nivelAtual, custo);
    }

    [Test]
    public void Should_NeverHaveNegativeRemovalCost()
    {
        var dados = CriarSave(); // nível 1, XP 0
        int custo = BestiarioRegras.CalcularCustoOuroRemocaoRuna(dados);
        Assert.GreaterOrEqual(custo, 0);
    }

    [Test]
    public void Should_IncreaseRemovalCost_AsPlayerLevelIncreases()
    {
        var dadosBaixo = CriarSave();
        var dadosAlto = CriarSave();
        dadosAlto.AdicionarExpPermanenteRaca(TipoRaca.Goblin, 1100); // nível 3

        int custoNivel1 = BestiarioRegras.CalcularCustoOuroRemocaoRuna(dadosBaixo);
        int custoNivel3 = BestiarioRegras.CalcularCustoOuroRemocaoRuna(dadosAlto);

        Assert.Less(custoNivel1, custoNivel3,
            "O custo de remoção deve aumentar com o nível do jogador");
    }
}
