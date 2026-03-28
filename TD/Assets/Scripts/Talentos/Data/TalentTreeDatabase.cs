using UnityEngine;

[CreateAssetMenu(fileName = "TalentTreeDatabase", menuName = "TD/Talentos/Talent Tree Database")]
public class TalentTreeDatabase : ScriptableObject
{
    public int niveisPorNivelGanho = 2;
    public int pontosPorGanho = 1;
    public TalentRaceBundle[] arvoresPorRaca = new TalentRaceBundle[0];

    public TalentNodeEntry[] ObterNos(TipoRaca raca)
    {
        if (arvoresPorRaca == null)
            return null;
        for (int i = 0; i < arvoresPorRaca.Length; i++)
        {
            if (arvoresPorRaca[i] != null && arvoresPorRaca[i].raca == raca)
                return arvoresPorRaca[i].nos;
        }
        return null;
    }

    public TalentNodeEntry ObterNo(TipoRaca raca, string noId)
    {
        if (string.IsNullOrEmpty(noId))
            return null;
        var nos = ObterNos(raca);
        if (nos == null)
            return null;
        for (int i = 0; i < nos.Length; i++)
        {
            if (nos[i] != null && nos[i].id == noId)
                return nos[i];
        }
        return null;
    }

    public void RecalcularPontosDisponiveis(DadosSlotSave dados)
    {
        if (dados == null)
            return;
        dados.NormalizarTalentos();
        int exp = dados.ObterExpPermanenteRaca(dados.raca);
        int nivel = NivelRacaCalculadora.CalcularNivelPorExpTotal(exp);
        int intervalo = Mathf.Max(1, niveisPorNivelGanho);
        int pontosTotais = (nivel / intervalo) * Mathf.Max(0, pontosPorGanho);
        int gastos = 0;
        var nos = ObterNos(dados.raca);
        if (nos != null && dados.talentos.nosDesbloqueados != null)
        {
            for (int i = 0; i < dados.talentos.nosDesbloqueados.Count; i++)
            {
                var no = ObterNo(dados.raca, dados.talentos.nosDesbloqueados[i]);
                if (no != null)
                    gastos += no.custoPontos;
            }
        }
        dados.talentos.pontosDisponiveis = Mathf.Max(0, pontosTotais - gastos);
    }
}
