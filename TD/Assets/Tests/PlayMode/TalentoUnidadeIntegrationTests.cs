using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;

/// <summary>
/// Integração: desbloqueio de talento → spawn de unidade → atributos aplicados.
/// Valida que o pipeline completo (TalentosRegras → ModificadoresPartidaTalentos → UnidadeController)
/// aplica corretamente os bônus de dano e vida máxima.
/// </summary>
public class TalentoUnidadeIntegrationTests
{
    private TalentTreeDatabase _db;
    private DadosSlotSave _dados;

    [SetUp]
    public void Setup()
    {
        // Database com 1 ponto por nível (intervalo=1) para facilitar os testes
        _db = ScriptableObject.CreateInstance<TalentTreeDatabase>();
        _db.niveisPorNivelGanho = 1;
        _db.pontosPorGanho = 1;

        _dados = new DadosSlotSave(0, TipoRaca.Goblin, GeneroChefe.Masculino);
        _dados.NormalizarTalentos();
    }

    [TearDown]
    public void Teardown()
    {
        PerfilJogadorAtivo.LimparPerfil();
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
        if (_db != null)
            Object.DestroyImmediate(_db);
    }

    // ── HELPERS ────────────────────────────────────────────────────────────

    private TalentNodeEntry CriarNoSemRequisitos(
        string id,
        StatTalento stat,
        float flat,
        float percentual,
        AlvoEfeitoTalento alvo = AlvoEfeitoTalento.MesmaRacaPerfil,
        UnidadeSubtipo subtipo = UnidadeSubtipo.Indefinido)
    {
        return new TalentNodeEntry
        {
            id = id,
            custoPontos = 1,
            nivelMinimoRaca = 1,
            requisitosItemIds = System.Array.Empty<string>(),
            grupoEscolhaId = "",
            efeitos = new[]
            {
                new TalentEffectEntry
                {
                    alvo = alvo,
                    subtipoFiltro = subtipo,
                    stat = stat,
                    valorFlat = flat,
                    valorPercentual = percentual
                }
            }
        };
    }

    private void ConfigurarDatabase(TalentNodeEntry[] nos)
    {
        _db.arvoresPorRaca = new[]
        {
            new TalentRaceBundle { raca = TipoRaca.Goblin, nos = nos }
        };
    }

    private UnidadeController CriarUnidadeGoblin(int danoBase = 10, int vida = 100)
    {
        var go = new GameObject("GoblinUnidade");
        var u = go.AddComponent<UnidadeController>();
        var id = go.AddComponent<UnidadeIdentidade>();
        id.raca = TipoRaca.Goblin;
        id.subtipo = UnidadeSubtipo.Indefinido;
        u.faccao = Faccao.Jogador;
        u.danoBase = danoBase;
        u.vidaMaxima = vida;
        u.SincronizarVidaAposSpawn();
        return u;
    }

    // ── DANO FLAT ──────────────────────────────────────────────────────────

    [Test]
    public void Should_BoostDamage_When_FlatDamageTalentUnlocked()
    {
        var no = CriarNoSemRequisitos("dano_flat", StatTalento.Dano, flat: 20, percentual: 0);
        ConfigurarDatabase(new[] { no });

        bool desbloqueado = TalentosRegras.TentarDesbloquear("dano_flat", _db, _dados);
        Assert.IsTrue(desbloqueado, "Talento deve ser desbloqueado com sucesso");

        PerfilJogadorAtivo.DefinirPerfil(_dados);

        var unidade = CriarUnidadeGoblin(danoBase: 10);
        ModificadoresPartidaTalentos.Aplicar(unidade, _db);

        Assert.AreEqual(30, unidade.GetDanoAtaque(),
            "Dano deve ser 10 (base) + 20 (flat talent) = 30");
    }

    // ── DANO PERCENTUAL ────────────────────────────────────────────────────

    [Test]
    public void Should_BoostDamage_When_PercentualDamageTalentUnlocked()
    {
        var no = CriarNoSemRequisitos("dano_perc", StatTalento.Dano, flat: 0, percentual: 100f);
        ConfigurarDatabase(new[] { no });

        TalentosRegras.TentarDesbloquear("dano_perc", _db, _dados);
        PerfilJogadorAtivo.DefinirPerfil(_dados);

        var unidade = CriarUnidadeGoblin(danoBase: 10);
        ModificadoresPartidaTalentos.Aplicar(unidade, _db);

        Assert.AreEqual(20, unidade.GetDanoAtaque(),
            "Dano deve ser 10 * (1 + 1.0) = 20 com talento +100%");
    }

    // ── VIDA MÁXIMA ────────────────────────────────────────────────────────

    [Test]
    public void Should_IncreaseMaxLife_When_MaxLifeTalentUnlocked()
    {
        var no = CriarNoSemRequisitos("vida_flat", StatTalento.VidaMaxima, flat: 50, percentual: 0);
        ConfigurarDatabase(new[] { no });

        TalentosRegras.TentarDesbloquear("vida_flat", _db, _dados);
        PerfilJogadorAtivo.DefinirPerfil(_dados);

        var unidade = CriarUnidadeGoblin(vida: 100);
        ModificadoresPartidaTalentos.Aplicar(unidade, _db);

        Assert.AreEqual(150, unidade.vidaMaxima,
            "Vida máxima deve ser 100 (base) + 50 (flat talent) = 150");
    }

    // ── SEM PERFIL ATIVO ───────────────────────────────────────────────────

    [Test]
    public void Should_NotApplyModifiers_When_NoActiveProfile()
    {
        var no = CriarNoSemRequisitos("dano_flat", StatTalento.Dano, flat: 999, percentual: 0);
        ConfigurarDatabase(new[] { no });
        // Intencionalmente NÃO chamar PerfilJogadorAtivo.DefinirPerfil

        var unidade = CriarUnidadeGoblin(danoBase: 10);
        ModificadoresPartidaTalentos.Aplicar(unidade, _db);

        Assert.AreEqual(10, unidade.GetDanoAtaque(),
            "Sem perfil ativo, modificadores não devem ser aplicados");
    }

    // ── SEM TALENTOS DESBLOQUEADOS ─────────────────────────────────────────

    [Test]
    public void Should_NotModifyDamage_When_NoTalentsUnlocked()
    {
        var no = CriarNoSemRequisitos("dano_flat", StatTalento.Dano, flat: 50, percentual: 0);
        ConfigurarDatabase(new[] { no });
        // Não desbloquear o talento
        PerfilJogadorAtivo.DefinirPerfil(_dados);

        var unidade = CriarUnidadeGoblin(danoBase: 10);
        ModificadoresPartidaTalentos.Aplicar(unidade, _db);

        Assert.AreEqual(10, unidade.GetDanoAtaque(),
            "Sem talentos desbloqueados, dano não deve ser alterado");
    }

    // ── TALENTO SÓ SE APLICA À RAÇA CORRETA ────────────────────────────────

    [Test]
    public void Should_NotApplyDamageBonus_When_TalentTargetsEnemyRace()
    {
        // Talento que bônus dano para inimigos Humanos (não se aplica a unidade Goblin aliada)
        var no = new TalentNodeEntry
        {
            id = "dano_vs_humano",
            custoPontos = 1,
            nivelMinimoRaca = 1,
            requisitosItemIds = System.Array.Empty<string>(),
            grupoEscolhaId = "",
            efeitos = new[]
            {
                new TalentEffectEntry
                {
                    alvo = AlvoEfeitoTalento.InimigoDeRaca,
                    racaAlvoInimigo = TipoRaca.Humano,
                    stat = StatTalento.Dano,
                    valorFlat = 999
                }
            }
        };
        ConfigurarDatabase(new[] { no });

        TalentosRegras.TentarDesbloquear("dano_vs_humano", _db, _dados);
        PerfilJogadorAtivo.DefinirPerfil(_dados);

        // Unidade Goblin ALIADA — o efeito não deve se aplicar
        var unidade = CriarUnidadeGoblin(danoBase: 10);
        ModificadoresPartidaTalentos.Aplicar(unidade, _db);

        Assert.AreEqual(10, unidade.GetDanoAtaque(),
            "Talento de bônus vs inimigo não deve modificar unidade aliada");
    }

    // ── MÚLTIPLOS TALENTOS ─────────────────────────────────────────────────

    [Test]
    public void Should_StackMultipleTalentEffects_Correctly()
    {
        // 2 talentos de dano flat = +10 cada → total +20
        _db.pontosPorGanho = 2; // 2 pontos no nível 1
        var no1 = CriarNoSemRequisitos("dano_1", StatTalento.Dano, flat: 10, percentual: 0);
        var no2 = CriarNoSemRequisitos("dano_2", StatTalento.Dano, flat: 10, percentual: 0);
        ConfigurarDatabase(new[] { no1, no2 });

        TalentosRegras.TentarDesbloquear("dano_1", _db, _dados);
        TalentosRegras.TentarDesbloquear("dano_2", _db, _dados);
        PerfilJogadorAtivo.DefinirPerfil(_dados);

        var unidade = CriarUnidadeGoblin(danoBase: 10);
        ModificadoresPartidaTalentos.Aplicar(unidade, _db);

        Assert.AreEqual(30, unidade.GetDanoAtaque(),
            "10 (base) + 10 (talento 1) + 10 (talento 2) = 30");
    }

    // ── DESBLOQUEIO FALHA SE PONTOS INSUFICIENTES ─────────────────────────

    [Test]
    public void Should_FailToUnlock_When_InsufficientPoints()
    {
        var no1 = CriarNoSemRequisitos("dano_1", StatTalento.Dano, flat: 10, percentual: 0);
        var no2 = CriarNoSemRequisitos("dano_2", StatTalento.Dano, flat: 10, percentual: 0);
        ConfigurarDatabase(new[] { no1, no2 });

        // Apenas 1 ponto disponível (nível 1, 1 ponto por nível) — dois talentos custam 2
        TalentosRegras.TentarDesbloquear("dano_1", _db, _dados);
        bool resultado = TalentosRegras.TentarDesbloquear("dano_2", _db, _dados);

        Assert.IsFalse(resultado, "Não deve desbloquear sem pontos suficientes");
        Assert.AreEqual(1, _dados.talentos.nosDesbloqueados.Count, "Apenas 1 talento deve estar desbloqueado");
    }
}
