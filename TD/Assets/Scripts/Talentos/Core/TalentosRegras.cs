using System.Collections.Generic;
using UnityEngine;

public static class TalentosRegras
{
    public static bool TentarDesbloquear(string noId, TalentTreeDatabase database, DadosSlotSave dados)
    {
        database = ModificadoresPartidaTalentos.CarregarDatabaseOuNull(database);
        if (database == null || dados == null || string.IsNullOrEmpty(noId))
            return false;
        dados.NormalizarTalentos();

        var no = database.ObterNo(dados.raca, noId);
        if (no == null)
            return false;
        if (dados.talentos.nosDesbloqueados.Contains(noId))
            return false;

        int exp = dados.ObterExpPermanenteRaca(dados.raca);
        int nivel = NivelRacaCalculadora.CalcularNivelPorExpTotal(exp);
        if (nivel < no.nivelMinimoRaca)
            return false;

        database.RecalcularPontosDisponiveis(dados);
        if (dados.talentos.pontosDisponiveis < no.custoPontos)
            return false;

        if (no.requisitosItemIds != null)
        {
            for (int i = 0; i < no.requisitosItemIds.Length; i++)
            {
                string req = no.requisitosItemIds[i];
                if (string.IsNullOrEmpty(req))
                    continue;
                if (LootManager.Instance == null || !LootManager.Instance.PossuiItem(req, 1))
                    return false;
            }
        }

        if (!string.IsNullOrEmpty(no.grupoEscolhaId))
            RemoverOutrosDoMesmoGrupo(database, dados, no.grupoEscolhaId, noId);

        dados.talentos.nosDesbloqueados.Add(noId);

        if (!string.IsNullOrEmpty(no.grupoEscolhaId))
            UpsertEscolhaGrupo(dados, no.grupoEscolhaId, noId);

        ConsumirItensRequisito(no);

        database.RecalcularPontosDisponiveis(dados);
        return true;
    }

    public static bool TentarReverter(string noId, TalentTreeDatabase database, DadosSlotSave dados)
    {
        database = ModificadoresPartidaTalentos.CarregarDatabaseOuNull(database);
        if (database == null || dados == null || string.IsNullOrEmpty(noId))
            return false;
        dados.NormalizarTalentos();
        if (dados.talentos.nosDesbloqueados == null || !dados.talentos.nosDesbloqueados.Contains(noId))
            return false;

        var no = database.ObterNo(dados.raca, noId);
        if (no == null)
            return false;

        dados.talentos.nosDesbloqueados.Remove(noId);

        if (!string.IsNullOrEmpty(no.grupoEscolhaId))
            RemoverEscolhaGrupoSeNo(dados, no.grupoEscolhaId, noId);

        DevolverItensRequisitoParaNo(database, dados.raca, noId);

        database.RecalcularPontosDisponiveis(dados);
        return true;
    }

    private static void ConsumirItensRequisito(TalentNodeEntry no)
    {
        if (no?.requisitosItemIds == null || LootManager.Instance == null)
            return;
        for (int i = 0; i < no.requisitosItemIds.Length; i++)
        {
            string req = no.requisitosItemIds[i];
            if (string.IsNullOrEmpty(req))
                continue;
            LootManager.Instance.RemoverItem(req, 1);
        }
    }

    private static void DevolverItensRequisito(TalentNodeEntry no)
    {
        if (no?.requisitosItemIds == null || LootManager.Instance == null)
            return;
        for (int i = 0; i < no.requisitosItemIds.Length; i++)
        {
            string req = no.requisitosItemIds[i];
            if (string.IsNullOrEmpty(req))
                continue;
            Sprite sp = LootManager.Instance.GetSpriteParaItem(req);
            LootManager.Instance.AdicionarItem(req, sp, 1);
        }
    }

    private static void DevolverItensRequisitoParaNo(TalentTreeDatabase database, TipoRaca raca, string noId)
    {
        if (database == null || string.IsNullOrEmpty(noId))
            return;
        var no = database.ObterNo(raca, noId);
        if (no == null)
            return;
        DevolverItensRequisito(no);
    }

    private static void RemoverEscolhaGrupoSeNo(DadosSlotSave dados, string grupoId, string noId)
    {
        if (dados.talentos.escolhasGrupo == null)
            return;
        for (int i = dados.talentos.escolhasGrupo.Count - 1; i >= 0; i--)
        {
            if (dados.talentos.escolhasGrupo[i].grupoId == grupoId && dados.talentos.escolhasGrupo[i].noIdEscolhido == noId)
                dados.talentos.escolhasGrupo.RemoveAt(i);
        }
    }

    private static void RemoverOutrosDoMesmoGrupo(TalentTreeDatabase db, DadosSlotSave dados, string grupoId, string noIdMantido)
    {
        var nos = db.ObterNos(dados.raca);
        if (nos == null)
            return;
        var remover = new List<string>();
        for (int i = 0; i < nos.Length; i++)
        {
            if (nos[i] == null || nos[i].id == noIdMantido)
                continue;
            if (nos[i].grupoEscolhaId == grupoId && dados.talentos.nosDesbloqueados.Contains(nos[i].id))
                remover.Add(nos[i].id);
        }
        for (int i = 0; i < remover.Count; i++)
        {
            DevolverItensRequisitoParaNo(db, dados.raca, remover[i]);
            dados.talentos.nosDesbloqueados.Remove(remover[i]);
        }
    }

    private static void UpsertEscolhaGrupo(DadosSlotSave dados, string grupoId, string noId)
    {
        if (dados.talentos.escolhasGrupo == null)
            dados.talentos.escolhasGrupo = new List<TalentoGrupoEscolhaSave>();
        for (int i = 0; i < dados.talentos.escolhasGrupo.Count; i++)
        {
            if (dados.talentos.escolhasGrupo[i].grupoId == grupoId)
            {
                dados.talentos.escolhasGrupo[i].noIdEscolhido = noId;
                return;
            }
        }
        dados.talentos.escolhasGrupo.Add(new TalentoGrupoEscolhaSave { grupoId = grupoId, noIdEscolhido = noId });
    }
}
