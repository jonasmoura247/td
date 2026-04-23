using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Protege SetaNavegacaoMapa:
/// - Carrega a cena correta ao clicar
/// - Não carrega cena se destino vazio
/// - Não crasha sem delegate configurado
/// - Aplica tint cinza imediatamente ao clicar
/// - Só carrega a cena após o delay do efeito
/// </summary>
public class SetaNavegacaoMapaTests
{
    private (GameObject go, SetaNavegacaoMapa ctrl) CriarSeta(string destino = "Mapa_Gelo")
    {
        var go = new GameObject("Seta");
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<BoxCollider2D>();
        var ctrl = go.AddComponent<SetaNavegacaoMapa>();

        var f = typeof(SetaNavegacaoMapa).GetField("nomeCenaDestino",
            BindingFlags.NonPublic | BindingFlags.Instance);
        f?.SetValue(ctrl, destino);

        var dur = typeof(SetaNavegacaoMapa).GetField("duracaoTintClique",
            BindingFlags.NonPublic | BindingFlags.Instance);
        dur?.SetValue(ctrl, 0.1f);

        return (go, ctrl);
    }

    private void InvocarOnMouseDown(SetaNavegacaoMapa ctrl)
    {
        typeof(SetaNavegacaoMapa)
            .GetMethod("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(ctrl, null);
    }

    [TearDown]
    public void TearDown() { }

    [UnityTest]
    public IEnumerator Should_CarregarCenaCorreta_QuandoDestinoConfigurado()
    {
        var (go, ctrl) = CriarSeta("Mapa_Gelo");
        string cenaChamada = null;
        ctrl.CarregarCena = c => cenaChamada = c;

        InvocarOnMouseDown(ctrl);

        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual("Mapa_Gelo", cenaChamada);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Should_NaoCarregarCena_QuandoDestinoVazio()
    {
        var (go, ctrl) = CriarSeta("");
        string cenaChamada = null;
        ctrl.CarregarCena = c => cenaChamada = c;

        InvocarOnMouseDown(ctrl);

        yield return new WaitForSeconds(0.2f);

        Assert.IsNull(cenaChamada);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Should_NaoCrash_QuandoCarregarCenaEhNulo()
    {
        var (go, ctrl) = CriarSeta("Mapa_Gelo");
        ctrl.CarregarCena = null;

        Assert.DoesNotThrow(() => InvocarOnMouseDown(ctrl));

        yield return new WaitForSeconds(0.2f);

        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Should_CarregarCenaAnterior_QuandoSetaVoltar()
    {
        var (go, ctrl) = CriarSeta("Mapa_Natural");
        string cenaChamada = null;
        ctrl.CarregarCena = c => cenaChamada = c;

        InvocarOnMouseDown(ctrl);

        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual("Mapa_Natural", cenaChamada);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Should_AplicarTintCinza_QuandoClicar()
    {
        var (go, ctrl) = CriarSeta("Mapa_Gelo");
        ctrl.CarregarCena = _ => { };
        var sr = go.GetComponent<SpriteRenderer>();

        InvocarOnMouseDown(ctrl);

        // Aguarda um frame para o coroutine iniciar
        yield return null;

        Assert.Less(sr.color.r, 0.7f, "Vermelho deve estar reduzido (tint cinza)");
        Assert.Less(sr.color.g, 0.7f, "Verde deve estar reduzido (tint cinza)");
        Assert.Less(sr.color.b, 0.7f, "Azul deve estar reduzido (tint cinza)");

        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Should_NaoCarregarCenaImediatamente_AntesDeTintTerminar()
    {
        var (go, ctrl) = CriarSeta("Mapa_Gelo");
        string cenaChamada = null;
        ctrl.CarregarCena = c => cenaChamada = c;

        InvocarOnMouseDown(ctrl);

        // Imediatamente após o clique, a cena não deve ter sido carregada ainda
        Assert.IsNull(cenaChamada, "Cena não deve ser carregada antes do delay do efeito");

        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual("Mapa_Gelo", cenaChamada, "Cena deve ser carregada após o delay");
        Object.Destroy(go);
    }
}
