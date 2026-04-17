using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Protege o sistema de modificadores de talentos aplicados às unidades:
/// acumulação de flat/percentual, clamp a zero, reset e aplicação de vida máxima.
/// </summary>
public class UnidadeModificadoresTests
{
    private UnidadeModificadores CriarModificadores()
    {
        var go = new GameObject("TestMods");
        return go.AddComponent<UnidadeModificadores>();
    }

    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── SEM MODIFICADORES ──────────────────────────────────────────────────

    [Test]
    public void Should_NotModifyDamage_When_NoEffectsAccumulated()
    {
        var mods = CriarModificadores();
        Assert.AreEqual(42, mods.ModificarDanoFinal(42));
    }

    [Test]
    public void Should_NotModifyDamage_AfterNull_EffectAccumulated()
    {
        var mods = CriarModificadores();
        mods.AcumularEfeito(null);
        Assert.AreEqual(10, mods.ModificarDanoFinal(10));
    }

    // ── DANO FLAT ──────────────────────────────────────────────────────────

    [Test]
    public void Should_AddFlatDamage_Correctly()
    {
        var mods = CriarModificadores();
        mods.AcumularEfeito(new TalentEffectEntry
            { stat = StatTalento.Dano, valorFlat = 10, valorPercentual = 0 });

        Assert.AreEqual(30, mods.ModificarDanoFinal(20));
    }

    [Test]
    public void Should_StackMultipleFlatDamageBonuses()
    {
        var mods = CriarModificadores();
        mods.AcumularEfeito(new TalentEffectEntry
            { stat = StatTalento.Dano, valorFlat = 5, valorPercentual = 0 });
        mods.AcumularEfeito(new TalentEffectEntry
            { stat = StatTalento.Dano, valorFlat = 5, valorPercentual = 0 });

        Assert.AreEqual(20, mods.ModificarDanoFinal(10));
    }

    // ── DANO PERCENTUAL ────────────────────────────────────────────────────

    [Test]
    public void Should_ApplyPercentualDamageBonus_Correctly()
    {
        var mods = CriarModificadores();
        mods.AcumularEfeito(new TalentEffectEntry
            { stat = StatTalento.Dano, valorFlat = 0, valorPercentual = 50f }); // +50%

        Assert.AreEqual(150, mods.ModificarDanoFinal(100));
    }

    [Test]
    public void Should_ApplyCombinedFlatAndPercentual_InCorrectOrder()
    {
        // Fórmula: base * (1 + percentual/100) + flat
        var mods = CriarModificadores();
        mods.AcumularEfeito(new TalentEffectEntry
            { stat = StatTalento.Dano, valorFlat = 10, valorPercentual = 100f }); // base*2 +10

        // 20 * (1 + 1.0) + 10 = 40 + 10 = 50
        Assert.AreEqual(50, mods.ModificarDanoFinal(20));
    }

    // ── CLAMP ──────────────────────────────────────────────────────────────

    [Test]
    public void Should_ClampDamageToZero_When_ModsReduceBelowZero()
    {
        var mods = CriarModificadores();
        mods.AcumularEfeito(new TalentEffectEntry
            { stat = StatTalento.Dano, valorFlat = -9999, valorPercentual = 0 });

        Assert.AreEqual(0, mods.ModificarDanoFinal(5),
            "Dano final nunca pode ser negativo");
    }

    // ── LIMPAR ─────────────────────────────────────────────────────────────

    [Test]
    public void Should_ResetAllMods_When_LimparCalled()
    {
        var mods = CriarModificadores();
        mods.AcumularEfeito(new TalentEffectEntry
            { stat = StatTalento.Dano, valorFlat = 100, valorPercentual = 200f });
        mods.Limpar();

        Assert.AreEqual(10, mods.ModificarDanoFinal(10),
            "Após Limpar(), modificadores devem ser zerados");
    }

    // ── VIDA MÁXIMA ────────────────────────────────────────────────────────

    [Test]
    public void Should_NotModifyMaxLife_When_NoLifeEffectAccumulated()
    {
        var go = new GameObject("TestUnit");
        var u = go.AddComponent<UnidadeController>();
        u.vidaMaxima = 100;
        var mods = go.AddComponent<UnidadeModificadores>();

        mods.AplicarVidaMaxima(u);

        Assert.AreEqual(100, u.vidaMaxima);
    }

    [Test]
    public void Should_ApplyFlatMaxLifeBonus_Correctly()
    {
        var go = new GameObject("TestUnit");
        var u = go.AddComponent<UnidadeController>();
        u.vidaMaxima = 100;
        var mods = go.AddComponent<UnidadeModificadores>();
        mods.AcumularEfeito(new TalentEffectEntry
            { stat = StatTalento.VidaMaxima, valorFlat = 50, valorPercentual = 0 });

        mods.AplicarVidaMaxima(u);

        Assert.AreEqual(150, u.vidaMaxima);
    }

    [Test]
    public void Should_ApplyPercentualMaxLifeBonus_Correctly()
    {
        var go = new GameObject("TestUnit");
        var u = go.AddComponent<UnidadeController>();
        u.vidaMaxima = 200;
        var mods = go.AddComponent<UnidadeModificadores>();
        mods.AcumularEfeito(new TalentEffectEntry
            { stat = StatTalento.VidaMaxima, valorFlat = 0, valorPercentual = 50f }); // +50%

        mods.AplicarVidaMaxima(u);

        Assert.AreEqual(300, u.vidaMaxima);
    }

    [Test]
    public void Should_NeverReduceMaxLifeBelow1()
    {
        var go = new GameObject("TestUnit");
        var u = go.AddComponent<UnidadeController>();
        u.vidaMaxima = 10;
        var mods = go.AddComponent<UnidadeModificadores>();
        mods.AcumularEfeito(new TalentEffectEntry
            { stat = StatTalento.VidaMaxima, valorFlat = -9999, valorPercentual = 0 });

        mods.AplicarVidaMaxima(u);

        Assert.GreaterOrEqual(u.vidaMaxima, 1,
            "Vida máxima nunca pode ser menor que 1");
    }
}
