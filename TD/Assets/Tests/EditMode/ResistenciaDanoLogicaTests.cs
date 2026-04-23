using NUnit.Framework;

/// <summary>
/// Protege a lógica pura de cálculo de resistência a dano:
/// - Dano com resistência zero = dano original
/// - Imunidade total (1f) = dano zero
/// - Resistência parcial reduz corretamente
/// - Dano mínimo 1 quando resistência < 1
/// - Entradas inválidas não crasham
/// </summary>
public class ResistenciaDanoLogicaTests
{
    [Test]
    public void SemResistencia_RetornaDanoOriginal()
    {
        Assert.AreEqual(100, ResistenciaDanoLogica.AplicarResistencia(100, 0f));
    }

    [Test]
    public void ImunidadeTotal_RetornaZero()
    {
        Assert.AreEqual(0, ResistenciaDanoLogica.AplicarResistencia(100, 1f));
    }

    [Test]
    public void Resistencia50Porcento_RetornaMetade()
    {
        int resultado = ResistenciaDanoLogica.AplicarResistencia(100, 0.5f);
        Assert.AreEqual(50, resultado);
    }

    [Test]
    public void Resistencia40Porcento_GolemPedra_ReduzCorretamente()
    {
        int resultado = ResistenciaDanoLogica.AplicarResistencia(10, 0.40f);
        Assert.AreEqual(6, resultado, "10 * (1 - 0.4) = 6");
    }

    [Test]
    public void Resistencia20Porcento_Ogre_ReduzCorretamente()
    {
        int resultado = ResistenciaDanoLogica.AplicarResistencia(10, 0.20f);
        Assert.AreEqual(8, resultado, "10 * (1 - 0.2) = 8");
    }

    [Test]
    public void Resistencia80Porcento_VenenoEspantalho_ReduzCorretamente()
    {
        int resultado = ResistenciaDanoLogica.AplicarResistencia(10, 0.80f);
        Assert.AreEqual(2, resultado, "10 * (1 - 0.8) = 2");
    }

    [Test]
    public void DanoOriginalZero_RetornaZero()
    {
        Assert.AreEqual(0, ResistenciaDanoLogica.AplicarResistencia(0, 0.5f));
    }

    [Test]
    public void DanoNegativo_RetornaZero()
    {
        Assert.AreEqual(0, ResistenciaDanoLogica.AplicarResistencia(-10, 0.5f));
    }

    [Test]
    public void ResistenciaAcimaDeUm_TratadoComoImunidade()
    {
        Assert.AreEqual(0, ResistenciaDanoLogica.AplicarResistencia(100, 1.5f),
            "Resistência > 1 é clampada para 1 (imunidade)");
    }

    [Test]
    public void ResistenciaAbaixoDeZero_TratadoComoSemResistencia()
    {
        Assert.AreEqual(100, ResistenciaDanoLogica.AplicarResistencia(100, -0.5f));
    }

    [Test]
    public void DanoMinimo1_QuandoResistenciaParcialEDanoGrande()
    {
        int resultado = ResistenciaDanoLogica.AplicarResistencia(1, 0.99f);
        Assert.AreEqual(1, resultado, "Dano mínimo deve ser 1 quando não há imunidade total");
    }
}
