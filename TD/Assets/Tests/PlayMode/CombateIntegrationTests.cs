using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.TestTools;

/// <summary>
/// Integração: combat entre unidades, registro no manager, e morte.
/// Cobre: dano aplicado corretamente, morte ao esgotar vida, deregistro automático,
/// e proteção contra stacking (mesma unidade registrada duas vezes).
/// </summary>
public class CombateIntegrationTests
{
    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── HELPERS ────────────────────────────────────────────────────────────

    private UnidadeController CriarUnidade(
        Faccao faccao, int vida = 100, int dano = 10, string nome = "Unidade")
    {
        var go = new GameObject(nome);
        var u = go.AddComponent<UnidadeController>();
        u.faccao = faccao;
        u.vidaMaxima = vida;
        u.danoBase = dano;
        u.SincronizarVidaAposSpawn();
        return u;
    }

    private UnidadesManager CriarManager()
    {
        var go = new GameObject("UnidadesManager");
        return go.AddComponent<UnidadesManager>();
    }

    // ── SPAWN E ESTADO INICIAL ─────────────────────────────────────────────

    [Test]
    public void Should_SpawnAlive_With_CorrectFaction()
    {
        var inimigo = CriarUnidade(Faccao.Inimigo, vida: 80);
        Assert.IsTrue(inimigo.EstaVivo);
        Assert.AreEqual(Faccao.Inimigo, inimigo.faccao);
    }

    [Test]
    public void Should_SpawnWithFullHealth()
    {
        var u = CriarUnidade(Faccao.Jogador, vida: 150);
        Assert.AreEqual(150, u.GetVidaAtual());
    }

    [UnityTest]
    public IEnumerator Should_OverrideFaction_When_DefinirFaccaoParaSpawnCalledBeforeStart()
    {
        var go = new GameObject("UnidadeOverride");
        var u = go.AddComponent<UnidadeController>();
        // GoblinController define Inimigo por padrão; forçamos Jogador antes do Start
        u.DefinirFaccaoParaSpawn(Faccao.Jogador);

        yield return null; // Start executa e aplica a facção forçada

        Assert.AreEqual(Faccao.Jogador, u.faccao,
            "DefinirFaccaoParaSpawn deve sobrescrever a facção padrão no Start");
    }

    // ── DANO ENTRE UNIDADES ────────────────────────────────────────────────

    [Test]
    public void Should_ReduceHealth_When_EnemyReceivesDamage()
    {
        var inimigo = CriarUnidade(Faccao.Inimigo, vida: 100);
        inimigo.ReceberDanoSemProcs(35);
        Assert.AreEqual(65, inimigo.GetVidaAtual());
    }

    [Test]
    public void Should_ReduceHealth_When_AllyReceivesDamage()
    {
        var aliado = CriarUnidade(Faccao.Jogador, vida: 100);
        aliado.ReceberDanoSemProcs(20);
        Assert.AreEqual(80, aliado.GetVidaAtual());
    }

    [Test]
    public void Should_Die_When_DamageExceedsRemainingHealth()
    {
        var inimigo = CriarUnidade(Faccao.Inimigo, vida: 50);
        inimigo.ReceberDanoSemProcs(50);
        Assert.IsFalse(inimigo.EstaVivo);
    }

    [Test]
    public void Should_SurviveOverkillLessThanFatal_WhenHealthRemains()
    {
        var aliado = CriarUnidade(Faccao.Jogador, vida: 100);
        aliado.ReceberDanoSemProcs(99);
        Assert.IsTrue(aliado.EstaVivo);
        Assert.AreEqual(1, aliado.GetVidaAtual());
    }

    [Test]
    public void Should_IgnoreSubsequentDamage_When_UnitIsDead()
    {
        var inimigo = CriarUnidade(Faccao.Inimigo, vida: 10);
        inimigo.ReceberDanoSemProcs(10); // mata

        Assert.IsFalse(inimigo.EstaVivo);
        Assert.DoesNotThrow(() => inimigo.ReceberDanoSemProcs(999),
            "Unidade morta deve ignorar dano silenciosamente");
    }

    // ── REGISTRO NO MANAGER (ANTI-STACKING) ───────────────────────────────

    [Test]
    public void Should_NotRegisterSameUnit_Twice()
    {
        var manager = CriarManager();
        var u = CriarUnidade(Faccao.Jogador);

        manager.Registar(u);
        manager.Registar(u);

        Assert.AreEqual(1, manager.UnidadesRegistadas.Count,
            "Registar a mesma unidade duas vezes deve manter apenas uma entrada");
    }

    [Test]
    public void Should_RegisterMultipleDistinctUnits()
    {
        var manager = CriarManager();
        var u1 = CriarUnidade(Faccao.Jogador, nome: "A");
        var u2 = CriarUnidade(Faccao.Jogador, nome: "B");
        var u3 = CriarUnidade(Faccao.Jogador, nome: "C");

        manager.Registar(u1);
        manager.Registar(u2);
        manager.Registar(u3);

        Assert.AreEqual(3, manager.UnidadesRegistadas.Count);
    }

    [Test]
    public void Should_DeregisterUnit_When_ExplicitlyRemoved()
    {
        var manager = CriarManager();
        var u = CriarUnidade(Faccao.Jogador);
        manager.Registar(u);

        manager.Desregistar(u);

        Assert.AreEqual(0, manager.UnidadesRegistadas.Count);
    }

    [Test]
    public void Should_OnlyRemoveTargetUnit_When_DeregisteredFromMultiple()
    {
        var manager = CriarManager();
        var u1 = CriarUnidade(Faccao.Jogador, nome: "A");
        var u2 = CriarUnidade(Faccao.Jogador, nome: "B");

        manager.Registar(u1);
        manager.Registar(u2);
        manager.Desregistar(u1);

        Assert.AreEqual(1, manager.UnidadesRegistadas.Count);
        Assert.IsTrue(manager.UnidadesRegistadas.Contains(u2));
    }

    // ── DEREGISTRO AUTOMÁTICO NA MORTE ─────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_AutoRegister_WhenUnitStartsAsAlly()
    {
        // Manager precisa existir ANTES do Start da unidade
        var manager = CriarManager();

        var go = new GameObject("AliadoAutoRegistro");
        var u = go.AddComponent<UnidadeController>();
        u.faccao = Faccao.Jogador;
        u.vidaMaxima = 50;

        yield return null; // Start: UnidadesManager.Instancia?.Registar(this)

        Assert.AreEqual(1, manager.UnidadesRegistadas.Count,
            "Unidade aliada deve se auto-registrar no Start");
    }

    [UnityTest]
    public IEnumerator Should_DeregisterAutomatically_When_AllyUnitDies()
    {
        var manager = CriarManager();

        var go = new GameObject("AliadoMortal");
        var u = go.AddComponent<UnidadeController>();
        u.faccao = Faccao.Jogador;
        u.vidaMaxima = 20;

        yield return null; // Start e auto-registro

        Assert.AreEqual(1, manager.UnidadesRegistadas.Count, "Deve estar registrado");

        u.ReceberDanoSemProcs(20); // Morrer() → Destroy(go, 0.1f) → OnDestroy → Desregistar

        yield return new WaitForSeconds(0.15f);

        Assert.AreEqual(0, manager.UnidadesRegistadas.Count,
            "Unidade morta deve se desregistrar automaticamente via OnDestroy");
    }

    [UnityTest]
    public IEnumerator Should_NotRegisterEnemies_InUnidadesManager()
    {
        var manager = CriarManager();

        var go = new GameObject("Inimigo");
        var u = go.AddComponent<UnidadeController>();
        u.faccao = Faccao.Inimigo;
        u.vidaMaxima = 50;

        yield return null; // Start

        Assert.AreEqual(0, manager.UnidadesRegistadas.Count,
            "Inimigos não devem ser registrados no UnidadesManager de aliados");
    }

    // ── MÚLTIPLAS UNIDADES — SOBREVIVÊNCIA CORRETA ─────────────────────────

    [Test]
    public void Should_TrackHealthIndependently_ForMultipleUnits()
    {
        var u1 = CriarUnidade(Faccao.Jogador, vida: 100, nome: "U1");
        var u2 = CriarUnidade(Faccao.Jogador, vida: 100, nome: "U2");
        var u3 = CriarUnidade(Faccao.Inimigo, vida: 100, nome: "U3");

        u1.ReceberDanoSemProcs(30);
        u3.ReceberDanoSemProcs(100); // mata u3

        Assert.AreEqual(70, u1.GetVidaAtual());
        Assert.AreEqual(100, u2.GetVidaAtual());
        Assert.IsFalse(u3.EstaVivo);
    }
}
