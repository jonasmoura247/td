using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Testes de integração dos sistemas de mobs:
/// - ResistenciaDano reduz dano físico e bloqueia DoT por imunidade
/// - RegeneracaoVida restaura vida ao longo do tempo
/// - CurarVida respeita o máximo de vida
/// - Mobs rápidos têm atributos corretos
/// - Mobs tankões têm resistência configurada
/// - MobZumbie é imune a veneno e regenera
/// - MobEsqueleto é imune a veneno
/// - MobEspantalho tem resistência parcial a veneno
/// </summary>
public class MobSistemasIntegrationTests
{
    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    private T CriarMob<T>() where T : MobController
    {
        var go = new GameObject(typeof(T).Name);
        go.AddComponent<Rigidbody2D>();
        var mov = go.AddComponent<MovimentoUnidade>();
        return go.AddComponent<T>();
    }

    // ── CurarVida ─────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator CurarVida_AumentaVida_SemUltrapassarMaximo()
    {
        var mob = CriarMob<MobRato>();
        yield return null; // Start()

        mob.ReceberDanoSemProcs(20);
        int vidaAntes = mob.GetVidaAtual();

        mob.CurarVida(999);

        Assert.AreEqual(mob.vidaMaxima, mob.GetVidaAtual(), "CurarVida não pode ultrapassar vidaMaxima");
        Assert.Greater(mob.GetVidaAtual(), vidaAntes, "CurarVida deve aumentar a vida");
    }

    [UnityTest]
    public IEnumerator CurarVida_NaoFazNada_QuandoMorto()
    {
        var mob = CriarMob<MobFormiga>();
        yield return null;

        mob.ReceberDanoSemProcs(9999);
        // Unidade morta — CurarVida não deve reanimá-la
        mob.CurarVida(100);
        Assert.IsFalse(mob.EstaVivo, "Unidade morta não ressuscita com CurarVida");
    }

    // ── ResistenciaDano ───────────────────────────────────────────────────

    [Test]
    public void ResistenciaDano_ReducaoDanoFisico_Correta()
    {
        var go = new GameObject("Teste");
        var res = go.AddComponent<ResistenciaDano>();
        res.resistenciaFisica = 0.40f;

        int resultado = res.CalcularDanoFinal(10, TipoDanoCombate.Fisico);
        Assert.AreEqual(6, resultado, "40% resistência física: 10 * 0.6 = 6");
    }

    [Test]
    public void ResistenciaDano_ImunidadeVeneno_RetornaZero()
    {
        var go = new GameObject("Teste");
        var res = go.AddComponent<ResistenciaDano>();
        res.resistenciaVeneno = 1f;

        int resultado = res.CalcularDanoFinal(50, TipoDanoCombate.Veneno);
        Assert.AreEqual(0, resultado);
        Assert.IsTrue(res.EhImuneVeneno);
    }

    [Test]
    public void ResistenciaDano_ResistenciaParcialVeneno_Espantalho()
    {
        var go = new GameObject("Teste");
        var res = go.AddComponent<ResistenciaDano>();
        res.resistenciaVeneno = 0.80f;

        int resultado = res.CalcularDanoFinal(10, TipoDanoCombate.Veneno);
        Assert.AreEqual(2, resultado, "80% resistência veneno: 10 * 0.2 = 2");
        Assert.IsFalse(res.EhImuneVeneno, "80% não é imunidade total");
    }

    // ── Atributos dos mobs rápidos ────────────────────────────────────────

    [UnityTest]
    public IEnumerator MobRato_Atributos_Corretos()
    {
        var mob = CriarMob<MobRato>();
        yield return null;

        Assert.AreEqual(35, mob.vidaMaxima);
        Assert.AreEqual(6, mob.danoBase);
        Assert.AreEqual(14f, mob.GetComponent<MovimentoUnidade>().velocidade);
    }

    [UnityTest]
    public IEnumerator MobLobo_Atributos_Corretos()
    {
        var mob = CriarMob<MobLobo>();
        yield return null;

        Assert.AreEqual(60, mob.vidaMaxima);
        Assert.AreEqual(12f, mob.GetComponent<MovimentoUnidade>().velocidade);
    }

    [UnityTest]
    public IEnumerator MobTigre_Atributos_Corretos()
    {
        var mob = CriarMob<MobTigre>();
        yield return null;

        Assert.AreEqual(80, mob.vidaMaxima);
        Assert.AreEqual(11f, mob.GetComponent<MovimentoUnidade>().velocidade);
    }

    // ── Tankões com resistência ───────────────────────────────────────────

    [UnityTest]
    public IEnumerator MobGolem_TemResistenciaFisica40Porcento()
    {
        var mob = CriarMob<MobGolemPedra>();
        yield return null;

        var res = mob.GetComponent<ResistenciaDano>();
        Assert.IsNotNull(res, "Golem deve ter ResistenciaDano");
        Assert.AreEqual(500, mob.vidaMaxima);
        Assert.AreEqual(0.40f, res.resistenciaFisica, 0.001f);
    }

    [UnityTest]
    public IEnumerator MobOgre_TemResistenciaFisica20Porcento()
    {
        var mob = CriarMob<MobOgre>();
        yield return null;

        var res = mob.GetComponent<ResistenciaDano>();
        Assert.IsNotNull(res);
        Assert.AreEqual(0.20f, res.resistenciaFisica, 0.001f);
    }

    // ── Mortos-vivos ──────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator MobZumbie_EhImuneVeneno_ETemRegeneracao()
    {
        var mob = CriarMob<MobZumbie>();
        yield return null;

        var res = mob.GetComponent<ResistenciaDano>();
        Assert.IsNotNull(res);
        Assert.IsTrue(res.EhImuneVeneno, "Zumbie deve ser imune a veneno");

        var regen = mob.GetComponent<RegeneracaoVida>();
        Assert.IsNotNull(regen, "Zumbie deve ter RegeneracaoVida");
        Assert.AreEqual(5f, regen.vidaPorSegundo, 0.001f);
    }

    [UnityTest]
    public IEnumerator MobEsqueleto_EhImuneVeneno()
    {
        var mob = CriarMob<MobEsqueleto>();
        yield return null;

        var res = mob.GetComponent<ResistenciaDano>();
        Assert.IsNotNull(res);
        Assert.IsTrue(res.EhImuneVeneno);
        Assert.IsNull(mob.GetComponent<RegeneracaoVida>(), "Esqueleto não regenera");
    }

    [UnityTest]
    public IEnumerator MobEspantalho_TemResistenciaVeneno80Porcento()
    {
        var mob = CriarMob<MobEspantalho>();
        yield return null;

        var res = mob.GetComponent<ResistenciaDano>();
        Assert.IsNotNull(res);
        Assert.AreEqual(0.80f, res.resistenciaVeneno, 0.001f);
        Assert.IsFalse(res.EhImuneVeneno, "80% não é imunidade total");
    }

    // ── Regeneração de vida em runtime ────────────────────────────────────

    [UnityTest]
    public IEnumerator RegeneracaoVida_RestaurasVida_AoLongoDotempo()
    {
        var mob = CriarMob<MobZumbie>();
        yield return null; // Start()

        // Aplica dano para reduzir a vida
        mob.ReceberDanoSemProcs(50);
        int vidaAposLesao = mob.GetVidaAtual();
        Assert.Less(vidaAposLesao, mob.vidaMaxima);

        // Espera regeneração agir (5 vida/s × 1s = 5 de cura mínima)
        yield return new WaitForSeconds(1.5f);

        Assert.Greater(mob.GetVidaAtual(), vidaAposLesao,
            "Vida deve aumentar após aguardar regeneração");
    }

    // ── Todos mobs têm facção Inimigo ─────────────────────────────────────

    [UnityTest]
    public IEnumerator TodosMobs_TemFaccaoInimigo()
    {
        var rato = CriarMob<MobRato>();
        var golem = CriarMob<MobGolemPedra>();
        var zumbie = CriarMob<MobZumbie>();

        yield return null;

        Assert.AreEqual(Faccao.Inimigo, rato.faccao);
        Assert.AreEqual(Faccao.Inimigo, golem.faccao);
        Assert.AreEqual(Faccao.Inimigo, zumbie.faccao);
    }
}
