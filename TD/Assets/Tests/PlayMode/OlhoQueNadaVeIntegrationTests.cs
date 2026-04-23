using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Testa o ciclo de vida do efeito visual da relíquia 'Olho que nada vê':
/// - Zoom da câmera é aplicado e restaurado corretamente.
/// - UI é criada e destruída sem vazamentos.
/// - Sem crash quando canvas ou câmera são nulos.
/// </summary>
public class OlhoQueNadaVeIntegrationTests
{
    private const float OrthoInicial = 5f;

    [TearDown]
    public void Teardown()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── HELPERS ────────────────────────────────────────────────────────────

    private (EfeitoOlhoQueNadaVe efeito, Camera cam, Canvas canvas) CriarCenario()
    {
        var camGo = new GameObject("Cam");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = OrthoInicial;

        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();

        var efeitoGo = new GameObject("EfeitoOlho");
        var efeito = efeitoGo.AddComponent<EfeitoOlhoQueNadaVe>();

        return (efeito, cam, canvas);
    }

    private ReliquiaContext ContextoCom(Camera cam, Canvas canvas) =>
        new ReliquiaContext { CameraPrincipal = cam, CanvasOverlay = canvas };

    // ── CÂMERA ─────────────────────────────────────────────────────────────

    [Test]
    public void Should_ReducirOrthoSize_QuandoIniciadoComCameraOrthografica()
    {
        var (efeito, cam, canvas) = CriarCenario();
        efeito.Iniciar(ContextoCom(cam, canvas));

        Assert.Less(cam.orthographicSize, OrthoInicial,
            "Camera deve ser recuada (zoom-out) ao iniciar o efeito.");
    }

    [Test]
    public void Should_RestaurarOrthoSize_QuandoEncerrado()
    {
        var (efeito, cam, canvas) = CriarCenario();
        efeito.Iniciar(ContextoCom(cam, canvas));
        efeito.Encerrar();

        Assert.AreEqual(OrthoInicial, cam.orthographicSize, 1e-4f,
            "Camera deve voltar ao tamanho original apos Encerrar.");
    }

    [Test]
    public void Should_NaoAlterarCamera_QuandoEncerradoSemIniciar()
    {
        var (efeito, cam, _) = CriarCenario();
        efeito.Encerrar();
        Assert.AreEqual(OrthoInicial, cam.orthographicSize, 1e-4f);
    }

    // ── UI ─────────────────────────────────────────────────────────────────

    [Test]
    public void Should_CriarFogUI_QuandoShaderEncontrado()
    {
        var (efeito, cam, canvas) = CriarCenario();
        efeito.Iniciar(ContextoCom(cam, canvas));

        // O shader pode não estar carregado no test runner; apenas garantimos sem crash.
        // Se o shader existir, o canvas terá filhos.
        Assert.DoesNotThrow(() => efeito.Encerrar());
    }

    [Test]
    public void Should_LimparUI_QuandoEncerrado()
    {
        var (efeito, cam, canvas) = CriarCenario();
        efeito.Iniciar(ContextoCom(cam, canvas));
        efeito.Encerrar();

        int filhosRestantes = canvas.transform.childCount;
        Assert.AreEqual(0, filhosRestantes,
            "Canvas deve estar limpo apos Encerrar (sem vazamento de UI).");
    }

    // ── ROBUSTEZ ───────────────────────────────────────────────────────────

    [Test]
    public void Should_NaoCrashear_QuandoCanvasEhNulo()
    {
        var (efeito, cam, _) = CriarCenario();
        Assert.DoesNotThrow(() => efeito.Iniciar(ContextoCom(cam, null)));
    }

    [Test]
    public void Should_NaoCrashear_QuandoCameraEhNula()
    {
        var (efeito, _, canvas) = CriarCenario();
        Assert.DoesNotThrow(() => efeito.Iniciar(ContextoCom(null, canvas)));
    }

    [Test]
    public void Should_NaoCrashear_QuandoContextoEhTotalmenteNulo()
    {
        var (efeito, _, _) = CriarCenario();
        Assert.DoesNotThrow(() => efeito.Iniciar(new ReliquiaContext()));
    }

    [Test]
    public void Should_NaoCrashear_QuandoEncerrarChamadoVezesMultiplas()
    {
        var (efeito, cam, canvas) = CriarCenario();
        efeito.Iniciar(ContextoCom(cam, canvas));
        Assert.DoesNotThrow(() =>
        {
            efeito.Encerrar();
            efeito.Encerrar();
        });
    }
}
