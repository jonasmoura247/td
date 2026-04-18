using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Protege o comportamento do evento OnAtaqueExecutado em UnidadeController:
/// - Dispara ao atacar um alvo vivo
/// - NÃO dispara quando alvo é null
/// - NÃO dispara quando alvo está morto
/// </summary>
public class AnimacaoAtaqueEventoTests
{
    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    private UnidadeController CriarUnidade(Faccao faccao = Faccao.Jogador, int vida = 100)
    {
        var go = new GameObject("TestUnit");
        var u = go.AddComponent<UnidadeController>();
        u.faccao = faccao;
        u.vidaMaxima = vida;
        u.danoBase = 1;
        u.SincronizarVidaAposSpawn();
        return u;
    }

    [Test]
    public void Should_FireOnAtaqueExecutado_When_TargetIsAlive()
    {
        var atacante = CriarUnidade(Faccao.Inimigo);
        var alvo = CriarUnidade(Faccao.Jogador);

        bool eventDisparado = false;
        atacante.OnAtaqueExecutado += () => eventDisparado = true;

        // TextoDanoFlutuante.Inicializar chama Destroy em EditMode quando texto é null
        LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));
        atacante.ExecutarAtaqueIA(alvo);

        Assert.IsTrue(eventDisparado,
            "OnAtaqueExecutado deve disparar ao atacar um alvo vivo");
    }

    [Test]
    public void Should_NotFireOnAtaqueExecutado_When_TargetIsNull()
    {
        var atacante = CriarUnidade(Faccao.Inimigo);

        bool eventDisparado = false;
        atacante.OnAtaqueExecutado += () => eventDisparado = true;

        atacante.ExecutarAtaqueIA(null);

        Assert.IsFalse(eventDisparado,
            "OnAtaqueExecutado não deve disparar quando o alvo é null");
    }

    [Test]
    public void Should_NotFireOnAtaqueExecutado_When_TargetIsDead()
    {
        var atacante = CriarUnidade(Faccao.Inimigo);
        var alvo = CriarUnidade(Faccao.Jogador, vida: 10);

        LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));
        alvo.ReceberDanoSemProcs(10); // mata o alvo

        bool eventDisparado = false;
        atacante.OnAtaqueExecutado += () => eventDisparado = true;

        atacante.ExecutarAtaqueIA(alvo);

        Assert.IsFalse(eventDisparado,
            "OnAtaqueExecutado não deve disparar quando o alvo está morto");
    }

    [Test]
    public void Should_AllowMultipleListeners_OnOnAtaqueExecutado()
    {
        var atacante = CriarUnidade(Faccao.Inimigo);
        var alvo = CriarUnidade(Faccao.Jogador);

        int contagemDisparo = 0;
        atacante.OnAtaqueExecutado += () => contagemDisparo++;
        atacante.OnAtaqueExecutado += () => contagemDisparo++;

        LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));
        atacante.ExecutarAtaqueIA(alvo);

        Assert.AreEqual(2, contagemDisparo,
            "Múltiplos subscribers devem todos receber o evento");
    }
}
