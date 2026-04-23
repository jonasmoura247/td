using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;

/// <summary>
/// Testa a sinergia de explosão alquímica da relíquia Carvão-Enxofre-Salitre:
/// - Explosão dispara quando Queimadura + Veneno estão ativos simultaneamente.
/// - Explosão NÃO dispara sem a relíquia equipada.
/// - Explosão NÃO dispara com apenas um DoT ativo.
/// - Ambos os DoTs são removidos após a explosão (verificado por ciclo de re-aplicação).
/// </summary>
public class CarvaoEnxofreSalitreIntegrationTests
{
    [TearDown]
    public void Teardown()
    {
        PerfilJogadorAtivo.LimparPerfil();
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            Object.DestroyImmediate(go);
    }

    // ── HELPERS ────────────────────────────────────────────────────────────

    private void EquiparReliquiaCarvao()
    {
        var dados = new DadosSlotSave
        {
            ocupado = true,
            reliquiasPorSlot = new string[] { ReliquiaIds.CarvaoEnxofreSalitre, "", "", "", "" }
        };

        // Injeta via reflexão para evitar efeitos colaterais de DefinirPerfil (OuroManager etc.)
        typeof(PerfilJogadorAtivo)
            .GetField("perfilAtual", BindingFlags.NonPublic | BindingFlags.Static)
            ?.SetValue(null, dados);

        var mgrGo = new GameObject("ReliquiasManager");
        mgrGo.AddComponent<ReliquiasManager>();
    }

    private (UnidadeController uc, ControladorEfeitosStatusUnidade dots) CriarAlvo(int vida = 200)
    {
        var go = new GameObject("Alvo");
        var uc = go.AddComponent<UnidadeController>();
        uc.vidaMaxima = vida;
        uc.SincronizarVidaAposSpawn();
        var dots = go.AddComponent<ControladorEfeitosStatusUnidade>();
        return (uc, dots);
    }

    private DefinicaoDanoPorTempoSO CriarDot(TipoEfeitoStatusDot tipo, float intervalo = 60f)
    {
        var so = ScriptableObject.CreateInstance<DefinicaoDanoPorTempoSO>();
        so.tipoEfeito = tipo;
        so.danoPorTick = 5;
        so.intervaloSegundos = intervalo; // intervalo longo: ticks não disparam durante o teste
        so.quantidadeTicks = 3;
        so.corTint = Color.white;
        return so;
    }

    // ── EXPLOSÃO DISPARA ──────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_TriggerExplosion_WhenBothDotsActiveAndRelicEquipped()
    {
        EquiparReliquiaCarvao();
        var (uc, dots) = CriarAlvo(200);
        var defQ = CriarDot(TipoEfeitoStatusDot.Queimadura);
        var defV = CriarDot(TipoEfeitoStatusDot.Veneno);

        dots.AplicarOuRenovar(defQ);
        yield return null; // aguarda um frame (sem ticks: intervalo = 60s)

        int vidaAntesExplosao = uc.GetVidaAtual();
        Assert.AreEqual(200, vidaAntesExplosao, "Pré-condição: Queimadura sozinha não causa dano imediato.");

        dots.AplicarOuRenovar(defV); // segundo DoT → aciona explosão (30 dano padrão)
        yield return null;

        Assert.AreEqual(200 - 30, uc.GetVidaAtual(),
            "Explosão deve causar 30 de dano ao aplicar segundo DoT com relíquia equipada.");
    }

    // ── EXPLOSÃO NÃO DISPARA SEM RELÍQUIA ─────────────────────────────────

    [UnityTest]
    public IEnumerator Should_NotTriggerExplosion_WhenRelicNotEquipped()
    {
        // Sem chamar EquiparReliquiaCarvao → ReliquiasManager.Instance == null
        var (uc, dots) = CriarAlvo(200);
        var defQ = CriarDot(TipoEfeitoStatusDot.Queimadura);
        var defV = CriarDot(TipoEfeitoStatusDot.Veneno);

        dots.AplicarOuRenovar(defQ);
        yield return null;

        dots.AplicarOuRenovar(defV);
        yield return null;

        Assert.AreEqual(200, uc.GetVidaAtual(),
            "Sem relíquia equipada, nenhuma explosão deve ser disparada.");
    }

    // ── EXPLOSÃO NÃO DISPARA COM APENAS UM DoT ────────────────────────────

    [UnityTest]
    public IEnumerator Should_NotTriggerExplosion_WithOnlyOneDoT()
    {
        EquiparReliquiaCarvao();
        var (uc, dots) = CriarAlvo(200);
        var defQ = CriarDot(TipoEfeitoStatusDot.Queimadura);

        dots.AplicarOuRenovar(defQ); // apenas Queimadura, sem Veneno
        yield return null;

        Assert.AreEqual(200, uc.GetVidaAtual(),
            "Um único DoT não deve acionar a sinergia de explosão.");
    }

    // ── EXPLOSÃO REMOVE AMBOS OS DoTs ─────────────────────────────────────

    [UnityTest]
    public IEnumerator Should_RemoveBothDots_AfterExplosion()
    {
        EquiparReliquiaCarvao();
        var (uc, dots) = CriarAlvo(500); // vida alta para absorver múltiplas explosões
        var defQ = CriarDot(TipoEfeitoStatusDot.Queimadura);
        var defV = CriarDot(TipoEfeitoStatusDot.Veneno);

        // Primeiro ciclo: Queimadura + Veneno → explosão (30 dano)
        dots.AplicarOuRenovar(defQ);
        yield return null;
        dots.AplicarOuRenovar(defV);
        yield return null;

        int vidaApos1aExplosao = uc.GetVidaAtual();
        Assert.AreEqual(500 - 30, vidaApos1aExplosao, "Primeira explosão deve causar 30 de dano.");

        // Após explosão, ambos os DoTs foram removidos.
        // Aplicar apenas Queimadura novamente NÃO deve explodir (Veneno ausente).
        dots.AplicarOuRenovar(defQ);
        yield return null;

        Assert.AreEqual(vidaApos1aExplosao, uc.GetVidaAtual(),
            "Após explosão, DoTs foram removidos — reaplicar apenas Queimadura não explode.");

        // Agora aplicar Veneno → nova explosão (comprova que ciclo reinicia corretamente)
        dots.AplicarOuRenovar(defV);
        yield return null;

        Assert.AreEqual(vidaApos1aExplosao - 30, uc.GetVidaAtual(),
            "Segunda explosão (novo ciclo Q+V) deve causar mais 30 de dano.");
    }
}
