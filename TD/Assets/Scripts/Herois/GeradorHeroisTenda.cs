using System.Collections.Generic;

/// <summary>
/// Gera opções determinísticas de heróis para a tenda.
/// Se houver poucos heróis cadastrados, retorna o que existir sem duplicar.
/// </summary>
public class GeradorHeroisTenda
{
    private const int QuantidadePadrao = 3;

    public List<string> GerarOpcoes(HeroiDatabase db, RNGDeterministico rng)
    {
        var resultado = new List<string>();
        if (db == null || db.entradas == null || db.entradas.Length == 0)
            return resultado;

        var pool = new List<HeroiDef>();
        foreach (var def in db.entradas)
        {
            if (def == null || string.IsNullOrEmpty(def.idHeroi))
                continue;
            pool.Add(def);
        }

        while (pool.Count > 0 && resultado.Count < QuantidadePadrao)
        {
            int indice = rng.Next(0, pool.Count);
            resultado.Add(pool[indice].idHeroi);
            pool.RemoveAt(indice);
        }

        return resultado;
    }
}
