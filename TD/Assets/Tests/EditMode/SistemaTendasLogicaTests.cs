using NUnit.Framework;

/// <summary>
/// Garante as regras de negócio das tendas:
/// - Pity ativa após 3 fases sem tenda
/// - Probabilidade base de 30%
/// - Tipo de tenda segue distribuição configurada
/// </summary>
public class SistemaTendasLogicaTests
{
    private SistemaTendasLogica _logica;

    [SetUp]
    public void SetUp() => _logica = new SistemaTendasLogica();

    // ── Pity ──────────────────────────────────────────────────────────────

    [Test]
    public void Should_GarantirTenda_QuandoPityAtivado()
    {
        // Usa seed que normalmente não geraria tenda, mas pity deve forçar
        var rng = new RNGDeterministico(0);
        bool resultado = _logica.DecidarTemTenda(rng, SistemaTendasLogica.PityFasesSemTenda);
        Assert.IsTrue(resultado);
    }

    [Test]
    public void Should_GarantirTenda_QuandoFasesSemTendaExcedePity()
    {
        var rng = new RNGDeterministico(0);
        bool resultado = _logica.DecidarTemTenda(rng, SistemaTendasLogica.PityFasesSemTenda + 5);
        Assert.IsTrue(resultado);
    }

    [Test]
    public void Should_PityConst_SerTres()
    {
        Assert.AreEqual(3, SistemaTendasLogica.PityFasesSemTenda);
    }

    // ── ChanceBase ────────────────────────────────────────────────────────

    [Test]
    public void Should_ChanceBase_SerTrintaPorcento()
    {
        Assert.AreEqual(0.30f, SistemaTendasLogica.ChanceBase, 0.001f);
    }

    [Test]
    public void Should_NaoTerTenda_QuandoRNGAcimaDoLimiar()
    {
        // Seed 1 com fasesSemTenda=0 — verificamos que o roll sem pity pode falhar
        // Garantimos que a lógica do pity=0 pode retornar false
        bool algumaFalhou = false;
        for (int seed = 0; seed < 50; seed++)
        {
            var rng = new RNGDeterministico(seed);
            if (!_logica.DecidarTemTenda(rng, 0))
            {
                algumaFalhou = true;
                break;
            }
        }
        Assert.IsTrue(algumaFalhou, "Com 50 seeds diferentes e 30% de chance, ao menos uma deve falhar.");
    }

    [Test]
    public void Should_TerTenda_QuandoRNGAbaixoDoLimiar()
    {
        bool algumaSucedeu = false;
        for (int seed = 0; seed < 50; seed++)
        {
            var rng = new RNGDeterministico(seed);
            if (_logica.DecidarTemTenda(rng, 0))
            {
                algumaSucedeu = true;
                break;
            }
        }
        Assert.IsTrue(algumaSucedeu, "Com 50 seeds e 30% de chance, ao menos uma deve ter tenda.");
    }

    // ── TipoTenda ─────────────────────────────────────────────────────────

    [Test]
    public void Should_DecidarTipoTenda_RetornarValorValido()
    {
        for (int seed = 0; seed < 50; seed++)
        {
            var rng = new RNGDeterministico(seed);
            TipoTenda tipo = _logica.DecidarTipoTenda(rng);
            Assert.IsTrue(
                tipo == TipoTenda.Mercador ||
                tipo == TipoTenda.Herois ||
                tipo == TipoTenda.Mista ||
                tipo == TipoTenda.EventoEspecial,
                $"Tipo inválido: {tipo}"
            );
        }
    }

    [Test]
    public void Should_MercadorAparecerMaisFrequentemente()
    {
        int mercador = 0;
        int eventoespecial = 0;
        for (int seed = 0; seed < 1000; seed++)
        {
            var rng = new RNGDeterministico(seed);
            TipoTenda tipo = _logica.DecidarTipoTenda(rng);
            if (tipo == TipoTenda.Mercador) mercador++;
            if (tipo == TipoTenda.EventoEspecial) eventoespecial++;
        }
        Assert.Greater(mercador, eventoespecial, "Mercador (40%) deve ser mais comum que EventoEspecial (10%).");
    }

    // ── Determinismo ──────────────────────────────────────────────────────

    [Test]
    public void Should_MesmaSeed_ProducirMesmoResultado_ParaTenda()
    {
        var rng1 = new RNGDeterministico(777);
        var rng2 = new RNGDeterministico(777);
        bool r1 = _logica.DecidarTemTenda(rng1, 0);
        bool r2 = _logica.DecidarTemTenda(rng2, 0);
        Assert.AreEqual(r1, r2);
    }

    [Test]
    public void Should_MesmaSeed_ProducirMesmoTipo_ParaTenda()
    {
        var rng1 = new RNGDeterministico(888);
        var rng2 = new RNGDeterministico(888);
        TipoTenda t1 = _logica.DecidarTipoTenda(rng1);
        TipoTenda t2 = _logica.DecidarTipoTenda(rng2);
        Assert.AreEqual(t1, t2);
    }
}
