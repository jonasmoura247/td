using System.Collections.Generic;
using UnityEngine;

public static class BestiarioRegras
{
    public const int OuroRemocaoRunaPorNivelJogador = 100;

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

    public static bool RunaEstaLiberada(DadosBestiario b, string runaId)
    {
        return b?.runasLiberadas != null && !string.IsNullOrEmpty(runaId) && b.runasLiberadas.Contains(runaId);
    }

    public static int CalcularCustoOuroRemocaoRuna(DadosSlotSave dados)
    {
        if (dados == null)
            return 0;
        int nv = dados.ObterNivelRaca(dados.raca);
        return Mathf.Max(0, OuroRemocaoRunaPorNivelJogador * nv);
    }

    public static bool PodeLiberarRuna(
        string runaId,
        BestiarioDatabase database,
        DadosSlotSave dados,
        out string motivoFalha)
    {
        motivoFalha = "";
        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (database == null || dados == null || string.IsNullOrEmpty(runaId))
        {
            motivoFalha = "Dados invalidos.";
            return false;
        }
        dados.NormalizarBestiario();
        var runa = database.ObterRuna(runaId);
        if (runa == null)
        {
            motivoFalha = "Runa desconhecida.";
            return false;
        }
        if (RunaEstaLiberada(dados.bestiario, runaId))
        {
            motivoFalha = "Runa ja liberada.";
            return false;
        }
        if (BestiarioModoTeste.Ativo)
            return true;
        if (dados.bestiario.pontosBestiario < runa.custoPontosLiberacao)
        {
            motivoFalha = "Pontos de bestiario insuficientes.";
            return false;
        }
        return true;
    }

    public static bool TentarLiberarRuna(string runaId, BestiarioDatabase database, DadosSlotSave dados)
    {
        if (!PodeLiberarRuna(runaId, database, dados, out _))
            return false;
        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        var runa = database.ObterRuna(runaId);
        dados.NormalizarBestiario();
        if (!BestiarioModoTeste.Ativo)
        {
            dados.bestiario.pontosBestiario -= runa.custoPontosLiberacao;
            dados.bestiario.pontosBestiario = Mathf.Max(0, dados.bestiario.pontosBestiario);
        }
        if (!dados.bestiario.runasLiberadas.Contains(runaId))
            dados.bestiario.runasLiberadas.Add(runaId);
        return true;
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
        if (!BestiarioModoTeste.Ativo)
        {
            if (!RunaEstaLiberada(dados.bestiario, runaId))
            {
                motivoFalha = "Libere a runa com pontos antes de equipar.";
                return false;
            }

            var prog = ObterOuCriarProgresso(dados.bestiario, criaturaId);
            int estagios = ObterEstagiosCompletos(criatura, prog.mortes);
            if (estagios < runa.estagiosMinimos)
            {
                motivoFalha = "Estagios insuficientes nesta criatura.";
                return false;
            }
        }

        if (ObterCriaturaQueEquipaRuna(dados.bestiario, runaId, out string outra) && outra != criaturaId)
        {
            motivoFalha = "Runa ja equipada noutra criatura.";
            return false;
        }

        string atualNaCriatura = ObterRunaEquipadaNaCriatura(dados.bestiario, criaturaId);
        if (!string.IsNullOrEmpty(atualNaCriatura) && atualNaCriatura != runaId)
        {
            motivoFalha = "Remova a runa atual (custo em ouro) antes de equipar outra.";
            return false;
        }

        if (ObterRunaEquipadaNaCriatura(dados.bestiario, criaturaId) == runaId)
        {
            motivoFalha = "Ja equipada aqui.";
            return false;
        }

        return true;
    }

    public static bool TentarEquiparRuna(string criaturaId, string runaId, BestiarioDatabase database, DadosSlotSave dados)
    {
        if (!PodeEquiparRuna(criaturaId, runaId, database, dados, out _))
            return false;

        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        dados.NormalizarBestiario();

        if (BestiarioModoTeste.Ativo && !RunaEstaLiberada(dados.bestiario, runaId) &&
            !string.IsNullOrEmpty(runaId) && dados.bestiario.runasLiberadas != null)
            dados.bestiario.runasLiberadas.Add(runaId);

        RemoverRunaGlobal(dados.bestiario, runaId);
        dados.bestiario.runasEquipadas.Add(new BestiarioRunaEquipada { runaId = runaId, criaturaId = criaturaId });
        return true;
    }

    public static bool PodeRemoverRunaComOuro(
        string criaturaId,
        string runaId,
        BestiarioDatabase database,
        DadosSlotSave dados,
        out string motivoFalha)
    {
        motivoFalha = "";
        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (database == null || dados == null || string.IsNullOrEmpty(runaId))
        {
            motivoFalha = "Dados invalidos.";
            return false;
        }
        dados.NormalizarBestiario();
        if (database.ObterRuna(runaId) == null)
        {
            motivoFalha = "Runa desconhecida.";
            return false;
        }
        if (ObterRunaEquipadaNaCriatura(dados.bestiario, criaturaId) != runaId)
        {
            motivoFalha = "Runa nao esta equipada nesta criatura.";
            return false;
        }
        int custo = CalcularCustoOuroRemocaoRuna(dados);
        if (!BestiarioModoTeste.Ativo && dados.ouro < custo)
        {
            motivoFalha = $"Ouro insuficiente (precisa {custo}).";
            return false;
        }
        return true;
    }

    public static bool TentarRemoverRunaComOuro(string criaturaId, string runaId, BestiarioDatabase database, DadosSlotSave dados)
    {
        if (!PodeRemoverRunaComOuro(criaturaId, runaId, database, dados, out _))
            return false;

        dados.NormalizarBestiario();
        if (!BestiarioModoTeste.Ativo)
        {
            int custo = CalcularCustoOuroRemocaoRuna(dados);
            dados.ouro -= custo;
            dados.ouro = Mathf.Max(0, dados.ouro);
        }

        for (int i = dados.bestiario.runasEquipadas.Count - 1; i >= 0; i--)
        {
            var e = dados.bestiario.runasEquipadas[i];
            if (e != null && e.runaId == runaId && e.criaturaId == criaturaId)
            {
                dados.bestiario.runasEquipadas.RemoveAt(i);
                break;
            }
        }

        OuroPerfilUtil.AplicarOuroDoPerfilAoManager(dados);
        return true;
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

    /// <summary>Catálogo: destaque se a runa está liberada, pode ser liberada agora, ou está equipada em alguma criatura.</summary>
    public static bool RunaHabilitadaNoCatalogo(
        string runaId,
        BestiarioDatabase database,
        DadosSlotSave dados,
        out string motivoBloqueio)
    {
        motivoBloqueio = "";
        database = BestiarioCatalogoUtil.CarregarOuNull(database);
        if (database == null || dados == null || string.IsNullOrEmpty(runaId))
        {
            motivoBloqueio = "Dados invalidos.";
            return false;
        }
        dados.NormalizarBestiario();
        if (BestiarioModoTeste.Ativo)
            return true;
        if (RunaEstaLiberada(dados.bestiario, runaId))
            return true;
        if (ObterCriaturaQueEquipaRuna(dados.bestiario, runaId, out _))
            return true;
        if (PodeLiberarRuna(runaId, database, dados, out _))
            return true;
        var runa = database.ObterRuna(runaId);
        if (runa != null && dados.bestiario.pontosBestiario < runa.custoPontosLiberacao)
            motivoBloqueio = "Pontos insuficientes para liberar.";
        else
            motivoBloqueio = "Bloqueada.";
        return false;
    }
}
