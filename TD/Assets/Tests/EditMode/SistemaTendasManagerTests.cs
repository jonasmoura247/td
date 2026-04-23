using NUnit.Framework;
using UnityEngine;

public class SistemaTendasManagerTests
{
    private GameObject _go;
    private SistemaTendasManager _manager;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("SistemaTendasManagerTests");
        _manager = _go.AddComponent<SistemaTendasManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null)
            Object.DestroyImmediate(_go);
    }

    [Test]
    public void Should_ReutilizarEventoExistente_SemCriarNovoRegistro()
    {
        var estado = new EstadoEventosRun();

        var primeiro = _manager.ObterOuGerarTenda("save_1", 1, 2, estado);
        int contadorDepoisPrimeiro = estado.contadorEventos;
        int totalDepoisPrimeiro = estado.eventos.Count;

        var segundo = _manager.ObterOuGerarTenda("save_1", 1, 2, estado);

        Assert.AreSame(primeiro, segundo);
        Assert.AreEqual(contadorDepoisPrimeiro, estado.contadorEventos);
        Assert.AreEqual(totalDepoisPrimeiro, estado.eventos.Count);
    }

    [Test]
    public void Should_GerarSeedEChave_QuandoEventoNovo()
    {
        var estado = new EstadoEventosRun();

        var evento = _manager.ObterOuGerarTenda("save_abc", 3, 7, estado);

        Assert.IsNotNull(evento);
        Assert.AreEqual("tenda_3_7", evento.chaveEvento);
        Assert.AreEqual("tenda", evento.tipoEvento);
        Assert.AreEqual(3, evento.mundoId);
        Assert.AreEqual(7, evento.faseId);
        Assert.AreNotEqual(0, evento.seedEvento);
        Assert.AreEqual(evento.seedEvento, estado.seedAtual);
    }

    [Test]
    public void Should_EventoMercador_SerResolvido_AoResgatarReliquia()
    {
        var evento = new EventoGerado
        {
            temTenda = true,
            tipoTendaInt = (int)TipoTenda.Mercador,
            reliquiaResgatada = true
        };

        Assert.IsTrue(SistemaTendasManager.EventoFoiResolvido(evento));
    }

    [Test]
    public void Should_EventoHerois_SerResolvido_AoResgatarHeroi()
    {
        var evento = new EventoGerado
        {
            temTenda = true,
            tipoTendaInt = (int)TipoTenda.Herois,
            heroiResgatado = true
        };

        Assert.IsTrue(SistemaTendasManager.EventoFoiResolvido(evento));
    }

    [Test]
    public void Should_EventoEspecial_SerResolvido_AoVisitar()
    {
        var evento = new EventoGerado
        {
            temTenda = true,
            tipoTendaInt = (int)TipoTenda.EventoEspecial,
            foiVisitada = true
        };

        Assert.IsTrue(SistemaTendasManager.EventoFoiResolvido(evento));
    }

    [Test]
    public void Should_EventoMisto_ExigirAsDuasRecompensas()
    {
        var evento = new EventoGerado
        {
            temTenda = true,
            tipoTendaInt = (int)TipoTenda.Mista,
            reliquiaResgatada = true,
            heroiResgatado = false
        };

        Assert.IsFalse(SistemaTendasManager.EventoFoiResolvido(evento));
        evento.heroiResgatado = true;
        Assert.IsTrue(SistemaTendasManager.EventoFoiResolvido(evento));
    }

    [Test]
    public void Should_EventoNuloOuSemTenda_SerConsideradoResolvido()
    {
        Assert.IsTrue(SistemaTendasManager.EventoFoiResolvido(null));

        var evento = new EventoGerado
        {
            temTenda = false,
            tipoTendaInt = (int)TipoTenda.Mercador
        };

        Assert.IsTrue(SistemaTendasManager.EventoFoiResolvido(evento));
    }
}
