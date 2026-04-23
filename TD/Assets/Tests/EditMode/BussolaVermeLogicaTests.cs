using NUnit.Framework;

/// <summary>
/// Testa a lógica pura da relíquia Bússola do Verme.
/// Sem MonoBehaviour, sem cena, sem Unity runtime.
/// </summary>
public class BussolaVermeLogicaTests
{
    // ── CONSTANTE DE ID ────────────────────────────────────────────────────

    [Test]
    public void ReliquiaId_BussolaVerme_DeveSerConstanteCorreta()
    {
        Assert.AreEqual("reliquia_bussola_verme", ReliquiaIds.BussolaVerme);
    }

    // ── ROTAÇÃO POR LADO ───────────────────────────────────────────────────

    [Test]
    public void RotacaoZPorLado_Esquerda_Retorna180()
    {
        Assert.AreEqual(180f, BussolaVermeLogica.RotacaoZPorLado(LadoSpawn.Esquerda));
    }

    [Test]
    public void RotacaoZPorLado_Direita_Retorna0()
    {
        Assert.AreEqual(0f, BussolaVermeLogica.RotacaoZPorLado(LadoSpawn.Direita));
    }

    [Test]
    public void RotacaoZPorLado_Cima_Retorna90()
    {
        Assert.AreEqual(90f, BussolaVermeLogica.RotacaoZPorLado(LadoSpawn.Cima));
    }

    [Test]
    public void RotacaoZPorLado_Baixo_Retorna270()
    {
        Assert.AreEqual(270f, BussolaVermeLogica.RotacaoZPorLado(LadoSpawn.Baixo));
    }

    // ── NOME POR LADO ──────────────────────────────────────────────────────

    [Test]
    public void NomePorLado_Esquerda_RetornaStringNaoVazia()
    {
        Assert.IsFalse(string.IsNullOrEmpty(BussolaVermeLogica.NomePorLado(LadoSpawn.Esquerda)));
    }

    [Test]
    public void NomePorLado_Direita_RetornaStringNaoVazia()
    {
        Assert.IsFalse(string.IsNullOrEmpty(BussolaVermeLogica.NomePorLado(LadoSpawn.Direita)));
    }

    [Test]
    public void NomePorLado_Cima_RetornaStringNaoVazia()
    {
        Assert.IsFalse(string.IsNullOrEmpty(BussolaVermeLogica.NomePorLado(LadoSpawn.Cima)));
    }

    [Test]
    public void NomePorLado_Baixo_RetornaStringNaoVazia()
    {
        Assert.IsFalse(string.IsNullOrEmpty(BussolaVermeLogica.NomePorLado(LadoSpawn.Baixo)));
    }

    [Test]
    public void NomePorLado_TodosOsLados_RetornamTextoEmMaiusculo()
    {
        foreach (LadoSpawn lado in System.Enum.GetValues(typeof(LadoSpawn)))
        {
            string nome = BussolaVermeLogica.NomePorLado(lado);
            Assert.AreEqual(nome.ToUpper(), nome,
                $"NomePorLado({lado}) deve retornar texto em maiusculo");
        }
    }

    [Test]
    public void NomePorLado_TodosOsLados_SaoDistintos()
    {
        var lados = (LadoSpawn[])System.Enum.GetValues(typeof(LadoSpawn));
        var nomes = new System.Collections.Generic.HashSet<string>();
        foreach (var lado in lados)
            Assert.IsTrue(nomes.Add(BussolaVermeLogica.NomePorLado(lado)),
                $"Nome duplicado para lado {lado}");
    }

    // ── ROTAÇÕES SÃO DISTINTAS ─────────────────────────────────────────────

    [Test]
    public void RotacaoZPorLado_TodosOsLados_SaoDistintos()
    {
        var lados = (LadoSpawn[])System.Enum.GetValues(typeof(LadoSpawn));
        var rotacoes = new System.Collections.Generic.HashSet<float>();
        foreach (var lado in lados)
            Assert.IsTrue(rotacoes.Add(BussolaVermeLogica.RotacaoZPorLado(lado)),
                $"Rotacao duplicada para lado {lado}");
    }
}
