using UnityEngine;

public static class BestiarioTextoRunaUtil
{
    public static string ResumoEfeito(BestiarioRunaDef r)
    {
        if (r == null)
            return "";
        switch (r.tipoEfeito)
        {
            case TipoEfeitoRuna.DanoEcoAoReceberDano:
                return $"Eco: {Mathf.RoundToInt(r.parametroChance * 100f)}% de {Mathf.RoundToInt(r.parametroValor * 100f)}% vida max.";
            case TipoEfeitoRuna.CriticoMultiplicadorAtaque:
                return $"Critico ao castelo: x{r.parametroValor:0.##}";
            default:
                return "Sem efeito configurado.";
        }
    }

    public static string TextoRequisitos(BestiarioRunaDef r, BestiarioCriaturaDef criaturaLoreOpcional)
    {
        if (r == null)
            return "";
        if (!string.IsNullOrWhiteSpace(r.textoRequisitosCatalogo))
            return r.textoRequisitosCatalogo.Trim();
        string lib = $"Liberar: {r.custoPontosLiberacao} pts (uma vez)";
        string est = $"Equipar nesta criatura: min. {r.estagiosMinimos} estagio(s) completo(s)";
        if (criaturaLoreOpcional != null)
            return $"{lib}\n{est}\n(Lore: {criaturaLoreOpcional.nomeExibicao})";
        if (!string.IsNullOrEmpty(r.criaturaAlvoId))
            return $"{lib}\n{est}\n(Ref.: {r.criaturaAlvoId})";
        return $"{lib}\n{est}";
    }
}
