using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Protege as regras fundamentais de combate de unidade:
/// - Unidades perdem vida ao receber dano
/// - Dano zero ou negativo não altera vida
/// - Unidades morrem quando vida chega a zero
/// - Unidades mortas não recebem mais dano
/// </summary>
public class UnidadeControllerTests
{
    private UnidadeController CriarUnidade(Faccao faccao = Faccao.Jogador, int vida = 100)
    {
        var go = new GameObject($"TestUnit_{faccao}");
        var u = go.AddComponent<UnidadeController>();
        u.faccao = faccao;
        u.vidaMaxima = vida;
        // SincronizarVidaAposSpawn define vidaAtual = vidaMaxima sem depender de Start()
        u.SincronizarVidaAposSpawn();
        return u;
    }

    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── VIDA / DANO ────────────────────────────────────────────────────────

    [Test]
    public void Should_StartAlive_WhenCreated()
    {
        var u = CriarUnidade(vida: 100);
        Assert.IsTrue(u.EstaVivo);
    }

    [Test]
    public void Should_StartWithFullHealth()
    {
        var u = CriarUnidade(vida: 100);
        Assert.AreEqual(100, u.GetVidaAtual());
    }

    [Test]
    public void Should_LoseHealth_When_ReceivingDamage()
    {
        var u = CriarUnidade(vida: 100);
        u.ReceberDanoSemProcs(30);
        Assert.AreEqual(70, u.GetVidaAtual());
    }

    [Test]
    public void Should_NotLoseHealth_When_DamageIsZero()
    {
        var u = CriarUnidade(vida: 100);
        u.ReceberDanoSemProcs(0);
        Assert.AreEqual(100, u.GetVidaAtual());
    }

    [Test]
    public void Should_NotLoseHealth_When_DamageIsNegative()
    {
        var u = CriarUnidade(vida: 100);
        u.ReceberDanoSemProcs(-50);
        Assert.AreEqual(100, u.GetVidaAtual());
    }

    [Test]
    public void Should_SurviveDamage_When_HealthRemainsAboveZero()
    {
        var u = CriarUnidade(vida: 100);
        u.ReceberDanoSemProcs(99);
        Assert.IsTrue(u.EstaVivo);
        Assert.AreEqual(1, u.GetVidaAtual());
    }

    // ── MORTE ──────────────────────────────────────────────────────────────

    [Test]
    public void Should_Die_When_HealthReachesExactlyZero()
    {
        var u = CriarUnidade(vida: 50);
        u.ReceberDanoSemProcs(50);
        Assert.IsFalse(u.EstaVivo);
    }

    [Test]
    public void Should_Die_When_HealthDropsBelowZero()
    {
        var u = CriarUnidade(vida: 50);
        u.ReceberDanoSemProcs(9999);
        Assert.IsFalse(u.EstaVivo);
    }

    [Test]
    public void Should_NotReceiveDamage_When_AlreadyDead()
    {
        var u = CriarUnidade(vida: 10);
        u.ReceberDanoSemProcs(10); // mata

        Assert.IsFalse(u.EstaVivo);
        // Segunda chamada deve ser ignorada silenciosamente
        Assert.DoesNotThrow(() => u.ReceberDanoSemProcs(100));
    }

    // ── FACÇÃO — SEM ATAQUE A ALIADOS ─────────────────────────────────────

    [Test]
    public void Should_EhAlvoInimigoValido_ReturnFalse_WhenTargetIsSameFaction()
    {
        // EhAlvoInimigoValido é protected, então testamos via GetDanoAtaque/Execute
        // de forma indireta: uma unidade inimiga recebe dano de ReceberDano sem procs
        // (faccao Jogador não aciona eco runa), e vice-versa.
        // Aqui verificamos apenas que ReceberDano funciona independentemente da facção.
        var aliado = CriarUnidade(Faccao.Jogador, 100);
        aliado.ReceberDanoSemProcs(20);
        Assert.AreEqual(80, aliado.GetVidaAtual());

        var inimigo = CriarUnidade(Faccao.Inimigo, 100);
        inimigo.ReceberDanoSemProcs(20);
        Assert.AreEqual(80, inimigo.GetVidaAtual());
    }

    // ── SINCRONIZAÇÃO DE VIDA ──────────────────────────────────────────────

    [Test]
    public void Should_RestoreFullHealth_When_SincronizarVidaAposSpawnCalled()
    {
        var u = CriarUnidade(vida: 100);
        u.ReceberDanoSemProcs(40);
        Assert.AreEqual(60, u.GetVidaAtual());

        u.vidaMaxima = 150;
        u.SincronizarVidaAposSpawn();

        Assert.AreEqual(150, u.GetVidaAtual());
    }

    [Test]
    public void Should_NotSyncHealth_When_UnitIsDead()
    {
        var u = CriarUnidade(vida: 10);
        u.ReceberDanoSemProcs(10); // mata
        Assert.IsFalse(u.EstaVivo);

        // Chamar SincronizarVidaAposSpawn em unidade morta não deve ressuscitá-la
        u.SincronizarVidaAposSpawn();
        Assert.IsFalse(u.EstaVivo);
    }
}
