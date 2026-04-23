using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Protege BandeiraFaseController:
/// - Sprite correto é exibido para cada raça
/// - Collider desabilitado quando bloqueada
/// - Estado correto baseado na progressão do perfil
/// - TentarEntrar respeita bloqueio e seta FaseIdentificacao
/// - Sem crash quando perfil não está ativo
/// </summary>
public class BandeiraFaseControllerTests
{
    // ── Helpers ───────────────────────────────────────────────────────────

    private (GameObject go, BandeiraFaseController ctrl, SpriteRenderer sr, BoxCollider2D col)
        CriarBandeira(string faseId = "natural_1", string preReq = "")
    {
        var go = new GameObject("Bandeira");
        var sr = go.AddComponent<SpriteRenderer>();
        var col = go.AddComponent<BoxCollider2D>();
        var ctrl = go.AddComponent<BandeiraFaseController>();

        SetField(ctrl, "faseId", faseId);
        SetField(ctrl, "fasePreRequisito", preReq);
        SetField(ctrl, "nomeCena", "Mapa_1");

        return (go, ctrl, sr, col);
    }

    private Sprite CriarSprite(string nome)
    {
        var tex = new Texture2D(2, 2);
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
    }

    private DadosSlotSave CriarPerfil(TipoRaca raca = TipoRaca.Goblin)
    {
        var dados = new DadosSlotSave(0, raca, GeneroChefe.Masculino);
        dados.NormalizarProgressaoFases();
        return dados;
    }

    private void DefinirPerfil(TipoRaca raca = TipoRaca.Goblin)
        => PerfilJogadorAtivo.DefinirPerfil(CriarPerfil(raca));

    private static void SetField(object obj, string nome, object valor)
    {
        var f = obj.GetType().GetField(nome, BindingFlags.NonPublic | BindingFlags.Instance);
        f?.SetValue(obj, valor);
    }

    private static void SetSprite(BandeiraFaseController ctrl, string campo, Sprite sprite)
        => SetField(ctrl, campo, sprite);

    [TearDown]
    public void TearDown()
    {
        PerfilJogadorAtivo.LimparPerfil();
        FaseIdentificacao.Limpar();
    }

    // ── Sprite por raça ───────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_RetornarSpriteGoblin_QuandoRacaGoblin()
    {
        var (go, ctrl, _, _) = CriarBandeira();
        var sprite = CriarSprite("goblin");
        SetSprite(ctrl, "bandeiraGoblin", sprite);

        Assert.AreEqual(sprite, ctrl.ObterBandeiraParaRaca(TipoRaca.Goblin));

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_RetornarSpriteElfo_QuandoRacaElfo()
    {
        var (go, ctrl, _, _) = CriarBandeira();
        var sprite = CriarSprite("elfo");
        SetSprite(ctrl, "bandeiraElfo", sprite);

        Assert.AreEqual(sprite, ctrl.ObterBandeiraParaRaca(TipoRaca.Elfo));

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_RetornarSpriteAnao_QuandoRacaAnao()
    {
        var (go, ctrl, _, _) = CriarBandeira();
        var sprite = CriarSprite("anao");
        SetSprite(ctrl, "bandeiraAnao", sprite);

        Assert.AreEqual(sprite, ctrl.ObterBandeiraParaRaca(TipoRaca.Anao));

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_RetornarSpriteHumano_QuandoRacaHumano()
    {
        var (go, ctrl, _, _) = CriarBandeira();
        var sprite = CriarSprite("humano");
        SetSprite(ctrl, "bandeiraHumano", sprite);

        Assert.AreEqual(sprite, ctrl.ObterBandeiraParaRaca(TipoRaca.Humano));

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_RetornarNull_QuandoSpriteNaoConfigurado()
    {
        var (go, ctrl, _, _) = CriarBandeira();

        Assert.IsNull(ctrl.ObterBandeiraParaRaca(TipoRaca.Goblin));
        Assert.IsNull(ctrl.ObterBandeiraParaRaca(TipoRaca.Elfo));
        Assert.IsNull(ctrl.ObterBandeiraParaRaca(TipoRaca.Anao));
        Assert.IsNull(ctrl.ObterBandeiraParaRaca(TipoRaca.Humano));

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_AplicarSpriteCorreto_QuandoPerfilAtivo_Elfo()
    {
        var (go, ctrl, sr, _) = CriarBandeira();
        var spriteElfo = CriarSprite("elfo");
        var spriteGoblin = CriarSprite("goblin");
        SetSprite(ctrl, "bandeiraElfo", spriteElfo);
        SetSprite(ctrl, "bandeiraGoblin", spriteGoblin);

        DefinirPerfil(TipoRaca.Elfo);
        ctrl.AtualizarSpritePorRaca();

        Assert.AreEqual(spriteElfo, sr.sprite);

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_AplicarSpriteCorreto_QuandoPerfilAtivo_Goblin()
    {
        var (go, ctrl, sr, _) = CriarBandeira();
        var spriteGoblin = CriarSprite("goblin");
        var spriteElfo = CriarSprite("elfo");
        SetSprite(ctrl, "bandeiraGoblin", spriteGoblin);
        SetSprite(ctrl, "bandeiraElfo", spriteElfo);

        DefinirPerfil(TipoRaca.Goblin);
        ctrl.AtualizarSpritePorRaca();

        Assert.AreEqual(spriteGoblin, sr.sprite);

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_NaoCrash_QuandoSpriteNulo_EPerfilAtivo()
    {
        var (go, ctrl, sr, _) = CriarBandeira();
        var spriteAnterior = sr.sprite;

        DefinirPerfil(TipoRaca.Goblin); // sprite goblin não configurado = null
        Assert.DoesNotThrow(() => ctrl.AtualizarSpritePorRaca());

        // sprite não mudou pois o novo era null
        Assert.AreEqual(spriteAnterior, sr.sprite);

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_NaoCrash_QuandoSemPerfilAtivo()
    {
        var (go, ctrl, _, _) = CriarBandeira();
        // sem DefinirPerfil — PerfilJogadorAtivo.TemPerfilAtivo == false
        Assert.DoesNotThrow(() => ctrl.AtualizarSpritePorRaca());
        Assert.DoesNotThrow(() => ctrl.AtualizarEstado());

        Object.Destroy(go);
        yield return null;
    }

    // ── Estado (bloqueada / disponível / vencida) ─────────────────────────

    [UnityTest]
    public IEnumerator Should_EstadoDisponivel_QuandoSemPreRequisito_SemPerfil()
    {
        var (go, ctrl, _, _) = CriarBandeira(faseId: "natural_1", preReq: "");

        Assert.AreEqual(EstadoBandeira.Disponivel, ctrl.ObterEstado());

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_EstadoDisponivel_QuandoSemPreRequisito_ComPerfil()
    {
        var (go, ctrl, _, _) = CriarBandeira(faseId: "natural_1", preReq: "");
        DefinirPerfil();

        Assert.AreEqual(EstadoBandeira.Disponivel, ctrl.ObterEstado());

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_EstadoBloqueada_QuandoPreRequisitoNaoVencido()
    {
        var (go, ctrl, _, _) = CriarBandeira(faseId: "natural_2", preReq: "natural_1");
        DefinirPerfil();

        Assert.AreEqual(EstadoBandeira.Bloqueada, ctrl.ObterEstado());

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_EstadoDisponivel_QuandoPreRequisitoVencido()
    {
        var (go, ctrl, _, _) = CriarBandeira(faseId: "natural_2", preReq: "natural_1");
        var dados = CriarPerfil();
        dados.progressaoFases.RegistrarVitoria("natural_1");
        PerfilJogadorAtivo.DefinirPerfil(dados);

        Assert.AreEqual(EstadoBandeira.Disponivel, ctrl.ObterEstado());

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_EstadoVencida_QuandoFaseJaFoiVencida()
    {
        var (go, ctrl, _, _) = CriarBandeira(faseId: "natural_1", preReq: "");
        var dados = CriarPerfil();
        dados.progressaoFases.RegistrarVitoria("natural_1");
        PerfilJogadorAtivo.DefinirPerfil(dados);

        Assert.AreEqual(EstadoBandeira.Vencida, ctrl.ObterEstado());

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_EstadoBloqueada_QuandoSemPerfil_ComPreRequisito()
    {
        var (go, ctrl, _, _) = CriarBandeira(faseId: "natural_2", preReq: "natural_1");
        // sem perfil ativo: progressao retorna null → bloqueada

        Assert.AreEqual(EstadoBandeira.Bloqueada, ctrl.ObterEstado());

        Object.Destroy(go);
        yield return null;
    }

    // ── Collider ──────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_ColliderDesabilitado_QuandoBloqueada()
    {
        var (go, ctrl, _, col) = CriarBandeira(faseId: "natural_2", preReq: "natural_1");
        DefinirPerfil();

        ctrl.AtualizarEstado();

        Assert.IsFalse(col.enabled);

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_ColliderHabilitado_QuandoDisponivel()
    {
        var (go, ctrl, _, col) = CriarBandeira(faseId: "natural_1", preReq: "");
        DefinirPerfil();

        ctrl.AtualizarEstado();

        Assert.IsTrue(col.enabled);

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_ColliderHabilitado_QuandoVencida()
    {
        var (go, ctrl, _, col) = CriarBandeira(faseId: "natural_1", preReq: "");
        var dados = CriarPerfil();
        dados.progressaoFases.RegistrarVitoria("natural_1");
        PerfilJogadorAtivo.DefinirPerfil(dados);

        ctrl.AtualizarEstado();

        Assert.IsTrue(col.enabled);

        Object.Destroy(go);
        yield return null;
    }

    // ── TentarEntrar ──────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_SetarFaseAtualId_QuandoDisponivel()
    {
        var (go, ctrl, _, _) = CriarBandeira(faseId: "natural_1", preReq: "");
        DefinirPerfil();
        string cenaChamada = null;
        ctrl.CarregarCena = c => cenaChamada = c;

        ctrl.TentarEntrar();

        Assert.AreEqual("natural_1", FaseIdentificacao.FaseAtualId);
        Assert.AreEqual("Mapa_1", cenaChamada);

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_NaoCarregarCena_QuandoBloqueada()
    {
        var (go, ctrl, _, _) = CriarBandeira(faseId: "natural_2", preReq: "natural_1");
        DefinirPerfil();
        string cenaChamada = null;
        ctrl.CarregarCena = c => cenaChamada = c;

        ctrl.TentarEntrar();

        Assert.IsNull(cenaChamada);
        Assert.IsNull(FaseIdentificacao.FaseAtualId);

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_CarregarCena_QuandoVencida_PermiteReentrada()
    {
        var (go, ctrl, _, _) = CriarBandeira(faseId: "natural_1", preReq: "");
        var dados = CriarPerfil();
        dados.progressaoFases.RegistrarVitoria("natural_1");
        PerfilJogadorAtivo.DefinirPerfil(dados);
        string cenaChamada = null;
        ctrl.CarregarCena = c => cenaChamada = c;

        ctrl.TentarEntrar();

        Assert.AreEqual("Mapa_1", cenaChamada);

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Should_NaoCrash_TentarEntrar_SemPerfil_SemPreRequisito()
    {
        var (go, ctrl, _, _) = CriarBandeira(faseId: "natural_1", preReq: "");
        ctrl.CarregarCena = _ => { };

        Assert.DoesNotThrow(() => ctrl.TentarEntrar());

        Object.Destroy(go);
        yield return null;
    }
}
