using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Garante as regras do gerador de relíquias para tendas:
/// - Nunca oferece relíquias já possuídas
/// - Gera exatamente 3 opções quando possível
/// - Sem duplicatas nas opções
/// - Funciona com banco vazio ou possuídas vazias
/// </summary>
public class GeradorReliquiasTendaTests
{
    private GeradorReliquiasTenda _gerador;
    private RNGDeterministico _rng;

    [SetUp]
    public void SetUp()
    {
        _gerador = new GeradorReliquiasTenda();
        _rng = new RNGDeterministico(42);
    }

    private ReliquiaDatabase CriarDatabase(int quantidade, RaridadeReliquia raridade = RaridadeReliquia.Comum)
    {
        var db = ScriptableObject.CreateInstance<ReliquiaDatabase>();
        var entradas = new ReliquiaDef[quantidade];
        for (int i = 0; i < quantidade; i++)
        {
            var def = ScriptableObject.CreateInstance<ReliquiaDef>();
            def.idReliquia = $"reliquia_{i}";
            def.raridade = raridade;
            entradas[i] = def;
        }
        db.entradas = entradas;
        return db;
    }

    private ReliquiaDatabase CriarDatabaseMista()
    {
        var db = ScriptableObject.CreateInstance<ReliquiaDatabase>();
        var entradas = new ReliquiaDef[8];
        RaridadeReliquia[] raridades = {
            RaridadeReliquia.Comum, RaridadeReliquia.Comum, RaridadeReliquia.Comum,
            RaridadeReliquia.Incomum, RaridadeReliquia.Incomum,
            RaridadeReliquia.Raro,
            RaridadeReliquia.Lendario, RaridadeReliquia.Lendario
        };
        for (int i = 0; i < entradas.Length; i++)
        {
            var def = ScriptableObject.CreateInstance<ReliquiaDef>();
            def.idReliquia = $"reliquia_{i}";
            def.raridade = raridades[i];
            entradas[i] = def;
        }
        db.entradas = entradas;
        return db;
    }

    // ── Relíquias possuídas ───────────────────────────────────────────────

    [Test]
    public void Should_NuncoOferecer_ReliquiasPossuidas()
    {
        var db = CriarDatabase(5);
        var possuidas = new List<string> { "reliquia_0", "reliquia_1" };

        for (int seed = 0; seed < 20; seed++)
        {
            var rng = new RNGDeterministico(seed);
            var opcoes = _gerador.GerarOpcoes(db, possuidas, rng);
            foreach (var id in opcoes)
                Assert.IsFalse(possuidas.Contains(id), $"Relíquia possuída '{id}' foi oferecida.");
        }
    }

    [Test]
    public void Should_RetornarListaVazia_QuandoTodasPossuidas()
    {
        var db = CriarDatabase(3);
        var possuidas = new List<string> { "reliquia_0", "reliquia_1", "reliquia_2" };
        var opcoes = _gerador.GerarOpcoes(db, possuidas, _rng);
        Assert.AreEqual(0, opcoes.Count);
    }

    // ── Quantidade de opções ──────────────────────────────────────────────

    [Test]
    public void Should_RetornarTresOpcoes_QuandoReliquiasSuficientes()
    {
        var db = CriarDatabase(10);
        var opcoes = _gerador.GerarOpcoes(db, new List<string>(), _rng);
        Assert.AreEqual(3, opcoes.Count);
    }

    [Test]
    public void Should_RetornarMenosDeTres_QuandoPoucasReliquiasDisponiveis()
    {
        var db = CriarDatabase(2);
        var opcoes = _gerador.GerarOpcoes(db, new List<string>(), _rng);
        Assert.LessOrEqual(opcoes.Count, 2);
    }

    [Test]
    public void Should_RetornarListaVazia_QuandoDatabaseNula()
    {
        var opcoes = _gerador.GerarOpcoes(null, new List<string>(), _rng);
        Assert.AreEqual(0, opcoes.Count);
    }

    [Test]
    public void Should_RetornarListaVazia_QuandoDatabaseVazia()
    {
        var db = ScriptableObject.CreateInstance<ReliquiaDatabase>();
        db.entradas = new ReliquiaDef[0];
        var opcoes = _gerador.GerarOpcoes(db, new List<string>(), _rng);
        Assert.AreEqual(0, opcoes.Count);
    }

    // ── Sem duplicatas ─────────────────────────────────────────────────────

    [Test]
    public void Should_NaoRepetir_ReliquiasNasMesmasOpcoes()
    {
        var db = CriarDatabase(10);
        for (int seed = 0; seed < 30; seed++)
        {
            var rng = new RNGDeterministico(seed);
            var opcoes = _gerador.GerarOpcoes(db, new List<string>(), rng);
            var unicos = new HashSet<string>(opcoes);
            Assert.AreEqual(opcoes.Count, unicos.Count, $"Seed {seed}: duplicatas encontradas nas opções.");
        }
    }

    // ── Pesos de raridade ─────────────────────────────────────────────────

    [Test]
    public void Should_PesosRaridade_SomairemUm()
    {
        float soma = 0f;
        foreach (float p in GeradorReliquiasTenda.PesosRaridade)
            soma += p;
        Assert.AreEqual(1f, soma, 0.001f);
    }

    // ── Determinismo ──────────────────────────────────────────────────────

    [Test]
    public void Should_MesmaSeed_GerarMesmasOpcoes()
    {
        var db = CriarDatabaseMista();
        var possuidas = new List<string>();

        var opcoes1 = _gerador.GerarOpcoes(db, possuidas, new RNGDeterministico(99));
        var opcoes2 = _gerador.GerarOpcoes(db, possuidas, new RNGDeterministico(99));

        Assert.AreEqual(opcoes1.Count, opcoes2.Count);
        for (int i = 0; i < opcoes1.Count; i++)
            Assert.AreEqual(opcoes1[i], opcoes2[i]);
    }

    // ── Possuídas nulas ────────────────────────────────────────────────────

    [Test]
    public void Should_NaoCrashear_QuandoPossuidasNula()
    {
        var db = CriarDatabase(5);
        Assert.DoesNotThrow(() => _gerador.GerarOpcoes(db, null, _rng));
    }
}
