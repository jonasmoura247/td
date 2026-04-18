using NUnit.Framework;
using UnityEngine;
using System.Reflection;

/// <summary>
/// Protege o intervalo de ataque universal da unidade (vale para unidades e castelo):
/// - Valor padrão é positivo
/// - Valor mínimo de 0.1s é sempre respeitado
/// - Valor configurado no Inspector é retornado corretamente
/// - Unidades diferentes têm intervalos independentes
/// </summary>
public class IntervaloAtaqueUnidadeTests
{
    private static readonly FieldInfo CampoIntervalo = typeof(UnidadeController)
        .GetField("intervaloAtaque", BindingFlags.NonPublic | BindingFlags.Instance);

    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    private UnidadeController CriarUnidade(float intervalo = 1f)
    {
        var go = new GameObject("Unidade");
        var u = go.AddComponent<UnidadeController>();
        u.faccao = Faccao.Inimigo;
        u.SincronizarVidaAposSpawn();
        CampoIntervalo.SetValue(u, intervalo);
        return u;
    }

    // ── VALOR PADRÃO ──────────────────────────────────────────────────────────

    [Test]
    public void Should_HavePositiveInterval_ByDefault()
    {
        var u = CriarUnidade();
        Assert.Greater(u.GetIntervaloAtaque(), 0f,
            "Intervalo de ataque deve ser positivo por padrão");
    }

    // ── VALOR MÍNIMO ──────────────────────────────────────────────────────────

    [Test]
    public void Should_ReturnMinimum_WhenIntervalIsZero()
    {
        var u = CriarUnidade(intervalo: 0f);
        Assert.GreaterOrEqual(u.GetIntervaloAtaque(), 0.1f,
            "Intervalo zero deve retornar o mínimo de 0.1s");
    }

    [Test]
    public void Should_ReturnMinimum_WhenIntervalIsNegative()
    {
        var u = CriarUnidade(intervalo: -5f);
        Assert.GreaterOrEqual(u.GetIntervaloAtaque(), 0.1f,
            "Intervalo negativo deve retornar o mínimo de 0.1s");
    }

    // ── VALOR CONFIGURADO ─────────────────────────────────────────────────────

    [Test]
    public void Should_ReturnConfiguredInterval_WhenSetTo2Seconds()
    {
        var u = CriarUnidade(intervalo: 2f);
        Assert.AreEqual(2f, u.GetIntervaloAtaque(), 0.001f,
            "Deve retornar exatamente o valor configurado no Inspector");
    }

    [Test]
    public void Should_ReturnConfiguredInterval_WhenSetToHalfSecond()
    {
        var u = CriarUnidade(intervalo: 0.5f);
        Assert.AreEqual(0.5f, u.GetIntervaloAtaque(), 0.001f,
            "Intervalo de 0.5s deve ser retornado corretamente");
    }

    // ── INDEPENDÊNCIA ENTRE UNIDADES ──────────────────────────────────────────

    [Test]
    public void Should_HaveIndependentIntervals_ForDifferentUnits()
    {
        var u1 = CriarUnidade(intervalo: 0.5f);
        var u2 = CriarUnidade(intervalo: 3f);

        Assert.AreEqual(0.5f, u1.GetIntervaloAtaque(), 0.001f);
        Assert.AreEqual(3f,   u2.GetIntervaloAtaque(), 0.001f);
        Assert.AreNotEqual(u1.GetIntervaloAtaque(), u2.GetIntervaloAtaque(),
            "Unidades diferentes devem ter intervalos independentes");
    }
}
