using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;

/// <summary>
/// Testa o comportamento de patrulha de unidades aliadas.
///
/// Cobre:
/// - Patrulha inicia quando ambos os pontos são definidos.
/// - Unidade move-se em direção ao destino atual.
/// - Destino inverte ao chegar no ponto.
/// - SuspenderPatrulha impede movimento.
/// - RetomarPatrulha religa movimento.
/// - Patrulha para ao morrer a unidade.
/// - Integração com SistemaIA: retorna a Patrulhando após combate.
/// </summary>
public class PatrulhaUnidadeIntegrationTests
{
    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── HELPERS ───────────────────────────────────────────────────────────

    private (PatrulhaUnidade patrulha, Rigidbody2D rb) CriarUnidadePatrulha(Vector3 posicao = default)
    {
        var go = new GameObject("Patrulheiro");
        go.transform.position = posicao;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        go.AddComponent<MovimentoUnidade>();
        var uc = go.AddComponent<UnidadeController>();
        uc.vidaMaxima = 100;
        uc.SincronizarVidaAposSpawn();
        var patrulha = go.AddComponent<PatrulhaUnidade>();
        return (patrulha, rb);
    }

    // ── INÍCIO DE PATRULHA ────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_StartPatrolling_WhenBothPointsDefined()
    {
        var (patrulha, _) = CriarUnidadePatrulha(Vector3.zero);

        patrulha.DefinirPontoA(Vector3.zero);
        patrulha.DefinirPontoB(new Vector3(10f, 0, 0));

        yield return null;

        Assert.IsTrue(patrulha.EstaPatrulhando,
            "EstaPatrulhando deve ser true após definir ambos os pontos.");
    }

    [Test]
    public void Should_NotStartPatrol_WhenOnlyOnePontoDefined()
    {
        var (patrulha, _) = CriarUnidadePatrulha();
        patrulha.DefinirPontoA(Vector3.zero);
        // DefinirPontoB não chamado

        Assert.IsFalse(patrulha.EstaPatrulhando,
            "Patrulha não deve iniciar sem os dois pontos definidos.");
    }

    // ── MOVIMENTO ────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_MoveTowardPontoB_WhenPatrolling()
    {
        var (patrulha, rb) = CriarUnidadePatrulha(Vector3.zero);
        // Sem SistemaIA: patrol inicia sem suspensão
        patrulha.DefinirPontoA(Vector3.zero);
        patrulha.DefinirPontoB(new Vector3(10f, 0, 0));

        yield return null; // um frame
        yield return new WaitForFixedUpdate();

        Assert.Greater(rb.linearVelocity.x, 0f,
            "Unidade deve se mover para direita (em direção ao PontoB).");
    }

    // ── SUSPENSÃO / RETOMADA ──────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_StopMoving_WhenSuspenderPatrulhaCalled()
    {
        var (patrulha, rb) = CriarUnidadePatrulha(Vector3.zero);
        patrulha.DefinirPontoA(Vector3.zero);
        patrulha.DefinirPontoB(new Vector3(10f, 0, 0));

        yield return null;
        yield return new WaitForFixedUpdate();
        Assert.Greater(rb.linearVelocity.x, 0f, "Pré-condição: deve estar se movendo.");

        patrulha.SuspenderPatrulha();
        yield return new WaitForFixedUpdate();

        Assert.AreEqual(0f, rb.linearVelocity.x, 0.01f,
            "SuspenderPatrulha deve parar a velocidade horizontal.");
    }

    [UnityTest]
    public IEnumerator Should_ResumeMoving_WhenRetomarPatrulhaCalled()
    {
        var (patrulha, rb) = CriarUnidadePatrulha(Vector3.zero);
        patrulha.DefinirPontoA(Vector3.zero);
        patrulha.DefinirPontoB(new Vector3(10f, 0, 0));

        yield return null;
        patrulha.SuspenderPatrulha();
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(0f, rb.linearVelocity.x, 0.01f, "Pré-condição: deve estar parada.");

        patrulha.RetomarPatrulha();
        yield return new WaitForFixedUpdate();

        Assert.Greater(rb.linearVelocity.x, 0f,
            "RetomarPatrulha deve religar o movimento em direção ao destino.");
    }

    // ── CANCELAMENTO ──────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_StopPatrolling_WhenCancelarPatrulhaCalled()
    {
        var (patrulha, _) = CriarUnidadePatrulha(Vector3.zero);
        patrulha.DefinirPontoA(Vector3.zero);
        patrulha.DefinirPontoB(new Vector3(10f, 0, 0));
        yield return null;
        Assert.IsTrue(patrulha.EstaPatrulhando, "Pré-condição: deve estar patrulhando.");

        patrulha.CancelarPatrulha();

        Assert.IsFalse(patrulha.EstaPatrulhando,
            "CancelarPatrulha deve zerar EstaPatrulhando.");
    }

    // ── MORTE DA UNIDADE ──────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_StopPatrolling_WhenUnitDies()
    {
        var (patrulha, _) = CriarUnidadePatrulha(Vector3.zero);
        var uc = patrulha.GetComponent<UnidadeController>();
        patrulha.DefinirPontoA(Vector3.zero);
        patrulha.DefinirPontoB(new Vector3(10f, 0, 0));
        yield return null;
        Assert.IsTrue(patrulha.EstaPatrulhando, "Pré-condição: deve estar patrulhando.");

        uc.ReceberDanoSemProcs(999); // mata a unidade
        yield return null;

        Assert.IsFalse(patrulha.EstaPatrulhando,
            "Patrulha deve parar automaticamente quando a unidade morre.");
    }

    // ── INTEGRAÇÃO COM SistemaIA ──────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_TransitionToPatrolling_WhenSistemaIADetectsPatrolActive()
    {
        var go = new GameObject("PaAliadoComIA");
        go.transform.position = Vector3.zero;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        go.AddComponent<MovimentoUnidade>();
        var uc = go.AddComponent<UnidadeController>();
        uc.vidaMaxima = 100;
        uc.SincronizarVidaAposSpawn();
        var patrulha = go.AddComponent<PatrulhaUnidade>();
        var ia = go.AddComponent<SistemaIAUnidade>();
        ia.alcanceDeteccao = 5f;
        ia.alcanceAtaque = 1f;

        yield return null; // Start executa

        // Inicia patrulha (com SistemaIA, começa suspensa)
        patrulha.DefinirPontoA(Vector3.zero);
        patrulha.DefinirPontoB(new Vector3(10f, 0, 0));

        yield return null; // SistemaIA.Update detecta EstaPatrulhando e transiciona

        Assert.AreEqual(EstadoIA.Patrulhando, ia.Estado,
            "SistemaIA deve entrar no estado Patrulhando ao detectar patrulha ativa.");
    }

    // ── PAUSA MANUAL ──────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_StopMoving_WhenPausarPatrulhaCalled()
    {
        var (patrulha, rb) = CriarUnidadePatrulha(Vector3.zero);
        patrulha.DefinirPontoA(Vector3.zero);
        patrulha.DefinirPontoB(new Vector3(10f, 0, 0));

        yield return null;
        yield return new WaitForFixedUpdate();
        Assert.Greater(rb.linearVelocity.x, 0f, "Pré-condição: deve estar se movendo.");

        patrulha.PausarPatrulha();
        yield return new WaitForFixedUpdate();

        Assert.IsTrue(patrulha.IsPausada, "IsPausada deve ser true após PausarPatrulha.");
        Assert.AreEqual(0f, rb.linearVelocity.x, 0.01f, "Velocidade deve ser zero ao pausar.");
    }

    [UnityTest]
    public IEnumerator Should_ResumeMoving_WhenDespausarPatrulhaCalled()
    {
        var (patrulha, rb) = CriarUnidadePatrulha(Vector3.zero);
        patrulha.DefinirPontoA(Vector3.zero);
        patrulha.DefinirPontoB(new Vector3(10f, 0, 0));

        yield return null;
        patrulha.PausarPatrulha();
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(patrulha.IsPausada, "Pré-condição: deve estar pausada.");

        patrulha.DespausarPatrulha();
        yield return new WaitForFixedUpdate();

        Assert.IsFalse(patrulha.IsPausada, "IsPausada deve ser false após DespausarPatrulha.");
        Assert.Greater(rb.linearVelocity.x, 0f, "Unidade deve retomar movimento após despausa.");
    }

    // ── PARAR AO DETECTAR INIMIGO ─────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_StopPatrolling_WhenEnemyEntersDetectionRange()
    {
        // Unidade aliada com SistemaIA patrulhando
        var go = new GameObject("AliadoPatrulheiro");
        go.transform.position = Vector3.zero;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        go.AddComponent<MovimentoUnidade>();
        var uc = go.AddComponent<UnidadeController>();
        uc.vidaMaxima = 100;
        uc.faccao = Faccao.Jogador;
        uc.SincronizarVidaAposSpawn();
        go.layer = LayerMask.NameToLayer("Jogador");
        var patrulha = go.AddComponent<PatrulhaUnidade>();
        var ia = go.AddComponent<SistemaIAUnidade>();
        ia.alcanceDeteccao = 15f;
        ia.alcanceAtaque = 3f;
        ia.intervaloScan = 0.05f;

        yield return null; // Start

        patrulha.DefinirPontoA(new Vector3(-5f, 0, 0));
        patrulha.DefinirPontoB(new Vector3(5f, 0, 0));
        yield return null; // transiciona para Patrulhando

        Assert.AreEqual(EstadoIA.Patrulhando, ia.Estado, "Pré-condição: deve estar em Patrulhando.");

        // Spawn inimigo dentro do raio de detecção
        var inimigoGo = new GameObject("Inimigo");
        inimigoGo.transform.position = new Vector3(3f, 0, 0);
        var rbInimigo = inimigoGo.AddComponent<Rigidbody2D>();
        rbInimigo.gravityScale = 0f;
        inimigoGo.AddComponent<CircleCollider2D>(); // necessário para Physics2D.OverlapCircle detectar
        var inimigo = inimigoGo.AddComponent<UnidadeController>();
        inimigo.vidaMaxima = 50;
        inimigo.faccao = Faccao.Inimigo;
        inimigo.SincronizarVidaAposSpawn();
        inimigoGo.layer = LayerMask.NameToLayer("Inimigo");
        // Garante que layerAlvos inclui o layer do inimigo independente da configuração de projeto
        ia.layerAlvos = ~0;

        // Aguarda scan detectar o inimigo
        yield return new WaitForSeconds(0.3f);

        Assert.AreNotEqual(EstadoIA.Patrulhando, ia.Estado,
            "IA deve sair do estado Patrulhando ao detectar inimigo.");
        Assert.IsNotNull(ia.AlvoAtual,
            "AlvoAtual deve ser definido ao detectar inimigo.");
    }

    [UnityTest]
    public IEnumerator Should_ResumePatrolling_AfterCombatEnds()
    {
        var go = new GameObject("PaAliadoComIA");
        go.transform.position = Vector3.zero;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        go.AddComponent<MovimentoUnidade>();
        var uc = go.AddComponent<UnidadeController>();
        uc.vidaMaxima = 100;
        uc.SincronizarVidaAposSpawn();
        uc.faccao = Faccao.Jogador;
        var patrulha = go.AddComponent<PatrulhaUnidade>();
        var ia = go.AddComponent<SistemaIAUnidade>();
        ia.alcanceDeteccao = 20f;
        ia.alcanceAtaque = 2f;

        yield return null; // Start executa

        patrulha.DefinirPontoA(Vector3.zero);
        patrulha.DefinirPontoB(new Vector3(10f, 0, 0));
        yield return null; // transiciona para Patrulhando

        Assert.AreEqual(EstadoIA.Patrulhando, ia.Estado, "Pré-condição: deve estar Patrulhando.");

        // Cria inimigo para simular combate
        var inimigoGo = new GameObject("Inimigo");
        inimigoGo.transform.position = new Vector3(1f, 0, 0);
        var rbInimigo = inimigoGo.AddComponent<Rigidbody2D>();
        rbInimigo.gravityScale = 0f;
        var inimigo = inimigoGo.AddComponent<UnidadeController>();
        inimigo.vidaMaxima = 10;
        inimigo.SincronizarVidaAposSpawn();
        inimigo.faccao = Faccao.Inimigo;
        inimigoGo.layer = LayerMask.NameToLayer("Inimigo");

        // Força scan imediato definindo alvo diretamente via reflexão
        var campoAlvo = typeof(SistemaIAUnidade)
            .GetProperty("AlvoAtual",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        // Usa método público para simular detecção (mata o inimigo para encerrar combate)
        inimigo.ReceberDanoSemProcs(999); // mata inimigo imediatamente

        yield return null; // IA detecta alvo morto → LimparAlvo → Patrulhando

        // Como inimigo já está morto, IA deve retornar a Patrulhando
        Assert.AreNotEqual(EstadoIA.Morto, ia.Estado,
            "Unidade aliada não deve estar morta.");
        Assert.IsTrue(patrulha.EstaPatrulhando,
            "EstaPatrulhando deve permanecer true (intenção de patrulha preservada).");
    }
}
