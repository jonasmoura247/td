using System.Text;
using UnityEngine;

public static class TalentDescricaoResumo
{
    public static string Montar(TalentNodeEntry no)
    {
        if (no == null)
            return "";
        var sb = new StringBuilder();
        sb.AppendLine("<b>").Append(no.ObterNomeCurto()).Append("</b>");
        sb.AppendLine("Nv. minimo: " + no.nivelMinimoRaca + " | Custo: " + no.custoPontos + " pt(s)");
        if (no.requisitosItemIds != null && no.requisitosItemIds.Length > 0)
        {
            sb.Append("Requer item: ");
            for (int i = 0; i < no.requisitosItemIds.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(no.requisitosItemIds[i]);
            }
            sb.AppendLine();
        }
        if (no.efeitos == null || no.efeitos.Length == 0)
            return sb.ToString().TrimEnd();
        sb.AppendLine();
        for (int i = 0; i < no.efeitos.Length; i++)
        {
            TalentEffectEntry e = no.efeitos[i];
            if (e == null) continue;
            sb.AppendLine("- " + FormatarEfeito(e));
        }
        return sb.ToString().TrimEnd();
    }

    private static string FormatarEfeito(TalentEffectEntry e)
    {
        string alvo = e.alvo == AlvoEfeitoTalento.MesmaRacaPerfil ? "Aliados (sua raca)" : "Inimigos " + e.racaAlvoInimigo;
        string stat = e.stat.ToString();
        if (Mathf.Abs(e.valorFlat) > 0.0001f)
            return alvo + ": " + stat + " " + (e.valorFlat > 0 ? "+" : "") + e.valorFlat;
        if (Mathf.Abs(e.valorPercentual) > 0.0001f)
            return alvo + ": " + stat + " " + (e.valorPercentual > 0 ? "+" : "") + e.valorPercentual + "%";
        return stat + " (" + alvo + ")";
    }
}
