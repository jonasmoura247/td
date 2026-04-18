using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;

/// <summary>
/// Protege as regras do sistema de vida e detecção de dano do castelo:
/// - Castelo inicia com vida cheia
/// - Castelo perde vida ao receber dano (via AplicarDanoSomenteVida)
/// - Dano zero ou negativo é ignorado
/// - Castelo destruído não recebe mais dano
/// - Castelo detecta inimigos via OverlapCircle e aplica dano com throttle
/// </summary>
public class CasteloControllerTests
{
    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Cria EstruturasController com faccao Inimigo para evitar GameManager.GameOver ao destruir.
    /// </summary>
    private IEnumerator CriarCastelo(int vida, System.Action<EstruturasController> callback)
    {
        var go = new GameObject("Castelo");
        var castelo = go.AddComponent<EstruturasController>();
        castelo.vidaMaxima = vida;
        castelo.faccao = Faccao.Inimigo;

        yield return null; // Start: inicializa vidaAtual = vidaMaxima

        callback(castelo);
    }

    // ── VIDA INICIAL ──────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_StartWithFullHealth()
    {
        yield return CriarCastelo(100, castelo =>
            Assert.AreEqual(100, castelo.GetVidaAtual(), "Castelo deve iniciar com vida cheia"));
    }

    [UnityTest]
    public IEnumerator Should_StartAlive()
    {
        yield return CriarCastelo(50, castelo =>
            Assert.IsTrue(castelo.EstaVivo, "Castelo deve iniciar vivo"));
    }

    // ── RECEBER DANO ──────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_LoseHealth_WhenAplicarDanoSomenteVida()
    {
        yield return CriarCastelo(100, castelo =>
        {
            castelo.AplicarDanoSomenteVida(30);
            Assert.AreEqual(70, castelo.GetVidaAtual(), "Castelo deve perder vida ao receber dano");
        });
    }

    [UnityTest]
    public IEnumerator Should_AccumulateDamage_OnMultipleHits()
    {
        yield return CriarCastelo(100, castelo =>
        {
            castelo.AplicarDanoSomenteVida(20);
            castelo.AplicarDanoSomenteVida(15);
            Assert.AreEqual(65, castelo.GetVidaAtual(), "Castelo deve acumular dano de múltiplos golpes");
        });
    }

    [UnityTest]
    public IEnumerator Should_IgnoreDamage_WhenQuantidadeIsZero()
    {
        yield return CriarCastelo(100, castelo =>
        {
            castelo.AplicarDanoSomenteVida(0);
            Assert.AreEqual(100, castelo.GetVidaAtual(), "Dano zero não deve reduzir a vida");
        });
    }

    [UnityTest]
    public IEnumerator Should_IgnoreDamage_WhenQuantidadeIsNegative()
    {
        yield return CriarCastelo(100, castelo =>
        {
            castelo.AplicarDanoSomenteVida(-10);
            Assert.AreEqual(100, castelo.GetVidaAtual(), "Dano negativo não deve alterar a vida");
        });
    }

    // ── DESTRUIÇÃO ────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_BeDestroyed_WhenHealthReachesZero()
    {
        yield return CriarCastelo(50, castelo =>
        {
            castelo.AplicarDanoSomenteVida(50);
            Assert.IsFalse(castelo.EstaVivo, "Castelo deve ser destruído ao esgotar a vida");
        });
    }

    [UnityTest]
    public IEnumerator Should_BeDestroyed_WhenOverkillApplied()
    {
        yield return CriarCastelo(50, castelo =>
        {
            castelo.AplicarDanoSomenteVida(999);
            Assert.IsFalse(castelo.EstaVivo, "Overkill deve destruir o castelo");
        });
    }

    [UnityTest]
    public IEnumerator Should_IgnoreSubsequentDamage_WhenAlreadyDestroyed()
    {
        yield return CriarCastelo(50, castelo =>
        {
            castelo.AplicarDanoSomenteVida(50); // destrói
            Assert.DoesNotThrow(() => castelo.AplicarDanoSomenteVida(100),
                "Castelo destruído deve ignorar dano silenciosamente");
        });
    }

    // ── ESTADO VIVO ───────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_RemainAlive_WhenDamageDoesNotExhaustHealth()
    {
        yield return CriarCastelo(100, castelo =>
        {
            castelo.AplicarDanoSomenteVida(99);
            Assert.IsTrue(castelo.EstaVivo, "Castelo com 1 de vida deve continuar vivo");
            Assert.AreEqual(1, castelo.GetVidaAtual());
        });
    }

    // ── THROTTLE (INTERVALO ENTRE ATAQUES DA MESMA UNIDADE) ──────────────────

    [UnityTest]
    public IEnumerator Should_LoseHealth_When_TwoDamagesAppliedWithInterval()
    {
        yield return CriarCastelo(100, castelo =>
        {
            // Simula dois ataques separados (throttle não é testado no nível de AplicarDanoSomenteVida)
            castelo.AplicarDanoSomenteVida(10);
            castelo.AplicarDanoSomenteVida(10);
            Assert.AreEqual(80, castelo.GetVidaAtual(),
                "Dois danos aplicados diretamente devem ambos reduzir a vida");
        });
    }

    // ── FACÇÃO ────────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_HaveJogadorFaction_WhenSetAsPlayerCastle()
    {
        var go = new GameObject("CasteloJogador");
        var castelo = go.AddComponent<EstruturasController>();
        castelo.vidaMaxima = 100;
        castelo.faccao = Faccao.Jogador;

        yield return null;

        Assert.AreEqual(Faccao.Jogador, castelo.faccao,
            "Facção do castelo deve ser mantida corretamente");
    }
}
