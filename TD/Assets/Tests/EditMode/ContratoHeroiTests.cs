using NUnit.Framework;

/// <summary>
/// Garante as regras de negócio dos contratos de heróis:
/// - Penúltima fase detectada corretamente
/// - Expiração detectada corretamente
/// - Custo de renovação cresce com cada renovação
/// </summary>
public class ContratoHeroiTests
{
    private ContratoHeroi Criar(int fasesRestantes = 3, int custoBase = 100, int vezesRenovado = 0)
    {
        return new ContratoHeroi
        {
            idHeroi = "heroi_pirata",
            fasesRestantes = fasesRestantes,
            fasesTotais = fasesRestantes,
            custoRenovacaoBase = custoBase,
            vezesRenovado = vezesRenovado
        };
    }

    // ── NaPenultimaFase ────────────────────────────────────────────────────

    [Test]
    public void Should_NaPenultimaFase_QuandoFasesRestantesEhUm()
    {
        var contrato = Criar(fasesRestantes: 1);
        Assert.IsTrue(contrato.NaPenultimaFase);
    }

    [Test]
    public void Should_NaoPenultimaFase_QuandoFasesRestantesMaiorQueUm()
    {
        var contrato = Criar(fasesRestantes: 2);
        Assert.IsFalse(contrato.NaPenultimaFase);
    }

    [Test]
    public void Should_NaoPenultimaFase_QuandoFasesRestantesEhZero()
    {
        var contrato = Criar(fasesRestantes: 0);
        Assert.IsFalse(contrato.NaPenultimaFase);
    }

    // ── Expirado ──────────────────────────────────────────────────────────

    [Test]
    public void Should_Expirado_QuandoFasesRestantesEhZero()
    {
        var contrato = Criar(fasesRestantes: 0);
        Assert.IsTrue(contrato.Expirado);
    }

    [Test]
    public void Should_Expirado_QuandoFasesRestantesNegativo()
    {
        var contrato = Criar(fasesRestantes: -1);
        Assert.IsTrue(contrato.Expirado);
    }

    [Test]
    public void Should_NaoExpirado_QuandoFasesRestantesPositivo()
    {
        var contrato = Criar(fasesRestantes: 3);
        Assert.IsFalse(contrato.Expirado);
    }

    // ── CalcularCustoRenovacao ────────────────────────────────────────────

    [Test]
    public void Should_CustoRenovacao_SerBase_NaPrimeiraVez()
    {
        var contrato = Criar(custoBase: 100, vezesRenovado: 0);
        Assert.AreEqual(100, contrato.CalcularCustoRenovacao());
    }

    [Test]
    public void Should_CustoRenovacao_DobroNaSegundaRenovacao()
    {
        var contrato = Criar(custoBase: 100, vezesRenovado: 1);
        Assert.AreEqual(200, contrato.CalcularCustoRenovacao());
    }

    [Test]
    public void Should_CustoRenovacao_CrescerLinearmente()
    {
        var contrato = Criar(custoBase: 50, vezesRenovado: 0);
        int custo0 = contrato.CalcularCustoRenovacao(); // 50 * (1+0) = 50

        contrato.vezesRenovado = 1;
        int custo1 = contrato.CalcularCustoRenovacao(); // 50 * (1+1) = 100

        contrato.vezesRenovado = 2;
        int custo2 = contrato.CalcularCustoRenovacao(); // 50 * (1+2) = 150

        Assert.AreEqual(50, custo0);
        Assert.AreEqual(100, custo1);
        Assert.AreEqual(150, custo2);
        Assert.Greater(custo1, custo0);
        Assert.Greater(custo2, custo1);
    }

    // ── Estado inicial ────────────────────────────────────────────────────

    [Test]
    public void Should_AvisoNaoEnviado_NoContratoCriado()
    {
        var contrato = Criar(fasesRestantes: 5);
        Assert.IsFalse(contrato.avisoPenultimaFaseEnviado);
    }

    [Test]
    public void Should_NaoExpirado_NoContratoCriado_ComFasesPositivas()
    {
        var contrato = Criar(fasesRestantes: 3);
        Assert.IsFalse(contrato.Expirado);
        Assert.IsFalse(contrato.NaPenultimaFase);
    }
}
