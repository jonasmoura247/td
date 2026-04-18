using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;

/// <summary>
/// Protege o comportamento de movimento de unidades via waypoints.
/// Cobre: parada sem rota, avanço de waypoints, parada ao esgotar rota,
/// parada explícita e modo livre (controle cedido à IA).
/// </summary>
public class MovimentoUnidadeTests
{
    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────

    private (MovimentoUnidade mov, Rigidbody2D rb) CriarUnidade(Vector3 posicao = default)
    {
        var go = new GameObject("Unit");
        go.transform.position = posicao;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        var mov = go.AddComponent<MovimentoUnidade>();
        mov.velocidade = 5f;
        mov.distanciaParada = 0.3f;
        return (mov, rb);
    }

    private Transform CriarRota(params Vector3[] posicoes)
    {
        var rota = new GameObject("Rota").transform;
        foreach (var pos in posicoes)
        {
            var wp = new GameObject("WP");
            wp.transform.parent = rota;
            wp.transform.position = pos;
        }
        return rota;
    }

    // ── SEM ROTA ──────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_HaveZeroVelocity_WhenNoRouteSet()
    {
        var (_, rb) = CriarUnidade();

        yield return new WaitForFixedUpdate();

        Assert.AreEqual(Vector2.zero, rb.linearVelocity,
            "Sem rota definida, unidade não deve se mover");
    }

    [UnityTest]
    public IEnumerator Should_HaveZeroVelocity_WhenRouteHasNoWaypoints()
    {
        var (mov, rb) = CriarUnidade();
        var rota = new GameObject("RotaVazia").transform;

        mov.DefinirRota(rota);

        yield return new WaitForFixedUpdate();

        Assert.AreEqual(Vector2.zero, rb.linearVelocity,
            "Rota sem waypoints não deve causar movimento");
    }

    // ── MOVIMENTO NORMAL ──────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_MoveTowardWaypoint_WhenRouteSet()
    {
        var (mov, rb) = CriarUnidade(Vector3.zero);
        var rota = CriarRota(new Vector3(10f, 0, 0));

        mov.DefinirRota(rota);

        yield return new WaitForFixedUpdate();

        Assert.Greater(rb.linearVelocity.x, 0f,
            "Unidade deve se mover em direção ao waypoint");
    }

    [UnityTest]
    public IEnumerator Should_AdvanceToNextWaypoint_WhenCloseEnough()
    {
        var (mov, rb) = CriarUnidade(Vector3.zero);
        // WP1 muito próximo (< distanciaParada 0.3f), WP2 longe
        var rota = CriarRota(new Vector3(0.1f, 0, 0), new Vector3(10f, 0, 0));

        mov.DefinirRota(rota);

        yield return new WaitForFixedUpdate(); // avança WP1 → pontoAtual = 1
        yield return new WaitForFixedUpdate(); // move em direção ao WP2

        Assert.AreEqual(1, mov.IndiceWaypointAtual,
            "Unidade deve ter avançado para o segundo waypoint");
        Assert.Greater(rb.linearVelocity.x, 0f,
            "Unidade deve estar se movendo em direção ao segundo waypoint");
    }

    // ── PARADA AO ESGOTAR ROTA (COMPORTAMENTO CORRETO NO TD) ─────────────────

    [UnityTest]
    public IEnumerator Should_StopMoving_WhenAllWaypointsExhausted()
    {
        var (mov, rb) = CriarUnidade(Vector3.zero);
        // Único waypoint dentro do distanciaParada: avançado imediatamente
        var rota = CriarRota(new Vector3(0.1f, 0, 0));
        mov.distanciaParada = 0.5f;

        mov.DefinirRota(rota);

        yield return new WaitForFixedUpdate(); // avança WP → pontoAtual >= Length
        yield return new WaitForFixedUpdate(); // waypoints esgotados: deve parar

        Assert.AreEqual(Vector2.zero, rb.linearVelocity,
            "Unidade deve parar ao esgotar todos os waypoints (aguarda detecção do castelo via OverlapCircle)");
    }

    [UnityTest]
    public IEnumerator Should_HaveExhaustedRoute_WhenAllWaypointsVisited()
    {
        var (mov, rb) = CriarUnidade(Vector3.zero);
        var rota = CriarRota(new Vector3(0.1f, 0, 0));
        mov.distanciaParada = 0.5f;

        mov.DefinirRota(rota);

        yield return new WaitForFixedUpdate();

        Assert.GreaterOrEqual(mov.IndiceWaypointAtual, mov.TotalPontosNaRota,
            "IndiceWaypointAtual deve ser >= TotalPontosNaRota ao esgotar a rota");
    }

    // ── PARADA EXPLÍCITA ──────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_StopImmediately_WhenPararMovimentoCalled()
    {
        var (mov, rb) = CriarUnidade(Vector3.zero);
        var rota = CriarRota(new Vector3(10f, 0, 0));
        mov.DefinirRota(rota);

        yield return new WaitForFixedUpdate();

        mov.PararMovimento();

        yield return new WaitForFixedUpdate();

        Assert.AreEqual(Vector2.zero, rb.linearVelocity,
            "PararMovimento deve zerar a velocidade imediatamente");
    }

    // ── RETOMADA DE MOVIMENTO ─────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_ResumeMoving_WhenContinuarMovimentoCalled()
    {
        var (mov, rb) = CriarUnidade(Vector3.zero);
        var rota = CriarRota(new Vector3(10f, 0, 0));
        mov.DefinirRota(rota);

        yield return new WaitForFixedUpdate();

        mov.PararMovimento();
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(Vector2.zero, rb.linearVelocity, "Pré-condição: deve estar parado");

        mov.ContinuarMovimento();
        yield return new WaitForFixedUpdate();

        Assert.Greater(rb.linearVelocity.x, 0f,
            "ContinuarMovimento deve retomar o movimento em direção ao waypoint");
    }

    // ── MODO LIVRE (IA ASSUMINDO CONTROLE) ───────────────────────────────────

    [UnityTest]
    public IEnumerator Should_NotOverrideVelocity_WhenInModoLivre()
    {
        var (mov, rb) = CriarUnidade(Vector3.zero);
        var rota = CriarRota(new Vector3(10f, 0, 0));
        mov.DefinirRota(rota);
        mov.EntrarModoLivre();

        rb.linearVelocity = new Vector2(99f, 0f);

        yield return new WaitForFixedUpdate();

        Assert.AreEqual(99f, rb.linearVelocity.x,
            "Em modo livre, MovimentoUnidade não deve sobrescrever a velocidade da IA");
    }

    [UnityTest]
    public IEnumerator Should_ResumeWaypointControl_WhenEntrarModoWaypointsCalled()
    {
        var (mov, rb) = CriarUnidade(Vector3.zero);
        var rota = CriarRota(new Vector3(10f, 0, 0));
        mov.DefinirRota(rota);
        mov.EntrarModoLivre();

        rb.linearVelocity = new Vector2(99f, 0f);
        yield return new WaitForFixedUpdate();

        mov.EntrarModoWaypoints();
        yield return new WaitForFixedUpdate();

        Assert.AreNotEqual(99f, rb.linearVelocity.x,
            "Após EntrarModoWaypoints, controle de velocidade volta aos waypoints");
    }
}
