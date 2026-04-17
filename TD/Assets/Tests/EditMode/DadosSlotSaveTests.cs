using NUnit.Framework;

/// <summary>
/// Protege o sistema de progressão permanente (XP/nível por raça) e
/// invariantes do save: XP nunca negativa, ouro nunca negativo, normalização robusta.
/// </summary>
public class DadosSlotSaveTests
{
    private DadosSlotSave CriarSave(TipoRaca raca = TipoRaca.Goblin)
    {
        return new DadosSlotSave(0, raca, GeneroChefe.Masculino);
    }

    // ── XP / NÍVEL ─────────────────────────────────────────────────────────

    [Test]
    public void Should_StartWithZeroExp_ForAllRaces()
    {
        var dados = CriarSave();
        Assert.AreEqual(0, dados.ObterExpPermanenteRaca(TipoRaca.Goblin));
        Assert.AreEqual(0, dados.ObterExpPermanenteRaca(TipoRaca.Humano));
        Assert.AreEqual(0, dados.ObterExpPermanenteRaca(TipoRaca.Elfo));
        Assert.AreEqual(0, dados.ObterExpPermanenteRaca(TipoRaca.Anao));
    }

    [Test]
    public void Should_StartAtLevel1_WhenExpIsZero()
    {
        var dados = CriarSave();
        Assert.AreEqual(1, dados.ObterNivelRaca(TipoRaca.Goblin));
    }

    [Test]
    public void Should_AddExpPermanente_Correctly()
    {
        var dados = CriarSave();
        dados.AdicionarExpPermanenteRaca(TipoRaca.Goblin, 400);
        Assert.AreEqual(400, dados.ObterExpPermanenteRaca(TipoRaca.Goblin));
    }

    [Test]
    public void Should_ReachLevel2_When_ExpReachesThreshold()
    {
        var dados = CriarSave();
        dados.AdicionarExpPermanenteRaca(TipoRaca.Goblin, 400);
        Assert.AreEqual(2, dados.ObterNivelRaca(TipoRaca.Goblin));
    }

    [Test]
    public void Should_NeverHaveNegativeExp_When_NegativeDeltaApplied()
    {
        var dados = CriarSave();
        dados.AdicionarExpPermanenteRaca(TipoRaca.Goblin, -9999);
        Assert.AreEqual(0, dados.ObterExpPermanenteRaca(TipoRaca.Goblin));
    }

    [Test]
    public void Should_DoNothing_When_DeltaIsZero()
    {
        var dados = CriarSave();
        dados.AdicionarExpPermanenteRaca(TipoRaca.Goblin, 100);
        dados.AdicionarExpPermanenteRaca(TipoRaca.Goblin, 0);
        Assert.AreEqual(100, dados.ObterExpPermanenteRaca(TipoRaca.Goblin));
    }

    [Test]
    public void Should_TrackExpPerRaceIndependently()
    {
        var dados = CriarSave();
        dados.AdicionarExpPermanenteRaca(TipoRaca.Goblin, 400);
        dados.AdicionarExpPermanenteRaca(TipoRaca.Humano, 200);

        Assert.AreEqual(400, dados.ObterExpPermanenteRaca(TipoRaca.Goblin));
        Assert.AreEqual(200, dados.ObterExpPermanenteRaca(TipoRaca.Humano));
        Assert.AreEqual(0,   dados.ObterExpPermanenteRaca(TipoRaca.Elfo));
        Assert.AreEqual(0,   dados.ObterExpPermanenteRaca(TipoRaca.Anao));
    }

    // ── OURO ───────────────────────────────────────────────────────────────

    [Test]
    public void Should_NeverHaveNegativeOuro_After_Normalization()
    {
        var dados = CriarSave();
        dados.ouro = -500;
        dados.NormalizarOuroMeta();
        Assert.AreEqual(0, dados.ouro);
    }

    [Test]
    public void Should_PreservePositiveOuro_After_Normalization()
    {
        var dados = CriarSave();
        dados.ouro = 250;
        dados.NormalizarOuroMeta();
        Assert.AreEqual(250, dados.ouro);
    }

    // ── NORMALIZAÇÃO ───────────────────────────────────────────────────────

    [Test]
    public void Should_RecreateNullBestiario_OnNormalizacao()
    {
        var dados = CriarSave();
        dados.bestiario = null;
        dados.NormalizarBestiario();

        Assert.IsNotNull(dados.bestiario);
        Assert.IsNotNull(dados.bestiario.progressoPorCriatura);
        Assert.IsNotNull(dados.bestiario.runasLiberadas);
        Assert.IsNotNull(dados.bestiario.runasEquipadas);
    }

    [Test]
    public void Should_ClampNegativePontosBestiario_ToZero_OnNormalizacao()
    {
        var dados = CriarSave();
        dados.bestiario.pontosBestiario = -100;
        dados.NormalizarBestiario();
        Assert.AreEqual(0, dados.bestiario.pontosBestiario);
    }

    [Test]
    public void Should_RecreateNullTalentos_OnNormalizacao()
    {
        var dados = CriarSave();
        dados.talentos = null;
        dados.NormalizarTalentos();

        Assert.IsNotNull(dados.talentos);
        Assert.IsNotNull(dados.talentos.nosDesbloqueados);
        Assert.IsNotNull(dados.talentos.escolhasGrupo);
    }

    // ── FORMATAÇÃO DE TEMPO ─────────────────────────────────────────────────

    [Test]
    public void Should_FormatarTempoJogo_AsSeconds_When_LessThanOneMinute()
    {
        Assert.AreEqual("45s", DadosSlotSave.FormatarTempoJogo(45f));
    }

    [Test]
    public void Should_FormatarTempoJogo_AsMinutes_When_LessThanOneHour()
    {
        Assert.AreEqual("2 min", DadosSlotSave.FormatarTempoJogo(120f));
    }

    [Test]
    public void Should_FormatarTempoJogo_AsHours_When_OneHourOrMore()
    {
        Assert.AreEqual("1h 1min", DadosSlotSave.FormatarTempoJogo(3661f));
    }

    [Test]
    public void Should_FormatarTempoJogo_AsZeroSeconds_When_TimeIsZero()
    {
        Assert.AreEqual("0s", DadosSlotSave.FormatarTempoJogo(0f));
    }

    // ── UUID ───────────────────────────────────────────────────────────────

    [Test]
    public void Should_GenerateUuid_WhenGarantirUuidCalledOnEmptyUuid()
    {
        var dados = CriarSave();
        dados.uuid = null;
        dados.GarantirUuid();
        Assert.IsFalse(string.IsNullOrEmpty(dados.uuid));
    }

    [Test]
    public void Should_PreserveExistingUuid_WhenGarantirUuidCalled()
    {
        var dados = CriarSave();
        dados.uuid = "meu-uuid-fixo";
        dados.GarantirUuid();
        Assert.AreEqual("meu-uuid-fixo", dados.uuid);
    }
}
