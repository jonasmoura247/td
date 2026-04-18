using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using System.Text.RegularExpressions;

/// <summary>
/// Protege as regras base de todos os mobs:
/// - Facção sempre Inimigo
/// - Dano calculado com multiplicador
/// - Multiplicador 1 preserva dano base
/// - Cada mob é independente
/// </summary>
public class MobControllerTests
{
    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    private T CriarMob<T>(int vida = 100, int dano = 10) where T : MobController
    {
        var go = new GameObject(typeof(T).Name);
        var mob = go.AddComponent<T>();
        // Em EditMode o Unity não chama Awake() automaticamente — invocar via reflection
        // para garantir que MobController.Awake() execute (define faccao = Inimigo, etc.)
        typeof(MobController)
            .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(mob, null);
        mob.vidaMaxima = vida;
        mob.danoBase = dano;
        mob.SincronizarVidaAposSpawn();
        return mob;
    }

    private void SetMultiplicador(MobController mob, float valor)
    {
        typeof(MobController)
            .GetField("multiplicadorDano", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(mob, valor);
    }

    // ── FACÇÃO ────────────────────────────────────────────────────────────────

    [Test]
    public void Should_BeInimigo_WhenCreated()
    {
        var mob = CriarMob<MobEspantalho>();
        Assert.AreEqual(Faccao.Inimigo, mob.faccao,
            "Todo mob deve ter facção Inimigo por padrão");
    }

    [Test]
    public void Should_AllMobsBeInimigo()
    {
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobAmazona>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobAranha>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobEsqueleto>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobFormiga>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobGolemPedra>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobHarpia>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobLobo>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobMinotauro>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobOgre>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobOrc>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobRato>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobTigre>().faccao);
        Assert.AreEqual(Faccao.Inimigo, CriarMob<MobZumbie>().faccao);
    }

    // ── VIDA ──────────────────────────────────────────────────────────────────

    [Test]
    public void Should_StartAlive_WithFullHealth()
    {
        var mob = CriarMob<MobLobo>(vida: 80);
        Assert.IsTrue(mob.EstaVivo);
        Assert.AreEqual(80, mob.GetVidaAtual());
    }

    [Test]
    public void Should_Die_WhenDamageExceedsHealth()
    {
        var mob = CriarMob<MobRato>(vida: 30);
        LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));
        mob.ReceberDanoSemProcs(30);
        Assert.IsFalse(mob.EstaVivo);
    }

    // ── DANO ──────────────────────────────────────────────────────────────────

    [Test]
    public void Should_ReturnDanoBase_WhenMultiplicadorIsOne()
    {
        var mob = CriarMob<MobOrc>(dano: 15);
        SetMultiplicador(mob, 1f);
        Assert.AreEqual(15, mob.GetDanoAtaque(),
            "Multiplicador 1 não deve alterar o dano base");
    }

    [Test]
    public void Should_DoubleAttack_WhenMultiplicadorIsTwo()
    {
        var mob = CriarMob<MobMinotauro>(dano: 10);
        SetMultiplicador(mob, 2f);
        Assert.AreEqual(20, mob.GetDanoAtaque(),
            "Multiplicador 2 deve dobrar o dano base");
    }

    [Test]
    public void Should_HaveIndependentDamage_ForDifferentMobs()
    {
        var lobo = CriarMob<MobLobo>(dano: 5);
        var golem = CriarMob<MobGolemPedra>(dano: 20);

        SetMultiplicador(lobo, 1f);
        SetMultiplicador(golem, 1f);

        Assert.AreNotEqual(lobo.GetDanoAtaque(), golem.GetDanoAtaque(),
            "Mobs com danoBase diferentes devem ter danos finais distintos");
    }
}
