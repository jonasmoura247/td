using NUnit.Framework;

/// <summary>
/// Protege FaseIdentificacao:
/// - FaseAtualId é gravado e lido corretamente entre cenas
/// - Limpar() zera o ID (evita que fase errada seja registrada)
/// </summary>
public class FaseIdentificacaoTests
{
    [SetUp]
    public void SetUp() => FaseIdentificacao.Limpar();

    [TearDown]
    public void TearDown() => FaseIdentificacao.Limpar();

    [Test]
    public void Should_IniciarNulo_AposLimpar()
    {
        Assert.IsNull(FaseIdentificacao.FaseAtualId);
    }

    [Test]
    public void Should_GravarFaseAtualId_Corretamente()
    {
        FaseIdentificacao.FaseAtualId = "natural_1";
        Assert.AreEqual("natural_1", FaseIdentificacao.FaseAtualId);
    }

    [Test]
    public void Should_SobrescreverFaseAtualId_QuandoDefinidoNovamente()
    {
        FaseIdentificacao.FaseAtualId = "natural_1";
        FaseIdentificacao.FaseAtualId = "natural_5";
        Assert.AreEqual("natural_5", FaseIdentificacao.FaseAtualId);
    }

    [Test]
    public void Should_ZerarFaseAtualId_AposLimpar()
    {
        FaseIdentificacao.FaseAtualId = "natural_3";
        FaseIdentificacao.Limpar();
        Assert.IsNull(FaseIdentificacao.FaseAtualId);
    }

    [Test]
    public void Should_AceitarFaseIdVazia()
    {
        FaseIdentificacao.FaseAtualId = "";
        Assert.AreEqual("", FaseIdentificacao.FaseAtualId);
    }
}
