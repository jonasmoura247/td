using NUnit.Framework;
using UnityEngine;

public class GeradorHeroisTendaTests
{
    [Test]
    public void Should_GerarMesmasOpcoes_ComMesmaSeed()
    {
        var db = CriarDatabase(3);
        var gerador = new GeradorHeroisTenda();

        var opcoes1 = gerador.GerarOpcoes(db, new RNGDeterministico(77));
        var opcoes2 = gerador.GerarOpcoes(db, new RNGDeterministico(77));

        Assert.AreEqual(opcoes1.Count, opcoes2.Count);
        for (int i = 0; i < opcoes1.Count; i++)
            Assert.AreEqual(opcoes1[i], opcoes2[i]);
    }

    [Test]
    public void Should_NaoDuplicarHerois_NaMesmaTenda()
    {
        var db = CriarDatabase(4);
        var gerador = new GeradorHeroisTenda();
        var opcoes = gerador.GerarOpcoes(db, new RNGDeterministico(11));

        CollectionAssert.AllItemsAreUnique(opcoes);
    }

    [Test]
    public void Should_RetornarSoOsDisponiveis_QuandoBancoTemMenosDeTres()
    {
        var db = CriarDatabase(1);
        var gerador = new GeradorHeroisTenda();
        var opcoes = gerador.GerarOpcoes(db, new RNGDeterministico(5));

        Assert.AreEqual(1, opcoes.Count);
    }

    private static HeroiDatabase CriarDatabase(int quantidade)
    {
        var db = ScriptableObject.CreateInstance<HeroiDatabase>();
        db.entradas = new HeroiDef[quantidade];

        for (int i = 0; i < quantidade; i++)
        {
            var def = ScriptableObject.CreateInstance<HeroiDef>();
            def.idHeroi = $"heroi_{i}";
            def.nomeExibicao = $"Heroi {i}";
            db.entradas[i] = def;
        }

        return db;
    }
}
