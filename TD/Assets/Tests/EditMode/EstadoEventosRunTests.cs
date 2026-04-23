using NUnit.Framework;
using System.Collections.Generic;

/// <summary>
/// Garante que EstadoEventosRun reutiliza eventos existentes (anti-exploit)
/// e registra novos corretamente.
/// </summary>
public class EstadoEventosRunTests
{
    private EstadoEventosRun Criar() => new EstadoEventosRun();

    // ── CriarEvento ───────────────────────────────────────────────────────

    [Test]
    public void Should_CriarEvento_IncrementsContadorEventos()
    {
        var estado = Criar();
        estado.CriarEvento("tenda", 1, 1);
        Assert.AreEqual(1, estado.contadorEventos);
    }

    [Test]
    public void Should_CriarEvento_AdicionaALista()
    {
        var estado = Criar();
        estado.CriarEvento("tenda", 1, 1);
        Assert.AreEqual(1, estado.eventos.Count);
    }

    [Test]
    public void Should_CriarEvento_RegistraTipoCorreto()
    {
        var estado = Criar();
        var ev = estado.CriarEvento("tenda", 1, 2);
        Assert.AreEqual("tenda", ev.tipoEvento);
        Assert.AreEqual(1, ev.mundoId);
        Assert.AreEqual(2, ev.faseId);
    }

    [Test]
    public void Should_CriarEvento_GeraChaveCorreta()
    {
        var estado = Criar();
        var ev = estado.CriarEvento("tenda", 1, 3);
        Assert.AreEqual("tenda_1_3", ev.chaveEvento);
    }

    // ── ObterEvento ───────────────────────────────────────────────────────

    [Test]
    public void Should_ObterEvento_RetornaNull_QuandoNaoExiste()
    {
        var estado = Criar();
        Assert.IsNull(estado.ObterEvento("tenda", 1, 1));
    }

    [Test]
    public void Should_ObterEvento_RetornaEventoExistente()
    {
        var estado = Criar();
        estado.CriarEvento("tenda", 1, 1);
        var ev = estado.ObterEvento("tenda", 1, 1);
        Assert.IsNotNull(ev);
    }

    [Test]
    public void Should_ObterEvento_NaoRetorna_EventoDeOutraFase()
    {
        var estado = Criar();
        estado.CriarEvento("tenda", 1, 1);
        Assert.IsNull(estado.ObterEvento("tenda", 1, 2));
    }

    [Test]
    public void Should_ObterEvento_NaoRetorna_EventoDeOutroTipo()
    {
        var estado = Criar();
        estado.CriarEvento("tenda", 1, 1);
        Assert.IsNull(estado.ObterEvento("reliquia", 1, 1));
    }

    // ── Anti-exploit: reutilização ─────────────────────────────────────────

    [Test]
    public void Should_ObterEvento_RetornarMesmoObjeto_QuandoChamadoVariasVezes()
    {
        var estado = Criar();
        estado.CriarEvento("tenda", 1, 1);
        var ev1 = estado.ObterEvento("tenda", 1, 1);
        var ev2 = estado.ObterEvento("tenda", 1, 1);
        Assert.AreSame(ev1, ev2);
    }

    [Test]
    public void Should_ContadorNaoAumentar_QuandoObterEventoChamado()
    {
        var estado = Criar();
        estado.CriarEvento("tenda", 1, 1);
        estado.ObterEvento("tenda", 1, 1);
        estado.ObterEvento("tenda", 1, 1);
        Assert.AreEqual(1, estado.contadorEventos);
    }

    // ── CriarChave ────────────────────────────────────────────────────────

    [Test]
    public void Should_CriarChave_SerDiferente_ParaFasesDiferentes()
    {
        string c1 = EstadoEventosRun.CriarChave("tenda", 1, 1);
        string c2 = EstadoEventosRun.CriarChave("tenda", 1, 2);
        Assert.AreNotEqual(c1, c2);
    }

    [Test]
    public void Should_CriarChave_SerDiferente_ParaTiposDiferentes()
    {
        string c1 = EstadoEventosRun.CriarChave("tenda", 1, 1);
        string c2 = EstadoEventosRun.CriarChave("reliquia", 1, 1);
        Assert.AreNotEqual(c1, c2);
    }

    // ── Normalizar ────────────────────────────────────────────────────────

    [Test]
    public void Should_Normalizar_CriaListaSeNula()
    {
        var estado = Criar();
        estado.eventos = null;
        estado.Normalizar();
        Assert.IsNotNull(estado.eventos);
    }

    [Test]
    public void Should_Normalizar_ClampContadorNegativo()
    {
        var estado = Criar();
        estado.contadorEventos = -5;
        estado.Normalizar();
        Assert.AreEqual(0, estado.contadorEventos);
    }

    [Test]
    public void Should_Normalizar_ClampFasesSemTendaNegativo()
    {
        var estado = Criar();
        estado.fasesSemTenda = -3;
        estado.Normalizar();
        Assert.AreEqual(0, estado.fasesSemTenda);
    }
}
