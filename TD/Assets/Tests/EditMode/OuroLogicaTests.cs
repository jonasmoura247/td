using NUnit.Framework;

/// <summary>
/// Testa todas as regras de negócio de ouro em isolamento total.
/// Sem MonoBehaviour, sem cena, sem singleton.
/// </summary>
public class OuroLogicaTests
{
    // ── ESTADO INICIAL ─────────────────────────────────────────────────────

    [Test]
    public void Should_StartWithZeroOuro_When_NoneProvided()
    {
        var ouro = new OuroLogica();
        Assert.AreEqual(0, ouro.GetOuro());
    }

    [Test]
    public void Should_StartWithProvidedAmount_When_InitialValueGiven()
    {
        var ouro = new OuroLogica(150);
        Assert.AreEqual(150, ouro.GetOuro());
    }

    [Test]
    public void Should_ClampInitialValueToZero_When_NegativeProvided()
    {
        var ouro = new OuroLogica(-100);
        Assert.AreEqual(0, ouro.GetOuro());
    }

    // ── ADICIONAR OURO ─────────────────────────────────────────────────────

    [Test]
    public void Should_IncreaseOuro_When_ValidAmountAdded()
    {
        var ouro = new OuroLogica(50);
        ouro.AdicionarOuro(30);
        Assert.AreEqual(80, ouro.GetOuro());
    }

    [Test]
    public void Should_ReturnAmountAdded_When_AdicionarOuroCalled()
    {
        var ouro = new OuroLogica();
        int retorno = ouro.AdicionarOuro(75);
        Assert.AreEqual(75, retorno);
    }

    [Test]
    public void Should_NotAddOuro_When_AmountIsZero()
    {
        var ouro = new OuroLogica(50);
        int retorno = ouro.AdicionarOuro(0);
        Assert.AreEqual(50, ouro.GetOuro());
        Assert.AreEqual(0, retorno);
    }

    [Test]
    public void Should_NotAddOuro_When_AmountIsNegative()
    {
        var ouro = new OuroLogica(50);
        int retorno = ouro.AdicionarOuro(-20);
        Assert.AreEqual(50, ouro.GetOuro());
        Assert.AreEqual(0, retorno);
    }

    [Test]
    public void Should_FireOnOuroAlterado_When_OuroAdded()
    {
        var ouro = new OuroLogica();
        int valorRecebido = -1;
        ouro.OnOuroAlterado += v => valorRecebido = v;

        ouro.AdicionarOuro(100);

        Assert.AreEqual(100, valorRecebido);
    }

    [Test]
    public void Should_NotFireOnOuroAlterado_When_InvalidAmountAdded()
    {
        var ouro = new OuroLogica(50);
        bool eventoDisparado = false;
        ouro.OnOuroAlterado += _ => eventoDisparado = true;

        ouro.AdicionarOuro(0);
        ouro.AdicionarOuro(-10);

        Assert.IsFalse(eventoDisparado);
    }

    // ── GASTAR OURO ────────────────────────────────────────────────────────

    [Test]
    public void Should_DecreasOuro_When_ValidAmountSpent()
    {
        var ouro = new OuroLogica(100);
        bool resultado = ouro.GastarOuro(40);
        Assert.IsTrue(resultado);
        Assert.AreEqual(60, ouro.GetOuro());
    }

    [Test]
    public void Should_SpendExactBalance_When_AmountEqualsBalance()
    {
        var ouro = new OuroLogica(50);
        bool resultado = ouro.GastarOuro(50);
        Assert.IsTrue(resultado);
        Assert.AreEqual(0, ouro.GetOuro());
    }

    [Test]
    public void Should_ReturnFalse_When_InsufficientFunds()
    {
        var ouro = new OuroLogica(30);
        bool resultado = ouro.GastarOuro(50);
        Assert.IsFalse(resultado);
        Assert.AreEqual(30, ouro.GetOuro(), "Ouro não deve ser alterado em caso de saldo insuficiente");
    }

    [Test]
    public void Should_NeverHaveNegativeOuro_After_FailedSpend()
    {
        var ouro = new OuroLogica(10);
        ouro.GastarOuro(999);
        Assert.GreaterOrEqual(ouro.GetOuro(), 0);
    }

    [Test]
    public void Should_ReturnFalse_When_SpendAmountIsZeroOrNegative()
    {
        var ouro = new OuroLogica(100);
        Assert.IsFalse(ouro.GastarOuro(0));
        Assert.IsFalse(ouro.GastarOuro(-10));
        Assert.AreEqual(100, ouro.GetOuro());
    }

    [Test]
    public void Should_FireOnOuroAlterado_When_OuroSpent()
    {
        var ouro = new OuroLogica(100);
        int valorRecebido = -1;
        ouro.OnOuroAlterado += v => valorRecebido = v;

        ouro.GastarOuro(40);

        Assert.AreEqual(60, valorRecebido);
    }

    [Test]
    public void Should_NotFireOnOuroAlterado_When_SpendFails()
    {
        var ouro = new OuroLogica(10);
        bool eventoDisparado = false;
        ouro.OnOuroAlterado += _ => eventoDisparado = true;

        ouro.GastarOuro(999);

        Assert.IsFalse(eventoDisparado);
    }

    // ── SET OURO ───────────────────────────────────────────────────────────

    [Test]
    public void Should_SetOuroToExactValue()
    {
        var ouro = new OuroLogica(50);
        ouro.SetOuro(200);
        Assert.AreEqual(200, ouro.GetOuro());
    }

    [Test]
    public void Should_ClampSetOuroToZero_When_NegativeValue()
    {
        var ouro = new OuroLogica(50);
        ouro.SetOuro(-100);
        Assert.AreEqual(0, ouro.GetOuro());
    }

    [Test]
    public void Should_FireOnOuroAlterado_When_SetOuroCalled()
    {
        var ouro = new OuroLogica();
        int valorRecebido = -1;
        ouro.OnOuroAlterado += v => valorRecebido = v;

        ouro.SetOuro(300);

        Assert.AreEqual(300, valorRecebido);
    }
}
