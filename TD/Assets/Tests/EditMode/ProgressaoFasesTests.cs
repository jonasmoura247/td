using NUnit.Framework;
using System.Collections.Generic;

/// <summary>
/// Protege as regras de progressão de fases:
/// - Vitória é registrada e persistida por faseId
/// - Fase sem pré-requisito está sempre desbloqueada
/// - Fase com pré-requisito só é acessível após vencer a fase anterior
/// - Duplicatas não são criadas
/// - Lista nula nunca causa crash
/// </summary>
public class ProgressaoFasesTests
{
    // ── RegistrarVitoria ──────────────────────────────────────────────────

    [Test]
    public void Should_RegistrarVitoria_Corretamente()
    {
        var p = new ProgressaoFases();
        p.RegistrarVitoria("natural_1");
        Assert.IsTrue(p.FaseVencida("natural_1"));
    }

    [Test]
    public void Should_NaoDuplicar_QuandoRegistrarMesmaFaseDuasVezes()
    {
        var p = new ProgressaoFases();
        p.RegistrarVitoria("natural_1");
        p.RegistrarVitoria("natural_1");
        Assert.AreEqual(1, p.fasesVencidas.Count);
    }

    [Test]
    public void Should_NaoRegistrar_QuandoFaseIdNula()
    {
        var p = new ProgressaoFases();
        p.RegistrarVitoria(null);
        Assert.AreEqual(0, p.fasesVencidas.Count);
    }

    [Test]
    public void Should_NaoRegistrar_QuandoFaseIdVazia()
    {
        var p = new ProgressaoFases();
        p.RegistrarVitoria("");
        Assert.AreEqual(0, p.fasesVencidas.Count);
    }

    [Test]
    public void Should_InicializarLista_QuandoNula_AoRegistrar()
    {
        var p = new ProgressaoFases();
        p.fasesVencidas = null;
        p.RegistrarVitoria("natural_1");
        Assert.IsNotNull(p.fasesVencidas);
        Assert.IsTrue(p.FaseVencida("natural_1"));
    }

    // ── FaseVencida ───────────────────────────────────────────────────────

    [Test]
    public void Should_RetornarFalso_QuandoFaseNaoVencida()
    {
        var p = new ProgressaoFases();
        Assert.IsFalse(p.FaseVencida("natural_1"));
    }

    [Test]
    public void Should_RetornarFalso_QuandoListaVencidas_Nula()
    {
        var p = new ProgressaoFases();
        p.fasesVencidas = null;
        Assert.IsFalse(p.FaseVencida("natural_1"));
    }

    [Test]
    public void Should_RetornarFalso_QuandoFaseIdNulaOuVazia()
    {
        var p = new ProgressaoFases();
        Assert.IsFalse(p.FaseVencida(null));
        Assert.IsFalse(p.FaseVencida(""));
    }

    [Test]
    public void Should_NaoAfetar_OutrasFases_AoRegistrarVitoria()
    {
        var p = new ProgressaoFases();
        p.RegistrarVitoria("natural_1");
        Assert.IsFalse(p.FaseVencida("natural_2"));
        Assert.IsFalse(p.FaseVencida("natural_3"));
    }

    [Test]
    public void Should_RegistrarMultiplasFases_Independentemente()
    {
        var p = new ProgressaoFases();
        p.RegistrarVitoria("natural_1");
        p.RegistrarVitoria("natural_2");
        Assert.IsTrue(p.FaseVencida("natural_1"));
        Assert.IsTrue(p.FaseVencida("natural_2"));
        Assert.IsFalse(p.FaseVencida("natural_3"));
    }

    // ── FaseDesbloqueada (regra de progressão) ────────────────────────────

    [Test]
    public void Should_FaseDesbloqueada_QuandoSemPreRequisito()
    {
        var p = new ProgressaoFases();
        Assert.IsTrue(p.FaseDesbloqueada(""));
        Assert.IsTrue(p.FaseDesbloqueada(null));
    }

    [Test]
    public void Should_FaseBloqueada_QuandoPreRequisitoNaoVencido()
    {
        var p = new ProgressaoFases();
        Assert.IsFalse(p.FaseDesbloqueada("natural_1"));
    }

    [Test]
    public void Should_FaseDesbloqueada_QuandoPreRequisitoVencido()
    {
        var p = new ProgressaoFases();
        p.RegistrarVitoria("natural_1");
        Assert.IsTrue(p.FaseDesbloqueada("natural_1"));
    }

    [Test]
    public void Should_FaseBloqueada_QuandoPreRequisitoNaoEhAFaseAnterior()
    {
        var p = new ProgressaoFases();
        p.RegistrarVitoria("natural_1");
        Assert.IsFalse(p.FaseDesbloqueada("natural_2"));
    }

    // ── Normalizar ────────────────────────────────────────────────────────

    [Test]
    public void Should_CriarListaVencidas_QuandoNula_AoNormalizar()
    {
        var p = new ProgressaoFases();
        p.fasesVencidas = null;
        p.Normalizar();
        Assert.IsNotNull(p.fasesVencidas);
    }

    [Test]
    public void Should_ManterListaExistente_AoNormalizar()
    {
        var p = new ProgressaoFases();
        p.RegistrarVitoria("natural_1");
        p.Normalizar();
        Assert.IsTrue(p.FaseVencida("natural_1"));
    }
}
