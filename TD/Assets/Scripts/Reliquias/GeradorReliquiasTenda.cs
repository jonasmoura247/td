using System.Collections.Generic;

/// <summary>
/// Gera as 3 opções de relíquias exibidas numa tenda.
/// Regras: nunca oferecer relíquias já possuídas, aplicar pesos de raridade,
/// evitar repetir raridade dominante nas 3 opções.
/// Classe pura — sem dependência de MonoBehaviour.
/// </summary>
public class GeradorReliquiasTenda
{
    private const int QuantidadeOpcoes = 3;

    // Pesos por raridade: [Comum, Incomum, Raro, Lendario]
    public static readonly float[] PesosRaridade = { 0.55f, 0.30f, 0.12f, 0.03f };

    /// <summary>
    /// Gera lista de IDs de relíquias para oferecer na tenda.
    /// </summary>
    /// <param name="db">Database com todas as relíquias.</param>
    /// <param name="possuidas">IDs das relíquias já possuídas pelo jogador.</param>
    /// <param name="rng">RNG determinístico para esta fase.</param>
    public List<string> GerarOpcoes(ReliquiaDatabase db, List<string> possuidas, RNGDeterministico rng)
    {
        if (db == null || db.entradas == null || db.entradas.Length == 0)
            return new List<string>();

        var disponiveis = ConstruirPoolDisponivel(db, possuidas);
        var resultado = new List<string>();
        var usadasNestaTenda = new List<string>();
        var raridadesUsadas = new List<RaridadeReliquia>();

        for (int i = 0; i < QuantidadeOpcoes; i++)
        {
            if (disponiveis.Count == 0) break;

            RaridadeReliquia raridade = SortearRaridade(rng);

            // Evitar repetir raridade dominante (max 2x a mesma nas 3 opções)
            int repeticoes = ContarRaridade(raridadesUsadas, raridade);
            if (repeticoes >= 2)
                raridade = SortearRaridadeAlternativa(rng, raridade);

            var candidatos = FiltrarPorRaridade(disponiveis, raridade, usadasNestaTenda);
            if (candidatos.Count == 0)
                candidatos = FiltrarDisponiveis(disponiveis, usadasNestaTenda);
            if (candidatos.Count == 0) break;

            int idx = rng.Next(0, candidatos.Count);
            var escolhido = candidatos[idx];
            resultado.Add(escolhido.idReliquia);
            usadasNestaTenda.Add(escolhido.idReliquia);
            raridadesUsadas.Add(escolhido.raridade);
        }

        return resultado;
    }

    private List<ReliquiaDef> ConstruirPoolDisponivel(ReliquiaDatabase db, List<string> possuidas)
    {
        var pool = new List<ReliquiaDef>();
        foreach (var def in db.entradas)
        {
            if (def == null || string.IsNullOrEmpty(def.idReliquia)) continue;
            if (possuidas != null && possuidas.Contains(def.idReliquia)) continue;
            pool.Add(def);
        }
        return pool;
    }

    private RaridadeReliquia SortearRaridade(RNGDeterministico rng)
    {
        float val = rng.NextFloat();
        float acum = 0f;
        for (int i = 0; i < PesosRaridade.Length; i++)
        {
            acum += PesosRaridade[i];
            if (val < acum) return (RaridadeReliquia)i;
        }
        return RaridadeReliquia.Comum;
    }

    private RaridadeReliquia SortearRaridadeAlternativa(RNGDeterministico rng, RaridadeReliquia excluir)
    {
        // Tenta uma raridade diferente da excluída
        for (int tentativa = 0; tentativa < 10; tentativa++)
        {
            RaridadeReliquia r = SortearRaridade(rng);
            if (r != excluir) return r;
        }
        return excluir;
    }

    private int ContarRaridade(List<RaridadeReliquia> lista, RaridadeReliquia raridade)
    {
        int count = 0;
        foreach (var r in lista)
            if (r == raridade) count++;
        return count;
    }

    private List<ReliquiaDef> FiltrarPorRaridade(List<ReliquiaDef> pool, RaridadeReliquia raridade, List<string> excluir)
    {
        var result = new List<ReliquiaDef>();
        foreach (var d in pool)
            if (d.raridade == raridade && !excluir.Contains(d.idReliquia))
                result.Add(d);
        return result;
    }

    private List<ReliquiaDef> FiltrarDisponiveis(List<ReliquiaDef> pool, List<string> excluir)
    {
        var result = new List<ReliquiaDef>();
        foreach (var d in pool)
            if (!excluir.Contains(d.idReliquia))
                result.Add(d);
        return result;
    }
}
