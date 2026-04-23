using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;

/// <summary>
/// Testa o comportamento de runtime da Bússola do Verme:
/// indicador aparece/some nos momentos certos e efeito limpa eventos ao encerrar.
/// </summary>
public class BussolaVermeIntegrationTests
{
    [TearDown]
    public void Teardown()
    {
        // DestroyImmediate dispara OnDestroy → Encerrar() → desassina os eventos
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── HELPERS ────────────────────────────────────────────────────────────

    private IndicadorDirecaoWaveUI CriarIndicador()
    {
        var go = new GameObject("Indicador");
        return go.AddComponent<IndicadorDirecaoWaveUI>();
    }

    private (EfeitoBussolaVerme efeito, IndicadorDirecaoWaveUI indicador) CriarEfeitoComIndicador()
    {
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();

        var indicador = CriarIndicador();

        var efeitoGo = new GameObject("EfeitoBussolaVerme");
        var efeito = efeitoGo.AddComponent<EfeitoBussolaVerme>();

        // Injeta prefab via campo serializado (reflexão para testes)
        var campo = typeof(EfeitoBussolaVerme)
            .GetField("prefabIndicador",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        campo?.SetValue(efeito, indicador);

        var ctx = new ReliquiaContext { CanvasOverlay = canvas };
        efeito.Iniciar(ctx);

        // Recupera o indicador instanciado (filho do canvas) — true = inclui inativos,
        // pois Iniciar() chama Esconder() que desativa o objeto imediatamente.
        var indicadorInstanciado = canvas.GetComponentInChildren<IndicadorDirecaoWaveUI>(true);
        return (efeito, indicadorInstanciado);
    }

    // ── INDICADOR DIRETO ───────────────────────────────────────────────────

    [Test]
    public void Indicador_Escondido_QuandoCriado()
    {
        var indicador = CriarIndicador();
        indicador.Esconder();
        Assert.IsFalse(indicador.EstaVisivel);
    }

    [Test]
    public void Indicador_FicaVisivel_QuandoMostrar()
    {
        var indicador = CriarIndicador();
        indicador.Mostrar(LadoSpawn.Esquerda);
        Assert.IsTrue(indicador.EstaVisivel);
    }

    [Test]
    public void Indicador_FicaInvisivel_QuandoEsconder()
    {
        var indicador = CriarIndicador();
        indicador.Mostrar(LadoSpawn.Direita);
        indicador.Esconder();
        Assert.IsFalse(indicador.EstaVisivel);
    }

    [Test]
    public void Indicador_MostrarTodosOsLados_SemExcecao()
    {
        var indicador = CriarIndicador();
        foreach (LadoSpawn lado in System.Enum.GetValues(typeof(LadoSpawn)))
            Assert.DoesNotThrow(() => indicador.Mostrar(lado),
                $"Mostrar({lado}) nao deve lancar excecao");
    }

    // ── INTEGRAÇÃO COM EVENTOS DO WAVEMANAGER ─────────────────────────────

    [UnityTest]
    public IEnumerator Efeito_MostraIndicador_QuandoOnProximaWaveDefinidaDisparado()
    {
        var (_, indicador) = CriarEfeitoComIndicador();

        yield return null;

        WaveManager.OnProximaWaveDefinida?.Invoke(LadoSpawn.Direita);

        Assert.IsTrue(indicador.EstaVisivel,
            "Indicador deve aparecer ao receber OnProximaWaveDefinida");
    }

    [UnityTest]
    public IEnumerator Efeito_EscondeIndicador_QuandoOnWaveIniciadaDisparado()
    {
        var (_, indicador) = CriarEfeitoComIndicador();

        yield return null;

        WaveManager.OnProximaWaveDefinida?.Invoke(LadoSpawn.Esquerda);
        Assert.IsTrue(indicador.EstaVisivel);

        WaveManager.OnWaveIniciada?.Invoke();
        Assert.IsFalse(indicador.EstaVisivel,
            "Indicador deve sumir ao receber OnWaveIniciada");
    }

    [UnityTest]
    public IEnumerator Efeito_NaoLancaExcecao_QuandoEventoDisparadoAposEncerrar()
    {
        var (efeito, _) = CriarEfeitoComIndicador();

        yield return null;

        efeito.Encerrar();

        Assert.DoesNotThrow(() =>
        {
            WaveManager.OnProximaWaveDefinida?.Invoke(LadoSpawn.Cima);
            WaveManager.OnWaveIniciada?.Invoke();
        }, "Eventos apos Encerrar nao devem causar excecao (eventos desassinados)");
    }

    [UnityTest]
    public IEnumerator Efeito_SemPrefab_NaoLancaExcecao()
    {
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();

        var efeitoGo = new GameObject("EfeitoBussolaVermeSemPrefab");
        var efeito = efeitoGo.AddComponent<EfeitoBussolaVerme>();
        // prefabIndicador é null (não atribuído)

        var ctx = new ReliquiaContext { CanvasOverlay = canvas };

        Assert.DoesNotThrow(() => efeito.Iniciar(ctx),
            "Iniciar sem prefab deve apenas logar warning, nao lancar excecao");

        yield return null;
    }
}
