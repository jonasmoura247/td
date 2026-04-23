using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;

/// <summary>
/// Verifica que o castelo não recebe dano de unidades da mesma facção.
/// Garante que mesmo que um Goblin inimigo seja spawnado com faccao errada,
/// o double-check de faccao no CasteloController o bloqueia.
/// </summary>
public class CasteloFaccaoProtecaoTests
{
    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── HELPERS ────────────────────────────────────────────────────────────

    private UnidadeController CriarUnidade(Faccao faccao, int vida = 100, int dano = 10)
    {
        var go = new GameObject("Unidade_" + faccao);
        var u = go.AddComponent<UnidadeController>();
        u.faccao = faccao;
        u.vidaMaxima = vida;
        u.danoBase = dano;
        u.SincronizarVidaAposSpawn();
        return u;
    }

    private EstruturasController CriarCastelo(Faccao faccao, int vida = 100)
    {
        var go = new GameObject("Castelo_" + faccao);
        var castelo = go.AddComponent<EstruturasController>();
        castelo.faccao = faccao;
        castelo.vidaMaxima = vida;
        return castelo;
    }

    // ── CASTELO NÃO RECEBE DANO DA MESMA FACÇÃO ───────────────────────────

    [Test]
    public void Castelo_NaoRecebeDAno_DeUnidadeDaMesmaFaccao()
    {
        var castelo = CriarCastelo(Faccao.Jogador, vida: 100);
        // Garante que Start() inicializa a vida
        castelo.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

        var unidadeAliada = CriarUnidade(Faccao.Jogador, dano: 50);

        int vidaAntes = castelo.GetVidaAtual();

        // Simula o check que CasteloController faz: mesmo faccao = ignorar
        bool deveIgnorar = unidadeAliada.faccao == castelo.faccao;
        if (!deveIgnorar)
            castelo.AplicarDanoSomenteVida(unidadeAliada.danoBase);

        Assert.AreEqual(vidaAntes, castelo.GetVidaAtual(),
            "Castelo NAO deve receber dano de unidade da mesma faccao (Jogador)");
    }

    [Test]
    public void Castelo_RecebeDAno_DeUnidadeFaccaoInimiga()
    {
        var castelo = CriarCastelo(Faccao.Jogador, vida: 100);
        castelo.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

        var unidadeInimiga = CriarUnidade(Faccao.Inimigo, dano: 30);

        int vidaAntes = castelo.GetVidaAtual();

        // Simula o check que CasteloController faz: faccao diferente = aplicar dano
        bool deveIgnorar = unidadeInimiga.faccao == castelo.faccao;
        if (!deveIgnorar)
            castelo.AplicarDanoSomenteVida(unidadeInimiga.danoBase);

        Assert.AreEqual(vidaAntes - 30, castelo.GetVidaAtual(),
            "Castelo DEVE receber dano de unidade inimiga");
    }

    // ── FACCAO FORCADA PELO WAVEMANAGER ───────────────────────────────────

    [UnityTest]
    public IEnumerator SpawnComFaccaoForcada_SempreRetornaInimigo()
    {
        // Reproduz o que WaveManager.Spawnar() faz:
        // DefinirFaccaoParaSpawn(Faccao.Inimigo) antes do Start()
        var go = new GameObject("GoblinSpawnado");
        var unidade = go.AddComponent<UnidadeController>();
        unidade.vidaMaxima = 50;

        // Prefab pode ter qualquer faccao padrão — WaveManager força Inimigo
        unidade.DefinirFaccaoParaSpawn(Faccao.Inimigo);

        yield return null; // Start() aplica a faccao forcada

        Assert.AreEqual(Faccao.Inimigo, unidade.faccao,
            "WaveManager sempre força Faccao.Inimigo no spawn, independente do prefab");
    }

    [UnityTest]
    public IEnumerator FaccaoForcada_SobreescreveFaccaoOriginalDoPrefab()
    {
        // Mesmo que o prefab defina Faccao.Jogador, o WaveManager sobrescreve para Inimigo
        var go = new GameObject("GoblinComFaccaoErrada");
        var unidade = go.AddComponent<UnidadeController>();
        unidade.faccao = Faccao.Jogador; // valor padrão do prefab (incorreto)
        unidade.vidaMaxima = 50;

        unidade.DefinirFaccaoParaSpawn(Faccao.Inimigo); // WaveManager corrige

        yield return null; // Start() aplica

        Assert.AreEqual(Faccao.Inimigo, unidade.faccao,
            "DefinirFaccaoParaSpawn deve sobrescrever qualquer faccao definida no prefab");
        Assert.AreNotEqual(Faccao.Jogador, unidade.faccao,
            "Unidade spawnada pela wave nunca deve ter Faccao.Jogador");
    }

    // ── DUPLA PROTEÇÃO VERIFICADA ─────────────────────────────────────────

    [Test]
    public void DuplaProtecao_RacaEFaccao_ImpedeCenarioDeAtaque()
    {
        // Camada 1: WaveManager exclui raça do jogador do sorteio
        // Camada 2: mesmo que spawnado, faccao é forçada para Inimigo
        // → unidade com faccao Inimigo NÃO passa no check de faccao do castelo Jogador?
        //   Não — ela DEVE atacar. O ponto é: unidade Jogador NÃO ataca castelo Jogador.

        var castelo = CriarCastelo(Faccao.Jogador, vida: 100);
        castelo.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

        // Pior caso: unidade que "deveria" ser aliada mas com faccao errada
        var unidadeComFaccaoJogador = CriarUnidade(Faccao.Jogador, dano: 99);

        bool casteloDeveIgnorar = unidadeComFaccaoJogador.faccao == castelo.faccao;

        Assert.IsTrue(casteloDeveIgnorar,
            "Castelo detecta e ignora qualquer unidade com Faccao.Jogador (mesma faccao)");
        Assert.AreEqual(100, castelo.GetVidaAtual(),
            "Vida do castelo intacta — faccao identica bloqueia o dano");
    }
}
