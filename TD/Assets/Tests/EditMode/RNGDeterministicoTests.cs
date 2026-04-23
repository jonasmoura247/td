using NUnit.Framework;

/// <summary>
/// Garante que RNGDeterministico é verdadeiramente determinístico:
/// mesma seed → mesma sequência, mesmo após restart.
/// </summary>
public class RNGDeterministicoTests
{
    // ── Determinismo ──────────────────────────────────────────────────────

    [Test]
    public void Should_ProduceSameSequence_WithSameSeed()
    {
        var rng1 = new RNGDeterministico(42);
        var rng2 = new RNGDeterministico(42);

        for (int i = 0; i < 20; i++)
            Assert.AreEqual(rng1.Next(0, 100), rng2.Next(0, 100));
    }

    [Test]
    public void Should_ProduceDifferentSequence_WithDifferentSeeds()
    {
        var rng1 = new RNGDeterministico(1);
        var rng2 = new RNGDeterministico(9999);

        bool algumDiferente = false;
        for (int i = 0; i < 10; i++)
        {
            if (rng1.Next(0, 10000) != rng2.Next(0, 10000))
            {
                algumDiferente = true;
                break;
            }
        }
        Assert.IsTrue(algumDiferente);
    }

    [Test]
    public void Should_ProduceSameFloatSequence_WithSameSeed()
    {
        var rng1 = new RNGDeterministico(7);
        var rng2 = new RNGDeterministico(7);

        for (int i = 0; i < 10; i++)
            Assert.AreEqual(rng1.NextFloat(), rng2.NextFloat());
    }

    // ── Limites ───────────────────────────────────────────────────────────

    [Test]
    public void Should_Next_AlwaysBeWithinRange()
    {
        var rng = new RNGDeterministico(123);
        for (int i = 0; i < 100; i++)
        {
            int val = rng.Next(5, 10);
            Assert.IsTrue(val >= 5 && val < 10, $"Valor {val} fora do intervalo [5,10)");
        }
    }

    [Test]
    public void Should_NextFloat_AlwaysBeInZeroOneRange()
    {
        var rng = new RNGDeterministico(321);
        for (int i = 0; i < 100; i++)
        {
            float val = rng.NextFloat();
            Assert.IsTrue(val >= 0f && val < 1f, $"Float {val} fora de [0,1)");
        }
    }

    [Test]
    public void Should_Next_ReturnMin_WhenMinEqualsMax()
    {
        var rng = new RNGDeterministico(0);
        Assert.AreEqual(5, rng.Next(5, 5));
    }

    // ── GerarSeed ─────────────────────────────────────────────────────────

    [Test]
    public void Should_GerarSeed_BeDeterministic_ForSameInputs()
    {
        int s1 = RNGDeterministico.GerarSeed("uuid-abc", 1, 2, 3);
        int s2 = RNGDeterministico.GerarSeed("uuid-abc", 1, 2, 3);
        Assert.AreEqual(s1, s2);
    }

    [Test]
    public void Should_GerarSeed_BeDifferent_WhenContadorMuda()
    {
        int s1 = RNGDeterministico.GerarSeed("uuid-abc", 1, 2, 0);
        int s2 = RNGDeterministico.GerarSeed("uuid-abc", 1, 2, 1);
        Assert.AreNotEqual(s1, s2);
    }

    [Test]
    public void Should_GerarSeed_BeDifferent_WhenFaseIdMuda()
    {
        int s1 = RNGDeterministico.GerarSeed("uuid-abc", 1, 1, 0);
        int s2 = RNGDeterministico.GerarSeed("uuid-abc", 1, 2, 0);
        Assert.AreNotEqual(s1, s2);
    }

    [Test]
    public void Should_GerarSeed_BeDifferent_WhenUuidMuda()
    {
        int s1 = RNGDeterministico.GerarSeed("uuid-A", 1, 1, 0);
        int s2 = RNGDeterministico.GerarSeed("uuid-B", 1, 1, 0);
        Assert.AreNotEqual(s1, s2);
    }

    [Test]
    public void Should_GerarSeed_NotCrash_WhenUuidIsNull()
    {
        Assert.DoesNotThrow(() => RNGDeterministico.GerarSeed(null, 1, 1, 0));
    }
}
