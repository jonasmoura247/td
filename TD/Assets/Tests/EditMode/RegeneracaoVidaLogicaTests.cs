using NUnit.Framework;

/// <summary>
/// Protege a lógica pura de regeneração de vida:
/// - CalcularCura produz valor correto baseado em vidaPorSegundo e deltaTime
/// - AplicarCura não ultrapassa vidaMaxima
/// - Entradas inválidas retornam zero ou valor original
/// </summary>
public class RegeneracaoVidaLogicaTests
{
    // ── CalcularCura ─────────────────────────────────────────────────────────

    [Test]
    public void CalcularCura_Retorna5_QuandoRegenEhPorSegundo()
    {
        int cura = RegeneracaoVidaLogica.CalcularCura(5f, 1f);
        Assert.AreEqual(5, cura);
    }

    [Test]
    public void CalcularCura_RetornaMinimo1_QuandoRegenPequenaEDeltaGrande()
    {
        // 0.1 vida/s * 1s = 0.1, arredondado = 0, forçado para 1
        int cura = RegeneracaoVidaLogica.CalcularCura(0.1f, 1f);
        Assert.AreEqual(1, cura);
    }

    [Test]
    public void CalcularCura_RetornaZero_QuandoRegenZero()
    {
        Assert.AreEqual(0, RegeneracaoVidaLogica.CalcularCura(0f, 1f));
    }

    [Test]
    public void CalcularCura_RetornaZero_QuandoDeltaZero()
    {
        Assert.AreEqual(0, RegeneracaoVidaLogica.CalcularCura(5f, 0f));
    }

    [Test]
    public void CalcularCura_RetornaZero_QuandoDeltaNegativo()
    {
        Assert.AreEqual(0, RegeneracaoVidaLogica.CalcularCura(5f, -1f));
    }

    // ── AplicarCura ──────────────────────────────────────────────────────────

    [Test]
    public void AplicarCura_NaoUltrapassaVidaMaxima()
    {
        int resultado = RegeneracaoVidaLogica.AplicarCura(90, 20, 100);
        Assert.AreEqual(100, resultado, "Vida não pode ultrapassar o máximo");
    }

    [Test]
    public void AplicarCura_SomaCuraCorretamente()
    {
        int resultado = RegeneracaoVidaLogica.AplicarCura(50, 10, 100);
        Assert.AreEqual(60, resultado);
    }

    [Test]
    public void AplicarCura_CuraZero_RetornaVidaAtual()
    {
        int resultado = RegeneracaoVidaLogica.AplicarCura(70, 0, 100);
        Assert.AreEqual(70, resultado);
    }

    [Test]
    public void AplicarCura_CuraNegativa_RetornaVidaAtual()
    {
        int resultado = RegeneracaoVidaLogica.AplicarCura(70, -5, 100);
        Assert.AreEqual(70, resultado);
    }

    [Test]
    public void AplicarCura_VidaJaMaxima_RetornaMaxima()
    {
        int resultado = RegeneracaoVidaLogica.AplicarCura(100, 10, 100);
        Assert.AreEqual(100, resultado);
    }
}
