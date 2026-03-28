using System;
using UnityEngine;

[Serializable]
public class TalentEffectEntry
{
    public AlvoEfeitoTalento alvo;
    public UnidadeSubtipo subtipoFiltro = UnidadeSubtipo.Indefinido;
    public TipoRaca racaAlvoInimigo;
    public StatTalento stat;
    public float valorFlat;
    public float valorPercentual;
}

[Serializable]
public class TalentNodeEntry
{
    public string id;
    public string nomeExibicao;
    public string descricaoTooltip;
    public int coluna;
    public int linha;
    public Sprite icone;
    public int custoPontos = 1;
    public int nivelMinimoRaca = 1;
    public string grupoEscolhaId = "";
    public string[] requisitosItemIds = Array.Empty<string>();
    public TalentEffectEntry[] efeitos = Array.Empty<TalentEffectEntry>();

    public string ObterNomeCurto()
    {
        if (!string.IsNullOrEmpty(nomeExibicao))
            return nomeExibicao;
        return id;
    }

    public string ObterTextoTooltipCompleto()
    {
        if (!string.IsNullOrEmpty(descricaoTooltip))
            return descricaoTooltip;
        return TalentDescricaoResumo.Montar(this);
    }
}

[Serializable]
public class TalentRaceBundle
{
    public TipoRaca raca;
    public TalentNodeEntry[] nos = Array.Empty<TalentNodeEntry>();
}
