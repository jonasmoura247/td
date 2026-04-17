using NUnit.Framework;

/// <summary>
/// Protege a fórmula de progressão de nível/XP. Qualquer mudança na curva
/// de XP quebrará estes testes — isso é intencional.
/// </summary>
public class NivelRacaCalculadoraTests
{
    // ExpTotalNecessariaParaAlcancarNivel(n) = 150*(n-1)^2 + 250*(n-1)
    // Nível 1 = 0 XP
    // Nível 2 = 150*1 + 250 = 400 XP
    // Nível 3 = 150*4 + 500 = 1100 XP
    // Nível 4 = 150*9 + 750 = 2100 XP

    [Test]
    public void Should_ReturnLevel1_When_ExpIsZero()
    {
        Assert.AreEqual(1, NivelRacaCalculadora.CalcularNivelPorExpTotal(0));
    }

    [Test]
    public void Should_ReturnLevel1_When_ExpIsNegative()
    {
        Assert.AreEqual(1, NivelRacaCalculadora.CalcularNivelPorExpTotal(-500));
    }

    [Test]
    public void Should_StayAtLevel1_When_ExpJustBelowLevel2Threshold()
    {
        Assert.AreEqual(1, NivelRacaCalculadora.CalcularNivelPorExpTotal(399));
    }

    [Test]
    public void Should_ReachLevel2_When_ExpIsExactlyAtThreshold()
    {
        int threshold = NivelRacaCalculadora.ExpTotalNecessariaParaAlcancarNivel(2);
        Assert.AreEqual(400, threshold, "Threshold para nível 2 deve ser 400");
        Assert.AreEqual(2, NivelRacaCalculadora.CalcularNivelPorExpTotal(threshold));
    }

    [Test]
    public void Should_ReachLevel3_When_ExpIsExactlyAtThreshold()
    {
        int threshold = NivelRacaCalculadora.ExpTotalNecessariaParaAlcancarNivel(3);
        Assert.AreEqual(1100, threshold, "Threshold para nível 3 deve ser 1100");
        Assert.AreEqual(3, NivelRacaCalculadora.CalcularNivelPorExpTotal(threshold));
    }

    [Test]
    public void Should_NeverReturnLevelBelow1()
    {
        Assert.GreaterOrEqual(NivelRacaCalculadora.CalcularNivelPorExpTotal(0), 1);
        Assert.GreaterOrEqual(NivelRacaCalculadora.CalcularNivelPorExpTotal(-9999), 1);
    }

    [Test]
    public void Should_HaveStrictlyIncreasingThresholds_ForSuccessiveLevels()
    {
        for (int nivel = 2; nivel <= 10; nivel++)
        {
            int atual = NivelRacaCalculadora.ExpTotalNecessariaParaAlcancarNivel(nivel);
            int proximo = NivelRacaCalculadora.ExpTotalNecessariaParaAlcancarNivel(nivel + 1);
            Assert.Less(atual, proximo,
                $"Limiar do nível {nivel} ({atual}) deve ser menor que o do nível {nivel + 1} ({proximo})");
        }
    }

    [Test]
    public void Should_ReturnZeroAccumulatedExp_When_AtExactLevelThreshold()
    {
        int expNivel2 = NivelRacaCalculadora.ExpTotalNecessariaParaAlcancarNivel(2);
        int acumulada = NivelRacaCalculadora.ObterExpAcumuladaNoNivelAtual(expNivel2);
        Assert.AreEqual(0, acumulada, "Ao entrar no nível, XP acumulada dentro do nível deve ser 0");
    }

    [Test]
    public void Should_ReturnCorrectAccumulatedExp_WhenPartiallyIntoLevel()
    {
        int expNivel2 = NivelRacaCalculadora.ExpTotalNecessariaParaAlcancarNivel(2);
        int acumulada = NivelRacaCalculadora.ObterExpAcumuladaNoNivelAtual(expNivel2 + 150);
        Assert.AreEqual(150, acumulada);
    }

    [Test]
    public void Should_KeepProgressBetween0And1_ForAnyExp()
    {
        int[] casos = { 0, 1, 399, 400, 1099, 1100, 5000, 99999 };
        foreach (int exp in casos)
        {
            float progress = NivelRacaCalculadora.CalcularProgressoDentroDoNivel01(exp);
            Assert.GreaterOrEqual(progress, 0f, $"Progresso negativo para exp={exp}");
            Assert.LessOrEqual(progress, 1f, $"Progresso > 1 para exp={exp}");
        }
    }

    [Test]
    public void Should_ReturnProgressZero_When_AtExactLevelEntryThreshold()
    {
        int expNivel2 = NivelRacaCalculadora.ExpTotalNecessariaParaAlcancarNivel(2);
        float progress = NivelRacaCalculadora.CalcularProgressoDentroDoNivel01(expNivel2);
        Assert.AreEqual(0f, progress, 0.001f,
            "Exatamente no limiar do nível, o progresso dentro do nível deve ser 0");
    }

    [Test]
    public void Should_ReturnExpRemaining_GreaterThanZero_AtLevelStart()
    {
        int restante = NivelRacaCalculadora.ObterExpRestanteParaProximoNivel(0);
        Assert.Greater(restante, 0, "Sempre deve haver XP restante para o próximo nível");
    }

    [Test]
    public void Should_DecreaseExpRemaining_AsExpIncreases()
    {
        int resto1 = NivelRacaCalculadora.ObterExpRestanteParaProximoNivel(100);
        int resto2 = NivelRacaCalculadora.ObterExpRestanteParaProximoNivel(200);
        Assert.Less(resto2, resto1, "XP restante deve diminuir conforme XP total aumenta (dentro do mesmo nível)");
    }
}
