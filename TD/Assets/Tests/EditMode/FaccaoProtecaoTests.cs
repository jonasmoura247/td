using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

/// <summary>
/// Verifica que o sistema de facção impede ataques entre unidades da mesma facção.
/// Protege a regra: jogador Goblin NÃO pode ser atacado por unidades Goblin aliadas,
/// e inimigos nunca atacam outros inimigos.
/// </summary>
public class FaccaoProtecaoTests
{
    // Helper: expõe EhAlvoInimigoValido (protected) para testes
    private class UnidadeExposta : UnidadeController
    {
        public bool AlvoEhValido(UnidadeController alvo) => EhAlvoInimigoValido(alvo);
    }

    private UnidadeExposta CriarUnidade(Faccao faccao, int vida = 100)
    {
        var go = new GameObject();
        var u = go.AddComponent<UnidadeExposta>();
        u.faccao = faccao;
        u.vidaMaxima = vida;
        u.SincronizarVidaAposSpawn();
        return u;
    }

    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── MESMA FACÇÃO — NUNCA DEVE ATACAR ──────────────────────────────────

    [Test]
    public void AlvoInvalido_QuandoAmbosJogador()
    {
        var atacante = CriarUnidade(Faccao.Jogador);
        var alvo     = CriarUnidade(Faccao.Jogador);

        Assert.IsFalse(atacante.AlvoEhValido(alvo),
            "Unidade Jogador NAO pode atacar outra unidade Jogador");
    }

    [Test]
    public void AlvoInvalido_QuandoAmbosInimigo()
    {
        var atacante = CriarUnidade(Faccao.Inimigo);
        var alvo     = CriarUnidade(Faccao.Inimigo);

        Assert.IsFalse(atacante.AlvoEhValido(alvo),
            "Unidade Inimigo NAO pode atacar outra unidade Inimigo");
    }

    // ── FACÇÕES DIFERENTES — DEVE PODER ATACAR ────────────────────────────

    [Test]
    public void AlvoValido_QuandoJogadorAtacaInimigo()
    {
        var atacante = CriarUnidade(Faccao.Jogador);
        var alvo     = CriarUnidade(Faccao.Inimigo);

        Assert.IsTrue(atacante.AlvoEhValido(alvo),
            "Unidade Jogador DEVE poder atacar unidade Inimigo");
    }

    [Test]
    public void AlvoValido_QuandoInimigoAtacaJogador()
    {
        var atacante = CriarUnidade(Faccao.Inimigo);
        var alvo     = CriarUnidade(Faccao.Jogador);

        Assert.IsTrue(atacante.AlvoEhValido(alvo),
            "Unidade Inimigo DEVE poder atacar unidade Jogador");
    }

    // ── CASOS ESPECIAIS ───────────────────────────────────────────────────

    [Test]
    public void AlvoInvalido_QuandoAlvoNulo()
    {
        var atacante = CriarUnidade(Faccao.Inimigo);

        Assert.IsFalse(atacante.AlvoEhValido(null),
            "Alvo nulo nunca e valido");
    }

    [Test]
    public void AlvoInvalido_QuandoAlvoEhASiMesmo()
    {
        var unidade = CriarUnidade(Faccao.Inimigo);

        Assert.IsFalse(unidade.AlvoEhValido(unidade),
            "Unidade nao pode se atacar");
    }

    [Test]
    public void AlvoInvalido_QuandoAlvoEstaMorto()
    {
        var atacante = CriarUnidade(Faccao.Inimigo);
        var alvo     = CriarUnidade(Faccao.Jogador);

        // Morrer() chama Destroy() que não é permitido em EditMode — suprime o erro esperado
        LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));
        alvo.ReceberDanoSemProcs(9999);

        Assert.IsFalse(atacante.AlvoEhValido(alvo),
            "Unidade morta nunca e alvo valido");
    }

    // ── REGRA ESPECÍFICA: GOBLIN JOGADOR vs GOBLIN INIMIGO ───────────────

    [Test]
    public void GoblinJogador_NaoAtacaGoblinJogador()
    {
        // Simula dois Goblins com faccao Jogador (aliados do mesmo time)
        var goblinAliado1 = CriarUnidade(Faccao.Jogador);
        var goblinAliado2 = CriarUnidade(Faccao.Jogador);

        Assert.IsFalse(goblinAliado1.AlvoEhValido(goblinAliado2),
            "Goblins aliados nao se atacam entre si");
    }

    [Test]
    public void GoblinInimigo_AtacaJogador()
    {
        // Goblins spawnados como wave têm Faccao.Inimigo (forçado pelo WaveManager)
        var goblinInimigo = CriarUnidade(Faccao.Inimigo);
        var unidadeJogador = CriarUnidade(Faccao.Jogador);

        Assert.IsTrue(goblinInimigo.AlvoEhValido(unidadeJogador),
            "Goblin inimigo (spawnado pela wave) DEVE atacar unidades do jogador");
    }

    [Test]
    public void FaccaoForcadaParaInimigo_NaoAtacaOutroInimigo()
    {
        // Reproduz o comportamento do WaveManager: força Faccao.Inimigo no spawn
        var goblinSpawnado = CriarUnidade(Faccao.Inimigo);
        var outroInimigo   = CriarUnidade(Faccao.Inimigo);

        Assert.IsFalse(goblinSpawnado.AlvoEhValido(outroInimigo),
            "Inimigos spawnados nao se atacam entre si (mesma faccao)");
    }
}
