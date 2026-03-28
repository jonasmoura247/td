using System.Collections.Generic;
using UnityEngine;

public static class BestiarioRegras
{
    public static void RegistrarMorte(string criaturaId, BestiarioDatabase database, DadosSlotSave dados)
    {
        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (database == null || dados == null || string.IsNullOrEmpty(criaturaId))
            return;
        if (database.ObterCriatura(criaturaId) == null)
            return;

        dados.NormalizarBestiario();
        var prog = ObterOuCriarProgresso(dados.bestiario, criaturaId);
        prog.mortes = Mathf.Max(0, prog.mortes + 1);

        var def = database.ObterCriatura(criaturaId);
        AplicarRecompensasEstagio(dados.bestiario, prog, def);
    }

    public static BestiarioCriaturaProgresso ObterOuCriarProgresso(DadosBestiario b, string criaturaId)
    {
        if (b.progressoPorCriatura == null)
            b.progressoPorCriatura = new List<BestiarioCriaturaProgresso>();
        for (int i = 0; i < b.progressoPorCriatura.Count; i++)
        {
            if (b.progressoPorCriatura[i] != null && b.progressoPorCriatura[i].criaturaId == criaturaId)
                return b.progressoPorCriatura[i];
        }
        var novo = new BestiarioCriaturaProgresso { criaturaId = criaturaId, mortes = 0, estagiosRecompensados = 0 };
        b.progressoPorCriatura.Add(novo);
        return novo;
    }

    public static int ObterMortes(DadosSlotSave dados, string criaturaId)
    {
        if (dados == null || string.IsNullOrEmpty(criaturaId))
            return 0;
        dados.NormalizarBestiario();
        for (int i = 0; i < dados.bestiario.progressoPorCriatura.Count; i++)
        {
            var p = dados.bestiario.progressoPorCriatura[i];
            if (p != null && p.criaturaId == criaturaId)
                return p.mortes;
        }
        return 0;
    }

    public static int ObterEstagiosCompletos(BestiarioCriaturaDef def, int mortes)
    {
        if (def?.marcosMortes == null || def.marcosMortes.Length == 0)
            return 0;
        int c = 0;
        for (int i = 0; i < def.marcosMortes.Length; i++)
        {
            if (mortes >= def.marcosMortes[i])
                c++;
            else
                break;
        }
        return c;
    }

    private static void AplicarRecompensasEstagio(DadosBestiario b, BestiarioCriaturaProgresso prog, BestiarioCriaturaDef def)
    {
        if (def?.marcosMortes == null || def.pontosPorEstagio == null)
            return;
        while (prog.estagiosRecompensados < def.marcosMortes.Length &&
               prog.mortes >= def.marcosMortes[prog.estagiosRecompensados])
        {
            int pts = prog.estagiosRecompensados < def.pontosPorEstagio.Length
                ? def.pontosPorEstagio[prog.estagiosRecompensados]
                : 0;
            b.pontosBestiario = Mathf.Max(0, b.pontosBestiario + Mathf.Max(0, pts));
            prog.estagiosRecompensados++;
        }
    }

    public static bool PodeEquiparRuna(
        string criaturaId,
        string runaId,
        BestiarioDatabase database,
        DadosSlotSave dados,
        out string motivoFalha)
    {
        motivoFalha = "";
        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (database == null || dados == null)
        {
            motivoFalha = "Dados invalidos.";
            return false;
        }
        dados.NormalizarBestiario();
        var runa = database.ObterRuna(runaId);
        var criatura = database.ObterCriatura(criaturaId);
        if (runa == null || criatura == null)
        {
            motivoFalha = "Runa ou criatura desconhecida.";
            return false;
        }
        if (runa.criaturaAlvoId != criaturaId)
        {
            motivoFalha = "Runa nao pertence a esta criatura.";
            return false;
        }

        var prog = ObterOuCriarProgresso(dados.bestiario, criaturaId);
        int estagios = ObterEstagiosCompletos(criatura, prog.mortes);
        if (estagios < runa.estagiosMinimos)
        {
            motivoFalha = "Estagios insuficientes.";
            return false;
        }

        if (ObterCriaturaQueEquipaRuna(dados.bestiario, runaId, out string outra) && outra != criaturaId)
        {
            motivoFalha = "Runa ja equipada em outra criatura.";
            return false;
        }

        if (ObterRunaEquipadaNaCriatura(dados.bestiario, criaturaId) == runaId)
        {
            motivoFalha = "Ja equipada aqui.";
            return false;
        }

        int pontosDisponiveis = dados.bestiario.pontosBestiario;
        string runaAtualCriatura = ObterRunaEquipadaNaCriatura(dados.bestiario, criaturaId);
        if (!string.IsNullOrEmpty(runaAtualCriatura) && runaAtualCriatura != runaId)
        {
            var defAtual = database.ObterRuna(runaAtualCriatura);
            if (defAtual != null)
                pontosDisponiveis += defAtual.custoPontos;
        }

        if (pontosDisponiveis < runa.custoPontos)
        {
            motivoFalha = "Pontos de bestiario insuficientes.";
            return false;
        }

        return true;
    }

    public static bool TentarEquiparRuna(string criaturaId, string runaId, BestiarioDatabase database, DadosSlotSave dados)
    {
        if (!PodeEquiparRuna(criaturaId, runaId, database, dados, out _))
            return false;

        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        var runa = database.ObterRuna(runaId);
        dados.NormalizarBestiario();

        RemoverRunaGlobal(dados.bestiario, runaId);

        string existente = ObterRunaEquipadaNaCriatura(dados.bestiario, criaturaId);
        if (!string.IsNullOrEmpty(existente) && existente != runaId)
            TentarDesequiparRuna(criaturaId, existente, database, dados);

        dados.bestiario.pontosBestiario -= runa.custoPontos;
        dados.bestiario.runasEquipadas.Add(new BestiarioRunaEquipada { runaId = runaId, criaturaId = criaturaId });
        return true;
    }

    public static bool TentarDesequiparRuna(string criaturaId, string runaId, BestiarioDatabase database, DadosSlotSave dados)
    {
        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (database == null || dados == null || string.IsNullOrEmpty(runaId))
            return false;
        dados.NormalizarBestiario();
        var runa = database.ObterRuna(runaId);
        if (runa == null)
            return false;

        for (int i = dados.bestiario.runasEquipadas.Count - 1; i >= 0; i--)
        {
            var e = dados.bestiario.runasEquipadas[i];
            if (e != null && e.runaId == runaId && e.criaturaId == criaturaId)
            {
                dados.bestiario.runasEquipadas.RemoveAt(i);
                dados.bestiario.pontosBestiario += runa.custoPontos;
                return true;
            }
        }
        return false;
    }

    public static string ObterRunaEquipadaNaCriatura(DadosBestiario b, string criaturaId)
    {
        if (b?.runasEquipadas == null)
            return null;
        for (int i = 0; i < b.runasEquipadas.Count; i++)
        {
            if (b.runasEquipadas[i] != null && b.runasEquipadas[i].criaturaId == criaturaId)
                return b.runasEquipadas[i].runaId;
        }
        return null;
    }

    public static bool ObterCriaturaQueEquipaRuna(DadosBestiario b, string runaId, out string criaturaId)
    {
        criaturaId = null;
        if (b?.runasEquipadas == null)
            return false;
        for (int i = 0; i < b.runasEquipadas.Count; i++)
        {
            if (b.runasEquipadas[i] != null && b.runasEquipadas[i].runaId == runaId)
            {
                criaturaId = b.runasEquipadas[i].criaturaId;
                return true;
            }
        }
        return false;
    }

    private static void RemoverRunaGlobal(DadosBestiario b, string runaId)
    {
        if (b?.runasEquipadas == null)
            return;
        for (int i = b.runasEquipadas.Count - 1; i >= 0; i--)
        {
            if (b.runasEquipadas[i] != null && b.runasEquipadas[i].runaId == runaId)
                b.runasEquipadas.RemoveAt(i);
        }
    }
}
