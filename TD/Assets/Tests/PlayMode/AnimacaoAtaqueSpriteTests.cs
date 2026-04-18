using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Protege o comportamento de AnimacaoAtaqueSprite:
/// - Sprite muda para o primeiro frame ao atacar (via LateUpdate com ordem 10)
/// - Avança frames conforme o tempo passa
/// - Volta ao controle da animação de movimento após terminar
/// - Reinicia do frame 0 se novo ataque ocorrer no meio
/// - Não quebra com array vazio ou null
/// </summary>
public class AnimacaoAtaqueSpriteTests
{
    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    private Sprite CriarSprite()
    {
        var tex = new Texture2D(1, 1);
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
    }

    private void SetField(object obj, string campo, object valor)
    {
        typeof(AnimacaoAtaqueSprite)
            .GetField(campo, BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(obj, valor);
    }

    private AnimacaoAtaqueSprite CriarEspantalhoTeste(Sprite[] spritesAtaque, float tempoPorFrame)
    {
        var go = new GameObject("EspantalhoTeste");
        go.AddComponent<Rigidbody2D>();
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<AnimacaoSpritePorVelocidade2D>();

        var unidade = go.AddComponent<UnidadeController>();
        unidade.faccao = Faccao.Inimigo;
        unidade.vidaMaxima = 100;
        unidade.SincronizarVidaAposSpawn();

        var animAtk = go.AddComponent<AnimacaoAtaqueSprite>();
        SetField(animAtk, "spritesAtaque", spritesAtaque);
        SetField(animAtk, "tempoPorFrame", tempoPorFrame);
        return animAtk;
    }

    // ── SPRITE CORRETO AO ATACAR ──────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_ShowFirstAttackFrame_WhenAttackFires()
    {
        var primeiroSprite = CriarSprite();
        var sprites = new[] { primeiroSprite, CriarSprite(), CriarSprite() };
        var animAtk = CriarEspantalhoTeste(sprites, 0.5f);
        var sr = animAtk.GetComponent<SpriteRenderer>();

        yield return null; // Start

        InvocarEvento(animAtk.GetComponent<UnidadeController>());

        yield return null; // LateUpdate roda — AnimacaoAtaqueSprite deve ter prioridade

        Assert.AreEqual(primeiroSprite, sr.sprite,
            "Primeiro frame do ataque deve aparecer após LateUpdate");
    }

    [UnityTest]
    public IEnumerator Should_AdvanceFrames_AsTimePasses()
    {
        var s0 = CriarSprite();
        var s1 = CriarSprite();
        var s2 = CriarSprite();
        var sprites = new[] { s0, s1, s2 };
        const float tempo = 0.1f;
        var animAtk = CriarEspantalhoTeste(sprites, tempo);
        var sr = animAtk.GetComponent<SpriteRenderer>();

        yield return null;

        InvocarEvento(animAtk.GetComponent<UnidadeController>());
        yield return null;
        Assert.AreEqual(s0, sr.sprite, "frame 0");

        yield return new WaitForSeconds(tempo + 0.02f);
        Assert.AreEqual(s1, sr.sprite, "frame 1 após tempoPorFrame");

        yield return new WaitForSeconds(tempo);
        Assert.AreEqual(s2, sr.sprite, "frame 2");
    }

    // ── RETORNO AO CONTROLE DA ANIMAÇÃO DE MOVIMENTO ─────────────────────

    [UnityTest]
    public IEnumerator Should_StopOverridingSprite_AfterAllFramesPlayed()
    {
        var idleSprite = CriarSprite();
        var sprites = new[] { CriarSprite(), CriarSprite() };
        const float tempo = 0.05f;
        var animAtk = CriarEspantalhoTeste(sprites, tempo);
        var sr = animAtk.GetComponent<SpriteRenderer>();

        // Simula sprite idle sendo definido por AnimacaoSpritePorVelocidade2D
        sr.sprite = idleSprite;

        yield return null;

        InvocarEvento(animAtk.GetComponent<UnidadeController>());
        yield return null;
        Assert.AreNotEqual(idleSprite, sr.sprite, "durante ataque sprite deve mudar");

        // Aguarda terminar todos os frames
        yield return new WaitForSeconds(sprites.Length * tempo + 0.15f);

        // Após animação, AnimacaoSpritePorVelocidade2D volta a controlar
        // (neste teste verificamos que AnimacaoAtaqueSprite parou de sobrescrever)
        sr.sprite = idleSprite; // simula LateUpdate de AnimacaoSpritePorVelocidade2D
        yield return null;

        Assert.AreEqual(idleSprite, sr.sprite,
            "Após fim da animação de ataque, AnimacaoSpritePorVelocidade2D retoma o controle");
    }

    // ── REINÍCIO POR NOVO ATAQUE ──────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_RestartFromFrame0_WhenNewAttackFiresMidAnimation()
    {
        var frame0 = CriarSprite();
        var sprites = new[] { frame0, CriarSprite(), CriarSprite() };
        const float tempo = 0.3f;
        var animAtk = CriarEspantalhoTeste(sprites, tempo);
        var sr = animAtk.GetComponent<SpriteRenderer>();
        var unidade = animAtk.GetComponent<UnidadeController>();

        yield return null;

        InvocarEvento(unidade);
        yield return new WaitForSeconds(tempo * 1.5f); // no meio da animação (frame 1)

        Assert.AreNotEqual(frame0, sr.sprite, "deve estar em frame 1+");

        // Novo ataque — reinicia do frame 0
        InvocarEvento(unidade);
        yield return null;

        Assert.AreEqual(frame0, sr.sprite,
            "Novo ataque deve reiniciar a animação do frame 0");
    }

    // ── ROBUSTEZ ──────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_NotCrash_When_SpritesArrayIsEmpty()
    {
        var animAtk = CriarEspantalhoTeste(new Sprite[0], 0.05f);

        yield return null;

        Assert.DoesNotThrow(() => InvocarEvento(animAtk.GetComponent<UnidadeController>()),
            "Array vazio não deve causar exceção");

        yield return null;
        // Nenhuma asserção adicional — verificamos apenas que não crashou
    }

    [UnityTest]
    public IEnumerator Should_NotCrash_When_SpritesArrayIsNull()
    {
        var animAtk = CriarEspantalhoTeste(null, 0.05f);

        yield return null;

        Assert.DoesNotThrow(() => InvocarEvento(animAtk.GetComponent<UnidadeController>()),
            "Array null não deve causar exceção");
    }

    // ── HELPER ────────────────────────────────────────────────────────────

    private static void InvocarEvento(UnidadeController unidade)
    {
        var campo = typeof(UnidadeController)
            .GetField("OnAtaqueExecutado", BindingFlags.Instance | BindingFlags.NonPublic);
        var delegado = campo?.GetValue(unidade) as System.Action;
        delegado?.Invoke();
    }
}
