using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Garante que HeroiDatabase retorna heróis corretamente por ID,
/// e que HeroiIds.Pirata está correto.
/// </summary>
public class HeroiDatabaseTests
{
    private HeroiDatabase CriarDatabase()
    {
        var db = ScriptableObject.CreateInstance<HeroiDatabase>();
        var pirata = ScriptableObject.CreateInstance<HeroiDef>();
        pirata.idHeroi = HeroiIds.Pirata;
        pirata.nomeExibicao = "Pirata";
        pirata.raridade = HeroiRaridade.Azul;
        db.entradas = new HeroiDef[] { pirata };
        return db;
    }

    // ── ObterPorId ─────────────────────────────────────────────────────────

    [Test]
    public void Should_ObterPorId_RetornarHeroi_QuandoExiste()
    {
        var db = CriarDatabase();
        var heroi = db.ObterPorId(HeroiIds.Pirata);
        Assert.IsNotNull(heroi);
        Assert.AreEqual(HeroiIds.Pirata, heroi.idHeroi);
    }

    [Test]
    public void Should_ObterPorId_RetornarNull_QuandoNaoExiste()
    {
        var db = CriarDatabase();
        Assert.IsNull(db.ObterPorId("id_inexistente"));
    }

    [Test]
    public void Should_ObterPorId_RetornarNull_QuandoIdNulo()
    {
        var db = CriarDatabase();
        Assert.IsNull(db.ObterPorId(null));
    }

    [Test]
    public void Should_ObterPorId_RetornarNull_QuandoIdVazio()
    {
        var db = CriarDatabase();
        Assert.IsNull(db.ObterPorId(""));
    }

    [Test]
    public void Should_ObterPorId_RetornarNull_QuandoDatabaseVazia()
    {
        var db = ScriptableObject.CreateInstance<HeroiDatabase>();
        db.entradas = new HeroiDef[0];
        Assert.IsNull(db.ObterPorId(HeroiIds.Pirata));
    }

    [Test]
    public void Should_ObterPorId_RetornarNull_QuandoEntradasNulas()
    {
        var db = ScriptableObject.CreateInstance<HeroiDatabase>();
        db.entradas = null;
        Assert.IsNull(db.ObterPorId(HeroiIds.Pirata));
    }

    // ── HeroiDef Pirata ────────────────────────────────────────────────────

    [Test]
    public void Should_HeroiPirata_TerRaridadeAzul()
    {
        var db = CriarDatabase();
        var heroi = db.ObterPorId(HeroiIds.Pirata);
        Assert.AreEqual(HeroiRaridade.Azul, heroi.raridade);
    }

    // ── HeroiIds ───────────────────────────────────────────────────────────

    [Test]
    public void Should_HeroiIds_Pirata_NaoSerNuloOuVazio()
    {
        Assert.IsFalse(string.IsNullOrEmpty(HeroiIds.Pirata));
    }
}
